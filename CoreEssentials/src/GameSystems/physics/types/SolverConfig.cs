namespace CoreEssentials.GameSystems.Physics.Types;

/// <summary>
/// 🔒 Internal use only. Configuration for the physics solver iterations.
/// </summary>
public class SolverConfig
{
    /// <summary>
    /// Gets or sets the number of velocity solver iterations (default: 8).
    /// More iterations = more accurate contacts but slower simulation.
    /// </summary>
    public int VelocityIterations { get; set; } = 8;

    /// <summary>
    /// Gets or sets the number of position solver iterations (default: 3).
    /// More iterations = less positional drift but slower simulation.
    /// </summary>
    public int PositionIterations { get; set; } = 3;

    /// <summary>
    /// Gets or sets whether continuous collision detection is enabled.
    /// CCD prevents fast-moving objects from tunneling through static objects.
    /// </summary>
    public bool ContinuousCollisionDetection { get; set; } = false;
}
