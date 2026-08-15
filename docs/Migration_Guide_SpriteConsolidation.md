# Migration Guide — Sprite Consolidation (Sprint 15.5)

This guide covers the **breaking changes** introduced by consolidating `Sprite` and `AnimatedSprite` into a single `Sprite` type, and adding `AnimationComponent`.

> **Status:** Breaking change. Public API rename + content XML schema change.

## Summary of Changes

| Before | After |
|--------|-------|
| `Sprite` (static) and `AnimatedSprite` (animated) — two types | A single `Sprite` type handles both |
| `AnimatedSprite` class | **Removed** (folded into `Sprite`) |
| `AnimationState.AnimatedSprite` property | `AnimationState.Sprite` property |
| `AnimationState(AnimatedSprite)` constructor | `AnimationState(Sprite)` constructor |
| `AnimatedSprite.SpriteSize` | `Sprite.SpriteSize` (same as `Sprite.GetSize()`) |
| `AnimatedSprite.Frames` | `Sprite.Frames` |
| `AnimatedSprite.FrameRate` | `Sprite.FrameRate` |
| `AnimatedSprite.FrameCount` | `Sprite.FrameCount` |
| `AnimatedSprite.DrawFrame(...)` | `Sprite.DrawFrame(...)` |
| Red debug outline on every sprite draw | **Removed** |
| — | New `AnimationComponent` (multi-animation) |
| — | `Entity.GetSize()`/`GetOrigin()` resolve from the `SpriteComponent` (single source of truth) |

## 1. Type Rename: `AnimatedSprite` → `Sprite`

The `AnimatedSprite` class no longer exists. All animated-sprite usage now uses `Sprite`.

### API Mapping

| Old (`AnimatedSprite`) | New (`Sprite`) |
|------------------------|----------------|
| `new AnimatedSprite(name)` | `new Sprite(name)` |
| `AssetManager.LoadAsset<AnimatedSprite>(...)` | `AssetManager.LoadAsset<Sprite>(...)` |
| `sprite.FrameCount` | `sprite.FrameCount` |
| `sprite.FrameRate` | `sprite.FrameRate` |
| `sprite.SpriteSize` | `sprite.SpriteSize` |
| `sprite.Frames` | `sprite.Frames` |
| `sprite.SpriteSheet` | `sprite.SpriteSheet` |
| `sprite.DrawFrame(batch, pos, frame, color, ...)` | `sprite.DrawFrame(batch, pos, frame, color, ...)` |

### Old `Sprite` (static) — unchanged surface

The old static `Sprite` is retained as the base of the unified type. Its `Draw(...)`, `GetSize()`, and `Texture` members behave the same, except:
- `Draw(...)` now delegates to `DrawFrame(0, ...)` (frame 0), and
- the red debug outline is no longer drawn.

### `AnimationState`

The `AnimationState` type still exists (it tracks per-entity playback). Only the type of its backing sprite changed:

```csharp
// Before
var state = new AnimationState(animatedSprite);   // AnimatedSprite
var count = state.AnimatedSprite.FrameCount;

// After
var state = new AnimationState(sprite);           // Sprite
var count = state.Sprite.FrameCount;
```

## 2. Content XML Schema

The animated-sprite XML root `AnimatedSpriteData` is replaced by the unified `SpriteData` root. The `SpriteData` root already supported `texture2d` and `spritesheet` sources; it now also accepts the `Frames` and `FrameRate` elements that used to live on `AnimatedSpriteData`.

### Before (`AnimatedSpriteData`)

```xml
<?xml version="1.0" encoding="utf-8"?>
<AnimatedSpriteData xmlns="http://schemas.coreessentials.monogame/2025/sprite">
  <SourceType>spritesheet</SourceType>
  <Source>character_sheet.xml</Source>
  <Size>
    <Width>192</Width>
    <Height>256</Height>
  </Size>
  <Frames>36,37,38,39,40,41,42,43</Frames>
  <FrameRate>11</FrameRate>
</AnimatedSpriteData>
```

### After (unified `SpriteData`)

