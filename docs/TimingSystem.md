# Timing System

The Timing system in CoreEssentials-MonoGame provides a way to access frame-rate independent timing information, crucial for smooth and consistent game logic and animations.

## `Time` Class

The static `Time` class is the central point for accessing timing data.

### `Time.DeltaTime`

-   **Type**: `double`
-   **Description**: Gets the time in seconds it took to complete the last frame. This value is essential for frame-rate independent movement and calculations. For example, if you want an object to move 100 units per second, you would update its position by `100 * Time.DeltaTime` each frame.
-   **Usage**:

    ```csharp
    // Example: Moving an entity based on DeltaTime
    public class MyEntity : Entity
    {
        public float Speed = 100f;

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            // Assuming movement along the X axis
            Position += new Vector2(Speed * (float)Time.DeltaTime, 0);
        }
    }
    ```

### `Time.SetDeltaTime(double deltaTime)`

-   **Accessibility**: `internal` (intended for use by the CoreEssentials library itself, primarily `MainGame.Update`)
-   **Description**: This method is called by the main game loop (e.g., `MainGame.Update`) to set the `DeltaTime` for the current frame. As it's `internal`, it's not meant to be called directly from game-specific code outside the CoreEssentials assembly.
-   **Parameters**:
    -   `deltaTime` (`double`): The time in seconds it took to complete the last frame. Must be non-negative.
-   **Throws**: `ArgumentOutOfRangeException` if `deltaTime` is negative.

## How it Works

The `MainGame` class (or your equivalent game loop manager within the CoreEssentials library) is responsible for calculating the elapsed time since the last frame and then calling `Time.SetDeltaTime()` at the beginning of each `Update` cycle. This makes `Time.DeltaTime` available throughout your game logic for that frame.

## Best Practices

-   **Always use `Time.DeltaTime` for movement and physics calculations**: This ensures that your game behaves consistently across different hardware and frame rates.
-   **Avoid very small `DeltaTime` values in sensitive calculations**: If `DeltaTime` is extremely small (e.g., due to a very high frame rate or a game pause), it might lead to precision issues or unintended behavior in some calculations. Consider clamping or handling such cases if necessary.
-   **Do not call `SetDeltaTime` from your game code**: This method is `internal` to the CoreEssentials library and is managed by the main game loop. Modifying `DeltaTime` directly can lead to unpredictable timing issues.
