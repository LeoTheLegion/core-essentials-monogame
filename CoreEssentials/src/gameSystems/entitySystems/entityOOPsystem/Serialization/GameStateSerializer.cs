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

        Console.WriteLine($"[LoadState] Parsed XML, mergeExisting={mergeExisting}");

        if (!mergeExisting)
        {
            // Clear existing entities when not merging
            Console.WriteLine("[LoadState] Clearing existing entities...");
            system.ClearEntities();
            Console.WriteLine("[LoadState] Entities cleared");
        }

        var entitiesElement = root.Element(EntitiesElement);
        if (entitiesElement == null)
        {
            Console.WriteLine("[LoadState] No <Entities> element found, returning early");
            return;
        }

        var entityCount = entitiesElement.Elements(EntityElement).Count();
        Console.WriteLine($"[LoadState] === First pass: creating {entityCount} entities ===");
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
        Console.WriteLine($"[LoadState] === Second pass: restoring state for {entitiesToProcess.Count} entities ===");
        foreach (var (element, entity) in entitiesToProcess)
        {
            var entityId = element.Attribute("Id")?.Value ?? "unnamed";
            Console.WriteLine($"[LoadState] Restoring state for {entityId}...");
            Console.WriteLine($"[LoadState]   Entity position before restore: ({entity.Position.X}, {entity.Position.Y})");
            
            try
            {
                RestoreEntityState(entity, element, system, mergeExisting);
                Console.WriteLine($"[LoadState]   Position/Rotation restored for {entityId} -> ({entity.Position.X}, {entity.Position.Y})");
                
                LoadEntityComponents(entity, element);
                Console.WriteLine($"[LoadState]   Components loaded for {entityId}");

                // Now start the entity after state is restored so OnStart uses correct position
                if (!entity.HasStarted)
                {
                    entity.OnStart();
                    Console.WriteLine($"[LoadState]   OnStart completed for {entityId} -> pos=({entity.Position.X}, {entity.Position.Y})");
                }
                else
                {
                    Console.WriteLine($"[LoadState]   Skipping OnStart (already started) for {entityId}");
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine($"[LoadState]   ERROR restoring {entityId}: {ex.GetType().Name}: {ex.Message}");
                Console.WriteLine($"[LoadState]   Stack: {ex.StackTrace?.Split('\n').Take(5).Aggregate((a, b) => a + "\n" + b)}");
                throw;
            }
            
            // Handle children - restore state and start them too
            var childrenElement = element.Element(ChildrenElement);
            if (childrenElement != null)
            {
                foreach (var childElement in childrenElement.Elements(EntityElement))
                {
                    var childEntity = CreateEntityFromElement(childElement, system, idToEntity);
                    if (childEntity != null)
                    {
                        // Restore child state (position, rotation, tags) before starting
                        RestoreEntityState(childEntity, childElement, system, mergeExisting);
                        LoadEntityComponents(childEntity, childElement);
                        childEntity.OnStart();

                        entity.AddChild(childEntity);
                        if (!string.IsNullOrEmpty(childEntity.Id))
                        {
                            idToEntity[childEntity.Id] = childEntity;
                        }
                    }
                }
            }
        }
        Console.WriteLine($"[LoadState] === All entities restored ===");
    }

    private static XDocument CreateGameStateDocument(EntitySystem system)
    {
        var entities = system.GetEntities().Where(e => e.Id != null).ToList();
        Console.WriteLine($"[SaveState] Saving {entities.Count} entities");
        foreach (var e in entities.Take(5))
        {
            Console.WriteLine($"[SaveState]   {e.Id}: pos=({e.Position.X}, {e.Position.Y})");
        }

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

        // Serialize public Vector2 properties (like WorldBorder.Size)
        var vector2Props = entity.GetType().GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)
            .Where(p => p.PropertyType == typeof(Vector2) && p.CanRead && p.CanWrite && p.Name != nameof(Entity.Position))
            .ToList();
        foreach (var prop in vector2Props)
        {
            var value = (Vector2)prop.GetValue(entity)!;
            element.Add(new XElement(prop.Name,
                new XAttribute("X", value.X.ToString(CultureInfo.InvariantCulture)),
                new XAttribute("Y", value.Y.ToString(CultureInfo.InvariantCulture))
            ));
        }

        // Serialize components
        if (entity.Components.Any())
        {
            var componentsElement = new XElement(ComponentsElement);
            foreach (var component in entity.Components)
            {
                if (component is ISerializableComponent serializable)
                {
                    var componentElement = new XElement(ComponentElement,
                        new XAttribute("Type", component.GetType().FullName),
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

        Console.WriteLine($"[LoadState] Creating entity: Id={id}, Type={typeName}");

        if (string.IsNullOrWhiteSpace(typeName))
        {
            Console.WriteLine($"[LoadState]   SKIP: type name is empty");
            return null;
        }

        // Check if entity already exists in the system (for merge mode)
        if (!string.IsNullOrWhiteSpace(id))
        {
            var existingInSystem = system.GetEntities().FirstOrDefault(e => e.Id == id);
            if (existingInSystem != null)
            {
                Console.WriteLine($"[LoadState]   REUSE: entity already in system");
                idToEntity[id] = existingInSystem;
                return existingInSystem;
            }
            
            // Check if entity already created in this load operation
            if (idToEntity.TryGetValue(id, out var existingEntity))
            {
                Console.WriteLine($"[LoadState]   REUSE: entity already created this load");
                return existingEntity;
            }
        }

        // Create new entity using reflection
        var entityType = Type.GetType(typeName) ?? 
            AppDomain.CurrentDomain.GetAssemblies()
                .Select(a => a.GetType(typeName))
                .FirstOrDefault(t => t != null);
        
        if (entityType == null)
        {
            Console.WriteLine($"[LoadState]   FAIL: could not resolve type {typeName}");
            return null;
        }
        
        if (!typeof(Entity).IsAssignableFrom(entityType))
        {
            Console.WriteLine($"[LoadState]   FAIL: {typeName} is not an Entity subtype");
            return null;
        }

        // Create entity without triggering OnStart yet — position/rotation will be restored first in pass 2
        Console.WriteLine($"[LoadState]   Creating {typeName} (unstarted)...");
        try
        {
            var entity = system.CreateEntityUnstarted(entityType, Array.Empty<object>());
            
            // Set saved ID before OnStart so no auto-ID collision
            if (!string.IsNullOrWhiteSpace(id))
            {
                entity.SetId(id);
                idToEntity[id] = entity;
                Console.WriteLine($"[LoadState]   Set ID to: {id}");
            }
            else
            {
                Console.WriteLine($"[LoadState]   Created entity: {entity.GetType().Name}, Id={entity.Id}");
            }

            // NOTE: OnStart() is deferred to pass 2, after position/rotation are restored
            return entity;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[LoadState]   ERROR creating {typeName}: {ex.GetType().Name}: {ex.Message}");
            Console.WriteLine($"[LoadState]   Stack: {ex.StackTrace?.Split('\n').Take(5).Aggregate((a, b) => a + "\n" + b)}");
            throw;
        }
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

        // Restore public Vector2 properties (like WorldBorder.Size)
        var vector2Props = entity.GetType().GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)
            .Where(p => p.PropertyType == typeof(Vector2) && p.CanWrite && p.Name != nameof(Entity.Position))
            .ToList();
        foreach (var prop in vector2Props)
        {
            var propElement = element.Element(prop.Name);
            if (propElement != null)
            {
                if (float.TryParse(propElement.Attribute("X")?.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out float x) &&
                    float.TryParse(propElement.Attribute("Y")?.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out float y))
                {
                    prop.SetValue(entity, new Vector2(x, y));
                }
            }
        }
    }

    private static void LoadEntityComponents(Entity entity, XElement element)
    {
        var componentsElement = element.Element(ComponentsElement);
        Console.WriteLine($"[LoadEntityComponents] Looking for '<{ComponentsElement}>' in entity {entity.Id}, found={componentsElement != null}");
        
        if (componentsElement == null)
            return;

        Console.WriteLine($"[LoadEntityComponents] Found {componentsElement.Elements(ComponentElement).Count()} components to process");

        foreach (var componentElement in componentsElement.Elements(ComponentElement))
        {
            var typeName = componentElement.Attribute("Type")?.Value;
            Console.WriteLine($"[LoadEntityComponents] Processing component type: {typeName}");
            
            if (string.IsNullOrWhiteSpace(typeName))
            {
                Console.WriteLine($"[LoadEntityComponents]   SKIP: typeName is empty");
                continue;
            }

            // Find existing component or create new one
            var existingComponent = entity.Components.FirstOrDefault(c => 
                c.GetType().FullName.Equals(typeName, StringComparison.OrdinalIgnoreCase));

            EntityComponent component;
            if (existingComponent != null)
            {
                Console.WriteLine($"[LoadEntityComponents]   Found existing component");
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

                    Console.WriteLine($"[LoadEntityComponents]   Resolved type: {componentType?.FullName ?? "NULL"}");

                    if (componentType != null && typeof(EntityComponent).IsAssignableFrom(componentType))
                    {
                        component = (EntityComponent)Activator.CreateInstance(componentType)!;
                        component.Owner = entity;
                        entity.AddComponent(component);
                        Console.WriteLine($"[LoadEntityComponents]   Created new component: {component.GetType().Name}");
                    }
                    else
                    {
                        Console.WriteLine($"[LoadEntityComponents]   SKIP: type is not an EntityComponent");
                        continue;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[LoadEntityComponents]   ERROR creating component: {ex.Message}");
                    continue;
                }
            }

            // Deserialize if component supports serialization
            if (component is ISerializableComponent serializable)
            {
                var stateElement = componentElement.Elements().FirstOrDefault();
                if (stateElement != null)
                {
                    Console.WriteLine($"[LoadEntityComponents] Deserializing {component.GetType().Name} for entity {entity.Id}");
                    serializable.DeserializeFromXml(stateElement);
                    Console.WriteLine($"[LoadEntityComponents] Done deserializing {component.GetType().Name}");
                }
                else
                {
                    Console.WriteLine($"[LoadEntityComponents] WARNING: No state element found for {component.GetType().Name}");
                }
            }
            else
            {
                Console.WriteLine($"[LoadEntityComponents] {component.GetType().Name} is NOT serializable");
            }
        }
    }
}
