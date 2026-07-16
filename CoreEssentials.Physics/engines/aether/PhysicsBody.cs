using CoreEssentials.Physics.Types;
using Microsoft.Xna.Framework;
using nkast.Aether.Physics2D.Collision.Shapes;
using nkast.Aether.Physics2D.Common;
using nkast.Aether.Physics2D.Dynamics;

namespace CoreEssentials.Physics.Engines.Aether;

/// <summary>
/// Implements IPhysicsBody, wraps Aether.Body.
/// </summary>
public class PhysicsBody : IPhysicsBody
{
    private readonly World _world;
    internal Body? _body; // Internal so PhysicsEngine can access it for removal
    private readonly string? _type;
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
        _type = type;
    }

    #region IDisposable

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        // Signal the world to remove this body — do not dispose here, let PhysicsEngine manage removal.
        _body = null;
    }

    #endregion

    #region Position & Rotation

    public Vector2 WorldPosition => _body?.Position ?? default;

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

    public string? Type
    {
        get => _type;
        set { /* type is set at creation, stored but not mutated on body */ }
    }

    public bool IsStatic => _body?.BodyType == BodyType.Static;
    public bool IsDynamic => _body?.BodyType == BodyType.Dynamic;
    public bool IsKinematic => _body?.BodyType == BodyType.Kinematic;

    #endregion

    #region Shape Creation

    /// <summary>
    /// Creates and adds a circle shape fixture to this body.
    /// </summary>
    public IFixture CreateCircle(float radius, Vector2? offset = null) => throw new NotImplementedException();

    /// <summary>
    /// Creates and adds a rectangle shape fixture to this body.
    /// </summary>
    public IFixture CreateRectangle(Vector2 size, Vector2? offset = null) => throw new NotImplementedException();

    /// <summary>
    /// Creates and adds a polygon shape fixture to this body.
    /// </summary>
    public IFixture CreatePolygon(params Vector2[] vertices) => throw new NotImplementedException();

    /// <summary>
    /// Creates and adds a convex hull shape from the given points.
    /// </summary>
    public IFixture CreateConvexHull(params Vector2[] points) => throw new NotImplementedException();

    #endregion

    #region Fixture Management

    public void AddFixture(IFixture fixture) { /* no-op — fixtures added at creation time */ }

    public void RemoveFixture(IFixture fixture) { /* no-op — handled by PhysicsEngine in Sprint 5 */ }

    #endregion

    #region Material Properties

    public float Mass => _body?.Mass ?? 0f;
    public float Inertia => _body?.Inertia ?? 0f;

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

    public void ApplyTorque(float torque) => _body?.ApplyTorque(torque);

    /// <summary>
    /// Applies an impulse at the body's center of mass.
    /// Uses Aether.Body.ApplyLinearImpulse(ref Vector2) which applies to center.
    /// </summary>
    public void ApplyImpulse(Vector2 impulse) => _body?.ApplyLinearImpulse(ref impulse);

    #endregion

    #region Velocity Control

    public Vector2 LinearVelocity => _body?.LinearVelocity ?? default;

    public void SetLinearVelocity(Vector2 linearVelocity)
    {
        if (_body == null || !IsDynamic) return;
        _body.LinearVelocity = linearVelocity;
    }

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

    public bool IsActive
    {
        get => _body?.Enabled ?? false;
        set => _body!.Enabled = value;
    }

    #endregion
}
