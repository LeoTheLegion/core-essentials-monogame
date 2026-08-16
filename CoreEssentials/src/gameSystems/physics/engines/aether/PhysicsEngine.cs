using System;
using System.Collections.Generic;
using System.Linq;
using CoreEssentials.GameSystems;
using CoreEssentials.GameSystems.Physics.Types;
using Microsoft.Xna.Framework;
using nkast.Aether.Physics2D.Collision;
using nkast.Aether.Physics2D.Collision.Shapes;
using nkast.Aether.Physics2D.Common;
using nkast.Aether.Physics2D.Dynamics;
using nkast.Aether.Physics2D.Dynamics.Contacts;

// Re-export Settings for external use if needed.

namespace CoreEssentials.GameSystems.Physics.Engines.Aether;

/// <summary>
/// ⭐ Main entry point for the physics system. Wraps Aether.World and implements IFixedUpdateGameSystem.
/// Users interact through this GameSystem to create bodies, set gravity, and step the simulation.
/// Bodies are automatically pooled on destroy (recycled instead of GC'd) to reduce allocation pressure.
/// </summary>
public class PhysicsEngine : GameSystem, IFixedUpdateGameSystem, IPhysicsWorld
{
    private readonly World _world;
    private bool _disposed;

    /// <inheritdoc/>
    public IReadOnlyList<IPhysicsBody> GetBodies() => _physicsBodies.Values.Cast<IPhysicsBody>().ToList();

    // Solver configuration — maps to Sprint 1 SolverConfig once available.
    /// <summary>
    /// Number of velocity iterations per time step (default: 8).
    /// Higher values produce more accurate contact resolution but cost more CPU.
    /// </summary>
    public int VelocityIterations { get; set; } = 8;

    /// <summary>
    /// Number of position iterations per time step (default: 3).
    /// Higher values reduce joint/solver drift but cost more CPU.
    /// </summary>
    public int PositionIterations { get; set; } = 3;

    /// <summary>
    /// Gets the declarative configuration this engine was created from, if any.
    /// Exposes the named collision-category map so scenes can resolve friendly names
    /// (e.g. <c>config.Resolve("Player")</c>) to <see cref="CollisionCategory"/> bits.
    /// </summary>
    public PhysicsConfig? Config { get; }

    // Cache of PhysicsBody wrappers keyed by Aether Body — prevents duplicate wrappers.
    private readonly Dictionary<Body, PhysicsBody> _physicsBodies = new();

    // Pool of recycled bodies to reduce GC pressure on frequent create/destroy cycles.
    private readonly Queue<Body> _bodyPool = new();

    // Tracks body pairs currently in active contact (reference-counted to handle
    // multiple simultaneous contacts between the same two bodies). Used by GetActiveContacts().
    private readonly Dictionary<ContactKey, int> _activeContacts = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="PhysicsEngine"/> class with default gravity (0, -9.81).
    /// Creates an internal Aether World that users never see directly.
    /// </summary>
    public PhysicsEngine()
    {
        _world = new World();
        WireContactManager();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PhysicsEngine"/> class with custom gravity.
    /// </summary>
    /// <param name="gravity">The global gravity vector (e.g., Vector2.Zero for no gravity).</param>
    public PhysicsEngine(Vector2 gravity)
    {
        _world = new World(gravity);
        WireContactManager();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PhysicsEngine"/> class from a declarative
    /// <see cref="PhysicsConfig"/>. Applies the configured gravity and solver iterations, and
    /// exposes the config (for named collision-category resolution) via <see cref="Config"/>.
    /// </summary>
    /// <param name="config">The physics configuration loaded from XML.</param>
    public PhysicsEngine(PhysicsConfig config)
    {
        if (config == null) throw new ArgumentNullException(nameof(config));

        Config = config;
        _world = new World(config.Gravity);
        VelocityIterations = config.VelocityIterations;
        PositionIterations = config.PositionIterations;
        WireContactManager();
    }

    private void WireContactManager()
    {
        _world.ContactManager.BeginContact += OnWorldBeginContact;
        _world.ContactManager.EndContact += OnWorldEndContact;
    }

    #region IDisposable (inherited from IPhysicsWorld)

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
            // Unsubscribe from Aether world events to prevent memory leaks.
            _world.ContactManager.BeginContact -= OnWorldBeginContact;
            _world.ContactManager.EndContact -= OnWorldEndContact;

            // Clear all bodies, joints, and fixtures from the world.
            _world.Clear();
            _physicsBodies.Clear();
            _activeContacts.Clear();
        }

        _disposed = true;
    }

