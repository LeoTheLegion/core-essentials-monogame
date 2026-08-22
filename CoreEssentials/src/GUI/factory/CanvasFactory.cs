using CoreEssentials.GUI.Types;
using CoreEssentials.GUI.Engines.Myra;

namespace CoreEssentials.GUI.Factory;

/// <summary>
/// Static factory class for creating canvas instances in screen or world space mode.
/// </summary>
public static class CanvasFactory
{
    /// <summary>
    /// Creates a screen-space canvas (positioned in absolute screen coordinates).
    /// </summary>
    public static ICanvas CreateScreenSpace() => new CanvasImpl(isScreenSpace: true);

    /// <summary>
    /// Creates a world-space canvas (positioned using game world coordinates, converted via the active camera).
    /// </summary>
    public static ICanvas CreateWorldSpace() => new CanvasImpl(isScreenSpace: false);
}
