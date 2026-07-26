using System;
using System.IO;
using System.Reflection;
using System.Xml.Linq;
using System.Globalization;
using Microsoft.Xna.Framework;
using CoreEssentials.GUI.Types;
using CoreEssentials.GUI.Factory;
using CoreEssentials.GUI.Internal;
using CoreEssentials.Assets;

namespace CoreEssentials.GUI;

/// <summary>
/// Static utility class for serializing UI layouts from XML strings or assets.
/// </summary>
public static class GuiSerializer
{
    /// <summary>
    /// Loads a label widget from an XML string.
    /// </summary>
    public static ILabel LoadLabelFromXml(string xmlData, IContentManager? contentManager = null)
    {
        var element = ParseRootElement(xmlData, "Label");
        
        string text = element.Attribute("Text")?.Value ?? "";
        var label = WidgetFactory.CreateLabel(text);
        
        ApplyBaseProperties(label, element);
        
        if (element.Attribute("TextColor") != null)
        {
            var colorValue = element.Attribute("TextColor")!.Value;
            if (colorValue.Equals("Red", StringComparison.OrdinalIgnoreCase)) label.TextColor = Color.Red;
            else if (colorValue.Equals("Green", StringComparison.OrdinalIgnoreCase)) label.TextColor = Color.Green;
            else if (colorValue.Equals("Blue", StringComparison.OrdinalIgnoreCase)) label.TextColor = Color.Blue;
            else if (colorValue.Equals("White", StringComparison.OrdinalIgnoreCase)) label.TextColor = Color.White;
            else if (colorValue.Equals("Black", StringComparison.OrdinalIgnoreCase)) label.TextColor = Color.Black;
            else if (colorValue.Equals("Yellow", StringComparison.OrdinalIgnoreCase)) label.TextColor = Color.Yellow;
        }

        // Note: Font loading would typically use contentManager here if a path was provided
        // but based on ILabel, Font is object?. We'll handle specific font loading in a later pass if needed
        // or assume the implementation of the interface handles the resource mapping.

        return label;
    }

    /// <summary>
    /// Loads a label widget from an XMLAsset.
    /// </summary>
    public static ILabel LoadLabelFromXml(XMLAsset asset, IContentManager? contentManager = null)
    {
        if (asset?.XMLContent == null)
            throw new ArgumentException("XMLAsset content is null. Ensure the asset is loaded.");
            
        return LoadLabelFromXml(asset.XMLContent, contentManager);
    }

    /// <summary>
    /// Loads a button widget from an XML string.
    /// </summary>
    public static IButton LoadButtonFromXml(string xmlData, IContentManager? contentManager = null)
    {
        var element = ParseRootElement(xmlData, "Button");
        
        string text = element.Attribute("Text")?.Value ?? "";
        var button = WidgetFactory.CreateTextButton(text);
        
        ApplyBaseProperties(button, element);
        
        return button;
    }

    /// <summary>
    /// Loads a button widget from an XMLAsset.
    /// </summary>
    public static IButton LoadButtonFromXml(XMLAsset asset, IContentManager? contentManager = null)
    {
        if (asset?.XMLContent == null)
            throw new ArgumentException("XMLAsset content is null. Ensure the asset is loaded.");
            
        return LoadButtonFromXml(asset.XMLContent, contentManager);
    }

    /// <summary>
    /// Loads a panel widget from an XML string.
    /// </summary>
    public static IPanel LoadPanelFromXml(string xmlData, IContentManager? contentManager = null)
    {
        var element = ParseRootElement(xmlData, "Panel");
        var panel = WidgetFactory.CreatePanel();
        
        ApplyBaseProperties(panel, element);
        
        if (element.Attribute("BorderThickness") != null)
        {
            panel.BorderThickness = ParseThickness(element.Attribute("BorderThickness")!.Value);
        }

        var panelBackground = ParseBackgroundAttribute(element);
        if (panelBackground != null) panel.Background = panelBackground;

        LoadChildren(panel, element, contentManager);

        return panel;
    }

    /// <summary>
    /// Loads a panel widget from an XMLAsset.
    /// </summary>
    public static IPanel LoadPanelFromXml(XMLAsset asset, IContentManager? contentManager = null)
    {
        if (asset?.XMLContent == null)
            throw new ArgumentException("XMLAsset content is null. Ensure the asset is loaded.");
            
        return LoadPanelFromXml(asset.XMLContent, contentManager);
    }

