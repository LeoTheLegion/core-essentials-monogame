using CoreEssentials.GameSystems.Physics.Types;
using Microsoft.Xna.Framework;
using AEPolygon = nkast.Aether.Physics2D.Collision.Shapes.PolygonShape;

namespace CoreEssentials.GameSystems.Physics.Engines.Aether.Shapes;

/// <summary>
/// 🔒 Implements IShape, wraps Aether PolygonShape with explicit vertices.
/// </summary>
public class PolygonShape : IShape
{
    internal readonly AEPolygon _aetherShape;

    /// <summary>The local space offset applied to this shape's position.</summary>
    protected Vector2 _localOffset = Vector2.Zero;

    /// <summary>The local space rotation (in radians) applied to this shape.</summary>
    protected float _localRotation = 0f;
    private bool _disposed;

    /// <summary>
    /// Gets whether this shape has been disposed.
    /// </summary>
    protected bool IsDisposed => _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="PolygonShape"/> class from explicit vertices.
    /// The vertices will be converted to a convex hull if needed by Aether's settings.
    /// </summary>
    /// <param name="vertices">The polygon vertices in local space.</param>
    /// <param name="density">The density (mass per unit area) for mass calculations.</param>
    public PolygonShape(IEnumerable<Vector2> vertices, float density = 1f)
    {
        if (vertices == null) throw new ArgumentNullException(nameof(vertices));

        var vertexList = vertices.ToList();
        if (vertexList.Count < 3)
            throw new ArgumentOutOfRangeException(nameof(vertices), "At least 3 vertices are required.");

        var aetherVertices = new nkast.Aether.Physics2D.Common.Vertices(vertexList);
        _aetherShape = new AEPolygon(aetherVertices, density);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PolygonShape"/> class from an existing Aether shape.
    /// </summary>
    internal PolygonShape(AEPolygon aetherShape)
    {
        _aetherShape = aetherShape ?? throw new ArgumentNullException(nameof(aetherShape));
    }

    #region IShape Properties

    /// <summary>
    /// Gets the center of mass in local space, adjusted for any transform offsets.
    /// </summary>
    public virtual Vector2 Center => GetTransformedCenter();

    /// <summary>
    /// Gets the bounding radius (small fixed value from Aether for polygon collision optimization).
    /// </summary>
    public virtual float Radius => _aetherShape.Radius;

    /// <summary>
    /// Returns the vertices of this polygon in local space, adjusted for any transform offsets.
    /// These may be re-ordered by Aether's convex hull computation.
    /// </summary>
    public IReadOnlyList<Vector2> Vertices => GetTransformedVertices();

    #endregion

    #region Internal Methods

    /// <summary>
    /// Creates a polygon shape from the convex hull of the given points, delegating to Aether's convex hull utility.
    /// </summary>
    /// <param name="points">The input points.</param>
    /// <param name="density">The density for mass calculations.</param>
    /// <returns>A new PolygonShape wrapping a convex hull created from the points.</returns>
    public static PolygonShape CreateConvexHull(IEnumerable<Vector2> points, float density = 1f)
    {
        if (points == null) throw new ArgumentNullException(nameof(points));

        var pointList = points.ToList();
        if (pointList.Count < 3)
            throw new ArgumentOutOfRangeException(nameof(points), "At least 3 points are required to create a convex hull.");

        // Aether's GiftWrap algorithm for convex hull computation.
        var hull = nkast.Aether.Physics2D.Common.ConvexHull.GiftWrap.GetConvexHull(new nkast.Aether.Physics2D.Common.Vertices(pointList));
        return new PolygonShape(hull, density);
    }

    #endregion

    #region Transform Operations

    /// <summary>
    /// Translates the polygon by accumulating an offset.
    /// </summary>
    public void Translate(Vector2 offset)
    {
        if (_disposed) return;
        _localOffset += offset;
    }

    /// <summary>
    /// Rotates the polygon around its center by accumulating a rotation angle (radians).
    /// </summary>
    public void Rotate(float angleRadians)
    {
        if (_disposed) return;
        _localRotation += angleRadians;
    }

    #endregion

    #region Query Methods

    /// <summary>
    /// Tests whether a point is contained within this polygon in local space.
    /// </summary>
    public virtual bool PointContains(Vector2 point)
    {
        if (_disposed) return false;

        // Transform the point into the shape's untransformed local space, then use Aether's TestPoint.
        var transformedPoint = ApplyInverseTransform(point);
        return IsPointInAetherShape(transformedPoint);
    }

    #endregion

    #region Type Identification

    /// <summary>
    /// Returns <see cref="ShapeType.Polygon"/>.
    /// </summary>
    public virtual ShapeType GetShapeType() => Types.ShapeType.Polygon;

    #endregion

    #region IDisposable

    /// <summary>
    /// Releases resources. Aether's PolygonShape does not implement IDisposable.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Disposes the instance. Called from <see cref="Dispose()"/> or when the finalizer runs.
    /// </summary>
    /// <param name="disposing">True if called from <see cref="Dispose()"/> (managed resources can be released); false if called from the finalizer.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (_disposed) return;
        // No managed resources to dispose (Aether's PolygonShape doesn't implement IDisposable).
        _disposed = true;
    }

