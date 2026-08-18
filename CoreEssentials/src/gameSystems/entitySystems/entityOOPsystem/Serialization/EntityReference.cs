using System;
using System.Collections.Generic;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem;

namespace CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Serialization;

/// <summary>
/// A deferred reference to another entity by ID.
/// Used during XML scene loading when the target entity may not exist yet.
/// The reference is automatically resolved after all entities are loaded.
/// </summary>
public class EntityReference
{
    /// <summary>
    /// The ID of the entity this reference points to.
    /// </summary>
    public string TargetId { get; }

    /// <summary>
    /// Gets whether this reference has been resolved to an actual entity.
    /// </summary>
    public bool IsResolved => ResolvedEntity != null;

    /// <summary>
    /// The resolved entity, or null if the target could not be found.
    /// </summary>
    public Entity? ResolvedEntity { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="EntityReference"/> class.
    /// </summary>
    /// <param name="targetId">The ID of the entity to reference.</param>
    public EntityReference(string targetId)
    {
        TargetId = targetId ?? throw new ArgumentNullException(nameof(targetId));
    }

    /// <summary>
    /// Resolves this reference against the provided entity lookup.
    /// </summary>
    /// <param name="entities">Dictionary mapping entity IDs to entities.</param>
    /// <returns>True if the entity was found; otherwise, false.</returns>
    public bool Resolve(Dictionary<string, Entity> entities)
    {
        if (entities.TryGetValue(TargetId, out var entity))
        {
            ResolvedEntity = entity;
            return true;
        }

        ResolvedEntity = null;
        return false;
    }

    /// <summary>
    /// Gets the resolved entity, throwing if the reference has not been resolved yet.
    /// </summary>
    /// <returns>The resolved entity.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the reference is not resolved or the target was not found.</exception>
    public Entity GetEntity()
    {
        if (ResolvedEntity == null)
            throw new InvalidOperationException(
                $"Entity reference '{TargetId}' has not been resolved. " +
                "Ensure Resolve() is called after all entities are loaded.");

        return ResolvedEntity;
    }

    /// <summary>
    /// Implicitly converts an <see cref="EntityReference"/> to its resolved entity.
    /// </summary>
    /// <param name="reference">The reference to convert.</param>
    /// <returns>The resolved entity.</returns>
    public static implicit operator Entity?(EntityReference reference) => reference.ResolvedEntity;
}
