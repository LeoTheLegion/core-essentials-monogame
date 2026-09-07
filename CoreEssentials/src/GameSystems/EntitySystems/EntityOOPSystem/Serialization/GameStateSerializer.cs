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
    /// Only entities carrying an <see cref="ISaveableComponent"/> are included during save/load operations.
/// Uses entity IDs to determine whether to update an existing entity or create a new one on load.
/// </summary>
public static class GameStateSerializer
{
    private const string GameStateRootElement = "GameState";
    private const string EntitiesElement = "Entities";
    private const string EntityElement = "Entity";
    private const string ChildrenElement = "Children";

    /// <summary>
    /// Saves the state of all entities carrying an <see cref="ISaveableComponent"/> in the entity system to an XML file.
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
        ValidateRootElement(document.Root);

        var entitiesElement = document.Root?.Element(EntitiesElement);
        if (entitiesElement == null)
        {
            return;
        }

        // Collect all IDs from the save file for cleanup later
        var loadedIds = CollectLoadedEntityIds(entitiesElement);

        // Build ID mapping and process entities
        var idToEntity = new Dictionary<string, Entity>(StringComparer.OrdinalIgnoreCase);
        ProcessRootEntities(entitiesElement, system, idToEntity);

        // Remove entities that weren't in the saved state
        RemoveUnsavedEntities(system, loadedIds);
    }

    /// <summary>
    /// Validates that the XML document root is the expected GameState element.
    /// </summary>
    private static void ValidateRootElement(XElement? root)
    {
        if (root == null || !string.Equals(root.Name.LocalName, GameStateRootElement, StringComparison.OrdinalIgnoreCase))
        {
            throw new FormatException($"Root element must be <{GameStateRootElement}>.");
        }
    }

    /// <summary>
    /// Collects all entity IDs from the save file, including nested children.
    /// </summary>
    private static HashSet<string> CollectLoadedEntityIds(XElement entitiesElement)
    {
        var loadedIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var entityElement in entitiesElement.Elements(EntityElement))
        {
            AddEntityIdToSet(entityElement, loadedIds);

            var childrenElement = entityElement.Element(ChildrenElement);
            if (childrenElement != null)
            {
                foreach (var childElement in childrenElement.Elements(EntityElement))
                {
                    AddEntityIdToSet(childElement, loadedIds);
                }
            }
        }

        return loadedIds;
    }

    /// <summary>
    /// Adds the entity ID from an XML element to the loaded IDs set if it's not null or empty.
    /// </summary>
    private static void AddEntityIdToSet(XElement element, HashSet<string> loadedIds)
    {
        var id = element.Attribute("Id")?.Value;
        if (!string.IsNullOrWhiteSpace(id))
        {
            loadedIds.Add(id);
        }
    }

    /// <summary>
    /// Processes all root-level entity elements, resolving or creating entities and loading their state.
    /// </summary>
    private static void ProcessRootEntities(XElement entitiesElement, EntitySystem system, Dictionary<string, Entity> idToEntity)
    {
        foreach (var entityElement in entitiesElement.Elements(EntityElement))
        {
            try
            {
                var entity = ResolveOrCreateEntity(entityElement, system, idToEntity);
                LoadEntityAndChildrenState(entity, entityElement, system, idToEntity);
            }
            catch (Exception ex)
            {
                var id = entityElement.Attribute("Id")?.Value ?? "unknown";
                throw new InvalidOperationException($"Error loading entity '{id}': {ex.Message}", ex);
            }
        }
    }

    /// <summary>
    /// Loads state into a saveable entity and its children.
    /// </summary>
    private static void LoadEntityAndChildrenState(Entity entity, XElement entityElement, EntitySystem system, Dictionary<string, Entity> idToEntity)
    {
        // The save component handles the restore.
        var saveComponent = FindSaveComponent(entity);
        if (saveComponent != null)
        {
            saveComponent.LoadState(entityElement);
        }

        var childrenElement = entityElement.Element(ChildrenElement);
        if (childrenElement == null)
        {
            return;
        }

        foreach (var childElement in childrenElement.Elements(EntityElement))
        {
            var childEntity = ResolveOrCreateEntity(childElement, system, idToEntity);
            LoadEntityAndChildrenState(childEntity, childElement, system, idToEntity);
            entity.AddChild(childEntity);
        }
    }

    /// <summary>
    /// Removes entities from the system that were not present in the saved state.
    /// </summary>
    private static void RemoveUnsavedEntities(EntitySystem system, HashSet<string> loadedIds)
    {
        var unsavedEntities = system.GetEntities()
            .Where(e => FindSaveComponent(e) != null)
            .Where(e =>
            {
                var id = e.Id;
                return !string.IsNullOrWhiteSpace(id) && !loadedIds.Contains(id);
            })
            .ToList();

        foreach (var entity in unsavedEntities)
        {
            system.RemoveEntity(entity);
        }
    }

    /// <summary>
    /// Returns an existing entity with the given ID, or creates a new one and adds it to the system.
    /// </summary>
    private static Entity ResolveOrCreateEntity(XElement element, EntitySystem system, Dictionary<string, Entity> idToEntity)
    {
        var id = element.Attribute("Id")?.Value;

        // Already resolved this ID during this load pass
        if (!string.IsNullOrWhiteSpace(id) && idToEntity.TryGetValue(id, out var cached))
        {
            return cached;
        }

        // Check if an entity with this ID already exists in the system (update in place)
        if (!string.IsNullOrWhiteSpace(id))
        {
            var existing = system.GetEntities().FirstOrDefault(e => e.Id == id);
            if (existing != null)
            {
                idToEntity[id] = existing;
                return existing;
            }
        }

        // The element must carry a Prefab → recreate via Instantiate so prefab-declared
        // components come back automatically. The prefab must already be registered.
        var prefabName = element.Attribute("Prefab")?.Value;
        if (string.IsNullOrWhiteSpace(prefabName))
        {
            throw new FormatException(
                $"Entity '{id ?? "?"}' is missing a 'Prefab' attribute — saves must stamp the " +
                $"prefab so it can be recreated on load.");
        }

        if (!system.HasPrefab(prefabName))
        {
            throw new KeyNotFoundException(
                $"Prefab '{prefabName}' referenced by saved entity '{id ?? "?"}' is not registered. " +
                $"Register the prefab (or its scene) before loading this save.");
        }

        var position = ReadPosition(element);
        var entity = system.Instantiate(prefabName, position);
        if (!string.IsNullOrWhiteSpace(id))
        {
            entity.SetId(id);
            idToEntity[id] = entity;
        }
        return entity;
    }

    private static XDocument CreateGameStateDocument(EntitySystem system)
    {
        // Saveable = has an ISaveableComponent.
        var saveables = system.GetEntities()
            .Where(e => IsSaveable(e) && !string.IsNullOrWhiteSpace(e.Id))
            .ToList();

        // Every saveable entity must have an ID.
        var missingId = system.GetEntities()
            .Where(e => IsSaveable(e) && string.IsNullOrWhiteSpace(e.Id))
            .ToList();

        if (missingId.Any())
        {
            var types = string.Join(", ", missingId.Select(e => e.GetType().Name));
            throw new InvalidOperationException(
                $"Saveable entities must have an ID set before saving. " +
                $"Missing IDs on: {types}");
        }

        // New-path (save component) entities require a known prefab so they can be recreated.
        var missingPrefab = saveables
            .Where(e => FindSaveComponent(e) != null && string.IsNullOrWhiteSpace(e.PrefabName))
            .ToList();

        if (missingPrefab.Any())
        {
            var ids = string.Join(", ", missingPrefab.Select(e => e.Id));
            throw new InvalidOperationException(
                $"Entities with a save component must be instantiated from a registered prefab to be saved. " +
                $"Missing prefab on: {ids}");
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
        // The save component produces the full <Entity> element.
        var saveComponent = FindSaveComponent(entity);
        if (saveComponent == null)
            throw new InvalidOperationException($"Entity '{entity.Id}' is marked saveable but has no ISaveableComponent.");
        XElement element = saveComponent.SaveState();

        // Stamp the authoritative prefab so load can recreate via Instantiate.
        if (!string.IsNullOrWhiteSpace(entity.PrefabName))
        {
            element.SetAttributeValue("Prefab", entity.PrefabName);
        }

        // Serialize saveable children (the entity doesn't know about its own children in SaveState).
        var saveableChildren = entity.Children.Where(IsSaveable).ToList();
        if (saveableChildren.Any())
        {
            var childrenElement = new XElement(ChildrenElement);
            foreach (var child in saveableChildren)
            {
                childrenElement.Add(CreateEntityElement(child));
            }
            element.Add(childrenElement);
        }

        return element;
    }

    /// <summary>An entity is saveable when it has a save component.</summary>
    private static bool IsSaveable(Entity entity) => FindSaveComponent(entity) != null;

    /// <summary>Finds the first save component attached to an entity, if any.</summary>
    private static Serialization.ISaveableComponent? FindSaveComponent(Entity entity)
    {
        foreach (var component in entity.Components)
        {
            if (component is ISaveableComponent saveable)
                return saveable;
        }
        return null;
    }

    /// <summary>Reads the saved world position from an <c>&lt;Entity&gt;</c> element.</summary>
    private static Vector2 ReadPosition(XElement element)
    {
        var p = element.Element("Position");
        float x = 0f, y = 0f;
        if (p != null)
        {
            _ = float.TryParse(p.Attribute("X")?.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out x);
            _ = float.TryParse(p.Attribute("Y")?.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out y);
        }
        return new Vector2(x, y);
    }
}