    /// <summary>
    /// Loads a grid widget from an XML string.
    /// </summary>
    public static IGrid LoadGridFromXml(string xmlData, IContentManager? contentManager = null)
    {
        var element = ParseRootElement(xmlData, "Grid");
        var grid = WidgetFactory.CreateGrid();
        
        ApplyBaseProperties(grid, element);
        
        if (float.TryParse(element.Attribute("RowSpacing")?.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out float rs))
            grid.RowSpacing = rs;
            
        if (float.TryParse(element.Attribute("ColumnSpacing")?.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out float cs))
            grid.ColumnSpacing = cs;

        var gridBackground = ParseBackgroundAttribute(element);
        if (gridBackground != null) grid.Background = gridBackground;

        LoadChildren(grid, element, contentManager);

        return grid;
    }

    /// <summary>
    /// Loads a grid widget from an XMLAsset.
    /// </summary>
    public static IGrid LoadGridFromXml(XMLAsset asset, IContentManager? contentManager = null)
    {
        if (asset?.XMLContent == null)
            throw new ArgumentException("XMLAsset content is null. Ensure the asset is loaded.");
            
        return LoadGridFromXml(asset.XMLContent, contentManager);
    }

    /// <summary>
    /// Loads a grid widget from an embedded resource by its logical name.
    /// </summary>
    public static IGrid LoadGridFromXmlEmbedded(string resourceName)
    {
        using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName);
        if (stream == null)
            throw new FileNotFoundException($"Embedded resource '{resourceName}' not found.");

