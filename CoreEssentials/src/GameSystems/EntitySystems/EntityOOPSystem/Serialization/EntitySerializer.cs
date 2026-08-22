using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml.Linq;
using System.Globalization;
using Microsoft.Xna.Framework;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Components;

namespace CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Serialization;

/// <summary>
/// Static utility class for serializing and deserializing entities from XML.
/// Mirrors the pattern established by <see cref="GUI.GuiSerializer"/> for UI widgets.
/// </summary>
public static class EntitySerializer
{
    private const string MalformedXmlMessage = "Malformed XML or unexpected root element for entity definition.";
    private const string EntityElement = "EntityDefinition";

    /// <summary>
    /// Loads a single entity of the specified type from an XML string and adds it to the given EntitySystem.
    /// </summary>
    /// <typeparam name="T">The concrete Entity type to create.</typeparam>
    /// <param name="xmlData">The XML string containing the entity definition.</param>
    /// <param name="system">The EntitySystem that will manage the loaded entity.</param>
    /// <param name="componentFactory">Optional factory for creating components. Uses built-in types if null.</param>
    /// <returns>The newly created and configured entity.</returns>
    /// <exception cref="FormatException">Thrown when the XML is malformed or missing required elements.</exception>
    public static T LoadEntity<T>(string xmlData, EntitySystem system, IComponentFactory? componentFactory = null) where T : Entity
    {
        var element = ParseRootElement(xmlData, "Entity");

        var entity = system.CreateEntity<T>();
        ApplyEntityProperties(entity, element);

        // Load components if defined
        var componentsElement = element.Element("Components");
        if (componentsElement != null)
        {
            var factory = componentFactory ?? CreateDefaultComponentFactory();
            LoadComponents(entity, componentsElement, factory);
        }

        return entity;
    }

