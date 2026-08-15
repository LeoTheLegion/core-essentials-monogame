using System;
using CoreEssentials.Assets;
using Microsoft.Xna.Framework.Audio;

namespace CoreEssentials.Audio;

/// <summary>
/// Represents an active instance of an <see cref="AudioClip"/>.
/// Provides playback control and lifetime management for a single playing sound.
/// </summary>
public class AudioClipInstance
{
    private AudioClip audioClip;
    // This field is intentionally nullable until the first Play call.
    private ISoundEffectInstance? soundEffectInstance;

    /// <summary>
    /// Gets the <see cref="AudioClip"/> associated with this instance.
    /// </summary>
    public AudioClip AudioClip => audioClip;

    /// <summary>
    /// Initializes a new instance of the <see cref="AudioClipInstance"/> class.
    /// </summary>
    /// <param name="audioClip">The audio clip to play.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="audioClip"/> is null.</exception>
    public AudioClipInstance(AudioClip audioClip)
    {
        this.audioClip = audioClip ?? throw new ArgumentNullException(nameof(audioClip));
    }

    /// <summary>
    /// Determines whether this audio instance has finished playing.
    /// </summary>
    /// <returns>
    /// <see langword="true"/> if the sound has never been played, has been stopped, or has finished naturally;
    /// otherwise, <see langword="false"/>.
    /// </returns>
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

    /// <summary>
    /// Starts or resumes playback of the audio clip.
    /// </summary>
    /// <param name="masterVolume">The master volume scale to apply.</param>
    /// <exception cref="InvalidOperationException">Thrown when the underlying sound effect is not loaded.</exception>
    public void Play(float masterVolume)
    {
        if (soundEffectInstance == null)
        {
            if (audioClip.SoundEffect == null)
            {
                throw new InvalidOperationException("Cannot play audio clip: sound effect is not loaded.");
            }
            soundEffectInstance = audioClip.SoundEffect.CreateInstance();
            UpdateVolume(masterVolume);
        }
        soundEffectInstance?.Play();
    }

    /// <summary>
    /// Pauses playback of the audio clip without releasing the underlying sound effect instance.
    /// A paused instance can later be resumed with <see cref="Play"/>.
    /// </summary>
    public void Pause()
    {
        soundEffectInstance?.Pause();
    }

    /// <summary>
    /// Stops playback of the audio clip and releases the underlying sound effect instance.
    /// </summary>
    public virtual void Stop()
    {
        soundEffectInstance?.Stop();
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

    /// <summary>
    /// Updates the volume of the currently playing sound.
    /// </summary>
    /// <param name="masterVolume">The master volume scale to apply.</param>
    public void UpdateVolume(float masterVolume)
    {
        if (soundEffectInstance != null)
        {
            soundEffectInstance.Volume = audioClip.Volume * masterVolume;
            Console.WriteLine($"AudioClipInstance: Volume set to {soundEffectInstance.Volume}");
        }
    }
}
