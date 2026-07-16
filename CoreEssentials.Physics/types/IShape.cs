using Microsoft.Xna.Framework;

namespace CoreEssentials.Physics.Types;

/// <summary>
/// 🔒 Internal use only by PhysicsBody/Factory. Not exposed to end users.
/// </summary>
public interface IShape : IDisposable
{
    /// <summary>
    /// Gets the center of mass of this shape in local space.
    /// </summary>
    Vector2 Center { get; }

    /// <summary>
    /// Gets the radius of this shape (0 for non-circular shapes).
    /// </summary>
    float Radius { get; }

    /// <summary>
    /// Gets the vertices of this shape in local space.
    /// May be empty for circle shapes.
    /// </summary>
    IReadOnlyList<Vector2> Vertices { get; }

    // ─── Transform Operations ───────────────────────────────────────────

    /// <summary>
    /// Translates all vertices of this shape by the given offset.
    /// </summary>
    void Translate(Vector2 offset);

    /// <summary>
    /// Rotates all vertices of this shape around its center by the given angle in radians.
    /// </summary>
    void Rotate(float angleRadians);

    // ─── Query Methods ──────────────────────────────────────────────────

    /// <summary>
    /// Tests whether a point is contained within this shape (in local space).
    /// </summary>
    /// <param name="point">Point to test.</param>
    /// <returns>true if the point is inside or on the boundary of the shape.</returns>
    bool PointContains(Vector2 point);

    // ─── Type Identification ────────────────────────────────────────────

    /// <summary>
    /// Gets the type of this shape.
    /// </summary>
    ShapeType GetShapeType();
}

/// <summary>
/// Identifies the kind of physics shape.
/// 🔒 Internal use only.
/// </summary>
public enum ShapeType
{
    Unknown,
    Circle,
    Rectangle,
    Polygon,
    ConvexHull,
    LineSegment
}
