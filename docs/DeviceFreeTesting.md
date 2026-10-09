# Device-Free Testing

Much of CoreEssentials touches a `GraphicsDevice` (via `SpriteBatch`, textures, effects) only at the very
end of a method — after real logic such as geometry math, timing, config gating, and state transitions.
That logic is fully unit-testable **without a device**, which is exactly what the 80% line-coverage gate
measures. This page describes the two techniques the test project uses to reach it:

1. **Internal test seams** — small `internal` accessors that let tests set up (or read) private state that
   would normally only be produced by loading an asset or attaching to a live system.
2. **Interface injection at the draw boundary** — pulling the device hop behind a small interface so a
   recording fake can stand in for `SpriteBatch`/primitives and assert on *what* was drawn and *where*.

Both keep the public API unchanged and require no graphics device, no `Effect`/`Texture2D` allocation, and
no live `SpriteBatch`. They are not part of the shipped behavior; they exist so the real logic can be driven
and asserted. Production code paths are untouched.

## Internal test seams

A seam is an `internal` getter/setter for a private field, visible to the test project through
`InternalsVisibleTo("CoreEssentials.Tests")`. Use this when the device is needed only *after* a value has
already been computed — the logic you care about is in how that value is derived.

Example from [`Sprite`](../CoreEssentials/src/Assets/Sprite.cs) — the frame sequence and rate are normally
filled by loading XML, but the internal seams let a test configure them directly:

```csharp
// In Sprite.cs (production) — not part of the public API.
internal int[]? TestFrames { get => _frames; set => _frames = value; }
internal float TestFrameRate { get => _frameRate; set => _frameRate = value; }
```

Test drives a real `Sprite` with a known frame count and rate, then asserts on pure-timing behavior in
`AnimationState.Update(GameTime)` — no device involved:

```csharp
var sprite = new Sprite("anim");
var frames = new[] { 0, 1, 2 };
sprite.TestFrames = frames;          // 3-frame clip
sprite.TestFrameRate = 0.1f;         // one step per 0.1 s

var state = new AnimationState(sprite) { IsLooping = false };
bool raised = false;
state.AnimationCompleted += (_, _) => raised = true;

for (var i = 0; i < 3; i++) state.Update(new GameTime(TimeSpan.Zero, TimeSpan.FromSeconds(0.1)));

Assert.Equal(2, state.CurrentFrame); // clamped to the last frame
Assert.False(state.IsPlaying);        // playback stopped
Assert.True(raised);                  // completed event fired on end
```

See `CoreEssentials.Tests/Asset/AnimationStateLogicTests.cs` and `SpriteLogicTests.cs`.

## Interface injection at the draw boundary

Use this when a method's *logic* is about **where** to draw (bounds from position-minus-origin, crosshair
endpoints, hierarchy lines grouped by id) rather than the GPU call itself. Extract the device hop behind a
small interface and let production adapt it while tests supply a recording fake.

`EntityDebugDraw` used to call `Debug.Primitives.*` and `SpriteBatch.DrawString` directly. It now routes
through an internal target:

```csharp
// In EntityDebugDraw.cs (production) — the seam plus its default adapter.
internal interface IEntityDebugTarget
{
    void DrawLine(Vector2 start, Vector2 end, Color color, float thickness);
    void DrawRectangle(Rectangle bounds, Color color, float thickness);
    void DrawText(SpriteFont font, string text, Vector2 position, Color color);
}

internal sealed class SpriteBatchDebugTarget : IEntityDebugTarget
{
    private readonly SpriteBatch _spriteBatch;
    public SpriteBatchDebugTarget(SpriteBatch spriteBatch) => _spriteBatch = spriteBatch;
    public void DrawLine(Vector2 s, Vector2 e, Color c, float t)   => Debug.Primitives.DrawLine(_spriteBatch, s, e, c, t);
    public void DrawRectangle(Rectangle b, Color c, float t)       => Debug.Primitives.DrawRectangle(_spriteBatch, b, c, t);
    public void DrawText(SpriteFont f, string tx, Vector2 p, Color c) => _spriteBatch.DrawString(f, tx, p, c);
}

// The public entry point is unchanged; it simply wraps the batch in the adapter.
public void DrawOverlays(IEnumerable<Entity> entities, SpriteBatch spriteBatch, FontAsset? fontAsset = null)
    => DrawOverlays(entities, new SpriteBatchDebugTarget(spriteBatch), fontAsset);

internal void DrawOverlays(IEnumerable<Entity> entities, IEntityDebugTarget target, FontAsset? fontAsset = null) { /* ... */ }
```

