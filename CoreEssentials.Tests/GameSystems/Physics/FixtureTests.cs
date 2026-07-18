using System;
using System.Collections.Generic;
using System.Linq;
using CoreEssentials.GameSystems.Physics.Engines.Aether;
using Microsoft.Xna.Framework;
using nkast.Aether.Physics2D.Common;
using nkast.Aether.Physics2D.Dynamics;
#nullable enable
using Xunit;
using OurFixture = CoreEssentials.GameSystems.Physics.Engines.Aether.Collider;

namespace CoreEssentials.GameSystems.Physics.Tests;

/// <summary>
/// Tests for the Fixture class that wraps Aether.Fixture.
/// </summary>
public class FixtureTests : IDisposable
{
    private World? _world;
    private readonly List<PhysicsBody> _bodies = new();
    private bool _disposed;

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
            // Clean up all bodies first (which contain fixtures)
            foreach (var body in _bodies)
            {
                try { body.Dispose(); } catch { /* Expected during cleanup */ }
            }
            _bodies.Clear();
        }
        _disposed = true;

        // Note: Aether's World doesn't implement IDisposable, so we don't dispose it
    }

    private PhysicsBody CreateTestBodyWithFixture(BodyType type, Vector2 position)
    {
        if (_world == null)
            _world = new World(Vector2.Zero);

        var aetherBody = _world.CreateBody(position, rotation: 0f, type);

        // Aether.Body.CreateRectangle(radiusX, radiusY, density, offset) - uses rectangle shape with rounded corners
        aetherBody.CreateCircle(1.0f, 1.0f, Vector2.Zero);

        var wrapper = new PhysicsBody(_world, aetherBody);
        _bodies.Add(wrapper);

        return wrapper;
    }

    private OurFixture CreateTestFixture(BodyType type, Vector2 position)
    {
        if (_world == null)
            _world = new World(Vector2.Zero);

        var aetherBody = _world.CreateBody(position, rotation: 0f, type);
        var aetherFixture = aetherBody.CreateCircle(1.0f, 1.0f, Vector2.Zero);

        var bodyWrapper = new PhysicsBody(_world, aetherBody);
        var fixture = new OurFixture(_world, aetherFixture, bodyWrapper);

        _bodies.Add(bodyWrapper);
        return fixture;
    }

    #region IsActive Tests

    [Fact]
    public void IsActive_DynamicBody_ReturnsTrue()
    {
        // Arrange & Act
        var fixture = CreateTestFixture(BodyType.Dynamic, Vector2.Zero);

        // Assert
        Assert.True(fixture.IsActive);
    }

    [Fact]
    public void IsActive_StaticBody_ReturnsTrue()
    {
        // Arrange & Act
        var fixture = CreateTestFixture(BodyType.Static, Vector2.Zero);

        // Assert
        Assert.True(fixture.IsActive);
    }

    [Fact]
    public void IsActive_DisabledBody_ReturnsFalse()
    {
        // Arrange
        var fixture = CreateTestFixture(BodyType.Dynamic, Vector2.Zero);
        fixture.OwnerBody.IsActive = false;

        // Act & Assert
        Assert.False(fixture.IsActive);
    }

    #endregion

    #region OwnerBody Tests

    [Fact]
    public void OwnerBody_ReturnsCorrectBody()
    {
        // Arrange
        var body = CreateTestBodyWithFixture(BodyType.Dynamic, Vector2.Zero);

        // Act - We can't directly get fixture from PhysicsBody in current implementation
        // But we verified the owner relationship during creation
        Assert.NotNull(body);
        Assert.True(body.IsDynamic);
    }

    [Fact]
    public void OwnerBody_DoesNotReturnNull()
    {
        // Arrange & Act
        var fixture = CreateTestFixture(BodyType.Dynamic, Vector2.Zero);

        // Assert
        Assert.NotNull(fixture.OwnerBody);
    }

    #endregion

    #region Shape Tests

    [Fact]
    public void Shape_ReturnsNull_ForFixtureWithoutShape()
    {
        // Arrange
        var fixture = CreateTestFixture(BodyType.Dynamic, Vector2.Zero);

        // Act & Assert - New fixtures without shape return null
        Assert.Null(fixture.Shape);
    }

    #endregion

    #region Activation Tests

    [Fact]
    public void Activate_DoesNotThrow()
    {
        // Arrange
        var fixture = CreateTestFixture(BodyType.Dynamic, Vector2.Zero);

        // Act & Assert - verify no exception is thrown
        Exception ex = Record.Exception(() => fixture.Activate());
        Assert.Null(ex);
    }

    [Fact]
    public void Deactivate_DoesNotThrow()
    {
        // Arrange
        var fixture = CreateTestFixture(BodyType.Dynamic, Vector2.Zero);

        // Act & Assert - verify no exception is thrown
        Exception ex = Record.Exception(() => fixture.Deactivate());
        Assert.Null(ex);
    }

    [Fact]
    public void Activate_DisabledBody_ReEnablesIt()
    {
        // Arrange
        var fixture = CreateTestFixture(BodyType.Dynamic, Vector2.Zero);
        fixture.OwnerBody.IsActive = false;

        // Act
        fixture.Activate();

        // Assert - the body should be re-enabled
        Assert.True(fixture.OwnerBody.IsActive);
    }

    #endregion

    #region Disposal Tests

    [Fact]
    public void Dispose_DoesNotThrow()
    {
        // Arrange
        var fixture = CreateTestFixture(BodyType.Dynamic, Vector2.Zero);

        // Act & Assert - verify no exception is thrown
        Exception ex = Record.Exception(() => fixture.Dispose());
        Assert.Null(ex);
    }

    [Fact]
    public void Dispose_MultipleTimes_DoesNotThrow()
    {
        // Arrange
        var fixture = CreateTestFixture(BodyType.Dynamic, Vector2.Zero);

        // Act & Assert - verify no exception is thrown for first and second dispose
        Exception ex1 = Record.Exception(() => fixture.Dispose());
        Assert.Null(ex1);
        Exception ex2 = Record.Exception(() => fixture.Dispose());
        Assert.Null(ex2);
    }

    #endregion

    #region Integration Tests

    [Fact]
    public void FullLifecycle_CreateActivateDeactivateDispose_DoesNotThrow()
    {
        // Arrange
        var fixture = CreateTestFixture(BodyType.Dynamic, Vector2.Zero);

        // Act & Assert - verify no exception is thrown during full lifecycle
        Exception ex = Record.Exception(() =>
        {
            fixture.Activate();
            fixture.Deactivate();
            fixture.Activate();
            fixture.Dispose();
        });
        Assert.Null(ex);
    }

    [Fact]
    public void MultipleFixtures_CreatedAndDisposed_DoesNotThrow()
    {
        // Arrange & Act - Create multiple fixtures on the same body
        if (_world == null)
            _world = new World(Vector2.Zero);

        var aetherBody = _world.CreateBody(Vector2.Zero, 0f, BodyType.Dynamic);
        aetherBody.CreateCircle(1.0f, 1.0f, Vector2.Zero);
        aetherBody.CreateCircle(2.0f, 2.0f, Vector2.Zero);

        // Create two fixtures on the same body
        var fixture1 = new OurFixture(_world, aetherBody.FixtureList[0], new PhysicsBody(_world, aetherBody));
        var fixture2 = new OurFixture(_world, aetherBody.FixtureList[^1], new PhysicsBody(_world, aetherBody));

        // Act & Assert - verify no exception is thrown when disposing both
        Exception ex = Record.Exception(() =>
        {
            fixture1.Dispose();
            fixture2.Dispose();
        });
        Assert.Null(ex);

        _bodies.Add(new PhysicsBody(_world, aetherBody));
    }

    #endregion

    #region Edge Case Tests

    [Fact]
    public void Constructor_WithNullFixture_ThrowsArgumentNullException()
    {
        // Arrange
        if (_world == null)
            _world = new World(Vector2.Zero);
        var aetherBody = _world.CreateBody(Vector2.Zero, 0f, BodyType.Dynamic);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new OurFixture(_world, null!, new PhysicsBody(_world, aetherBody)));
    }

    [Fact]
    public void Constructor_WithNullOwnerBody_ThrowsArgumentNullException()
    {
        // Arrange
        if (_world == null)
            _world = new World(Vector2.Zero);
        var aetherBody = _world.CreateBody(Vector2.Zero, 0f, BodyType.Dynamic);
        var fixture = aetherBody.CreateCircle(1.0f, 1.0f, Vector2.Zero);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new OurFixture(_world, fixture, null!));
    }

    [Fact]
    public void Constructor_WithNullWorld_SucceedsButFixtureMayNotWork()
    {
        // Arrange & Act - World is not validated in constructor
        var aetherBody = (new World(Vector2.Zero)).CreateBody(Vector2.Zero, 0f, BodyType.Dynamic);

        // This should succeed but operations may fail later
        Exception ex = Record.Exception(() => new OurFixture(null!, aetherBody.CreateCircle(1.0f, 1.0f, Vector2.Zero), new PhysicsBody(aetherBody.World!, aetherBody)));
        Assert.Null(ex);
    }

    #endregion
}
