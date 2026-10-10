using CoreEssentials.GameSystems.Physics.Types;
using Microsoft.Xna.Framework;
using AEPolygon = nkast.Aether.Physics2D.Collision.Shapes.PolygonShape;

namespace CoreEssentials.GameSystems.Physics.Engines.Aether.Shapes;

/// <summary>
/// 🔒 Implements IShape, wraps Aether PolygonShape with explicit vertices.
/// </summary>
public class PolygonShape : IShape
{
    internal AEPolygon _aetherShape;

    /// <summary>Density captured at construction so a rebuilt (transformed) polygon preserves its mass properties.</summary>
    private readonly float _density;
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
        _density = density;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PolygonShape"/> class from an existing Aether shape.
    /// </summary>
    internal PolygonShape(AEPolygon aetherShape, float density = 1f)
    {
        _aetherShape = aetherShape ?? throw new ArgumentNullException(nameof(aetherShape));
        _density = density;
    }

    #region IShape Properties

    /// <summary>
    /// Gets the center of mass in local space. Because Translate/Rotate bake directly into the
    /// geometry, this is the true baked centroid (the same value Aether uses for collision).
    /// </summary>
    public virtual Vector2 Center => _aetherShape.MassData.Centroid;

    /// <summary>
    /// Gets the bounding radius (small fixed value from Aether for polygon collision optimization).
    /// </summary>
    public virtual float Radius => _aetherShape.Radius;

    /// <summary>
    /// Returns the vertices of this polygon in local space. Transforms are baked into these, so they
    /// match what the underlying Aether fixture simulates with.
    /// </summary>
    public IReadOnlyList<Vector2> Vertices => _aetherShape.Vertices;

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

        // Bake the translation into the geometry so the underlying Aether fixture (and therefore the
        // simulation) matches what PointContains/Center report — not just a wrapper-side offset.
        var current = _aetherShape.Vertices;
        var updated = new Vector2[current.Count];
        for (int i = 0; i < current.Count; i++)
            updated[i] = current[i] + offset;
        Rebuild(updated);
    }

    /// <summary>
    /// Rotates the polygon around its center by accumulating a rotation angle (radians).
    /// </summary>
    public void Rotate(float angleRadians)
    {
        if (_disposed) return;

        // Bake the rotation about the current centroid into the geometry (same rationale as Translate).
        var center = _aetherShape.MassData.Centroid;
        var current = _aetherShape.Vertices;
        var cos = (float)Math.Cos(angleRadians);
        var sin = (float)Math.Sin(angleRadians);
        var updated = new Vector2[current.Count];
        for (int i = 0; i < current.Count; i++)
        {
            var d = current[i] - center;
            updated[i] = center + new Vector2(d.X * cos - d.Y * sin, d.X * sin + d.Y * cos);
        }
        Rebuild(updated);
    }

    /// <summary>
    /// Rebuilds the underlying Aether polygon from the supplied (already-baked) vertices, preserving density.
    /// </summary>
    private void Rebuild(IReadOnlyList<Vector2> vertices)
    {
        _aetherShape = new AEPolygon(new nkast.Aether.Physics2D.Common.Vertices(new List<Vector2>(vertices)), _density);
    }

    #endregion

    #region Query Methods

    /// <summary>
    /// Tests whether a point is contained within this polygon in local space.
    /// </summary>
    public virtual bool PointContains(Vector2 point)
    {
        if (_disposed) return false;

        // Geometry is baked in body-local space, so an incoming local point can be tested directly.
        return IsPointInAetherShape(point);
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

    #region Geometry Query

    // Translate/Rotate bake transforms directly into _aetherShape (see above), so the Aether geometry
    // is authoritative. No separate transform state is kept, and PointContains reads straight off it.

    /// <summary>
    /// Tests whether a point (in body-local space) is contained within this polygon's baked geometry.
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