    /// <summary>
    /// Loads a single entity of the specified type from an XML file and adds it to the given EntitySystem.
    /// </summary>
    /// <typeparam name="T">The concrete Entity type to create.</typeparam>
    /// <param name="filePath">The path to the XML file containing the entity definition.</param>
    /// <param name="system">The EntitySystem that will manage the loaded entity.</param>
    /// <param name="componentFactory">Optional factory for creating components. Uses built-in types if null.</param>
    /// <returns>The newly created and configured entity.</returns>
    /// <exception cref="FormatException">Thrown when the XML is malformed or missing required elements.</exception>
    /// <exception cref="FileNotFoundException">Thrown when the file does not exist.</exception>
    public static T LoadEntityFromFile<T>(string filePath, EntitySystem system, IComponentFactory? componentFactory = null) where T : Entity
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"Entity definition file not found: {filePath}");

        var xmlData = File.ReadAllText(filePath);
        return LoadEntity<T>(xmlData, system, componentFactory);
    }

    /// <summary>
    /// Saves the specified entity to an XML file.
    /// </summary>
    /// <param name="entity">The entity to serialize.</param>
    /// <param name="filePath">The path to save the XML file to.</param>
    public static void SaveEntity(Entity entity, string filePath)
    {
        var document = CreateEntityDocument(entity);
        document.Save(filePath);
    }

    /// <summary>
    /// Serializes the specified entity to an XML string.
    /// </summary>
    /// <param name="entity">The entity to serialize.</param>
    /// <returns>An XML string representing the entity definition.</returns>
    public static string SaveEntityToString(Entity entity)
    {
        var document = CreateEntityDocument(entity);
        return document.ToString();
    }

    #region Scene Loading (T3)

    /// <summary>
    /// Loads a complete scene from an XML file.
    /// Parses multiple <c>&lt;EntityDefinition&gt;</c> elements, supports <c>&lt;Children&gt;</c> hierarchy and <c>&lt;Reference&gt;</c> linking by Id.
    /// </summary>
    /// <param name="filePath">The path to the scene XML file.</param>
    /// <param name="system">The EntitySystem that will manage the loaded entities.</param>
    /// <param name="componentFactory">Optional factory for creating components. Uses built-in types if null.</param>
    /// <returns>A list of all root entities loaded from the scene.</returns>
    public static List<Entity> LoadSceneFromFile(string filePath, EntitySystem system, IComponentFactory? componentFactory = null)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"Scene definition file not found: {filePath}");

        var xmlData = File.ReadAllText(filePath);
        return LoadSceneFromXml(xmlData, system, componentFactory);
    }

    /// <summary>
    /// Loads a complete scene from an XML string.
    /// </summary>
    /// <param name="xmlData">The XML string containing the scene definition with <c>&lt;Scene&gt;</c> root and <c>&lt;EntityDefinition&gt;</c> children.</param>
    /// <param name="system">The EntitySystem that will manage the loaded entities.</param>
    /// <param name="componentFactory">Optional factory for creating components.</param>
    /// <returns>A list of all root entities loaded from the scene.</returns>
    public static List<Entity> LoadSceneFromXml(string xmlData, EntitySystem system, IComponentFactory? componentFactory = null)
    {
        var root = ParseRootElement(xmlData, "Scene");
        var factory = componentFactory ?? CreateDefaultComponentFactory();

        // Two-pass loading: first create all entities, then resolve references
        var idToEntity = new Dictionary<string, Entity>();
        var rootEntities = new List<Entity>();

        // First pass - create all entities and instantiate templates
        foreach (var element in root.Elements())
        {
            if (element.Name.LocalName == EntityElement)
            {
                var entity = LoadEntityFromDefinition(element, system, factory, idToEntity);
                rootEntities.Add(entity);
            }
            else if (element.Name.LocalName == "Template")
            {
                var entity = LoadEntityFromTemplate(element, system, idToEntity);
                rootEntities.Add(entity);
            }
        }

        // Second pass - resolve <Reference> links
        foreach (var element in root.Elements())
        {
            if (element.Name.LocalName == EntityElement || element.Name.LocalName == "Template")
            {
                ResolveReferences(element, idToEntity, rootEntities);
            }
        }

        return rootEntities;
    }

    private static Entity LoadEntityFromTemplate(XElement templateElem, EntitySystem system, Dictionary<string, Entity> idToEntity)
    {
        var source = templateElem.Attribute("Source")?.Value;
        if (string.IsNullOrWhiteSpace(source))
            throw new FormatException("Template element missing required 'Source' attribute.");

        // Parse position override
        var posElement = templateElem.Element("Position");
        Vector2 position = Vector2.Zero;
        if (posElement != null)
        {
            position = ParseVector2(posElement);
        }
        else if (float.TryParse(templateElem.Attribute("X")?.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out float x) &&
                 float.TryParse(templateElem.Attribute("Y")?.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out float y))
        {
            position = new Vector2(x, y);
        }

        // Instantiate from system
        var entity = system.Instantiate(source, position);

        // Track by Id if available for reference resolution
        var entityId = templateElem.Attribute("Id")?.Value;
        if (!string.IsNullOrWhiteSpace(entityId))
        {
            idToEntity[entityId] = entity;
        }

        // Support overrides (Tags, etc.)
        var tagsElement = templateElem.Element("Tags");
        if (tagsElement != null)
        {
            foreach (var tagElement in tagsElement.Elements("Tag"))
            {
                var tagName = tagElement.Attribute("Name")?.Value;
                if (!string.IsNullOrWhiteSpace(tagName))
                    entity.SetTag(tagName);
            }
        }

        return entity;
    }

    private static Entity LoadEntityFromDefinition(XElement entityDef, EntitySystem system, IComponentFactory factory, Dictionary<string, Entity> idToEntity)
    {
        var entityType = entityDef.Attribute("Type")?.Value;
        if (string.IsNullOrWhiteSpace(entityType))
            throw new FormatException($"EntityDefinition missing required 'Type' attribute.");

        // Create entity by type name using reflection
        var entity = CreateEntityByTypeName(entityType, system);

        // Track by Id if available
        var entityId = entityDef.Attribute("Id")?.Value;
        if (!string.IsNullOrWhiteSpace(entityId))
        {
            idToEntity[entityId] = entity;
        }

        // Apply properties (position, rotation, sort, tags, active)
        ApplyEntityProperties(entity, entityDef);

        // Load components - support both <Components><Component .../></Components> and direct <Component .../>
        var componentsElement = entityDef.Element("Components");
        if (componentsElement != null)
        {
            LoadComponents(entity, componentsElement, factory);
        }
        else
        {
            // Support direct <Component> children without wrapper - create a virtual wrapper
            foreach (var componentElement in entityDef.Elements("Component"))
            {
                var typeName = componentElement.Attribute("Type")?.Value;
                if (!string.IsNullOrWhiteSpace(typeName))
                {
                    LoadComponent(entity, componentElement, factory);
                }
            }
        }

        // Load nested children from &lt;Children&gt; element
        var childrenElement = entityDef.Element("Children");
        if (childrenElement != null)
        {
            foreach (var childDef in childrenElement.Elements("EntityDefinition"))
            {
                var child = LoadEntityFromDefinition(childDef, system, factory, idToEntity);
                entity.AddChild(child);
            }
        }

        return entity;
    }

    private static void ResolveReferences(XElement entityDef, Dictionary<string, Entity> idToEntity, List<Entity> rootEntities)
    {
        var references = entityDef.Element("References");
        if (references == null)
            return;

        // Find the actual entity for this definition by Id
        var entityId = entityDef.Attribute("Id")?.Value;
        if (string.IsNullOrWhiteSpace(entityId) || !idToEntity.TryGetValue(entityId, out var targetEntity))
            return;

        foreach (var reference in references.Elements("Reference"))
        {
            var refName = reference.Attribute("Name")?.Value;
            var refTargetId = reference.Attribute("TargetId")?.Value;

            if (string.IsNullOrWhiteSpace(refName) || string.IsNullOrWhiteSpace(refTargetId))
                continue;

            if (idToEntity.TryGetValue(refTargetId, out var referencedEntity))
            {
                // Set the reference as a named property on the entity using reflection
                SetReference(targetEntity, refName, referencedEntity);
            }
        }

        // Recursively resolve references for nested children
        var childrenElement = entityDef.Element("Children");
        if (childrenElement != null)
        {
            foreach (var childDef in childrenElement.Elements("EntityDefinition"))
            {
                ResolveReferences(childDef, idToEntity, rootEntities);
            }
        }
    }

    private static void SetReference(Entity target, string name, Entity reference)
    {
        // Try to set a property on the entity with this name
        var property = target.GetType().GetProperty(name, BindingFlags.Instance | BindingFlags.Public);
        if (property != null && property.PropertyType.IsAssignableFrom(typeof(Entity)))
        {
            property.SetValue(target, reference);
        }
        // If no matching property exists, silently skip - forward-compatible for future entity subclasses
    }

    #endregion

    #region Private Helpers

    private static Entity CreateEntityByTypeName(string typeName, EntitySystem system)
    {
        // Try to find type across all loaded assemblies
        Type? type = null;

        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            try
            {
                var foundType = assembly.GetType(typeName);
                if (foundType != null && typeof(Entity).IsAssignableFrom(foundType))
                {
                    type = foundType;
                    break;
                }

                // Fallback: search by name only
                if (type == null)
                {
                    var candidates = assembly.GetTypes()
                        .Where(t => t.Name.Equals(typeName, StringComparison.OrdinalIgnoreCase) && typeof(Entity).IsAssignableFrom(t));
                    type = candidates.FirstOrDefault();
                }
            }
            catch (ReflectionTypeLoadException)
            {
                // Skip assemblies that can't be loaded
            }
        }

        if (type == null || !typeof(Entity).IsAssignableFrom(type))
            throw new FormatException($"Entity type '{typeName}' not found or does not inherit from Entity.");

        // Use the non-generic CreateEntity method for proper registration
        return system.CreateEntity(type);
    }

    private static XElement ParseRootElement(string xmlData, string expectedName)
    {
        try
        {
            var doc = XDocument.Parse(xmlData);
            var root = doc.Root;

            if (root == null || !string.Equals(root.Name.LocalName, expectedName, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException($"Root element must be <{expectedName}>.");
            }

            return root;
        }
        catch (Exception ex) when (ex is System.Xml.XmlException || ex is InvalidOperationException)
        {
            throw new FormatException($"{MalformedXmlMessage} {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Applies entity properties from an XML element to an existing entity.
    /// This includes position, rotation, sort order, tags, active state, and ID.
    /// </summary>
    /// <param name="entity">The entity to configure.</param>
    /// <param name="element">The XML element containing entity properties.</param>
    public static void ApplyEntityProperties(Entity entity, XElement element)
    {
        // ID
        var idAttribute = element.Attribute("Id")?.Value;
        if (!string.IsNullOrWhiteSpace(idAttribute))
        {
            entity.SetId(idAttribute);
        }

        // Position
        var positionElement = element.Element("Position");
        if (positionElement != null)
        {
            entity.Position = ParseVector2(positionElement);
        }

        // Rotation
        if (float.TryParse(element.Attribute("Rotation")?.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out float rotation))
        {
            entity.Rotation = rotation;
        }

        // Sort order
        if (int.TryParse(element.Attribute("Sort")?.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out int sort))
        {
            entity.SetSort(sort);
        }

        // Tags
        var tagsElement = element.Element("Tags");
        if (tagsElement != null)
        {
            foreach (var tagElement in tagsElement.Elements("Tag"))
            {
                var tagName = tagElement.Attribute("Name")?.Value;
                if (!string.IsNullOrWhiteSpace(tagName))
                {
                    entity.SetTag(tagName);
                }
            }
        }

        // Active state
        if (bool.TryParse(element.Attribute("Active")?.Value, out bool active))
        {
            entity.SetActive(active);
        }
    }

    private static XDocument CreateEntityDocument(Entity entity)
    {
        var document = new XDocument(
            new XElement("Entity",
                new XAttribute("Type", entity.GetType().Name),
                new XElement("Position",
                    new XAttribute("X", entity.Position.X.ToString(CultureInfo.InvariantCulture)),
                    new XAttribute("Y", entity.Position.Y.ToString(CultureInfo.InvariantCulture))
                ),
                new XAttribute("Rotation", entity.Rotation.ToString(CultureInfo.InvariantCulture)),
                new XAttribute("Sort", entity.GetSort()),
                new XAttribute("Active", entity.GetActive()),
                new XElement("Tags",
                    from tag in entity.Tags
                    select new XElement("Tag", new XAttribute("Name", tag))
                )
            )
        );

        return document;
    }

    private static Vector2 ParseVector2(XElement element)
    {
        var x = 0f;
        var y = 0f;

        if (float.TryParse(element.Attribute("X")?.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out float parsedX))
            x = parsedX;

        if (float.TryParse(element.Attribute("Y")?.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out float parsedY))
            y = parsedY;

        return new Vector2(x, y);
    }

    private static void LoadComponents(Entity entity, XElement componentsElement, IComponentFactory factory)
    {
        foreach (var componentElement in componentsElement.Elements("Component"))
        {
            var typeName = componentElement.Attribute("Type")?.Value;
            if (!string.IsNullOrWhiteSpace(typeName))
            {
                LoadComponent(entity, componentElement, factory);
            }
        }
    }

    private static void LoadComponent(Entity entity, XElement componentElement, IComponentFactory factory)
    {
        string? typeName = componentElement.Attribute("Type")?.Value;
        if (string.IsNullOrWhiteSpace(typeName))
            return;

        var existingComponent = GetExistingComponent(entity, typeName);
        EntityComponent? component = existingComponent ?? factory.Create(typeName);
        
        if (component == null)
            return;

        ApplyProperties(entity, component, componentElement);

        if (existingComponent == null)
        {
            entity.AddComponent(component);
        }
    }

    /// <summary>Applies all Property elements from the XML to the component or entity.</summary>
    private static void ApplyProperties(Entity entity, EntityComponent component, XElement componentElement)
    {
        var propertiesElement = componentElement.Element("Properties");
        if (propertiesElement == null)
            return;

        foreach (var propertyElement in propertiesElement.Elements("Property"))
        {
            var propertyName = propertyElement.Attribute("Name")?.Value;
            var propertyValue = propertyElement.Attribute("Value")?.Value;

            if (string.IsNullOrWhiteSpace(propertyName) || string.IsNullOrWhiteSpace(propertyValue))
                continue;

            HandleSpecialProperties(entity, component, propertyName, propertyValue);
            SetProperty(component, propertyName, propertyValue);
        }
    }

    /// <summary>Handles properties that have special migration logic (e.g., Scale moved from SpriteComponent to Entity).</summary>
    private static void HandleSpecialProperties(Entity entity, EntityComponent component, string propertyName, string propertyValue)
    {
        if (propertyName != "Scale" || component is not Components.BuiltIn.SpriteComponent)
            return;

        var parts = propertyValue.Split(',');
        if (parts.Length != 2)
            return;

        if (!float.TryParse(parts[0], NumberStyles.Any, CultureInfo.InvariantCulture, out float scaleX))
            return;
        if (!float.TryParse(parts[1], NumberStyles.Any, CultureInfo.InvariantCulture, out float scaleY))
            return;

        entity.Scale = new Vector2(scaleX, scaleY);
    }

    private static EntityComponent? GetExistingComponent(Entity entity, string typeName) =>
        entity.Components.FirstOrDefault(c => c.GetType().Name.Equals(typeName, StringComparison.OrdinalIgnoreCase));

    private static void SetProperty(object target, string propertyName, string valueString)
    {
        var property = target.GetType().GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public);
        if (property == null || !property.CanWrite)
            return;

        try
        {
            property.SetValue(target, SerializationUtils.ParseValue(property.PropertyType, valueString));
        }
        catch (Exception)
        {
            // Silently skip properties we can't parse
        }
    }

    #region Helpers from SerializationUtils
    // The following methods are now handled by SerializationUtils to avoid duplication
    // Vector2 ParseVector2FromString(string value) { ... }
    // Color ParseColor(string value) { ... }
    #endregion

    private static IComponentFactory CreateDefaultComponentFactory()
    {
        var factory = new DefaultComponentFactory();
        factory.RegisterBuiltIns();
        return factory;
    }

    #endregion
}

/// <summary>
/// Factory interface for creating entity components from XML definitions.
/// </summary>
public interface IComponentFactory
{
    /// <summary>
    /// Creates a new component instance of the specified type name.
    /// Returns null if the type is not registered.
    /// </summary>
    EntityComponent? Create(string typeName);

    /// <summary>
    /// Registers a component type with the factory.
    /// </summary>
    void Register<T>(string typeName) where T : EntityComponent, new();

    /// <summary>
    /// Registers a component type with a custom factory function (for components requiring constructor args).
    /// </summary>
    void Register(string typeName, Func<EntityComponent> factory);

    /// <summary>
    /// Registers all built-in components (SpriteComponent, RigidbodyComponent, ColliderComponent).
    /// </summary>
    void RegisterBuiltIns();
}

/// <summary>
/// Default implementation of IComponentFactory using reflection to create components.
/// </summary>
public class DefaultComponentFactory : IComponentFactory
{
    private readonly Dictionary<string, Func<EntityComponent>> _factories = new();

    /// <inheritdoc />
    public EntityComponent? Create(string typeName)
    {
        if (_factories.TryGetValue(typeName, out var factory))
            return factory();

        // Fallback: try to resolve by fully qualified type name
        var type = Type.GetType(typeName);
        if (type != null && typeof(EntityComponent).IsAssignableFrom(type) && !type.IsAbstract)
            return (EntityComponent)Activator.CreateInstance(type)!;

        return null;
    }

    /// <inheritdoc />
    public void Register<T>(string typeName) where T : EntityComponent, new()
    {
        _factories[typeName] = () => new T();
    }

    /// <inheritdoc />
    public void Register(string typeName, Func<EntityComponent> factory)
    {
        _factories[typeName] = factory;
    }

    /// <inheritdoc />
    public void RegisterBuiltIns()
    {
        // SpriteComponent has a parameterless constructor
        Register<Components.BuiltIn.SpriteComponent>("SpriteComponent");

        // AnimationComponent has a parameterless constructor
        Register<Components.BuiltIn.AnimationComponent>("AnimationComponent");

        // RigidbodyComponent requires a type param, use factory function with default Dynamic
        Register("RigidbodyComponent", () => new Components.BuiltIn.RigidbodyComponent());

        // ColliderComponent requires constructor args, use factory function with default circle collider
        Register("ColliderComponent", () => new Components.BuiltIn.ColliderComponent(radius: 1f));
    }
}
