using System.Xml.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using CoreEssentials.Assets;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Components;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Serialization;

namespace CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Components.BuiltIn;

/// <summary>
/// Component that provides sprite-based rendering for an entity.
/// In the hybrid rendering model, this component provides an additional draw path
/// alongside the existing Entity.Render() method.
/// </summary>
public class SpriteComponent : EntityComponent, ISerializableComponent
{
    /// <summary>
    /// Gets or sets the sprite to render.
    /// </summary>
    public Sprite? Sprite { get; set; }

    /// <summary>
    /// Gets or sets the origin point for rotation and positioning, as a fraction of the sprite size.
    /// (0, 0) = top-left, (0.5, 0.5) = center, (1, 1) = bottom-right.
    /// Default is (0.5, 0.5) for center-origin.
    /// </summary>
    public Vector2 Origin { get; set; } = new Vector2(0.5f, 0.5f);

    /// <summary>
    /// Gets or sets the color tint applied to the sprite. Default is white (no tint).
    /// </summary>
    public Color Color { get; set; } = Color.White;

    /// <summary>
    /// Gets or sets the sprite effects (flip). Default is None.
    /// </summary>
    public SpriteEffects Effects { get; set; } = SpriteEffects.None;

    /// <summary>
    /// Gets or sets the layer depth for 3D sorting. Default is 0.
    /// </summary>
    public float LayerDepth { get; set; } = 0f;

    /// <summary>
    /// Gets or sets an optional sort order override. When set, this value is used
    /// instead of the entity's default sort order for render ordering.
    /// </summary>
    public int? SortOrderOverride { get; set; }

    /// <summary>
    /// Gets or sets the animation frame index (only applicable when Sprite uses a SpriteSheet).
    /// </summary>
    public int AnimationFrame { get; set; } = 0;

    /// <summary>
    /// Initializes a new instance of the <see cref="SpriteComponent"/> class.
    /// </summary>
    public SpriteComponent()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SpriteComponent"/> class with a sprite.
    /// </summary>
    /// <param name="sprite">The sprite to render.</param>
    public SpriteComponent(Sprite sprite)
    {
        Sprite = sprite;
    }

    /// <summary>
    /// Draws the sprite using the entity's transform.
    /// Call this method from Entity.Render() or EntitySystem.Draw() to render this component.
    /// </summary>
    /// <param name="spriteBatch">The SpriteBatch used for drawing.</param>
    public void Draw(SpriteBatch spriteBatch)
    {
        if (Sprite == null || Owner == null)
            return;

        Sprite.Draw(
            spriteBatch,
            Owner.Position,
            Color,
            Owner.Rotation,
            Owner.Scale,
            Effects,
            LayerDepth
        );
    }

    /// <summary>
    /// Gets the effective sort order for this component.
    /// Returns the SortOrderOverride if set, otherwise falls back to the entity's sort order.
    /// </summary>
    public int GetEffectiveSortOrder()
    {
        return SortOrderOverride ?? Owner.GetSort();
    }

    /// <summary>
    /// Serializes the sprite component's state to an XML element.
    /// </summary>
    /// <returns>An XML element containing the component's serialized state.</returns>
    public XElement SerializeToXml()
    {
        return new XElement("SpriteState",
            new XAttribute("ColorR", Color.R),
            new XAttribute("ColorG", Color.G),
            new XAttribute("ColorB", Color.B),
            new XAttribute("ColorA", Color.A),
            new XAttribute("OriginX", Origin.X),
            new XAttribute("OriginY", Origin.Y),
            new XAttribute("Effects", Effects.ToString()),
            new XAttribute("LayerDepth", LayerDepth),
            new XAttribute("SortOrderOverride", SortOrderOverride.HasValue ? SortOrderOverride.Value.ToString() : "-1"),
            new XAttribute("AnimationFrame", AnimationFrame)
        );
    }

    /// <summary>
    /// Deserializes the sprite component's state from an XML element.
    /// </summary>
    /// <param name="element">The XML element containing the component's state.</param>
    public void DeserializeFromXml(XElement element)
    {
        var colorR = byte.Parse(element.Attribute("ColorR")?.Value ?? "255");
        var colorG = byte.Parse(element.Attribute("ColorG")?.Value ?? "255");
        var colorB = byte.Parse(element.Attribute("ColorB")?.Value ?? "255");
        var colorA = byte.Parse(element.Attribute("ColorA")?.Value ?? "255");
        Color = new Color(colorR, colorG, colorB, colorA);

        Origin = new Vector2(
            float.Parse(element.Attribute("OriginX")?.Value ?? "0.5"),
            float.Parse(element.Attribute("OriginY")?.Value ?? "0.5")
        );

        string effectsAttr = GetAttribute(element, "Effects");
        if (!string.IsNullOrEmpty(effectsAttr) && Enum.TryParse<SpriteEffects>(effectsAttr, out var effects))
        {
            Effects = effects;
        }

        string layerDepthAttr = GetAttribute(element, "LayerDepth");
        if (!string.IsNullOrEmpty(layerDepthAttr))
        {
            LayerDepth = float.Parse(layerDepthAttr);
        }

        string sortOrderValue = GetAttribute(element, "SortOrderOverride", "-1");
        if (int.TryParse(sortOrderValue, out int sortOrder) && sortOrder >= 0)
        {
            SortOrderOverride = sortOrder;
        }
        else
        {
            SortOrderOverride = null;
        }

        string animationFrameAttr = GetAttribute(element, "AnimationFrame");
        if (!string.IsNullOrEmpty(animationFrameAttr))
        {
            AnimationFrame = int.Parse(animationFrameAttr);
        }
    }

    /// <summary>Gets the attribute value or a default fallback.</summary>
    private static string GetAttribute(XElement element, string name, string @default = "")
    {
        return element.Attribute(name)?.Value ?? @default;
    }
}
