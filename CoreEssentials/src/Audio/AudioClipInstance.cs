using System;
using CoreEssentials.Assets;
using Microsoft.Xna.Framework.Audio;

namespace CoreEssentials.Audio;

public class AudioClipInstance
{
    private AudioClip audioClip;
    private SoundEffectInstance soundEffectInstance;

    public AudioClipInstance(AudioClip audioClip)
    {
        this.audioClip = audioClip;
    }

    internal bool IsDonePlaying()
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

    internal void Play(float masterVolume)
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

    internal void Stop()
    {
        Cleanup();
    }

    private void Cleanup()
    {
        if (soundEffectInstance != null)
        {
            soundEffectInstance.Dispose();
            soundEffectInstance = null;
            AssetManager.UnloadAsset<AudioClip>(audioClip.Name);
        }
    }

    internal void UpdateVolume(float masterVolume)
    {
        if (soundEffectInstance != null)
        {
            soundEffectInstance.Volume = audioClip.Volume * masterVolume;
        }
    }
}
