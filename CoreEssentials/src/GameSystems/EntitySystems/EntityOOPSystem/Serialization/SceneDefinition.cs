using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Serialization;

/// <summary>
/// Parsed, in-memory representation of a self-describing scene file.
/// A scene declares which game systems it needs and, per system, the prefab
/// registrations and entity definitions that make up the scene's content.
/// Entities and prefabs only ever exist inside a &lt;System&gt; — invalid states are unwriteable.
/// </summary>
public class SceneDefinition
{
    /// <summary>The game systems declared by this scene, in document order.</summary>
    public List<SystemDefinition> Systems { get; } = new();
}

/// <summary>
/// A single &lt;System&gt; entry inside a scene. Carries the resolved system type plus,
/// for entity systems, the prefab registrations and entity definitions it owns.
/// </summary>
public class SystemDefinition
{
    /// <summary>The short or fully-qualified name of the system as written in the file.</summary>
    public string TypeName { get; init; } = string.Empty;

    /// <summary>The concrete <see cref="CoreEssentials.GameSystems.GameSystem"/> type resolved from <see cref="TypeName"/>.</summary>
    public Type SystemType { get; init; } = typeof(object);

    /// <summary>Prefab registrations owned by this system (name → asset file).</summary>
    public List<PrefabRegistration> Prefabs { get; } = new();

    /// <summary>Entity definitions owned by this system, in document order.</summary>
    public List<EntityDefinition> Entities { get; } = new();
}

/// <summary>
/// A &lt;Prefab Name= Asset=/&gt; registration: binds a scene-local name to an XML prefab asset.
/// </summary>
public class PrefabRegistration
{
    /// <summary>The scene-local name used by entity definitions' <c>Source</c> attribute.</summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>The XML prefab asset this registration points at (e.g. "TextTemplate.xml").</summary>
    public string Asset { get; init; } = string.Empty;

    /// <summary>The parsed prefab, loaded from <see cref="Asset"/> when the scene is parsed.</summary>
    public Prefab? Prefab { get; set; }
}

/// <summary>
/// A single &lt;EntityDefinition&gt; inside a system's &lt;Entities&gt; element.
/// Exactly one of <see cref="Type"/> or <see cref="Source"/> must be set:
/// <c>Type</c> names the entity class directly, <c>Source</c> references a registered prefab by name.
/// </summary>
public class EntityDefinition
{
    /// <summary>The entity class to create (short or fully-qualified). Mutually exclusive with <see cref="Source"/>.</summary>
    public string? Type { get; init; }

    /// <summary>The name of a registered prefab this definition instantiates. Mutually exclusive with <see cref="Type"/>.</summary>
    public string? Source { get; init; }

    /// <summary>Optional stable identifier, unique within the scene, usable for cross-entity references.</summary>
    public string? Id { get; init; }

    /// <summary>Initial position. Defaults to (0, 0).</summary>
    public Microsoft.Xna.Framework.Vector2 Position { get; set; }

    /// <summary>Initial rotation in degrees. Defaults to 0.</summary>
    public float Rotation { get; init; }

    /// <summary>Render sort order. Defaults to 0.</summary>
    public int Sort { get; init; }

    /// <summary>Whether the entity starts active. Defaults to true.</summary>
    public bool Active { get; init; } = true;

    /// <summary>Tags applied to the entity after creation.</summary>
    public List<string> Tags { get; } = new();

    /// <summary>
    /// Flat-attribute overrides as written on the element (property name → value string),
    /// before resolution against the source prefab's components.
    /// </summary>
    public Dictionary<string, string> FlatOverrides { get; } = new(StringComparer.Ordinal);

    /// <summary>
    /// Final per-instantiation overrides ready to hand to
    /// <see cref="EntitySystem.Instantiate(string, Microsoft.Xna.Framework.Vector2, IReadOnlyDictionary{string, Dictionary{string, string}})"/>:
    /// component type name → property name → value. Flat attributes are resolved into this map at parse time;
    /// the precise &lt;Overrides&gt; form is merged on top. Empty for definitions with no overrides.
    /// </summary>
    public Dictionary<string, Dictionary<string, string>> ResolvedOverrides { get; } = new(StringComparer.Ordinal);

    /// <summary>Nested entity definitions from a &lt;Children&gt; element.</summary>
    public List<EntityDefinition> Children { get; } = new();

    /// <summary>Component type names declared in this definition's &lt;Components&gt; element (in document order).</summary>
    public List<string> DeclaredComponentTypes { get; } = new();

    /// <summary>Declarative &lt;Bind&gt; elements applied to the entity after its components attach.</summary>
    public List<XElement> Binds { get; } = new();

    /// <summary>&lt;Reference Name= TargetId=/&gt; links resolved by Id after all entities exist.</summary>
    public List<XElement> References { get; } = new();
}
