using CoreEssentials.GameSystems.Physics.Types;
using Microsoft.Xna.Framework;
using nkast.Aether.Physics2D.Dynamics;
using nkast.Aether.Physics2D.Dynamics.Joints;

namespace CoreEssentials.GameSystems.Physics.Engines.Aether.Joints;

/// <summary>
/// 🔒 Implements IWeldJoint, wraps Aether WeldJoint for rigid connections between two bodies.
/// </summary>
public class WeldJoint : IWeldJoint
{
    private readonly World _world;
    internal readonly nkast.Aether.Physics2D.Dynamics.Joints.WeldJoint _aetherJoint;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="WeldJoint"/> class.
    /// </summary>
    /// <param name="world">The physics world this joint belongs to.</param>
    /// <param name="aetherJoint">The underlying Aether weld joint.</param>
    public WeldJoint(World world, nkast.Aether.Physics2D.Dynamics.Joints.WeldJoint aetherJoint)
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

    #region IWeldJoint Properties

    /// <inheritdoc/>
    public Vector2 LocalAnchorA => _aetherJoint.LocalAnchorA;

    /// <inheritdoc/>
    public Vector2 LocalAnchorB => _aetherJoint.LocalAnchorB;

    /// <inheritdoc/>
    public bool CollideConnected
    {
        get => _aetherJoint.CollideConnected;
        set => _aetherJoint.CollideConnected = value;
    }

    /// <summary>
    /// Gets or sets the stiffness as a frequency in Hertz (maps to Aether's FrequencyHz).
    /// Higher values mean stiffer joints. Use 0f for rigid connection.
    /// </summary>
    public float Stiffness
    {
        get => _aetherJoint.FrequencyHz;
        set => _aetherJoint.FrequencyHz = value;
    }

    /// <summary>
    /// Gets or sets the damping ratio (maps to Aether's DampingRatio).
    /// 0 = no damping, 1 = critical damping.
    /// </summary>
    public float Damping
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
