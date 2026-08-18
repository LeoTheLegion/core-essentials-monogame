using System.Xml.Linq;

namespace CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Serialization;

/// <summary>
/// Interface for components that can be serialized and deserialized for game state saving.
/// Components implementing this interface can save their state to XML and restore it later.
/// </summary>
public interface ISerializableComponent
{
    /// <summary>
    /// Serializes the component's state to an XML element.
    /// </summary>
    /// <returns>An XML element containing the component's serialized state.</returns>
    XElement SerializeToXml();

    /// <summary>
    /// Deserializes the component's state from an XML element.
    /// </summary>
    /// <param name="element">The XML element containing the component's state.</param>
    void DeserializeFromXml(XElement element);
}
