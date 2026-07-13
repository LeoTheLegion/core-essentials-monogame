using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using nkast.Aether.Physics2D.Dynamics;

namespace CoreEssentials.Physics.Adapters.Interfaces;

/// <summary>
/// Interface for creating various physics entities.
/// This factory pattern allows for centralized creation logic and future engine swapping.
/// </summary>
public interface IPhysicsFactory : IDisposable
{
    /// <summary>
    /// Gets the physics world used by this factory.
    /// </summary>
    IPhysicsWorldAdapter World { get; }

    /// <summary>
    /// Creates a new static body at the specified position and rotation.
    /// Static bodies do not respond to forces or collisions.
    /// </summary>
    IPhysicsBodyAdapter CreateStaticBody(Vector2 position, float rotation);

    /// <summary>
    /// Creates a new dynamic body at the specified position and rotation.
    /// Dynamic bodies respond to forces and participate in collision detection.
    /// </summary>
    IPhysicsBodyAdapter CreateDynamicBody(Vector2 position, float rotation);

    /// <summary>
    /// Creates a new kinematic body (moves without being affected by physics).
    /// Kinematic bodies move deterministically without being influenced by forces.
    /// </summary>
    IPhysicsBodyAdapter CreateKinematicBody(Vector2 position, float rotation);

    /// <summary>
    /// Creates a revolute joint between two bodies.
    /// </summary>
    IConstraintAdapter CreateRevoluteJoint(
        IPhysicsBodyAdapter bodyA,
        Vector2 anchorA,
        IPhysicsBodyAdapter bodyB = null);

    /// <summary>
    /// Creates a distance constraint between two points on different bodies.
    /// </summary>
    IConstraintAdapter CreateDistanceConstraint(
        IPhysicsBodyAdapter bodyA,
        Vector2 anchorA,
        IPhysicsBodyAdapter bodyB,
        Vector2 anchorB);

    /// <summary>
    /// Creates a circular spatial shape adapter.
    /// </summary>
    ISpatialShapeAdapter CreateCircleShape(float radius);

    /// <summary>
    /// Creates a rectangular spatial shape adapter.
    /// </summary>
    ISpatialShapeAdapter CreateRectangleShape(float width, float height);
}
