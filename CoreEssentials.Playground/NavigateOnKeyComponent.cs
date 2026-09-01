using System;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Components;
using CoreEssentials.Inputs;
using Microsoft.Xna.Framework.Input;

namespace CoreEssentials.Playground;

/// <summary>
/// Loads a data-driven scene when a configured key is released. The navigation target is the
/// asset-name string of a scene XML (see <c>SceneManager.LoadScene(string)</c>), so scenes can
/// be driven entirely from data: bind a key here and the component transitions to that scene.
///
/// Both keys are declarative — set them from scene XML via &lt;Properties&gt;:
/// <code>
/// &lt;Component Type="NavigateOnKeyComponent"&gt;
///   &lt;Properties&gt;
///     &lt;Property Name="TriggerKey" Value="OemPlus" /&gt;
///     &lt;Property Name="TargetSceneAsset" Value="PhysicsEntityScene.xml" /&gt;
///   &lt;/Properties&gt;
/// &lt;/Component&gt;
/// </code>
/// </summary>
public class NavigateOnKeyComponent : EntityComponent
{
    /// <summary>The key that triggers navigation. Defaults to OemPlus (+).</summary>
    public Keys TriggerKey { get; set; } = Keys.OemPlus;

    /// <summary>The asset-name string of the scene XML to load (e.g., "PhysicsEntityScene.xml").</summary>
    public string TargetSceneAsset { get; set; } = string.Empty;

    private EventHandler<KeyboardEventArgs>? _onKeyReleased;

    /// <inheritdoc />
    public override void OnAttach()
    {
        _onKeyReleased = (_, args) => HandleKey(args.Key);
        Input.Keyboard.KeyReleased += _onKeyReleased;
    }

    /// <inheritdoc />
    public override void OnDetach()
    {
        if (_onKeyReleased != null)
            Input.Keyboard.KeyReleased -= _onKeyReleased;
        _onKeyReleased = null;
    }

    /// <summary>
    /// Handles a key release: navigates to <see cref="TargetSceneAsset"/> when the key matches
    /// <see cref="TriggerKey"/>. Exposed publicly so it can be invoked directly (e.g. from tests).
    /// </summary>
    public void HandleKey(Keys key)
    {
        if (key != TriggerKey) return;
        LoadScene(TargetSceneAsset);
    }

    /// <summary>
    /// Performs the scene transition. Virtual so unit tests can observe the requested asset name
    /// without driving a full SceneManager transition.
    /// </summary>
    protected virtual void LoadScene(string sceneAssetName)
        => Game?.SceneManager.LoadScene(sceneAssetName);
}
