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
    private readonly Dictionary<AudioChannel, float> _channelVolumes = new Dictionary<AudioChannel, float>();

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
        foreach (var channel in Enum.GetValues<AudioChannel>())
            _channelVolumes[channel] = 1f;
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
                    _audioClipInstances[key].Play(GetExternalScale(_audioClipInstances[key].Channel));
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
    /// <param name="channel">The channel the instance plays on. Defaults to <see cref="AudioChannel.Master"/>.</param>
    /// <returns>A unique identifier that can be used to stop the sound instance.</returns>
    public string PlayOneShotSound(string soundName, AudioChannel channel = AudioChannel.Master)
    {
        var audioClip = (AudioClip)AssetManager.LoadAsset<AudioClip>(soundName);
        return PlaySound(audioClip, channel);
    }

    /// <summary>
    /// Plays the specified audio clip and tracks the resulting instance.
    /// </summary>
    /// <param name="audioClip">The audio clip to play.</param>
    /// <param name="channel">The channel the instance plays on. Defaults to <see cref="AudioChannel.Master"/>.</param>
    /// <returns>A unique identifier that can be used to stop the sound instance.</returns>
    public string PlaySound(AudioClip audioClip, AudioChannel channel = AudioChannel.Master)
    {
        var instance = CreateAudioClipInstance(audioClip);
        instance.Channel = channel;

        var id = Guid.NewGuid().ToString();
        instance.Play(GetExternalScale(channel));

        _audioClipInstances[id] = instance;

        return id;
    }

    /// <summary>
    /// Loads an <see cref="AudioClip"/> by name and plays it.
    /// </summary>
    /// <param name="name">The asset name of the sound to play.</param>
    /// <param name="channel">The channel the instance plays on. Defaults to <see cref="AudioChannel.Master"/>.</param>
    /// <returns>A unique identifier that can be used to stop the sound instance.</returns>
    public string PlaySound(string name, AudioChannel channel = AudioChannel.Master)
    {
        return PlayOneShotSound(name, channel);
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
    /// Pauses all active audio instances without releasing them.
    /// Paused instances can be resumed with <see cref="ResumeAll"/>.
    /// </summary>
    public void PauseAll()
    {
        foreach (var instance in _audioClipInstances.Values)
        {
            instance.Pause();
        }
    }

    /// <summary>
    /// Resumes all paused audio instances.
    /// </summary>
    public void ResumeAll()
    {
        foreach (var instance in _audioClipInstances.Values)
        {
            instance.Play(GetExternalScale(instance.Channel));
        }
    }

    /// <summary>
    /// Pauses the active audio instance associated with the specified identifier.
    /// </summary>
    /// <param name="soundName">The unique identifier returned by a <c>Play</c> method.</param>
    public void PauseSound(string soundName)
    {
        if (_audioClipInstances.TryGetValue(soundName, out var instance))
        {
            instance.Pause();
        }
    }

    /// <summary>
    /// Resumes the paused audio instance associated with the specified identifier.
    /// </summary>
    /// <param name="soundName">The unique identifier returned by a <c>Play</c> method.</param>
    public void ResumeSound(string soundName)
    {
        if (_audioClipInstances.TryGetValue(soundName, out var instance))
        {
            instance.Play(_masterVolume);
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
            instance.UpdateVolume(GetExternalScale(instance.Channel));
        }
    }

    /// <summary>
    /// Gets the volume of the specified channel.
    /// </summary>
    /// <param name="channel">The channel to read.</param>
    /// <returns>The channel volume (0.0–1.0).</returns>
    public float GetChannelVolume(AudioChannel channel)
        => _channelVolumes[channel];

    /// <summary>
    /// Sets the volume of a single channel, re-applying it to every active instance on that channel.
    /// </summary>
    /// <param name="channel">The channel to update.</param>
    /// <param name="volume">The channel volume, clamped between 0.0 and 1.0.</param>
    public void SetChannelVolume(AudioChannel channel, float volume)
    {
        _channelVolumes[channel] = MathHelper.Clamp(volume, 0f, 1f);

        foreach (var instance in _audioClipInstances.Values)
        {
            if (instance.Channel == channel)
                instance.UpdateVolume(GetExternalScale(channel));
        }
    }

    /// <summary>
    /// Sets the per-instance volume multiplier for a single active instance, independent of the clip,
    /// channel and master volumes.
    /// </summary>
    /// <param name="soundName">The identifier returned by a play method.</param>
    /// <param name="volume">The per-instance volume, clamped between 0.0 and 1.0.</param>
    public void SetInstanceVolume(string soundName, float volume)
    {
        if (_audioClipInstances.TryGetValue(soundName, out var instance))
            instance.SetInstanceVolume(volume);
    }

    /// <summary>
    /// Sets the per-instance pitch for a single active instance. Expressed as a Unity-style ratio
    /// (1.0 = normal pitch, 2.0 = one octave up) and converted internally to MonoGame's semitone unit.
    /// </summary>
    /// <param name="soundName">The identifier returned by a play method.</param>
    /// <param name="pitchRatio">The pitch ratio (1.0 = normal).</param>
    public void SetInstancePitch(string soundName, float pitchRatio)
    {
        if (_audioClipInstances.TryGetValue(soundName, out var instance))
            instance.SetPitch(RatioToSemitones(pitchRatio));
    }

    /// <summary>
    /// Sets the per-instance stereo pan for a single active instance (-1 = left, 0 = center, +1 = right).
    /// </summary>
    /// <param name="soundName">The identifier returned by a play method.</param>
    /// <param name="pan">The pan position, clamped between -1.0 and 1.0.</param>
    public void SetInstancePan(string soundName, float pan)
    {
        if (_audioClipInstances.TryGetValue(soundName, out var instance))
            instance.SetPan(pan);
    }

    /// <summary>
    /// The combined external volume scale for a channel: its per-channel volume multiplied by the
    /// global master volume. This is what each instance multiplies into its clip and per-instance
    /// volumes.
    /// </summary>
    private float GetExternalScale(AudioChannel channel)
        => _channelVolumes[channel] * _masterVolume;

    /// <summary>
    /// Converts a Unity-style pitch ratio (1.0 = normal) to MonoGame's semitone unit
    /// (0 = normal, +12 = one octave up). Non-positive ratios are treated as normal pitch.
    /// </summary>
    private static float RatioToSemitones(float ratio)
        => ratio <= 0f ? 0f : (float)(12d * Math.Log(ratio, 2d));
}
