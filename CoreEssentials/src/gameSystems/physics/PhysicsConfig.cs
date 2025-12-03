namespace CoreEssentials.GameSystems.Physics
{
    /// <summary>
    /// Configuration settings for the physics simulation solver.
    /// Allows tuning the physics engine for different scenarios to balance accuracy vs performance.
    /// </summary>
    public class PhysicsConfig
    {
        /// <summary>
        /// Gets or sets the pixel-to-meter scale factor for the physics world.
        /// This determines how physics units map to rendering units.
        /// Default: 0.
        /// </summary>
        public int Scale { get; set; } = 0;

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
        /// Note: This is a global setting in the underlying physics library. If using multiple
        /// PhysicsEngine instances, they will share this setting.
        /// </summary>
        public bool ContinuousPhysics { get; set; } = true;
    }
}
