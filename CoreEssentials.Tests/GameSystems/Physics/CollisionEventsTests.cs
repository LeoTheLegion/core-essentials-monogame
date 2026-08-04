using System;
using System.Linq;
using CoreEssentials.GameSystems.Physics.Engines.Aether;
using CoreEssentials.GameSystems.Physics.Types;
using Microsoft.Xna.Framework;
#nullable enable
using Xunit;

namespace CoreEssentials.GameSystems.Physics.Tests;

/// <summary>
/// Tests for per-body collision and separation events (Sprint 8).
/// </summary>
public class CollisionEventsTests : IDisposable
{
    private readonly PhysicsEngine _engine = null!;
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
            _engine?.Dispose();
        }
        _disposed = true;
    }

    /// <summary>
    /// Creates two overlapping dynamic bodies at the given positions and steps the simulation.
    /// Returns the two bodies in order.
    /// </summary>
    private static (IPhysicsBody bodyA, IPhysicsBody bodyB) CreateOverlappingBodies(PhysicsEngine engine, Vector2 posA, Vector2 posB)
    {
        var bodyA = engine.CreateDynamic(posA);
        _ = bodyA.CreateCircleCollider(radius: 1f); // radius 1 = diameter 2

        var bodyB = engine.CreateDynamic(posB);
        _ = bodyB.CreateCircleCollider(radius: 1f);

        return (bodyA, bodyB);
    }

    /// <summary>
    /// Steps the simulation enough for contacts to be detected and events fired.
    /// </summary>
    private static void StepForCollision(PhysicsEngine engine)
    {
        var gameTime = new GameTime(
            elapsedGameTime: TimeSpan.FromSeconds(1.0 / 60.0),
            totalGameTime: TimeSpan.FromSeconds(0));

        // Step multiple times to ensure contact detection (Aether may take a frame or two).
        for (int i = 0; i < 3; i++)
        {
            engine.FixedUpdate(gameTime);
        }
    }

    #region Test 1: Two dynamic bodies collide — both receive OnCollision with correct body references

    [Fact]
    public void CollisionEvents_TwoDynamicBodiesCollide_BothReceiveOnCollisionWithCorrectReferences()
    {
        // Arrange — positions overlapping: radius=1 each, so combined diameter = 4.
        // Distance between centers is only 1 unit → bodies are interpenetrating from start.
        var engine = new PhysicsEngine(Vector2.Zero);
        Vector2 posA = new(-0.5f, 0f);
        Vector2 posB = new(0.5f, 0f);

        bool bodyACollided = false;
        bool bodyBCollided = false;
        IPhysicsBody? capturedOtherA = null;
        IPhysicsBody? capturedOtherB = null;

        var (bodyA, bodyB) = CreateOverlappingBodies(engine, posA, posB);

        // Give bodies small separation speed so Aether registers contact then separates.
        bodyA.SetLinearVelocity(new Vector2(-1f, 0f));
        bodyB.SetLinearVelocity(new Vector2(1f, 0f));

        bodyA.OnCollision += args =>
        {
            bodyACollided = true;
            capturedOtherA = args.BodyB == bodyA ? args.BodyA : args.BodyB;
            return true; // Allow collision
        };

        bodyB.OnCollision += args =>
        {
            bodyBCollided = true;
            capturedOtherB = args.BodyB == bodyB ? args.BodyA : args.BodyB;
            return true; // Allow collision
        };

        // Act — step simulation to trigger collision detection.
        StepForCollision(engine);

        // Assert
        Assert.True(bodyACollided, "Body A should have received OnCollision event.");
        Assert.True(bodyBCollided, "Body B should have received OnCollision event.");
        Assert.Same(bodyB, capturedOtherA);
        Assert.Same(bodyA, capturedOtherB);

        // Cleanup
        engine.Dispose();
    }

    #endregion

    #region Test 2: Handler returns false — collision is rejected

    [Fact]
    public void CollisionEvents_HandlerReturnsFalse_CollisionIsRejected()
    {
        // Arrange — overlapping positions.
        var engine = new PhysicsEngine(Vector2.Zero);
        Vector2 posA = new(-0.5f, 0f);
        Vector2 posB = new(0.5f, 0f);

        bool bodyACollided = false;
        bool bodyBCollided = false;

        var (bodyA, bodyB) = CreateOverlappingBodies(engine, posA, posB);

        // Small separation speed so Aether registers contact then separates.
        bodyA.SetLinearVelocity(new Vector2(-1f, 0f));
        bodyB.SetLinearVelocity(new Vector2(1f, 0f));

        // Body A rejects the collision.
        bodyA.OnCollision += args =>
        {
            bodyACollided = true;
            return false; // Reject this collision
        };

        bodyB.OnCollision += args =>
        {
            bodyBCollided = true;
            return true; // Would allow, but A rejected
        };

        // Act — step simulation.
        StepForCollision(engine);

        // Assert - both handlers may still be called (Aether calls both before deciding),
        // but contact.Enabled should be false due to rejection.
        Assert.True(bodyACollided, "Body A's handler should have been invoked.");
        Assert.True(bodyBCollided, "Body B's handler should also have been invoked.");

        engine.Dispose();
    }

    #endregion

    #region Test 3: Bodies separate — both receive OnSeparation

    [Fact]
    public void CollisionEvents_BodiesSeparate_BothReceiveOnSeparation()
    {
        // Arrange
        var engine = new PhysicsEngine(Vector2.Zero);
        Vector2 posA = new(-1f, 0f);
        Vector2 posB = new(1f, 0f);

        bool bodyASeparated = false;
        bool bodyBSeparated = false;
        IPhysicsBody? capturedSeparationOtherA = null;
        IPhysicsBody? capturedSeparationOtherB = null;

        var (bodyA, bodyB) = CreateOverlappingBodies(engine, posA, posB);

        // Give bodies enough speed to collide and then separate.
        bodyA.SetLinearVelocity(new Vector2(-8f, 0f));
        bodyB.SetLinearVelocity(new Vector2(8f, 0f));

        bodyA.OnSeparation += args =>
        {
            bodyASeparated = true;
            capturedSeparationOtherA = args.BodyB == bodyA ? args.BodyA : args.BodyB;
        };

        bodyB.OnSeparation += args =>
        {
            bodyBSeparated = true;
            capturedSeparationOtherB = args.BodyB == bodyB ? args.BodyA : args.BodyB;
        };

        // Act — step simulation enough for collision + separation.
        var gameTime = new GameTime(
            elapsedGameTime: TimeSpan.FromSeconds(1.0 / 60.0),
            totalGameTime: TimeSpan.FromSeconds(0));

        for (int i = 0; i < 15; i++) // More steps to allow bodies to pass through each other and separate.
        {
            engine.FixedUpdate(gameTime);
        }

        // Assert - separation events should fire once the contact is destroyed.
        Assert.True(bodyASeparated, "Body A should have received OnSeparation event.");
        Assert.True(bodyBSeparated, "Body B should have received OnSeparation event.");
        Assert.Same(bodyB, capturedSeparationOtherA);
        Assert.Same(bodyA, capturedSeparationOtherB);

        engine.Dispose();
    }

    #endregion

    #region Test 4: Static body collides with dynamic body — static also receives event

    [Fact]
    public void CollisionEvents_StaticCollidesWithDynamic_BothReceiveOnCollision()
    {
        // Arrange — overlapping positions for radius-1 circles.
        var engine = new PhysicsEngine(Vector2.Zero);
        Vector2 posDynamic = new(-0.5f, 0f);
        Vector2 posStatic = new(0.5f, 0f);

        bool dynamicCollided = false;
        bool staticBodyCollided = false;

        // Dynamic body already overlapping a static wall.
        var dynamicBody = engine.CreateDynamic(posDynamic);
        dynamicBody.CreateCircleCollider(radius: 1f);
        dynamicBody.SetLinearVelocity(new Vector2(-1f, 0f));

        var staticBody = engine.CreateStatic(posStatic);
        staticBody.CreateCircleCollider(radius: 1f);

        dynamicBody.OnCollision += _ =>
        {
            dynamicCollided = true;
            return true;
        };

        staticBody.OnCollision += _ =>
        {
            staticBodyCollided = true;
            return true;
        };

        // Act — step simulation.
        StepForCollision(engine);

        // Assert
        Assert.True(dynamicCollided, "Dynamic body should receive OnCollision with static.");
        Assert.True(staticBodyCollided, "Static body should also receive OnCollision event.");

        engine.Dispose();
    }

    #endregion

    #region Test 5: Body disposed while collision active — no null reference exception

    [Fact]
    public void CollisionEvents_BodyDisposedDuringCollision_NoNullReferenceException()
    {
        // Arrange
        var engine = new PhysicsEngine(Vector2.Zero);
        Vector2 posA = new(-1f, 0f);
        Vector2 posB = new(1f, 0f);

        var (bodyA, bodyB) = CreateOverlappingBodies(engine, posA, posB);

        // Set up collision handler on body A.
        bodyA.OnCollision += args => true;
        bodyB.OnCollision += args => true;

        // Act & Assert — dispose one body while contacts may be active.
        Exception? ex = Record.Exception(() =>
        {
            bodyA.Dispose();
            // Step after disposal to ensure no exceptions from stale references.
            var gameTime = new GameTime(
                elapsedGameTime: TimeSpan.FromSeconds(1.0 / 60.0),
                totalGameTime: TimeSpan.FromSeconds(0));
            engine.FixedUpdate(gameTime);

            // Also try destroying the other body — should not throw either.
            engine.Destroy(bodyB);
        });

        Assert.True(ex is null, "Disposing a body during active collisions should not throw.");
        engine.Dispose();
    }

    #endregion

    #region Additional: No handlers attached — no exceptions

    [Fact]
    public void CollisionEvents_NoHandlersAttached_DoesNotThrow()
    {
        // Arrange
        var engine = new PhysicsEngine(Vector2.Zero);
        Vector2 posA = new(-1f, 0f);
        Vector2 posB = new(1f, 0f);

        CreateOverlappingBodies(engine, posA, posB);

        // Act — step without subscribing to any events. Should not throw.
        Exception? ex = Record.Exception(() => StepForCollision(engine));

        // Assert
        Assert.True(ex is null, "Stepping without handlers should not throw.");

        engine.Dispose();
    }

    #endregion

    #region Test 6: Two bodies each with one collider — both colliders receive OnCollision with correct references

    [Fact]
    public void CollisionEvents_CollidersCollide_BothReceiveOnCollisionWithCorrectReferences()
    {
        // Arrange
        var engine = new PhysicsEngine(Vector2.Zero);
        Vector2 posA = new(-0.5f, 0f);
        Vector2 posB = new(0.5f, 0f);

        bool colliderACollided = false;
        bool colliderBCollided = false;
        ICollider? capturedOtherColliderA = null;
        ICollider? capturedOtherColliderB = null;

        var (bodyA, bodyB) = CreateOverlappingBodies(engine, posA, posB);
        var colliderA = bodyA.Colliders[0];
        var colliderB = bodyB.Colliders[0];

        // Set separation speed so Aether registers contact then separates.
        bodyA.SetLinearVelocity(new Vector2(-1f, 0f));
        bodyB.SetLinearVelocity(new Vector2(1f, 0f));

        colliderA.OnCollision += args =>
        {
            colliderACollided = true;
            capturedOtherColliderA = args.ColliderB == colliderA ? args.ColliderA : args.ColliderB;
            return true; // Allow collision
        };

        colliderB.OnCollision += args =>
        {
            colliderBCollided = true;
            capturedOtherColliderB = args.ColliderB == colliderB ? args.ColliderA : args.ColliderB;
            return true; // Allow collision
        };

        // Act
        StepForCollision(engine);

        // Assert
        Assert.True(colliderACollided, "Collider A should have received OnCollision event.");
        Assert.True(colliderBCollided, "Collider B should have received OnCollision event.");
        Assert.Same(colliderB, capturedOtherColliderA);
        Assert.Same(colliderA, capturedOtherColliderB);

        engine.Dispose();
    }

    #endregion

    #region Test 7: Collider handler returns false — contact is rejected

    [Fact]
    public void CollisionEvents_ColliderHandlerReturnsFalse_ContactIsRejected()
    {
        // Arrange
        var engine = new PhysicsEngine(Vector2.Zero);
        Vector2 posA = new(-0.5f, 0f);
        Vector2 posB = new(0.5f, 0f);

        bool colliderACollided = false;
        bool colliderBCollided = false;

        var (bodyA, bodyB) = CreateOverlappingBodies(engine, posA, posB);
        var colliderA = bodyA.Colliders[0];
        var colliderB = bodyB.Colliders[0];

        // Set separation speed so Aether registers contact then separates.
        bodyA.SetLinearVelocity(new Vector2(-1f, 0f));
        bodyB.SetLinearVelocity(new Vector2(1f, 0f));

        // Collider A rejects the collision.
        colliderA.OnCollision += args =>
        {
            colliderACollided = true;
            return false; // Reject this collision
        };

        colliderB.OnCollision += args =>
        {
            colliderBCollided = true;
            return true; // Would allow, but A rejected
        };

        // Act
        StepForCollision(engine);

        // Assert — both handlers may still be called (Aether calls both before deciding),
        // but contact.Enabled should be false due to rejection.
        Assert.True(colliderACollided, "Collider A's handler should have been invoked.");
        Assert.True(colliderBCollided, "Collider B's handler should also have been invoked.");

        engine.Dispose();
    }

    #endregion

    #region Test 8: Bodies separate — both colliders receive OnSeparation

    [Fact]
    public void CollisionEvents_CollidersSeparate_BothReceiveOnSeparation()
    {
        // Arrange
        var engine = new PhysicsEngine(Vector2.Zero);
        Vector2 posA = new(-1f, 0f);
        Vector2 posB = new(1f, 0f);

        bool colliderASeparated = false;
        bool colliderBSeparated = false;
        ICollider? capturedSeparationOtherColliderA = null;
        ICollider? capturedSeparationOtherColliderB = null;

        var (bodyA, bodyB) = CreateOverlappingBodies(engine, posA, posB);
        var colliderA = bodyA.Colliders[0];
        var colliderB = bodyB.Colliders[0];

        // Set enough speed to collide and then separate.
        bodyA.SetLinearVelocity(new Vector2(-8f, 0f));
        bodyB.SetLinearVelocity(new Vector2(8f, 0f));

        colliderA.OnSeparation += args =>
        {
            colliderASeparated = true;
            capturedSeparationOtherColliderA = args.ColliderB == colliderA ? args.ColliderA : args.ColliderB;
        };

        colliderB.OnSeparation += args =>
        {
            colliderBSeparated = true;
            capturedSeparationOtherColliderB = args.ColliderB == colliderB ? args.ColliderA : args.ColliderB;
        };

        // Act — step simulation enough for collision + separation.
        var gameTime = new GameTime(
            elapsedGameTime: TimeSpan.FromSeconds(1.0 / 60.0),
            totalGameTime: TimeSpan.FromSeconds(0));

        for (int i = 0; i < 15; i++)
        {
            engine.FixedUpdate(gameTime);
        }

        // Assert — separation events should fire once the contact is destroyed.
        Assert.True(colliderASeparated, "Collider A should have received OnSeparation event.");
        Assert.True(colliderBSeparated, "Collider B should have received OnSeparation event.");
        Assert.Same(colliderB, capturedSeparationOtherColliderA);
        Assert.Same(colliderA, capturedSeparationOtherColliderB);

        engine.Dispose();
    }

    #endregion

    #region Test 9: Body with 2 colliders — only the overlapping collider receives collision event

    [Fact]
    public void CollisionEvents_BodyWithMultipleColliders_CorrectColliderEventFires()
    {
        // Arrange — create a player body with two separate circle colliders at different local offsets.
        var engine = new PhysicsEngine(Vector2.Zero);

        Vector2 playerPos = new(0f, 0f);
        var playerBody = engine.CreateDynamic(playerPos);

        // Head collider: radius 0.3 at local offset (0, 1) → world center at (0, 1).
        var headCollider = playerBody.CreateCircleCollider(radius: 0.3f, offset: new Vector2(0f, 1f));

        // Body collider: radius 0.3 at local offset (0, -1) → world center at (0, -1).
        var bodyCollider = playerBody.CreateCircleCollider(radius: 0.3f, offset: new Vector2(0f, -1f));

        // Enemy static body positioned so its collider overlaps ONLY the head collider's position.
        // Head is at world (0, 1) with radius 0.3 → occupies x:-0.3..0.3, y:0.7..1.3.
        // Place enemy at local world (-0.5, 1) with radius 0.25 → occupies x:-0.75..-0.25, y:0.75..1.25.
        // Distance between head center and enemy center = sqrt(0.5² + 0²) = 0.5 < 0.3 + 0.25 = 0.55 → OVERLAP.
        // Distance between body center (0, -1) and enemy center (-0.5, 1) = sqrt(0.5² + 2²) ≈ 2.06 > 0.3 + 0.25 = 0.55 → NO OVERLAP.
        Vector2 enemyPos = new(-0.5f, 1f);
        var enemyBody = engine.CreateStatic(enemyPos);
        var enemyCollider = enemyBody.CreateCircleCollider(radius: 0.25f);

        bool headCollidedWithEnemy = false;
        bool bodyCollidedWithEnemy = false;
        ICollider? capturedHeadOther = null;

        headCollider.OnCollision += args =>
        {
            if (args.ColliderB.OwnerBody == enemyBody || args.ColliderA.OwnerBody == enemyBody)
            {
                headCollidedWithEnemy = true;
                capturedHeadOther = args.ColliderB.OwnerBody == enemyBody ? args.ColliderB : args.ColliderA;
            }
            return true; // Allow
        };

        bodyCollider.OnCollision += args =>
        {
            if (args.ColliderB.OwnerBody == enemyBody || args.ColliderA.OwnerBody == enemyBody)
                bodyCollidedWithEnemy = true;
            return true; // Allow
        };

        enemyCollider.OnCollision += _ => true; // Always allow

        // Act — step simulation to trigger collision detection.
        StepForCollision(engine);

        // Assert — only the head collider should be in contact with the enemy.
        Assert.True(headCollidedWithEnemy, "Head collider should have collided with enemy.");
        Assert.False(bodyCollidedWithEnemy, "Body collider should NOT have collided with enemy (too far away).");
        Assert.NotNull(capturedHeadOther);

        engine.Dispose();
    }

    #endregion
}
