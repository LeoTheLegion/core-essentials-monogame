using System;

namespace CoreEssentials.Timing;

/// <summary>
/// Provides timing-related functionality, such as tracking the time elapsed between frames.
/// </summary>
public static class Time
{
    /// <summary>
    /// Gets the time in milliseconds it took to complete the last frame.
    /// </summary>
    public static double DeltaTime { get; private set; }

    /// <summary>
    /// Sets the delta time (in milliseconds) for the current frame.
    /// This method is intended for internal use by the main game loop (e.g., MainGame.Update).
    /// </summary>
    /// <param name="deltaTime">The time in milliseconds it took to complete the last frame. Must be non-negative.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="deltaTime"/> is negative.</exception>
    internal static void SetDeltaTime(double deltaTime)
    {
        if (deltaTime < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(deltaTime), "Delta time cannot be negative.");
        }
        DeltaTime = deltaTime;
    }
}
