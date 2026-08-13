using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Microsoft.Xna.Framework;

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
    private const string ChildrenElement = "Children";
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
    /// <remarks>
    /// Flow: CreateEntity (OnStart runs → defaults set, components created) → RestoreState (applies saved data).
    /// This ensures components exist when entity-derived classes restore component-dependent state.
    /// In merge mode, pre-existing entities get their transform restored but runtime tags are preserved.
    /// </remarks>
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
            system.ClearEntities();
        }

        // In merge mode, track which entities already existed so we preserve their runtime tags
        var preExistingIds = mergeExisting 
            ? new HashSet<string>(system.GetEntities().Where(e => e.Id != null).Select(e => e.Id!), StringComparer.OrdinalIgnoreCase)
            : new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        var entitiesElement = root.Element(EntitiesElement);
        if (entitiesElement == null)
        {
            return;
        }

        // Build ID mapping for cross-entity references
        var idToEntity = new Dictionary<string, Entity>(StringComparer.OrdinalIgnoreCase);

        foreach (var entityElement in entitiesElement.Elements(EntityElement))
        {
            try
            {
                // Create entity normally — OnStart runs, components are initialized with defaults
                var entity = CreateEntityFromElement(entityElement, system, idToEntity, preExistingIds);
                if (entity != null)
                {
                    bool isPreExisting = preExistingIds.Contains(entity.Id ?? string.Empty);

                    // Restore state — for pre-existing entities in merge mode, we skip tag clearing
                    entity.RestoreState(entityElement, mergeTags: isPreExisting);

                    // Handle children - create them normally then add as children
                    var childrenElement = entityElement.Element(ChildrenElement);
                    if (childrenElement != null)
                    {
                        foreach (var childElement in childrenElement.Elements(EntityElement))
                        {
                            var childEntity = CreateEntityFromElement(childElement, system, idToEntity, preExistingIds);
                            if (childEntity != null)
                            {
                                childEntity.RestoreState(childElement);
                                entity.AddChild(childEntity);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error restoring entity {entityElement.Attribute("Id")?.Value}: {ex.Message}", ex);
            }
        }
    }

    private static XDocument CreateGameStateDocument(EntitySystem system)
    {
        var entities = system.GetEntities().Where(e => e.Id != null).ToList();

        var document = new XDocument(
            new XElement(GameStateRootElement,
                new XAttribute("Version", "1.0"),
                new XAttribute("Timestamp", DateTime.UtcNow.ToString("o")),
                new XElement(EntitiesElement,
                    entities.Select(CreateEntityElement)
                )
            )
        );

        return document;
    }

    private static XElement CreateEntityElement(Entity entity)
    {
        // Let the entity serialize itself - it knows what to save
        var element = entity.SerializeToXml();

        // Serialize children (entity doesn't know about its children in SerializeToXml)
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

    private static Entity? CreateEntityFromElement(XElement element, EntitySystem system, Dictionary<string, Entity> idToEntity, HashSet<string>? preExistingIds = null)
    {
        var id = element.Attribute("Id")?.Value;
        var typeName = element.Attribute("Type")?.Value;

        if (string.IsNullOrWhiteSpace(typeName))
        {
            return null;
        }

        // Check if entity already exists in the system (for merge mode)
        if (!string.IsNullOrWhiteSpace(id) && preExistingIds != null && preExistingIds.Contains(id))
        {
            var existingInSystem = system.GetEntities().FirstOrDefault(e => e.Id == id);
            if (existingInSystem != null)
            {
                idToEntity[id] = existingInSystem;
                return existingInSystem;
            }
        }

        // Check if entity already created in this load operation
        if (!string.IsNullOrWhiteSpace(id) && idToEntity.TryGetValue(id, out var existingEntity))
        {
            return existingEntity;
        }

        // Create new entity using reflection
        var entityType = Type.GetType(typeName) ?? 
            AppDomain.CurrentDomain.GetAssemblies()
                .Select(a => a.GetType(typeName))
                .FirstOrDefault(t => t != null);
        
        if (entityType == null || !typeof(Entity).IsAssignableFrom(entityType))
        {
            return null;
        }

        // Create entity normally — OnStart runs, components are initialized with defaults
        try
        {
            var entity = system.CreateEntity(entityType, Array.Empty<object>());

            // Override the auto-generated ID with the saved ID
            if (!string.IsNullOrWhiteSpace(id))
            {
                entity.SetId(id);
                idToEntity[id] = entity;
            }

            return entity;
        }
        catch (Exception ex)
        {
            throw new Exception($"Error creating entity of type {typeName}: {ex.Message}", ex);
        }
    }
}
