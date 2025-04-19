using Microsoft.Xna.Framework;
using nkast.Aether.Physics2D.Dynamics;
using System.Collections.Generic;
using System.Linq;
using CoreEssentials.Debugging;

namespace CoreEssentials.GameSystems.Physics
{
    /// <summary>
    /// Manages a pool of physics bodies to reduce the overhead of creating and destroying bodies.
    /// This class recycles disabled bodies instead of completely removing them from the physics world.
    /// </summary>
    public class WorldPool
    {
        private World _world;

        private Queue<Body> _worldPool;

        /// <summary>
        /// Gets the number of available bodies in the pool.
        /// </summary>
        public int Count => _worldPool.Count;

        /// <summary>
        /// Initializes a new instance of the WorldPool class.
        /// </summary>
        /// <param name="world">The physics world to which bodies belong.</param>
        public WorldPool(World world)
        {
            _world = world;

            _worldPool = new Queue<Body>();
        }

        /// <summary>
        /// Creates or reuses a body from the pool with the specified parameters.
        /// </summary>
        /// <param name="vector">The position of the body.</param>
        /// <param name="rot">The rotation of the body.</param>
        /// <param name="type">The type of the body (static, dynamic, or kinematic).</param>
        /// <returns>A new or recycled body with the specified parameters.</returns>
        public Body CreateBody(Vector2 vector, float rot, BodyType type)
        {
            Debug.StickyLog.Log("Pool Size:", _worldPool.Count.ToString());

            if (_worldPool.Count > 0)
            {
                var body = _worldPool.Dequeue();
                body.Enabled = true;
                body.Position = new (vector.X,vector.Y);
                body.BodyType = type;
                return body;
            }

            var b = _world.CreateBody(vector, rot, type);

            return b;
        }

        /// <summary>
        /// Returns a body to the pool for later reuse instead of destroying it.
        /// Removes all fixtures and disables the body.
        /// </summary>
        /// <param name="body">The body to return to the pool.</param>
        public void DestroyBody(Body body)
        {
            body.Enabled = false;

            Fixture[] fixtures = body.FixtureList.ToArray();

            foreach (var fixture in fixtures)
            {
                body.Remove(fixture);
            }

            _worldPool.Enqueue(body);
        }
    }
}
