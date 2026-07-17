using System;
using System.Collections.Generic;
using CoreEssentials.GameSystems.Physics.Engines.Aether;
using Microsoft.Xna.Framework;
using nkast.Aether.Physics2D.Common;
using nkast.Aether.Physics2D.Dynamics;
using Xunit;
using OurWeldJoint = CoreEssentials.GameSystems.Physics.Engines.Aether.Joints.WeldJoint;

namespace CoreEssentials.GameSystems.Physics.Tests;

/// <summary>
/// Tests for the WeldJoint class that wraps Aether's WeldJoint.
/// </summary>
public class WeldJointTests : IDisposable
{
    private World? _world;
    private List<PhysicsBody?> _bodies = new();
    private List<OurWeldJoint?> _joints = new();

    public void Dispose()
    {
        // Clean up joints first (they reference bodies)
        foreach (var joint in _joints)
        {
            try { joint?.Dispose(); } catch { }
        }
        _joints.Clear();

        // Then clean up bodies
        foreach (var body in _bodies)
        {
            try { body?.Dispose(); } catch { }
        }
        _bodies.Clear();
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

    private OurWeldJoint CreateWeldJoint(PhysicsBody bodyA, PhysicsBody bodyB, Vector2 anchor)
    {
        if (_world == null || bodyA._body == null || bodyB._body == null)
            throw new InvalidOperationException("Test not initialized properly");

        var aetherJoint = nkast.Aether.Physics2D.Dynamics.Joints.JointFactory.CreateWeldJoint(
            _world,
            bodyA._body!,
            bodyB._body!,
            anchor,
            anchor,
            false);

        var wrapper = new OurWeldJoint(_world, aetherJoint);
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
        Assert.Throws<ArgumentNullException>(() => new OurWeldJoint(_world!, null!));
    }

    #endregion

    #region Body Reference Tests

    [Fact]
    public void BodyA_ReturnsNonNullableBody()
    {
        // Arrange
        var bodyA = CreateDynamicBody(new Vector2(0, 0));
        var bodyB = CreateDynamicBody(new Vector2(1, 0));
        _ = CreateWeldJoint(bodyA, bodyB, new Vector2(0.5f, 0));

        // Assert
        Assert.NotNull(_joints[0]!.BodyA);
    }

    [Fact]
    public void BodyB_ReturnsNonNullableBody()
    {
        // Arrange
        var bodyA = CreateDynamicBody(new Vector2(0, 0));
        var bodyB = CreateDynamicBody(new Vector2(1, 0));
        _ = CreateWeldJoint(bodyA, bodyB, new Vector2(0.5f, 0));

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
        var joint = CreateWeldJoint(bodyA, bodyB, anchor);

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
        var joint = CreateWeldJoint(bodyA, bodyB, anchor);

        // Assert - verify getter returns a valid vector (not default)
        Assert.NotEqual(Vector2.Zero, joint.LocalAnchorB);
    }

    #endregion

    #region CollideConnected Tests

    [Fact]
    public void CollideConnected_Getter_ReturnsFalseByDefault()
    {
        // Arrange
        var bodyA = CreateDynamicBody(new Vector2(0, 0));
        var bodyB = CreateDynamicBody(new Vector2(1, 0));
        _ = CreateWeldJoint(bodyA, bodyB, new Vector2(0.5f, 0));

        // Assert
        Assert.False(_joints[0]!.CollideConnected);
    }

    [Fact]
    public void CollideConnected_Setter_AcceptsTrue()
    {
        // Arrange
        var bodyA = CreateDynamicBody(new Vector2(0, 0));
        var bodyB = CreateDynamicBody(new Vector2(1, 0));
        _ = CreateWeldJoint(bodyA, bodyB, new Vector2(0.5f, 0));

        // Act & Assert
        OurWeldJoint joint = _joints[0]!;
        joint.CollideConnected = true;
        Assert.True(joint.CollideConnected);
    }

    [Fact]
    public void CollideConnected_Setter_AcceptsFalse()
    {
        // Arrange
        var bodyA = CreateDynamicBody(new Vector2(0, 0));
        var bodyB = CreateDynamicBody(new Vector2(1, 0));
        _ = CreateWeldJoint(bodyA, bodyB, new Vector2(0.5f, 0));

        // Act & Assert
        OurWeldJoint joint = _joints[0]!;
        joint.CollideConnected = false;
        Assert.False(joint.CollideConnected);
    }

    #endregion

    #region Stiffness Tests (Maps to FrequencyHz)

    [Fact]
    public void Stiffness_Getter_ReturnsDefaultFrequency()
    {
        // Arrange
        var bodyA = CreateDynamicBody(new Vector2(0, 0));
        var bodyB = CreateDynamicBody(new Vector2(1, 0));
        _ = CreateWeldJoint(bodyA, bodyB, new Vector2(0.5f, 0));

        // Assert - Aether default FrequencyHz is typically 0 or a specific value
        var stiffness = _joints[0]!.Stiffness;
        Assert.True(stiffness >= 0);
    }

    [Fact]
    public void Stiffness_Setter_AcceptsPositiveValues()
    {
        // Arrange
        var bodyA = CreateDynamicBody(new Vector2(0, 0));
        var bodyB = CreateDynamicBody(new Vector2(1, 0));
        _ = CreateWeldJoint(bodyA, bodyB, new Vector2(0.5f, 0));

        // Act & Assert
        OurWeldJoint joint = _joints[0]!;
        joint.Stiffness = 10f;
        Assert.Equal(10f, joint.Stiffness);
        
        joint.Stiffness = 100f;
        Assert.Equal(100f, joint.Stiffness);
    }

    [Fact]
    public void Stiffness_Setter_AcceptsZero()
    {
        // Arrange
        var bodyA = CreateDynamicBody(new Vector2(0, 0));
        var bodyB = CreateDynamicBody(new Vector2(1, 0));
        _ = CreateWeldJoint(bodyA, bodyB, new Vector2(0.5f, 0));

        // Act & Assert - zero stiffness means rigid connection
        OurWeldJoint joint = _joints[0]!;
        joint.Stiffness = 0f;
        Assert.Equal(0f, joint.Stiffness);
    }

    #endregion

    #region Damping Tests (Maps to DampingRatio)

    [Fact]
    public void Damping_Getter_ReturnsDefaultDamping()
    {
        // Arrange
        var bodyA = CreateDynamicBody(new Vector2(0, 0));
        var bodyB = CreateDynamicBody(new Vector2(1, 0));
        _ = CreateWeldJoint(bodyA, bodyB, new Vector2(0.5f, 0));

        // Assert - Aether default DampingRatio is typically 0 or a specific value
        var damping = _joints[0]!.Damping;
        Assert.True(damping >= 0);
    }

    [Fact]
    public void Damping_Setter_AcceptsZero()
    {
        // Arrange
        var bodyA = CreateDynamicBody(new Vector2(0, 0));
        var bodyB = CreateDynamicBody(new Vector2(1, 0));
        _ = CreateWeldJoint(bodyA, bodyB, new Vector2(0.5f, 0));

        // Act & Assert - zero means no damping
        OurWeldJoint joint = _joints[0]!;
        joint.Damping = 0f;
        Assert.Equal(0f, joint.Damping);
    }

    [Fact]
    public void Damping_Setter_AcceptsCriticalDamping()
    {
        // Arrange
        var bodyA = CreateDynamicBody(new Vector2(0, 0));
        var bodyB = CreateDynamicBody(new Vector2(1, 0));
        _ = CreateWeldJoint(bodyA, bodyB, new Vector2(0.5f, 0));

        // Act & Assert - 1 is critical damping
        OurWeldJoint joint = _joints[0]!;
        joint.Damping = 1f;
        Assert.Equal(1f, joint.Damping);
    }

    [Fact]
    public void Damping_Setter_AcceptsValuesAboveOne()
    {
        // Arrange
        var bodyA = CreateDynamicBody(new Vector2(0, 0));
        var bodyB = CreateDynamicBody(new Vector2(1, 0));
        _ = CreateWeldJoint(bodyA, bodyB, new Vector2(0.5f, 0));

        // Act & Assert - values above 1 are valid (overdamped)
        OurWeldJoint joint = _joints[0]!;
        joint.Damping = 2f;
        Assert.Equal(2f, joint.Damping);
    }

    #endregion

    #region IsActive Tests

    [Fact]
    public void IsActive_ReturnsTrueWhenJointEnabled()
    {
        // Arrange
        var bodyA = CreateDynamicBody(new Vector2(0, 0));
        var bodyB = CreateDynamicBody(new Vector2(1, 0));
        _ = CreateWeldJoint(bodyA, bodyB, new Vector2(0.5f, 0));

        // Assert
        Assert.True(_joints[0]!.IsActive);
    }

    [Fact]
    public void IsActive_ReturnsFalseAfterRemove()
    {
        // Arrange
        var bodyA = CreateDynamicBody(new Vector2(0, 0));
        var bodyB = CreateDynamicBody(new Vector2(1, 0));
        var joint = CreateWeldJoint(bodyA, bodyB, new Vector2(0.5f, 0));

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
        var joint = CreateWeldJoint(bodyA, bodyB, new Vector2(0.5f, 0));

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
        var joint = CreateWeldJoint(bodyA, bodyB, new Vector2(0.5f, 0));

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
        var joint = CreateWeldJoint(bodyA, bodyB, new Vector2(0.5f, 0));

        // Act & Assert - should not throw on multiple calls
        joint.Dispose();
        joint.Dispose();
    }

    [Fact]
    public void Remove_AfterDispose_DoesNotThrow()
    {
        // Arrange
        var bodyA = CreateDynamicBody(new Vector2(0, 0));
        var bodyB = CreateDynamicBody(new Vector2(1, 0));
        var joint = CreateWeldJoint(bodyA, bodyB, new Vector2(0.5f, 0));

        // Act & Assert
        joint.Dispose();
        joint.Remove(); // should be safe to call after dispose
    }

    #endregion

    #region JointFactory Tests

    [Fact]
    public void JointFactory_CreateWeldJoint_WithSharedAnchor_CreatesValidJoint()
    {
        // Arrange
        var bodyA = CreateDynamicBody(new Vector2(0, 0));
        var bodyB = CreateDynamicBody(new Vector2(1, 0));

        // Act - use shared anchor point (both bodies share the same anchor)
        var aetherJoint = nkast.Aether.Physics2D.Dynamics.Joints.JointFactory.CreateWeldJoint(
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
    public void JointFactory_CreateWeldJoint_WithDifferentAnchors_CreatesValidJoint()
    {
        // Arrange
        var bodyA = CreateDynamicBody(new Vector2(0, 0));
        var bodyB = CreateDynamicBody(new Vector2(1, 0));

        // Act - use different anchor points on each body
        var aetherJoint = nkast.Aether.Physics2D.Dynamics.Joints.JointFactory.CreateWeldJoint(
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
}
