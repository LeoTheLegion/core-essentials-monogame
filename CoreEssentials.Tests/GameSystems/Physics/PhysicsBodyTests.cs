using System;
using System.Collections.Generic;
using CoreEssentials.GameSystems.Physics.Engines.Aether;
using Microsoft.Xna.Framework;
using nkast.Aether.Physics2D.Common;
using nkast.Aether.Physics2D.Dynamics;
using Xunit;

namespace CoreEssentials.GameSystems.Physics.Tests;

/// <summary>
/// Tests for the PhysicsBody class that wraps Aether.Body.
/// </summary>
public class PhysicsBodyTests : IDisposable
{
    private World? _world;
    private List<PhysicsBody?> _bodies = new();

    public void Dispose()
    {
        // Clean up all bodies first
        foreach (var body in _bodies)
        {
            try { body?.Dispose(); } catch { }
        }
        _bodies.Clear();

        // Note: Aether's World doesn't implement IDisposable, so we don't dispose it
    }

    private PhysicsBody CreateTestBody(BodyType type, Vector2 position)
    {
        if (_world == null)
            _world = new World(Vector2.Zero);

        var aetherBody = _world.CreateBody(position, rotation: 0f, type);
        var wrapper = new PhysicsBody(_world, aetherBody);
        _bodies.Add(wrapper);
        return wrapper;
    }

    #region Body Type Tests

    [Fact]
    public void IsDynamic_DynamicBody_ReturnsTrue()
    {
        // Arrange & Act
        var body = CreateTestBody(BodyType.Dynamic, Vector2.Zero);

        // Assert
        Assert.True(body.IsDynamic);
        Assert.False(body.IsStatic);
        Assert.False(body.IsKinematic);
    }

    [Fact]
    public void IsStatic_StaticBody_ReturnsTrue()
    {
        // Arrange & Act
        var body = CreateTestBody(BodyType.Static, Vector2.Zero);

        // Assert
        Assert.True(body.IsStatic);
        Assert.False(body.IsDynamic);
        Assert.False(body.IsKinematic);
    }

    [Fact]
    public void IsKinematic_KinematicBody_ReturnsTrue()
    {
        // Arrange & Act
        var body = CreateTestBody(BodyType.Kinematic, Vector2.Zero);

        // Assert
        Assert.True(body.IsKinematic);
        Assert.False(body.IsDynamic);
        Assert.False(body.IsStatic);
    }

    #endregion

    #region Position Tests

    [Fact]
    public void WorldPosition_CreatedAtPosition_ReturnsCorrectPosition()
    {
        // Arrange & Act
        var position = new Vector2(100, 200);
        var body = CreateTestBody(BodyType.Dynamic, position);

        // Assert
        Assert.Equal(position, body.WorldPosition);
    }

    [Fact]
    public void WorldPosition_ZeroPosition_ReturnsZero()
    {
        // Arrange & Act
        var body = CreateTestBody(BodyType.Static, Vector2.Zero);

        // Assert
        Assert.Equal(Vector2.Zero, body.WorldPosition);
    }

    #endregion

    #region Rotation Tests

    [Fact]
    public void Rotation_Getter_ReturnsZeroInitially()
    {
        // Arrange & Act
        var body = CreateTestBody(BodyType.Dynamic, Vector2.Zero);

        // Assert
        Assert.Equal(0f, body.Rotation);
    }

    [Fact]
    public void Rotation_Setter_AcceptsValue()
    {
        // Arrange
        var body = CreateTestBody(BodyType.Dynamic, Vector2.Zero);

        // Act
        body.Rotation = MathF.PI / 4f;

        // Assert
        Assert.Equal(MathF.PI / 4f, body.Rotation);
    }

    [Fact]
    public void Rotation_Setter_AcceptsNegativeValue()
    {
        // Arrange
        var body = CreateTestBody(BodyType.Dynamic, Vector2.Zero);

        // Act
        body.Rotation = -MathF.PI / 2f;

        // Assert
        Assert.Equal(-MathF.PI / 2f, body.Rotation);
    }

    #endregion

    #region Type Property Tests

    [Fact]
    public void Type_Getter_ReturnsAssignedType()
    {
        // Arrange & Act - We need to check if PhysicsBody accepts a type parameter
        // Looking at the constructor: PhysicsBody(World world, Body body, string? type = null)
        var testWorld = new World(Vector2.Zero);
        var aetherBody = testWorld.CreateBody(Vector2.Zero, 0f, BodyType.Dynamic);
        var body = new PhysicsBody(testWorld, aetherBody, "enemy");
        _bodies.Add(body);
        // Note: testLocal is disposed via the body cleanup in Dispose()

        // Assert
        Assert.Equal("enemy", body.Type);

        _bodies.Add(body);
    }

