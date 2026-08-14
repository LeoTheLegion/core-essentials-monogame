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
/// Logic for parsing EntityTemplates from XML and instantiating them into Entities.
/// </summary>
public static class EntityTemplateLoader
{
    /// <summary>
    /// Loads an EntityTemplate from an XML asset.
    /// </summary>
    public static EntityTemplate LoadFromAsset(string assetName)
    {
        var xmlAsset = AssetManager.LoadAsset<XMLAsset>(assetName);
        if (xmlAsset.XMLContent == null)
            throw new InvalidOperationException($"XML asset '{assetName}' has no content loaded.");

        return LoadFromXml(xmlAsset.XMLContent);
    }

    /// <summary>
    /// Loads an EntityTemplate from an XML file (legacy, for testing).
    /// </summary>
    public static EntityTemplate LoadFromFile(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"Template definition file not found: {filePath}");

        var xmlData = File.ReadAllText(filePath);
        return LoadFromXml(xmlData);
    }

    /// <summary>
    /// Parses an entity template from an XML string.
    /// Expects a root element named <c>EntityTemplate</c>.
    /// </summary>
    public static EntityTemplate LoadFromXml(string xmlData)
    {
        var doc = XDocument.Parse(xmlData);
        var root = doc.Root;

        if (root == null || !string.Equals(root.Name.LocalName, "EntityTemplate", StringComparison.OrdinalIgnoreCase))
            throw new FormatException("Root element must be 'EntityTemplate'.");

        var template = new EntityTemplate
        {
            Type = root.Attribute("Type")?.Value ?? throw new FormatException("EntityTemplate missing required 'Type' attribute."),
            Rotation = float.Parse(root.Attribute("Rotation")?.Value ?? "0", NumberStyles.Any, CultureInfo.InvariantCulture),
            Sort = int.Parse(root.Attribute("Sort")?.Value ?? "0"),
            Active = bool.Parse(root.Attribute("Active")?.Value ?? "true")
        };

        ParseTags(root, template);
        ParseComponents(root, template);
        ParseChildren(root, template);

        return template;
    }

    private static EntityTemplate ParseTemplateElement(XElement element)
    {
        var template = new EntityTemplate
        {
            Type = element.Attribute("Type")?.Value ?? throw new FormatException("Nested EntityTemplate missing 'Type' attribute."),
            Rotation = float.Parse(element.Attribute("Rotation")?.Value ?? "0", NumberStyles.Any, CultureInfo.InvariantCulture),
            Sort = int.Parse(element.Attribute("Sort")?.Value ?? "0"),
            Active = bool.Parse(element.Attribute("Active")?.Value ?? "true")
        };

        ParseTags(element, template);
        ParseComponents(element, template);
        ParseChildren(element, template);

        return template;
    }

    /// <summary>Parses the Tags element and populates the template's tags list.</summary>
    private static void ParseTags(XElement element, EntityTemplate template)
    {
        var tagsElement = element.Element("Tags");
        if (tagsElement == null)
            return;

        template.Tags = tagsElement.Elements("Tag")
            .Select(t => t.Attribute("Name")?.Value)
            .Where(v => v != null)
            .ToList()!;
    }

    /// <summary>Parses the Components element and populates the template's components list.</summary>
    private static void ParseComponents(XElement element, EntityTemplate template)
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
    private static EntityTemplate.ComponentDefinition ParseComponentDefinition(XElement compElem)
    {
        var compDef = new EntityTemplate.ComponentDefinition
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

    /// <summary>Parses the Children element and recursively populates nested templates.</summary>
    private static void ParseChildren(XElement element, EntityTemplate template)
    {
        var childrenElement = element.Element("Children");
        if (childrenElement == null)
            return;

        foreach (var childElem in childrenElement.Elements("EntityTemplate"))
        {
            template.Children.Add(ParseTemplateElement(childElem));
        }
    }

    /// <summary>
    /// Instantiates an entity from a template and adds it to the system.
    /// </summary>
    public static Entity Instantiate(EntityTemplate template, EntitySystem system, Vector2 position)
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

        // 3. Apply component definitions to any pre-existing components (most won't exist yet — that's fine)
        foreach (var compDef in template.Components)
        {
            ApplyComponentDefinitionIfExists(entity, compDef);
        }

        // 4. NOW call OnStart() — components will initialize at the correct position
        entity.OnStart();

        // 5. Re-apply component overrides after OnStart() — this ensures template values win over OnStart() defaults
        foreach (var compDef in template.Components)
        {
            ApplyComponentDefinition(entity, compDef);
        }

        // 6. Recurse for children
        foreach (var childTemplate in template.Children)
        {
            var child = Instantiate(childTemplate, system, position); // Relative positioning usually handled by Entity logic if children are added
            entity.AddChild(child);
        }

        return entity;
    }

    /// <summary>
    /// Applies component definition properties to an existing component only (does NOT create new components).
    /// Used before OnStart() to set properties that affect initialization.
    /// </summary>
    private static void ApplyComponentDefinitionIfExists(Entity entity, EntityTemplate.ComponentDefinition def)
    {
        Type? type = ResolveComponentType(def.Type);
        if (type == null) return;

        var component = entity.GetComponent(type);
        if (component == null) return; // Component doesn't exist yet — skip

        ApplyProperties(component, type, def.Properties);
    }

    /// <summary>
    /// Applies component definition, creating the component if it doesn't exist or updating existing one.
    /// Used after OnStart() to ensure template values override defaults.
    /// </summary>
    private static void ApplyComponentDefinition(Entity entity, EntityTemplate.ComponentDefinition def)
    {
        Type? type = ResolveComponentType(def.Type);
        if (type == null) return;

        var component = entity.GetComponent(type);
        if (component == null)
        {
            try
            {
                component = (EntityComponent)Activator.CreateInstance(type)!;
                entity.AddComponent(component);
            }
            catch (MissingMethodException)
            {
                // Component requires constructor args — skip creation, just log
                Console.WriteLine($"[Template] Skipping creation of '{def.Type}' — no parameterless constructor.");
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

    private static Type? ResolveComponentType(string typeName)
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
