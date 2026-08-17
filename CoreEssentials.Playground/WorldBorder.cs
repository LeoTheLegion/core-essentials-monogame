using System;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem;
using CoreEssentials.GameSystems.Physics.Engines.Aether;
using CoreEssentials.GameSystems.Physics.Types;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

#nullable enable

namespace CoreEssentials.Playground;

public class WorldBorder : Entity
{
    /// <summary>
    /// Gets or sets the size of the world border.
    /// Must be set before OnStart() for proper initialization.
    /// </summary>
    public Vector2 Size { get; set; }

    public WorldBorder(Vector2 position, Vector2 size)
    {
        Position = position;
        Size = size;
        sort = 0;
    }

    // Parameterless constructor for serialization
    public WorldBorder() : this(Vector2.Zero, Vector2.Zero) { }

    public override void OnStart()
    {
        base.OnStart();
        CreateWorldBorder();
    }

    private void CreateWorldBorder()
    {
        if (Size.X <= 0 || Size.Y <= 0)
        {
            Console.WriteLine($"[WorldBorder] Size is invalid: {Size}, skipping border creation");
            return;
        }

        if (EntitySystem == null) return;
        PhysicsEngine? physicsEngineResult = EntitySystem.GetGameSystem<PhysicsEngine>();
        if (physicsEngineResult == null) return;
        PhysicsEngine physicsEngine = physicsEngineResult;

        // Sprint 19 demo: the border must contain BOTH regular ("Player") and VIP ("Vip") balls.
        // The category names are resolved from the engine's PhysicsConfig (Content/PhysicsConfig.xml).
        var config = physicsEngine.Config ?? PhysicsConfig.CreateDefault();
        var allBalls = config.ResolveMask("Player|Vip");

        void Configure(ICollider collider)
        {
            collider.Categories = allBalls;
            collider.CollidesWith = allBalls;
        }

        var borders = new IPhysicsBody[4];

        // Create the left border
        borders[0] = physicsEngine.CreateStatic(new Vector2(Position.X, Position.Y + Size.Y / 2));
        Configure(borders[0].CreateRectangleCollider(new Vector2(1, Size.Y), Vector2.Zero));

        // Create the right border
        borders[1] = physicsEngine.CreateStatic(new Vector2(Position.X + Size.X, Position.Y + Size.Y / 2));
        Configure(borders[1].CreateRectangleCollider(new Vector2(1, Size.Y), Vector2.Zero));

        // Create the top border
        borders[2] = physicsEngine.CreateStatic(new Vector2(Position.X + Size.X / 2, Position.Y));
        Configure(borders[2].CreateRectangleCollider(new Vector2(Size.X, 1), Vector2.Zero));

        // Create the bottom border
        borders[3] = physicsEngine.CreateStatic(new Vector2(Position.X + Size.X / 2, Position.Y + Size.Y));
        Configure(borders[3].CreateRectangleCollider(new Vector2(Size.X, 1), Vector2.Zero));
    }
}
