using System;
using System.Xml.Linq;
using Microsoft.Xna.Framework;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Components;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Serialization;
using CoreEssentials.GameSystems.Physics.Types;
using nkast.Aether.Physics2D.Dynamics;

namespace CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Components.BuiltIn;

/// <summary>
/// The type of physics body this component will manage.
/// </summary>
public enum RigidbodyType
{
    /// <summary>
    /// Immovable body with infinite mass. Not affected by forces or collisions.
    /// </summary>
    Static,

    /// <summary>
    /// Body affected by forces, gravity, and collisions.
    /// </summary>
    Dynamic,

    /// <summary>
    /// User-controlled body with infinite mass. Pushes dynamic bodies but is not affected by forces.
    /// </summary>
    Kinematic
}

/// <summary>
/// Component that adds physics behavior to an entity by managing an IPhysicsBody.
/// Syncs entity Position/Rotation with the physics body and vice versa.
/// </summary>
public class RigidbodyComponent : EntityComponent, ISerializableComponent
{
    private IPhysicsBody? _body;
    private bool _bodyCreated;

    /// <summary>
    /// Gets the type of this rigidbody.
    /// </summary>
    public RigidbodyType Type { get; }

    /// <summary>
    /// Gets the underlying physics body. Returns null until the body is created.
    /// The body is lazily created on first access or when Update is called.
    /// </summary>
    internal IPhysicsBody? Body
    {
        get
        {
            EnsureBody();
            return _body;
        }
    }

    /// <summary>
    /// Gets the underlying physics body without triggering lazy creation.
    /// Use during teardown to avoid null-reference when physics engine is unavailable.
    /// </summary>
    internal IPhysicsBody? RawBody => _body;

    /// <summary>
    /// Gets whether the physics body has been created.
    /// </summary>
    public bool IsBodyCreated => _bodyCreated;

    /// <summary>
    /// Gets or sets a value indicating whether physics controls this entity's transform.
    /// When true (default for Dynamic), the entity Position/Rotation sync from the physics body.
    /// When false, the entity transform drives the physics body.
    /// </summary>
    public bool SyncFromPhysics { get; set; }

    /// <summary>
    /// Gets or sets the mass of the physics body. Default is 1.0.
    /// Applied automatically when the body is created, or synced immediately if already created.
    /// </summary>
    public float Mass
    {
        get => _mass;
        set
        {
            _mass = value;
            if (_body != null)
                _body.Mass = value;
        }
    }

    /// <summary>
    /// Gets or sets whether the body's rotation is fixed (prevents torque from rotating it). Default is false.
    /// Applied automatically when the body is created, or synced immediately if already created.
    /// </summary>
    public bool FixedRotation
    {
        get => _fixedRotation;
        set
        {
            _fixedRotation = value;
            if (_body != null)
                _body.FixedRotation = value;
        }
    }

    private float _mass = 1.0f;
    private bool _fixedRotation;

    /// <summary>
    /// Tolerance for treating two transforms as "the same" when detecting external moves.
    /// </summary>
    private const float PositionEpsilon = 0.0001f;
    private const float RotationEpsilon = 0.0001f;

    /// <summary>
    /// The entity transform last written by this component during a body → entity sync.
    /// In <see cref="SyncFromPhysics"/> mode only this component and external code
    /// (save/load, teleport, debug) write the entity transform, so comparing the entity's
    /// current transform against this snapshot tells us whether external code moved it.
    /// </summary>
    private Vector2 _lastEntityPosition;
    private float _lastEntityRotation;

    /// <summary>
    /// Initializes a new instance of the <see cref="RigidbodyComponent"/> class.
    /// </summary>
    /// <param name="type">The type of rigidbody to create.</param>
    public RigidbodyComponent(RigidbodyType type = RigidbodyType.Dynamic)
    {
        Type = type;
        SyncFromPhysics = type == RigidbodyType.Dynamic;
    }

    /// <inheritdoc/>
    public override void OnAttach()
    {
        // Body is created lazily on first Body access or Update call.
    }

    /// <inheritdoc/>
    public override void OnDetach()
    {
        DestroyBody();
    }

    /// <inheritdoc/>
    public override void Update(GameTime gameTime)
    {
        if (_body == null)
            return;

        if (SyncFromPhysics)
        {
            // Physics drives the entity. But if external code (save/load, teleport, debug) moved
            // the entity since the last sync, the entity transform is the new source of truth —
            // adopt it so physics integrates from there instead of snapping the entity back.
            if (EntityMovedExternally())
            {
                _body.Position = Owner.Position;
                _body.Rotation = Owner.Rotation;
                _lastEntityPosition = Owner.Position;
                _lastEntityRotation = Owner.Rotation;
                return;
            }

            Owner.Position = _body.Position;
            Owner.Rotation = _body.Rotation;
            _lastEntityPosition = Owner.Position;
            _lastEntityRotation = Owner.Rotation;
        }
        else
        {
            // Entity drives physics.
            _body.Position = Owner.Position;
            _body.Rotation = Owner.Rotation;
        }
    }

    /// <summary>
    /// In <see cref="SyncFromPhysics"/> mode, returns true when the entity's transform differs from
    /// the transform this component last wrote, meaning external code moved the entity.
    /// </summary>
    private bool EntityMovedExternally()
    {
        var positionMoved = System.Math.Abs(Owner.Position.X - _lastEntityPosition.X) > PositionEpsilon ||
                            System.Math.Abs(Owner.Position.Y - _lastEntityPosition.Y) > PositionEpsilon;
        var rotationMoved = System.Math.Abs(Owner.Rotation - _lastEntityRotation) > RotationEpsilon;
        return positionMoved || rotationMoved;
    }

