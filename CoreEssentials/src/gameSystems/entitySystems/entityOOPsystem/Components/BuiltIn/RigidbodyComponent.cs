using System;
using Microsoft.Xna.Framework;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Components;
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
public class RigidbodyComponent : EntityComponent
{
    private IPhysicsBody? _body;
    private bool _bodyCreated;

    /// <summary>
    /// Gets the type of this rigidbody.
    /// </summary>
    public RigidbodyType Type { get; }

    /// <summary>
    /// Gets the underlying physics body. Returns null until the body is created.
    /// The body is lazily created when first accessed or when Update is called.
    /// </summary>
    public IPhysicsBody? Body => _body;

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
        // Body is lazily created on first Update or explicit CreateBody call.
    }

    /// <inheritdoc/>
    public override void OnDetach()
    {
        DestroyBody();
    }

    /// <inheritdoc/>
    public override void Update(GameTime gameTime)
    {
        if (!_bodyCreated)
        {
            CreateBody();
        }

        if (_body == null)
            return;

        if (SyncFromPhysics)
        {
            // Physics drives entity
            Owner.Position = _body.WorldPosition;
            Owner.Rotation = _body.Rotation;
        }
        else
        {
            // Entity drives physics
            var pos = Owner.Position;
            _body.Rotation = Owner.Rotation;
        }
    }

    /// <summary>
    /// Creates the physics body immediately using the entity's current position.
    /// </summary>
    public void CreateBody()
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

        _bodyCreated = true;
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
}
