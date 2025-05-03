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
            soundEffectInstance.Dispose();
            soundEffectInstance = null;
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
        if (soundEffectInstance != null)
        {
            soundEffectInstance.Stop(true);
            soundEffectInstance.Dispose();
            soundEffectInstance = null;
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
