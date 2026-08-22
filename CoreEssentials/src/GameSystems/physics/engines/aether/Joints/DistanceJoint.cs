using CoreEssentials.GameSystems.Physics.Types;
using Microsoft.Xna.Framework;
using nkast.Aether.Physics2D.Dynamics;
using nkast.Aether.Physics2D.Dynamics.Joints;

namespace CoreEssentials.GameSystems.Physics.Engines.Aether.Joints;

/// <summary>
/// 🔒 Implements IDistanceJoint, wraps Aether DistanceJoint for maintaining fixed distance between two bodies.
/// Can act as a spring when frequency/damping are configured.
/// </summary>
public class DistanceJoint : IDistanceJoint
{
    private readonly World _world;
    internal readonly nkast.Aether.Physics2D.Dynamics.Joints.DistanceJoint _aetherJoint;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="DistanceJoint"/> class.
    /// </summary>
    /// <param name="world">The physics world this joint belongs to.</param>
    /// <param name="aetherJoint">The underlying Aether distance joint.</param>
    public DistanceJoint(World world, nkast.Aether.Physics2D.Dynamics.Joints.DistanceJoint aetherJoint)
    {
        _world = world;
        _aetherJoint = aetherJoint ?? throw new ArgumentNullException(nameof(aetherJoint));
    }

    #region IConstraint Properties

    /// <inheritdoc/>
    public IPhysicsBody BodyA => new PhysicsBody(_world, _aetherJoint.BodyA);

    /// <inheritdoc/>
    public IPhysicsBody? BodyB => new PhysicsBody(_world, _aetherJoint.BodyB);

    /// <inheritdoc/>
    public bool IsActive => !_disposed && _aetherJoint.Enabled;

    /// <inheritdoc/>
    public void Apply()
    {
        // Aether solver handles constraint resolution internally during world step.
        // This method is a no-op in our wrapper as the joint is automatically applied.
    }

    /// <inheritdoc/>
    public void Remove()
    {
        if (_disposed) return;
        _disposed = true;
        _world.Remove(_aetherJoint);
    }

    #endregion

    #region IDistanceJoint Properties

    /// <inheritdoc/>
    public Vector2 LocalAnchorA => _aetherJoint.LocalAnchorA;

    /// <inheritdoc/>
    public Vector2 LocalAnchorB => _aetherJoint.LocalAnchorB;

    /// <inheritdoc/>
    public float Length
    {
        get => _aetherJoint.Length;
        set => _aetherJoint.Length = value;
    }

    /// <summary>
    /// Aether's DistanceJoint does not support max force — this property always returns 0 and setting has no effect.
    /// For force-limited behavior, consider using a different joint type or applying forces manually.
    /// </summary>
    public float MaxForce
    {
        get => 0f;
        set { /* Not supported by Aether's DistanceJoint */ }
    }

    /// <inheritdoc/>
    public bool CollideConnected
    {
        get => _aetherJoint.CollideConnected;
        set => _aetherJoint.CollideConnected = value;
    }

    /// <inheritdoc/>
    public float FrequencyHz
    {
        get => _aetherJoint.Frequency;
        set => _aetherJoint.Frequency = value;
    }

    /// <inheritdoc/>
    public float DampingRatio
    {
        get => _aetherJoint.DampingRatio;
        set => _aetherJoint.DampingRatio = value;
    }

    #endregion

    #region IDisposable

    /// <inheritdoc/>
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        Remove();
    }

    #endregion
}
