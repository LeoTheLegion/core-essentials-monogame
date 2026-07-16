using CoreEssentials.Physics.Types;
using Microsoft.Xna.Framework;

namespace CoreEssentials.Physics.Engines.Aether;

/// <summary>
/// Implements IPhysicsBody, wraps Aether.Body.
/// </summary>
public class PhysicsBody : IPhysicsBody
{
    // TODO: Implement in Sprint 2 - wrapper around Aether.Body

    #region IDisposable

    public void Dispose() { }

    #endregion

    #region Position & Rotation

    public Vector2 WorldPosition => throw new NotImplementedException();
    public float Rotation { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    #endregion

    #region Type & Category

    public string? Type { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public bool IsStatic => throw new NotImplementedException();
    public bool IsDynamic => throw new NotImplementedException();
    public bool IsKinematic => throw new NotImplementedException();

    #endregion

    #region Shape Creation

    public IFixture CreateCircle(float radius, Vector2? offset = null) => throw new NotImplementedException();
    public IFixture CreateRectangle(Vector2 size, Vector2? offset = null) => throw new NotImplementedException();
    public IFixture CreatePolygon(params Vector2[] vertices) => throw new NotImplementedException();
    public IFixture CreateConvexHull(params Vector2[] points) => throw new NotImplementedException();

    #endregion

    #region Fixture Management

    public void AddFixture(IFixture fixture) => throw new NotImplementedException();
    public void RemoveFixture(IFixture fixture) => throw new NotImplementedException();

    #endregion

    #region Material Properties

    public float Mass => throw new NotImplementedException();
    public float Inertia => throw new NotImplementedException();
    public float Friction { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public float Restitution { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public bool FixedRotation { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    #endregion

    #region Forces, Torque & Impulses

    public void ApplyForce(Vector2 force) => throw new NotImplementedException();
    public void ApplyTorque(float torque) => throw new NotImplementedException();
    public void ApplyImpulse(Vector2 impulse) => throw new NotImplementedException();

    #endregion

    #region Velocity Control

    public Vector2 LinearVelocity => throw new NotImplementedException();
    public void SetLinearVelocity(Vector2 linearVelocity) => throw new NotImplementedException();
    public float AngularVelocity { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    #endregion

    #region Body State

    public void StopAll() => throw new NotImplementedException();
    public bool IsAwake => throw new NotImplementedException();
    public bool IsActive { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    #endregion
}
