using CoreEssentials.Debugging;
using CoreEssentials.GameSystems;
using Microsoft.Xna.Framework;
using nkast.Aether.Physics2D.Dynamics;
using System;


namespace CoreEssentials.GameSystems.Physics
{
    /// <summary>
    /// Provides physics simulation capabilities for the game.
    /// This class manages a physics world using the Aether Physics2D engine,
    /// and provides methods for creating, manipulating, and simulating physics bodies.
    /// </summary>
    public class PhysicsEngine : GameSystem, IFixedUpdateGameSystem
    {
        private const float SIM_SPEED = 2;
        private World _world;

        /// <summary>
        /// The body pool that manages recycling of physics bodies.
        /// </summary>
        private WorldPool _worldPool;

        /// <summary>
        /// The configuration for the physics solver.
        /// </summary>
        private PhysicsConfig _config = new PhysicsConfig();

        /// <summary>
        /// Gets the physics configuration. Modify properties to tune performance vs accuracy.
        /// </summary>
        public PhysicsConfig Config => _config;

        /// <summary>
        /// Gets all bodies currently in the physics world.
        /// </summary>
        public BodyCollection Bodies => _world.BodyList;

        /// <summary>
        /// Initializes a new instance of the PhysicsEngine class.
        /// Sets up gravity and creates a physics world.
        /// </summary>
        public PhysicsEngine()
        {
            _world = new World();
            _world.Gravity = new(0, 9.8f);

            // enable multithreading
            _world.ContactManager.VelocityConstraintsMultithreadThreshold = 256;
            _world.ContactManager.PositionConstraintsMultithreadThreshold = 256;
            _world.ContactManager.CollideMultithreadThreshold = 256;

            _worldPool = new WorldPool(_world);
        }

        /// <summary>
        /// Initializes a new instance of the PhysicsEngine class with a configuration object.
        /// </summary>
        /// <param name="config">The physics configuration to use.</param>
        public PhysicsEngine(PhysicsConfig config) : this()
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        /// <summary>
        /// Updates the physics simulation with a fixed time step.
        /// This ensures physics calculations are stable regardless of frame rate.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public void FixedUpdate(GameTime gameTime)
        {
            var adjust = AdjustSimSpeed();

            var speed = SIM_SPEED * adjust;

            // Apply user-configured solver iterations
            var iterations = new nkast.Aether.Physics2D.Dynamics.SolverIterations
            {
                VelocityIterations = _config.VelocityIterations,
                PositionIterations = _config.PositionIterations,
                TOIVelocityIterations = _config.VelocityIterations,
                TOIPositionIterations = _config.PositionIterations * 2
            };

            // Apply CCD setting (Note: This is a global setting in Aether Physics2D.
            // If using multiple PhysicsEngine instances, they will share this setting.)
            nkast.Aether.Physics2D.Settings.ContinuousPhysics = _config.ContinuousPhysics;

            _world.Step((float)gameTime.ElapsedGameTime.TotalSeconds * speed, ref iterations);

            int bodies = _world.BodyList.Count;
            int activeBodies = bodies - _worldPool.Count;
            int inactiveBodies = _worldPool.Count;

            Debug.StickyLog.Log("Physics Engine Bodies", String.Format("{0} ({1} Pool)", activeBodies, inactiveBodies));
            Debug.StickyLog.Log("Physics Engine Sim Speed", String.Format("{0}", Math.Round(speed, 2)));
        }

        /// <summary>
        /// Returns a float that adjusts the simulation speed based on the current state of the simulation.
        /// Bodies above a certain threshold will slow down the simulation.
        /// </summary>
        /// <returns>A float value representing the adjusted simulation speed.</returns>
        private float AdjustSimSpeed()
        {
            var bodies = Bodies.Count - _worldPool.Count;

            if (bodies < 1000)
            {
                return 1;
            }

            bodies -= 1000;

            var c = 1 - (bodies / 1000f);

            return Math.Max(c, 0.1f);
        }

        /// <summary>
        /// Creates a new body in the physics world.
        /// </summary>
        /// <param name="vector">The initial position of the body.</param>
        /// <param name="rot">The initial rotation of the body in radians.</param>
        /// <param name="type">The type of body: Static, Dynamic, or Kinematic.</param>
        /// <returns>The newly created physics body.</returns>
        public Body CreateBody(Vector2 vector, float rot, BodyType type)
        {
            return _worldPool.CreateBody(vector, rot, type);
        }

        /// <summary>
        /// Removes a body from the physics world.
        /// The body is actually returned to the pool for later reuse.
        /// </summary>
        /// <param name="body">The body to remove.</param>
        public void Destroy(Body body)
        {
            this._worldPool.DestroyBody(body);
        }
    }
}
