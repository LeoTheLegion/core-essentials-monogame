using System;
using Microsoft.Xna.Framework.Audio;

namespace CoreEssentials.Audio;

/// <summary>
/// Interface for sound effect functionality
/// </summary>
public interface ISoundEffect
{
    /// <summary>
    /// Creates a new ISoundEffectInstance for this sound effect
    /// </summary>
    /// <returns>A new ISoundEffectInstance that can be played, paused, resumed and have its volume adjusted</returns>
    ISoundEffectInstance CreateInstance();
    
    /// <summary>
    /// Gets the duration of the sound effect
    /// </summary>
    TimeSpan Duration { get; }
    
    /// <summary>
    /// Gets or sets the master volume for all SoundEffectInstance objects created from SoundEffect objects
    /// </summary>
    float MasterVolume { get; set; }
}