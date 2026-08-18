# Sprite System

The `Sprite` asset is the single, unified drawable type in CoreEssentials. One type handles both a **static** sprite (a single `texture2d` frame) and an **animated** sprite (an N-frame `spritesheet` sequence with a frame rate). A static sprite is simply a one-frame sprite, so there is no separate `AnimatedSprite` type anymore.

> **Note:** This is the post-Sprint 15.5 model. The former `AnimatedSprite` class has been removed and folded into `Sprite`. See [Migration Guide](Migration_Guide_SpriteConsolidation.md) for the breaking changes.

## Source Types

A `Sprite` is backed by one of two source types, declared in its XML definition:

| Source Type | Frames | Notes |
|-------------|--------|-------|
| `texture2d` | 1 | A single texture. Uses the per-sprite `Origin` and `Size` from XML. |
| `spritesheet` | N | A grid of frames from a `SpriteSheet`. Uses a `Frames` sequence (or a single `Frame`) and an optional `FrameRate`. |

## Loading a Sprite

```csharp
// Static sprite (single texture2d frame)
var ball = AssetManager.LoadAsset<Sprite>("ball_sprite.xml");

// Animated sprite (spritesheet sequence)
var walk = AssetManager.LoadAsset<Sprite>("character_anim_walk.xml");
```

## API

| Member | Type | Description |
|--------|------|-------------|
| `Texture` | `Texture2DAsset?` | The underlying texture (for `texture2d`). `null` for `spritesheet` sources. Used by instanced rendering/batching. |
| `SpriteSheet` | `SpriteSheet?` | The underlying sheet (for `spritesheet`). `null` for `texture2d`. |
| `FrameCount` | `int` | Number of frames in the sequence (1 for a static sprite). |
| `FrameRate` | `float` | Seconds per frame (i.e. `1 / frames-per-second`). |
| `Frames` | `int[]?` | The sprite-sheet frame indices in the sequence. |
| `SpriteSize` | `Vector2` | Size of a single frame (alias of `GetSize()`). |
| `GetSize()` | `Vector2` | Size of a single frame in pixels. |
| `Draw(...)` | `void` | Draws the first frame. Several overloads (rotation, scale, effects, layer depth). |
| `DrawFrame(...)` | `void` | Draws a specific frame index. For a `texture2d` sprite the index is ignored. |

### Drawing

```csharp
// Draw the first frame (static sprite or the first animation frame)
sprite.Draw(spriteBatch, position, Color.White, rotation: 0f,
            effects: SpriteEffects.None, layerDepth: 0f);

// Draw a specific frame (animated sprite)
sprite.DrawFrame(spriteBatch, position, frameIndex: 3, color: Color.White,
                 rotation: 0f, effects: SpriteEffects.None, layerDepth: 0f);
```

> **Note:** The old unconditional red debug outline (`Debug.Primitives.DrawRectangle(..., Color.Red)`) that was drawn around every sprite has been removed. Debug bounds are now the responsibility of the entity debug visualization system, not the sprite draw path.

## XML Schema

A `Sprite` is defined by a `SpriteData` XML document that supports **both** source types:

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

| Element | Required | Description |
|---------|----------|-------------|
| `SourceType` | Yes | `texture2d` or `spritesheet`. |
| `Source` | Yes | Asset name — a texture for `texture2d`, a sprite sheet for `spritesheet`. |
| `Size` | `texture2d` | Frame size in pixels (required for `texture2d`; informational for `spritesheet`). |
| `Origin` | `texture2d` | Pivot point in pixels for rotation/positioning (required for `texture2d`). |
| `Frames` | No | Comma-separated sprite-sheet frame indices. Defaults to the single `Frame`, then to `0`. |
| `FrameRate` | No | Frames per second (default 10). |
| `Frame` | No | Single frame to use when no `Frames` list is provided. |

A static `texture2d` sprite:

```xml
<?xml version="1.0" encoding="utf-8"?>
<SpriteData xmlns="http://schemas.coreessentials.monogame/2025/sprite">
  <SourceType>texture2d</SourceType>
  <Source>ball</Source>
  <Size>
    <Width>64</Width>
    <Height>64</Height>
  </Size>
  <Origin>
    <X>32</X>
    <Y>32</Y>
  </Origin>
</SpriteData>
```

## Rendering via a Component

The idiomatic way to render a sprite is through a `SpriteComponent` (see [Entity System](EntitySystem.md)). For animated sprites, pair it with an `AnimationComponent` — see [Animation Component](AnimationComponent.md).

```csharp
// Static sprite
AddComponent(new SpriteComponent(AssetManager.LoadAsset<Sprite>("ball_sprite.xml")));
```

## Instanced Rendering / Batching

`Sprite.Texture` exposes the underlying texture for entities that use texture-based batching. For `spritesheet` sources `Texture` is `null` (the sheet is not batched directly), so such entities render through the no-texture path.

```csharp
// Register a sprite's texture for instanced rendering
RegisterForInstancedRendering(sprite); // uses sprite.Texture
```

## Related

- [Animation Component](AnimationComponent.md) — drives named animations on an entity.
- [Sprite Scaling](SpriteScaling.md) — working with sprite scale and origin.
- [Migration Guide](Migration_Guide_SpriteConsolidation.md) — `AnimatedSprite` → `Sprite` breaking changes.
