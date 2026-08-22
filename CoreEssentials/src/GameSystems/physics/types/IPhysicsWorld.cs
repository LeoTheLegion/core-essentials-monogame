using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace CoreEssentials.GameSystems.Physics.Types;

/// <summary>
/// 🔒 Internal use ONLY (completely hidden from users).
/// This world type is managed internally by the PhysicsEngine and should never be exposed in any public API.
/// </summary>
public interface IPhysicsWorld : IDisposable
{
    /// <summary>
    /// Gets or sets the gravity vector applied to all bodies each step.
    /// </summary>
    Vector2 Gravity { get; set; }

    // ─── Body Management ────────────────────────────────────────────────

    /// <summary>
    /// Adds a body to this world for simulation.
    /// </summary>
    void AddBody(IPhysicsBody body);

    /// <summary>
    /// Removes a body from this world (it will no longer be simulated).
    /// </summary>
    void RemoveBody(IPhysicsBody body);

    /// <summary>
    /// Clears all bodies and constraints from the world.
    /// </summary>
    void ClearAllBodies();

    // ─── Simulation Step ────────────────────────────────────────────────

    /// <summary>
    /// Advances the simulation by one frame using the solver configuration.
    /// </summary>
    /// <param name="deltaTime">Time since last step in seconds.</param>
    /// <param name="solverConfig">Solver iterations and options (null uses defaults).</param>
    void Step(float deltaTime, SolverConfig? solverConfig = null);

    // ─── Query Methods ──────────────────────────────────────────────────

    /// <summary>
    /// Gets all bodies currently in this world.
    /// </summary>
    IReadOnlyList<IPhysicsBody> GetBodies();
}
