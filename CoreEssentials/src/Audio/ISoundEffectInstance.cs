using System;
using Microsoft.Xna.Framework.Audio;

namespace CoreEssentials.Audio;

/// <summary>
/// Interface for sound effect instance functionality
/// </summary>
public interface ISoundEffectInstance : IDisposable
{
    /// <summary>
    /// Plays or resumes playing the sound effect instance
    /// </summary>
    void Play();

    /// <summary>
    /// Stops playing the sound effect instance
    /// </summary>
    void Stop();

    /// <summary>
    /// Pauses playback of the sound effect instance
    /// </summary>
    void Pause();

    /// <summary>
    /// Gets the current playback state of the sound effect instance
    /// </summary>
    SoundState State { get; }

    /// <summary>
    /// Gets or sets the volume of the sound effect instance
    /// </summary>
    float Volume { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the sound effect instance should loop when it reaches the end
    /// </summary>
    bool IsLooped { get; set; }

    /// <summary>
    /// Gets or sets the pitch adjustment of the sound effect instance
    /// </summary>
    float Pitch { get; set; }

    /// <summary>
    /// Gets or sets the pan position of the sound effect instance
    /// </summary>
    float Pan { get; set; }
}