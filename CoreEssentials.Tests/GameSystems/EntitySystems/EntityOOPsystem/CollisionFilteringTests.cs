using System;
using System.Collections;
using Microsoft.Xna.Framework;
using Xunit;
using CoreEssentials.GameSystems;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Components.BuiltIn;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Serialization;
using CoreEssentials.GameSystems.Physics.Engines.Aether;
using CoreEssentials.GameSystems.Physics.Types;
using CoreEssentials.Scenes;

namespace CoreEssentials.Tests.GameSystems.EntitySystems.EntityOOPsystem;

/// <summary>
/// Tests for engine-agnostic collision filtering (Sprint 19).
/// Covers the <see cref="CollisionCategory"/> flags, the <see cref="ICollider.Categories"/> /
/// <see cref="ICollider.CollidesWith"/> adapter, <see cref="ColliderComponent"/> exposure, and
/// XML round-tripping of the filter.
///
/// Note: these tests assert the *filter configuration* (that the adapter forwards categories/mask
/// and that the component + XML serialize it). They deliberately do NOT assert a live physics
/// contact is detected — Aether's broad-phase contact registration is order/state-sensitive in
/// this environment (see the Sprint 19 notes), and the existing CollisionEventsTests already cover
/// live contact detection.
/// </summary>
public class CollisionFilteringTests : IDisposable
{
    private readonly SceneWrapper _scene = null!;
    private readonly EntitySystem _entitySystem = null!;
    private readonly PhysicsEngine _physicsEngine = null!;
    private bool _disposed;

    public CollisionFilteringTests()
    {
        _scene = new SceneWrapper();
        _scene.SetSceneManager(new CoreEssentials.Scenes.SceneManager());

        var gameSystemsDict = typeof(CoreEssentials.Scenes.Scene).GetField("_gameSystems",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
            .GetValue(_scene) as System.Collections.Generic.Dictionary<System.Type, GameSystem>;

        _physicsEngine = new PhysicsEngine(Vector2.Zero);
        _entitySystem = new EntitySystem();

        gameSystemsDict!.Add(typeof(PhysicsEngine), _physicsEngine);
        gameSystemsDict.Add(typeof(EntitySystem), _entitySystem);

        _entitySystem.SetScene(_scene);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed) return;
        if (disposing)
        {
            _physicsEngine?.Dispose();
        }
        _disposed = true;
    }

    private class CollisionTestEntity : Entity
    {
        public override void Update(GameTime gameTime) { }
        public override void Render(Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch) { }
    }

    #region CollisionCategory enum

    [Fact]
    public void CollisionCategory_IsFlagsAndMirrorsBits()
    {
        Assert.True(typeof(CollisionCategory).IsDefined(typeof(FlagsAttribute), inherit: false));
        Assert.Equal(0x1, (int)CollisionCategory.Cat1);
        Assert.Equal(0x2, (int)CollisionCategory.Cat2);
        Assert.Equal(0x40000000, (int)CollisionCategory.Cat31);
        Assert.Equal(int.MaxValue, (int)CollisionCategory.All);

        var combined = CollisionCategory.Cat1 | CollisionCategory.Cat2;
        Assert.Equal(0x3, (int)combined);
        Assert.True(combined.HasFlag(CollisionCategory.Cat1));
        Assert.True(combined.HasFlag(CollisionCategory.Cat2));
        Assert.False(combined.HasFlag(CollisionCategory.Cat3));
    }

    #endregion

    #region ICollider adapter — Categories / CollidesWith

    [Fact]
    public void ICollider_Defaults_AreCat1AndAll()
    {
        var body = _physicsEngine.CreateDynamic(Vector2.Zero);
        var collider = body.CreateCircleCollider(1f);

        Assert.Equal(CollisionCategory.Cat1, collider.Categories);
        Assert.Equal(CollisionCategory.All, collider.CollidesWith);
    }

