using System;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Components;
using CoreEssentials.Inputs;
using Microsoft.Xna.Framework.Input;

namespace CoreEssentials.Playground;

/// <summary>
/// Toggles entity debug visualization on the owning EntitySystem when a configured key is released.
/// The key and the individual overlay flags are declarative, so a scene can wire debug toggling
/// purely from data:
/// <code>
/// &lt;Component Type="DebugToggleComponent"&gt;
///   &lt;Properties&gt;
///     &lt;Property Name="TriggerKey" Value="F3" /&gt;
///     &lt;Property Name="ShowEntityBounds" Value="true" /&gt;
///     &lt;Property Name="ShowEntityIds" Value="true" /&gt;
///   &lt;/Properties&gt;
/// &lt;/Component&gt;
/// </code>
/// When toggled on, the configured flags are applied to the system's debug config; toggling off
/// simply disables debug mode (the flags remain set for the next toggle-on).
/// </summary>
public class DebugToggleComponent : EntityComponent
{
    /// <summary>The key that toggles debug mode. Defaults to F3.</summary>
    public Keys TriggerKey { get; set; } = Keys.F3;

    /// <summary>Draw entity bounds when debug mode is enabled.</summary>
    public bool ShowEntityBounds { get; set; }

    /// <summary>Draw entity IDs when debug mode is enabled.</summary>
    public bool ShowEntityIds { get; set; }

    /// <summary>Draw entity tags when debug mode is enabled.</summary>
    public bool ShowEntityTags { get; set; }

    /// <summary>Draw the entity hierarchy when debug mode is enabled.</summary>
    public bool ShowEntityHierarchy { get; set; }

    /// <summary>Draw entity positions when debug mode is enabled.</summary>
    public bool ShowEntityPosition { get; set; }

    /// <summary>
    /// When true, debug mode is enabled (and the configured flags applied) as soon as the component
    /// attaches — so a scene can start with the overlay already visible instead of waiting for the
    /// first toggle. Defaults to false (toggle-only), preserving prior behavior.
    /// </summary>
    public bool StartEnabled { get; set; }

    /// <summary>
    /// Optional asset-name string of the font used to render debug text (IDs, tags, positions). When
    /// non-empty it is loaded on attach and assigned to the system's debug font. Empty by default.
    /// </summary>
    public string DebugFontAsset { get; set; } = string.Empty;

    private EventHandler<KeyboardEventArgs>? _onKeyReleased;

    /// <inheritdoc />
    public override void OnAttach()
    {
        _onKeyReleased = (_, args) => HandleKey(args.Key);
        Input.Keyboard.KeyReleased += _onKeyReleased;

        var system = EntitySystem;
        if (system == null) return;

        if (!string.IsNullOrEmpty(DebugFontAsset))
            system.DebugFont = LoadDebugFont(DebugFontAsset);

        if (StartEnabled)
        {
            system.DebugMode = true;
            ApplyDebugConfig(system);
        }
    }

    /// <inheritdoc />
    public override void OnDetach()
    {
        if (_onKeyReleased != null)
            Input.Keyboard.KeyReleased -= _onKeyReleased;
        _onKeyReleased = null;
    }

    /// <summary>
    /// Handles a key release: toggles debug mode on the owning EntitySystem when the key matches
    /// <see cref="TriggerKey"/>. Exposed publicly so it can be invoked directly (e.g. from tests).
    /// </summary>
    public void HandleKey(Keys key)
    {
        if (key != TriggerKey) return;

        var system = EntitySystem;
        if (system == null) return;

        system.DebugMode = !system.DebugMode;
        if (system.DebugMode)
            ApplyDebugConfig(system);
    }

    /// <summary>
    /// Loads the debug font asset by name. Virtual so unit tests can observe the requested asset
    /// without driving real content loading.
    /// </summary>
    protected virtual Assets.FontAsset LoadDebugFont(string assetName)
        => Assets.AssetManager.LoadAsset<Assets.FontAsset>(assetName);

    /// <summary>
    /// Applies the configured overlay flags to the system's debug config. Virtual so unit tests
    /// can observe the applied configuration without a live EntitySystem.
    /// </summary>
    protected virtual void ApplyDebugConfig(GameSystems.EntitySystems.EntityOOPSystem.EntitySystem system)
    {
        system.DebugConfig.ShowEntityBounds = ShowEntityBounds;
        system.DebugConfig.ShowEntityIds = ShowEntityIds;
        system.DebugConfig.ShowEntityTags = ShowEntityTags;
        system.DebugConfig.ShowEntityHierarchy = ShowEntityHierarchy;
        system.DebugConfig.ShowEntityPosition = ShowEntityPosition;
    }
}