```xml
<?xml version="1.0" encoding="utf-8"?>
<SpriteData xmlns="http://schemas.coreessentials.monogame/2025/sprite">
  <SourceType>spritesheet</SourceType>
  <Source>character_sheet.xml</Source>
  <Size>
    <Width>192</Width>
    <Height>256</Height>
  </Size>
  <Frames>36,37,38,39,40,41,42,43</Frames>
  <FrameRate>11</FrameRate>
</SpriteData>
```

**Migration step:** change the root element name from `AnimatedSpriteData` to `SpriteData`. All inner elements (`SourceType`, `Source`, `Size`, `Frames`, `FrameRate`) are unchanged.

Static `texture2d` sprites that already used the `SpriteData` root (`ball_sprite.xml`) need **no** changes.

## 3. Entity Refactors

Entities that previously animated manually should switch to components.

### Before — `AnimatedCharacterEntity` (manual animation)

```csharp
private AnimatedSprite _animatedSprite;
private AnimationState _animationState;

public override void OnStart()
{
    base.OnStart();
    _animatedSprite = (AnimatedSprite)AssetManager.LoadAsset<AnimatedSprite>("character_anim_walk.xml");
    _animationState = new AnimationState(_animatedSprite);
}

public override void Update(GameTime gameTime)
{
    base.Update(gameTime);
    _animationState.Update(gameTime);
}

public override void Render(SpriteBatch spriteBatch)
{
    _animationState.Draw(spriteBatch, _position, Color.White, 0f, SpriteEffects.None, 0f);
}

public override Vector2 GetSize()
{
    if (_animatedSprite == null) return Vector2.Zero;
    return _animatedSprite.SpriteSize * Scale;
}
```

### After — `AnimatedCharacterEntity` (component-based, no overrides)

```csharp
public override void OnStart()
{
    base.OnStart();
    var sprite = AssetManager.LoadAsset<Sprite>("character_anim_walk.xml");
    AddComponent(new SpriteComponent(sprite));      // owns rendering + geometry
    var animation = AddComponent(new AnimationComponent()); // pure controller
    animation.AddAnimation("walk", sprite);
    animation.Play("walk");
}
```

The `Update`, `Render`, and `GetSize` overrides are gone — the base `Entity` handles all three through the components. Note that the `AnimationComponent` is a **pure controller**: it drives the frames, but the `SpriteComponent` owns rendering and geometry, so both must be attached.

A static-sprite entity (`CharacterEntity`) likewise drops its `Render`/`GetSize` overrides in favor of a `SpriteComponent`:

```csharp
public override void OnStart()
{
    base.OnStart();
    var sprite = AssetManager.LoadAsset<Sprite>("character_sprite.xml");
    AddComponent(new SpriteComponent(sprite));
}
```

## 4. Rendering / Debugging

- The unconditional red debug outline (`Debug.Primitives.DrawRectangle(..., Color.Red)`) that was drawn around every sprite in both draw paths has been **removed**.
- Debug bounds visualization is the responsibility of the entity debug visualization system (`EntityDebugDraw`), not the sprite draw path.

## 5. Batching

`Sprite.Texture` is still available for `Entity.RegisterForInstancedRendering(Sprite)`. For `spritesheet` sources `Texture` is `null` (sheets are not batched directly); for `texture2d` sources it returns the underlying texture, so instanced rendering/batching is unchanged.

## Checklist

- [ ] Replace `AnimatedSprite` with `Sprite` (type + `LoadAsset<T>`).
- [ ] Replace `state.AnimatedSprite` with `state.Sprite`.
- [ ] Change animated-sprite XML root `AnimatedSpriteData` → `SpriteData`.
- [ ] Refactor animated entities to use `AnimationComponent` (drop `Render`/`Update`/`GetSize` overrides).
- [ ] Refactor static-sprite entities to use `SpriteComponent` (drop `Render`/`GetSize` overrides).
- [ ] Remove any reliance on the red debug outline (it no longer draws).
- [ ] Verify instanced rendering still batches `texture2d` sprites.
