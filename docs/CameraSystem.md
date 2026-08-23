# Camera System

The CoreEssentials-MonoGame Camera system provides an orthographic camera implementation that can be used to transform the game view, enabling features like zooming, panning, and rotation.

## Features

- Orthographic camera for 2D games
- Camera positioning, rotation, and zooming
- Static MainCamera property for easy access to the current camera
- Easy conversion between screen and world coordinates
- Unity-style orthographic projection (`OrthographicSize` / `ViewportSize` / `RenderScale`) for pixel-art games where the render resolution differs from the game resolution
- Built-in `CameraComponent` that anchors a camera to an entity (no input handling)

## Basic Usage

### Creating and Setting Up a Camera

```csharp
// Create a new camera
Camera camera = new Camera();

// Set its initial position
camera.Position = new Vector2(100, 100);

// Set some zoom
camera.Zoom = 1.5f;

// Make this the main camera
camera.SetAsMainCamera();
// OR
Camera.SetMainCamera(camera);
```

### Disposing a Camera

The `Camera` class implements `IDisposable`. When a camera instance is no longer needed, you should call its `Dispose()` method to release resources. A key behavior of `Dispose()` is that if the camera instance being disposed is currently set as the `Camera.MainCamera`, `Dispose()` will set `Camera.MainCamera` to `null`.

```csharp
Camera myCamera = new Camera();
myCamera.SetAsMainCamera();

// ...later, when the camera is no longer needed...
myCamera.Dispose(); 
// Now, Camera.MainCamera will be null (if myCamera was indeed the MainCamera)
```

This is important for preventing the game from trying to use a disposed camera instance for rendering or other calculations.

### Using the Camera in Rendering

```csharp
// In your Draw method
SpriteBatch.Begin(
    SpriteSortMode.Deferred,
    BlendState.AlphaBlend,
    SamplerState.LinearClamp,
    null,
    null,
    null,
    Camera.MainCamera.ViewMatrix); // Use the MainCamera's ViewMatrix

// Draw your sprites
SpriteBatch.Draw(texture, position, null, Color.White);

SpriteBatch.End();
```

### Converting Between Screen and World Coordinates

```csharp
// Convert mouse position from screen to world coordinates
Vector2 mouseScreenPosition = new Vector2(Mouse.GetState().X, Mouse.GetState().Y);
Vector2 mouseWorldPosition = Camera.MainCamera.ScreenToWorld(mouseScreenPosition);

// Convert a world position to screen coordinates
Vector2 entityWorldPosition = entity.Position;
Vector2 entityScreenPosition = Camera.MainCamera.WorldToScreen(entityWorldPosition);
```

## Orthographic Projection (Pixel Art)

By default the camera uses a legacy projection where **one world unit equals one pixel** at zoom 1. For pixel-art games — where the game is authored at a small logical resolution (e.g. 320×180) and presented upscaled on a larger window (e.g. 1280×720) — the camera supports a Unity-style orthographic model via three properties:

