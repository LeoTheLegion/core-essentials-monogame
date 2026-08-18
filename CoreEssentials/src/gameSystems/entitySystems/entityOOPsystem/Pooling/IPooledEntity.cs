using System;
using Microsoft.Xna.Framework;

namespace CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Pooling;

/// <summary>
/// Interface for entities that support pooling.
/// Implementing this interface allows entities to be recycled instead of destroyed,
/// reducing garbage collection pressure for high-spawn-rate scenarios (projectiles, particles, effects).
/// </summary>
public interface IPooledEntity
{
    /// <summary>
    /// Resets the entity to its initial state for reuse.
    /// Called when the entity is acquired from the pool.
    /// Override this in derived classes to reset any custom state.
    /// </summary>
    void Reset();

    /// <summary>
    /// Activates the entity at the specified position.
    /// Called by the pool when the entity is acquired.
    /// </summary>
    /// <param name="position">The position to place the entity.</param>
    void Activate(Vector2 position);
}
