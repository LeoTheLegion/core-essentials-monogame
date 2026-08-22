using System.Collections.Generic;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem;

namespace CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Serialization;

/// <summary>
/// Interface for entities that hold deferred entity references.
/// Implement this interface to have references automatically resolved by <see cref="EntitySystem.ResolveReferences"/>.
/// </summary>
public interface IEntityReferenceHolder
{
    /// <summary>
    /// Resolves all pending entity references against the provided entity lookup.
    /// </summary>
    /// <param name="entities">Dictionary mapping entity IDs to entities.</param>
    /// <returns>The number of successfully resolved references.</returns>
    int ResolveReferences(Dictionary<string, Entity> entities);
}