| Property | Type | Default | Description |
|---|---|---|---|
| `OrthographicSize` | `float` | `0` | Half-height of the visible area in world units (Unity's `orthographicSize`). **Zero keeps the legacy behavior.** |
| `ViewportSize` | `Vector2` | `(0, 0)` | The logical game resolution the camera projects into (e.g. `(320, 180)`). When its height is zero it falls back to `2 × OrthographicSize`. |
| `RenderScale` | `float` | `1` | Render-to-game resolution ratio for pixel-art upscaling (`4` = a 320×180 game view on a 1280×720 window). |

The view matrix's scale is computed as:

```
projectionScale = (viewportHeight × Zoom) / (2 × OrthographicSize) × RenderScale
```

because the projection and the conversions share the same matrix, `ScreenToWorld`/`WorldToScreen` remain exact inverses of rendering under any combination of zoom and render scale.

### VisibleWorldHeight

```csharp
public float VisibleWorldHeight => OrthographicSize > 0f ? (2f * OrthographicSize) / Zoom : ViewportSize.Y;
```

The height of the visible world in world units, accounting for zoom — handy for level design and camera bounds.

### Example — 320×180 pixel-art game on a 1280×720 window

```csharp
camera.OrthographicSize = 90f;              // visible world height = 180 world units
camera.ViewportSize = new Vector2(320, 180); // logical game resolution
camera.RenderScale = 4f;                    // 4x upscale to the backbuffer
```

With `OrthographicSize = 90` and `ViewportSize.Y = 180`, the projection scale at zoom 1 is exactly `1` — one world unit maps to one game pixel, which is then upscaled 4× by the renderer. Zooming in (e.g. `Zoom = 2f`) halves the visible world (`VisibleWorldHeight = 90`).

> **Backwards compatibility:** with `OrthographicSize == 0` nothing changes — the legacy `Zoom`-only behavior is preserved.

## CameraComponent (Entity Anchor)

`CameraComponent` is a built-in entity component that makes an entity **the thing the camera is attached to**. It deliberately contains **no input handling** — movement belongs to whatever drives the entity (player code, a follow routine, WASD handlers, physics, etc.). Its entire job is to keep the camera anchored to the owning entity:

- **On attach** — creates a `Camera` and registers it as `Camera.MainCamera`.
- **LateUpdate** (every frame, *after* all regular updates) — syncs `_camera.Position = Owner.Position`, and rotation too when `SyncRotation` is true. This guarantees the camera always sees the entity's final position for that frame.
- **On detach** — disposes the camera and clears `MainCamera` if it still points at it.

### Properties

| Property | Type | Default | Description |
|---|---|---|---|
| `Camera` | `Camera` | — | The camera owned by this component. |
| `SyncRotation` | `bool` | `true` | Whether the camera's rotation is synced from the owner each frame. |
| `Zoom` | `float` | `1` | Pass-through to the camera; not driven by the owner, so it survives per-frame position syncs. |
| `OrthographicSize` | `float` | `0` | Pass-through (see Orthographic Projection). |
| `ViewportSize` | `Vector2` | `(0, 0)` | Pass-through (see Orthographic Projection). |
| `RenderScale` | `float` | `1` | Pass-through (see Orthographic Projection). |

### Example — camera anchored to an entity

```csharp
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Components.BuiltIn;

var cameraEntity = new Entity();
cameraEntity.Position = new Vector2(400, 300);
var cam = cameraEntity.AddComponent<CameraComponent>();
cam.OrthographicSize = 90f;
cam.ViewportSize = new Vector2(320, 180);
cam.RenderScale = 4f;
scene.Add(cameraEntity);

// Later — move the entity and the camera follows automatically in LateUpdate:
cameraEntity.Position = player.Position;
```

### XML scenes

`CameraComponent` is registered with the serializer as `"CameraComponent"`, so any scene file can get a working camera by attaching it to a plain entity — no game-layer code needed:

```xml
<EntityDefinition Type="Entity" Id="mainCamera">
    <Position X="400" Y="300" />
    <Components>
        <Component Type="CameraComponent">
            <Properties>
                <Property Name="OrthographicSize" Value="90" />
                <Property Name="ViewportSize" Value="320,180" />
                <Property Name="RenderScale" Value="4" />
            </Properties>
        </Component>
    </Components>
</EntityDefinition>
```

## Advanced Features

> **Tip:** If your camera is driven by a `CameraComponent`, you don't need to write any of this follow code — moving the owning entity is enough, and the sync happens in `LateUpdate`.

### Camera Follow Behavior

You can easily implement camera follow behavior by updating the camera's position to match an entity:

```csharp
// In your Update method
public void Update(GameTime gameTime)
{
    // Make the camera follow the player with some smoothing
    Vector2 targetPosition = player.Position;
    float smoothFactor = 0.1f;
    
    Camera.MainCamera.Position = Vector2.Lerp(
        Camera.MainCamera.Position,
        targetPosition,
        smoothFactor);
}
```

### Camera Shake Effect

Camera shake can be implemented by adding a random offset to the camera position:

```csharp
private float shakeIntensity = 0;
private float shakeDuration = 0;
private Random random = new Random();

public void Update(GameTime gameTime)
{
    // Update camera shake
    if (shakeDuration > 0)
    {
        float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
        shakeDuration -= deltaTime;
        
        if (shakeDuration <= 0)
        {
            shakeIntensity = 0;
        }
        else
        {
            // Add random offset to camera position
            float offsetX = (float)(random.NextDouble() * 2 - 1) * shakeIntensity;
            float offsetY = (float)(random.NextDouble() * 2 - 1) * shakeIntensity;
            Camera.MainCamera.Position += new Vector2(offsetX, offsetY);
        }
    }
}

public void StartShake(float intensity, float duration)
{
    shakeIntensity = intensity;
    shakeDuration = duration;
}
```

### Camera Boundaries

To restrict the camera to specific boundaries:

```csharp
public void Update(GameTime gameTime)
{
    // Update camera position

    // Then clamp it to boundaries
    Vector2 minBoundary = new Vector2(0, 0);
    Vector2 maxBoundary = new Vector2(worldWidth, worldHeight);
    
    Camera.MainCamera.Position = new Vector2(
        MathHelper.Clamp(Camera.MainCamera.Position.X, minBoundary.X, maxBoundary.X),
        MathHelper.Clamp(Camera.MainCamera.Position.Y, minBoundary.Y, maxBoundary.Y)
    );
}
```

## Integration with Entity System

The camera can be used alongside the entity system to provide a clear view of your game world:

```csharp
// Create a camera entity
var cameraEntity = new Entity("MainCamera");
var camera = new Camera();
cameraEntity.Position = new Vector2(400, 300); // Center of screen
camera.SetAsMainCamera();

// Follow a specific entity
public void Update(GameTime gameTime)
{
    // Make camera follow player
    cameraEntity.Position = playerEntity.Position;
}
```
