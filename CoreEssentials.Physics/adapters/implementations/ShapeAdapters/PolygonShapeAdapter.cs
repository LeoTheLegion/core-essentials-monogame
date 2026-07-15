using System;
using System.Collections.Generic;
using CoreEssentials.Physics.Adapters.Interfaces;
using Microsoft.Xna.Framework;

namespace CoreEssentials.Physics.Adapters.Implementations.ShapeAdapters;

/// <summary>
/// Adapter that wraps Aether's PolygonShape, implementing ISpatialShapeAdapter.
/// Used for convex polygons and rectangles in the physics simulation.
/// INTERNAL USE ONLY - Not exposed to users directly.
/// </summary>
internal class PolygonShapeAdapter : ISpatialShapeAdapter
{
    private readonly nkast.Aether.Physics2D.Collision.Shapes.PolygonShape _shape;

    /// <summary>
    /// Creates a new polygon shape adapter wrapping the specified Aether PolygonShape.
    /// </summary>
    public PolygonShapeAdapter(nkast.Aether.Physics2D.Collision.Shapes.PolygonShape shape)
    {
        _shape = shape ?? throw new ArgumentNullException(nameof(shape));
    }

    /// <inheritdoc />
    public ShapeType Type => ShapeType.Polygon;

    /// <inheritdoc />
    /// Returns the centroid of the polygon computed during property calculation.
    public Vector2 Center => _shape.MassData.Centroid;

    /// <inheritdoc />
    /// Radius in Aether's PolygonShape is a skin thickness (0.01f) for collision detection.
    public float Radius => _shape.Radius;

    /// <inheritdoc />
    public IEnumerable<Vector2> LocalVertices => _shape.Vertices;

    /// <inheritdoc />
    public bool ContainsPoint(Vector2 point)
    {
        var transform = new nkast.Aether.Physics2D.Common.Transform();
        return _shape.TestPoint(ref transform, ref point);
    }

    /// <summary>
    /// Gets the underlying Aether PolygonShape instance.
    /// </summary>
    internal nkast.Aether.Physics2D.Collision.Shapes.PolygonShape Shape => _shape;

    /// <inheritdoc />
    public static ISpatialShapeAdapter Create(ShapeType type)
    {
        if (type != ShapeType.Polygon && type != ShapeType.ConvexHull)
            throw new ArgumentException($"Cannot create PolygonShapeAdapter for {type}", nameof(type));

        // Create with empty vertices. Actual vertices will be set when attached to a fixture/body.
        return new PolygonShapeAdapter(new nkast.Aether.Physics2D.Collision.Shapes.PolygonShape(0f));
    }

    /// <inheritdoc />
    public void Dispose()
    {
        // Shapes don't implement IDisposable in Aether - they're managed by their owning Fixture.
    }
}
