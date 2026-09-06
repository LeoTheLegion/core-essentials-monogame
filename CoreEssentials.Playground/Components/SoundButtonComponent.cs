using System;
using CoreEssentials.Audio;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Components;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Components.BuiltIn;

namespace CoreEssentials.Playground.Components;

/// <summary>
/// Makes a pre-declared <see cref="ButtonComponent"/> on the same entity play a one-shot sound
/// when clicked. This is the thin, data-driven replacement for the old hand-written sound button
/// entity: the canvas and the button widget are owned by the built-in <c>CanvasComponent</c> /
/// <see cref="ButtonComponent"/> siblings (declared before this component in the XML so they attach
/// first); this component only subscribes to the button's <c>Clicked</c> event:
/// <code>
/// &lt;Component Type="SoundButtonComponent"&gt;
///   &lt;Properties&gt;&lt;Property Name="SoundAsset" Value="Audio/footstep1_sound.xml" /&gt;&lt;/Properties&gt;
/// &lt;/Component&gt;
/// </code>
/// The component does nothing (and logs a warning) if no <see cref="ButtonComponent"/> is present.
/// </summary>
public class SoundButtonComponent : EntityComponent
{
    /// <summary>The asset-name string of the one-shot sound to play when the button is clicked.</summary>
    public string SoundAsset { get; set; } = string.Empty;

    private ButtonComponent? _button;
    private Action? _onClicked;

    /// <inheritdoc />
    public override void OnAttach()
    {
        _button = Owner?.GetComponent<ButtonComponent>();
        if (_button == null)
        {
            Console.WriteLine("[SoundButtonComponent] No ButtonComponent found on the owning entity — sound button disabled.");
            return;
        }

        _onClicked = () => PlaySound(SoundAsset);
        _button.Clicked += _onClicked;
    }

    /// <inheritdoc />
    public override void OnDetach()
    {
        if (_button != null && _onClicked != null)
            _button.Clicked -= _onClicked;

        _button = null;
        _onClicked = null;
    }

    /// <summary>
    /// Plays the configured one-shot sound. Virtual so unit tests can observe the requested asset
    /// name without driving real audio playback.
    /// </summary>
    protected virtual void PlaySound(string soundAsset)
    {
        if (string.IsNullOrEmpty(soundAsset)) return;
        var id = AudioManager.Instance.PlayOneShotSound(soundAsset);
        Console.WriteLine($"[SoundButtonComponent] Sound played with ID: {id}");
    }
}
