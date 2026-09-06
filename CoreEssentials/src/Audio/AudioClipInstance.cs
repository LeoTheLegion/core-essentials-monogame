using System;
using CoreEssentials.Assets;
using Microsoft.Xna.Framework;
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
    /// Gets or sets the channel this instance plays on. Set by the manager at creation; used to
    /// resolve the per-channel volume factor into the effective volume.
    /// </summary>
    public AudioChannel Channel { get; set; } = AudioChannel.Master;

    // Per-instance knobs, owned by this instance (0..1 multiplier / semitones / -1..1).
    private float _instanceVolume = 1f;
    private float _pitchSemitones = 0f;
    private float _pan = 0f;
    // Last external scale (channel × master) the manager applied, so knob changes after Play
    // can be re-applied immediately without the manager having to push it again.
    private float _lastExternalScale = 1f;

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
    /// Starts or resumes playback of the audio clip and (re)applies the current per-instance knobs.
    /// </summary>
    /// <param name="externalScale">The combined channel × master volume scale to apply.</param>
    /// <exception cref="InvalidOperationException">Thrown when the underlying sound effect is not loaded.</exception>
    public void Play(float externalScale)
    {
        if (soundEffectInstance == null)
        {
            if (audioClip.SoundEffect == null)
            {
                throw new InvalidOperationException("Cannot play audio clip: sound effect is not loaded.");
            }
            soundEffectInstance = audioClip.SoundEffect.CreateInstance();
        }

        UpdateVolume(externalScale);
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
    /// Re-applies the full effective volume plus the current pitch and pan to the underlying
    /// instance. This is the single place where all volume factors are combined, so the clip,
    /// per-instance, channel and master knobs can never drift apart.
    /// </summary>
    /// <param name="externalScale">The combined channel × master volume scale.</param>
    public void UpdateVolume(float externalScale)
    {
        _lastExternalScale = externalScale;

        if (soundEffectInstance != null)
        {
            soundEffectInstance.Volume = MathHelper.Clamp(audioClip.Volume * _instanceVolume * externalScale, 0f, 1f);
            soundEffectInstance.Pitch = _pitchSemitones;
            soundEffectInstance.Pan = _pan;
        }
    }

    /// <summary>
    /// Sets this instance's own volume multiplier (0.0–1.0), independent of the clip, channel and
    /// master volumes. Re-applied immediately if the sound is already playing.
    /// </summary>
    /// <param name="volume">The per-instance volume, clamped between 0.0 and 1.0.</param>
    public void SetInstanceVolume(float volume)
    {
        _instanceVolume = MathHelper.Clamp(volume, 0f, 1f);
        UpdateVolume(_lastExternalScale);
    }

    /// <summary>
    /// Sets this instance's pitch in semitones (MonoGame's native unit; 0 = normal pitch, +12 = one
    /// octave up). Re-applied immediately if the sound is already playing.
    /// </summary>
    /// <param name="semitones">The pitch shift in semitones.</param>
    public void SetPitch(float semitones)
    {
        _pitchSemitones = semitones;
        UpdateVolume(_lastExternalScale);
    }

    /// <summary>
    /// Sets this instance's stereo pan (-1.0 = fully left, 0 = center, +1.0 = fully right).
    /// Re-applied immediately if the sound is already playing.
    /// </summary>
    /// <param name="pan">The pan position, clamped between -1.0 and 1.0.</param>
    public void SetPan(float pan)
    {
        _pan = MathHelper.Clamp(pan, -1f, 1f);
        UpdateVolume(_lastExternalScale);
    }
}
