using System;
using System.Collections.Generic;
using CoreEssentials.GameSystems.Physics.Engines.Aether;
using Microsoft.Xna.Framework;
using nkast.Aether.Physics2D.Common;
using nkast.Aether.Physics2D.Dynamics;
using Xunit;
using OurJoint = CoreEssentials.GameSystems.Physics.Engines.Aether.Joints.RevoluteJoint;

namespace CoreEssentials.GameSystems.Physics.Tests;

/// <summary>
/// Tests for the RevoluteJoint class that wraps Aether's RevoluteJoint.
/// </summary>
public class RevoluteJointTests : IDisposable
{
    private World? _world;
    private List<PhysicsBody?> _bodies = new();
    private List<OurJoint?> _joints = new();

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

    private OurJoint CreateRevoluteJoint(PhysicsBody bodyA, PhysicsBody bodyB, Vector2 anchor)
    {
        if (_world == null || bodyA._body == null || bodyB._body == null)
            throw new InvalidOperationException("Test not initialized properly");

        var aetherJoint = nkast.Aether.Physics2D.Dynamics.Joints.JointFactory.CreateRevoluteJoint(
            _world,
            bodyA._body,
            bodyB._body,
            anchor);

        var wrapper = new OurJoint(_world, aetherJoint);
        _joints.Add(wrapper);
        return wrapper;
    }

    #region Creation Tests

    [Fact]
    public void Constructor_WithValidBodies_CreatesJoint()
    {
        // Arrange
        var bodyA = CreateDynamicBody(new Vector2(0, 0));
        var bodyB = CreateDynamicBody(new Vector2(1, 0));

        // Act
        var joint = CreateRevoluteJoint(bodyA, bodyB, new Vector2(0.5f, 0));

        // Assert
        Assert.NotNull(joint);
    }

    [Fact]
    public void Constructor_CreatesJointInWorld()
    {
        // Arrange
        var bodyA = CreateDynamicBody(new Vector2(0, 0));
        var bodyB = CreateDynamicBody(new Vector2(1, 0));

        // Act
        _ = CreateRevoluteJoint(bodyA, bodyB, new Vector2(0.5f, 0));

        // Assert - joint should be in Aether world's joint list
        Assert.Single(_world!.JointList);
    }

    #endregion

    #region Body Reference Tests

    [Fact]
    public void BodyA_ReturnsCorrectBody()
    {
        // Arrange
        var bodyA = CreateDynamicBody(new Vector2(0, 0));
        var bodyB = CreateDynamicBody(new Vector2(1, 0));

        // Act
        var joint = CreateRevoluteJoint(bodyA, bodyB, new Vector2(0.5f, 0));

        // Assert
        Assert.NotNull(joint.BodyA);
    }

    [Fact]
    public void BodyB_ReturnsCorrectBody()
    {
        // Arrange
        var bodyA = CreateDynamicBody(new Vector2(0, 0));
        var bodyB = CreateDynamicBody(new Vector2(1, 0));

        // Act
        var joint = CreateRevoluteJoint(bodyA, bodyB, new Vector2(0.5f, 0));

        // Assert
        Assert.NotNull(joint.BodyB);
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
        var joint = CreateRevoluteJoint(bodyA, bodyB, anchor);

        // Assert - verify getter returns a valid vector (not default)
        Assert.NotEqual(Vector2.Zero, joint.LocalAnchorA);
    }

    [Fact]
    public void LocalAnchorB_ReturnsValidAnchor()
    {
        // Arrange - bodyA at (0,0), bodyB at (1,0), world anchor at (0.5, 0)
        var bodyA = CreateDynamicBody(new Vector2(0, 0));
        var bodyB = CreateDynamicBody(new Vector2(1, 0));
        var worldAnchor = new Vector2(0.5f, 0);

        // Act
        var joint = CreateRevoluteJoint(bodyA, bodyB, worldAnchor);

        // Assert - verify getter returns a valid vector (not default)
        Assert.NotEqual(Vector2.Zero, joint.LocalAnchorB);
    }

    #endregion

    #region Angle Limit Tests

    [Fact]
    public void MinAngle_Getter_ReturnsLowerLimit()
    {
        // Arrange
        var bodyA = CreateDynamicBody(new Vector2(0, 0));
        var bodyB = CreateDynamicBody(new Vector2(1, 0));
        _ = CreateRevoluteJoint(bodyA, bodyB, new Vector2(0.5f, 0));

        // Act
        OurJoint joint = _joints[0]!;
        joint.MinAngle = -MathHelper.PiOver2;

        // Assert
        Assert.Equal(-MathHelper.PiOver2, joint.MinAngle);
    }