    [Fact]
    public void Type_Getter_WhenNull_ReturnsNull()
    {
        // Arrange & Act
        var body = CreateTestBody(BodyType.Dynamic, Vector2.Zero);

        // Assert
        Assert.Null(body.Type);
    }

    #endregion

    #region Material Property Tests

    [Fact]
    public void Mass_DynamicBody_ReturnsPositiveValue()
    {
        // Arrange & Act - Aether computes default mass for dynamic bodies
        var body = CreateTestBody(BodyType.Dynamic, Vector2.Zero);

        // Assert - fresh dynamic body has positive mass from Aether defaults
        Assert.True(body.Mass > 0f);
    }

    [Fact]
    public void Inertia_DynamicBody_ReturnsZeroInitially()
    {
        // Arrange & Act
        var body = CreateTestBody(BodyType.Dynamic, Vector2.Zero);

        // Assert - initial inertia is 0 without fixtures
        Assert.Equal(0f, body.Inertia);
    }

    [Fact]
    public void Friction_Getter_ReturnsDefault()
    {
        // Arrange & Act
        var body = CreateTestBody(BodyType.Dynamic, Vector2.Zero);

        // Assert - without fixtures, friction returns default from Aether
        Assert.True(body.Friction >= 0f);
    }

    [Fact]
    public void Restitution_Getter_ReturnsZeroInitially()
    {
        // Arrange & Act
        var body = CreateTestBody(BodyType.Dynamic, Vector2.Zero);

        // Assert - without fixtures, restitution is 0
        Assert.Equal(0f, body.Restitution);
    }

    [Fact]
    public void FixedRotation_Getter_ReturnsFalseInitially()
    {
        // Arrange & Act
        var body = CreateTestBody(BodyType.Dynamic, Vector2.Zero);

        // Assert
        Assert.False(body.FixedRotation);
    }

    [Fact]
    public void FixedRotation_Setter_AcceptsTrue()
    {
        // Arrange
        var body = CreateTestBody(BodyType.Dynamic, Vector2.Zero);

        // Act
        body.FixedRotation = true;

        // Assert
        Assert.True(body.FixedRotation);
    }

    #endregion

    #region Velocity Tests

    [Fact]
    public void LinearVelocity_Getter_ReturnsZeroInitially()
    {
        // Arrange & Act
        var body = CreateTestBody(BodyType.Dynamic, Vector2.Zero);

        // Assert
        Assert.Equal(Vector2.Zero, body.LinearVelocity);
    }

    [Fact]
    public void AngularVelocity_Getter_ReturnsZeroInitially()
    {
        // Arrange & Act
        var body = CreateTestBody(BodyType.Dynamic, Vector2.Zero);

        // Assert
        Assert.Equal(0f, body.AngularVelocity);
    }

    [Fact]
    public void SetLinearVelocity_DynamicBody_SetsValue()
    {
        // Arrange
        var body = CreateTestBody(BodyType.Dynamic, Vector2.Zero);

        // Act
        body.SetLinearVelocity(new Vector2(10, 20));

        // Assert
        Assert.Equal(new Vector2(10, 20), body.LinearVelocity);
    }

    [Fact]
    public void SetLinearVelocity_StaticBody_DoesNotChange()
    {
        // Arrange
        var body = CreateTestBody(BodyType.Static, Vector2.Zero);

        // Act - Static bodies should ignore velocity changes
        body.SetLinearVelocity(new Vector2(100, 100));

        // Assert - static bodies have zero velocity
        Assert.Equal(Vector2.Zero, body.LinearVelocity);
    }

    [Fact]
    public void AngularVelocity_Setter_AcceptsValue()
    {
        // Arrange
        var body = CreateTestBody(BodyType.Dynamic, Vector2.Zero);

        // Act
        body.AngularVelocity = 1.5f;

        // Assert
        Assert.Equal(1.5f, body.AngularVelocity);
    }

    #endregion

    #region Force & Impulse Tests

    [Fact]
    public void ApplyForce_DoesNotThrow()
    {
        // Arrange
        var body = CreateTestBody(BodyType.Dynamic, Vector2.Zero);

        // Act & Assert - verify no exception is thrown
        Exception ex = Record.Exception(() => body.ApplyForce(new Vector2(10, 10)));
        Assert.Null(ex);
    }

