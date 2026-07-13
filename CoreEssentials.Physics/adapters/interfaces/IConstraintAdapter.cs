using System;
using Microsoft.Xna.Framework;

namespace CoreEssentials.Physics.Adapters.Interfaces;

/// <summary>
/// Base interface representing a physics constraint (joint) between bodies.
/// This abstracts away the underlying Aether Physics2D Constraint implementation,
/// allowing for future engine swapping without breaking user code.
/// </summary>
public interface IConstraintAdapter : IDisposable
{
    /// <summary>
    /// Gets or sets the anchor point in world space where the constraint is applied on body A.
    /// </summary>
    Vector2 AnchorA { get; set; }

    /// <summary>
    /// Gets or sets the connected body (for binary constraints).
    /// Null for unary constraints that attach a single body to ground.
    /// </summary>
    IPhysicsBodyAdapter ConnectedBody { get; set; }

    /// <summary>
    /// Gets or sets the anchor point on body A in local space relative to body's origin.
    /// </summary>
    Vector2 LocalAnchorA { get; set; }

    /// <summary>
    /// Gets or sets the anchor point on body B in world space (for binary constraints).
    /// Ignored for unary constraints where ConnectedBody is null.
    /// </summary>
    Vector2 AnchorB { get; set; }

    /// <summary>
    /// Gets or sets the anchor point on body B in local space (for binary constraints).
    /// </summary>
    Vector2 LocalAnchorB { get; set; }

    /// <summary>
    /// Gets a value indicating whether this constraint is currently active.
    /// </summary>
    bool IsActive { get; }
}

/// <summary>
/// Interface for revolute (hinge) joints between two bodies.
/// </summary>
public interface IRevoluteJointAdapter : IConstraintAdapter
{
    /// <summary>
    /// Gets or sets the maximum torque allowed before the joint breaks.
    /// Negative value means infinite torque (never breaks).
    /// </summary>
    float MaxTorque { get; set; }

    /// <summary>
    /// Enables or disables collision between the two bodies connected by this joint.
    /// </summary>
    void EnableCollision();

    /// <summary>
    /// Disables collision between the two bodies connected by this joint.
    /// </summary>
    void DisableCollision();

    /// <summary>
    /// Creates a revolute joint (hinge) between two bodies at specified anchor points.
    /// </summary>
    /// <param name="world">The physics world to create the joint in.</param>
    /// <param name="bodyA">The first body (anchor).</param>
    /// <param name="anchorA">World-space position of the hinge on body A.</param>
    /// <param name="bodyB">Optional second body. If null, creates a one-way hinge to ground.</param>
    /// <returns>The created revolute joint adapter.</returns>
    static abstract IRevoluteJointAdapter CreateRevoluteJoint(
        IPhysicsWorldAdapter world,
        IPhysicsBodyAdapter bodyA,
        Vector2 anchorA,
        IPhysicsBodyAdapter bodyB = null);

    /// <summary>
    /// Creates a distance constraint (spring-like) between two bodies.
    /// </summary>
    static abstract IRevoluteJointAdapter CreateDistanceConstraint(
        IPhysicsWorldAdapter world,
        IPhysicsBodyAdapter bodyA,
        Vector2 anchorA,
        IPhysicsBodyAdapter bodyB,
        Vector2 anchorB);
}
