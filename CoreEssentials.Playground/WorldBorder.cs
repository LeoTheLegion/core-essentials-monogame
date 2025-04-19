using System;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem;
using CoreEssentials.GameSystems.Physics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using nkast.Aether.Physics2D.Dynamics;

namespace CoreEssentials.Playground;

public class WorldBorder : Entity
{
    private Vector2 _size;
    private Body[] _borderBodies;
    public WorldBorder(Vector2 position, Vector2 size)
    {
        _position = position;
        _size = size;
        sort = 0;
    }

    public override void OnStart()
    {
        base.OnStart();
        CreateWorldBorder();
    }

    private void CreateWorldBorder()
    {
        PhysicsEngine physicsEngine = EntitySystem.GetGameSystem<PhysicsEngine>();

        _borderBodies = new Body[4];
        
        // Create the left border
        _borderBodies[0] = physicsEngine.CreateBody(new Vector2(_position.X, _position.Y + _size.Y / 2), 0, BodyType.Static);
        _borderBodies[0].CreateRectangle(1, _size.Y, 1, Vector2.Zero);

        // Create the right border
        _borderBodies[1] = physicsEngine.CreateBody(new Vector2(_position.X + _size.X, _position.Y + _size.Y / 2), 0, BodyType.Static);
        _borderBodies[1].CreateRectangle(1, _size.Y, 1, Vector2.Zero);

        // Create the top border
        _borderBodies[2] = physicsEngine.CreateBody(new Vector2(_position.X + _size.X / 2, _position.Y), 0, BodyType.Static);
        _borderBodies[2].CreateRectangle(_size.X, 1, 1, Vector2.Zero);

        // Create the bottom border
        _borderBodies[3] = physicsEngine.CreateBody(new Vector2(_position.X + _size.X / 2, _position.Y + _size.Y), 0, BodyType.Static);
        _borderBodies[3].CreateRectangle(_size.X, 1, 1, Vector2.Zero);
    }
}
