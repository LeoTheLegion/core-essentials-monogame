using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Serialization;

/// <summary>
/// A type alias kept for one release so existing code referencing the old name keeps compiling.
/// Prefer <see cref="Prefab"/> in new code.
/// </summary>
[Obsolete("Renamed to Prefab. EntityTemplate will be removed in a future release.")]
public class EntityTemplate : Prefab { }

/// <summary>
/// Represents a reusable blueprint for an entity — a prefab.
/// Prefabs can be loaded from XML and instantiated multiple times to create entities.
/// </summary>
public class Prefab
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
    /// Per-instantiation overrides for properties that live on the entity itself (not on a
    /// component), e.g. an entity's own <c>Text</c> or <c>CameraSpeed</c>. Property name → value
    /// string, applied to the created entity via reflection before <c>OnStart</c>/<c>OnAttach</c>.
    /// </summary>
    public Dictionary<string, string> EntityOverrides { get; set; } = new(StringComparer.Ordinal);

    /// <summary>
    /// Prefabs for child entities to be created and attached to the parent.
    /// </summary>
    public List<Prefab> Children { get; set; } = new();

    /// <summary>
    /// Declarative &lt;Bind&gt; elements (event-to-command wiring) applied to each entity
    /// instantiated from this prefab. Populated when the prefab is parsed from XML.
    /// </summary>
    public List<XElement> Binds { get; set; } = new();

    /// <summary>
    /// Creates a deep copy of this prefab. The original is never mutated — per-instantiation
    /// overrides (see <see cref="PrefabOverrides"/>) are merged into a clone.
    /// </summary>
    public Prefab Clone()
    {
        var clone = new Prefab
        {
            Type = Type,
            Rotation = Rotation,
            Sort = Sort,
            Active = Active,
            Tags = new List<string>(Tags),
            Components = Components.Select(c => new ComponentDefinition
            {
                Type = c.Type,
                Properties = new Dictionary<string, string>(c.Properties)
            }).ToList(),
            EntityOverrides = new Dictionary<string, string>(EntityOverrides, StringComparer.Ordinal),
            Children = Children.Select(c => c.Clone()).ToList(),
            Binds = Binds.Select(b => new XElement(b)).ToList() // deep copy — matches the bind-clone pattern in the loader
        };
        return clone;
    }

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
