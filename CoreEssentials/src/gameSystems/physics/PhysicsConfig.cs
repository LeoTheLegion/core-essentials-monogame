namespace CoreEssentials.GameSystems.Physics
{
    /// <summary>
    /// Configuration settings for the physics simulation solver.
    /// Allows tuning the physics engine for different scenarios to balance accuracy vs performance.
    /// </summary>
    public class PhysicsConfig
    {
        /// <summary>
        /// Number of velocity constraint solver iterations per step.
        /// Higher values provide more accurate velocity resolution but slower performance.
        /// Default: 8 (high accuracy). Recommended for particle systems: 4-6.
        /// </summary>
        public int VelocityIterations { get; set; } = 8;

        /// <summary>
        /// Number of position constraint solver iterations per step.
        /// Higher values provide better constraint satisfaction but slower performance.
        /// Default: 3 (high accuracy). Recommended for particle systems: 2.
        /// </summary>
        public int PositionIterations { get; set; } = 3;

        /// <summary>
        /// Enable continuous collision detection (CCD) to prevent tunneling of fast-moving objects.
        /// Disable for slow-moving or short-lived objects to improve performance.
        /// Default: true. Recommended for particle systems: false.
        /// </summary>
        public bool ContinuousPhysics { get; set; } = true;
    }
}
