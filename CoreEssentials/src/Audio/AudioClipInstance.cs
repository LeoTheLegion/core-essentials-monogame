using System;
using CoreEssentials.Assets;
using CoreEssentials.Debugging;
using Microsoft.Xna.Framework.Audio;

namespace CoreEssentials.Audio;

public class AudioClipInstance
{
    private IAudioClip audioClip;
    private ISoundEffectInstance soundEffectInstance;

    public IAudioClip AudioClip => audioClip;

    public AudioClipInstance(IAudioClip audioClip)
    {
        this.audioClip = audioClip;
    }

    internal virtual bool IsDonePlaying()
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

    internal virtual void Play(float masterVolume)
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

    internal virtual void Stop()
    {
        Cleanup();
    }

    protected virtual void Cleanup()
    {
        if (soundEffectInstance != null)
        {
            soundEffectInstance.Dispose();
            soundEffectInstance = null;
            // Only unload if it's an actual AudioClip (not a mock in tests)
            if (audioClip is AudioClip actualClip)
            {
                AssetManager.UnloadAsset<AudioClip>(actualClip.Name);
            }
        }
    }

    internal virtual void UpdateVolume(float masterVolume)
    {
        if (soundEffectInstance != null)
        {
            soundEffectInstance.Volume = audioClip.Volume * masterVolume;
            Debug.Console.WriteLine($"AudioClipInstance: Volume set to {soundEffectInstance.Volume}");
        }
    }
}
