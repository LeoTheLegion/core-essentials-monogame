using System;
using Microsoft.Xna.Framework;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Components;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Components.BuiltIn;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Serialization;
using CoreEssentials.GameSystems.Physics.Engines.Aether;
using CoreEssentials.GameSystems.Physics.Types;

namespace CoreEssentials.Playground;

/// <summary>
/// Declaratively reproduces the physics-demo ball spawning that used to live in a scene
/// subclass, so the scene can be pure data. On attach it spawns a configurable number of
/// "regular" balls at random positions within a spawn area, plus a fixed set of "VIP" balls
/// (each with its own id, position, color and scale), applies per-group collision filters
/// resolved from the scene's <see cref="PhysicsConfig"/>, kicks each ball with a random
/// impulse, and optionally creates a world border.
/// <code>
/// &lt;Component Type="PhysicsSpawnComponent"&gt;
///   &lt;Properties&gt;
///     &lt;Property Name="BallPrefabName" Value="BallPrefab" /&gt;
///     &lt;Property Name="RegularBallCount" Value="5" /&gt;
///     &lt;Property Name="RegularCategory" Value="Player" /&gt;
///     &lt;Property Name="VipBallIds" Value="vip_ball_blue,vip_ball_green,vip_ball_red" /&gt;
///     &lt;Property Name="VipBallPositions" Value="640,360;580,300;700,420" /&gt;
///     &lt;Property Name="VipBallColors" Value="Blue,Green,Red" /&gt;
///     &lt;Property Name="VipCategory" Value="Vip" /&gt;
///     &lt;Property Name="CreateWorldBorder" Value="true" /&gt;
///   &lt;/Properties&gt;
/// &lt;/Component&gt;
/// </code>
/// Every external side effect (instantiate, filter, impulse, VIP config, world border) is a
/// small <c>protected virtual</c> seam so unit tests can observe the requests without a live
/// physics engine or prefab assets.
/// </summary>
public class PhysicsSpawnComponent : EntityComponent
{
    // ── Regular balls ────────────────────────────────────────────────────────────

    /// <summary>The registered prefab name used to instantiate every ball.</summary>
    public string BallPrefabName { get; set; } = "BallPrefab";

    /// <summary>How many regular balls to spawn at random positions.</summary>
    public int RegularBallCount { get; set; } = 5;

    /// <summary>Inclusive lower bound of the random spawn area (pixels).</summary>
    public Vector2 SpawnAreaMin { get; set; } = new(32, 32);

    /// <summary>Inclusive upper bound of the random spawn area (pixels).</summary>
    public Vector2 SpawnAreaMax { get; set; } = new(1248, 688);

    /// <summary>The named collision category for regular balls, resolved from PhysicsConfig.</summary>
    public string RegularCategory { get; set; } = "Player";

    /// <summary>Half-range of the random impulse applied to each regular ball (uniform in [-r, r]).</summary>
    public float RegularImpulseHalfRange { get; set; } = 5f;

    // ── VIP balls (parallel comma/semicolon-separated lists) ─────────────────────

    /// <summary>Comma-separated ids for the VIP balls.</summary>
    public string VipBallIds { get; set; } = "vip_ball_blue,vip_ball_green,vip_ball_red";

    /// <summary>Semicolon-separated "x,y" positions for the VIP balls (aligned with <see cref="VipBallIds"/>).</summary>
    public string VipBallPositions { get; set; } = "640,360;580,300;700,420";

    /// <summary>Comma-separated colors for the VIP balls (aligned with <see cref="VipBallIds"/>).</summary>
    public string VipBallColors { get; set; } = "Blue,Green,Red";

    /// <summary>Uniform scale applied to every VIP ball.</summary>
    public float VipBallScale { get; set; } = 2.0f;

    /// <summary>The named collision category for VIP balls, resolved from PhysicsConfig.</summary>
    public string VipCategory { get; set; } = "Vip";

    /// <summary>Half-range of the random impulse applied to each VIP ball (uniform in [-r, r]).</summary>
    public float VipImpulseHalfRange { get; set; } = 7.5f;

    // ── World border ─────────────────────────────────────────────────────────────

    /// <summary>Whether to create a world border entity that contains the balls.</summary>
    public bool CreateWorldBorder { get; set; } = true;

    /// <summary>The size of the world border (pixels).</summary>
    public Vector2 WorldBorderSize { get; set; } = new(1280, 720);

    private readonly Random _random = new();

    /// <inheritdoc />
    public override void OnAttach()
    {
        Spawn();
    }

