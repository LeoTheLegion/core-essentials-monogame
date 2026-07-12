using System;
using Microsoft.Xna.Framework.Audio;

namespace CoreEssentials.Audio;

/// <summary>
/// Adapter class that wraps the MonoGame SoundEffectInstance class and implements ISoundEffectInstance
/// </summary>
public class SoundEffectInstanceAdapter : ISoundEffectInstance
{
    private readonly SoundEffectInstance _soundEffectInstance;

    /// <summary>
    /// Initializes a new instance of the <see cref="SoundEffectInstanceAdapter"/> class.
    /// </summary>
    /// <param name="soundEffectInstance">The MonoGame <see cref="SoundEffectInstance"/> to wrap.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="soundEffectInstance"/> is <see langword="null"/>.</exception>
    public SoundEffectInstanceAdapter(SoundEffectInstance soundEffectInstance)
    {
        _soundEffectInstance = soundEffectInstance ?? throw new ArgumentNullException(nameof(soundEffectInstance));
    }

    /// <summary>
    /// Plays the sound effect.
    /// </summary>
    public void Play()
    {
        _soundEffectInstance.Play();
    }

    /// <summary>
    /// Stops the sound effect.
    /// </summary>
    public void Stop()
    {
        _soundEffectInstance.Stop();
    }

    /// <summary>
    /// Pauses the sound effect.
    /// </summary>
    public void Pause()
    {
        _soundEffectInstance.Pause();
    }

    /// <summary>
    /// Gets the current state of the sound effect instance.
    /// </summary>
    /// <value>The current playback state.</value>
    public SoundState State => _soundEffectInstance.State;

    /// <summary>
    /// Gets or sets the volume of the sound effect instance.
    /// </summary>
    /// <value>The volume level. Typically ranges from 0.0 (silent) to 1.0 (full volume).</value>
    public float Volume
    {
        get => _soundEffectInstance.Volume;
        set => _soundEffectInstance.Volume = value;
    }

    /// <summary>
    /// Gets or sets a value indicating whether the sound effect should loop.
    /// </summary>
    /// <value><see langword="true"/> if the sound effect should loop; otherwise, <see langword="false"/>.</value>
    public bool IsLooped
    {
        get => _soundEffectInstance.IsLooped;
        set => _soundEffectInstance.IsLooped = value;
    }

    /// <summary>
    /// Gets or sets the pitch of the sound effect instance.
    /// </summary>
    /// <value>The pitch adjustment. Typically ranges from -1.0 to 1.0.</value>
    public float Pitch
    {
        get => _soundEffectInstance.Pitch;
        set => _soundEffectInstance.Pitch = value;
    }

    /// <summary>
    /// Gets or sets the pan of the sound effect instance.
    /// </summary>
    /// <value>The pan position. Typically ranges from -1.0 (left) to 1.0 (right).</value>
    public float Pan
    {
        get => _soundEffectInstance.Pan;
        set => _soundEffectInstance.Pan = value;
    }

    /// <summary>
    /// Releases the resources used by the underlying sound effect instance.
    /// </summary>
    public void Dispose()
    {
        _soundEffectInstance.Dispose();
    }
}