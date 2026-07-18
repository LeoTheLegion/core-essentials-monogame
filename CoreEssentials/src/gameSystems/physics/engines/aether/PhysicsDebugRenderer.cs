using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using CoreEssentials.Debugging;
using CoreEssentials.GameSystems.Physics.Types;

namespace CoreEssentials.GameSystems.Physics.Engines.Aether;

/// <summary>
/// Debug renderer for physics bodies that visualizes physics shapes and colliders.
/// Provides visual representation of the physics entities in the game world using our abstraction layer.
/// </summary>
public class PhysicsDebugRenderer : GameSystem, IPhysicsDebugRenderer, IDisposable
{
    private readonly IPhysicsWorld _world;
    private bool _drawDebug = false;

    /// <summary>
    /// Initializes a new instance of the PhysicsDebugRenderer class.
    /// </summary>
    /// <param name="world">The physics world containing the bodies to debug render.</param>
    public PhysicsDebugRenderer(IPhysicsWorld world)
    {
        _world = world ?? throw new ArgumentNullException(nameof(world));
    }

    private bool _disposed;

    #region IDisposable

    /// <inheritdoc />
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
    }

    #endregion

    /// <inheritdoc />
    public bool IsEnabled 
    { 
        get => _drawDebug; 
        set => _drawDebug = value; 
    }

    /// <summary>
    /// Draws debug visualizations for all physics bodies.
    /// </summary>
    /// <param name="spriteBatch">The SpriteBatch used for drawing.</param>
    public void Draw(SpriteBatch spriteBatch)
    {
        if (!_drawDebug)
            return;

        spriteBatch.Begin();

        foreach (var body in _world.Bodies)
        {
            var position = body.WorldPosition;
            var rotation = body.Rotation;

            foreach (var fixture in body.Colliders)
            {
                if (!fixture.IsActive)
                    continue;

                DrawFixture(spriteBatch, fixture, position, rotation);
            }
        }

        spriteBatch.End();
    }

    /// <summary>
    /// Draws a physics fixture based on its shape type.
    /// </summary>
    private void DrawFixture(SpriteBatch spriteBatch, ICollider fixture, Vector2 position, float rotation)
    {
        var shape = fixture.Shape;
        if (shape == null)
            return;

        switch (shape.GetShapeType())
        {
            case ShapeType.Polygon:
            case ShapeType.Rectangle:
                DrawPolygon(spriteBatch, shape, position, rotation);
                break;
            case ShapeType.Circle:
                DrawCircle(spriteBatch, shape, position, rotation);
                break;
        }
    }

    /// <summary>
    /// Draws a circular physics shape.
    /// </summary>
    private void DrawCircle(SpriteBatch spriteBatch, IShape shape, Vector2 bodyPosition, float rotation)
    {
        var center = bodyPosition + RotateVector(shape.Center, rotation);
        var radius = shape.Radius;

        // Apply world scale (default 16x for pixel conversion)
        const float scale = 16f;
        center *= scale;
        radius *= scale;

        Debug.Primitives.DrawCircle(spriteBatch, center, radius, Color.Green, 16);
    }

    /// <summary>
    /// Draws a polygon or rectangle physics shape.
    /// </summary>
    private void DrawPolygon(SpriteBatch spriteBatch, IShape shape, Vector2 bodyPosition, float rotation)
    {
        var vertices = shape.Vertices;

        if (vertices.Count < 3)
            return;

        // Transform and scale all vertices
        var transformedVertices = new Vector2[vertices.Count];
        for (int i = 0; i < vertices.Count; i++)
        {
            transformedVertices[i] = bodyPosition + RotateVector(vertices[i], rotation);
        }

        float f_scale = 16f; // Default scale - should be configurable

        for (int j = 0; j < vertices.Count; j++)
        {
            var v1 = transformedVertices[j] * f_scale;
            var v2 = transformedVertices[(j + 1) % vertices.Count] * f_scale;

            Debug.Primitives.DrawLine(spriteBatch, v1, v2, Color.Green, 2);
        }
    }

    /// <summary>
    /// Rotates a vector by the given angle in radians.
    /// </summary>
    private static Vector2 RotateVector(Vector2 vector, float angle)
    {
        float cos = MathF.Cos(angle);
        float sin = MathF.Sin(angle);
        return new Vector2(
            vector.X * cos - vector.Y * sin,
            vector.X * sin + vector.Y * cos
        );
    }
}
