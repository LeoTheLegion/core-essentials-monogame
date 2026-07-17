using System.Reflection;
using CoreEssentials.Physics.Types;
using Microsoft.Xna.Framework;
using nkast.Aether.Physics2D.Collision;
using nkast.Aether.Physics2D.Common;
using nkast.Aether.Physics2D.Dynamics;

namespace CoreEssentials.Physics.Engines.Aether;

/// <summary>
/// 🔒 Internal use only by PhysicsBody. Implements IFixture, wraps Aether.Fixture.
/// </summary>
public class Fixture : IFixture
{
    private readonly World _world;
    internal readonly nkast.Aether.Physics2D.Dynamics.Fixture _aetherFixture;
    private readonly PhysicsBody _ownerBody;
    private readonly IShape? _shape;
    private bool _disposed;

    // Cached reflection info for internal Aether methods (called only when needed).
    private static readonly MethodInfo? s_createProxies = typeof(nkast.Aether.Physics2D.Dynamics.Fixture)
        .GetMethod("CreateProxies", BindingFlags.NonPublic | BindingFlags.Instance);
    private static readonly MethodInfo? s_destroyProxies = typeof(nkast.Aether.Physics2D.Dynamics.Fixture)
        .GetMethod("DestroyProxies", BindingFlags.NonPublic | BindingFlags.Instance);

    /// <summary>
    /// Initializes a new instance of the <see cref="Fixture"/> class.
    /// </summary>
    /// <param name="world">The physics world this fixture belongs to.</param>
    /// <param name="aetherFixture">The underlying Aether fixture.</param>
    /// <param name="ownerBody">The PhysicsBody that owns this fixture.</param>
    public Fixture(World world, nkast.Aether.Physics2D.Dynamics.Fixture aetherFixture, PhysicsBody ownerBody)
        : this(world, aetherFixture, ownerBody, shape: null) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="Fixture"/> class with an associated IShape wrapper.
    /// </summary>
    /// <param name="world">The physics world this fixture belongs to.</param>
    /// <param name="aetherFixture">The underlying Aether fixture.</param>
    /// <param name="ownerBody">The PhysicsBody that owns this fixture.</param>
    /// <param name="shape">The IShape wrapper associated with this fixture, if available.</param>
    internal Fixture(World world, nkast.Aether.Physics2D.Dynamics.Fixture aetherFixture, PhysicsBody ownerBody, IShape? shape)
    {
        _world = world;
        _aetherFixture = aetherFixture ?? throw new ArgumentNullException(nameof(aetherFixture));
        _ownerBody = ownerBody ?? throw new ArgumentNullException(nameof(ownerBody));
        _shape = shape;
    }

    #region IDisposable

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        // The owner PhysicsBody manages removal — no-op here.
    }

    #endregion

    /// <summary>
    /// Gets the shape associated with this fixture, or null if not available.
    /// </summary>
    public IShape? Shape => _shape;

    /// <summary>
    /// Gets whether this fixture is currently active (enabled) in the simulation.
    /// A fixture is considered active if it has proxies registered on the broad-phase.
    /// </summary>
    public bool IsActive => _aetherFixture.ProxyCount > 0;

    /// <summary>
    /// Gets the body that owns this fixture.
    /// </summary>
    public IPhysicsBody OwnerBody => _ownerBody;

    /// <summary>
    /// Activates this fixture so it participates in collision detection.
    /// Re-creates proxies on the broad-phase using reflection to access internal Aether method.
    /// </summary>
    public void Activate()
    {
        if (_aetherFixture.Body == null || _world.IsLocked) return;

        // Ensure body is enabled
        _aetherFixture.Body.Enabled = true;

        // Create proxies via reflection (internal Aether method).
        var broadPhase = _world.ContactManager.BroadPhase;
        var createMethod = s_createProxies;
        if (createMethod != null)
        {
            try
            {
                // Get the internal _xf field from Body
                var bodyType = _aetherFixture.Body.GetType();
                var xfField = bodyType.GetField("_xf", BindingFlags.NonPublic | BindingFlags.Instance);
                if (xfField != null)
                {
                    var xf = (Transform)xfField.GetValue(_aetherFixture.Body)!;
                    createMethod.Invoke(_aetherFixture, new object[] { broadPhase, xf });
                }
            }
            catch
            {
                // If reflection fails, the fixture may already have proxies from creation.
            }
        }
    }

    /// <summary>
    /// Deactivates this fixture so it no longer participates in collision detection.
    /// Destroys proxies from the broad-phase using reflection to access internal Aether method.
    /// Note: Only affects this specific fixture, not the owner body or sibling fixtures.
    /// </summary>
    public void Deactivate()
    {
        if (_aetherFixture.Body == null) return;

        // Destroy proxies via reflection (internal Aether method).
        var broadPhase = _world.ContactManager.BroadPhase;
        var destroyMethod = s_destroyProxies;
        if (destroyMethod != null)
        {
            try
            {
                destroyMethod.Invoke(_aetherFixture, new object[] { broadPhase });
            }
            catch
            {
                // If reflection fails, proxies may already be destroyed.
            }
        }
    }
}
