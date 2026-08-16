using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using CoreEssentials.Assets;
using Microsoft.Xna.Framework;

namespace CoreEssentials.GameSystems.Physics.Types;

/// <summary>
/// Loads and owns the physics engine's declarative configuration from an XML file.
/// <para>
/// A dedicated <c>PhysicsConfig.xml</c> lets developers give collision categories
/// meaningful names (e.g. <c>Player</c>, <c>Vip</c>, <c>Wall</c>) instead of the raw
/// <see cref="CollisionCategory.Cat1"/> bit names, and to tune engine settings
/// (gravity, solver iterations) without touching code.
/// </para>
/// <para>
/// This type is engine-agnostic: it only ever produces <see cref="CollisionCategory"/>
/// values and plain settings, so the underlying physics engine can be swapped without
/// changing configuration.
/// </para>
/// <para>
/// Categories are assigned bits by their <b>order of appearance</b> — the first
/// <c>Category</c> is bit 1, the second is bit 2, and so on (up to 31). No
/// explicit bit value is needed.
/// </para>
/// Example XML:
/// <code>
/// &lt;PhysicsConfig&gt;
///     &lt;Gravity X="0" Y="1000" /&gt;
///     &lt;Solver VelocityIterations="8" PositionIterations="3" /&gt;
///     &lt;Categories&gt;
///         &lt;Category Name="Player" /&gt;
///         &lt;Category Name="Vip" /&gt;
///         &lt;Category Name="Wall" /&gt;
///     &lt;/Categories&gt;
/// &lt;/PhysicsConfig&gt;
/// </code>
/// </summary>
public sealed class PhysicsConfig
{
    private readonly Dictionary<string, CollisionCategory> _nameToCategory =
        new(StringComparer.OrdinalIgnoreCase);

    private readonly Dictionary<CollisionCategory, string> _categoryToName = new();

    /// <summary>
    /// Gets the global gravity vector configured for the physics engine.
    /// Defaults to <see cref="Vector2.Zero"/> when the config omits a <c>Gravity</c> element.
    /// </summary>
    public Vector2 Gravity { get; }

    /// <summary>
    /// Gets the number of velocity iterations per solver step (default: 8).
    /// </summary>
    public int VelocityIterations { get; }

    /// <summary>
    /// Gets the number of position iterations per solver step (default: 3).
    /// </summary>
    public int PositionIterations { get; }

    /// <summary>
    /// Gets the collision categories defined in this config, in declaration order.
    /// </summary>
    public IReadOnlyCollection<CollisionCategory> Categories => _categoryToName.Keys;

    /// <summary>
    /// Gets the friendly name assigned to a collision category, if one was defined.
    /// </summary>
    /// <param name="category">The category to look up.</param>
    /// <returns>The friendly name, or <c>null</c> if the category has no name in this config.</returns>
    public string? GetCategoryName(CollisionCategory category)
    {
        return _categoryToName.TryGetValue(category, out var name) ? name : null;
    }

    private PhysicsConfig(Vector2 gravity, int velocityIterations, int positionIterations)
    {
        Gravity = gravity;
        VelocityIterations = velocityIterations;
        PositionIterations = positionIterations;
    }

    /// <summary>
    /// Creates a config with default settings and no named categories.
    /// Useful as a fallback when no config file is present.
    /// </summary>
    public static PhysicsConfig CreateDefault() => new(Vector2.Zero, 8, 3);

    /// <summary>
    /// Loads a config from an XML asset (resolved through the <see cref="AssetManager"/>).
    /// </summary>
    /// <param name="assetName">The name of the XML asset (e.g. <c>"PhysicsConfig.xml"</c>).</param>
    public static PhysicsConfig LoadFromAsset(string assetName)
    {
        var xmlAsset = AssetManager.LoadAsset<XMLAsset>(assetName);
        if (xmlAsset.XMLContent == null)
            throw new InvalidOperationException($"Physics config asset '{assetName}' has no content loaded.");

        return LoadFromXml(xmlAsset.XMLContent);
    }

