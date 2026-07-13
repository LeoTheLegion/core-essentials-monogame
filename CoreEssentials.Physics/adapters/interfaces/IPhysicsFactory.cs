using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace CoreEssentials.Physics.Adapters.Interfaces;

/// <summary>
/// Interface for creating various physics entities.
/// This factory pattern allows for centralized creation logic and future engine swapping.
/// INTERNAL USE ONLY - Not exposed to users directly.
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
    /// Creates a circular spatial shape adapter with the specified radius.
    /// </summary>
    /// <param name="radius">The radius of the circle in world units.</param>
    /// <returns>The created circular shape adapter, or null if radius is invalid.</returns>
    ISpatialShapeAdapter CreateCircleShape(float radius);

    /// <summary>
    /// Creates a rectangular spatial shape adapter with the specified dimensions.
    /// </summary>
    /// <param name="width">The width of the rectangle along the local X axis.</param>
    /// <param name="height">The height of the rectangle along the local Y axis.</param>
    /// <returns>The created rectangular shape adapter, or null if dimensions are invalid.</returns>
    ISpatialShapeAdapter CreateRectangleShape(float width, float height);

    /// <summary>
    /// Creates a polygonal spatial shape adapter defined by vertices.
    /// The polygon must be convex and vertices should be in counter-clockwise order.
    /// </summary>
    /// <param name="vertices">Array of local-space vertices defining the polygon.</param>
    /// <returns>The created polygon shape adapter, or null if vertex count is invalid.</returns>
    ISpatialShapeAdapter CreatePolygonShape(Vector2[] vertices);

    /// <summary>
    /// Creates a convex hull spatial shape from multiple points.
    /// Automatically computes the smallest enclosing convex polygon.
    /// </summary>
    /// <param name="points">Array of points to compute the convex hull for.</param>
    /// <returns>The created convex hull shape adapter, or null if point count is invalid.</returns>
    ISpatialShapeAdapter CreateConvexHullShape(IEnumerable<Vector2> points);
}
