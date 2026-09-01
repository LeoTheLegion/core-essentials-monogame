using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Globalization;
using Microsoft.Xna.Framework;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Components;
using CoreEssentials.Assets;
using System.Reflection;

namespace CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Serialization;

/// <summary>
/// Logic for parsing prefabs from XML and instantiating them into Entities.
/// </summary>
public static class EntityPrefabLoader
{
    /// <summary>
    /// Loads a prefab from an XML asset.
    /// </summary>
    public static Prefab LoadFromAsset(string assetName)
    {
        var xmlAsset = AssetManager.LoadAsset<XMLAsset>(assetName);
        if (xmlAsset.XMLContent == null)
            throw new InvalidOperationException($"XML asset '{assetName}' has no content loaded.");

        return LoadFromXml(xmlAsset.XMLContent);
    }

    /// <summary>
    /// Loads a prefab from an XML file (legacy, for testing).
    /// </summary>
    public static Prefab LoadFromFile(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"Template definition file not found: {filePath}");

        var xmlData = File.ReadAllText(filePath);
        return LoadFromXml(xmlData);
    }

    /// <summary>
    /// Parses a prefab from an XML string.
    /// Expects a root element named <c>Prefab</c>.
    /// </summary>
    public static Prefab LoadFromXml(string xmlData)
    {
        var doc = XDocument.Parse(xmlData);
        var root = doc.Root;

        if (root == null || !string.Equals(root.Name.LocalName, "Prefab", StringComparison.OrdinalIgnoreCase))
            throw new FormatException("Root element must be 'Prefab'.");

        var template = new Prefab
        {
            Type = root.Attribute("Type")?.Value ?? throw new FormatException("Prefab missing required 'Type' attribute."),
            Rotation = float.Parse(root.Attribute("Rotation")?.Value ?? "0", NumberStyles.Any, CultureInfo.InvariantCulture),
            Sort = int.Parse(root.Attribute("Sort")?.Value ?? "0"),
            Active = bool.Parse(root.Attribute("Active")?.Value ?? "true")
        };

        ParseTags(root, template);
        ParseComponents(root, template);
        ParseChildren(root, template);
        ParseBinds(root, template);

        return template;
    }

    private static Prefab ParseTemplateElement(XElement element)
    {
        var template = new Prefab
        {
            Type = element.Attribute("Type")?.Value ?? throw new FormatException("Nested Prefab missing 'Type' attribute."),
            Rotation = float.Parse(element.Attribute("Rotation")?.Value ?? "0", NumberStyles.Any, CultureInfo.InvariantCulture),
            Sort = int.Parse(element.Attribute("Sort")?.Value ?? "0"),
            Active = bool.Parse(element.Attribute("Active")?.Value ?? "true")
        };

        ParseTags(element, template);
        ParseComponents(element, template);
        ParseChildren(element, template);
        ParseBinds(element, template);

        return template;
    }

    /// <summary>
    /// Parses &lt;Bind&gt; elements from a template element. Collects the same set of binds
    /// that <see cref="CommandBindings.ApplyBindings"/> understands: direct children of the
    /// template and binds nested inside its &lt;Components&gt; element.
    /// </summary>
    private static void ParseBinds(XElement element, Prefab template)
    {
        foreach (var bind in element.Elements("Bind"))
            template.Binds.Add(bind);

        var componentsElement = element.Element("Components");
        if (componentsElement != null)
        {
            foreach (var child in componentsElement.Elements())
            {
                if (child.Name.LocalName == "Bind")
                    template.Binds.Add(child);
                else if (child.Name.LocalName == "Component")
                    template.Binds.AddRange(child.Elements("Bind"));
            }
        }
    }

    /// <summary>Parses the Tags element and populates the prefab's tags list.</summary>
    private static void ParseTags(XElement element, Prefab template)
    {
        var tagsElement = element.Element("Tags");
        if (tagsElement == null)
            return;

        template.Tags = tagsElement.Elements("Tag")
            .Select(t => t.Attribute("Name")?.Value)
            .Where(v => v != null)
            .ToList()!;
    }

    /// <summary>Parses the Components element and populates the prefab's components list.</summary>
    private static void ParseComponents(XElement element, Prefab template)
    {
        var componentsElement = element.Element("Components");
        if (componentsElement == null)
            return;

        foreach (var compElem in componentsElement.Elements("Component"))
        {
            var typeName = compElem.Attribute("Type")?.Value;
            if (string.IsNullOrWhiteSpace(typeName))
                continue;

            var compDef = ParseComponentDefinition(compElem);
            template.Components.Add(compDef);
        }
    }

    /// <summary>Parses a single Component element into a ComponentDefinition.</summary>
    private static Prefab.ComponentDefinition ParseComponentDefinition(XElement compElem)
    {
        var compDef = new Prefab.ComponentDefinition
        {
            Type = compElem.Attribute("Type")?.Value ?? string.Empty
        };

        var propsElem = compElem.Element("Properties");
        if (propsElem == null)
            return compDef;

        foreach (var propElem in propsElem.Elements("Property"))
        {
            var name = propElem.Attribute("Name")?.Value;
            var val = propElem.Attribute("Value")?.Value;
            if (!string.IsNullOrWhiteSpace(name) && !string.IsNullOrWhiteSpace(val))
                compDef.Properties[name] = val;
        }

        return compDef;
    }

    /// <summary>Parses the Children element and recursively populates nested prefabs.</summary>
    private static void ParseChildren(XElement element, Prefab template)
    {
        var childrenElement = element.Element("Children");
        if (childrenElement == null)
            return;

        foreach (var childElem in childrenElement.Elements("Prefab"))
        {
            template.Children.Add(ParseTemplateElement(childElem));
        }
    }

    /// <summary>
    /// Instantiates an entity from a prefab and adds it to the system.
    /// The full child subtree is created first, then components are attached pre-order
    /// (parents before children) so hierarchy-dependent components — e.g. a widget component
    /// looking up its CanvasComponent through the parent chain — can find their ancestors.
    /// </summary>
    public static Entity Instantiate(Prefab template, EntitySystem system, Vector2 position)
    {
        var root = BuildSubtree(template, system, position);
        AttachPreOrder(root, template);
        ApplyBindsPreOrder(root, template);
        return root;
    }

    /// <summary>
    /// Applies the template's declarative &lt;Bind&gt; wiring to the instantiated entity,
    /// recursively for child templates. Runs after all components are attached so event
    /// sources and command handlers are resolvable.
    /// </summary>
    private static void ApplyBindsPreOrder(Entity entity, Prefab template)
    {
        if (template.Binds.Count > 0)
        {
            // Clone the binds into a fresh wrapper element so repeated instantiation of the
            // same template never mutates the stored elements.
            var wrapper = new XElement("Prefab");
            foreach (var bind in template.Binds)
                wrapper.Add(new XElement(bind)); // deep copy — repeated instantiation must not mutate the stored binds

            CommandBindings.ApplyBindings(entity, wrapper);
        }

        for (int i = 0; i < template.Children.Count && i < entity.Children.Count; i++)
        {
            ApplyBindsPreOrder(entity.Children[i], template.Children[i]);
        }
    }

    /// <summary>
    /// Recursively creates and starts the entity for a template and all of its children,
    /// without attaching any component definitions yet.
    /// </summary>
    private static Entity BuildSubtree(Prefab template, EntitySystem system, Vector2 position)
    {
        // Use the robust type resolution logic from EntitySerializer (if accessible) or mirror it here
        Type? type = null;

        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            try
            {
                var foundType = assembly.GetType(template.Type);
                if (foundType != null && typeof(Entity).IsAssignableFrom(foundType))
                {
                    type = foundType;
                    break;
                }

                if (type == null)
                {
                    var candidates = assembly.GetTypes()
                        .Where(t => t.Name.Equals(template.Type, StringComparison.OrdinalIgnoreCase) && typeof(Entity).IsAssignableFrom(t));
                    type = candidates.FirstOrDefault();
                }
            }
            catch (ReflectionTypeLoadException ex)
            {
                Console.WriteLine($"[Template] Failed to load types from assembly: {ex.Message}");
            }
        }

        if (type == null) 
            throw new FormatException($"Could not resolve entity type '{template.Type}' in any loaded assembly.");

        // 1. Create entity WITHOUT calling OnStart() yet — we need to set position first so physics bodies initialize correctly
        var entity = system.CreateEntityUnstarted(type);

        // 2. Apply base properties BEFORE OnStart() so components initialize at the correct position
        entity.Position = position;
        entity.Rotation = template.Rotation;
        entity.SetSort(template.Sort);
        entity.SetActive(template.Active);

        foreach (var tag in template.Tags)
            entity.SetTag(tag);

        // 3. Apply component definitions to any pre-existing components (most won't exist yet —
        //    that's fine). Components are NOT created here: entities like Ball expect their own
        //    OnStart to be the creator, so creating early would hide the prefab definition from them.
        foreach (var compDef in template.Components)
        {
            ApplyComponentDefinitionIfExists(entity, compDef);
        }

        // 4. NOW call OnStart() — with the deferred-attach window open so components the entity's
        //    own OnStart adds attach only after prefab properties and overrides are final.
        entity.BeginDeferringComponentAttach();
        try
        {
            entity.OnStart();
        }
        catch
        {
            // Close the window so the entity isn't left with permanently deferred components.
            entity.AttachPendingComponents();
            throw;
        }

        // Recurse for children before any component attachments happen.
        foreach (var childTemplate in template.Children)
        {
            var child = BuildSubtree(childTemplate, system, position); // Relative positioning usually handled by Entity logic if children are added
            entity.AddChild(child);
        }

        return entity;
    }

    /// <summary>
    /// Recursively applies component definitions so that a parent's components attach before
    /// its children's (pre-order).
    /// </summary>
    private static void AttachPreOrder(Entity entity, Prefab template)
    {
        // Re-apply component overrides after OnStart() — this ensures prefab values win over OnStart() defaults
        foreach (var compDef in template.Components)
        {
            ApplyComponentDefinition(entity, compDef);
        }

        // Complete attachment for everything created during the window (OnStart additions plus
        // prefab-created components) now that all properties are final.
        entity.AttachPendingComponents();

        for (int i = 0; i < template.Children.Count && i < entity.Children.Count; i++)
        {
            AttachPreOrder(entity.Children[i], template.Children[i]);
        }
    }

    /// <summary>
    /// Applies component definition properties to an existing component only (does NOT create new components).
    /// Used before OnStart() to set properties that affect initialization.
    /// </summary>
    private static void ApplyComponentDefinitionIfExists(Entity entity, Prefab.ComponentDefinition def)
    {
        Type? type = ResolveComponentType(def.Type);
        if (type == null) return;

        var component = entity.GetComponent(type);
        if (component == null) return; // Component doesn't exist yet — skip

        ApplyProperties(component, type, def.Properties);
    }


    /// <summary>
    /// Applies component definition, creating the component if it doesn't exist or updating an
    /// existing one. Runs inside the deferred-attach window, so components created here (via
    /// <see cref="Entity.AddComponent"/>) attach only after all properties — including
    /// per-instantiation overrides — are final, and observe those values in <c>OnAttach</c>.
    /// Pre-existing components already attached keep their state; their properties are simply updated.
    /// </summary>
    private static void ApplyComponentDefinition(Entity entity, Prefab.ComponentDefinition def)
    {
        Type? type = ResolveComponentType(def.Type);
        if (type == null) return;

        var component = entity.GetComponent(type);
        if (component == null)
        {
            try
            {
                component = (EntityComponent)Activator.CreateInstance(type)!;
                // AddComponent defers OnAttach while the prefab instantiation window is open.
                entity.AddComponent(component);
            }
            catch (MissingMethodException)
            {
                // Component requires constructor args — skip creation, just log
                Console.WriteLine($"[Prefab] Skipping creation of '{def.Type}' — no parameterless constructor.");
                return;
            }
        }

        ApplyProperties(component, type, def.Properties);
    }


    private static void ApplyProperties(EntityComponent component, Type type, Dictionary<string, string> properties)
    {
        foreach (var prop in properties)
        {
            var property = type.GetProperty(prop.Key, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
            if (property != null && property.CanWrite)
            {
                object value = SerializationUtils.ParseValue(property.PropertyType, prop.Value);
                property.SetValue(component, value);
            }
        }
    }

    /// <summary>
    /// Resolves an entity type name (short or fully-qualified) across loaded assemblies.
    /// Public so the scene parser can validate &lt;EntityDefinition Type=&gt; at parse time.
    /// </summary>
    public static Type? ResolveEntityType(string typeName)
    {
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            try
            {
                var foundType = assembly.GetType(typeName);
                if (foundType != null && typeof(Entity).IsAssignableFrom(foundType))
                    return foundType;

                var candidates = assembly.GetTypes()
                    .Where(t => t.Name.Equals(typeName, StringComparison.OrdinalIgnoreCase) && typeof(Entity).IsAssignableFrom(t));
                var type = candidates.FirstOrDefault();
                if (type != null) return type;
            }
            catch (ReflectionTypeLoadException ex)
            {
                Console.WriteLine($"[Template] Failed to load types from assembly: {ex.Message}");
            }
        }

        return null;
    }

    /// <summary>
    /// Resolves a component type name (short or fully-qualified) across loaded assemblies.
    /// Public so per-instantiation overrides can reuse the exact same resolution path.
    /// </summary>
    public static Type? ResolveComponentType(string typeName)
    {
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            try
            {
                var foundType = assembly.GetType(typeName);
                if (foundType != null && typeof(EntityComponent).IsAssignableFrom(foundType))
                    return foundType;

                var candidates = assembly.GetTypes()
                    .Where(t => t.Name.Equals(typeName, StringComparison.OrdinalIgnoreCase) && typeof(EntityComponent).IsAssignableFrom(t));
                var type = candidates.FirstOrDefault();
                if (type != null) return type;
            }
            catch (ReflectionTypeLoadException ex)
            {
                Console.WriteLine($"[Template] Failed to load types from assembly: {ex.Message}");
            }
        }

        Console.WriteLine($"[Template] Could not resolve component type '{typeName}'.");
        return null;
    }

    // Removed ParseValue as it is now handled by SerializationUtils
}
