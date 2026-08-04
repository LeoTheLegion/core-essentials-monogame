using System;
using System.Collections.Generic;
using CoreEssentials.Assets;
using Microsoft.Xna.Framework;

namespace CoreEssentials.Audio;

/// <summary>
/// Manages playback, lifetime tracking, and volume control for audio clips.
/// Uses a singleton pattern to provide a single audio system instance per game.
/// </summary>
public class AudioManager
{
    private static AudioManager? _instance;

    private Dictionary<string, AudioClipInstance> _audioClipInstances = new Dictionary<string, AudioClipInstance>();
    private float _masterVolume;

    /// <summary>
    /// Gets the singleton instance of the <see cref="AudioManager"/>.
    /// </summary>
    public static AudioManager Instance => _instance ??= new AudioManager();

    /// <summary>
    /// Initializes a new instance of the <see cref="AudioManager"/> class.
    /// </summary>
    /// <remarks>
    /// Changed from private to protected to allow for extension and unit testing.
    /// </remarks>
    protected AudioManager()
    {
        // Initialize audio system here
        _masterVolume = 1; // Default volume
    }

    /// <summary>
    /// Updates active audio instances, removing finished sounds and restarting looping sounds.
    /// </summary>
    /// <param name="gameTime">The current game timing information.</param>
    public void Update(GameTime gameTime)
    {
        // Update audio system if needed
        // Loop through audio clip instances and remove any that are done playing
        foreach (var key in new List<string>(_audioClipInstances.Keys))
        {
            if (_audioClipInstances[key].IsDonePlaying())
            {
                if(_audioClipInstances[key].AudioClip.Loop)
                {
                    _audioClipInstances[key].Play(_masterVolume);
                }
                else
                {
                    _audioClipInstances[key].Stop();
                    _audioClipInstances.Remove(key);
                }
            }
        }
    }

    /// <summary>
    /// Loads an <see cref="AudioClip"/> by name and plays it once.
    /// </summary>
    /// <param name="soundName">The asset name of the sound to play.</param>
    /// <returns>A unique identifier that can be used to stop the sound instance.</returns>
    public string PlayOneShotSound(string soundName)
    {
        // Play sound logic here
        var audioClip = (AudioClip)AssetManager.LoadAsset<AudioClip>(soundName);
        return PlaySound(audioClip);
    }

    /// <summary>
    /// Plays the specified audio clip and tracks the resulting instance.
    /// </summary>
    /// <param name="audioClip">The audio clip to play.</param>
    /// <returns>A unique identifier that can be used to stop the sound instance.</returns>
    public string PlaySound(AudioClip audioClip)
    {
        var instance = CreateAudioClipInstance(audioClip);

        var id = Guid.NewGuid().ToString();
        instance.Play(_masterVolume);

        Console.WriteLine($"Playing sound with ID: {id}");

        _audioClipInstances[id] = instance;

        return id;
    }

    /// <summary>
    /// Loads an <see cref="AudioClip"/> by name and plays it.
    /// </summary>
    /// <param name="name">The asset name of the sound to play.</param>
    /// <returns>A unique identifier that can be used to stop the sound instance.</returns>
    public string PlaySound(string name)
    {
        return PlayOneShotSound(name);
    }

    /// <summary>
    /// Creates a new <see cref="AudioClipInstance"/> for the specified audio clip.
    /// </summary>
    /// <param name="audioClip">The audio clip to wrap.</param>
    /// <returns>A new <see cref="AudioClipInstance"/>.</returns>
    /// <remarks>
    /// Changed from private to protected to allow derived classes to override instance creation.
    /// </remarks>
    protected virtual AudioClipInstance CreateAudioClipInstance(AudioClip audioClip)
    {
        return new AudioClipInstance(audioClip);
    }

    /// <summary>
    /// Stops the active audio instance associated with the specified identifier.
    /// </summary>
    /// <param name="soundName">The unique identifier returned by a <c>Play</c> method.</param>
    public void StopSound(string soundName)
    {
        // Stop sound logic here
        if (_audioClipInstances.ContainsKey(soundName))
        {
            _audioClipInstances[soundName].Stop();
            _audioClipInstances.Remove(soundName);
        }
    }

    /// <summary>
    /// Sets the master volume applied to all active audio instances.
    /// </summary>
    /// <param name="volume">The master volume, clamped between 0.0 and 1.0.</param>
    public void SetMasterVolume(float volume)
    {
        // Set volume logic here
        _masterVolume = MathHelper.Clamp(volume, 0f, 1f);

        foreach (var instance in _audioClipInstances.Values)
        {
            instance.UpdateVolume(_masterVolume);
        }
    }
}
