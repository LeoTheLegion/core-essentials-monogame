using Microsoft.Xna.Framework;

namespace CoreEssentials.Physics.Types;

/// <summary>
/// 🔒 Internal use only. Factory for creating physics worlds and bodies through interfaces.
/// </summary>
public interface IPhysicsFactory : IDisposable
{
    /// <summary>
    /// Creates a default physics world with zero gravity.
    /// </summary>
    IPhysicsWorld CreateDefault();

    /// <summary>
    /// Creates a physics world with the specified gravity vector.
    /// </summary>
    IPhysicsWorld CreateWithGravity(Vector2 gravity);

    /// <summary>
    /// Creates a physics world using the given configuration.
    /// </summary>
    IPhysicsWorld CreateWithConfig(SolverConfig config);

    // ─── Body Creation ──────────────────────────────────────────────────

    /// <summary>
    /// Creates a static body (immovable, infinite mass).
    /// </summary>
    IPhysicsBody CreateStatic(Vector2 position = default);

    /// <summary>
    /// Creates a dynamic body (affected by forces and collisions).
    /// </summary>
    /// <param name="position">Initial world position.</param>
    /// <param name="density">Density in kg/m² (used to calculate mass).</param>
    IPhysicsBody CreateDynamic(Vector2 position, float density = 1.0f);

    /// <summary>
    /// Creates a kinematic body (user-controlled, infinite mass for collisions).
    /// </summary>
    IPhysicsBody CreateKinematic(Vector2 position = default);

    // ─── Shape Factory Accessor ─────────────────────────────────────────

    /// <summary>
    /// Gets the shape factory for creating shapes.
    /// </summary>
    ISpatialShapeFactory Shapes { get; }
}

/// <summary>
/// 🔒 Internal use only. Factory for creating physics shapes through interfaces.
/// </summary>
public interface ISpatialShapeFactory : IDisposable
{
    /// <summary>
    /// Creates a circle shape.
    /// </summary>
    IShape CreateCircle(float radius);

    /// <summary>
    /// Creates a rectangle shape centered at origin.
    /// </summary>
    /// <param name="width">Width of the rectangle.</param>
    /// <param name="height">Height of the rectangle.</param>
    IShape CreateRectangle(float width, float height);

    /// <summary>
    /// Creates a polygon shape from vertices (must be in counter-clockwise order).
    /// </summary>
    IShape CreatePolygon(params Vector2[] vertices);

    /// <summary>
    /// Creates a convex hull shape from the given points.
    /// </summary>
    IShape CreateConvexHull(params Vector2[] points);
}
