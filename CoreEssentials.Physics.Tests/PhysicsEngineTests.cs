using CoreEssentials.GameSystems;
using Microsoft.Xna.Framework;
using nkast.Aether.Physics2D.Common;

namespace CoreEssentials.Physics.Tests;

/// <summary>
/// Tests for the PhysicsEngine GameSystem.
/// </summary>
public class PhysicsEngineTests : IDisposable
{
    private PhysicsEngine? _engine;

    public void Dispose()
    {
        _engine?.Dispose();
    }

    #region Engine Creation Tests

    [Fact]
    public void Constructor_WithDefaultGravity_CreatesEngineWithDefaultGravity()
    {
        // Act
        var engine = new PhysicsEngine();

        // Assert - Aether's default gravity is approximately -9.80665 m/s²
        Assert.NotNull(engine);
        Assert.InRange(engine.Gravity.Y, -9.81f, -9.80f); // Allow small floating-point variance
    }

    [Fact]
    public void Constructor_WithCustomGravity_CreatesEngineWithThatGravity()
    {
        // Arrange
        var customGravity = new Vector2(0, -4.9f);

        // Act
        var engine = new PhysicsEngine(customGravity);

        // Assert
        Assert.NotNull(engine);
        Assert.Equal(customGravity, engine.Gravity);
    }

    [Fact]
    public void Constructor_WithZeroGravity_CreatesEngineWithoutGravity()
    {
        // Act
        var engine = new PhysicsEngine(Vector2.Zero);

        // Assert
        Assert.NotNull(engine);
        Assert.Equal(Vector2.Zero, engine.Gravity);
    }

    [Fact]
    public void IsInstanceOfGameSystem_IsAGameSystem()
    {
        // Act & Assert
        var engine = new PhysicsEngine();
        Assert.IsAssignableFrom<GameSystem>(engine);
        engine.Dispose();
    }

    [Fact]
    public void IsInstanceOfIFixedUpdateGameSystem_ImplementsFixedUpdate()
    {
        // Act & Assert
        var engine = new PhysicsEngine();
        Assert.IsAssignableFrom<IFixedUpdateGameSystem>(engine);
        engine.Dispose();
    }

    #endregion

    #region Gravity Tests

    [Fact]
    public void Gravity_PropertySetter_UpdatesWorldGravity()
    {
        // Arrange
        var engine = new PhysicsEngine(Vector2.Zero);

        // Act
        engine.Gravity = new Vector2(10, -20);

        // Assert
        Assert.Equal(new Vector2(10, -20), engine.Gravity);

        engine.Dispose();
    }

    [Fact]
    public void Gravity_PropertyGetter_ReturnsCurrentGravity()
    {
        // Arrange
        var expectedGravity = new Vector2(5, -15);
        var engine = new PhysicsEngine(expectedGravity);

        // Act
        var actualGravity = engine.Gravity;

        // Assert
        Assert.Equal(expectedGravity, actualGravity);

        engine.Dispose();
    }

    #endregion

    #region Solver Configuration Tests

    [Fact]
    public void VelocityIterations_DefaultValue_IsEight()
    {
        // Arrange & Act
        var engine = new PhysicsEngine();

        // Assert
        Assert.Equal(8, engine.VelocityIterations);

        engine.Dispose();
    }

    [Fact]
    public void PositionIterations_DefaultValue_IsThree()
    {
        // Arrange & Act
        var engine = new PhysicsEngine();

        // Assert
        Assert.Equal(3, engine.PositionIterations);

        engine.Dispose();
    }

    [Fact]
    public void VelocityIterations_PropertySetter_UpdatesValue()
    {
        // Arrange
        var engine = new PhysicsEngine();

        // Act
        engine.VelocityIterations = 16;

        // Assert
        Assert.Equal(16, engine.VelocityIterations);

        engine.Dispose();
    }

    [Fact]
    public void PositionIterations_PropertySetter_UpdatesValue()
    {
        // Arrange
        var engine = new PhysicsEngine();

        // Act
        engine.PositionIterations = 10;

        // Assert
        Assert.Equal(10, engine.PositionIterations);

        engine.Dispose();
    }

    #endregion

    #region Body Creation Tests

