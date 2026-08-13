using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Microsoft.Xna.Framework;

namespace CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Serialization;

/// <summary>
/// Handles serialization and deserialization of saveable entity state for game saves.
/// Only entities implementing <see cref="ISaveableEntity"/> are included during save/load operations.
/// Uses entity IDs to determine whether to update an existing entity or create a new one on load.
/// </summary>
public static class GameStateSerializer
{
    private const string GameStateRootElement = "GameState";
    private const string EntitiesElement = "Entities";
    private const string EntityElement = "Entity";
    private const string ChildrenElement = "Children";

    /// <summary>
    /// Saves the state of all <see cref="ISaveableEntity"/> instances in the entity system to an XML file.
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
    /// For each saved entity, if an entity with that ID already exists it will be updated; otherwise a new entity is created.
    /// </summary>
    /// <param name="system">The EntitySystem to load state into.</param>
    /// <param name="filePath">The path to the game state file.</param>
    public static void LoadState(EntitySystem system, string filePath)
    {
        if (system == null)
            throw new ArgumentNullException(nameof(system));

        if (!File.Exists(filePath))
            throw new FileNotFoundException($"Game state file not found: {filePath}");

        var xmlData = File.ReadAllText(filePath);
        LoadStateFromXml(system, xmlData);
    }

    /// <summary>
    /// Loads a game state from an XML string and applies it to the entity system.
    /// For each saved entity, if an entity with that ID already exists it will be updated; otherwise a new entity is created.
    /// </summary>
    /// <param name="system">The EntitySystem to load state into.</param>
    /// <param name="xmlData">The XML string containing game state.</param>
    public static void LoadStateFromXml(EntitySystem system, string xmlData)
    {
        if (system == null)
            throw new ArgumentNullException(nameof(system));

        var document = XDocument.Parse(xmlData);
        var root = document.Root;

        if (root == null || !string.Equals(root.Name.LocalName, GameStateRootElement, StringComparison.OrdinalIgnoreCase))
        {
            throw new FormatException($"Root element must be <{GameStateRootElement}>.");
        }

        var entitiesElement = root.Element(EntitiesElement);
        if (entitiesElement == null)
        {
            return;
        }

        // Collect all IDs from the save file (including nested children) for cleanup later
        var loadedIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var entityElement in entitiesElement.Elements(EntityElement))
        {
            var id = entityElement.Attribute("Id")?.Value;
            if (!string.IsNullOrWhiteSpace(id))
            {
                loadedIds.Add(id);
            }

            var childrenElement = entityElement.Element(ChildrenElement);
            if (childrenElement != null)
            {
                foreach (var childElement in childrenElement.Elements(EntityElement))
                {
                    var childId = childElement.Attribute("Id")?.Value;
                    if (!string.IsNullOrWhiteSpace(childId))
                    {
                        loadedIds.Add(childId);
                    }
                }
            }
        }

        // Build ID mapping for cross-entity references
        var idToEntity = new Dictionary<string, Entity>(StringComparer.OrdinalIgnoreCase);

        foreach (var entityElement in entitiesElement.Elements(EntityElement))
        {
            try
            {
                // Resolve or create the entity by ID
                var entity = ResolveOrCreateEntity(entityElement, system, idToEntity);
                if (entity is ISaveableEntity saveable)
                {
                    // Load state into the entity (existing or newly created)
                    saveable.LoadState(entityElement);

                    // Handle children
                    var childrenElement = entityElement.Element(ChildrenElement);
                    if (childrenElement != null)
                    {
                        foreach (var childElement in childrenElement.Elements(EntityElement))
                        {
                            var childEntity = ResolveOrCreateEntity(childElement, system, idToEntity);
                            if (childEntity is ISaveableEntity childSaveable)
                            {
                                childSaveable.LoadState(childElement);
                            }
                            entity.AddChild(childEntity);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                var id = entityElement.Attribute("Id")?.Value ?? "unknown";
                throw new Exception($"Error loading entity '{id}': {ex.Message}", ex);
            }
        }

        // Remove ISaveableEntity instances that weren't in the saved state
        var currentSaveables = system.GetEntities()
            .Where(e => e is ISaveableEntity && !string.IsNullOrWhiteSpace(e.Id))
            .ToList();

        foreach (var entity in currentSaveables)
        {
            if (!loadedIds.Contains(entity.Id))
            {
                system.RemoveEntity(entity);
            }
        }
    }

    /// <summary>
    /// Returns an existing entity with the given ID, or creates a new one and adds it to the system.
    /// </summary>
    private static Entity ResolveOrCreateEntity(XElement element, EntitySystem system, Dictionary<string, Entity> idToEntity)
    {
        var id = element.Attribute("Id")?.Value;
        var typeName = element.Attribute("Type")?.Value;

        if (string.IsNullOrWhiteSpace(typeName))
        {
            throw new FormatException($"Entity element missing 'Type' attribute.");
        }

        // Already resolved this ID during this load pass
        if (!string.IsNullOrWhiteSpace(id) && idToEntity.TryGetValue(id, out var cached))
        {
            return cached;
        }

        // Check if an entity with this ID already exists in the system
        if (!string.IsNullOrWhiteSpace(id))
        {
            var existing = system.GetEntities().FirstOrDefault(e => e.Id == id);
            if (existing != null)
            {
                idToEntity[id] = existing;
                return existing;
            }
        }

        // Create new entity using reflection — OnStart runs, components are initialized
        var entityType = Type.GetType(typeName) ??
            AppDomain.CurrentDomain.GetAssemblies()
                .Select(a => a.GetType(typeName))
                .FirstOrDefault(t => t != null);

        if (entityType == null || !typeof(Entity).IsAssignableFrom(entityType))
        {
            throw new FormatException($"Could not find entity type '{typeName}'.");
        }

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
        catch (Exception ex) when (ex is not FormatException)
        {
            throw new Exception($"Error creating entity of type '{typeName}': {ex.Message}", ex);
        }
    }

    private static XDocument CreateGameStateDocument(EntitySystem system)
    {
        // Only save entities that implement ISaveableEntity and have an ID
        var saveables = system.GetEntities()
            .Where(e => e is ISaveableEntity && !string.IsNullOrWhiteSpace(e.Id))
            .ToList();

        // Enforce: ISaveableEntity instances must have an ID
        var missingId = system.GetEntities().Where(e => e is ISaveableEntity && string.IsNullOrWhiteSpace(e.Id));
        if (missingId.Any())
        {
            var types = string.Join(", ", missingId.Select(e => e.GetType().Name));
            throw new InvalidOperationException(
                $"ISaveableEntity instances must have an ID set before saving. " +
                $"Missing IDs on: {types}");
        }

        var document = new XDocument(
            new XElement(GameStateRootElement,
                new XAttribute("Version", "1.0"),
                new XAttribute("Timestamp", DateTime.UtcNow.ToString("o")),
                new XElement(EntitiesElement,
                    saveables.Select(CreateEntityElement)
                )
            )
        );

        return document;
    }

    private static XElement CreateEntityElement(Entity entity)
    {
        var element = ((ISaveableEntity)entity).SaveState();

        // Serialize children (entity doesn't know about its children in SaveState)
        if (entity.Children.Any())
        {
            var childrenElement = new XElement(ChildrenElement);
            foreach (var child in entity.Children)
            {
                if (child is ISaveableEntity)
                {
                    childrenElement.Add(CreateEntityElement(child));
                }
            }
            element.Add(childrenElement);
        }

        return element;
    }
}
