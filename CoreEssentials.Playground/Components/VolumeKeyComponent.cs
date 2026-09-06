using System;
using CoreEssentials.Audio;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Components;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Components.BuiltIn;
using CoreEssentials.Inputs;
using Microsoft.Xna.Framework.Input;

namespace CoreEssentials.Playground.Components;

/// <summary>
/// Sets the master audio volume when a configured key is released. Both the key and the target
/// volume are declarative, so a scene can wire volume controls purely from data:
/// <code>
/// &lt;Component Type="VolumeKeyComponent"&gt;
///   &lt;Properties&gt;
///     &lt;Property Name="TriggerKey" Value="Z" /&gt;
///     &lt;Property Name="Volume" Value="0.1" /&gt;
///   &lt;/Properties&gt;
/// &lt;/Component&gt;
/// </code>
/// </summary>
public class VolumeKeyComponent : EntityComponent
{
    /// <summary>The key that triggers the volume change. Defaults to Z.</summary>
    public Keys TriggerKey { get; set; } = Keys.Z;

    /// <summary>The master volume (0.0–1.0) to set when triggered.</summary>
    public float Volume { get; set; } = 1.0f;

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
    /// Handles a key release: sets master volume to <see cref="Volume"/> when the key matches
    /// <see cref="TriggerKey"/>. Exposed publicly so it can be invoked directly (e.g. from tests).
    /// </summary>
    public void HandleKey(Keys key)
    {
        if (key != TriggerKey) return;
        SetVolume(Volume);
    }

    /// <summary>
    /// Sets the master volume, routing through the scene's active built-in
    /// <see cref="AudioListenerComponent"/> when one is present (falling back to the raw
    /// <see cref="AudioManager"/> otherwise). Virtual so unit tests can observe the requested
    /// value without driving real audio.
    /// </summary>
    protected virtual void SetVolume(float volume)
    {
        var listener = AudioListenerComponent.ActiveListener;
        if (listener != null)
            listener.MasterVolume = volume;
        else
            AudioManager.Instance.SetMasterVolume(volume);
    }
}