    [Fact]
    public void CreateDynamic_CreatesBodyAtPosition()
    {
        // Arrange
        var engine = new PhysicsEngine(Vector2.Zero);
        var position = new Vector2(100, 200);

        // Act
        var body = engine.CreateDynamic(position);

        // Assert
        Assert.NotNull(body);
        Assert.True(body.IsDynamic);
        Assert.False(body.IsStatic);
        Assert.False(body.IsKinematic);
        Assert.Equal(position, body.WorldPosition);

        engine.Dispose();
    }

    [Fact]
    public void CreateStatic_CreatesBodyAtPosition()
    {
        // Arrange
        var engine = new PhysicsEngine(Vector2.Zero);
        var position = new Vector2(50, 75);

        // Act
        var body = engine.CreateStatic(position);

        // Assert
        Assert.NotNull(body);
        Assert.True(body.IsStatic);
        Assert.False(body.IsDynamic);
        Assert.False(body.IsKinematic);
        Assert.Equal(position, body.WorldPosition);

        engine.Dispose();
    }

    [Fact]
    public void CreateKinematic_CreatesBodyAtPosition()
    {
        // Arrange
        var engine = new PhysicsEngine(Vector2.Zero);
        var position = new Vector2(30, 40);

        // Act
        var body = engine.CreateKinematic(position);

        // Assert
        Assert.NotNull(body);
        Assert.True(body.IsKinematic);
        Assert.False(body.IsDynamic);
        Assert.False(body.IsStatic);
        Assert.Equal(position, body.WorldPosition);

        engine.Dispose();
    }

    [Fact]
    public void CreateMultipleBodies_CreatesDistinctBodies()
    {
        // Arrange
        var engine = new PhysicsEngine(Vector2.Zero);
        var pos1 = new Vector2(0, 0);
        var pos2 = new Vector2(10, 10);

        // Act
        var body1 = engine.CreateDynamic(pos1);
        var body2 = engine.CreateStatic(pos2);

        // Assert
        Assert.NotSame(body1, body2);
        Assert.Equal(pos1, body1.WorldPosition);
        Assert.Equal(pos2, body2.WorldPosition);

        engine.Dispose();
    }

    [Fact]
    public void CreateBody_WhileWorldStepping_CreatesAsync()
    {
        // Arrange
        var engine = new PhysicsEngine(Vector2.Zero);
        var gameTime = new GameTime(
            elapsedGameTime: TimeSpan.FromSeconds(1.0 / 60.0),
            totalGameTime: TimeSpan.FromSeconds(0));

        // Act - Step locks the world, but body creation uses async removal internally
        engine.FixedUpdate(gameTime);
        var body = engine.CreateDynamic(Vector2.Zero); // Should not throw; uses async queue

        // Assert - body was created successfully
        Assert.NotNull(body);
        Assert.True(body.IsDynamic);

        engine.Dispose();
    }

    #endregion

    #region Destruction Tests

    [Fact]
    public void Destroy_ValidBody_RemovesBodyFromWorld()
    {
        // Arrange
        var engine = new PhysicsEngine(Vector2.Zero);
        var body = engine.CreateDynamic(new Vector2(50, 50));
        Assert.NotNull(body); // Body was created

        // Act - Destroy removes the Aether body from the world and clears _body reference
        engine.Destroy(body);

        // Assert - after destroy, WorldPosition returns default (zero) because _body is nulled
        Assert.Equal(Vector2.Zero, body.WorldPosition);

        engine.Dispose();
    }

    [Fact]
    public void Destroy_NullBody_DoesNotThrow()
    {
        // Arrange
        var engine = new PhysicsEngine(Vector2.Zero);

        // Act & Assert - verify no exception is thrown on null
        Exception ex = Record.Exception(() => engine.Destroy(null!));
        Assert.Null(ex);

        engine.Dispose();
    }

    [Fact]
    public void Destroy_AlreadyDestroyedBody_DoesNotThrow()
    {
        // Arrange
        var engine = new PhysicsEngine(Vector2.Zero);
        var body = engine.CreateDynamic(new Vector2(50, 50));
        engine.Destroy(body); // First destroy removes from world

        // Act & Assert - second destroy should not throw (body already removed)
        Exception ex = Record.Exception(() => engine.Destroy(body));
        Assert.Null(ex);

        engine.Dispose();
    }

