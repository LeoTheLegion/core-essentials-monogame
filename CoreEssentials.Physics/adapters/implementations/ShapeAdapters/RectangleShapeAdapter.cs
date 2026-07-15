using System;
using System.Collections.Generic;
using CoreEssentials.Physics.Adapters.Interfaces;
using Microsoft.Xna.Framework;

namespace CoreEssentials.Physics.Adapters.Implementations.ShapeAdapters;

/// <summary>
/// Adapter that wraps Aether's PolygonShape as a rectangle, implementing ISpatialShapeAdapter.
/// In Aether Physics2D, rectangles are represented as a 4-vertex convex polygon.
/// INTERNAL USE ONLY - Not exposed to users directly.
/// </summary>
internal class RectangleShapeAdapter : ISpatialShapeAdapter
{
    private readonly nkast.Aether.Physics2D.Collision.Shapes.PolygonShape _shape;

    /// <summary>
    /// Creates a new rectangle shape adapter wrapping the specified Aether PolygonShape.
    /// </summary>
    public RectangleShapeAdapter(nkast.Aether.Physics2D.Collision.Shapes.PolygonShape shape)
    {
        _shape = shape ?? throw new ArgumentNullException(nameof(shape));
    }

    /// <inheritdoc />
    public ShapeType Type => ShapeType.Rectangle;

    /// <inheritdoc />
    /// For a rectangle centered at origin, the centroid is at Vector2.Zero.
    public Vector2 Center => _shape.MassData.Centroid;

    /// <inheritdoc />
    /// Radius in Aether's PolygonShape is set to 0.01f as a skin thickness for collision detection.
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
        if (type != ShapeType.Rectangle)
            throw new ArgumentException($"Cannot create RectangleShapeAdapter for {type}", nameof(type));

        // Create a 4-vertex polygon representing a unit rectangle centered at origin.
        // Default dimensions will be set when attached to a fixture.
        var vertices = new nkast.Aether.Physics2D.Common.Vertices(4);
        vertices.Add(new Vector2(-0.5f, -0.5f));
        vertices.Add(new Vector2(0.5f, -0.5f));
        vertices.Add(new Vector2(0.5f, 0.5f));
        vertices.Add(new Vector2(-0.5f, 0.5f));

        return new RectangleShapeAdapter(new nkast.Aether.Physics2D.Collision.Shapes.PolygonShape(vertices, 0f));
    }

    /// <inheritdoc />
    public void Dispose()
    {
        // Shapes don't implement IDisposable in Aether - they're managed by their owning Fixture.
    }
}
