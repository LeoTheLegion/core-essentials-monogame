using Microsoft.Xna.Framework;

namespace CoreEssentials.Debugging;

/// <summary>
/// Configuration for entity debug visualization overlays.
/// Controls which metadata is drawn when debug mode is enabled.
/// </summary>
public class DebugConfig
{
    /// <summary>
    /// Gets or sets whether to draw entity bounding boxes.
    /// </summary>
    public bool ShowEntityBounds { get; set; }

    /// <summary>
    /// Gets or sets whether to draw entity IDs as text.
    /// </summary>
    public bool ShowEntityIds { get; set; }

    /// <summary>
    /// Gets or sets whether to draw entity tags as text.
    /// </summary>
    public bool ShowEntityTags { get; set; }

    /// <summary>
    /// Gets or sets whether to draw parent-child hierarchy lines.
    /// </summary>
    public bool ShowEntityHierarchy { get; set; }

    /// <summary>
    /// Gets or sets whether to draw entity position markers.
    /// </summary>
    public bool ShowEntityPosition { get; set; }

    /// <summary>
    /// Gets or sets the color used for drawing entity bounds.
    /// </summary>
    public Color BoundsColor { get; set; } = Color.Lime;

    /// <summary>
    /// Gets or sets the color used for drawing entity IDs.
    /// </summary>
    public Color IdColor { get; set; } = Color.Yellow;

    /// <summary>
    /// Gets or sets the color used for drawing entity tags.
    /// </summary>
    public Color TagColor { get; set; } = Color.Cyan;

    /// <summary>
    /// Gets or sets the color used for drawing hierarchy lines.
    /// </summary>
    public Color HierarchyColor { get; set; } = Color.Magenta;

    /// <summary>
    /// Gets or sets the color used for drawing position markers.
    /// </summary>
    public Color PositionColor { get; set; } = Color.Red;

    /// <summary>
    /// Gets or sets the line thickness for debug overlays (default: 1).
    /// </summary>
    public float LineThickness { get; set; } = 1f;
}