    #endregion

    #region ContactManager Routing

    private bool OnWorldBeginContact(Contact contact)
    {
        if (_disposed)
            return true;

        var fixtureA = contact.FixtureA;
        var fixtureB = contact.FixtureB;

        // Resolve owner bodies for each fixture.
        if (!_physicsBodies.TryGetValue(fixtureA.Body, out var bodyA))
            return true;
        if (!_physicsBodies.TryGetValue(fixtureB.Body, out var bodyB))
            return true;

        // Both wrappers must still be alive (body not disposed).
        if (bodyA is not PhysicsBody pbA || bodyB is not PhysicsBody pbB)
            return true;
        if (pbA._body == null || pbB._body == null)
            return true;

        // Resolve fixtures to colliders.
        var colliderA = GetColliderFor(pbA, fixtureA);
        var colliderB = GetColliderFor(pbB, fixtureB);

        // Notify both bodies of the collision and collect rejection results.
        bool rejectBodyA = pbA.RaiseOnCollision(bodyB);
        bool rejectBodyB = pbB.RaiseOnCollision(bodyA);

        // Notify colliders if we could resolve them (both must exist for collider events).
        bool rejectCollider = false;
        if (colliderA != null && colliderB != null)
        {
            var concreteA = colliderA as Collider;
            var concreteB = colliderB as Collider;
            if (concreteA != null && concreteB != null)
            {
                bool rejectA = concreteA.RaiseOnCollision(concreteB);
                bool rejectB = concreteB.RaiseOnCollision(concreteA);
                rejectCollider = rejectA || rejectB;
            }
        }

        // If any handler (body or collider) returned false, disable the contact to reject it.
        bool rejected = rejectBodyA || rejectBodyB || rejectCollider;
        if (rejected)
            contact.Enabled = false;

        // Track the active contact so GetActiveContacts() can report colliding body pairs.
        if (!rejected)
            AddActiveContact(bodyA, bodyB);

        // Return true to keep the contact (Aether will destroy it ourselves if rejected).
        return true;
    }

    private void OnWorldEndContact(Contact contact)
    {
        if (_disposed) return;

        var fixtureA = contact.FixtureA;
        var fixtureB = contact.FixtureB;

        // Resolve owner bodies for each fixture.
        if (!_physicsBodies.TryGetValue(fixtureA.Body, out var bodyA)) return;
        if (!_physicsBodies.TryGetValue(fixtureB.Body, out var bodyB)) return;

        // Both wrappers must still be alive.
        if (bodyA is not PhysicsBody pbA || bodyB is not PhysicsBody pbB) return;
        if (pbA._body == null || pbB._body == null) return;

        // Notify both bodies of the separation.
        pbA.RaiseOnSeparation(bodyB);
        pbB.RaiseOnSeparation(bodyA);

        // Resolve fixtures to colliders and notify per-collider events.
        var concreteColliderA = GetColliderFor(pbA, fixtureA) as Collider;
        var concreteColliderB = GetColliderFor(pbB, fixtureB) as Collider;
        if (concreteColliderA != null && concreteColliderB != null)
        {
            concreteColliderA.RaiseOnSeparation(concreteColliderB);
            concreteColliderB.RaiseOnSeparation(concreteColliderA);
        }

        // Clear the active-contact record for this body pair.
        RemoveActiveContact(bodyA, bodyB);
    }

    /// <summary>
    /// Finds the Collider wrapper for a given Aether fixture on a body.
    /// </summary>
    private static ICollider? GetColliderFor(PhysicsBody body, nkast.Aether.Physics2D.Dynamics.Fixture fixture)
    {
        // Iterate through the body's colliders to find the one wrapping this fixture.
        foreach (var collider in body.Colliders)
        {
            var concrete = collider as Collider;
            if (concrete != null && concrete._aetherFixture == fixture)
                return collider;
        }
        return null;
    }

    /// <summary>
    /// Gets the body pairs that are currently in active contact, as tracked from the
    /// physics world's BeginContact/EndContact callbacks.
    /// </summary>
    /// <returns>An immutable collection of the currently colliding body pairs.</returns>
    public IReadOnlyCollection<BodyContactPair> GetActiveContacts()
    {
        var result = new List<BodyContactPair>(_activeContacts.Count);
        foreach (var kvp in _activeContacts)
        {
            if (kvp.Value > 0)
                result.Add(new BodyContactPair(kvp.Key.A, kvp.Key.B));
        }
        return result;
    }