    #endregion

    #region Transform Helpers (internal for Rectangle override)

    /// <summary>
    /// Applies the accumulated local offset and rotation to a point.
    /// </summary>
    protected Vector2 ApplyTransform(Vector2 point)
    {
        if (_localRotation == 0f && _localOffset == Vector2.Zero)
            return point;

        // Rotate around origin, then translate
        var rotated = new Vector2(
            point.X * (float)Math.Cos(_localRotation) - point.Y * (float)Math.Sin(_localRotation),
            point.X * (float)Math.Sin(_localRotation) + point.Y * (float)Math.Cos(_localRotation));
        return rotated + _localOffset;
    }

    /// <summary>
    /// Applies the inverse of the accumulated local offset and rotation to a point.
    /// </summary>
    protected Vector2 ApplyInverseTransform(Vector2 point)
    {
        if (_localRotation == 0f && _localOffset == Vector2.Zero)
            return point;

        // Inverse: translate back, then rotate backwards
        var translated = point - _localOffset;
        var rotated = new Vector2(
            translated.X * (float)Math.Cos(-_localRotation) - translated.Y * (float)Math.Sin(-_localRotation),
            translated.X * (float)Math.Sin(-_localRotation) + translated.Y * (float)Math.Cos(-_localRotation));
        return rotated;
    }

    /// <summary>
    /// Gets the center adjusted for local transform offsets.
    /// </summary>
    protected Vector2 GetTransformedCenter()
    {
        var baseCenter = _aetherShape.MassData.Centroid;
        if (_localRotation == 0f && _localOffset == Vector2.Zero)
            return baseCenter;

        // Rotate around origin, then translate
        var rotated = new Vector2(
            baseCenter.X * (float)Math.Cos(_localRotation) - baseCenter.Y * (float)Math.Sin(_localRotation),
            baseCenter.X * (float)Math.Sin(_localRotation) + baseCenter.Y * (float)Math.Cos(_localRotation));
        return rotated + _localOffset;
    }

    /// <summary>
    /// Gets the vertices adjusted for local transform offsets.
    /// </summary>
    protected IReadOnlyList<Vector2> GetTransformedVertices()
    {
        if (_localRotation == 0f && _localOffset == Vector2.Zero)
            return _aetherShape.Vertices;

        var transformed = new Vector2[_aetherShape.Vertices.Count];
        for (int i = 0; i < _aetherShape.Vertices.Count; i++)
        {
            var v = _aetherShape.Vertices[i];
            // Rotate around origin, then translate
            var cos = (float)Math.Cos(_localRotation);
            var sin = (float)Math.Sin(_localRotation);
            transformed[i] = new Vector2(
                v.X * cos - v.Y * sin + _localOffset.X,
                v.X * sin + v.Y * cos + _localOffset.Y);
        }
        return transformed;
    }

    /// <summary>
    /// Tests whether a point (in the shape's untransformed local space) is contained within this polygon.
    /// </summary>
    protected bool IsPointInAetherShape(Vector2 point)
    {
        // Aether's TestPoint expects world-space or body-transform coordinates.
        // Since our shape has no body transform, we use the identity transform manually.

        for (int i = 0; i < _aetherShape.Vertices.Count; i++)
        {
            int next = (i + 1) % _aetherShape.Vertices.Count;
            Vector2 current = _aetherShape.Vertices[i];
            Vector2 nextVertex = _aetherShape.Vertices[next];

            // Edge normal pointing outward
            var edge = nextVertex - current;
            var normal = new Vector2(edge.Y, -edge.X); // outward normal for CCW polygon
            normal.Normalize();

            // If point is on the outside of this edge, it's not inside
            if (Vector2.Dot(normal, point - current) > 0f)
                return false;
        }
        return true;
    }

    #endregion
}
