using CoreEssentials.GameSystems.Physics.Engines.Aether.Shapes;
using CoreEssentials.GameSystems.Physics.Types;
using Microsoft.Xna.Framework;
using nkast.Aether.Physics2D.Collision.Shapes;
using nkast.Aether.Physics2D.Common;
using nkast.Aether.Physics2D.Dynamics;

namespace CoreEssentials.GameSystems.Physics.Engines.Aether;

/// <summary>
/// Implements IPhysicsBody, wraps Aether.Body.
/// </summary>
public class PhysicsBody : IPhysicsBody
{
    private readonly World _world;
    internal Body? _body; // Internal so PhysicsEngine can access it for removal
    private readonly List<ICollider> _colliders = new();
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="PhysicsBody"/> class.
    /// </summary>
    /// <param name="world">The physics world this body belongs to.</param>
    /// <param name="body">The underlying Aether body.</param>
    /// <param name="type">Optional type identifier for collision filtering.</param>
    public PhysicsBody(World world, Body body, string? type = null)
    {
        _world = world;
        _body = body;
        Type = type;
    }

    #region IDisposable

    /// <inheritdoc/>
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Disposes the instance. Called from <see cref="Dispose()"/> or when the finalizer runs.
    /// </summary>
    /// <param name="disposing">True if called from <see cref="Dispose()"/> (managed resources can be released); false if called from the finalizer.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (_disposed) return;

        if (disposing)
        {
            // Signal the world to remove this body — do not dispose here, let PhysicsEngine manage removal.
            _body = null;
        }

