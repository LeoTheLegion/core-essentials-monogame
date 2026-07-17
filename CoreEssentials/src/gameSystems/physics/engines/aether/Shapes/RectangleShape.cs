using CoreEssentials.GameSystems.Physics.Types;
using Microsoft.Xna.Framework;
using AEPolygon = nkast.Aether.Physics2D.Collision.Shapes.PolygonShape;

namespace CoreEssentials.GameSystems.Physics.Engines.Aether.Shapes;

/// <summary>
/// 🔒 Implements IShape, wraps Aether PolygonShape constructed from rectangle dimensions.
/// Inherits from PolygonShape since rectangles are a special case of polygons.
/// </summary>
public class RectangleShape : PolygonShape
{
    private readonly Vector2 _halfSize;

    /// <summary>
    /// Initializes a new instance of the <see cref="RectangleShape"/> class.
    /// </summary>
    /// <param name="width">Width of the rectangle.</param>
    /// <param name="height">Height of the rectangle.</param>
    /// <param name="density">The density (mass per unit area) for mass calculations.</param>
    public RectangleShape(float width, float height, float density = 1f)
        : base(ComputeRectangleVertices(width, height), density)
    {
        if (width <= 0 || height <= 0)
            throw new ArgumentOutOfRangeException(width <= 0 ? nameof(width) : nameof(height), "Width and height must be positive.");

        _halfSize = new Vector2(width / 2f, height / 2f);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RectangleShape"/> class from an existing Aether shape.
    /// </summary>
    internal RectangleShape(AEPolygon aetherShape)
        : base(aetherShape)
    {
        // Compute half-size from vertices (assuming rectangle was created with centered vertices).
        var verts = new List<Vector2>(aetherShape.Vertices);
        if (verts.Count >= 4)
        {
            float maxX = verts.Max(v => Math.Abs(v.X));
            float maxY = verts.Max(v => Math.Abs(v.Y));
            _halfSize = new Vector2(maxX, maxY);
        }
    }

    #region IShape Overrides

    /// <summary>
    /// Gets the center of mass in local space (always origin for centered rectangles).
    /// </summary>
    public override Vector2 Center => Vector2.Zero;

    /// <summary>
    /// Gets the bounding radius (distance from center to corner).
    /// </summary>
    public override float Radius => _halfSize.Length();

    #endregion

    #region Query Methods Overrides

    /// <summary>
    /// Tests whether a point is contained within this rectangle in local space using AABB check.
    /// Faster than the generic polygon TestPoint for rectangles.
    /// </summary>
    public override bool PointContains(Vector2 point)
    {
        if (base.IsDisposed) return false;

        // Inverse-transform to undo offset/rotation, then AABB check on unrotated rectangle.
        var localPoint = ApplyInverseTransform(point);

        // For rotated rectangles, AABB bounds expand — use the rotated bounding box.
        float absCos = Math.Abs((float)Math.Cos(_localRotation));
        float absSin = Math.Abs((float)Math.Sin(_localRotation));

        // Transformed half extents after rotation
        float extX = _halfSize.X * absCos + _halfSize.Y * absSin;
        float extY = _halfSize.X * absSin + _halfSize.Y * absCos;

        return Math.Abs(localPoint.X - _localOffset.X) <= extX &&
               Math.Abs(localPoint.Y - _localOffset.Y) <= extY;
    }

    #endregion

    #region Type Identification Override

    /// <summary>
    /// Returns <see cref="ShapeType.Rectangle"/>.
    /// </summary>
    public override ShapeType GetShapeType() => Types.ShapeType.Rectangle;

    #endregion

    #region Internal Helpers

    /// <summary>
    /// Computes the four corner vertices centered at origin (counter-clockwise order).
    /// </summary>
    private static List<Vector2> ComputeRectangleVertices(float width, float height)
    {
        var halfW = width / 2f;
        var halfH = height / 2f;
        return new List<Vector2>
        {
            new(-halfW, -halfH), // bottom-left
            new(-halfW, halfH),  // top-left
            new(halfW, halfH),   // top-right
            new(halfW, -halfH)   // bottom-right
        };
    }

    #endregion
}
