using Microsoft.Xna.Framework;
using nkast.Aether.Physics2D.Dynamics;
using System.Collections.Generic;
using System.Linq;
using CoreEssentials.Debugging;

namespace CoreEssentials.GameSystems.Physics
{
    internal class WorldPool
    {
        private World _world;

        private Queue<Body> _worldPool;

        public int Count => _worldPool.Count;

        public WorldPool(World world)
        {
            _world = world;

            _worldPool = new Queue<Body>();
        }

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