    /// <summary>
    /// Parses a config from an XML string. Expects a root element named <c>PhysicsConfig</c>.
    /// </summary>
    /// <param name="xmlData">The XML content.</param>
    /// <exception cref="FormatException">Thrown when the root element is missing/misnamed or a category is invalid.</exception>
    public static PhysicsConfig LoadFromXml(string xmlData)
    {
        if (string.IsNullOrWhiteSpace(xmlData))
            throw new FormatException("Physics config XML is empty.");

        var doc = XDocument.Parse(xmlData);
        var root = doc.Root
            ?? throw new FormatException("Physics config XML is empty.");

        if (!string.Equals(root.Name.LocalName, "PhysicsConfig", StringComparison.OrdinalIgnoreCase))
            throw new FormatException("Root element must be 'PhysicsConfig'.");

        var solverElement = FindChild(root, "Solver");
        var config = new PhysicsConfig(
            gravity: ParseGravity(root),
            velocityIterations: ParseInt(solverElement, "VelocityIterations", 8),
            positionIterations: ParseInt(solverElement, "PositionIterations", 3));

        var categoriesElement = FindChild(root, "Categories");
        if (categoriesElement != null)
        {
            // Bits are assigned by order of appearance: first Category = bit 1, second = bit 2, etc.
            int bit = 1;
            foreach (var categoryElement in categoriesElement.Elements()
                         .Where(e => string.Equals(e.Name.LocalName, "Category", StringComparison.OrdinalIgnoreCase)))
            {
                config.AddCategory(categoryElement, bit);
                bit++;
            }
        }

        return config;
    }

    private static XElement? FindChild(XElement parent, string localName)
    {
        return parent.Elements()
            .FirstOrDefault(e => string.Equals(e.Name.LocalName, localName, StringComparison.OrdinalIgnoreCase));
    }

    private static Vector2 ParseGravity(XElement root)
    {
        var gravityElement = FindChild(root, "Gravity");
        if (gravityElement == null)
            return Vector2.Zero;

        return new Vector2(
            ParseFloat(gravityElement, "X", 0f),
            ParseFloat(gravityElement, "Y", 0f));
    }

    private static int ParseInt(XElement? element, string attributeName, int fallback)
    {
        return int.TryParse(
            element?.Attribute(attributeName)?.Value,
            NumberStyles.Integer,
            CultureInfo.InvariantCulture,
            out var value) ? value : fallback;
    }

    private static float ParseFloat(XElement element, string attributeName, float fallback)
    {
        return float.TryParse(
            element.Attribute(attributeName)?.Value,
            NumberStyles.Float,
            CultureInfo.InvariantCulture,
            out var value) ? value : fallback;
    }

    private void AddCategory(XElement categoryElement, int bit)
    {
        var name = categoryElement.Attribute("Name")?.Value;
        if (string.IsNullOrWhiteSpace(name))
            throw new FormatException("Category element is missing a 'Name' attribute.");

        // Bits are assigned by order (1-based). There are only 31 usable bits.
        if (bit > 31)
            throw new FormatException($"Too many categories: only 31 are supported, but '{name}' would be bit {bit}.");

        var category = (CollisionCategory)(1 << (bit - 1));

        if (_nameToCategory.ContainsKey(name))
            throw new FormatException($"Duplicate category name '{name}'.");

        _nameToCategory[name] = category;
        _categoryToName[category] = name;
    }

    /// <summary>
    /// Resolves a friendly category name to its <see cref="CollisionCategory"/> bit.
    /// </summary>
    /// <param name="name">The friendly name (case-insensitive).</param>
    /// <exception cref="KeyNotFoundException">Thrown when the name is not defined in this config.</exception>
    public CollisionCategory Resolve(string name)
    {
        if (name == null) throw new ArgumentNullException(nameof(name));
        if (!_nameToCategory.TryGetValue(name, out var category))
            throw new KeyNotFoundException($"Unknown collision category '{name}'. Defined categories: {string.Join(", ", _nameToCategory.Keys)}.");
        return category;
    }

    /// <summary>
    /// Attempts to resolve a friendly category name to its <see cref="CollisionCategory"/> bit.
    /// </summary>
    /// <param name="name">The friendly name (case-insensitive).</param>
    /// <param name="category">When true, the resolved category; otherwise <see cref="CollisionCategory.None"/>.</param>
    /// <returns><c>true</c> if the name was defined; otherwise <c>false</c>.</returns>
    public bool TryResolve(string name, out CollisionCategory category)
    {
        if (name != null && _nameToCategory.TryGetValue(name, out category))
            return true;

        category = CollisionCategory.None;
        return false;
    }

    /// <summary>
    /// Resolves a pipe-separated list of friendly category names into a combined mask.
    /// </summary>
    /// <param name="names">A pipe-separated list, e.g. <c>"Player|Vip"</c>. Empty/whitespace yields <see cref="CollisionCategory.None"/>.</param>
    /// <exception cref="KeyNotFoundException">Thrown when any name is not defined in this config.</exception>
    public CollisionCategory ResolveMask(string names)
    {
        if (string.IsNullOrWhiteSpace(names))
            return CollisionCategory.None;

        var mask = CollisionCategory.None;
        foreach (var part in names.Split('|', StringSplitOptions.RemoveEmptyEntries))
        {
            mask |= Resolve(part.Trim());
        }

        return mask;
    }
}