        using var reader = new StreamReader(stream);
        var xmlData = reader.ReadToEnd();
        return LoadGridFromXml(xmlData);
    }

    /// <summary>
    /// Loads a widget of any supported type from an XML string.
    /// </summary>
    public static IWidget LoadFromXml(string xmlData, IContentManager? contentManager = null)
    {
        try
        {
            var doc = XDocument.Parse(xmlData);
            var root = doc.Root;
            if (root == null) throw new FormatException("XML document is empty.");

            string name = root.Name.LocalName;

            if (string.Equals(name, "Label", StringComparison.OrdinalIgnoreCase))
                return LoadLabelFromXml(xmlData, contentManager);
            if (string.Equals(name, "Button", StringComparison.OrdinalIgnoreCase))
                return LoadButtonFromXml(xmlData, contentManager);
            if (string.Equals(name, "Panel", StringComparison.OrdinalIgnoreCase))
                return LoadPanelFromXml(xmlData, contentManager);
            if (string.Equals(name, "Grid", StringComparison.OrdinalIgnoreCase))
                return LoadGridFromXml(xmlData, contentManager);

            throw new FormatException($"Unsupported root element <{name}>.");
        }
        catch (Exception ex) when (ex is System.Xml.XmlException || ex is FormatException)
        {
            throw new FormatException($"Error loading widget from XML: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Loads a widget of any supported type from an XMLAsset.
    /// </summary>
    public static IWidget LoadFromXml(XMLAsset asset, IContentManager? contentManager = null)
    {
        if (asset?.XMLContent == null)
            throw new ArgumentException("XMLAsset content is null. Ensure the asset is loaded.");
            
        return LoadFromXml(asset.XMLContent, contentManager);
    }

    private static void LoadChildren(IContainer container, XElement parentElement, IContentManager? contentManager)
    {
        foreach (var childElement in parentElement.Elements())
        {
            IWidget? childWidget = null;
            string name = childElement.Name.LocalName;

            if (string.Equals(name, "Label", StringComparison.OrdinalIgnoreCase))
                childWidget = (IWidget)LoadLabelFromXml(childElement.ToString(), contentManager);
            else if (string.Equals(name, "Button", StringComparison.OrdinalIgnoreCase))
                childWidget = (IWidget)LoadButtonFromXml(childElement.ToString(), contentManager);
            else if (string.Equals(name, "Panel", StringComparison.OrdinalIgnoreCase))
                childWidget = (IWidget)LoadPanelFromXml(childElement.ToString(), contentManager);
            else if (string.Equals(name, "Grid", StringComparison.OrdinalIgnoreCase))
                childWidget = (IWidget)LoadGridFromXml(childElement.ToString(), contentManager);

            if (childWidget != null)
            {
                container.AddChild(childWidget);
            }
        }
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
            throw new FormatException($"Malformed XML or unexpected root element for {expectedName}. {ex.Message}", ex);
        }
    }

    private static void ApplyBaseProperties(IWidget widget, XElement element)
    {
        // Width
        if (float.TryParse(element.Attribute("Width")?.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out float width))
            widget.Width = width;

        // Height
        if (float.TryParse(element.Attribute("Height")?.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out float height))
            widget.Height = height;

        // Visible
        if (bool.TryParse(element.Attribute("Visible")?.Value, out bool visible))
            widget.Visible = visible;

        // Enabled
        if (bool.TryParse(element.Attribute("Enabled")?.Value, out bool enabled))
            widget.Enabled = enabled;

        // Position
        var posX = element.Attribute("X")?.Value;
        var posY = element.Attribute("Y")?.Value;
        if (posX != null && posY != null)
        {
            if (float.TryParse(posX, NumberStyles.Any, CultureInfo.InvariantCulture, out float x) &&
                float.TryParse(posY, NumberStyles.Any, CultureInfo.InvariantCulture, out float y))
            {
                widget.Position = new Vector2(x, y);
            }
        }

        // Margin
        var marginAttr = element.Attribute("Margin")?.Value;
        if (marginAttr != null)
        {
            widget.Margin = ParseThickness(marginAttr);
        }
    }

    private static Thickness ParseThickness(string value)
    {
        var parts = value.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 1)
        {
            if (float.TryParse(parts[0], NumberStyles.Any, CultureInfo.InvariantCulture, out float v))
                return new Thickness(v);
        }
        else if (parts.Length == 4)
        {
            if (float.TryParse(parts[0], NumberStyles.Any, CultureInfo.InvariantCulture, out float l) &&
                float.TryParse(parts[1], NumberStyles.Any, CultureInfo.InvariantCulture, out float t) &&
                float.TryParse(parts[2], NumberStyles.Any, CultureInfo.InvariantCulture, out float r) &&
                float.TryParse(parts[3], NumberStyles.Any, CultureInfo.InvariantCulture, out float b))
            {
                return new Thickness(l, t, r, b);
            }
        }
        return Thickness.Zero;
    }

    #region Background Brush Parsing

    /// <summary>
    /// Parses the <c>Background</c> and optional <c>Opacity</c> attributes from an XML element,
    /// returning a fully configured <see cref="IBrush"/> or <c>null</c>.
    /// </summary>
    private static IBrush? ParseBackgroundAttribute(XElement element)
    {
        var bgAttr = element.Attribute("Background")?.Value;
        if (string.IsNullOrEmpty(bgAttr)) return null;

        Color color = ParseColorString(bgAttr);
        float opacity = 1.0f;

        if (element.Attribute("Opacity") != null &&
            float.TryParse(element.Attribute("Opacity")!.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out float op))
        {
            opacity = op;
        }

        var brush = color.AsBrush();
        brush.Opacity = opacity;
        return brush;
    }

    private static Color ParseColorString(string value)
    {
        // 1. Try hex ARGB: #AARRGGBB or #RRGGBB
        if (value.StartsWith("#", StringComparison.OrdinalIgnoreCase))
        {
            var hex = value.Substring(1);
            if (hex.Length == 6) return ParseHexRGB(hex);   // RGB → opaque
            if (hex.Length == 8) return ParseHexARGB(hex);  // ARGB with alpha
        }

        // 2. Try named colors
        var named = value.Trim();
        return named.ToUpperInvariant() switch
        {
            "BLACK"   => Color.Black,
            "WHITE"   => Color.White,
            "RED"     => new Color(255, 0, 0),
            "GREEN"   => new Color(0, 128, 0),
            "BLUE"    => new Color(0, 0, 255),
            "YELLOW"  => new Color(255, 255, 0),
            "GRAY"    => new Color(128, 128, 128),
            _         => throw new FormatException($"Unknown color: '{value}'"),
        };
    }

    private static Color ParseHexRGB(string hex)
    {
        byte r = Convert.ToByte(hex.Substring(0, 2), 16);
        byte g = Convert.ToByte(hex.Substring(2, 2), 16);
        byte b = Convert.ToByte(hex.Substring(4, 2), 16);
        return new Color((byte)r, (byte)g, (byte)b, (byte)255); // fully opaque
    }

    private static Color ParseHexARGB(string hex)
    {
        byte a = Convert.ToByte(hex.Substring(0, 2), 16);
        byte r = Convert.ToByte(hex.Substring(2, 2), 16);
        byte g = Convert.ToByte(hex.Substring(4, 2), 16);
        byte b = Convert.ToByte(hex.Substring(6, 2), 16);
        return new Color((byte)r, (byte)g, (byte)b, (byte)a);
    }

    #endregion
}
