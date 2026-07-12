using System;
using Microsoft.Xna.Framework.Audio;

namespace CoreEssentials.Audio;

/// <summary>
/// Adapter class that wraps the MonoGame SoundEffect class and implements ISoundEffect
/// </summary>
public class SoundEffectAdapter : ISoundEffect
{
    private readonly SoundEffect _soundEffect;

    /// <summary>
    /// Initializes a new instance of the <see cref="SoundEffectAdapter"/> class.
    /// </summary>
    /// <param name="soundEffect">The MonoGame <see cref="SoundEffect"/> to wrap.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="soundEffect"/> is <see langword="null"/>.</exception>
    public SoundEffectAdapter(SoundEffect soundEffect)
    {
        _soundEffect = soundEffect ?? throw new ArgumentNullException(nameof(soundEffect));
    }

    /// <summary>
    /// Creates a new <see cref="ISoundEffectInstance"/> for this sound effect.
    /// </summary>
    /// <returns>A new <see cref="ISoundEffectInstance"/> that wraps the underlying MonoGame instance.</returns>
    public ISoundEffectInstance CreateInstance()
    {
        return new SoundEffectInstanceAdapter(_soundEffect.CreateInstance());
    }

    /// <summary>
    /// Gets the duration of the sound effect.
    /// </summary>
    public TimeSpan Duration => _soundEffect.Duration;

    /// <summary>
    /// Gets or sets the master volume for all <see cref="SoundEffectInstance"/> objects created from <see cref="SoundEffect"/> objects.
    /// </summary>
    public float MasterVolume
    {
        get => SoundEffect.MasterVolume;
        set => SoundEffect.MasterVolume = value;
    }
}