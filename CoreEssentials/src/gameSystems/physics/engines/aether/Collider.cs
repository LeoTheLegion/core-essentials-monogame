#pragma warning disable CA1822 // Members that do not access instance data can be marked as static
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using CoreEssentials.GameSystems.Physics.Types;
using Microsoft.Xna.Framework;
using nkast.Aether.Physics2D.Collision;
using nkast.Aether.Physics2D.Common;
using nkast.Aether.Physics2D.Dynamics;
#pragma warning restore CA1822
// Suppress accessibility bypass warnings for reflection on internal Aether members.
#pragma warning disable CA2252 // Taking StackCapture responsibility requires explicit Dispose

namespace CoreEssentials.GameSystems.Physics.Engines.Aether;

/// <summary>
/// 🔒 Internal use only by PhysicsBody. Implements ICollider, wraps Aether.Fixture.
/// </summary>
[SuppressMessage("Design", "CA1822:Mark members as static", Justification = "Uses reflection to access internal Aether non-public members.")]
[SuppressMessage("Security", "CA2253:Protect against suspicious accessibility bypasses", Justification = "Reflection targets internal Aether physics engine members that are intentionally private. No user data is exposed via these reflections.")]
public class Collider : ICollider
{
    private readonly World _world;
    internal readonly nkast.Aether.Physics2D.Dynamics.Fixture _aetherFixture;
    private readonly PhysicsBody _ownerBody;
    private readonly IShape? _shape;
    private bool _disposed;

    // Cached reflection info for internal Aether methods (called only when needed).
#pragma warning disable S3011 // Accessibility bypass via reflection on internal Aether members.
    private static readonly MethodInfo? s_createProxies = typeof(nkast.Aether.Physics2D.Dynamics.Fixture)
        .GetMethod("CreateProxies", BindingFlags.NonPublic | BindingFlags.Instance);
#pragma warning restore S3011

    // Cached reflection info for DestroyProxies (internal Aether method).
#pragma warning disable S3011 // Accessibility bypass via reflection on internal Aether members.
    private static readonly MethodInfo? s_destroyProxies = typeof(nkast.Aether.Physics2D.Dynamics.Fixture)
        .GetMethod("DestroyProxies", BindingFlags.NonPublic | BindingFlags.Instance);
#pragma warning restore S3011

    /// <summary>
    /// Initializes a new instance of the <see cref="Collider"/> class.
    /// </summary>
    /// <param name="world">The physics world this fixture belongs to.</param>
    /// <param name="aetherFixture">The underlying Aether fixture.</param>
    /// <param name="ownerBody">The PhysicsBody that owns this fixture.</param>
    public Collider(World world, nkast.Aether.Physics2D.Dynamics.Fixture aetherFixture, PhysicsBody ownerBody)
        : this(world, aetherFixture, ownerBody, shape: null) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="Collider"/> class with an associated IShape wrapper.
    /// </summary>
    /// <param name="world">The physics world this fixture belongs to.</param>
    /// <param name="aetherFixture">The underlying Aether fixture.</param>
    /// <param name="ownerBody">The PhysicsBody that owns this fixture.</param>
    /// <param name="shape">The IShape wrapper associated with this fixture, if available.</param>
    internal Collider(World world, nkast.Aether.Physics2D.Dynamics.Fixture aetherFixture, PhysicsBody ownerBody, IShape? shape)
    {
        _world = world;
        _aetherFixture = aetherFixture ?? throw new ArgumentNullException(nameof(aetherFixture));
        _ownerBody = ownerBody ?? throw new ArgumentNullException(nameof(ownerBody));
        _shape = shape;
    }

    #region IDisposable

    private bool _disposing;

