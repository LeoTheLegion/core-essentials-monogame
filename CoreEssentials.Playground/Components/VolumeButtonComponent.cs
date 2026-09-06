using System;
using CoreEssentials.Audio;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Components;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Components.BuiltIn;
// (BuiltIn already provides both ButtonComponent and AudioListenerComponent.)

namespace CoreEssentials.Playground.Components;

/// <summary>
/// Makes a pre-declared <see cref="ButtonComponent"/> on the same entity set the master audio
/// volume when clicked. This is the thin, data-driven replacement for the old hand-written volume
/// button entity: the canvas and the button widget are owned by the built-in <c>CanvasComponent</c> /
/// <see cref="ButtonComponent"/> siblings (declared before this component in the XML so they attach
/// first); this component only subscribes to the button's <c>Clicked</c> event:
/// <code>
/// &lt;Component Type="VolumeButtonComponent"&gt;
///   &lt;Properties&gt;&lt;Property Name="VolumeLevel" Value="0.5" /&gt;&lt;/Properties&gt;
/// &lt;/Component&gt;
/// </code>
/// The component does nothing (and logs a warning) if no <see cref="ButtonComponent"/> is present.
/// </summary>
public class VolumeButtonComponent : EntityComponent
{
    /// <summary>The master volume (0.0–1.0) the button sets when clicked.</summary>
    public float VolumeLevel { get; set; }

    private ButtonComponent? _button;
    private Action? _onClicked;

    /// <inheritdoc />
    public override void OnAttach()
    {
        _button = Owner?.GetComponent<ButtonComponent>();
        if (_button == null)
        {
            Console.WriteLine("[VolumeButtonComponent] No ButtonComponent found on the owning entity — volume button disabled.");
            return;
        }

        _onClicked = () => SetVolume(VolumeLevel);
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
    /// Sets the master audio volume, routing through the scene's active built-in
    /// <see cref="AudioListenerComponent"/> when one is present (falling back to the raw
    /// <see cref="AudioManager"/> otherwise). Virtual so unit tests can observe the requested level
    /// without driving real audio.
    /// </summary>
    protected virtual void SetVolume(float volumeLevel)
    {
        var listener = AudioListenerComponent.ActiveListener;
        if (listener != null)
            listener.MasterVolume = volumeLevel;
        else
            AudioManager.Instance.SetMasterVolume(volumeLevel);
    }
}
