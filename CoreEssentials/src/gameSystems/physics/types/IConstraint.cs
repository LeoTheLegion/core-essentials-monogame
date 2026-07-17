using Microsoft.Xna.Framework;

namespace CoreEssentials.GameSystems.Physics.Types;

/// <summary>
/// 🔒 Internal use only by Factory. Represents a constraint/joint between two bodies.
/// </summary>
public interface IConstraint : IDisposable
{
    /// <summary>
    /// Gets the first body connected by this constraint.
    /// </summary>
    IPhysicsBody BodyA { get; }

    /// <summary>
    /// Gets the second body connected by this constraint (may be null for single-body constraints).
    /// </summary>
    IPhysicsBody? BodyB { get; }

    /// <summary>
    /// Gets whether this constraint is currently active.
    /// </summary>
    bool IsActive { get; }

    /// <summary>
    /// Applies the constraint forces for this simulation step.
    /// Called internally by the physics engine during Step().
    /// </summary>
    void Apply();

    /// <summary>
    /// Removes this constraint from the world. The bodies remain unaffected.
    /// </summary>
    void Remove();
}

/// <summary>
/// 🔒 Internal use only. A revolute (hinge) joint connecting two bodies at a pivot point.
/// Allows rotation around a single axis with optional angle limits.
/// </summary>
public interface IRevoluteJoint : IConstraint
{
    /// <summary>
    /// Gets the local anchor point on body A (relative to body's origin).
    /// </summary>
    Vector2 LocalAnchorA { get; }

    /// <summary>
    /// Gets the local anchor point on body B (relative to body's origin).
    /// </summary>
    Vector2 LocalAnchorB { get; }

    /// <summary>
    /// Gets or sets the minimum angle limit in radians (-Infinity for no limit).
    /// </summary>
    float MinAngle { get; set; }

    /// <summary>
    /// Gets or sets the maximum angle limit in radians (Infinity for no limit).
    /// </summary>
    float MaxAngle { get; set; }

    /// <summary>
    /// Gets or sets whether the motor is enabled.
    /// </summary>
    bool MotorEnabled { get; set; }

    /// <summary>
    /// Gets or sets the target motor speed in radians per second.
    /// </summary>
    float MotorSpeed { get; set; }

    /// <summary>
    /// Gets or sets the maximum motor torque.
    /// </summary>
    float MaxMotorTorque { get; set; }
}

/// <summary>
/// 🔒 Internal use only. A weld joint that fixes the relative orientation and position of two bodies.
/// Acts like a rigid connection between bodies.
/// </summary>
public interface IWeldJoint : IConstraint
{
    /// <summary>
    /// Gets the local anchor point on body A (relative to body's origin).
    /// </summary>
    Vector2 LocalAnchorA { get; }

    /// <summary>
    /// Gets the local anchor point on body B (relative to body's origin).
    /// </summary>
    Vector2 LocalAnchorB { get; }

    /// <summary>
    /// Gets or sets whether bodies should collide when connected by this joint.
    /// </summary>
    bool CollideConnected { get; set; }

    /// <summary>
    /// Gets or sets the stiffness (0 = very soft, 1 = rigid). Default: 1.
    /// </summary>
    float Stiffness { get; set; }

    /// <summary>
    /// Gets or sets the damping (0 = no damping, 1 = fully damped). Default: 0.
    /// </summary>
    float Damping { get; set; }
}

/// <summary>
/// 🔒 Internal use only. A distance joint that maintains a fixed distance between two points on different bodies.
/// Can act as a spring when frequency/damping are configured.
/// </summary>
public interface IDistanceJoint : IConstraint
{
    /// <summary>
    /// Gets the local anchor point on body A (relative to body's origin).
    /// </summary>
    Vector2 LocalAnchorA { get; }

    /// <summary>
    /// Gets the local anchor point on body B (relative to body's origin).
    /// </summary>
    Vector2 LocalAnchorB { get; }

    /// <summary>
    /// Gets or sets the rest length of this joint.
    /// </summary>
    float Length { get; set; }

    /// <summary>
    /// Gets or sets the maximum force the joint can apply (for spring behavior).
    /// </summary>
    float MaxForce { get; set; }

    /// <summary>
    /// Gets or sets whether bodies should collide when connected by this joint.
    /// </summary>
    bool CollideConnected { get; set; }

    /// <summary>
    /// Gets or sets the spring frequency in Hertz (0 = rigid, >0 = springy).
    /// </summary>
    float FrequencyHz { get; set; }

    /// <summary>
    /// Gets or sets the spring damping ratio (0 = oscillates forever, 1 = critically damped).
    /// </summary>
    float DampingRatio { get; set; }
}