    private void AddActiveContact(IPhysicsBody a, IPhysicsBody b)
    {
        var key = ContactKey.Create(a, b);
        _activeContacts[key] = _activeContacts.GetValueOrDefault(key) + 1;
    }

    private void RemoveActiveContact(IPhysicsBody a, IPhysicsBody b)
    {
        var key = ContactKey.Create(a, b);
        if (!_activeContacts.TryGetValue(key, out int count))
            return;

        count--;
        if (count <= 0)
            _activeContacts.Remove(key);
        else
            _activeContacts[key] = count;
    }

    private void RemoveContactsForBody(IPhysicsBody body)
    {
        foreach (var key in _activeContacts.Keys.ToList())
        {
            if (ReferenceEquals(key.A, body) || ReferenceEquals(key.B, body))
                _activeContacts.Remove(key);
        }
    }

    /// <summary>
    /// A normalized, order-independent key for an unordered physics body contact pair.
    /// </summary>
    private readonly struct ContactKey : IEquatable<ContactKey>
    {
        public readonly IPhysicsBody A;
        public readonly IPhysicsBody B;

        private ContactKey(IPhysicsBody a, IPhysicsBody b)
        {
            A = a;
            B = b;
        }

        public static ContactKey Create(IPhysicsBody x, IPhysicsBody y)
        {
            int hx = System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(x);
            int hy = System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(y);
            return hx <= hy ? new ContactKey(x, y) : new ContactKey(y, x);
        }

        public bool Equals(ContactKey other)
            => ReferenceEquals(A, other.A) && ReferenceEquals(B, other.B);

        public override bool Equals(object? obj) => obj is ContactKey other && Equals(other);

        public override int GetHashCode()
            => System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(A)
               ^ System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(B);
    }

    #endregion

    /// <summary>
    /// Gets or sets the global gravity vector. Default is (0, -9.81) m/s².
    /// </summary>
    public Vector2 Gravity
    {
        get => _world.Gravity;
        set => _world.Gravity = value;
    }

    /// <summary>
    /// Gets the underlying Aether <see cref="World"/>.
    /// <para>
    /// <b>Internal only:</b> exposed so the Aether-specific <c>PhysicsDebugRenderer</c>
    /// (same assembly) can drive Aether's built-in <c>DebugView</c>. This is deliberately
    /// <c>internal</c> so the engine-agnostic public API never leaks Aether types.
    /// </para>
    /// </summary>
    internal World AetherWorld => _world;

    #region Body Creation

    /// <summary>
    /// Creates a dynamic body at the given position. Dynamic bodies are affected by forces and collisions.
    /// </summary>
    /// <param name="position">World-space position for the new body.</param>
    /// <returns>An IPhysicsBody wrapper around the created Aether body.</returns>
    public IPhysicsBody CreateDynamic(Vector2 position)
        => CreateBody(position, BodyType.Dynamic);

    /// <summary>
    /// Creates a static body at the given position. Static bodies are immovable and never affected by forces.
    /// </summary>
    /// <param name="position">World-space position for the new body.</param>
    /// <returns>An IPhysicsBody wrapper around the created Aether body.</returns>
    public IPhysicsBody CreateStatic(Vector2 position)
        => CreateBody(position, BodyType.Static);

    /// <summary>
    /// Creates a kinematic body at the given position. Kinematic bodies move but are not affected by forces/collisions (they push others).
    /// </summary>
    /// <param name="position">World-space position for the new body.</param>
    /// <returns>An IPhysicsBody wrapper around the created Aether body.</returns>
    public IPhysicsBody CreateKinematic(Vector2 position)
        => CreateBody(position, BodyType.Kinematic);

    private IPhysicsBody CreateBody(Vector2 position, BodyType bodyType)
    {
        if (_disposed) throw new ObjectDisposedException(nameof(PhysicsEngine));
        if (_world.IsLocked)
            throw new InvalidOperationException("Cannot create bodies while the world is stepping.");

        // Try pool first before allocating a new body.
        var aetherBody = _bodyPool.Count > 0
            ? _bodyPool.Dequeue()
            : null;

        PhysicsBody wrapper;
        if (aetherBody != null)
        {
            // Recycle: reset position, type, and clear dynamics.
            aetherBody.Enabled = true;
            aetherBody.Position = position;
            aetherBody.BodyType = bodyType;
            aetherBody.Rotation = 0f;
            aetherBody.ResetDynamics();

            // Remove all old fixtures (they'll be re-added by the user).
            foreach (var fixture in aetherBody.FixtureList.ToArray())
                aetherBody.Remove(fixture);

            wrapper = new PhysicsBody(_world, aetherBody);
        }
        else
        {
            // Allocate fresh.
            aetherBody = _world.CreateBody(position, rotation: 0f, bodyType);
            wrapper = new PhysicsBody(_world, aetherBody);
        }

        _physicsBodies[aetherBody] = wrapper;
        return wrapper;
    }

