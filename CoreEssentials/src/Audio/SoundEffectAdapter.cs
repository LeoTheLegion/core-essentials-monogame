using System;
using Microsoft.Xna.Framework.Audio;

namespace CoreEssentials.Audio;

/// <summary>
/// Adapter class that wraps the MonoGame SoundEffect class and implements ISoundEffect
/// </summary>
public class SoundEffectAdapter : ISoundEffect
{
    private readonly SoundEffect _soundEffect;
    
    public SoundEffectAdapter(SoundEffect soundEffect)
    {
        _soundEffect = soundEffect ?? throw new ArgumentNullException(nameof(soundEffect));
    }
    
    public ISoundEffectInstance CreateInstance()
    {
        return new SoundEffectInstanceAdapter(_soundEffect.CreateInstance());
    }
    
    public TimeSpan Duration => _soundEffect.Duration;
    
    public float MasterVolume
    {
        get => SoundEffect.MasterVolume;
        set => SoundEffect.MasterVolume = value;
    }
}