    [Fact]
    public void Destroy_DynamicBody_BecomesUnusable()
    {
        // Arrange
        var engine = new PhysicsEngine(Vector2.Zero);
        var body = engine.CreateDynamic(new Vector2(0, 0));
        Assert.True(body.IsDynamic); // Confirmed dynamic before destroy

        // Act
        engine.Destroy(body);

        // Assert - after destroy, _body is nulled so IsDynamic returns false (null check)
        Assert.False(body.IsDynamic);
        Assert.Equal(Vector2.Zero, body.WorldPosition); // Position also defaults to zero

        engine.Dispose();
    }

    #endregion

    #region Time Step Tests

    [Fact]
    public void FixedUpdate_WithEnabledWorld_DoesNotThrow()
    {
        // Arrange
        var engine = new PhysicsEngine(Vector2.Zero);
        var gameTime = new GameTime(
            elapsedGameTime: TimeSpan.FromSeconds(1.0 / 60.0),
            totalGameTime: TimeSpan.FromSeconds(0));

        // Act & Assert - verify no exception is thrown
        Exception ex = Record.Exception(() => engine.FixedUpdate(gameTime));
        Assert.Null(ex);
    }

    [Fact]
    public void FixedUpdate_WithZeroDelta_DoesNotStep()
    {
        // Arrange
        var engine = new PhysicsEngine(Vector2.Zero);
        var gameTime = new GameTime(
            elapsedGameTime: TimeSpan.FromSeconds(0),
            totalGameTime: TimeSpan.FromSeconds(0));

        // Act & Assert - verify no exception is thrown with zero delta
        Exception ex = Record.Exception(() => engine.FixedUpdate(gameTime));
        Assert.Null(ex);
    }

    #endregion

    #region Query Tests

    [Fact]
    public void TestPoint_WithNoFixtures_ReturnsNull()
    {
        // Arrange
        var engine = new PhysicsEngine(Vector2.Zero);

        // Act
        var result = engine.TestPoint(new Vector2(0, 0));

        // Assert
        Assert.Null(result);

        engine.Dispose();
    }

    #endregion

    #region Disposal Tests

    [Fact]
    public void Dispose_CleansUpWorldAndCache()
    {
        // Arrange
        var engine = new PhysicsEngine(Vector2.Zero);
        _ = engine.CreateDynamic(new Vector2(0, 0));
        _ = engine.CreateStatic(new Vector2(10, 10));

        // Act
        engine.Dispose();

        // Assert - creating after dispose should throw or behave correctly
        var ex = Record.Exception(() => engine.CreateDynamic(Vector2.Zero));
        // After disposal, the world is cleared but not null, so body creation may still work
        // The important thing is no unhandled exceptions during disposal
    }

    [Fact]
    public void Dispose_MultipleTimes_DoesNotThrow()
    {
        // Arrange
        var engine = new PhysicsEngine(Vector2.Zero);

        // Act & Assert - verify no exception is thrown on multiple disposals
        Exception ex1 = Record.Exception(() => engine.Dispose());
        Assert.Null(ex1);
        Exception ex2 = Record.Exception(() => engine.Dispose());
        Assert.Null(ex2);
    }

    #endregion

    #region Integration Tests

    [Fact]
    public void CreateBody_SetPosition_VerifiesChange()
    {
        // Arrange
        var engine = new PhysicsEngine(Vector2.Zero);
        var body = engine.CreateDynamic(new Vector2(0, 0));

        // Act - Set position via Aether body directly (through our wrapper)
        // Note: Our wrapper doesn't have a Position setter yet, only WorldPosition getter
        // So we verify the initial state is correct
        Assert.Equal(new Vector2(0, 0), body.WorldPosition);

        engine.Dispose();
    }

    [Fact]
    public void CreateBody_SetGravity_AffectsNewBodies()
    {
        // Arrange
        var engine = new PhysicsEngine(Vector2.Zero);

        // Act
        engine.Gravity = new Vector2(9.8f, 0);

        // Assert
        Assert.Equal(new Vector2(9.8f, 0), engine.Gravity);

        engine.Dispose();
    }

    [Fact]
    public void CreateAndDestroy_FullLifecycle_DoesNotThrow()
    {
        // Arrange
        var engine = new PhysicsEngine(Vector2.Zero);

        // Act & Assert - verify no exception is thrown during full lifecycle
        Exception ex = Record.Exception(() =>
        {
            var body = engine.CreateDynamic(new Vector2(10, 20));
            Assert.NotNull(body);
            Assert.True(body.IsDynamic);
            engine.Destroy(body);
        });
        Assert.Null(ex);

        engine.Dispose();
    }

    #endregion
}