    /// <summary>
    /// Spawns all configured balls (and the optional world border). Exposed publicly so it can be
    /// invoked directly from tests.
    /// </summary>
    public void Spawn()
    {
        // Regular balls: random position, regular collision filter, random impulse.
        for (int i = 0; i < RegularBallCount; i++)
        {
            var ball = InstantiateBall(RandomPosition());
            if (ball == null) continue;

            ApplyCollisionFilter(ball, ResolveCategory(RegularCategory), ResolveCategory(RegularCategory));
            ApplyImpulse(ball, RandomImpulse(RegularImpulseHalfRange));
        }

        // VIP balls: fixed id/position/color/scale, VIP collision filter, random impulse.
        var ids = SplitList(VipBallIds);
        var positions = VipBallPositions.Split(';');
        var colors = SplitList(VipBallColors);

        for (int i = 0; i < ids.Length; i++)
        {
            var position = i < positions.Length ? SerializationUtils.ParseVector2FromString(positions[i].Trim()) : Vector2.Zero;
            var color = i < colors.Length ? SerializationUtils.ParseColor(colors[i].Trim()) : Color.White;

            var ball = InstantiateBall(position);
            if (ball == null) continue;

            ConfigureVipBall(ball, ids[i], color, VipBallScale);
            ApplyCollisionFilter(ball, ResolveCategory(VipCategory), ResolveCategory(VipCategory));
            ApplyImpulse(ball, RandomImpulse(VipImpulseHalfRange));
        }

        if (CreateWorldBorder)
            CreateWorldBorderEntity(Vector2.Zero, WorldBorderSize);
    }

    // ── Testability seams ────────────────────────────────────────────────────────

    /// <summary>
    /// Instantiates the ball prefab at the given position. Virtual so unit tests can observe the
    /// spawn without a live EntitySystem or registered prefab.
    /// </summary>
    protected virtual Entity? InstantiateBall(Vector2 position)
        => EntitySystem?.Instantiate(BallPrefabName, position);

    /// <summary>
    /// Resolves a named collision category from the scene's physics config. Returns null when no
    /// physics engine/config is available (e.g. in unit tests). Virtual so tests can stub it.
    /// </summary>
    protected virtual CollisionCategory? ResolveCategory(string name)
    {
        var system = EntitySystem;
        if (system == null) return null;

        try
        {
            var engine = system.GetGameSystem<PhysicsEngine>();
            return engine?.Config?.Resolve(name);
        }
        catch (Exception)
        {
            // No physics engine registered for this scene — treat the category as unresolvable.
            return null;
        }
    }

    /// <summary>
    /// Applies a collision filter to a ball's collider, keeping both the stored component values and
    /// the live collider in sync (so save/load round-trips the filter). No-op when either category
    /// is unresolvable or the ball has no collider. Virtual so unit tests can observe the request.
    /// </summary>
    protected virtual void ApplyCollisionFilter(Entity ball, CollisionCategory? categories, CollisionCategory? collidesWith)
    {
        if (categories == null || collidesWith == null) return;

        var collider = ball.GetComponent<ColliderComponent>();
        if (collider?.Collider == null) return;

        collider.Categories = categories.Value;
        collider.CollidesWith = collidesWith.Value;
        collider.Collider.Categories = categories.Value;
        collider.Collider.CollidesWith = collidesWith.Value;
    }

    /// <summary>Applies an impulse to a ball's rigidbody. Virtual so unit tests can observe it.</summary>
    protected virtual void ApplyImpulse(Entity ball, Vector2 impulse)
        => ball.GetComponent<RigidbodyComponent>()?.ApplyImpulse(impulse);

    /// <summary>
    /// Configures a VIP ball: sets its id, uniform scale, and sprite color. Virtual so unit tests
    /// can observe the configuration without demo assets.
    /// </summary>
    protected virtual void ConfigureVipBall(Entity ball, string id, Color color, float scale)
    {
        ball.SetId(id);
        ball.Scale = new Vector2(scale, scale);
        var sprite = ball.GetComponent<SpriteComponent>();
        if (sprite != null)
            sprite.Color = color;
    }

    /// <summary>
    /// Creates the world border entity at the given position/size. Virtual so unit tests can observe
    /// the request without a live EntitySystem.
    /// </summary>
    protected virtual void CreateWorldBorderEntity(Vector2 position, Vector2 size)
        => EntitySystem?.CreateEntity<WorldBorder>(position, size);

    // ── Helpers ──────────────────────────────────────────────────────────────────

    private Vector2 RandomPosition() => new(
        (float)_random.NextDouble() * (SpawnAreaMax.X - SpawnAreaMin.X) + SpawnAreaMin.X,
        (float)_random.NextDouble() * (SpawnAreaMax.Y - SpawnAreaMin.Y) + SpawnAreaMin.Y);

    private Vector2 RandomImpulse(float halfRange) => new(
        (float)_random.NextDouble() * 2f * halfRange - halfRange,
        (float)_random.NextDouble() * 2f * halfRange - halfRange);

    private static string[] SplitList(string value)
        => value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
}
