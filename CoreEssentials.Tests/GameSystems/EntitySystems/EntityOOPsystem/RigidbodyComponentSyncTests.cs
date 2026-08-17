using System;
using Microsoft.Xna.Framework;
using Xunit;
using CoreEssentials.GameSystems;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Components.BuiltIn;
using CoreEssentials.GameSystems.Physics.Engines.Aether;
using CoreEssentials.Scenes;

namespace CoreEssentials.Tests.GameSystems.EntitySystems.EntityOOPsystem;

/// <summary>
/// Tests the transform-sync behavior of <see cref="RigidbodyComponent"/>, in particular the
/// divergence detection that lets a physics-driven (SyncFromPhysics) entity adopt an externally
/// set transform (save/load, teleport) as the new physics source of truth.
/// </summary>
public class RigidbodyComponentSyncTests : IDisposable
{
    private readonly SceneWrapper _scene = null!;
    private readonly EntitySystem _entitySystem = null!;
    private readonly PhysicsEngine _physicsEngine = null!;
    private bool _disposed;

    public RigidbodyComponentSyncTests()
    {
        _scene = new SceneWrapper();
        _scene.SetSceneManager(new SceneManager());

        // Add game systems to Scene's _gameSystems dictionary via reflection
        var gameSystemsDict = typeof(CoreEssentials.Scenes.Scene).GetField("_gameSystems",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
            .GetValue(_scene) as System.Collections.Generic.Dictionary<System.Type, GameSystem>;

        // Zero gravity so bodies don't drift between steps.
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

    private class TestEntity : Entity
    {
        public override void Update(GameTime gameTime) { }
        public override void Render(Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch) { }
    }

    private (TestEntity entity, RigidbodyComponent component) CreateDynamicEntity(Vector2 position)
    {
        var entity = _entitySystem.CreateEntity<TestEntity>();
        entity.Position = position;
        var component = new RigidbodyComponent(RigidbodyType.Dynamic);
        entity.AddComponent(component);
        component.CreateBody();
        return (entity, component);
    }

    [Fact]
    public void Update_SyncFromPhysics_CopiesBodyTransformToEntity()
    {
        // Arrange
        var (entity, component) = CreateDynamicEntity(new Vector2(0, 0));
        Assert.True(component.SyncFromPhysics);

        // Act - move the physics body, then let the component sync.
        component.Body!.Position = new Vector2(5, 7);
        component.Body.Rotation = 0.5f;
        component.Update(new GameTime());

        // Assert - entity follows the body.
        Assert.Equal(new Vector2(5, 7), entity.Position);
        Assert.Equal(0.5f, entity.Rotation, 3);
    }

    [Fact]
    public void Update_ExternalEntityMove_IsAdoptedByPhysicsBody()
    {
        // Arrange - let the component sync once so the snapshot matches.
        var (entity, component) = CreateDynamicEntity(new Vector2(0, 0));
        component.Update(new GameTime());

        // Act - external code (e.g. save/load) moves the entity transform.
        entity.Position = new Vector2(50, 60);
        entity.Rotation = 1.2f;
        component.Update(new GameTime());

        // Assert - physics adopts the entity's new transform as the source of truth.
        Assert.Equal(new Vector2(50, 60), component.Body!.Position);
        Assert.Equal(1.2f, component.Body.Rotation, 3);
    }

    [Fact]
    public void Update_NoExternalMove_DoesNotSnapBodyBackToEntity()
    {
        // Arrange - sync once so snapshot matches.
        var (entity, component) = CreateDynamicEntity(new Vector2(0, 0));
        component.Update(new GameTime());

        // Act - the body moves (as physics would), entity is NOT touched externally.
        component.Body!.Position = new Vector2(3, 3);
        component.Update(new GameTime());

        // Assert - the body keeps its own (physics) position; it was not snapped back to the
        // stale entity transform. The entity instead follows the body.
        Assert.Equal(new Vector2(3, 3), component.Body.Position);
        Assert.Equal(new Vector2(3, 3), entity.Position);
    }

    [Fact]
    public void Update_ExternalMove_MatchesSaveLoadRoundTrip()
    {
        // Arrange - simulate a ball that has been moving, with the body at a live position.
        var (entity, component) = CreateDynamicEntity(new Vector2(10, 10));
        component.Body!.Position = new Vector2(99, 88); // live physics position
        component.Body.SetLinearVelocity(new Vector2(1, 0));
        component.Update(new GameTime()); // entity now tracks the live body position

        // Act - LoadState restores the saved entity transform (position is source of truth).
        entity.Position = new Vector2(10, 10);
        component.Update(new GameTime());

        // Assert - the body is re-anchored to the saved position so the next physics step
        // integrates from there, instead of the stale live position snapping the entity back.
        Assert.Equal(new Vector2(10, 10), component.Body.Position);
        Assert.Equal(new Vector2(10, 10), entity.Position);
    }

    [Fact]
    public void Update_EntityDriven_SyncsEntityToBody()
    {
        // Arrange
        var entity = _entitySystem.CreateEntity<TestEntity>();
        entity.Position = new Vector2(0, 0);
        var component = new RigidbodyComponent(RigidbodyType.Dynamic) { SyncFromPhysics = false };
        entity.AddComponent(component);
        component.CreateBody();

        // Act
        entity.Position = new Vector2(12, -4);
        entity.Rotation = 0.25f;
        component.Update(new GameTime());

        // Assert
        Assert.Equal(new Vector2(12, -4), component.Body!.Position);
        Assert.Equal(0.25f, component.Body.Rotation, 3);
    }
}
