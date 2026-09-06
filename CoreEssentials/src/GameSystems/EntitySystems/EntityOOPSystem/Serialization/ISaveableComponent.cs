using System.Xml.Linq;

namespace CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Serialization;

/// <summary>
/// Component-level save interface. An entity is saveable iff it has a component implementing this.
/// The component decides **exactly** what to save — typically the owner's transform, tags, and the
/// public properties of whichever sibling components matter for restoring state.
/// </summary>
public interface ISaveableComponent
{
    /// <summary>
    /// Serializes the owner entity's full state to an XML element. The returned element is the
    /// complete <c>&lt;Entity&gt;</c> node (including transform, tags, and any component state).
    /// </summary>
    /// <returns>An XML element containing the entity's serialized state.</returns>
    XElement SaveState();

    /// <summary>
    /// Restores the owner entity's state from a previously saved XML element.
    /// Called by the serializer after the entity has been instantiated from its prefab, so all
    /// components declared in the prefab are already attached and started.
    /// </summary>
    /// <param name="element">The XML element containing the saved state.</param>
    void LoadState(XElement element);
}