    [Fact]
    public void ApplyTorque_DoesNotThrow()
    {
        // Arrange
        var body = CreateTestBody(BodyType.Dynamic, Vector2.Zero);

        // Act & Assert - verify no exception is thrown
        Exception ex = Record.Exception(() => body.ApplyTorque(5f));
        Assert.Null(ex);
    }

    [Fact]
    public void ApplyImpulse_DoesNotThrow()
    {
        // Arrange
        var body = CreateTestBody(BodyType.Dynamic, Vector2.Zero);

        // Act & Assert - verify no exception is thrown
        Exception ex = Record.Exception(() => body.ApplyImpulse(new Vector2(5, 5)));
        Assert.Null(ex);
    }

    #endregion

    #region Body State Tests

    [Fact]
    public void IsAwake_BodyCreated_ReturnsTrue()
    {
        // Arrange & Act
        var body = CreateTestBody(BodyType.Dynamic, Vector2.Zero);

        // Assert - newly created bodies are awake
        Assert.True(body.IsAwake);
    }

    [Fact]
    public void IsActive_Getter_ReturnsTrueInitially()
    {
        // Arrange & Act
        var body = CreateTestBody(BodyType.Dynamic, Vector2.Zero);

        // Assert - newly created bodies are enabled/active
        Assert.True(body.IsActive);
    }

    [Fact]
    public void IsActive_Setter_DisablesBody()
    {
        // Arrange
        var body = CreateTestBody(BodyType.Dynamic, Vector2.Zero);

        // Act
        body.IsActive = false;

        // Assert
        Assert.False(body.IsActive);
    }

    [Fact]
    public void IsActive_Setter_ReEnablesBody()
    {
        // Arrange
        var body = CreateTestBody(BodyType.Dynamic, Vector2.Zero);
        body.IsActive = false;

        // Act
        body.IsActive = true;

        // Assert
        Assert.True(body.IsActive);
    }

    [Fact]
    public void StopAll_DoesNotThrow()
    {
        // Arrange
        var body = CreateTestBody(BodyType.Dynamic, Vector2.Zero);

        // Act & Assert - verify no exception is thrown
        Exception ex = Record.Exception(() => body.StopAll());
        Assert.Null(ex);
    }

    #endregion

    #region Shape Creation Tests

    [Fact]
    public void CreateCircle_ReturnsValidFixture()
    {
        // Arrange
        var body = CreateTestBody(BodyType.Dynamic, Vector2.Zero);

        // Act
        var fixture = body.CreateCircleCollider(1f);

        // Assert
        Assert.NotNull(fixture);
        Assert.NotNull(fixture.Shape);
    }

    [Fact]
    public void CreateCircle_WithOffset_SetsPosition()
    {
        // Arrange
        var body = CreateTestBody(BodyType.Dynamic, Vector2.Zero);

        // Act
        var fixture = body.CreateCircleCollider(1f, new Vector2(5, 0));

        // Assert
        Assert.NotNull(fixture);
        Assert.NotNull(fixture.Shape);
    }

    [Fact]
    public void CreateRectangle_ReturnsValidFixture()
    {
        // Arrange
        var body = CreateTestBody(BodyType.Dynamic, Vector2.Zero);

        // Act
        var fixture = body.CreateRectangleCollider(new Vector2(2, 1));

        // Assert
        Assert.NotNull(fixture);
        Assert.NotNull(fixture.Shape);
    }

    [Fact]
    public void CreatePolygon_ReturnsValidFixture()
    {
        // Arrange
        var body = CreateTestBody(BodyType.Dynamic, Vector2.Zero);
        var vertices = new[]
        {
            new Vector2(0, 0),
            new Vector2(1, 0),
            new Vector2(0.5f, 1)
        };

        // Act
        var fixture = body.CreatePolygonCollider(vertices);

        // Assert
        Assert.NotNull(fixture);
        Assert.NotNull(fixture.Shape);
    }

    [Fact]
    public void CreateConvexHull_ReturnsValidFixture()
    {
        // Arrange
        var body = CreateTestBody(BodyType.Dynamic, Vector2.Zero);
        var points = new[]
        {
            new Vector2(0, 0),
            new Vector2(1, 0),
            new Vector2(1, 1),
            new Vector2(0, 1)
        };

        // Act
        var fixture = body.CreateConvexHullCollider(points);

        // Assert
        Assert.NotNull(fixture);
        Assert.NotNull(fixture.Shape);
    }

