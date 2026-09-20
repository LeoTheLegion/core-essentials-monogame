using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using CoreEssentials.Assets;
using CoreEssentials.GameSystems;
using Microsoft.Xna.Framework;

namespace CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Serialization;

/// <summary>
/// Strict parser for the self-describing scene file format. The root must be &lt;Scene&gt;
/// containing a single &lt;GameSystems&gt; element; every prefab registration and entity
/// definition lives inside a &lt;System&gt;. Unknown elements or attributes are parse errors
/// that name the offending element, so typos fail fast instead of being silently ignored.
/// </summary>
public static class SceneParser
{
    /// <summary>Built-in system short names mapped to their concrete types.</summary>
    private static readonly Dictionary<string, Type> BuiltInSystems = new(StringComparer.OrdinalIgnoreCase)
    {
        ["EntitySystem"] = typeof(EntitySystem),
        ["PhysicsEngine"] = typeof(CoreEssentials.GameSystems.Physics.Engines.Aether.PhysicsEngine)
    };

    /// <summary>Attributes allowed directly on an &lt;EntityDefinition&gt; element.</summary>
    private static readonly HashSet<string> EntityDefinitionAttributes = new(StringComparer.OrdinalIgnoreCase)
    {
        "Type", "Source", "Id", "Rotation", "Sort", "Active"
    };

    /// <summary>The attribute name that carries a property's value in the scene format.</summary>
    private const string ValueAttribute = "Value";

    /// <summary>The attribute name that carries a named item (tag/property) in the scene format.</summary>
    private const string NameAttribute = "Name";

    /// <summary>Parses a scene definition from an XML string.</summary>
    /// <exception cref="FormatException">Thrown when the document violates the scene schema.</exception>
    public static SceneDefinition Parse(string xmlData)
    {
        XDocument doc;
        try
        {
            doc = XDocument.Parse(xmlData);
        }
        catch (System.Xml.XmlException ex)
        {
            throw new FormatException($"Scene XML is malformed: {ex.Message}", ex);
        }

        var root = doc.Root ?? throw new FormatException("Scene XML has no root element.");
        ExpectElementName(root, "Scene");
        RejectUnknownAttributes(root, EmptySet);

        var scene = new SceneDefinition();
        var gameSystemsElements = root.Elements().ToList();
        if (gameSystemsElements.Count != 1 || gameSystemsElements[0].Name.LocalName != "GameSystems")
            throw new FormatException("<Scene> must contain exactly one <GameSystems> element.");

        var gameSystemsElement = gameSystemsElements[0];
        RejectUnknownAttributes(gameSystemsElement, EmptySet);
        foreach (var child in gameSystemsElement.Elements())
            ParseSystem(child, scene.Systems);

        ValidateUniqueIds(scene);
        return scene;
    }

    /// <summary>Parses a scene definition from an XML asset (e.g. "MyScene.xml").</summary>
    public static SceneDefinition LoadFromAsset(string assetName)
    {
        var xmlAsset = AssetManager.LoadAsset<XMLAsset>(assetName);
        if (xmlAsset.XMLContent == null)
            throw new InvalidOperationException($"XML asset '{assetName}' has no content loaded.");

        return Parse(xmlAsset.XMLContent);
    }

    /// <summary>Resolves a &lt;System Type=&gt; name: built-in table first, then a reflection
    /// fallback for custom game systems in any loaded assembly.</summary>
    public static Type ResolveSystemType(string typeName)
    {
        if (BuiltInSystems.TryGetValue(typeName, out var builtIn))
            return builtIn;

        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            try
            {
                var foundType = assembly.GetType(typeName);
                if (foundType != null && typeof(GameSystem).IsAssignableFrom(foundType) && !foundType.IsAbstract)
                    return foundType;

                var candidates = assembly.GetTypes()
                    .Where(t => t.Name.Equals(typeName, StringComparison.OrdinalIgnoreCase)
                        && typeof(GameSystem).IsAssignableFrom(t) && !t.IsAbstract);
                var type = candidates.FirstOrDefault();
                if (type != null) return type;
            }
            catch (ReflectionTypeLoadException ex)
            {
                Console.WriteLine($"[Scene] Failed to load types from assembly: {ex.Message}");
            }
        }