A test records the calls and asserts the geometry — no `SpriteBatch` needed:

```csharp
private sealed class RecordingTarget : IEntityDebugTarget
{
    public List<(Rectangle Bounds, Color Color, float Thickness)> Rectangles { get; } = new();
    public void DrawRectangle(Rectangle bounds, Color color, float thickness) => Rectangles.Add((bounds, color, thickness));
    /* DrawLine / DrawText record similarly */
}

// A 40x20 sprite whose origin is its center (20,10), placed at (100,50):
// the bounds top-left is position - origin = (80,40).
var entity = MakeEntity(position: new Vector2(100f, 50f), size: new Vector2(40f, 20f), origin: new Vector2(20f, 10f));
var target = new RecordingTarget();

new EntityDebugDraw(config).DrawOverlays(new[] { entity }, target);

Assert.Equal(new Rectangle(80, 40, 40, 20), Assert.Single(target.Rectangles).Bounds);
```

See `CoreEssentials.Tests/Debugging/EntityDebugDrawLogicTests.cs`.

### The render pipeline (`IEntityDrawTarget`)

The same idea scales up to a whole rendering pass. `EntitySystem.Draw` used to own both the *grouping
logic* (which entities share a texture, which z-layer order, which contiguous runs share an effect) and the
actual `SpriteBatch.Begin/End` + `effect.SetMatrix` calls. That made the batch-structure logic untestable
without a device. The seam pulls only the GPU hop behind an interface; everything that *decides* the order
and grouping stays in the system and becomes reachable:

```csharp
// In EntitySystem.cs (production) — the seam plus its default adapter.
internal interface IEntityDrawTarget
{
    void Begin(Effect? effect, Matrix? viewMatrix);
    void End();
    void SyncProjection(Effect? effect);   // sets the projection matrix on a live Effect
    void DrawEntity(Entity entity);
}

internal sealed class SpriteBatchDrawTarget : IEntityDrawTarget
{
    private readonly SpriteBatch _spriteBatch;
    public SpriteBatchDrawTarget(SpriteBatch spriteBatch) => _spriteBatch = spriteBatch;
    public void Begin(Effect? effect, Matrix? viewMatrix)
        => _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp,
             DepthStencilState.None, RasterizerState.CullNone, effect, viewMatrix);
    public void End() => _spriteBatch.End();
    public void SyncProjection(Effect? effect)
        => RenderPipeline.SyncEffectProjection(effect, _spriteBatch.GraphicsDevice);
    public void DrawEntity(Entity entity) => entity.Render(_spriteBatch);
}

// Public entry point is unchanged; it just wraps the batch in the adapter.
public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
{
    RenderEntities(new SpriteBatchDrawTarget(spriteBatch));
    ResetTextureDirtyFlags();
    if (DebugMode) DrawDebugOverlays(spriteBatch);
}

internal void RenderEntities(IEntityDrawTarget target) { /* group → partition → Begin/Draw/End per run */ }
```

A test records the op sequence and asserts on *structure* — how many batches were opened, whether they
balance, which entities fell into each run — with no `SpriteBatch` or `Effect`:

```csharp
var target = new RecordingTarget(); // implements IEntityDrawTarget, appends to a List<string>
system.RenderEntities(target);

Assert.Equal(1, target.BeginCount);   // one texture group → one null-effect run
Assert.Equal(target.BeginCount, target.EndCount); // balanced
Assert.True(target.LastOpIsEnd());    // the batch was closed after its entities drew
```

`GroupEntitiesByZLayer`, `PartitionByEffect`, and the individual render methods were promoted to
`internal` (and `static` where they are pure) so tests can assert on their grouping directly. The one
uncovered block is the inner `if (effect != null)` sync/apply — a thin GPU hop that needs a real compiled
`Effect`.

See `CoreEssentials.Tests/GameSystems/EntitySystems/EntityOOPsystem/EntitySystemRenderPipelineTests.cs`.

## Choosing between the two

| Situation | Technique |
| --- | --- |
| Logic computes a value; the device is only used to emit it afterward. | Internal seam — set the inputs, assert the derived values. |
| Logic decides *where/what* to draw (positions, rects, grouping, gating). | Interface injection — record calls, assert geometry and selection. |
| The method is nothing but a forward to `SpriteBatch.Draw`. | Neither is worth it — that hop belongs in the manual playground smoke-run. |

Both patterns deliberately leave the thin GPU hop uncovered. Forcing coverage of a raw draw call only tests
the mock; the meaningful assertions are on the logic around it, which these techniques make reachable under
the 80% gate described in [CodeCoverage.md](./CodeCoverage.md).
