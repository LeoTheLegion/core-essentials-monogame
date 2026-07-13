using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace CoreEssentials.Physics.Adapters.Interfaces;

/// <summary>
/// Enum representing the type of spatial shape used in collision detection.
/// </summary>
public enum ShapeType
{
    /// <summary>Circular shape.</summary>
    Circle,

    /// <summary>Rectangular shape aligned with local axes.</summary>
    Rectangle,

    /// <summary>Polygonal shape defined by vertices.</summary>
    Polygon,

    /// <summary>Convex hull of multiple points.</summary>
    ConvexHull,

    /// <summary>Line segment (used for raycasting).</summary>
    LineSegment,

    /// <summary>Unknown or invalid shape type.</summary>
    Unknown
}

/// <summary>
/// Interface representing a spatial shape used in collision detection.
/// This abstracts away the underlying Aether Physics2D shape implementations,
/// allowing for future engine swapping without breaking user code.
/// </summary>
public interface ISpatialShapeAdapter : IDisposable
{
    /// <summary>
    /// Gets the type of this shape (Circle, Rectangle, Polygon, etc.).
    /// </summary>
    ShapeType Type { get; }

    /// <summary>
    /// Gets the center point of the shape in local space.
    /// For Circle and Polygon shapes, this is typically the geometric center.
    /// Empty for LineSegment and Unknown types.
    /// </summary>
    Vector2 Center { get; }

    /// <summary>
    /// Gets the radius of the shape.
    /// Applicable for Circle (exact radius), Rectangle and Polygon (approximate bounding radius).
    /// For LineSegment, this is half the segment length.
    /// Returns 0 or negative for Unknown/invalid shapes.
    /// </summary>
    float Radius { get; }

    /// <summary>
    /// Checks if a point is inside this shape.
    /// </summary>
    /// <param name="point">The world-space point to check.</param>
    /// <returns>True if the point is inside the shape, false otherwise.</returns>
    bool ContainsPoint(Vector2 point);

    /// <summary>
    /// Gets the local vertices of a polygon/convex hull shape.
    /// Returns empty collection for non-polygon shapes.
    /// </summary>
    IEnumerable<Vector2> LocalVertices { get; }

    /// <summary>
    /// Creates a new spatial shape adapter based on the specified type.
    /// This is called internally by factory classes and should not be 
    /// implemented directly by user code.
    /// </summary>
    /// <param name="type">The type of shape to create.</param>
    /// <returns>A new instance of the spatial shape adapter.</returns>
    static abstract ISpatialShapeAdapter Create(ShapeType type);
}