    #endregion

    #region Body Removal

    /// <summary>
    /// Destroys the given body by removing it from the simulation and recycling it into the pool for reuse.
    /// All fixtures attached to it are also removed. The Aether Body stays in the world (disabled) so it can be re-enabled on next create.
    /// </summary>
    /// <param name="body">The physics body to recycle.</param>
    public void Destroy(IPhysicsBody body)
    {
        if (body is null) return;

        var pb = body as PhysicsBody;
        var aetherBody = pb?._body;
        if (aetherBody == null) return;

        // Remove all fixtures.
        foreach (var fixture in aetherBody.FixtureList.ToArray())
            aetherBody.Remove(fixture);

        _physicsBodies.Remove(aetherBody);

        // Drop any tracked active contacts involving this body.
        RemoveContactsForBody(body);

        // Disable and reset — keep body in world so it can be re-enabled from the pool.
        // This is the same pattern as the old WorldPool.cs: disabled bodies are not simulated.
        aetherBody.Enabled = false;
        aetherBody.ResetDynamics();
        _bodyPool.Enqueue(aetherBody);

        pb!._body = null;
    }

    #endregion

    #region Time Step (IFixedUpdateGameSystem)

    /// <summary>
    /// Steps the physics simulation forward by the fixed time delta.
    /// Called automatically by CoreEssentials' game loop via IFixedUpdateGameSystem.
    /// </summary>
    /// <param name="gameTime">Provides timing info (gameTime.ElapsedGameTime.TotalSeconds is used as dt).</param>
    public void FixedUpdate(GameTime gameTime)
    {
        float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
        if (dt <= 0f || !_world.Enabled) return;

        var iterations = new SolverIterations();
        iterations.PositionIterations = PositionIterations;
        iterations.VelocityIterations = VelocityIterations;

        _world.Step(dt, ref iterations);
    }

    #endregion

    #region Query Methods

    /// <summary>
    /// Tests whether the given world-space point is inside any fixture.
    /// </summary>
    /// <param name="point">Point in world coordinates.</param>
    /// <returns>The first fixture containing the point, or null if none found.</returns>
    public ICollider? TestPoint(Vector2 point)
    {
        var aetherFixture = _world.TestPoint(point);
        return aetherFixture != null ? GetWrapperFor(aetherFixture) : null;
    }

    #endregion

    #region IPhysicsWorld Implementation

    /// <inheritdoc/>
    public void AddBody(IPhysicsBody body)
    {
        if (body is null) return;
        var pb = body as PhysicsBody;
        var aetherBody = pb?._body;
        if (aetherBody == null) return;
        _physicsBodies[aetherBody] = pb!;
    }

    /// <inheritdoc/>
    public void RemoveBody(IPhysicsBody body)
    {
        if (body is null) return;
        var pb = body as PhysicsBody;
        var aetherBody = pb?._body;
        if (aetherBody == null) return;
        _physicsBodies.Remove(aetherBody);
        RemoveContactsForBody(body);
    }

    /// <inheritdoc/>
    public void ClearAllBodies()
    {
        foreach (var aetherBody in _physicsBodies.Keys.ToList())
        {
            foreach (var fixture in aetherBody.FixtureList.ToArray())
                aetherBody.Remove(fixture);
            aetherBody.Enabled = false;
            aetherBody.ResetDynamics();
            _bodyPool.Enqueue(aetherBody);
        }

        _physicsBodies.Clear();
        _activeContacts.Clear();
    }

    /// <inheritdoc/>
    public void Step(float deltaTime, SolverConfig? solverConfig = null)
    {
        if (deltaTime <= 0f || !_world.Enabled) return;
        var iterations = new SolverIterations();
        iterations.PositionIterations = PositionIterations;
        iterations.VelocityIterations = VelocityIterations;
        _world.Step(deltaTime, ref iterations);
    }

    #endregion

    /// <summary>
    /// Retrieves or creates the IFixture wrapper for an Aether fixture.
    /// </summary>
    private ICollider? GetWrapperFor(nkast.Aether.Physics2D.Dynamics.Fixture aetherFixture)
    {
        if (aetherFixture.Body != null && _physicsBodies.TryGetValue(aetherFixture.Body, out var owner))
            return new Collider(_world, aetherFixture, owner);
        return null;
    }
}
