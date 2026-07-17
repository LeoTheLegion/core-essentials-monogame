using CoreEssentials.GameSystems.Physics.Types;
using Microsoft.Xna.Framework;
using AECircle = nkast.Aether.Physics2D.Collision.Shapes.CircleShape;

namespace CoreEssentials.GameSystems.Physics.Engines.Aether.Shapes;

/// <summary>
/// 🔒 Implements IShape, wraps Aether CircleShape.
/// </summary>
public class CircleShape : IShape
{
    internal AECircle _aetherShape;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="CircleShape"/> class.
    /// </summary>
    /// <param name="radius">The radius of the circle.</param>
    /// <param name="density">The density (mass per unit area) for mass calculations.</param>
    public CircleShape(float radius, float density = 1f)
    {
        _aetherShape = new AECircle(radius, density);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CircleShape"/> class from an existing Aether shape.
    /// </summary>
    internal CircleShape(AECircle aetherShape)
    {
        _aetherShape = aetherShape ?? throw new ArgumentNullException(nameof(aetherShape));
    }

    #region IShape Properties

    /// <summary>
    /// Gets the center of mass in local space. For circles, this is the Position offset.
    /// </summary>
    public Vector2 Center => _aetherShape.Position;

    /// <summary>
    /// Gets the radius of the circle from the underlying Aether shape.
    /// </summary>
    public float Radius => _aetherShape.Radius;

    /// <summary>
    /// Returns a single vertex at the circle's center point in local space.
    /// </summary>
    public IReadOnlyList<Vector2> Vertices
    {
        get
        {
            // Circles have a single "vertex" which is their center offset.
            return new[] { _aetherShape.Position };
        }
    }

    #endregion

    #region Transform Operations

    /// <summary>
    /// Translates the circle by modifying its Position offset in local space.
    /// </summary>
    public void Translate(Vector2 offset)
    {
        if (_disposed) return;
        _aetherShape.Position = _aetherShape.Position + offset;
    }

    /// <summary>
    /// Circles are rotationally symmetric — rotation has no visual effect.
    /// This method is a no-op but satisfies the IShape interface contract.
    /// </summary>
    public void Rotate(float angleRadians)
    {
        // No-op: circles look identical at any rotation.
    }

    #endregion

    #region Query Methods

    /// <summary>
    /// Tests whether a point is contained within this circle in local space.
    /// </summary>
    public bool PointContains(Vector2 point)
    {
        if (_disposed) return false;
        var diff = point - _aetherShape.Position;
        return diff.LengthSquared() <= Radius * Radius;
    }

    #endregion

    #region Type Identification

    /// <summary>
    /// Returns <see cref="ShapeType.Circle"/>.
    /// </summary>
    public ShapeType GetShapeType() => Types.ShapeType.Circle;

    #endregion

    #region IDisposable

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        // Aether's CircleShape does not implement IDisposable — no cleanup needed.
    }

    #endregion
}