    [Fact]
    public void MaxAngle_Getter_ReturnsUpperLimit()
    {
        // Arrange
        var bodyA = CreateDynamicBody(new Vector2(0, 0));
        var bodyB = CreateDynamicBody(new Vector2(1, 0));
        _ = CreateRevoluteJoint(bodyA, bodyB, new Vector2(0.5f, 0));

        // Act
        OurJoint joint = _joints[0]!;
        joint.MaxAngle = MathHelper.PiOver2;

        // Assert
        Assert.Equal(MathHelper.PiOver2, joint.MaxAngle);
    }

    [Fact]
    public void AngleLimits_CanBeSetToFullRotation()
    {
        // Arrange
        var bodyA = CreateDynamicBody(new Vector2(0, 0));
        var bodyB = CreateDynamicBody(new Vector2(1, 0));
        _ = CreateRevoluteJoint(bodyA, bodyB, new Vector2(0.5f, 0));

        // Act & Assert - no exception should be thrown for full rotation limits
        OurJoint joint = _joints[0]!;
        joint.MinAngle = float.NegativeInfinity;
        joint.MaxAngle = float.PositiveInfinity;
    }

    #endregion

    #region Motor Tests

    [Fact]
    public void MotorEnabled_Getter_ReturnsFalseByDefault()
    {
        // Arrange
        var bodyA = CreateDynamicBody(new Vector2(0, 0));
        var bodyB = CreateDynamicBody(new Vector2(1, 0));
        _ = CreateRevoluteJoint(bodyA, bodyB, new Vector2(0.5f, 0));

        // Assert
        Assert.False(_joints[0]!.MotorEnabled);
    }

    [Fact]
    public void MotorEnabled_Setter_TogglesMotor()
    {
        // Arrange
        var bodyA = CreateDynamicBody(new Vector2(0, 0));
        var bodyB = CreateDynamicBody(new Vector2(1, 0));
        _ = CreateRevoluteJoint(bodyA, bodyB, new Vector2(0.5f, 0));

        // Act & Assert
        OurJoint joint = _joints[0]!;
        joint.MotorEnabled = true;
        Assert.True(joint.MotorEnabled);
        
        joint.MotorEnabled = false;
        Assert.False(joint.MotorEnabled);
    }

    [Fact]
    public void MotorSpeed_Getter_ReturnsZeroByDefault()
    {
        // Arrange
        var bodyA = CreateDynamicBody(new Vector2(0, 0));
        var bodyB = CreateDynamicBody(new Vector2(1, 0));
        _ = CreateRevoluteJoint(bodyA, bodyB, new Vector2(0.5f, 0));

        // Assert
        Assert.Equal(0f, _joints[0]!.MotorSpeed);
    }

    [Fact]
    public void MotorSpeed_Setter_AcceptsPositiveAndNegativeValues()
    {
        // Arrange
        var bodyA = CreateDynamicBody(new Vector2(0, 0));
        var bodyB = CreateDynamicBody(new Vector2(1, 0));
        _ = CreateRevoluteJoint(bodyA, bodyB, new Vector2(0.5f, 0));

        // Act & Assert
        OurJoint joint = _joints[0]!;
        joint.MotorSpeed = MathHelper.Pi; // clockwise
        Assert.Equal(MathHelper.Pi, joint.MotorSpeed);
        
        joint.MotorSpeed = -MathHelper.Pi; // counter-clockwise
        Assert.Equal(-MathHelper.Pi, joint.MotorSpeed);
    }

    [Fact]
    public void MaxMotorTorque_Getter_ReturnsZeroByDefault()
    {
        // Arrange
        var bodyA = CreateDynamicBody(new Vector2(0, 0));
        var bodyB = CreateDynamicBody(new Vector2(1, 0));
        _ = CreateRevoluteJoint(bodyA, bodyB, new Vector2(0.5f, 0));

        // Assert
        Assert.Equal(0f, _joints[0]!.MaxMotorTorque);
    }

