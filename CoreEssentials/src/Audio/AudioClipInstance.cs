using System;
using CoreEssentials.Assets;
using CoreEssentials.Debugging;
using Microsoft.Xna.Framework.Audio;

namespace CoreEssentials.Audio;

public class AudioClipInstance
{
    private AudioClip audioClip;
    private ISoundEffectInstance soundEffectInstance;

    public AudioClip AudioClip => audioClip;

    public AudioClipInstance(AudioClip audioClip)
    {
        this.audioClip = audioClip;
    }

    public bool IsDonePlaying()
    {
        if (soundEffectInstance == null)
        {
            return true;
        }

        if (soundEffectInstance.State == SoundState.Stopped)
        {
            Cleanup();
            return true;
        }

        return false;
    }

    public void Play(float masterVolume)
    {
        if (soundEffectInstance == null)
        {
            soundEffectInstance = audioClip.SoundEffect.CreateInstance();
            UpdateVolume(masterVolume);
            soundEffectInstance.Play();
        }
        else
        {
            soundEffectInstance.Play();
        }
    }

    public virtual void Stop()
    {
        Cleanup();
    }

    private void Cleanup()
    {
        if (soundEffectInstance != null)
        {
            soundEffectInstance.Dispose();
            soundEffectInstance = null;
            // Only unload if it's an actual AudioClip (not a mock in tests)
            if (audioClip is Asset)
            {
               AssetManager.UnloadAsset<AudioClip>(audioClip.Name);
            }
        }
    }

    public void UpdateVolume(float masterVolume)
    {
        if (soundEffectInstance != null)
        {
            soundEffectInstance.Volume = audioClip.Volume * masterVolume;
            Debug.Console.WriteLine($"AudioClipInstance: Volume set to {soundEffectInstance.Volume}");
        }
    }
}
