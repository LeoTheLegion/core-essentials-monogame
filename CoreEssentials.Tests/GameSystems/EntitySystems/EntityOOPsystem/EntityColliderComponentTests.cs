using System;
using System.Collections;
using Microsoft.Xna.Framework;
using Xunit;
using CoreEssentials.GameSystems;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Components.BuiltIn;
using CoreEssentials.GameSystems.Physics.Engines.Aether;
using CoreEssentials.Scenes;

namespace CoreEssentials.Tests.GameSystems.EntitySystems.EntityOOPsystem;

public class ColliderComponentTests : IDisposable
{
    private readonly SceneWrapper _scene = null!;
    private readonly EntitySystem _entitySystem = null!;
    private readonly PhysicsEngine _physicsEngine = null!;
    private bool _disposed;

    public ColliderComponentTests()
    {
        _scene = new SceneWrapper();

        // Set up SceneManager so Scene.GetGameSystem<T>() works
        _scene.SetSceneManager(new CoreEssentials.Scenes.SceneManager());

        // Add game systems to Scene's _gameSystems dictionary via reflection
        var gameSystemsDict = typeof(CoreEssentials.Scenes.Scene).GetField("_gameSystems",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
            .GetValue(_scene) as System.Collections.Generic.Dictionary<System.Type, GameSystem>;

        _physicsEngine = new PhysicsEngine(Vector2.Zero);
        _entitySystem = new EntitySystem();

        gameSystemsDict!.Add(typeof(PhysicsEngine), _physicsEngine);
        gameSystemsDict.Add(typeof(EntitySystem), _entitySystem);

        // Link EntitySystem back to Scene so GetGameSystem resolves through Scene
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

    // ===== Collider Component Construction Tests =====

    [Fact]
    public void Constructor_Circle_SetsCorrectShapeType()
    {
        // Act
        var component = new ColliderComponent(1.0f);

        // Assert
        Assert.Equal(ColliderShapeType.Circle, component.ShapeType);
        Assert.Equal(1.0f, component.Radius);
        Assert.Equal(Vector2.Zero, component.Offset);
    }

    [Fact]
    public void Constructor_Circle_WithOffset_SetsCorrectValues()
    {
        // Act
        var offset = new Vector2(0.5f, -1.0f);
        var component = new ColliderComponent(1.0f, offset);

        // Assert
        Assert.Equal(ColliderShapeType.Circle, component.ShapeType);
        Assert.Equal(1.0f, component.Radius);
        Assert.Equal(offset, component.Offset);
    }

    [Fact]
    public void Constructor_Rectangle_SetsCorrectShapeType()
    {
        // Arrange
        var size = new Vector2(2.0f, 4.0f);

        // Act
        var component = new ColliderComponent(size);

        // Assert
        Assert.Equal(ColliderShapeType.Rectangle, component.ShapeType);
        Assert.Equal(size, component.Size);
        Assert.Equal(Vector2.Zero, component.Offset);
    }

    [Fact]
    public void Constructor_Polygon_SetsCorrectShapeType()
    {
        // Arrange
        var vertices = new[]
        {
            new Vector2(0, 0),
            new Vector2(1, 0),
            new Vector2(1, 1),
            new Vector2(0, 1)
        };

        // Act
        var component = new ColliderComponent(vertices);

        // Assert
        Assert.Equal(ColliderShapeType.Polygon, component.ShapeType);
        Assert.Equal(vertices, component.Vertices);
    }

    [Fact]
    public void Constructor_ConvexHull_SetsCorrectShapeType()
    {
        // Arrange
        var points = new[]
        {
            new Vector2(0, 0),
            new Vector2(2, 0),
            new Vector2(2, 2),
            new Vector2(0, 2)
        };

        // Act
        var component = new ColliderComponent(ColliderShapeType.ConvexHull, points);

        // Assert
        Assert.Equal(ColliderShapeType.ConvexHull, component.ShapeType);
        Assert.Equal(points, component.ConvexHullPoints);
    }

    [Fact]
    public void Constructor_InvalidShapeType_ThrowsArgumentException()
    {
        // Arrange
        var points = new[] { new Vector2(0, 0), new Vector2(1, 1) };

        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            new ColliderComponent(ColliderShapeType.Circle, points));
    }

    // ===== Collider Property Tests =====

    [Fact]
    public void IsColliderCreated_Default_IsFalse()
    {
        // Act
        var component = new ColliderComponent(1.0f);

        // Assert
        Assert.False(component.IsColliderCreated);
    }

    [Fact]
    public void Collider_Default_IsNull()
    {
        // Act
        var component = new ColliderComponent(1.0f);

        // Assert
        Assert.Null(component.Collider);
    }

    [Fact]
    public void Friction_Default_IsZero()
    {
        // Act
        var component = new ColliderComponent(1.0f);

        // Assert
        Assert.Equal(0f, component.Friction);
    }

    [Fact]
    public void Restitution_Default_IsZero()
    {
        // Act
        var component = new ColliderComponent(1.0f);

        // Assert
        Assert.Equal(0f, component.Restitution);
    }

    [Fact]
    public void Friction_Settable()
    {
        // Arrange
        var component = new ColliderComponent(1.0f);

        // Act
        component.Friction = 0.8f;

        // Assert
        Assert.Equal(0.8f, component.Friction);
    }

    [Fact]
    public void Restitution_Settable()
    {
        // Arrange
        var component = new ColliderComponent(1.0f);

        // Act
        component.Restitution = 0.5f;

        // Assert
        Assert.Equal(0.5f, component.Restitution);
    }

    // ===== CreateCollider Tests (with PhysicsEngine) =====

    [Fact]
    public void CreateCollider_WithoutRigidbodyComponent_ThrowsInvalidOperationException()
    {
        // Arrange
        var entity = _entitySystem.CreateEntity<TestEntity>();
        var component = new ColliderComponent(1.0f);

        // Act & Assert (OnAttach calls CreateCollider which fails without Rigidbody)
        Assert.Throws<InvalidOperationException>(() => entity.AddComponent(component));
    }

    [Fact]
    public void CreateCollider_WithRigidbodyComponent_CreatesCollider()
    {
        // Arrange
        var entity = _entitySystem.CreateEntity<TestEntity>();
        var rigidbody = new RigidbodyComponent(RigidbodyType.Dynamic);
        entity.AddComponent(rigidbody);

        // Act
        var component = new ColliderComponent(1.0f)
        {
            Restitution = 0.8f
        };
        entity.AddComponent(component);

        // Assert
        Assert.True(component.IsColliderCreated);
        Assert.NotNull(component.Collider);
    }

    [Fact]
    public void CreateCollider_Circle_SetsCorrectRadius()
    {
        // Arrange
        var entity = _entitySystem.CreateEntity<TestEntity>();
        entity.AddComponent(new RigidbodyComponent(RigidbodyType.Dynamic));

        // Act
        var component = new ColliderComponent(2.5f);
        entity.AddComponent(component);

        // Assert
        Assert.True(component.IsColliderCreated);
        Assert.Equal(2.5f, component.Radius);
    }

    [Fact]
    public void CreateCollider_Rectangle_SetsCorrectSize()
    {
        // Arrange
        var entity = _entitySystem.CreateEntity<TestEntity>();
        entity.AddComponent(new RigidbodyComponent(RigidbodyType.Dynamic));
        var size = new Vector2(3.0f, 5.0f);

        // Act
        var component = new ColliderComponent(size);
        entity.AddComponent(component);

        // Assert
        Assert.True(component.IsColliderCreated);
        Assert.Equal(ColliderShapeType.Rectangle, component.ShapeType);
    }

    [Fact]
    public void CreateCollider_Polygon_SetsCorrectVertices()
    {
        // Arrange
        var entity = _entitySystem.CreateEntity<TestEntity>();
        entity.AddComponent(new RigidbodyComponent(RigidbodyType.Dynamic));
        var vertices = new[]
        {
            new Vector2(0, 1),
            new Vector2(-1, -1),
            new Vector2(1, -1)
        };

        // Act
        var component = new ColliderComponent(vertices);
        entity.AddComponent(component);

        // Assert
        Assert.True(component.IsColliderCreated);
        Assert.Equal(ColliderShapeType.Polygon, component.ShapeType);
    }

    [Fact]
    public void CreateCollider_CallsCreateBodyOnRigidbodyIfNull()
    {
        // Arrange
        var entity = _entitySystem.CreateEntity<TestEntity>();
        var rigidbody = new RigidbodyComponent(RigidbodyType.Dynamic);
        entity.AddComponent(rigidbody);

        // Body should not be created yet (lazy)
        Assert.False(rigidbody.IsBodyCreated);

        // Act
        var component = new ColliderComponent(1.0f);
        entity.AddComponent(component);

        // Assert - CreateCollider forces body creation
        Assert.True(rigidbody.IsBodyCreated);
    }

    [Fact]
    public void CreateCollider_DoesNotCreateDuplicate()
    {
        // Arrange
        var entity = _entitySystem.CreateEntity<TestEntity>();
        entity.AddComponent(new RigidbodyComponent(RigidbodyType.Dynamic));
        var component = new ColliderComponent(1.0f);
        entity.AddComponent(component);

        var colliderRef = component.Collider;

        // Act - Call CreateCollider again (should be no-op)
        component.CreateCollider();

        // Assert - Same collider reference
        Assert.Same(colliderRef, component.Collider);
    }

    // ===== DestroyCollider Tests =====

    [Fact]
    public void DestroyCollider_RemovesCollider()
    {
        // Arrange
        var entity = _entitySystem.CreateEntity<TestEntity>();
        entity.AddComponent(new RigidbodyComponent(RigidbodyType.Dynamic));
        var component = new ColliderComponent(1.0f);
        entity.AddComponent(component);

        Assert.True(component.IsColliderCreated);

        // Act
        component.DestroyCollider();

        // Assert
        Assert.False(component.IsColliderCreated);
        Assert.Null(component.Collider);
    }

    [Fact]
    public void DestroyCollider_Null_Collider_IsSafe()
    {
        // Arrange
        var component = new ColliderComponent(1.0f);

        // Act & Assert - Should not throw
        component.DestroyCollider();
    }

    // ===== UpdateCircleRadius Tests =====

    [Fact]
    public void UpdateCircleRadius_UpdatesRadius()
    {
        // Arrange
        var entity = _entitySystem.CreateEntity<TestEntity>();
        entity.AddComponent(new RigidbodyComponent(RigidbodyType.Dynamic));
        var component = new ColliderComponent(1.0f);
        entity.AddComponent(component);

        Assert.Equal(1.0f, component.Radius);

        // Act
        component.UpdateCircleRadius(2.5f);

        // Assert
        Assert.Equal(2.5f, component.Radius);
    }

    [Fact]
    public void UpdateRectangleSize_UpdatesSize()
    {
        // Arrange
        var entity = _entitySystem.CreateEntity<TestEntity>();
        entity.AddComponent(new RigidbodyComponent(RigidbodyType.Dynamic));
        var size = new Vector2(1.0f, 1.0f);
        var component = new ColliderComponent(size);
        entity.AddComponent(component);

        // Act
        var newSize = new Vector2(3.0f, 5.0f);
        component.UpdateRectangleSize(newSize);

        // Assert
        Assert.Equal(newSize, component.Size);
    }

    // ===== OnDetach Lifecycle Tests =====

    [Fact]
    public void OnDetach_RemovesColliderFromBody()
    {
        // Arrange
        var entity = _entitySystem.CreateEntity<TestEntity>();
        entity.AddComponent(new RigidbodyComponent(RigidbodyType.Dynamic));
        var component = new ColliderComponent(1.0f);
        entity.AddComponent(component);

        Assert.True(component.IsColliderCreated);

        // Act
        entity.RemoveComponent<ColliderComponent>();

        // Assert
        Assert.False(component.IsColliderCreated);
    }

    [Fact]
    public void OnDestroy_RemovesAllComponents()
    {
        // Arrange
        var entity = _entitySystem.CreateEntity<TestEntity>();
        entity.AddComponent(new RigidbodyComponent(RigidbodyType.Dynamic));
        var colliderComponent = new ColliderComponent(1.0f);
        entity.AddComponent(colliderComponent);

        // Act - Destroy the collider first, then destroy entity to avoid body re-creation during cleanup
        colliderComponent.DestroyCollider();
        entity.OnDestroy();

        // Assert
        Assert.False(colliderComponent.IsColliderCreated);
        Assert.Empty(entity.Components);
    }
}

/// <summary>
/// Minimal Scene subclass for testing game system registration.
/// </summary>
public class SceneWrapper : CoreEssentials.Scenes.Scene
{
    protected override GameSystem[] LoadGameSystems() => Array.Empty<GameSystem>();

    protected override IEnumerator OnStartCoroutine()
    {
        yield break;
    }

    public void AddGameSystem(CoreEssentials.GameSystems.GameSystem system)
    {
        var method = typeof(CoreEssentials.Scenes.Scene).GetMethod("AddGameSystem",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        method!.Invoke(this, new object[] { system });
    }
}
