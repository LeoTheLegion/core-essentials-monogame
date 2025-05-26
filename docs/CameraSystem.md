# Camera System

The CoreEssentials-MonoGame Camera system provides an orthographic camera implementation that can be used to transform the game view, enabling features like zooming, panning, and rotation.

## Features

- Orthographic camera for 2D games
- Camera positioning, rotation, and zooming
- Static MainCamera property for easy access to the current camera
- Easy conversion between screen and world coordinates

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

## Advanced Features

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
