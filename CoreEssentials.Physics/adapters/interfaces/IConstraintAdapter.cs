using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using nkast.Aether.Physics2D.Dynamics;

namespace CoreEssentials.Physics.Adapters.Interfaces;

/// <summary>
/// Interface representing a physics constraint (joint) between bodies.
/// This abstracts away the underlying Aether Physics2D Constraint implementation,
/// allowing for future engine swapping without breaking user code.
/// </summary>
public interface IConstraintAdapter : IDisposable
{
    /// <summary>
    /// Gets or sets the anchor point in world space where the constraint is applied.
    /// </summary>
    Vector2 AnchorA { get; set; }

    /// <summary>
    /// Gets or sets the connected body.
    /// </summary>
    IPhysicsBodyAdapter ConnectedBody { get; set; }

    /// <summary>
    /// Creates a revolute joint (hinge) between two bodies at specified anchor points.
    /// </summary>
    static abstract IConstraintAdapter CreateRevoluteJoint(
        IPhysicsWorldAdapter world,
        IPhysicsBodyAdapter bodyA,
        Vector2 anchorA,
        IPhysicsBodyAdapter bodyB = null);

    /// <summary>
    /// Creates a distance constraint (spring) between two bodies.
    /// </summary>
    static abstract IConstraintAdapter CreateDistanceConstraint(
        IPhysicsWorldAdapter world,
        IPhysicsBodyAdapter bodyA,
        Vector2 anchorA,
        IPhysicsBodyAdapter bodyB,
        Vector2 anchorB);
}
