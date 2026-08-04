namespace CoreEssentials.GUI.Types;

/// <summary>
/// Panel-specific styling interface extending container functionality.
/// </summary>
public interface IPanel : IContainer
{
    /// <summary>
    /// Gets or sets the background brush for this panel.
    /// </summary>
    IBrush? Background { get; set; }

    /// <summary>
    /// Gets or sets the thickness of the border around this panel.
    /// </summary>
    Thickness BorderThickness { get; set; }
}
