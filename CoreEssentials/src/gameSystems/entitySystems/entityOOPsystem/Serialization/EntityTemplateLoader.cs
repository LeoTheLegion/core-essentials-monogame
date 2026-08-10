using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Globalization;
using Microsoft.Xna.Framework;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Components;

namespace CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Serialization;

/// <summary>
/// Logic for parsing EntityTemplates from XML and instantiating them into Entities.
/// </summary>
public static class EntityTemplateLoader
{
    /// <summary>
    /// Loads an EntityTemplate from an XML file.
    /// </summary>
    public static EntityTemplate LoadFromFile(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"Template definition file not found: {filePath}");

        var xmlData = File.ReadAllText(filePath);
        return LoadFromXml(xmlData);
    }

    /// <summary>
    /// Parses an EntityTemplate from an XML string.
    /// Expects a root element of <EntityTemplate>.
    /// </summary>
    public static EntityTemplate LoadFromXml(string xmlData)
    {
        var doc = XDocument.Parse(xmlData);
        var root = doc.Root;

        if (root == null || !string.Equals(root.Name.LocalName, "EntityTemplate", StringComparison.OrdinalIgnoreCase))
            throw new FormatException("Root element must be <EntityTemplate>.");

        var template = new EntityTemplate
        {
            Type = root.Attribute("Type")?.Value ?? throw new FormatException("EntityTemplate missing required 'Type' attribute."),
            Rotation = float.Parse(root.Attribute("Rotation")?.Value ?? "0", NumberStyles.Any, CultureInfo.InvariantCulture),
            Sort = int.Parse(root.Attribute("Sort")?.Value ?? "0"),
            Active = bool.Parse(root.Attribute("Active")?.Value ?? "true")
        };

        // Tags
        var tagsElement = root.Element("Tags");
        if (tagsElement != null)
        {
            template.Tags = tagsElement.Elements("Tag")
                .Select(t => t.Attribute("Name")?.Value)
                .Where(v => v != null)
                .ToList()!;
        }

        // Components
        var componentsElement = root.Element("Components");
        if (componentsElement != null)
        {
            foreach (var compElem in componentsElement.Elements("Component"))
            {
                var typeName = compElem.Attribute("Type")?.Value;
                if (string.IsNullOrWhiteSpace(typeName)) continue;

                var compDef = new EntityTemplate.ComponentDefinition { Type = typeName };
                var propsElem = compElem.Element("Properties");
                if (propsElem != null)
                {
                    foreach (var propElem in propsElem.Elements("Property"))
                    {
                        var name = propElem.Attribute("Name")?.Value;
                        var val = propElem.Attribute("Value")?.Value;
                        if (!string.IsNullOrWhiteSpace(name) && !string.IsNullOrWhiteSpace(val))
                            compDef.Properties[name] = val;
                    }
                }
                template.Components.Add(compDef);
            }
        }

        // Children templates
        var childrenElement = root.Element("Children");
        if (childrenElement != null)
        {
            foreach (var childElem in childrenElement.Elements("EntityTemplate"))
            {
                // Note: This is a simplified recursive load. 
                // For full consistency we'd use LoadFromXml on the inner XML string.
                template.Children.Add(ParseTemplateElement(childElem));
            }
        }

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

        var tagsElement = element.Element("Tags");
        if (tagsElement != null)
        {
            template.Tags = tagsElement.Elements("Tag")
                .Select(t => t.Attribute("Name")?.Value)
                .Where(v => v != null)
                .ToList()!;
        }

        var componentsElement = element.Element("Components");
        if (componentsElement != null)
        {
            foreach (var compElem in componentsElement.Elements("Component"))
            {
                var typeName = compElem.Attribute("Type")?.Value;
                if (string.IsNullOrWhiteSpace(typeName)) continue;

                var compDef = new EntityTemplate.ComponentDefinition { Type = typeName };
                var propsElem = compElem.Element("Properties");
                if (propsElem != null)
                {
                    foreach (var propElem in propsElem.Elements("Property"))
                    {
                        var name = propElem.Attribute("Name")?.Value;
                        var val = propElem.Attribute("Value")?.Value;
                        if (!string.IsNullOrWhiteSpace(name) && !string.IsNullOrWhiteSpace(val))
                            compDef.Properties[name] = val;
                    }
                }
                template.Components.Add(compDef);
            }
        }

        var childrenElement = element.Element("Children");
        if (childrenElement != null)
        {
            foreach (var childElem in childrenElement.Elements("EntityTemplate"))
            {
                template.Children.Add(ParseTemplateElement(childElem));
            }
        }

        return template;
    }

    /// <summary>
    /// Instantiates an entity from a template and adds it to the system.
    /// </summary>
    public static Entity Instantiate(EntityTemplate template, EntitySystem system, Vector2 position)
    {
        // 1. Create entity by type
        var entity = system.CreateEntity(Type.GetType(template.Type) ?? throw new FormatException($"Could not resolve type {template.Type}"));
        
        // 2. Apply base properties
        entity.Position = position;
        entity.Rotation = template.Rotation;
        entity.SetSort(template.Sort);
        entity.SetActive(template.Active);

        foreach (var tag in template.Tags)
            entity.SetTag(tag);

        // 3. Instantiate components using a temporary XML structure to leverage EntitySerializer's property parsing
        // Alternatively, we could mirror the property applying logic here. 
        // For consistency and avoiding duplication of complex parsing (Color, Vector2), let's use a helper that mirrors EntitySerializer.
        foreach (var compDef in template.Components)
        {
            // We can't easily call EntitySerializer since it works on XElements.
            // Let's implement a simple property applicator here or add one to EntitySerializer.
            // Since I can't modify EntitySerializer without reading it again and ensuring no regressions, 
            // I'll use reflection similar to how EntitySerializer does it.
            ApplyComponentDefinition(entity, compDef);
        }

        // 4. Recurse for children
        foreach (var childTemplate in template.Children)
        {
            var child = Instantiate(childTemplate, system, position); // Relative positioning usually handled by Entity logic if children are added
            entity.AddChild(child);
        }

        return entity;
    }

    private static void ApplyComponentDefinition(Entity entity, EntityTemplate.ComponentDefinition def)
    {
        var type = Type.GetType(def.Type) ?? throw new FormatException($"Could not resolve component type {def.Type}");
        var component = (EntityComponent)Activator.CreateInstance(type)!;

        foreach (var prop in def.Properties)
        {
            var property = type.GetProperty(prop.Key, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
            if (property != null && property.CanWrite)
            {
                object value = SerializationUtils.ParseValue(property.PropertyType, prop.Value);
                property.SetValue(component, value);
            }
        }

        entity.AddComponent(component);
    }

    // Removed ParseValue as it is now handled by SerializationUtils
}