        throw new FormatException(
            $"Could not resolve game system type '{typeName}'. Built-in systems: {string.Join(", ", BuiltInSystems.Keys)}. " +
            "Custom systems must derive from GameSystem and be in a loaded assembly.");
    }

    // ──────────────────────────── Element parsing ────────────────────────────

    private static void ParseSystem(XElement element, List<SystemDefinition> systems)
    {
        ExpectElementName(element, "System");
        RejectUnknownAttributes(element, new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "Type", "Config" });

        var typeName = element.Attribute("Type")!.Value;
        if (string.IsNullOrWhiteSpace(typeName))
            throw new FormatException("<System> is missing its required 'Type' attribute.");

        var configAttr = element.Attribute("Config")?.Value;
        if (configAttr != null && string.IsNullOrWhiteSpace(configAttr))
            throw new FormatException($"<System Type=\"{typeName}\"> has an empty 'Config' attribute.");

        var systemDef = new SystemDefinition
        {
            TypeName = typeName,
            SystemType = ResolveSystemType(typeName),
            ConfigAsset = configAttr
        };

        foreach (var child in element.Elements())
        {
            if (child.Name.LocalName == "Prefabs")
                ParsePrefabs(child, systemDef);
            else if (child.Name.LocalName == "Entities")
                ParseEntities(child, systemDef);
            else
                throw new FormatException($"Unknown element <{child.Name.LocalName}> inside <System Type=\"{typeName}\">. Allowed: Prefabs, Entities.");
        }

        systems.Add(systemDef);
    }

    private static void ParsePrefabs(XElement element, SystemDefinition systemDef)
    {
        RejectUnknownAttributes(element, EmptySet);

        foreach (var prefabElem in element.Elements())
        {
            ExpectElementName(prefabElem, "Prefab");
            RejectUnknownAttributes(prefabElem, new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "Name", "Asset" });

            var name = prefabElem.Attribute("Name")?.Value;
            var asset = prefabElem.Attribute("Asset")?.Value;
            if (string.IsNullOrWhiteSpace(name))
                throw new FormatException("<Prefab> is missing its required 'Name' attribute.");
            if (string.IsNullOrWhiteSpace(asset))
                throw new FormatException($"<Prefab Name=\"{name}\"> is missing its required 'Asset' attribute.");

            if (systemDef.Prefabs.Any(p => string.Equals(p.Name, name, StringComparison.OrdinalIgnoreCase)))
                throw new FormatException($"Duplicate prefab registration '{name}' inside <System Type=\"{systemDef.TypeName}\">.");

            var registration = new PrefabRegistration { Name = name, Asset = asset };
            registration.Prefab = EntityPrefabLoader.LoadFromAsset(asset);
            systemDef.Prefabs.Add(registration);
        }
    }

    private static void ParseEntities(XElement element, SystemDefinition systemDef)
    {
        RejectUnknownAttributes(element, EmptySet);

        foreach (var child in element.Elements())
        {
            if (child.Name.LocalName != "EntityDefinition")
                throw new FormatException($"Unknown element <{child.Name.LocalName}> inside <Entities>. Expected <EntityDefinition>.");

            var prefabByName = systemDef.Prefabs.ToDictionary(p => p.Name, StringComparer.OrdinalIgnoreCase);
            ParseEntityDefinition(child, systemDef, prefabByName, systemDef.Entities);
        }
    }

    private static void ParseEntityDefinition(XElement element, SystemDefinition systemDef, Dictionary<string, PrefabRegistration> prefabByName, List<EntityDefinition> siblings)
    {
        ExpectElementName(element, "EntityDefinition");

        var typeAttr = element.Attribute("Type")?.Value;
        var sourceAttr = element.Attribute("Source")?.Value;
        var hasType = !string.IsNullOrWhiteSpace(typeAttr);
        var hasSource = !string.IsNullOrWhiteSpace(sourceAttr);

        if (hasType && hasSource)
            throw new FormatException($"EntityDefinition '{Describe(element)}' sets both 'Type' and 'Source' — exactly one is allowed.");
        if (!hasType && !hasSource)
            throw new FormatException($"EntityDefinition '{Describe(element)}' must set either 'Type' or 'Source'.");

        // Any attribute beyond the known set is a flat-attribute override; it must resolve to a
        // single writable component property or parsing fails (validated below).
        var definition = new EntityDefinition
        {
            Type = hasType ? typeAttr! : null,
            Source = hasSource ? sourceAttr! : null,
            Id = element.Attribute("Id")?.Value,
            Rotation = ParseFloat(element.Attribute("Rotation")?.Value),
            Sort = ParseInt(element.Attribute("Sort")?.Value),
            Active = ParseBool(element.Attribute("Active")?.Value)
        };

        if (hasType && EntityPrefabLoader.ResolveEntityType(typeAttr!) == null)
            throw new FormatException($"EntityDefinition '{Describe(element)}' references unresolvable entity type '{typeAttr}'.");

        Prefab? sourcePrefab = null;
        if (hasSource)
        {
            if (!prefabByName.TryGetValue(sourceAttr!, out var registration))
                throw new FormatException($"EntityDefinition '{Describe(element)}' references prefab '{sourceAttr}' which is not registered in <System Type=\"{systemDef.TypeName}\">.");
            sourcePrefab = registration.Prefab;
        }

        ParseDefinitionChildren(element, systemDef, prefabByName, definition);
        ApplyFlatAttributeOverrides(element, definition, sourcePrefab);

        siblings.Add(definition);
    }

    /// <summary>Parses the child elements of an &lt;EntityDefinition&gt; into the definition.</summary>
    private static void ParseDefinitionChildren(XElement element, SystemDefinition systemDef, Dictionary<string, PrefabRegistration> prefabByName, EntityDefinition definition)
    {
        foreach (var child in element.Elements())
        {
            switch (child.Name.LocalName)
            {
                case "Position":
                    definition.Position = ParseVector2(child);
                    break;
                case "Tags":
                    ParseTags(child, definition, element);
                    break;
                case "Components":
                    ParseComponentsElement(child, definition);
                    break;
                case "Overrides":
                    ParsePreciseOverrides(child, definition, element);
                    break;
                case "EntityOverrides":
                    ParseEntityOverrides(child, definition, element);
                    break;
                case "Children":
                    ParseChildren(child, systemDef, prefabByName, definition.Children);
                    break;
                case "Bind":
                    definition.Binds.Add(child);
                    break;
                case "References":
                    foreach (var reference in child.Elements())
                    {
                        ExpectElementName(reference, "Reference");
                        RejectUnknownAttributes(reference, new HashSet<string>(StringComparer.OrdinalIgnoreCase) { NameAttribute, "TargetId" });
                        definition.References.Add(reference);
                    }
                    break;
                default:
                    throw new FormatException($"Unknown element <{child.Name.LocalName}> inside EntityDefinition '{Describe(element)}'. " +
                        "Allowed: Position, Tags, Components, Overrides, EntityOverrides, Bind, References, Children.");
            }
        }
    }

    /// <summary>Applies flat attributes (anything beyond the known set) as per-component property overrides.</summary>
    private static void ApplyFlatAttributeOverrides(XElement element, EntityDefinition definition, Prefab? sourcePrefab)
    {
        foreach (var attr in element.Attributes().Where(a => !EntityDefinitionAttributes.Contains(a.Name.LocalName)))
        {
            var name = attr.Name.LocalName;
            definition.FlatOverrides[name] = attr.Value;
            ResolveFlatOverride(definition, name, attr.Value, sourcePrefab, element);
        }
    }

    private static void ParseChildren(XElement element, SystemDefinition systemDef, Dictionary<string, PrefabRegistration> prefabByName, List<EntityDefinition> children)
    {
        RejectUnknownAttributes(element, EmptySet);

        foreach (var child in element.Elements())
        {
            if (child.Name.LocalName != "EntityDefinition")
                throw new FormatException($"Unknown element <{child.Name.LocalName}> inside <Children>. Expected <EntityDefinition>.");

            ParseEntityDefinition(child, systemDef, prefabByName, children);
        }
    }

    private static void ParseTags(XElement element, EntityDefinition definition, XElement context)
    {
        RejectUnknownAttributes(element, EmptySet);

        foreach (var tag in element.Elements())
        {
            ExpectElementName(tag, "Tag");
            RejectUnknownAttributes(tag, new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "Name" });

            var name = tag.Attribute("Name")?.Value;
            if (string.IsNullOrWhiteSpace(name))
                throw new FormatException($"<Tag> inside EntityDefinition '{Describe(context)}' is missing its required 'Name' attribute.");
            definition.Tags.Add(name);
        }
    }

    /// <summary>Parses the full component definitions (type + properties) declared in a
    /// &lt;Components&gt; element and captures any &lt;Bind&gt; elements nested inside it.</summary>
    private static void ParseComponentsElement(XElement componentsElement, EntityDefinition definition)
    {
        RejectUnknownAttributes(componentsElement, EmptySet);

        foreach (var child in componentsElement.Elements())
        {
            if (child.Name.LocalName == "Component")
                ParseComponentDefinition(child, definition);
            else if (child.Name.LocalName == "Bind")
                definition.Binds.Add(child);
            else
                throw new FormatException($"Unknown element <{child.Name.LocalName}> inside <Components>. Expected <Component> or <Bind>.");
        }
    }

    /// <summary>Parses a single &lt;Component&gt; child (type + properties) of a &lt;Components&gt; element.</summary>
    private static void ParseComponentDefinition(XElement componentElement, EntityDefinition definition)
    {
        var typeName = componentElement.Attribute("Type")?.Value;
        if (string.IsNullOrWhiteSpace(typeName))
            throw new FormatException("<Component> inside <Components> is missing its required 'Type' attribute.");

        RejectUnknownAttributes(componentElement, new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "Type" });
        var compDef = new Prefab.ComponentDefinition { Type = typeName };

        foreach (var child in componentElement.Elements())
        {
            switch (child.Name.LocalName)
            {
                case "Properties":
                    ParseComponentProperties(child, typeName, compDef);
                    break;
                case "EffectParameter":
                    RejectUnknownAttributes(child, new HashSet<string>(StringComparer.OrdinalIgnoreCase) { NameAttribute, ValueAttribute });

                    var effectParamName = child.Attribute(NameAttribute)?.Value;
                    if (string.IsNullOrWhiteSpace(effectParamName))
                        throw new FormatException($"<EffectParameter> inside <Component Type=\"{typeName}\"> is missing its required 'Name' attribute.");
                    compDef.EffectParameters[effectParamName] = child.Attribute(ValueAttribute)?.Value ?? string.Empty;
                    break;
                default:
                    throw new FormatException($"Unknown element <{child.Name.LocalName}> inside <Component Type=\"{typeName}\">. Expected <Properties> or <EffectParameter>.");
            }
        }

        definition.DeclaredComponents.Add(compDef);
    }

    /// <summary>Parses the &lt;Properties&gt; child of a &lt;Component&gt; into its property bag.</summary>
    private static void ParseComponentProperties(XElement propertiesElement, string typeName, Prefab.ComponentDefinition compDef)
    {
        foreach (var prop in propertiesElement.Elements())
        {
            ExpectElementName(prop, "Property");
            RejectUnknownAttributes(prop, new HashSet<string>(StringComparer.OrdinalIgnoreCase) { NameAttribute, ValueAttribute });

            var name = prop.Attribute(NameAttribute)?.Value;
            if (string.IsNullOrWhiteSpace(name))
                throw new FormatException($"<Property> inside <Component Type=\"{typeName}\"> is missing its required 'Name' attribute.");
            compDef.Properties[name] = prop.Attribute(ValueAttribute)?.Value ?? string.Empty;
        }
    }

    private static void ParsePreciseOverrides(XElement element, EntityDefinition definition, XElement context)
    {
        RejectUnknownAttributes(element, EmptySet);

        foreach (var component in element.Elements())
        {
            ExpectElementName(component, "Component");
            RejectUnknownAttributes(component, new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "Type" });

            var typeName = component.Attribute("Type")?.Value;
            if (string.IsNullOrWhiteSpace(typeName))
                throw new FormatException($"<Component> inside <Overrides> on EntityDefinition '{Describe(context)}' is missing its required 'Type' attribute.");

            var properties = new Dictionary<string, string>(StringComparer.Ordinal);
            foreach (var prop in component.Elements())
            {
                ExpectElementName(prop, "Property");
                RejectUnknownAttributes(prop, new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "Name", "Value" });

                var name = prop.Attribute(NameAttribute)?.Value;
                var value = prop.Attribute(ValueAttribute)?.Value;
                if (string.IsNullOrWhiteSpace(name))
                    throw new FormatException($"<Property> inside <Overrides> on EntityDefinition '{Describe(context)}' is missing its required 'Name' attribute.");
                properties[name] = value ?? string.Empty;
            }

            definition.ResolvedOverrides[typeName] = properties;
        }
    }

    /// <summary>
    /// Parses the &lt;EntityOverrides&gt; element — a flat set of property → value pairs targeting
    /// writable public properties on the entity itself (not a component). This is the escape hatch for
    /// entities that keep state directly on themselves (e.g. an entity's own <c>CameraSpeed</c>) with no
    /// component to target via &lt;Overrides&gt;. Values are applied to the created entity before <c>OnStart</c>/<c>OnAttach</c>.
    /// </summary>
    private static void ParseEntityOverrides(XElement element, EntityDefinition definition, XElement context)
    {
        RejectUnknownAttributes(element, EmptySet);

        foreach (var prop in element.Elements())
        {
            ExpectElementName(prop, "Property");
            RejectUnknownAttributes(prop, new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "Name", "Value" });

            var name = prop.Attribute("Name")?.Value;
            if (string.IsNullOrWhiteSpace(name))
                throw new FormatException($"<Property> inside <EntityOverrides> on EntityDefinition '{Describe(context)}' is missing its required 'Name' attribute.");

            definition.EntityOverrides[name] = prop.Attribute(ValueAttribute)?.Value ?? string.Empty;
        }
    }

    // ──────────────────── Flat-attribute override resolution ────────────────────

    /// <summary>
    /// Resolves a flat attribute (e.g. <c>Text="Score: 100"</c>) to the single component that
    /// exposes a writable property with that name, and records it in
    /// <see cref="EntityDefinition.ResolvedOverrides"/>. For prefab instances the source prefab's
    /// components are searched; for plain class definitions the declared &lt;Components&gt; are used.
    /// A name matching zero or multiple components is a parse error — use the precise
    /// &lt;Overrides&gt; form to disambiguate.
    /// </summary>
    private static void ResolveFlatOverride(EntityDefinition definition, string propertyName, string value, Prefab? sourcePrefab, XElement context)
    {
        IEnumerable<string> candidateTypeNames = sourcePrefab != null
            ? sourcePrefab.Components.Select(c => c.Type)
            : definition.DeclaredComponents.Select(c => c.Type);

        var matches = candidateTypeNames
            .Select(typeName => EntityPrefabLoader.ResolveComponentType(typeName))
            .Where(componentType => componentType != null
                && componentType.GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public)?.CanWrite == true)
            .Cast<Type>()
            .ToList();

        if (matches.Count == 0)
            throw new FormatException(
                $"EntityDefinition '{Describe(context)}' has flat attribute '{propertyName}' which does not match any " +
                $"writable property on its components. Use <Overrides> for precise wiring.");

        if (matches.Count > 1)
            throw new FormatException(
                $"Flat attribute '{propertyName}' on EntityDefinition '{Describe(context)}' is ambiguous — it matches a " +
                $"writable property on {string.Join(" and ", matches.Select(m => m.Name))}. Use <Overrides> to target a specific component.");

        var resolvedType = matches[0];
        var typeName = resolvedType.FullName
            ?? throw new InvalidOperationException($"Resolved component type has no full name for flat attribute '{propertyName}'.");
        if (!definition.ResolvedOverrides.TryGetValue(typeName, out var properties))
            definition.ResolvedOverrides[typeName] = properties = new Dictionary<string, string>(StringComparer.Ordinal);
        properties[propertyName] = value;
    }

    // ──────────────────────────── Strictness helpers ────────────────────────────

    private static readonly HashSet<string> EmptySet = new(StringComparer.OrdinalIgnoreCase);

    private static void ExpectElementName(XElement element, string expected)
    {
        if (!string.Equals(element.Name.LocalName, expected, StringComparison.Ordinal))
            throw new FormatException($"Expected <{expected}> but found <{element.Name.LocalName}>.");
    }

    private static void RejectUnknownAttributes(XElement element, HashSet<string> allowed)
    {
        var unknown = element.Attributes().FirstOrDefault(a => !allowed.Contains(a.Name.LocalName));
        if (unknown != null)
            throw new FormatException($"Unknown attribute '{unknown.Name.LocalName}' on <{element.Name.LocalName}>. " +
                $"Allowed: {(allowed.Count == 0 ? "(none)" : string.Join(", ", allowed.OrderBy(n => n, StringComparer.Ordinal)))}.");
    }

    private static Vector2 ParseVector2(XElement element)
    {
        RejectUnknownAttributes(element, new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "X", "Y" });
        return new Vector2(
            ParseFloat(element.Attribute("X")?.Value) ?? 0f,
            ParseFloat(element.Attribute("Y")?.Value) ?? 0f);
    }

    private static float? ParseFloat(string? raw)
        => raw == null ? null : float.Parse(raw, NumberStyles.Any, CultureInfo.InvariantCulture);

    private static int? ParseInt(string? raw)
        => raw == null ? null : int.Parse(raw, NumberStyles.Any, CultureInfo.InvariantCulture);

    private static bool? ParseBool(string? raw)
        => raw == null ? null : bool.Parse(raw);

    /// <summary>Human-readable identifier for error messages: Id when present, otherwise Type/Source.</summary>
    private static string Describe(XElement? element)
    {
        if (element == null) return "(unknown)";
        var id = element.Attribute("Id")?.Value;
        if (!string.IsNullOrWhiteSpace(id)) return $"Id=\"{id}\"";
        var type = element.Attribute("Type")?.Value;
        if (!string.IsNullOrWhiteSpace(type)) return $"Type=\"{type}\"";
        var source = element.Attribute("Source")?.Value;
        if (!string.IsNullOrWhiteSpace(source)) return $"Source=\"{source}\"";
        return "(unnamed)";
    }

    /// <summary>Ensures every Id in the scene is unique (including nested children).</summary>
    private static void ValidateUniqueIds(SceneDefinition scene)
    {
        var seen = new HashSet<string>(StringComparer.Ordinal);
        foreach (var system in scene.Systems)
            CollectIds(system.Entities, seen);
    }

    private static void CollectIds(List<EntityDefinition> definitions, HashSet<string> seen)
    {
        foreach (var definition in definitions)
        {
            if (!string.IsNullOrWhiteSpace(definition.Id) && !seen.Add(definition.Id))
                throw new FormatException($"Duplicate entity Id '{definition.Id}' in scene — Ids must be unique.");

            CollectIds(definition.Children, seen);
        }
    }
}
