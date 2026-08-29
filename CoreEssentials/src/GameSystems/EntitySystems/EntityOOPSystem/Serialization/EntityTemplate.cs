using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Serialization;

/// <summary>
/// Represents a reusable blueprint for an entity.
/// Templates can be loaded from XML and instantiated multiple times to create identical entities.
/// </summary>
public class EntityTemplate
{
    /// <summary>
    /// The name of the entity type (class name) this template creates.
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Initial tags to be applied to all entities instantiated from this template.
    /// </summary>
    public List<string> Tags { get; set; } = new();

    /// <summary>
    /// The base rotation for the entity.
    /// </summary>
    public float Rotation { get; set; }

    /// <summary>
    /// The sort order for the entity.
    /// </summary>
    public int Sort { get; set; }

    /// <summary>
    /// Whether entities created from this template are active by default.
    /// </summary>
    public bool Active { get; set; } = true;

    /// <summary>
    /// List of components to attach to the entity upon instantiation.
    /// </summary>
    public List<ComponentDefinition> Components { get; set; } = new();

    /// <summary>
    /// Templates for child entities to be created and attached to the parent.
    /// </summary>
    public List<EntityTemplate> Children { get; set; } = new();

    /// <summary>
    /// Declarative &lt;Bind&gt; elements (event-to-command wiring) applied to each entity
    /// instantiated from this template. Populated when the template is parsed from XML.
    /// </summary>
    public List<XElement> Binds { get; set; } = new();

    /// <summary>
    /// Defines a component blueprint, including its type and initial property values.
    /// </summary>
    public class ComponentDefinition
    {
        /// <summary>
        /// The type name of the component.
        /// </summary>
        public string Type { get; set; } = string.Empty;

        /// <summary>
        /// Property names and their corresponding values as strings, to be parsed during instantiation.
        /// </summary>
        public Dictionary<string, string> Properties { get; set; } = new();
    }
}
