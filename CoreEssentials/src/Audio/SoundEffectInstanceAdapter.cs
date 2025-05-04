using System;
using Microsoft.Xna.Framework.Audio;

namespace CoreEssentials.Audio;

/// <summary>
/// Adapter class that wraps the MonoGame SoundEffectInstance class and implements ISoundEffectInstance
/// </summary>
public class SoundEffectInstanceAdapter : ISoundEffectInstance
{
    private readonly SoundEffectInstance _soundEffectInstance;
    
    public SoundEffectInstanceAdapter(SoundEffectInstance soundEffectInstance)
    {
        _soundEffectInstance = soundEffectInstance ?? throw new ArgumentNullException(nameof(soundEffectInstance));
    }
    
    public void Play()
    {
        _soundEffectInstance.Play();
    }
    
    public void Stop()
    {
        _soundEffectInstance.Stop();
    }
    
    public void Pause()
    {
        _soundEffectInstance.Pause();
    }
    
    public SoundState State => _soundEffectInstance.State;
    
    public float Volume
    {
        get => _soundEffectInstance.Volume;
        set => _soundEffectInstance.Volume = value;
    }
    
    public bool IsLooped
    {
        get => _soundEffectInstance.IsLooped;
        set => _soundEffectInstance.IsLooped = value;
    }
    
    public float Pitch
    {
        get => _soundEffectInstance.Pitch;
        set => _soundEffectInstance.Pitch = value;
    }
    
    public float Pan
    {
        get => _soundEffectInstance.Pan;
        set => _soundEffectInstance.Pan = value;
    }
    
    public void Dispose()
    {
        _soundEffectInstance.Dispose();
    }
}