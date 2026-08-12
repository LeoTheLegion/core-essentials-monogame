using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Microsoft.Xna.Framework;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Components;

namespace CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Serialization;

/// <summary>
/// Handles serialization and deserialization of the complete entity system state for save games.
/// Supports saving entity positions, rotations, components, and hierarchical relationships.
/// </summary>
public static class GameStateSerializer
{
    private const string GameStateRootElement = "GameState";
    private const string EntitiesElement = "Entities";
    private const string EntityElement = "Entity";
    private const string ComponentsElement = "Components";
    private const string ComponentElement = "Component";
    private const string ChildrenElement = "Children";
    private const string PropertiesElement = "Properties";
    private const string PropertyElement = "Property";
    private const string PositionElement = "Position";

    /// <summary>
    /// Saves the complete entity system state to an XML file.
    /// </summary>
    /// <param name="system">The EntitySystem to save.</param>
    /// <param name="filePath">The path to save the game state file.</param>
    public static void SaveState(EntitySystem system, string filePath)
    {
        if (system == null)
            throw new ArgumentNullException(nameof(system));
        
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentNullException(nameof(filePath));

        var document = CreateGameStateDocument(system);
        document.Save(filePath);
    }

    /// <summary>
    /// Loads a game state from an XML file and applies it to the entity system.
    /// </summary>
    /// <param name="system">The EntitySystem to load state into.</param>
    /// <param name="filePath">The path to the game state file.</param>
    /// <param name="mergeExisting">If true, merges saved state with existing entities. If false, replaces all entities.</param>
    public static void LoadState(EntitySystem system, string filePath, bool mergeExisting = false)
    {
        if (system == null)
            throw new ArgumentNullException(nameof(system));
        
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"Game state file not found: {filePath}");

