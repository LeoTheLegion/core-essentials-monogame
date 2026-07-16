namespace CoreEssentials.Physics.Types;

/// <summary>
/// 🔒 Internal use only. Global configuration for the physics engine.
/// </summary>
public class PhysicsConfig
{
    /// <summary>
    /// Gets or sets the velocity solver iterations (default: 8).
    /// More iterations produce more accurate contact resolution but are slower.
    /// </summary>
    public int VelocityIterations { get; set; } = 8;

    /// <summary>
    /// Gets or sets the position solver iterations (default: 3).
    /// More iterations reduce positional drift but increase computation time.
    /// </summary>
    public int PositionIterations { get; set; } = 3;

    /// <summary>
    /// Gets or sets whether continuous collision detection is enabled globally.
    /// Prevents fast-moving objects from tunneling through static obstacles.
    /// </summary>
    public bool ContinuousCollisionDetection { get; set; } = false;

    /// <summary>
    /// Gets or sets the sub-stepping factor (default: 1).
    /// A value of 2 means each frame is split into 2 smaller steps for better accuracy.
    /// </summary>
    public int SubSteppingFactor { get; set; } = 1;

    /// <summary>
    /// Gets or sets the time step used when sub-stepping is enabled (default: 1/60).
    /// Must be positive. Common values are 1/60, 1/120, 1/240.
    /// </summary>
    public float TimeStep { get; set; } = 1f / 60f;

    /// <summary>
    /// Gets or sets whether the world should auto-sleep bodies that have been still for a period.
    /// Improves performance by skipping simulation of idle bodies.
    /// </summary>
    public bool AutoSleep { get; set; } = true;

    /// <summary>
    /// Gets or sets the sleep time threshold in seconds (default: 0.5).
    /// Bodies that have been still for this duration will go to sleep if auto-sleep is enabled.
    /// </summary>
    public float SleepThreshold { get; set; } = 0.5f;
}
