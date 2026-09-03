using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

        // Wire up declarative <Bind> event-to-command subscriptions now that all
        // components on this entity are attached.
        CommandBindings.ApplyBindings(entity, element);

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

    #region Private Helpers

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
        {
            Console.WriteLine($"[Serialization] Could not create component '{typeName}' for entity {entity.Id} — no matching registration in the component factory; skipping.");
            return;
        }

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

    private static IComponentFactory CreateDefaultComponentFactory() => new DefaultComponentFactory();

    #endregion
}

/// <summary>
/// Factory interface for creating entity components from XML definitions.
/// </summary>
public interface IComponentFactory
{
    /// <summary>
    /// Creates a new component instance of the specified type name.
    /// Resolution order: explicit registrations, fully qualified type names, then discovery
    /// (any concrete <see cref="EntityComponent"/> subclass in a loaded assembly, matched by simple
    /// name). Returns null if the type cannot be resolved.
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
    /// Registers all built-in components (SpriteComponent, AnimationComponent, RigidbodyComponent,
    /// ColliderComponent, the GUI components CanvasComponent, LabelComponent, ButtonComponent and
    /// AnchorComponent, and CameraComponent). <see cref="DefaultComponentFactory"/> calls this in
    /// its constructor, so it is idempotent; register custom components with one of the
    /// <see cref="Register{T}"/> overloads before loading scenes that reference them.
    /// </summary>
    void RegisterBuiltIns();
}

/// <summary>
/// Default implementation of IComponentFactory using reflection to create components.
/// Beyond explicit registrations and fully qualified names, it discovers any concrete
/// <see cref="EntityComponent"/> subclass in a loaded assembly by simple name (Unity-style:
/// if you wrote the component, XML can reference it), so custom components need no
/// registration unless they require constructor arguments.
/// </summary>
public class DefaultComponentFactory : IComponentFactory
{
    private readonly Dictionary<string, Func<EntityComponent>> _factories = new();

    // Process-wide discovery state: discovered types are cached by simple name and each
    // assembly is scanned at most once, so assemblies loaded later are picked up on the next
    // miss without re-scanning earlier ones.
    private static readonly Dictionary<string, Type> _discoveredTypes = new(StringComparer.OrdinalIgnoreCase);
    private static readonly HashSet<Assembly> _scannedAssemblies = new();
    private static readonly object _discoveryLock = new();

    /// <summary>
    /// Creates a factory with all built-in components pre-registered. Register additional
    /// custom components after construction — they are added to the built-ins rather than
    /// replacing them, so scenes can mix built-in and custom components freely.
    /// </summary>
    public DefaultComponentFactory()
    {
        RegisterBuiltIns();
    }

    /// <inheritdoc />
    public EntityComponent? Create(string typeName)
    {
        if (_factories.TryGetValue(typeName, out var factory))
            return factory();

        // Fallback: try to resolve by fully qualified type name
        var type = Type.GetType(typeName);
        if (type != null && typeof(EntityComponent).IsAssignableFrom(type) && !type.IsAbstract)
            return (EntityComponent)Activator.CreateInstance(type)!;

        // Discovery: match a concrete EntityComponent subclass by simple name across loaded assemblies.
        return CreateDiscovered(typeName);
    }

    private static EntityComponent? CreateDiscovered(string typeName)
    {
        Type? type;
        lock (_discoveryLock)
        {
            if (!_discoveredTypes.TryGetValue(typeName, out type))
            {
                ScanForComponents();
                _discoveredTypes.TryGetValue(typeName, out type);
            }
        }

        if (type == null)
            return null;

        try
        {
            return (EntityComponent)Activator.CreateInstance(type)!;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Serialization] Could not instantiate discovered component '{typeName}': {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Scans all loaded assemblies (skipping ones already scanned) for public, non-abstract,
    /// non-nested <see cref="EntityComponent"/> subclasses with a public parameterless constructor,
    /// indexing them by simple name. Duplicate names keep the first match and log a warning.
    /// </summary>
    private static void ScanForComponents()
    {
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            if (!_scannedAssemblies.Add(assembly))
                continue;

            Type[] types;
            try
            {
                types = assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                types = ex.Types.Where(t => t != null).ToArray()!;
            }

            foreach (var candidate in types)
            {
                if (!candidate.IsPublic || candidate.IsAbstract || candidate.IsNested)
                    continue;
                if (!typeof(EntityComponent).IsAssignableFrom(candidate))
                    continue;
                if (candidate.GetConstructor(Type.EmptyTypes) == null)
                    continue;

                if (!_discoveredTypes.TryAdd(candidate.Name, candidate))
                {
                    Console.WriteLine($"[Serialization] Duplicate component name '{candidate.Name}' discovered in {assembly.GetName().Name} — keeping the first match.");
                }
            }
        }
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

        // GUI components. Registered so data-driven XML scenes can compose canvases and
        // widgets without any game-layer code. CanvasComponent's constructor takes an optional
        // argument, so it needs the factory-function overload instead of Register<T>.
        Register("CanvasComponent", () => new Components.BuiltIn.CanvasComponent());
        Register<Components.BuiltIn.LabelComponent>("LabelComponent");
        Register<Components.BuiltIn.ButtonComponent>("ButtonComponent");
        Register<Components.BuiltIn.AnchorComponent>("AnchorComponent");
        Register<Components.BuiltIn.CameraComponent>("CameraComponent");
    }
}