        var xmlData = File.ReadAllText(filePath);
        LoadStateFromXml(system, xmlData, mergeExisting);
    }

    /// <summary>
    /// Loads a game state from XML string and applies it to the entity system.
    /// </summary>
    /// <param name="system">The EntitySystem to load state into.</param>
    /// <param name="xmlData">The XML string containing game state.</param>
    /// <param name="mergeExisting">If true, merges saved state with existing entities. If false, replaces all entities.</param>
    public static void LoadStateFromXml(EntitySystem system, string xmlData, bool mergeExisting = false)
    {
        if (system == null)
            throw new ArgumentNullException(nameof(system));

        var document = XDocument.Parse(xmlData);
        var root = document.Root;

        if (root == null || !string.Equals(root.Name.LocalName, GameStateRootElement, StringComparison.OrdinalIgnoreCase))
        {
            throw new FormatException($"Root element must be <{GameStateRootElement}>.");
        }

        if (!mergeExisting)
        {
            // Clear existing entities when not merging
            system.ClearEntities();
        }

        var entitiesElement = root.Element(EntitiesElement);
        if (entitiesElement == null)
            return;

        // First pass: Create all entities and build ID mapping
        var idToEntity = new Dictionary<string, Entity>(StringComparer.OrdinalIgnoreCase);
        var entitiesToProcess = new List<(XElement element, Entity entity)>();

        foreach (var entityElement in entitiesElement.Elements(EntityElement))
        {
            var entity = CreateEntityFromElement(entityElement, system, idToEntity);
            if (entity != null)
            {
                entitiesToProcess.Add((entityElement, entity));
                if (!string.IsNullOrEmpty(entity.Id))
                {
                    idToEntity[entity.Id] = entity;
                }
            }
        }

        // Second pass: Restore entity state and build hierarchy
        foreach (var (element, entity) in entitiesToProcess)
        {
            RestoreEntityState(entity, element, system, mergeExisting);
            LoadEntityComponents(entity, element);
            
            // Handle children
            var childrenElement = element.Element(ChildrenElement);
            if (childrenElement != null)
            {
                foreach (var childElement in childrenElement.Elements(EntityElement))
                {
                    var childEntity = CreateEntityFromElement(childElement, system, idToEntity);
                    if (childEntity != null)
                    {
                        entity.AddChild(childEntity);
                        if (!string.IsNullOrEmpty(childEntity.Id))
                        {
                            idToEntity[childEntity.Id] = childEntity;
                        }
                    }
                }
            }
        }
    }

    private static XDocument CreateGameStateDocument(EntitySystem system)
    {
        var document = new XDocument(
            new XElement(GameStateRootElement,
                new XAttribute("Version", "1.0"),
                new XAttribute("Timestamp", DateTime.UtcNow.ToString("o")),
                new XElement(EntitiesElement,
                    system.GetEntities()
                        .Where(e => e.Id != null)
                        .Select(CreateEntityElement)
                )
            )
        );

        return document;
    }

    private static XElement CreateEntityElement(Entity entity)
    {
        var element = new XElement(EntityElement,
            new XAttribute("Id", entity.Id),
            new XAttribute("Type", entity.GetType().FullName),
            new XAttribute("Rotation", entity.Rotation.ToString(CultureInfo.InvariantCulture)),
            new XAttribute("Sort", entity.GetSort()),
            new XAttribute("Active", entity.GetActive()),
            new XElement(PositionElement,
                new XAttribute("X", entity.Position.X.ToString(CultureInfo.InvariantCulture)),
                new XAttribute("Y", entity.Position.Y.ToString(CultureInfo.InvariantCulture))
            ),
            new XElement("Tags",
                entity.Tags.Select(tag => new XElement("Tag", new XAttribute("Name", tag)))
            )
        );

        // Serialize components
        if (entity.Components.Any())
        {
            var componentsElement = new XElement(ComponentsElement);
            foreach (var component in entity.Components)
            {
                if (component is ISerializableComponent serializable)
                {
                    var componentElement = new XElement(ComponentElement,
                        new XAttribute("Type", component.GetType().Name),
                        serializable.SerializeToXml()
                    );
                    componentsElement.Add(componentElement);
                }
            }
            if (componentsElement.HasElements)
                element.Add(componentsElement);
        }

        // Serialize children
        if (entity.Children.Any())
        {
            var childrenElement = new XElement(ChildrenElement);
            foreach (var child in entity.Children)
            {
                childrenElement.Add(CreateEntityElement(child));
            }
            element.Add(childrenElement);
        }

        return element;
    }

    private static Entity? CreateEntityFromElement(XElement element, EntitySystem system, Dictionary<string, Entity> idToEntity)
    {
        var id = element.Attribute("Id")?.Value;
        var typeName = element.Attribute("Type")?.Value;

        if (string.IsNullOrWhiteSpace(typeName))
            return null;

        // Check if entity already exists in the system (for merge mode)
        if (!string.IsNullOrWhiteSpace(id))
        {
            var existingInSystem = system.GetEntities().FirstOrDefault(e => e.Id == id);
            if (existingInSystem != null)
            {
                idToEntity[id] = existingInSystem;
                return existingInSystem;
            }
            
            // Check if entity already created in this load operation
            if (idToEntity.TryGetValue(id, out var existingEntity))
            {
                return existingEntity;
            }
        }

        // Create new entity using reflection
        var entityType = Type.GetType(typeName) ?? 
            AppDomain.CurrentDomain.GetAssemblies()
                .Select(a => a.GetType(typeName))
                .FirstOrDefault(t => t != null);
        
        if (entityType == null || !typeof(Entity).IsAssignableFrom(entityType))
            return null;
        
        var entity = system.CreateEntity(entityType, Array.Empty<object>());
        
        if (!string.IsNullOrWhiteSpace(id))
        {
            entity.SetId(id);
            idToEntity[id] = entity;
        }

        return entity;
    }

    private static void RestoreEntityState(Entity entity, XElement element, EntitySystem system, bool mergeExisting)
    {
        // Restore position
        var positionElement = element.Element(PositionElement);
        if (positionElement != null)
        {
            if (float.TryParse(positionElement.Attribute("X")?.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out float x) &&
                float.TryParse(positionElement.Attribute("Y")?.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out float y))
            {
                entity.Position = new Vector2(x, y);
            }
        }

        // Restore rotation
        if (float.TryParse(element.Attribute("Rotation")?.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out float rotation))
        {
            entity.Rotation = rotation;
        }

        // Restore sort order
        if (int.TryParse(element.Attribute("Sort")?.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out int sort))
        {
            entity.SetSort(sort);
        }

        // Restore active state
        if (bool.TryParse(element.Attribute("Active")?.Value, out bool active))
        {
            entity.SetActive(active);
        }

        // Restore tags (only if not merging to preserve runtime tags)
        if (!mergeExisting)
        {
            var tagsElement = element.Element("Tags");
            if (tagsElement != null)
            {
                // Clear existing tags
                foreach (var tag in entity.Tags.ToList())
                {
                    entity.RemoveTag(tag);
                }

                // Add saved tags
                foreach (var tagElement in tagsElement.Elements("Tag"))
                {
                    var tagName = tagElement.Attribute("Name")?.Value;
                    if (!string.IsNullOrWhiteSpace(tagName))
                    {
                        entity.SetTag(tagName);
                    }
                }
            }
        }
    }

    private static void LoadEntityComponents(Entity entity, XElement element)
    {
        var componentsElement = element.Element(ComponentsElement);
        if (componentsElement == null)
            return;

        foreach (var componentElement in componentsElement.Elements(ComponentElement))
        {
            var typeName = componentElement.Attribute("Type")?.Value;
            if (string.IsNullOrWhiteSpace(typeName))
                continue;

            // Find existing component or create new one
            var existingComponent = entity.Components.FirstOrDefault(c => 
                c.GetType().Name.Equals(typeName, StringComparison.OrdinalIgnoreCase));

            EntityComponent component;
            if (existingComponent != null)
            {
                component = existingComponent;
            }
            else
            {
                // Try to create component via reflection
                try
                {
                    var componentType = Type.GetType(typeName) ?? 
                        AppDomain.CurrentDomain.GetAssemblies()
                            .Select(a => a.GetType(typeName))
                            .FirstOrDefault(t => t != null);

                    if (componentType != null && typeof(EntityComponent).IsAssignableFrom(componentType))
                    {
                        component = (EntityComponent)Activator.CreateInstance(componentType)!;
                        component.Owner = entity;
                        entity.AddComponent(component);
                    }
                    else
                    {
                        continue;
                    }
                }
                catch
                {
                    continue;
                }
            }

            // Deserialize if component supports serialization
            if (component is ISerializableComponent serializable)
            {
                var stateElement = componentElement.Elements().FirstOrDefault();
                if (stateElement != null)
                {
                    serializable.DeserializeFromXml(stateElement);
                }
            }
        }
    }
}
