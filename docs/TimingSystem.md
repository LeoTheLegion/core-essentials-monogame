# Timing System

The Timing system in CoreEssentials-MonoGame provides a way to access frame-rate independent timing information, crucial for smooth and consistent game logic and animations.

## `Time` Class

The static `Time` class is the central point for accessing timing data.

### `Time.DeltaTime`

-   **Type**: `double`
-   **Description**: Gets the time in **milliseconds** it took to complete the last frame. This value is essential for frame-rate independent movement and calculations.
-   **Usage**:
    To use `DeltaTime` for calculations that expect seconds (e.g., speed in units per second), you will need to convert it:
    `float deltaTimeInSeconds = (float)Time.DeltaTime / 1000.0f;`

    ```csharp
    // Example: Moving an entity based on DeltaTime (converted to seconds)
    public class MyEntity : Entity
    {
        public float Speed = 100f; // Speed in units per second

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            float deltaTimeInSeconds = (float)Time.DeltaTime / 1000.0f;
            // Assuming movement along the X axis
            Position += new Vector2(Speed * deltaTimeInSeconds, 0);
        }
    }
    ```

### `Time.SetDeltaTime(double deltaTime)`

-   **Accessibility**: `internal` (intended for use by the CoreEssentials library itself, primarily `MainGame.Update`)
-   **Description**: This method is called by the main game loop (e.g., `MainGame.Update`) to set the `DeltaTime` for the current frame (in **milliseconds**). As it's `internal`, it's not meant to be called directly from game-specific code outside the CoreEssentials assembly.
-   **Parameters**:
    -   `deltaTime` (`double`): The time in **milliseconds** it took to complete the last frame. Must be non-negative.
-   **Throws**: `ArgumentOutOfRangeException` if `deltaTime` is negative.

## How it Works

The `MainGame` class (or your equivalent game loop manager within the CoreEssentials library) is responsible for calculating the elapsed time since the last frame (in milliseconds, e.g., using `gameTime.ElapsedGameTime.TotalMilliseconds`) and then calling `Time.SetDeltaTime()` at the beginning of each `Update` cycle. This makes `Time.DeltaTime` (in milliseconds) available throughout your game logic for that frame.

## Best Practices

-   **Convert to seconds when needed**: When using `Time.DeltaTime` with physics or speed calculations that are defined in units per second, remember to convert `Time.DeltaTime` from milliseconds to seconds (divide by 1000.0f).
-   **Avoid very small `DeltaTime` values in sensitive calculations**: If `DeltaTime` is extremely small (e.g., due to a very high frame rate or a game pause), it might lead to precision issues or unintended behavior in some calculations. Consider clamping or handling such cases if necessary.
-   **Do not call `SetDeltaTime` from your game code**: This method is `internal` to the CoreEssentials library and is managed by the main game loop. Modifying `DeltaTime` directly can lead to unpredictable timing issues.
