using System;
using System.Collections.Generic;
using CoreEssentials.Physics.Adapters.Interfaces;
using Microsoft.Xna.Framework;

namespace CoreEssentials.Physics.Adapters.Implementations.ShapeAdapters;

/// <summary>
/// Adapter that wraps Aether's CircleShape, implementing ISpatialShapeAdapter.
/// INTERNAL USE ONLY - Not exposed to users directly.
/// </summary>
internal class CircleShapeAdapter : ISpatialShapeAdapter
{
    private readonly nkast.Aether.Physics2D.Collision.Shapes.CircleShape _shape;

    /// <summary>
    /// Creates a new circle shape adapter wrapping the specified Aether CircleShape.
    /// </summary>
    public CircleShapeAdapter(nkast.Aether.Physics2D.Collision.Shapes.CircleShape shape)
    {
        _shape = shape ?? throw new ArgumentNullException(nameof(shape));
    }

    /// <inheritdoc />
    public ShapeType Type => ShapeType.Circle;

    /// <inheritdoc />
    /// CircleShape.Position is the local offset from body origin, which maps to our Center concept.
    public Vector2 Center => _shape.Position;

    /// <inheritdoc />
    /// Radius is inherited from base Shape class.
    public float Radius => _shape.Radius;

    /// <inheritdoc />
    public IEnumerable<Vector2> LocalVertices => Array.Empty<Vector2>();

    /// <inheritdoc />
    /// TestPoint requires a Transform - for simple point-in-shape checks, delegate to the shape's method.
    /// This implementation uses identity transform for local-space point tests.
    public bool ContainsPoint(Vector2 point)
    {
        var transform = new nkast.Aether.Physics2D.Common.Transform();
        return _shape.TestPoint(ref transform, ref point);
    }

    /// <summary>
    /// Gets the underlying Aether CircleShape instance.
    /// </summary>
    internal nkast.Aether.Physics2D.Collision.Shapes.CircleShape Shape => _shape;

    /// <inheritdoc />
    public static ISpatialShapeAdapter Create(ShapeType type)
    {
        if (type != ShapeType.Circle)
            throw new ArgumentException($"Cannot create CircleShapeAdapter for {type}", nameof(type));

        // CircleShape constructor requires radius and density.
        // Default values: radius=0, density=0 - will be set when attached to a fixture.
        return new CircleShapeAdapter(new nkast.Aether.Physics2D.Collision.Shapes.CircleShape(0f, 0f));
    }

    /// <inheritdoc />
    public void Dispose()
    {
        // Shapes don't implement IDisposable in Aether - they're managed by their owning Fixture.
    }
}
