using System;
using CoreEssentials.Assets;
using Microsoft.Xna.Framework.Audio;

namespace CoreEssentials.Audio;

public class AudioClipInstance
{
    private AudioClip audioClip;
    // This field is intentionally nullable until the first Play call.
    private ISoundEffectInstance? soundEffectInstance;

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
            // Don't cleanup if this is a looping sound, as we'll want to restart it
            if (!audioClip.Loop)
            {
                Cleanup();
            }
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
        }
        soundEffectInstance.Play();
    }

    public virtual void Stop()
    {
        if (soundEffectInstance != null)
        {
            soundEffectInstance.Stop();
        }
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

    public void UpdateVolume(float masterVolume)
    {
        if (soundEffectInstance != null)
        {
            soundEffectInstance.Volume = audioClip.Volume * masterVolume;
            Console.WriteLine($"AudioClipInstance: Volume set to {soundEffectInstance.Volume}");
        }
    }
}
