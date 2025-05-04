using System;
using System.Collections.Generic;
using CoreEssentials.Assets;
using Microsoft.Xna.Framework;
using CoreEssentials.Debugging;

namespace CoreEssentials.Audio;

public class AudioManager
{
    private static AudioManager _instance;

    private Dictionary<string, AudioClipInstance> _audioClipInstances = new Dictionary<string, AudioClipInstance>();
    private float _masterVolume;

    public static AudioManager Instance => _instance ??= new AudioManager();
    
    // Changed from private to protected to allow for extension and testing
    protected AudioManager()
    {
        // Initialize audio system here
        _masterVolume = 1; // Default volume
    }

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

    public string PlayOneShotSound(string soundName)
    {
        // Play sound logic here
        var audioClip = AssetManager.LoadAsset<AudioClip>(soundName);
        return PlaySound(audioClip);
    }

    public string PlaySound(IAudioClip audioClip)
    {
        var instance = CreateAudioClipInstance(audioClip);

        var id = Guid.NewGuid().ToString();
        instance.Play(_masterVolume);

        Debug.Console.WriteLine($"Playing sound with ID: {id}");

        _audioClipInstances[id] = instance;

        return id;
    }

    public string PlaySound(string name)
    {
        return PlayOneShotSound(name);
    }

    // Changed from private to protected to allow derived classes to override
    protected virtual AudioClipInstance CreateAudioClipInstance(IAudioClip audioClip)
    {
        return new AudioClipInstance(audioClip);
    }

    public void StopSound(string soundName)
    {
        // Stop sound logic here
        if (_audioClipInstances.ContainsKey(soundName))
        {
            _audioClipInstances[soundName].Stop();
            _audioClipInstances.Remove(soundName);
        }
    }

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