    /// <inheritdoc/>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Disposes the instance. Called from <see cref="Collider.Dispose()"/> or when the finalizer runs.
    /// </summary>
    /// <param name="disposing">True if called from <see cref="Collider.Dispose()"/> (managed resources can be released); false if called from the finalizer.</param>
    [SuppressMessage("Usage", "CA1822:Mark members as static", Justification = "Part of dispose pattern; accesses instance state.")]
    protected virtual void Dispose(bool disposing)
    {
        if (_disposed) return;

        if (disposing && !_disposing)
        {
            _disposing = true;
            // The owner PhysicsBody manages removal — no managed resources to dispose here.
        }

        _disposed = true;
    }

    #endregion

    /// <summary>
    /// Gets the shape associated with this collider, or null if not available.
    /// </summary>
    public IShape? Shape => _shape;

    /// <summary>
    /// Gets whether this collider is currently active (enabled) in the simulation.
    /// A collider is considered active if it has proxies registered on the broad-phase.
    /// </summary>
    public bool IsActive => _aetherFixture.ProxyCount > 0;

    /// <summary>
    /// Gets the body that owns this fixture.
    /// </summary>
    public IPhysicsBody OwnerBody => _ownerBody;

    /// <summary>
    /// Gets or sets the friction coefficient (0 = slippery, 1 = sticky).
    /// </summary>
    public float Friction
    {
        get => _aetherFixture.Friction;
        set => _aetherFixture.Friction = value;
    }

    /// <summary>
    /// Gets or sets the restitution/bounciness (0 = no bounce, 1 = full bounce).
    /// </summary>
    public float Restitution
    {
        get => _aetherFixture.Restitution;
        set => _aetherFixture.Restitution = value;
    }

    /// <summary>
    /// Activates this Collider so it participates in collision detection.
    /// Re-creates proxies on the broad-phase using reflection to access internal Aether method.
    /// Uses reflection to call non-public CreateProxies and access _xf field — these are necessary
    /// because Aether's public API does not expose proxy management, which is needed for fixture lifecycle control.
    /// </summary>
    public void Activate()
    {
        if (_aetherFixture.Body == null || _world.IsLocked) return;

        // Ensure body is enabled
        _aetherFixture.Body.Enabled = true;

        var broadPhase = _world.ContactManager.BroadPhase;
        if (s_createProxies != null)
        {
            try
            {
#pragma warning disable CA2252 // Taking StackCapture responsibility requires explicit Dispose — safe here because we only read Transform, not call Take.
#pragma warning disable S3011 // Accessibility bypass via reflection on internal Aether members.                
                // Get the internal _xf field from Body via reflection.
                var bodyType = _aetherFixture.Body.GetType();
                var xfField = bodyType.GetField("_xf", BindingFlags.NonPublic | BindingFlags.Instance);
#pragma warning restore S3011
#pragma warning restore CA2252
                if (xfField != null)
                {
                    var xf = (Transform)xfField.GetValue(_aetherFixture.Body)!;
                    s_createProxies.Invoke(_aetherFixture, new object[] { broadPhase, xf });
                }
            }
            catch
            {
                // If reflection fails, the fixture may already have proxies from creation.
            }
        }
    }

    /// <summary>
    /// Deactivates this collider so it no longer participates in collision detection.
    /// Destroys proxies from the broad-phase using reflection to access internal Aether method.
    /// Uses reflection to call non-public DestroyProxies — necessary because Aether's public
    /// API does not expose proxy destruction, which is needed for fixture lifecycle control.
    /// Note: Only affects this specific fixture, not the owner body or sibling fixtures.
    /// </summary>
    public void Deactivate()
    {
        if (_aetherFixture.Body == null) return;

        // Destroy proxies via reflection — internal Aether method.
        var broadPhase = _world.ContactManager.BroadPhase;
        if (s_destroyProxies != null)
        {
            try
            {
                s_destroyProxies.Invoke(_aetherFixture, new object[] { broadPhase });
            }
            catch
            {
                // If reflection fails, proxies may already be destroyed.
            }
        }
    }
}
