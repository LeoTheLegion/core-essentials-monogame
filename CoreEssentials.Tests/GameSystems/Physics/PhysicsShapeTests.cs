using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Xunit;
using CoreEssentials.GameSystems.Physics.Engines.Aether.Shapes;
using CoreEssentials.GameSystems.Physics.Types;

namespace CoreEssentials.Tests.GameSystems.Physics;

public class PhysicsShapeTests
{
    private static List<Vector2> UnitTriangle() => new()
    {
        new(0, 0),
        new(2, 0),
        new(1, 2),
    };

    // ---------- Polygon ----------

    [Fact]
    public void Polygon_CtorStoresVertices()
    {
        var shape = new PolygonShape(UnitTriangle());
        Assert.Equal(3, shape.Vertices.Count);
    }

    [Fact]
    public void Polygon_Ctor_NullThrows()
    {
        Assert.Throws<ArgumentNullException>(() => new PolygonShape((IEnumerable<Vector2>)null!));
    }

    [Fact]
    public void Polygon_Ctor_TooFewVerticesThrows()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new PolygonShape(new[] { new Vector2(0, 0), new Vector2(1, 0) }));
    }

    [Fact]
    public void Polygon_GetShapeType_ReturnsPolygon()
    {
        var shape = new PolygonShape(UnitTriangle());
        Assert.Equal(ShapeType.Polygon, shape.GetShapeType());
    }

    [Fact]
    public void Polygon_Center_IsFinite()
    {
        var shape = new PolygonShape(UnitTriangle());
        var c = shape.Center;
        Assert.True(float.IsFinite(c.X) && float.IsFinite(c.Y));
    }

    [Fact]
    public void Polygon_Radius_IsPositive()
    {
        var shape = new PolygonShape(UnitTriangle());
        Assert.True(shape.Radius > 0f);
    }

    [Fact]
    public void Polygon_PointContains_CoversInteriorRejectsExterior()
    {
        var shape = new PolygonShape(UnitTriangle());
        Assert.True(shape.PointContains(new Vector2(1f, 0.5f)));   // inside triangle
        Assert.False(shape.PointContains(new Vector2(5f, 5f)));     // well outside
    }

    [Fact]
    public void Polygon_Translate_ShiftsCenter()
    {
        var shape = new PolygonShape(UnitTriangle());
        var before = shape.Center;
        shape.Translate(new Vector2(3f, 4f));
        var after = shape.Center;
        Assert.InRange(after.X - before.X, 2.99f, 3.01f);
        Assert.InRange(after.Y - before.Y, 3.99f, 4.01f);
    }

    [Fact]
    public void Polygon_Rotate_KeepsPointInside()
    {
        var shape = new PolygonShape(UnitTriangle());
        shape.Rotate((float)(Math.PI / 2));
        // The centroid should still contain a point near the (rotated) center.
        Assert.True(shape.PointContains(shape.Center + new Vector2(0.1f, 0.1f)) ||
                    shape.PointContains(shape.Center - new Vector2(0.1f, 0.1f)));
    }

    [Fact]
    public void Polygon_Dispose_MakesPointContainsFalse()
    {
        var shape = new PolygonShape(UnitTriangle());
        Assert.True(shape.PointContains(new Vector2(1f, 0.5f)));
        shape.Dispose();
        Assert.False(shape.PointContains(new Vector2(1f, 0.5f)));
    }

    [Fact]
    public void Polygon_Dispose_IgnoresTranslateRotate()
    {
        var shape = new PolygonShape(UnitTriangle());
        shape.Dispose();
        // Should not throw and should be a no-op.
        shape.Translate(new Vector2(1, 1));
        shape.Rotate(0.5f);
        Assert.True(true);
    }

    [Fact]
    public void Polygon_CreateConvexHull_NullThrows()
    {
        Assert.Throws<ArgumentNullException>(() => PolygonShape.CreateConvexHull((IEnumerable<Vector2>)null!));
    }

    [Fact]
    public void Polygon_CreateConvexHull_TooFewPointsThrows()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            PolygonShape.CreateConvexHull(new[] { new Vector2(0, 0), new Vector2(1, 0) }));
    }

    [Fact]
    public void Polygon_CreateConvexHull_ProducesValidShape()
    {
        var points = new List<Vector2>
        {
            new(0, 0), new(4, 0), new(4, 3), new(2, 1), new(0, 3)
        };
        var hull = PolygonShape.CreateConvexHull(points);
        Assert.True(hull.Vertices.Count >= 3);
        Assert.Equal(ShapeType.Polygon, hull.GetShapeType());
    }

    // ---------- Rectangle ----------

    [Fact]
    public void Rectangle_CtorSetsDimensions()
    {
        var shape = new RectangleShape(4f, 2f);
        Assert.Equal(4, shape.Vertices.Count);
        // Bounding radius = length of half-size (2,1) = sqrt(5).
        Assert.InRange(shape.Radius, MathF.Sqrt(5f) - 0.01f, MathF.Sqrt(5f) + 0.01f);
    }

    [Fact]
    public void Rectangle_Ctor_NonPositiveWidthThrows()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new RectangleShape(0f, 2f));
    }

    [Fact]
    public void Rectangle_Ctor_NonPositiveHeightThrows()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new RectangleShape(4f, -1f));
    }

    [Fact]
    public void Rectangle_GetShapeType_ReturnsRectangle()
    {
        var shape = new RectangleShape(2f, 2f);
        Assert.Equal(ShapeType.Rectangle, shape.GetShapeType());
    }

    [Fact]
    public void Rectangle_Center_IsOriginForCenteredRect()
    {
        var shape = new RectangleShape(4f, 2f);
        Assert.InRange(shape.Center.X, -0.001f, 0.001f);
        Assert.InRange(shape.Center.Y, -0.001f, 0.001f);
    }

    [Fact]
    public void Rectangle_PointContains_UsesAabb()
    {
        var shape = new RectangleShape(4f, 2f);
        Assert.True(shape.PointContains(new Vector2(1.5f, 0.5f)));   // inside
        Assert.False(shape.PointContains(new Vector2(3f, 1f)));      // outside (x > 2)
    }

    [Fact]
    public void Rectangle_PointContains_RespectsTranslationOffset()
    {
        var shape = new RectangleShape(4f, 2f); // half-extents (2,1) centered at origin
        shape.Translate(new Vector2(10f, 0f));   // move so it's centered on (10, 0)

        Assert.True(shape.PointContains(new Vector2(10f, 0f)));            // center still inside
        Assert.True(shape.PointContains(new Vector2(11.99f, 0f)));         // just within x half-extent
        Assert.False(shape.PointContains(new Vector2(12.01f, 0f)));        // just beyond x half-extent
        Assert.False(shape.PointContains(Vector2.Zero));                   // old origin no longer inside
    }

    [Fact]
    public void Rectangle_PointContains_RespectsRotation()
    {
        var shape = new RectangleShape(4f, 2f); // half-extents (2,1), centered at origin
        shape.Rotate((float)Math.PI / 4f);      // 45 deg: local-space test must still hold

        Assert.True(shape.PointContains(new Vector2(0f, 0f)));             // center always inside
        Assert.True(shape.PointContains(new Vector2(1.4f, 1.4f)));         // maps to local (~1.98,0): within x-half 2
        Assert.False(shape.PointContains(new Vector2(2.6f, 2.6f)));        // maps to local (~3.68,0): beyond x-half 2
        Assert.True(shape.PointContains(new Vector2(0f, 0.5f)));           // maps to local (~0.35,-0.35): inside
        Assert.False(shape.PointContains(new Vector2(0f, 1.9f)));          // maps to local (~1.34,-1.34): beyond y-half 1
    }

    [Fact]
    public void Rectangle_Center_ReflectsTranslationOffset()
    {
        var shape = new RectangleShape(4f, 2f);
        Assert.InRange(shape.Center.X, -0.001f, 0.001f); // untransformed center is the origin

        shape.Translate(new Vector2(5f, -3f));
        Assert.InRange(shape.Center.X, 4.99f, 5.01f);
        Assert.InRange(shape.Center.Y, -3.01f, -2.99f);
    }

    [Fact]
    public void Rectangle_Dispose_MakesPointContainsFalse()
    {
        var shape = new RectangleShape(4f, 2f);
        shape.Dispose();
        Assert.False(shape.PointContains(Vector2.Zero));
    }

    // ---------- Circle ----------

    [Fact]
    public void Circle_CtorSetsRadius()
    {
        var shape = new CircleShape(3f);
        Assert.InRange(shape.Radius, 2.99f, 3.01f);
    }

    [Fact]
    public void Circle_GetShapeType_ReturnsCircle()
    {
        var shape = new CircleShape(1f);
        Assert.Equal(ShapeType.Circle, shape.GetShapeType());
    }

    [Fact]
    public void Circle_Vertices_IsSingleCenterPoint()
    {
        var shape = new CircleShape(2f);
        Assert.Single(shape.Vertices);
    }

    [Fact]
    public void Circle_Center_StartsAtOrigin()
    {
        var shape = new CircleShape(2f);
        Assert.InRange(shape.Center.X, -0.001f, 0.001f);
        Assert.InRange(shape.Center.Y, -0.001f, 0.001f);
    }

    [Fact]
    public void Circle_PointContains_UsesRadius()
    {
        var shape = new CircleShape(2f);
        Assert.True(shape.PointContains(new Vector2(1f, 0f)));   // inside
        Assert.False(shape.PointContains(new Vector2(3f, 0f)));  // outside (dist 3 > r 2)
    }

    [Fact]
    public void Circle_Translate_ShiftsContainment()
    {
        var shape = new CircleShape(1f);
        shape.Translate(new Vector2(5f, 0f));
        Assert.True(shape.PointContains(new Vector2(5f, 0.5f)));   // around new center
        Assert.False(shape.PointContains(new Vector2(0f, 0f)));    // old center no longer inside
    }

    [Fact]
    public void Circle_Rotate_IsNoOp()
    {
        var shape = new CircleShape(1f);
        shape.Rotate((float)(Math.PI / 4));
        Assert.True(shape.PointContains(Vector2.Zero));            // circle symmetric, still contains center
    }

    [Fact]
    public void Circle_Dispose_MakesPointContainsFalse()
    {
        var shape = new CircleShape(1f);
        shape.Dispose();
        Assert.False(shape.PointContains(Vector2.Zero));
    }
}
