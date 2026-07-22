using Microsoft.Xna.Framework;

using CoreEssentials.GUI.Types;

namespace CoreEssentials.GUI.Internal;

/// <summary>
/// Resolves the active GUI engine backend at runtime. Enables swapping between engines (e.g., Myra, custom) without user code changes.
/// </summary>
/// <remarks>
/// Usage:
/// <code>
/// // Default — uses the built-in Myra-based engine automatically on first access
/// var manager = EngineResolver.GetEngine();
/// manager.Init(this, 800, 600);
///
/// // Swap to a custom engine at startup (zero user code changes needed)
/// EngineResolver.SetEngine(new CustomGuiEngine());
/// </code>
/// </remarks>
public static class EngineResolver
{
    private static IGuiManager? _engine;

    /// <summary>
    /// Gets the currently active GUI engine. Throws if no engine has been set or resolved yet.
    /// </summary>
    public static IGuiManager GetEngine() => _engine ??= ResolveDefault();

    /// <summary>
    /// Sets the active GUI engine backend. Call this at startup to swap in a custom implementation.
    /// If called multiple times, the last call wins — subsequent <see cref="GetEngine"/> calls return the new instance.
    /// </summary>
    /// <param name="engine">The IGuiManager implementation to use.</param>
    public static void SetEngine(IGuiManager engine) => _engine = engine ?? throw new System.ArgumentNullException(nameof(engine));

    private static IGuiManager ResolveDefault()
    {
        var manager = new Engines.Myra.GuiManagerImpl();
        return manager;
    }
}
