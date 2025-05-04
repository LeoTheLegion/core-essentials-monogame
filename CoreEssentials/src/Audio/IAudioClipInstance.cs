using CoreEssentials.Assets;
using Microsoft.Xna.Framework.Audio;

namespace CoreEssentials.Audio;

/// <summary>
/// Interface for audio clip instance functionality to allow more flexible implementations
/// and better testability without direct dependencies on concrete types
/// </summary>
public interface IAudioClipInstance
{
    /// <summary>
    /// Gets the audio clip associated with this instance
    /// </summary>
    IAudioClip AudioClip { get; }
    
    /// <summary>
    /// Checks if the audio clip has finished playing
    /// </summary>
    /// <returns>True if the audio clip has finished playing, false otherwise</returns>
    bool IsDonePlaying();
    
    /// <summary>
    /// Plays the audio clip with the specified master volume
    /// </summary>
    /// <param name="masterVolume">The master volume to apply</param>
    void Play(float masterVolume);
    
    /// <summary>
    /// Stops playing the audio clip and cleans up resources
    /// </summary>
    void Stop();
    
    /// <summary>
    /// Updates the volume based on the master volume
    /// </summary>
    /// <param name="masterVolume">The master volume to apply</param>
    void UpdateVolume(float masterVolume);
}