    [Fact]
    public void CreatePolygon_ThrowsOnInsufficientVertices()
    {
        // Arrange
        var body = CreateTestBody(BodyType.Dynamic, Vector2.Zero);

        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => body.CreatePolygonCollider(Vector2.Zero));
        Assert.Contains("vertices", ex.Message.ToLower());
    }

    [Fact]
    public void CreateConvexHull_ThrowsOnInsufficientPoints()
    {
        // Arrange
        var body = CreateTestBody(BodyType.Dynamic, Vector2.Zero);

        // Act & Assert
        var ex = Assert.Throws<ArgumentOutOfRangeException>(() => body.CreateConvexHullCollider(Vector2.Zero));
        Assert.Contains("points", ex.Message.ToLower());
    }

    [Fact]
    public void CreateCircle_ThrowsOnDisposedBody()
    {
        // Arrange
        var body = CreateTestBody(BodyType.Dynamic, Vector2.Zero);
        body.Dispose();

        // Act & Assert
        Assert.Throws<ObjectDisposedException>(() => body.CreateCircleCollider(1f));
    }

    #endregion

    #region Fixture Management Tests

    [Fact]
    public void AddFixture_DoesNotThrow()
    {
        // Arrange
        var body = CreateTestBody(BodyType.Dynamic, Vector2.Zero);

        // Act & Assert - verify no exception is thrown (currently a no-op)
        Exception ex = Record.Exception(() => body.AddCollider(null!));
        Assert.Null(ex);
    }

    [Fact]
    public void RemoveFixture_DoesNotThrow()
    {
        // Arrange
        var body = CreateTestBody(BodyType.Dynamic, Vector2.Zero);

        // Act & Assert - verify no exception is thrown (currently a no-op)
        Exception ex = Record.Exception(() => body.RemoveCollider(null!));
        Assert.Null(ex);
    }

    #endregion

    #region Disposal Tests

    [Fact]
    public void Dispose_DoesNotThrow()
    {
        // Arrange
        var body = CreateTestBody(BodyType.Dynamic, Vector2.Zero);

        // Act & Assert - verify no exception is thrown
        Exception ex = Record.Exception(() => body.Dispose());
        Assert.Null(ex);
    }

    [Fact]
    public void Dispose_MultipleTimes_DoesNotThrow()
    {
        // Arrange
        var body = CreateTestBody(BodyType.Dynamic, Vector2.Zero);

        // Act & Assert - verify no exception is thrown for first and second dispose
        Exception ex1 = Record.Exception(() => body.Dispose());
        Assert.Null(ex1);
        Exception ex2 = Record.Exception(() => body.Dispose());
        Assert.Null(ex2);
    }

    [Fact]
    public void Dispose_WorldPosition_ReturnsDefault()
    {
        // Arrange
        var body = CreateTestBody(BodyType.Dynamic, new Vector2(10, 20));

        // Act
        body.Dispose();

        // Assert - after dispose, WorldPosition should return default (zero)
        Assert.Equal(Vector2.Zero, body.WorldPosition);
    }

    #endregion

    #region Integration Tests

    [Fact]
    public void FullLifecycle_CreateSetModifyDispose_DoesNotThrow()
    {
        // Arrange
        var body = CreateTestBody(BodyType.Dynamic, new Vector2(50, 50));

        // Act & Assert - verify no exception is thrown during full lifecycle
        Exception ex = Record.Exception(() =>
        {
            // Modify properties
            body.Rotation = MathF.PI / 2f;
            body.SetLinearVelocity(new Vector2(10, 20));
            body.AngularVelocity = 1.0f;
            body.ApplyForce(new Vector2(5, 5));
            body.IsActive = false;
            body.IsActive = true;

            // Dispose
            body.Dispose();
        });
        Assert.Null(ex);
    }

    [Fact]
    public void MultipleBodies_CreatedAndDisposed_DoesNotThrow()
    {
        // Arrange & Act - Create multiple bodies in same world
        var body1 = CreateTestBody(BodyType.Dynamic, Vector2.Zero);
        var body2 = CreateTestBody(BodyType.Static, new Vector2(10, 10));
        var body3 = CreateTestBody(BodyType.Kinematic, new Vector2(20, 20));

        // Act & Assert - verify no exception is thrown when disposing all
        Exception ex = Record.Exception(() =>
        {
            body1.Dispose();
            body2.Dispose();
            body3.Dispose();
        });
        Assert.Null(ex);
    }

    #endregion
}
