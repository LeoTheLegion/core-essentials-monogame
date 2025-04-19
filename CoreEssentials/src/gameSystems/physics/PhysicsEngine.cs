using CoreEssentials.Debugging;
using CoreEssentials.GameSystems;
using Microsoft.Xna.Framework;
using nkast.Aether.Physics2D.Dynamics;
using System;


namespace CoreEssentials.GameSystems.Physics
{
    public class PhysicsEngine : GameSystem, IFixedUpdateGameSystem
    {
        private const float SIM_SPEED = 2;
        private int _scale;
        private World _world;

        private WorldPool _worldPool;
        public int Scale => _scale;

        public BodyCollection Bodies => _world.BodyList;
        
        public PhysicsEngine(MainGame mainGame) : base(mainGame)
        {
            Reset();
        }

        public PhysicsEngine(MainGame mainGame,int scale) : this(mainGame)
        {
            _scale = scale;
        }

        public void SetScale(int scale)
        {
            _scale = scale;
        }

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

        //returns a float that adjusts the simulation speed based on the current state of the simulation
        //bodies above a certain threshold will slow down the simulation
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

        public Body CreateBody(Vector2 vector, float rot, BodyType type)
        {
            return _worldPool.CreateBody(vector, rot, type);
        }

        public void Destroy(Body body)
        {
            this._worldPool.DestroyBody(body);
        }

        public void Reset()
        {
            _world = new World();
            _world.Gravity = new(0, 9.8f);
            _scale = 0;


            // enable multithreading
            _world.ContactManager.VelocityConstraintsMultithreadThreshold = 256;
            _world.ContactManager.PositionConstraintsMultithreadThreshold = 256;
            _world.ContactManager.CollideMultithreadThreshold = 256;

            _worldPool = new WorldPool(_world);
        }
    }
}