    [Fact]
    public void ICollider_CanSetAndReadCategoriesAndMask()
    {
        var body = _physicsEngine.CreateDynamic(Vector2.Zero);
        var collider = body.CreateCircleCollider(1f);

        collider.Categories = CollisionCategory.Cat1 | CollisionCategory.Cat2;
        collider.CollidesWith = CollisionCategory.Cat3 | CollisionCategory.Cat5;

        Assert.Equal(CollisionCategory.Cat1 | CollisionCategory.Cat2, collider.Categories);
        Assert.Equal(CollisionCategory.Cat3 | CollisionCategory.Cat5, collider.CollidesWith);
    }

    #endregion

    #region ColliderComponent exposure

    [Fact]
    public void ColliderComponent_Defaults_AreCat1AndAll()
    {
        var component = new ColliderComponent(1f);
        Assert.Equal(CollisionCategory.Cat1, component.Categories);
        Assert.Equal(CollisionCategory.All, component.CollidesWith);
    }

    [Fact]
    public void ColliderComponent_AppliesFilterToUnderlyingCollider()
    {
        var entity = _entitySystem.CreateEntity<CollisionTestEntity>();
        entity.AddComponent(new RigidbodyComponent(RigidbodyType.Dynamic));

        var component = new ColliderComponent(1f);
        component.Categories = CollisionCategory.Cat1 | CollisionCategory.Cat4;
        component.CollidesWith = CollisionCategory.Cat2 | CollisionCategory.Cat3;
        entity.AddComponent(component);

        var collider = component.Collider;
        Assert.NotNull(collider);
        Assert.Equal(CollisionCategory.Cat1 | CollisionCategory.Cat4, collider!.Categories);
        Assert.Equal(CollisionCategory.Cat2 | CollisionCategory.Cat3, collider.CollidesWith);
    }

    #endregion

    #region XML round-trip

    [Fact]
    public void Xml_RoundTrips_CollisionFilter()
    {
        const string xml = """
            <Entity>
              <Position X="0" Y="0" />
              <Components>
                <Component Type="RigidbodyComponent">
                  <Properties>
                    <Property Name="Type" Value="Dynamic" />
                  </Properties>
                </Component>
                <Component Type="ColliderComponent">
                  <Properties>
                    <Property Name="Radius" Value="1" />
                    <Property Name="Categories" Value="Cat1, Cat2" />
                    <Property Name="CollidesWith" Value="Cat3, Cat5" />
                  </Properties>
                </Component>
              </Components>
            </Entity>
            """;

        var entity = EntitySerializer.LoadEntity<CollisionTestEntity>(xml, _entitySystem);

        var component = entity.GetComponent<ColliderComponent>();
        Assert.NotNull(component);
        Assert.Equal(CollisionCategory.Cat1 | CollisionCategory.Cat2, component!.Categories);
        Assert.Equal(CollisionCategory.Cat3 | CollisionCategory.Cat5, component.CollidesWith);

        // The underlying collider should reflect the filter too.
        Assert.Equal(CollisionCategory.Cat1 | CollisionCategory.Cat2, component.Collider!.Categories);
        Assert.Equal(CollisionCategory.Cat3 | CollisionCategory.Cat5, component.Collider.CollidesWith);
    }

    [Fact]
    public void ColliderComponent_SerializeToXml_IncludesFilter()
    {
        var component = new ColliderComponent(1f);
        component.Categories = CollisionCategory.Cat1 | CollisionCategory.Cat2;
        component.CollidesWith = CollisionCategory.Cat4;

        var element = component.SerializeToXml();

        Assert.Equal("Cat1, Cat2", element.Attribute("Categories")?.Value);
        Assert.Equal("Cat4", element.Attribute("CollidesWith")?.Value);

        var restored = new ColliderComponent(1f);
        restored.DeserializeFromXml(element);
        Assert.Equal(CollisionCategory.Cat1 | CollisionCategory.Cat2, restored.Categories);
        Assert.Equal(CollisionCategory.Cat4, restored.CollidesWith);
    }

    #endregion
}
