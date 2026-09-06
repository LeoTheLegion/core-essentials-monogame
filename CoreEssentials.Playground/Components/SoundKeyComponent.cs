using System;
using CoreEssentials.Audio;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Components;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Components.BuiltIn;
using CoreEssentials.Inputs;
using Microsoft.Xna.Framework.Input;

namespace CoreEssentials.Playground.Components;

/// <summary>
/// Plays a one-shot sound effect when a configured key is released. Both the key and the asset
/// name are declarative, so a scene can wire sound effects purely from data:
/// <code>
/// &lt;Component Type="SoundKeyComponent"&gt;
///   &lt;Properties&gt;
///     &lt;Property Name="TriggerKey" Value="Q" /&gt;
///     &lt;Property Name="SoundAsset" Value="Audio/footstep1_sound.xml" /&gt;
///   &lt;/Properties&gt;
/// &lt;/Component&gt;
/// </code>
/// </summary>
public class SoundKeyComponent : EntityComponent
{
    /// <summary>The key that triggers the sound. Defaults to Q.</summary>
    public Keys TriggerKey { get; set; } = Keys.Q;

    /// <summary>The asset-name string of the one-shot sound to play (e.g., "Audio/footstep1_sound.xml").</summary>
    public string SoundAsset { get; set; } = string.Empty;

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
    /// Handles a key release: plays <see cref="SoundAsset"/> when the key matches
    /// <see cref="TriggerKey"/>. Exposed publicly so it can be invoked directly (e.g. from tests).
    /// </summary>
    public void HandleKey(Keys key)
    {
        if (key != TriggerKey) return;
        PlaySound(SoundAsset);
    }

    private AudioSourceComponent? _source;

    /// <summary>
    /// Plays the configured one-shot by routing it through a built-in
    /// <see cref="AudioSourceComponent"/> hosted on the owning entity (created lazily, never
    /// auto-playing). Virtual so unit tests can observe the requested asset name without driving
    /// real audio playback.
    /// </summary>
    protected virtual void PlaySound(string soundAsset)
    {
        if (Owner == null || string.IsNullOrEmpty(soundAsset)) return;
        var source = _source ??= Owner.AddComponent(new AudioSourceComponent { PlayOnAttach = false });
        source.PlayOneShot(soundAsset);
    }
}
