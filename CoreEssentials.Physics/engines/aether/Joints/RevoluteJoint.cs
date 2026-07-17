using CoreEssentials.Physics.Types;
using Microsoft.Xna.Framework;
using nkast.Aether.Physics2D.Dynamics;
using nkast.Aether.Physics2D.Dynamics.Joints;

namespace CoreEssentials.Physics.Engines.Aether.Joints;

/// <summary>
/// 🔒 Implements IRevoluteJoint, wraps Aether RevoluteJoint for hinge-like rotation between two bodies.
/// </summary>
public class RevoluteJoint : IRevoluteJoint
{
    private readonly World _world;
    internal readonly nkast.Aether.Physics2D.Dynamics.Joints.RevoluteJoint _aetherJoint;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="RevoluteJoint"/> class.
    /// </summary>
    /// <param name="world">The physics world this joint belongs to.</param>
    /// <param name="aetherJoint">The underlying Aether revolute joint.</param>
    public RevoluteJoint(World world, nkast.Aether.Physics2D.Dynamics.Joints.RevoluteJoint aetherJoint)
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

    #region IRevoluteJoint Properties

    /// <inheritdoc/>
    public Vector2 LocalAnchorA => _aetherJoint.LocalAnchorA;

    /// <inheritdoc/>
    public Vector2 LocalAnchorB => _aetherJoint.LocalAnchorB;

    /// <inheritdoc/>
    public float MinAngle
    {
        get => _aetherJoint.LowerLimit;
        set => _aetherJoint.LowerLimit = value;
    }

    /// <inheritdoc/>
    public float MaxAngle
    {
        get => _aetherJoint.UpperLimit;
        set => _aetherJoint.UpperLimit = value;
    }

    /// <summary>
    /// Gets whether angle limits are enabled.
    /// </summary>
    private bool LimitEnabled
    {
        get => _aetherJoint.LimitEnabled;
        set => _aetherJoint.LimitEnabled = value;
    }

    /// <inheritdoc/>
    public bool MotorEnabled
    {
        get => _aetherJoint.MotorEnabled;
        set => _aetherJoint.MotorEnabled = value;
    }

    /// <inheritdoc/>
    public float MotorSpeed
    {
        get => _aetherJoint.MotorSpeed;
        set => _aetherJoint.MotorSpeed = value;
    }

    /// <inheritdoc/>
    public float MaxMotorTorque
    {
        get => _aetherJoint.MaxMotorTorque;
        set => _aetherJoint.MaxMotorTorque = value;
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
