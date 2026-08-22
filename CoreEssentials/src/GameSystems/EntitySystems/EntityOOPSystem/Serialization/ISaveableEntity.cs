using System.Xml.Linq;

namespace CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Serialization;

/// <summary>
/// Opt-in interface for entities that should be included in game state save/load operations.
/// </summary>
/// <remarks>
/// Only entities implementing this interface will be serialized when calling
/// <see cref="GameStateSerializer.SaveState"/>, <see cref="EntitySystem.SaveState"/>, or equivalent.
///
/// Implementing classes are responsible for saving/restoring all state they care about,
/// including position, rotation, scale, tags, and any component-dependent state.
///
/// Example:
/// <code>
/// public class Ball : Entity, ISaveableEntity
/// {
///     public XElement SaveState()
///     {
///         return new XElement("Entity",
///             new XAttribute("Id", Id),
///             new XAttribute("Type", GetType().FullName),
///             new XElement("Position", ...),
///             new XElement("Physics", ...) );
///     }
///
///     public void LoadState(XElement element)
///     {
///         // Restore position, rotation, scale, tags, component state...
///     }
/// }
/// </code>
/// </remarks>
public interface ISaveableEntity
{
    /// <summary>
    /// Saves the current state of the entity to an XML element.
    /// </summary>
    /// <returns>An XML element containing the entity's serialized state.</returns>
    XElement SaveState();

    /// <summary>
    /// Loads the entity's state from an XML element.
    /// </summary>
    /// <param name="element">The XML element containing the saved state.</param>
    void LoadState(XElement element);
}
