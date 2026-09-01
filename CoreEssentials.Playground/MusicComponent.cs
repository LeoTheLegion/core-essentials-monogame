using System;
using CoreEssentials.Audio;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Components;

namespace CoreEssentials.Playground;

/// <summary>
/// Plays a looping music track for the lifetime of its owning entity and pauses/resumes it when
/// the application loses/regains focus. The track asset name is declarative, so background music
/// can be driven purely from data:
/// <code>
/// &lt;Component Type="MusicComponent"&gt;
///   &lt;Properties&gt;
///     &lt;Property Name="MusicAsset" Value="song1_sound.xml" /&gt;
///   &lt;/Properties&gt;
/// &lt;/Component&gt;
/// </code>
/// The track starts on attach, stops when the entity is destroyed (e.g. scene unload), and is
/// paused/resumed automatically via <see cref="EntityComponent.OnApplicationPause"/> — no scene
/// subclass code required.
/// </summary>
public class MusicComponent : EntityComponent
{
    /// <summary>The asset-name string of the looping music track to play (e.g., "song1_sound.xml").</summary>
    public string MusicAsset { get; set; } = string.Empty;

    private string? _soundId;

    /// <inheritdoc />
    public override void OnAttach()
    {
        if (string.IsNullOrEmpty(MusicAsset)) return;
        _soundId = PlayMusic(MusicAsset);
    }

    /// <inheritdoc />
    public override void OnDetach()
    {
        StopCurrent();
    }

    /// <summary>
    /// Pauses or resumes the current track when the application is backgrounded/foregrounded.
    /// Forwarded automatically from the owning entity's OnApplicationPause.
    /// </summary>
    public override void OnApplicationPause(bool paused)
    {
        if (_soundId == null) return;
        if (paused)
            PauseMusic(_soundId);
        else
            ResumeMusic(_soundId);
    }

    private void StopCurrent()
    {
        if (_soundId == null) return;
        StopMusic(_soundId);
        _soundId = null;
    }

    /// <summary>
    /// Starts the looping music track. Virtual so unit tests can observe the requested asset name
    /// without driving real audio playback.
    /// </summary>
    protected virtual string PlayMusic(string musicAsset)
        => AudioManager.Instance.PlaySound(musicAsset);

    /// <summary>Pauses the given sound instance. Virtual for test observability.</summary>
    protected virtual void PauseMusic(string soundId)
        => AudioManager.Instance.PauseSound(soundId);

    /// <summary>Resumes the given sound instance. Virtual for test observability.</summary>
    protected virtual void ResumeMusic(string soundId)
        => AudioManager.Instance.ResumeSound(soundId);

    /// <summary>Stops the given sound instance. Virtual for test observability.</summary>
    protected virtual void StopMusic(string soundId)
        => AudioManager.Instance.StopSound(soundId);
}
