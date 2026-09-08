using System;

namespace CoreEssentials.Timing;

/// <summary>
/// Tracks elapsed game time against a fixed duration and reports when the deadline is reached.
/// Used for opt-in auto-exit (e.g., smoke-running a scene for N seconds) so a running game can
/// close itself without any manual interaction. Purely value-based: it has no window, game loop,
/// or I/O of its own, which keeps it trivially unit-testable.
/// </summary>
public sealed class AutoExitTimer
{
    private readonly double _durationMs;
    private double _elapsedMs;

    /// <summary>
    /// Creates a timer that expires after the given number of seconds.
    /// </summary>
    /// <param name="durationSeconds">How long (in seconds) before the timer is considered expired. Must be positive.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="durationSeconds"/> is not positive.</exception>
    public AutoExitTimer(double durationSeconds)
    {
        if (durationSeconds <= 0)
            throw new ArgumentOutOfRangeException(nameof(durationSeconds), "Duration must be a positive number of seconds.");

        _durationMs = durationSeconds * 1000.0;
    }

    /// <summary>
    /// Gets the configured duration in milliseconds.
    /// </summary>
    public double DurationMs => _durationMs;

    /// <summary>
    /// Gets the total elapsed time (in milliseconds) that has been ticked into this timer.
    /// </summary>
    public double ElapsedMs => _elapsedMs;

    /// <summary>
    /// Gets a value indicating whether the configured duration has been reached or exceeded.
    /// </summary>
    public bool IsExpired => _elapsedMs >= _durationMs;

    /// <summary>
    /// Advances the timer by one frame's worth of elapsed time.
    /// </summary>
    /// <param name="deltaMs">The elapsed time (in milliseconds) for the current frame. Must be non-negative.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="deltaMs"/> is negative.</exception>
    public void Tick(double deltaMs)
    {
        if (deltaMs < 0)
            throw new ArgumentOutOfRangeException(nameof(deltaMs), "Delta time cannot be negative.");

        _elapsedMs += deltaMs;
    }
}
