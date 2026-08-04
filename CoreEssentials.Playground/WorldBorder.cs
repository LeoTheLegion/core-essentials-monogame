using System;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem;
using CoreEssentials.GameSystems.Physics.Engines.Aether;
using CoreEssentials.GameSystems.Physics.Types;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace CoreEssentials.Playground;

public class WorldBorder : Entity
{
    private Vector2 _size;
    private IPhysicsBody[] _borderBodies;
    public WorldBorder(Vector2 position, Vector2 size)
    {
        Position = position;
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

        _borderBodies = new IPhysicsBody[4];

        // Create the left border
        _borderBodies[0] = physicsEngine.CreateStatic(new Vector2(Position.X, Position.Y + _size.Y / 2));
        _borderBodies[0].CreateRectangleCollider(new Vector2(1, _size.Y), Vector2.Zero);

        // Create the right border
        _borderBodies[1] = physicsEngine.CreateStatic(new Vector2(Position.X + _size.X, Position.Y + _size.Y / 2));
        _borderBodies[1].CreateRectangleCollider(new Vector2(1, _size.Y), Vector2.Zero);

        // Create the top border
        _borderBodies[2] = physicsEngine.CreateStatic(new Vector2(Position.X + _size.X / 2, Position.Y));
        _borderBodies[2].CreateRectangleCollider(new Vector2(_size.X, 1), Vector2.Zero);

        // Create the bottom border
        _borderBodies[3] = physicsEngine.CreateStatic(new Vector2(Position.X + _size.X / 2, Position.Y + _size.Y));
        _borderBodies[3].CreateRectangleCollider(new Vector2(_size.X, 1), Vector2.Zero);
    }
}
