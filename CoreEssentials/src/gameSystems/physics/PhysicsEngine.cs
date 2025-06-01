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
        private int _scale;
        private World _world;

        /// <summary>
        /// The body pool that manages recycling of physics bodies.
        /// </summary>
        private WorldPool _worldPool;

        /// <summary>
        /// Gets or sets the pixel-to-meter scale factor for the physics world.
        /// This determines how physics units map to rendering units.
        /// </summary>
        public int Scale => _scale;

        /// <summary>
        /// Gets all bodies currently in the physics world.
        /// </summary>
        public BodyCollection Bodies => _world.BodyList;

        /// <summary>
        /// Initializes a new instance of the PhysicsEngine class.
        /// Sets up gravity and creates a physics world.
        /// </summary>
        public PhysicsEngine() : this(0)
        {
        }

        /// <summary>
        /// Initializes a new instance of the PhysicsEngine class with a specified scale.
        /// </summary>
        /// <param name="scale">The pixel-to-meter scale factor for the physics world.</param>
        public PhysicsEngine(int scale)
        {
            _scale = scale;

            _world = new World();
            _world.Gravity = new(0, 9.8f);

            // enable multithreading
            _world.ContactManager.VelocityConstraintsMultithreadThreshold = 256;
            _world.ContactManager.PositionConstraintsMultithreadThreshold = 256;
            _world.ContactManager.CollideMultithreadThreshold = 256;

            _worldPool = new WorldPool(_world);
        }

        /// <summary>
        /// Sets the pixel-to-meter scale factor for the physics world.
        /// </summary>
        /// <param name="scale">The new scale factor.</param>
        public void SetScale(int scale)
        {
            _scale = scale;
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

            _world.Step((float)gameTime.ElapsedGameTime.TotalSeconds * speed);

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
