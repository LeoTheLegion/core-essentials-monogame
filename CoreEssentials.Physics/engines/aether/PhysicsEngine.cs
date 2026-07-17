using System;
using System.Collections.Generic;
using CoreEssentials.GameSystems;
using CoreEssentials.Physics.Types;
using Microsoft.Xna.Framework;
using nkast.Aether.Physics2D.Collision.Shapes;
using nkast.Aether.Physics2D.Common;
using nkast.Aether.Physics2D.Dynamics;

// Re-export Settings for external use if needed.

namespace CoreEssentials.Physics.Engines.Aether;

/// <summary>
/// ⭐ Main entry point for the physics system. Wraps Aether.World and implements IFixedUpdateGameSystem.
/// Users interact through this GameSystem to create bodies, set gravity, and step the simulation.
/// Bodies are automatically pooled on destroy (recycled instead of GC'd) to reduce allocation pressure.
/// </summary>
public class PhysicsEngine : GameSystem, IFixedUpdateGameSystem, IDisposable
{
    private readonly World _world;
    private bool _disposed;

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

    // Cache of PhysicsBody wrappers keyed by Aether Body — prevents duplicate wrappers.
    private readonly Dictionary<Body, PhysicsBody> _physicsBodies = new();

    // Pool of recycled bodies to reduce GC pressure on frequent create/destroy cycles.
    private readonly Queue<Body> _bodyPool = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="PhysicsEngine"/> class with default gravity (0, -9.81).
    /// Creates an internal Aether World that users never see directly.
    /// </summary>
    public PhysicsEngine()
    {
        _world = new World();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PhysicsEngine"/> class with custom gravity.
    /// </summary>
    /// <param name="gravity">The global gravity vector (e.g., Vector2.Zero for no gravity).</param>
    public PhysicsEngine(Vector2 gravity)
    {
        _world = new World(gravity);
    }

    #region IDisposable

    /// <inheritdoc/>
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        // Clear all bodies, joints, and fixtures from the world.
        _world.Clear();
        _physicsBodies.Clear();
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
    public IFixture? TestPoint(Vector2 point)
    {
        var aetherFixture = _world.TestPoint(point);
        return aetherFixture != null ? GetWrapperFor(aetherFixture) : null;
    }

    #endregion

    /// <summary>
    /// Retrieves or creates the IFixture wrapper for an Aether fixture.
    /// </summary>
    private IFixture? GetWrapperFor(nkast.Aether.Physics2D.Dynamics.Fixture aetherFixture)
    {
        if (aetherFixture.Body != null && _physicsBodies.TryGetValue(aetherFixture.Body, out var owner))
            return new Fixture(_world, aetherFixture, owner);
        return null;
    }
}
