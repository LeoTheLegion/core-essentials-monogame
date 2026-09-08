using System;
using CoreEssentials.Audio;
using Microsoft.Xna.Framework;

namespace CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Components.BuiltIn;

/// <summary>
/// Built-in audio listener — the audience for a scene, in the spirit of Unity's
/// <c>AudioListener</c>. Attach it to exactly one entity per scene (conventionally the camera host).
/// It owns the global master volume and provides the world position that opt-in spatial sources
/// (<see cref="AudioSourceComponent"/> with <c>SpatialEnabled</c>) pan and attenuate against.
/// </summary>
/// <remarks>
/// MonoGame has no native listener, so this class is a convention, not a platform hook: it registers
/// itself in a single static <see cref="ActiveListener"/> slot on attach and clears it on detach. If a
/// second listener attaches while one is active it warns and does not steal the slot, keeping the
/// "exactly one audience" rule explicit rather than accidental.
/// </remarks>
public class AudioListenerComponent : EntityComponent
{
    private static AudioListenerComponent? _active;

    /// <summary>
    /// The currently active listener, or null when none is attached. Spatial sources read this each
    /// frame to find their pan/attenuation reference.
    /// </summary>
    public static AudioListenerComponent? ActiveListener => _active;

    /// <summary>
    /// Gets or sets the global master volume (0.0–1.0) applied through the <see cref="AudioManager"/>.
    /// Setting it updates every active instance immediately.
    /// </summary>
    public float MasterVolume
    {
        get => _masterVolume;
        set
        {
            _masterVolume = value;
            ApplyMasterVolume();
        }
    }

    private float _masterVolume = 1f;

    /// <summary>
    /// Initializes a new instance of the <see cref="AudioListenerComponent"/> class.
    /// </summary>
    public AudioListenerComponent()
    {
    }

    /// <inheritdoc />
    public override void OnAttach()
    {
        base.OnAttach();

        if (_active != null && _active != this)
        {
            Console.WriteLine(
                "[AudioListenerComponent] A listener is already active; ignoring this one. " +
                "A scene should have exactly one AudioListenerComponent.");
            return;
        }

        _active = this;
        ApplyMasterVolume();
    }

    /// <inheritdoc />
    public override void OnDetach()
    {
        if (_active == this)
            _active = null;

        base.OnDetach();
    }

    /// <summary>
    /// The listener's current world position — the spatial reference point for pan and attenuation.
    /// Tracks the owner's position so moving the camera moves the audience with it.
    /// </summary>
    public Vector2 ListenerPosition => Owner?.Position ?? Vector2.Zero;

    private void ApplyMasterVolume()
        => AudioManager.Instance.SetMasterVolume(_masterVolume);
}
