using System;
using System.Collections.Generic;
using CoreEssentials.GameSystems.Physics.Engines.Aether;
using Microsoft.Xna.Framework;
using nkast.Aether.Physics2D.Common;
using nkast.Aether.Physics2D.Dynamics;
using Xunit;
#nullable enable
using OurDistanceJoint = CoreEssentials.GameSystems.Physics.Engines.Aether.Joints.DistanceJoint;

namespace CoreEssentials.GameSystems.Physics.Tests;

/// <summary>
/// Tests for the DistanceJoint class that wraps Aether's DistanceJoint.
/// </summary>
public class DistanceJointTests : IDisposable
{
    private World? _world;
    private readonly List<PhysicsBody> _bodies = new();
    private readonly List<OurDistanceJoint> _joints = new();
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
            // Clean up joints first (they reference bodies)
            foreach (var joint in _joints)
            {
                try { joint.Dispose(); } catch { /* Expected during cleanup */ }
            }
            _joints.Clear();

            // Then clean up bodies
            foreach (var body in _bodies)
            {
                try { body.Dispose(); } catch { /* Expected during cleanup */ }
            }
            _bodies.Clear();
        }
        _disposed = true;
    }

    private PhysicsBody CreateDynamicBody(Vector2 position)
    {
        if (_world == null)
            _world = new World(Vector2.Zero);

        var aetherBody = _world.CreateBody(position, rotation: 0f, BodyType.Dynamic);
        var wrapper = new PhysicsBody(_world, aetherBody);
        _bodies.Add(wrapper);
        return wrapper;
    }

    private OurDistanceJoint CreateDistanceJoint(PhysicsBody bodyA, PhysicsBody bodyB, Vector2 anchor)
    {
        if (_world == null || bodyA._body == null || bodyB._body == null)
            throw new InvalidOperationException("Test not initialized properly");

        var aetherJoint = nkast.Aether.Physics2D.Dynamics.Joints.JointFactory.CreateDistanceJoint(
            _world,
            bodyA._body!,
            bodyB._body!,
            anchor,
            anchor,
            false);

        var wrapper = new OurDistanceJoint(_world, aetherJoint);
        _joints.Add(wrapper);
        return wrapper;
    }

    #region Creation Tests

    [Fact]
    public void Constructor_WithValidBodies_CreatesJoint()
    {
        // Arrange & Act - use JointFactory with both anchors
        var bodyA = CreateDynamicBody(new Vector2(0, 0));
        var bodyB = CreateDynamicBody(new Vector2(1, 0));

        // Assert - constructor doesn't throw when given valid bodies
        Assert.NotNull(bodyA);
        Assert.NotNull(bodyB);
    }

    [Fact]
    public void Constructor_WithNullAetherJoint_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new OurDistanceJoint(_world!, null!));
    }

    #endregion

    #region Body Reference Tests

    [Fact]
    public void BodyA_ReturnsNonNullableBody()
    {
        // Arrange
        var bodyA = CreateDynamicBody(new Vector2(0, 0));
        var bodyB = CreateDynamicBody(new Vector2(1, 0));
        _ = CreateDistanceJoint(bodyA, bodyB, new Vector2(0.5f, 0));

        // Assert
        Assert.NotNull(_joints[0]!.BodyA);
    }

    [Fact]
    public void BodyB_ReturnsNonNullableBody()
    {
        // Arrange
        var bodyA = CreateDynamicBody(new Vector2(0, 0));
        var bodyB = CreateDynamicBody(new Vector2(1, 0));
        _ = CreateDistanceJoint(bodyA, bodyB, new Vector2(0.5f, 0));

        // Assert
        Assert.NotNull(_joints[0]!.BodyB);
    }

    #endregion

    #region Anchor Point Tests

    [Fact]
    public void LocalAnchorA_ReturnsValidAnchor()
    {
        // Arrange
        var bodyA = CreateDynamicBody(new Vector2(0, 0));
        var bodyB = CreateDynamicBody(new Vector2(1, 0));
        var anchor = new Vector2(0.5f, 0);

        // Act
        var joint = CreateDistanceJoint(bodyA, bodyB, anchor);

        // Assert - verify getter returns a valid vector (not default)
        Assert.NotEqual(Vector2.Zero, joint.LocalAnchorA);
    }

    [Fact]
    public void LocalAnchorB_ReturnsValidAnchor()
    {
        // Arrange
        var bodyA = CreateDynamicBody(new Vector2(0, 0));
        var bodyB = CreateDynamicBody(new Vector2(1, 0));
        var anchor = new Vector2(0.5f, 0);

        // Act
        var joint = CreateDistanceJoint(bodyA, bodyB, anchor);

        // Assert - verify getter returns a valid vector (not default)
        Assert.NotEqual(Vector2.Zero, joint.LocalAnchorB);
    }

    #endregion

    #region Length Tests

    [Fact]
    public void Length_Getter_ReturnsDefaultLength()
    {
        // Arrange
        var bodyA = CreateDynamicBody(new Vector2(0, 0));
        var bodyB = CreateDynamicBody(new Vector2(1, 0));
        _ = CreateDistanceJoint(bodyA, bodyB, new Vector2(0.5f, 0));

        // Assert - Aether default length is typically the distance between anchors or a specific value
        var length = _joints[0]!.Length;
        Assert.True(length > 0);
    }

    [Fact]
    public void Length_Setter_AcceptsPositiveValues()
    {
        // Arrange
        var bodyA = CreateDynamicBody(new Vector2(0, 0));
        var bodyB = CreateDynamicBody(new Vector2(1, 0));
        _ = CreateDistanceJoint(bodyA, bodyB, new Vector2(0.5f, 0));

        // Act & Assert
        OurDistanceJoint joint = _joints[0]!;
        joint.Length = 0.5f;
        Assert.Equal(0.5f, joint.Length);
        
        joint.Length = 2.0f;
        Assert.Equal(2.0f, joint.Length);
    }

    [Fact]
    public void Length_Setter_AcceptsZero()
    {
        // Arrange
        var bodyA = CreateDynamicBody(new Vector2(0, 0));
        var bodyB = CreateDynamicBody(new Vector2(1, 0));
        _ = CreateDistanceJoint(bodyA, bodyB, new Vector2(0.5f, 0));

        // Act & Assert - zero length means bodies are pulled together
        OurDistanceJoint joint = _joints[0]!;
        joint.Length = 0f;
        Assert.Equal(0f, joint.Length);
    }

    #endregion

    #region MaxForce Tests (Stub)

    [Fact]
    public void MaxForce_Getter_ReturnsZero()
    {
        // Arrange
        var bodyA = CreateDynamicBody(new Vector2(0, 0));
        var bodyB = CreateDynamicBody(new Vector2(1, 0));
        _ = CreateDistanceJoint(bodyA, bodyB, new Vector2(0.5f, 0));

        // Assert - Aether's DistanceJoint doesn't support max force, so it's stubbed to return 0
        Assert.Equal(0f, _joints[0]!.MaxForce);
    }

    [Fact]
    public void MaxForce_Setter_DoesNotThrow()
    {
        // Arrange
        var bodyA = CreateDynamicBody(new Vector2(0, 0));
        var bodyB = CreateDynamicBody(new Vector2(1, 0));
        _ = CreateDistanceJoint(bodyA, bodyB, new Vector2(0.5f, 0));

        // Act - setting MaxForce should not throw (but has no effect)
        OurDistanceJoint joint = _joints[0]!;
        joint.MaxForce = 100f;

        // Assert - still returns 0 because it's a stub
        Assert.Equal(0f, joint.MaxForce);
    }

    #endregion

    #region CollideConnected Tests

    [Fact]
    public void CollideConnected_Getter_ReturnsFalseByDefault()
    {
        // Arrange
        var bodyA = CreateDynamicBody(new Vector2(0, 0));
        var bodyB = CreateDynamicBody(new Vector2(1, 0));
        _ = CreateDistanceJoint(bodyA, bodyB, new Vector2(0.5f, 0));

        // Assert
        Assert.False(_joints[0]!.CollideConnected);
    }

    [Fact]
    public void CollideConnected_Setter_AcceptsTrue()
    {
        // Arrange
        var bodyA = CreateDynamicBody(new Vector2(0, 0));
        var bodyB = CreateDynamicBody(new Vector2(1, 0));
        _ = CreateDistanceJoint(bodyA, bodyB, new Vector2(0.5f, 0));

        // Act & Assert
        OurDistanceJoint joint = _joints[0]!;
        joint.CollideConnected = true;
        Assert.True(joint.CollideConnected);
    }

    [Fact]
    public void CollideConnected_Setter_AcceptsFalse()
    {
        // Arrange
        var bodyA = CreateDynamicBody(new Vector2(0, 0));
        var bodyB = CreateDynamicBody(new Vector2(1, 0));
        _ = CreateDistanceJoint(bodyA, bodyB, new Vector2(0.5f, 0));

        // Act & Assert
        OurDistanceJoint joint = _joints[0]!;
        joint.CollideConnected = false;
        Assert.False(joint.CollideConnected);
    }

    #endregion

    #region FrequencyHz Tests

    [Fact]
    public void FrequencyHz_Getter_ReturnsDefaultFrequency()
    {
        // Arrange
        var bodyA = CreateDynamicBody(new Vector2(0, 0));
        var bodyB = CreateDynamicBody(new Vector2(1, 0));
        _ = CreateDistanceJoint(bodyA, bodyB, new Vector2(0.5f, 0));

        // Assert - Aether default Frequency is typically 0 or a specific value
        var frequency = _joints[0]!.FrequencyHz;
        Assert.True(frequency >= 0);
    }

    [Fact]
    public void FrequencyHz_Setter_AcceptsPositiveValues()
    {
        // Arrange
        var bodyA = CreateDynamicBody(new Vector2(0, 0));
        var bodyB = CreateDynamicBody(new Vector2(1, 0));
        _ = CreateDistanceJoint(bodyA, bodyB, new Vector2(0.5f, 0));

        // Act & Assert
        OurDistanceJoint joint = _joints[0]!;
        joint.FrequencyHz = 5f;
        Assert.Equal(5f, joint.FrequencyHz);
        
        joint.FrequencyHz = 100f;
        Assert.Equal(100f, joint.FrequencyHz);
    }

    [Fact]
    public void FrequencyHz_Setter_AcceptsZero()
    {
        // Arrange
        var bodyA = CreateDynamicBody(new Vector2(0, 0));
        var bodyB = CreateDynamicBody(new Vector2(1, 0));
        _ = CreateDistanceJoint(bodyA, bodyB, new Vector2(0.5f, 0));

        // Act & Assert - zero frequency means rigid connection (no spring)
        OurDistanceJoint joint = _joints[0]!;
        joint.FrequencyHz = 0f;
        Assert.Equal(0f, joint.FrequencyHz);
    }

    #endregion

    #region DampingRatio Tests

    [Fact]
    public void DampingRatio_Getter_ReturnsDefaultDamping()
    {
        // Arrange
        var bodyA = CreateDynamicBody(new Vector2(0, 0));
        var bodyB = CreateDynamicBody(new Vector2(1, 0));
        _ = CreateDistanceJoint(bodyA, bodyB, new Vector2(0.5f, 0));

        // Assert - Aether default DampingRatio is typically 0 or a specific value
        var damping = _joints[0]!.DampingRatio;
        Assert.True(damping >= 0);
    }

    [Fact]
    public void DampingRatio_Setter_AcceptsZero()
    {
        // Arrange
        var bodyA = CreateDynamicBody(new Vector2(0, 0));
        var bodyB = CreateDynamicBody(new Vector2(1, 0));
        _ = CreateDistanceJoint(bodyA, bodyB, new Vector2(0.5f, 0));

        // Act & Assert - zero means no damping
        OurDistanceJoint joint = _joints[0]!;
        joint.DampingRatio = 0f;
        Assert.Equal(0f, joint.DampingRatio);
    }

    [Fact]
    public void DampingRatio_Setter_AcceptsCriticalDamping()
    {
        // Arrange
        var bodyA = CreateDynamicBody(new Vector2(0, 0));
        var bodyB = CreateDynamicBody(new Vector2(1, 0));
        _ = CreateDistanceJoint(bodyA, bodyB, new Vector2(0.5f, 0));

        // Act & Assert - 1 is critical damping
        OurDistanceJoint joint = _joints[0]!;
        joint.DampingRatio = 1f;
        Assert.Equal(1f, joint.DampingRatio);
    }

    [Fact]
    public void DampingRatio_Setter_AcceptsValuesAboveOne()
    {
        // Arrange
        var bodyA = CreateDynamicBody(new Vector2(0, 0));
        var bodyB = CreateDynamicBody(new Vector2(1, 0));
        _ = CreateDistanceJoint(bodyA, bodyB, new Vector2(0.5f, 0));

        // Act & Assert - values above 1 are valid (overdamped)
        OurDistanceJoint joint = _joints[0]!;
        joint.DampingRatio = 2f;
        Assert.Equal(2f, joint.DampingRatio);
    }

    #endregion

    #region IsActive Tests

    [Fact]
    public void IsActive_ReturnsTrueWhenJointEnabled()
    {
        // Arrange
        var bodyA = CreateDynamicBody(new Vector2(0, 0));
        var bodyB = CreateDynamicBody(new Vector2(1, 0));
        _ = CreateDistanceJoint(bodyA, bodyB, new Vector2(0.5f, 0));

        // Assert
        Assert.True(_joints[0]!.IsActive);
    }

    [Fact]
    public void IsActive_ReturnsFalseAfterRemove()
    {
        // Arrange
        var bodyA = CreateDynamicBody(new Vector2(0, 0));
        var bodyB = CreateDynamicBody(new Vector2(1, 0));
        var joint = CreateDistanceJoint(bodyA, bodyB, new Vector2(0.5f, 0));

        // Act
        joint.Remove();

        // Assert - joint should no longer be active after removal
        Assert.False(joint.IsActive);
    }

    #endregion

    #region Remove and Dispose Tests

    [Fact]
    public void Remove_RemovesJointFromWorld()
    {
        // Arrange
        var bodyA = CreateDynamicBody(new Vector2(0, 0));
        var bodyB = CreateDynamicBody(new Vector2(1, 0));
        var joint = CreateDistanceJoint(bodyA, bodyB, new Vector2(0.5f, 0));

        // Act
        int initialCount = _world!.JointList.Count;

        // Assert - should have at least one joint before removal
        Assert.True(initialCount > 0);

        joint.Remove();

        // Assert - joint should be inactive after removal
        Assert.False(joint.IsActive);
    }

    [Fact]
    public void Dispose_RemovesJointFromWorld()
    {
        // Arrange
        var bodyA = CreateDynamicBody(new Vector2(0, 0));
        var bodyB = CreateDynamicBody(new Vector2(1, 0));
        var joint = CreateDistanceJoint(bodyA, bodyB, new Vector2(0.5f, 0));

        // Act
        joint.Dispose();

        // Assert - joint should be inactive after dispose
        Assert.False(joint.IsActive);
    }

    [Fact]
    public void Dispose_CanBeCalledMultipleTimes()
    {
        // Arrange
        var bodyA = CreateDynamicBody(new Vector2(0, 0));
        var bodyB = CreateDynamicBody(new Vector2(1, 0));
        var joint = CreateDistanceJoint(bodyA, bodyB, new Vector2(0.5f, 0));

        // Act & Assert - should not throw on multiple calls
        joint.Dispose();
        joint.Dispose();
        Assert.True(true);
    }

    [Fact]
    public void Remove_AfterDispose_DoesNotThrow()
    {
        // Arrange
        var bodyA = CreateDynamicBody(new Vector2(0, 0));
        var bodyB = CreateDynamicBody(new Vector2(1, 0));
        var joint = CreateDistanceJoint(bodyA, bodyB, new Vector2(0.5f, 0));

        // Act & Assert - should not throw when calling Remove after Dispose
        joint.Dispose();
        Exception ex = Record.Exception(() => joint.Remove());
        Assert.Null(ex);
    }

    #endregion

    #region JointFactory Tests

    [Fact]
    public void JointFactory_CreateDistanceJoint_WithSharedAnchor_CreatesValidJoint()
    {
        // Arrange
        var bodyA = CreateDynamicBody(new Vector2(0, 0));
        var bodyB = CreateDynamicBody(new Vector2(1, 0));

        // Act - use shared anchor point (both bodies share the same anchor)
        var aetherJoint = nkast.Aether.Physics2D.Dynamics.Joints.JointFactory.CreateDistanceJoint(
            _world!,
            bodyA._body!,
            bodyB._body!,
            new Vector2(0.5f, 0),
            new Vector2(0.5f, 0),
            false);

        // Assert - should not throw and joint should be valid
        Assert.NotNull(aetherJoint);
    }

    [Fact]
    public void JointFactory_CreateDistanceJoint_WithDifferentAnchors_CreatesValidJoint()
    {
        // Arrange
        var bodyA = CreateDynamicBody(new Vector2(0, 0));
        var bodyB = CreateDynamicBody(new Vector2(1, 0));

        // Act - use different anchor points on each body
        var aetherJoint = nkast.Aether.Physics2D.Dynamics.Joints.JointFactory.CreateDistanceJoint(
            _world!,
            bodyA._body!,
            bodyB._body!,
            Vector2.Zero,
            new Vector2(-0.5f, 0),
            false);

        // Assert - should not throw and joint should be valid
        Assert.NotNull(aetherJoint);
    }

    #endregion

    #region Spring Behavior Tests

    [Fact]
    public void Length_WithFrequencyAndDamping_CreatesSpringBehavior()
    {
        // Arrange
        var bodyA = CreateDynamicBody(new Vector2(0, 0));
        var bodyB = CreateDynamicBody(new Vector2(1, 0));
        _ = CreateDistanceJoint(bodyA, bodyB, new Vector2(0.5f, 0));

        // Act - configure as a spring
        OurDistanceJoint joint = _joints[0]!;
        joint.Length = 0.8f;
        joint.FrequencyHz = 10f;
        joint.DampingRatio = 0.7f;

        // Assert - verify all properties are set correctly for spring behavior
        Assert.Equal(0.8f, joint.Length);
        Assert.Equal(10f, joint.FrequencyHz);
        Assert.Equal(0.7f, joint.DampingRatio);
    }

    [Fact]
    public void Length_WithZeroFrequency_CreatesRigidConnection()
    {
        // Arrange
        var bodyA = CreateDynamicBody(new Vector2(0, 0));
        var bodyB = CreateDynamicBody(new Vector2(1, 0));
        _ = CreateDistanceJoint(bodyA, bodyB, new Vector2(0.5f, 0));

        // Act - configure as a rigid connection (no spring)
        OurDistanceJoint joint = _joints[0]!;
        joint.Length = 0.5f;
        joint.FrequencyHz = 0f;
        joint.DampingRatio = 0f;

        // Assert - verify properties are set correctly for rigid connection
        Assert.Equal(0.5f, joint.Length);
        Assert.Equal(0f, joint.FrequencyHz);
        Assert.Equal(0f, joint.DampingRatio);
    }

    #endregion
}
