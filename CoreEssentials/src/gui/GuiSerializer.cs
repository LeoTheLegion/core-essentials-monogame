using System;
using System.Xml.Linq;
using System.Globalization;
using Microsoft.Xna.Framework;
using CoreEssentials.GUI.Types;
using CoreEssentials.GUI.Factory;
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

        // Handle background color (simplified for now similar to TextColor)
        if (element.Attribute("Background") != null)
        {
            // In a real scenario, we'd map this to an IBrush. 
            // For now, we leave it as a placeholder or a simple color check if the engine supports it.
        }

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
}