    /// <summary>
    /// Applies an impulse to the body.
    /// </summary>
    /// <param name="impulse">The impulse to apply.</param>
    public void ApplyImpulse(Vector2 impulse)
    {
        Body?.ApplyImpulse(impulse);
    }

    /// <summary>
    /// Gets or sets the linear velocity of the body.
    /// Setting this directly bypasses physics simulation (useful for restoring saved state).
    /// </summary>
    public Vector2 LinearVelocity
    {
        get => _body?.LinearVelocity ?? default;
        set => SetLinearVelocity(value);
    }

    /// <summary>
    /// Sets the linear velocity of the body directly.
    /// </summary>
    /// <param name="velocity">The new linear velocity.</param>
    public void SetLinearVelocity(Vector2 velocity)
    {
        Body?.SetLinearVelocity(velocity);
    }

    /// <summary>
    /// Gets or sets the angular velocity of the body in radians per second.
    /// </summary>
    public float AngularVelocity
    {
        get => Body?.AngularVelocity ?? 0f;
        set => Body!.AngularVelocity = value;
    }

    /// <summary>
    /// Applies an angular impulse to the body, causing it to rotate.
    /// </summary>
    /// <param name="angularImpulse">The angular impulse to apply.</param>
    public void ApplyAngularImpulse(float angularImpulse)
    {
        Body!.AngularVelocity += angularImpulse;
    }

    /// <summary>
    /// Syncs the physics body transform to match the entity's current Position and Rotation.
    /// Use this when you need to force the physics body to a specific position without waiting for Update().
    /// Setting this directly bypasses physics simulation (useful for teleporting or restoring saved state).
    /// </summary>
    public void SyncBodyFromEntity()
    {
        var body = Body;
        if (body != null)
        {
            body.Position = Owner.Position;
            body.Rotation = Owner.Rotation;
        }
    }

    /// <summary>
    /// Ensures the physics body is created. Creates it lazily if it doesn't exist yet.
    /// </summary>
    private void EnsureBody()
    {
        if (_bodyCreated)
            return;

        var physicsEngine = GetPhysicsEngine();
        if (physicsEngine == null)
            return;

        var position = Owner.Position;

        _body = Type switch
        {
            RigidbodyType.Static => physicsEngine.CreateStatic(position),
            RigidbodyType.Dynamic => physicsEngine.CreateDynamic(position),
            RigidbodyType.Kinematic => physicsEngine.CreateKinematic(position),
            _ => throw new InvalidOperationException($"Unknown RigidbodyType: {Type}")
        };

        // Apply deferred properties
        _body.Mass = _mass;
        _body.FixedRotation = _fixedRotation;

        // The body was just created at the entity's current transform, so seed the snapshot
        // with it. This prevents the first Update from mistaking the initial (matching)
        // transform for an external move.
        _lastEntityPosition = Owner.Position;
        _lastEntityRotation = Owner.Rotation;

        _bodyCreated = true;
    }

    /// <summary>
    /// Creates the physics body immediately using the entity's current position.
    /// This is called automatically when the component is attached to an entity.
    /// </summary>
    public void CreateBody()
    {
        EnsureBody();
    }

    /// <summary>
    /// Destroys the physics body and cleans up resources.
    /// </summary>
    public void DestroyBody()
    {
        if (_body != null)
        {
            var physicsEngine = GetPhysicsEngine();
            physicsEngine?.Destroy(_body);
            _body = null;
            _bodyCreated = false;
        }
    }

    private Physics.Engines.Aether.PhysicsEngine? GetPhysicsEngine()
    {
        var entitySystem = Owner.GetEntitySystem();
        if (entitySystem == null)
            return null;

        try
        {
            return entitySystem.GetGameSystem<Physics.Engines.Aether.PhysicsEngine>();
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Serializes the rigidbody component's state to an XML element.
    /// </summary>
    /// <returns>An XML element containing the component's serialized state.</returns>
    public XElement SerializeToXml()
    {
        var body = RawBody;
        return new XElement("RigidbodyState",
            new XAttribute("Type", Type.ToString()),
            new XAttribute("Mass", _mass),
            new XAttribute("FixedRotation", _fixedRotation),
            new XAttribute("SyncFromPhysics", SyncFromPhysics),
            body != null ? new XAttribute("LinearVelocityX", body.LinearVelocity.X) : null,
            body != null ? new XAttribute("LinearVelocityY", body.LinearVelocity.Y) : null,
            body != null ? new XAttribute("AngularVelocity", body.AngularVelocity) : null
        );
    }

    /// <summary>
    /// Deserializes the rigidbody component's state from an XML element.
    /// </summary>
    /// <param name="element">The XML element containing the component's state.</param>
    public void DeserializeFromXml(XElement element)
    {
        _mass = float.Parse(element.Attribute("Mass")?.Value ?? "1.0");
        _fixedRotation = bool.Parse(element.Attribute("FixedRotation")?.Value ?? "false");
        SyncFromPhysics = bool.Parse(element.Attribute("SyncFromPhysics")?.Value ?? "true");

        // Store velocity to apply after body is created
        var linearVelX = element.Attribute("LinearVelocityX")?.Value;
        var linearVelY = element.Attribute("LinearVelocityY")?.Value;
        var angularVel = element.Attribute("AngularVelocity")?.Value;

        if (_body != null && !string.IsNullOrEmpty(linearVelX) && !string.IsNullOrEmpty(linearVelY))
        {
            _body.SetLinearVelocity(new Vector2(
                float.Parse(linearVelX),
                float.Parse(linearVelY)
            ));
        }

        if (_body != null && !string.IsNullOrEmpty(angularVel))
        {
            _body.AngularVelocity = float.Parse(angularVel);
        }
    }
}
