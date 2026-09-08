using System;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Components;
using CoreEssentials.GameSystems.Physics.Engines.Aether;
using CoreEssentials.GameSystems.Physics.Types;
using Microsoft.Xna.Framework;

namespace CoreEssentials.Playground.Components;

/// <summary>
/// Builds the four static physics borders (left, right, top, bottom) that contain the balls in
/// the physics demo. This ports the border construction that used to live in the hand-written
/// <c>WorldBorder</c> entity, so a border can now be declared purely from data:
/// <code>
/// &lt;EntityDefinition Type="...GameObjectEntity"&gt;
///   &lt;Components&gt;
///     &lt;Component Type="WorldBorderComponent"&gt;
///       &lt;Properties&gt;
///         &lt;Property Name="Size" Value="1280,720" /&gt;
///       &lt;/Properties&gt;
///     &lt;/Component&gt;
///   &lt;/Components&gt;
/// &lt;/EntityDefinition&gt;
/// </code>
/// On attach it creates four static bodies sized to <see cref="Size"/> at the owner's position, and
/// gives each a collision mask that contains BOTH regular ("Player") and VIP ("Vip") balls — the
/// category names are resolved from the engine's <see cref="PhysicsConfig"/>. A regression in the
/// mask would silently let balls escape the arena.
/// </summary>
public class WorldBorderComponent : EntityComponent
{
    /// <summary>The size of the world border (pixels). Must be non-zero for borders to be created.</summary>
    public Vector2 Size { get; set; }

    /// <summary>
    /// The pipe-separated list of named collision categories the border must contain. Resolved from
    /// the engine's <see cref="PhysicsConfig"/> so both regular and VIP balls stay inside.
    /// </summary>
    public string BorderCategoryMask { get; set; } = "Player|Vip";

    /// <inheritdoc />
    public override void OnAttach()
    {
        CreateWorldBorder();
    }

    private void CreateWorldBorder()
    {
        if (Size.X <= 0 || Size.Y <= 0)
        {
            Console.WriteLine($"[WorldBorderComponent] Size is invalid: {Size}, skipping border creation");
            return;
        }

        var physicsEngine = EntitySystem?.GetGameSystem<PhysicsEngine>();
        if (physicsEngine == null) return;

        // The border must contain BOTH regular ("Player") and VIP ("Vip") balls. The category names
        // are resolved from the engine's PhysicsConfig (Content/PhysicsConfig.xml).
        var config = physicsEngine.Config ?? PhysicsConfig.CreateDefault();
        var allBalls = config.ResolveMask(BorderCategoryMask);

        void Configure(ICollider collider)
        {
            collider.Categories = allBalls;
            collider.CollidesWith = allBalls;
        }

        // Create the left border
        var left = physicsEngine.CreateStatic(new Vector2(Owner.Position.X, Owner.Position.Y + Size.Y / 2));
        Configure(left.CreateRectangleCollider(new Vector2(1, Size.Y), Vector2.Zero));

        // Create the right border
        var right = physicsEngine.CreateStatic(new Vector2(Owner.Position.X + Size.X, Owner.Position.Y + Size.Y / 2));
        Configure(right.CreateRectangleCollider(new Vector2(1, Size.Y), Vector2.Zero));

        // Create the top border
        var top = physicsEngine.CreateStatic(new Vector2(Owner.Position.X + Size.X / 2, Owner.Position.Y));
        Configure(top.CreateRectangleCollider(new Vector2(Size.X, 1), Vector2.Zero));

        // Create the bottom border
        var bottom = physicsEngine.CreateStatic(new Vector2(Owner.Position.X + Size.X / 2, Owner.Position.Y + Size.Y));
        Configure(bottom.CreateRectangleCollider(new Vector2(Size.X, 1), Vector2.Zero));
    }
}