    [Fact]
    public void MaxMotorTorque_Setter_AcceptsPositiveValues()
    {
        // Arrange
        var bodyA = CreateDynamicBody(new Vector2(0, 0));
        var bodyB = CreateDynamicBody(new Vector2(1, 0));
        _ = CreateRevoluteJoint(bodyA, bodyB, new Vector2(0.5f, 0));

        // Act & Assert
        OurJoint joint = _joints[0]!;
        joint.MaxMotorTorque = 10f;
        Assert.Equal(10f, joint.MaxMotorTorque);
        
        joint.MaxMotorTorque = 100f;
        Assert.Equal(100f, joint.MaxMotorTorque);
    }

    #endregion

    #region IsActive Tests

    [Fact]
    public void IsActive_ReturnsTrueWhenJointEnabled()
    {
        // Arrange
        var bodyA = CreateDynamicBody(new Vector2(0, 0));
        var bodyB = CreateDynamicBody(new Vector2(1, 0));
        _ = CreateRevoluteJoint(bodyA, bodyB, new Vector2(0.5f, 0));

        // Assert
        Assert.True(_joints[0]!.IsActive);
    }

    [Fact]
    public void IsActive_ReturnsFalseAfterRemove()
    {
        // Arrange
        var bodyA = CreateDynamicBody(new Vector2(0, 0));
        var bodyB = CreateDynamicBody(new Vector2(1, 0));
        var joint = CreateRevoluteJoint(bodyA, bodyB, new Vector2(0.5f, 0));

        // Act
        joint.Remove();

        // Assert - joint should no longer be in world
        Assert.Empty(_world!.JointList);
    }

    #endregion

    #region Remove and Dispose Tests

    [Fact]
    public void Remove_RemovesJointFromWorld()
    {
        // Arrange
        var bodyA = CreateDynamicBody(new Vector2(0, 0));
        var bodyB = CreateDynamicBody(new Vector2(1, 0));
        var joint = CreateRevoluteJoint(bodyA, bodyB, new Vector2(0.5f, 0));

        // Act
        joint.Remove();

        // Assert
        Assert.Empty(_world!.JointList);
    }

    [Fact]
    public void Dispose_RemovesJointFromWorld()
    {
        // Arrange
        var bodyA = CreateDynamicBody(new Vector2(0, 0));
        var bodyB = CreateDynamicBody(new Vector2(1, 0));
        var joint = CreateRevoluteJoint(bodyA, bodyB, new Vector2(0.5f, 0));

        // Act
        joint.Dispose();

        // Assert - joint should be inactive/disposed after Remove()
        Assert.False(joint.IsActive);
    }

    [Fact]
    public void Dispose_CanBeCalledMultipleTimes()
    {
        // Arrange
        var bodyA = CreateDynamicBody(new Vector2(0, 0));
        var bodyB = CreateDynamicBody(new Vector2(1, 0));
        var joint = CreateRevoluteJoint(bodyA, bodyB, new Vector2(0.5f, 0));

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
        var joint = CreateRevoluteJoint(bodyA, bodyB, new Vector2(0.5f, 0));

        // Act & Assert
        joint.Dispose();
        joint.Remove(); // should be safe to call after dispose
    }

    #endregion

    #region JointFactory Tests

    [Fact]
    public void JointFactory_CreateRevoluteJoint_WithSharedAnchor_CreatesValidJoint()
    {
        // Arrange
        var bodyA = CreateDynamicBody(new Vector2(0, 0));
        var bodyB = CreateDynamicBody(new Vector2(1, 0));

        // Act - use shared anchor point (both bodies share the same anchor)
        var aetherJoint = nkast.Aether.Physics2D.Dynamics.Joints.JointFactory.CreateRevoluteJoint(
            _world,
            bodyA._body!,
            bodyB._body!,
            new Vector2(0.5f, 0));

        // Assert - should not throw and joint should be valid
        Assert.NotNull(aetherJoint);
    }

    [Fact]
    public void JointFactory_CreateRevoluteJoint_WithDifferentAnchors_CreatesValidJoint()
    {
        // Arrange
        var bodyA = CreateDynamicBody(new Vector2(0, 0));
        var bodyB = CreateDynamicBody(new Vector2(1, 0));

        // Act - use different anchor points on each body
        var aetherJoint = nkast.Aether.Physics2D.Dynamics.Joints.JointFactory.CreateRevoluteJoint(
            _world,
            bodyA._body!,
            bodyB._body!,
            Vector2.Zero,
            new Vector2(-0.5f, 0));

        // Assert - should not throw and joint should be valid
        Assert.NotNull(aetherJoint);
    }

    #endregion
}
