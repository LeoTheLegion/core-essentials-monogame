using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using nkast.Aether.Physics2D.Dynamics;

namespace CoreEssentials.Physics.Adapters.Interfaces;

/// <summary>
/// Represents the solver configuration for the physics world.
/// These settings control how the physics simulation resolves collisions.
/// </summary>
public class SolverConfig
{
    /// <summary>
    /// Number of iterations for velocity constraint solving.
    /// Higher values improve stability but increase computation time.
    /// Recommended range: 4-10 (default: 8)
    /// </summary>
    public int VelocityIterations { get; set; } = 8;

    /// <summary>
    /// Number of iterations for position constraint solving.
    /// Higher values improve positional accuracy but increase computation time.
    /// Recommended range: 2-4 (default: 3)
    /// </summary>
    public int PositionIterations { get; set; } = 3;

    /// <summary>
    /// Total iterations for Time Of Impact (TOI) velocity solving.
    /// Used when continuous collision detection is enabled.
    /// </summary>
    public int TOIVelocityIterations => VelocityIterations;

    /// <summary>
    /// Total iterations for Time Of Impact (TOI) position solving.
    /// Used when continuous collision detection is enabled.
    /// </summary>
    public int TOIPositionIterations => PositionIterations * 2;
}

/// <summary>
/// Interface representing the physics world/simulation environment.
/// This abstracts away the underlying Aether Physics2D World implementation,
/// allowing for future engine swapping without breaking user code.
/// </summary>
public interface IPhysicsWorldAdapter : IDisposable
{
    /// <summary>
    /// Gets or sets the gravity vector applied to all dynamic bodies.
    /// </summary>
    Vector2 Gravity { get; set; }

    /// <summary>
    /// Gets a collection of all active bodies in this world.
    /// </summary>
    IEnumerable<IPhysicsBodyAdapter> Bodies { get; }

    /// <summary>
    /// Gets the solver configuration for collision resolution.
    /// </summary>
    SolverConfig SolverIterations { get; }

    /// <summary>
    /// Creates a new physics body at the specified position and rotation.
    /// </summary>
    /// <param name="position">World-space position of the body.</param>
    /// <param name="rotation">Initial rotation in radians.</param>
    /// <param name="bodyType">The type of body (static, dynamic, or kinematic).</param>
    /// <returns>The created physics body adapter.</returns>
    IPhysicsBodyAdapter CreateBody(Vector2 position, float rotation, BodyType bodyType);

    /// <summary>
    /// Applies a force to a body at a specific world-space point.
    /// </summary>
    /// <param name="body">The target body.</param>
    /// <param name="position">World-space point where force is applied.</param>
    /// <param name="force">Force vector to apply.</param>
    void ApplyForce(IPhysicsBodyAdapter body, Vector2 position, Vector2 force);

    /// <summary>
    /// Applies a torque (rotational force) directly to a body.
    /// </summary>
    /// <param name="body">The target body.</param>
    /// <param name="torque">Torque value to apply.</param>
    void ApplyTorque(IPhysicsBodyAdapter body, float torque);

    /// <summary>
    /// Steps the physics simulation forward by one frame.
    /// This must be called during game updates (e.g., in FixedUpdate).
    /// </summary>
    /// <param name="deltaTime">Time elapsed since last step in seconds.</param>
    /// <param name="solverConfig">Configuration for collision resolution.</param>
    void Step(float deltaTime, SolverConfig solverConfig);

    /// <summary>
    /// Clears all bodies and fixtures from the world.
    /// </summary>
    void Clear();
}