        _disposed = true;
    }

    #endregion

    #region Position & Rotation

    /// <inheritdoc/>
    public Vector2 WorldPosition => _body?.Position ?? default;

    /// <inheritdoc/>
    public float Rotation
    {
        get => _body?.Rotation ?? 0f;
        set
        {
            if (_body == null) return;
            var pos = _body.Position;
            _body.SetTransform(ref pos, value);
        }
    }

    #endregion

    #region Type & Category

    /// <inheritdoc/>
    public string? Type { get; set; } // Stored internally for filtering; does not affect Aether body.

    /// <inheritdoc/>
    public bool IsStatic => _body?.BodyType == BodyType.Static;
    /// <inheritdoc/>
    public bool IsDynamic => _body?.BodyType == BodyType.Dynamic;
    /// <inheritdoc/>
    public bool IsKinematic => _body?.BodyType == BodyType.Kinematic;

    #endregion

    #region Shape Creation

    /// <summary>
    /// Creates and adds a circle shape collider to this body.
    /// </summary>
    /// <param name="radius">The radius of the circle.</param>
    /// <param name="offset">Optional local offset for the circle's center from the body's origin.</param>
    /// <returns>An ICollider representing the created shape collider.</returns>
    public ICollider CreateCircleCollider(float radius, Vector2? offset = null)
    {
        if (_body == null) throw new ObjectDisposedException(nameof(PhysicsBody));

        var circle = new CoreEssentials.GameSystems.Physics.Engines.Aether.Shapes.CircleShape(radius, density: 1f);
        if (offset.HasValue)
            circle.Translate(offset.Value);

        var aetherFixture = _body.CreateFixture(circle._aetherShape);
        var fixture = new Collider(_world, aetherFixture, this, circle);
        _colliders.Add(fixture);
        return fixture;
    }

    /// <summary>
    /// Creates and adds a rectangle shape fixture to this body.
    /// </summary>
    /// <param name="size">The width and height of the rectangle.</param>
    /// <param name="offset">Optional local offset for the rectangle's center from the body's origin.</param>
    /// <returns>An ICollider representing the created shape collider.</returns>
    public ICollider CreateRectangleCollider(Vector2 size, Vector2? offset = null)
    {
        if (_body == null) throw new ObjectDisposedException(nameof(PhysicsBody));

        var rectangle = new CoreEssentials.GameSystems.Physics.Engines.Aether.Shapes.RectangleShape(size.X, size.Y);
        if (offset.HasValue)
            rectangle.Translate(offset.Value);

        var aetherFixture = _body.CreateFixture(rectangle._aetherShape);
        var fixture = new Collider(_world, aetherFixture, this, rectangle);
        _colliders.Add(fixture);
        return fixture;
    }

    /// <summary>
    /// Creates and adds a polygon shape fixture to this body.
    /// </summary>
    /// <param name="vertices">The vertices defining the polygon in local space.</param>
    /// <returns>An ICollider representing the created shape fixture.</returns>
    public ICollider CreatePolygonCollider(params Vector2[] vertices)
    {
        if (_body == null) throw new ObjectDisposedException(nameof(PhysicsBody));
        if (vertices == null || vertices.Length < 3)
            throw new ArgumentException("At least 3 vertices are required.", nameof(vertices));

        var polygon = new CoreEssentials.GameSystems.Physics.Engines.Aether.Shapes.PolygonShape(vertices);

        var aetherFixture = _body.CreateFixture(polygon._aetherShape);
        var fixture = new Collider(_world, aetherFixture, this, polygon);
        _colliders.Add(fixture);
        return fixture;
    }

    /// <summary>
    /// Creates and adds a convex hull shape from the given points.
    /// </summary>
    /// <param name="points">The points to compute the convex hull from.</param>
    /// <returns>An ICollider representing the created shape fixture.</returns>
    public ICollider CreateConvexHullCollider(params Vector2[] points)
    {
        if (_body == null) throw new ObjectDisposedException(nameof(PhysicsBody));

        var polygon = CoreEssentials.GameSystems.Physics.Engines.Aether.Shapes.PolygonShape.CreateConvexHull((IEnumerable<Vector2>)points);

        var aetherFixture = _body.CreateFixture(polygon._aetherShape);
        var fixture = new Collider(_world, aetherFixture, this, polygon);
        _colliders.Add(fixture);
        return fixture;
    }

    #endregion

    #region Fixture Access

    /// <inheritdoc/>
    public IReadOnlyList<ICollider> Colliders => _colliders;

    #endregion

    #region Fixture Management

    /// <inheritdoc/>
    public void AddCollider(ICollider fixture) => _colliders.Add(fixture);

    /// <inheritdoc/>
    public void RemoveCollider(ICollider fixture) => _colliders.Remove(fixture);

    #endregion

    #region Material Properties

    /// <inheritdoc/>
    public float Mass
    {
        get => _body?.Mass ?? 0f;
        set
        {
            if (_body == null || IsStatic) return;
            _body.Mass = value;
        }
    }

    /// <inheritdoc/>
    public float Inertia => _body?.Inertia ?? 0f;

    /// <inheritdoc/>
    public float Friction
    {
        get => _body?.FixtureList.FirstOrDefault()?.Friction ?? 0.5f;
        set
        {
            if (_body == null) return;
            foreach (var f in _body.FixtureList)
                f.Friction = value;
        }
    }

    /// <inheritdoc/>
    public float Restitution
    {
        get => _body?.FixtureList.FirstOrDefault()?.Restitution ?? 0f;
        set
        {
            if (_body == null) return;
            foreach (var f in _body.FixtureList)
                f.Restitution = value;
        }
    }

    /// <inheritdoc/>
    public bool FixedRotation
    {
        get => _body?.FixedRotation ?? false;
        set => _body!.FixedRotation = value;
    }

    #endregion

    #region Forces, Torque & Impulses

    /// <summary>
    /// Applies a force at the body's center of mass.
    /// Uses Aether.Body.ApplyForce(ref Vector2) which applies to center.
    /// </summary>
    public void ApplyForce(Vector2 force) => _body?.ApplyForce(ref force);

    /// <inheritdoc/>
    public void ApplyTorque(float torque) => _body?.ApplyTorque(torque);

    /// <summary>
    /// Applies an impulse at the body's center of mass.
    /// Uses Aether.Body.ApplyLinearImpulse(ref Vector2) which applies to center.
    /// </summary>
    public void ApplyImpulse(Vector2 impulse) => _body?.ApplyLinearImpulse(ref impulse);

    #endregion

    #region Velocity Control

    /// <inheritdoc/>
    public Vector2 LinearVelocity => _body?.LinearVelocity ?? default;

    /// <inheritdoc/>
    public void SetLinearVelocity(Vector2 linearVelocity)
    {
        if (_body == null || !IsDynamic) return;
        _body.LinearVelocity = linearVelocity;
    }

    /// <inheritdoc/>
    public float AngularVelocity
    {
        get => _body?.AngularVelocity ?? 0f;
        set
        {
            if (_body == null || !IsDynamic) return;
            _body.AngularVelocity = value;
        }
    }

    #endregion

    #region Body State

    /// <inheritdoc/>
    public void StopAll()
    {
        if (_body == null) return;
        _body.ResetDynamics();
    }

    /// <summary>
    /// Gets whether this body is currently awake (simulated).
    /// Aether uses the Awake property for sleeping state.
    /// </summary>
    public bool IsAwake => _body?.Awake == true;

    /// <inheritdoc/>
    public bool IsActive
    {
        get => _body?.Enabled ?? false;
        set => _body!.Enabled = value;
    }

    #endregion
}
