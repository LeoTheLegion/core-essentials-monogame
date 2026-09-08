using System;
using System.Collections.Generic;
using CoreEssentials.Assets;
using CoreEssentials.Audio;
using Microsoft.Xna.Framework;

namespace CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Components.BuiltIn;

/// <summary>
/// Built-in per-entity audio source — Unity's <c>AudioSource</c> translated to the Entity-Component
/// Framework. Attach it to any entity and the sound's playback state lives on that entity: it plays
/// (optionally looping) on attach, stops itself on detach (so a scene unload leaks no instances),
/// and exposes per-instance volume, pitch, pan and channel. All external effects go through small
/// <c>protected virtual</c> seams so unit tests can observe the requested asset/id without driving a
/// live <see cref="AudioManager"/> or loading real assets.
/// </summary>
/// <remarks>
/// Data-driven usage (looping music on a plain entity):
/// <code>
/// &lt;Component Type="AudioSourceComponent"&gt;
///   &lt;Properties&gt;
///     &lt;Property Name="SoundAsset" Value="Audio/background_music.xml" /&gt;
///     &lt;Property Name="Loop" Value="true" /&gt;
///     &lt;Property Name="Channel" Value="Music" /&gt;
///   &lt;/Properties&gt;
/// &lt;/Component&gt;
/// </code>
/// Transient effects use <see cref="PlayOneShot"/> (fire-and-forget, pruned by the manager when done)
/// — e.g. bound from a button's <c>Clicked</c> event in XML.
/// </remarks>
public class AudioSourceComponent : EntityComponent
{
    /// <summary>The asset name of the clip this source plays (e.g. "Audio/background_music.xml").</summary>
    public string SoundAsset { get; set; } = string.Empty;

    /// <summary>This source's volume multiplier (0.0–1.0), independent of clip/channel/master.</summary>
    public float Volume { get; set; } = 1f;

    /// <summary>This source's pitch as a Unity-style ratio (1.0 = normal, 2.0 = one octave up).</summary>
    public float Pitch { get; set; } = 1f;

    /// <summary>This source's stereo pan (-1 = left, 0 = center, +1 = right). Overridden per-frame when spatial is on.</summary>
    public float Pan { get; set; } = 0f;

    /// <summary>Whether the main source loops. Applied to the resolved clip before playback.</summary>
    public bool Loop { get; set; } = false;

    /// <summary>The channel this source plays on (affects which per-channel volume applies).</summary>
    public AudioChannel Channel { get; set; } = AudioChannel.Master;

    /// <summary>Whether to start playing automatically on attach. Defaults to true.</summary>
    public bool PlayOnAttach { get; set; } = true;

    /// <summary>
    /// Enables opt-in 2D spatialization: each frame the source's pan and attenuation are derived from
    /// its offset relative to the active <see cref="AudioListenerComponent"/>. Off by default — MonoGame
    /// has no native spatial audio, so this is our own math (see <see cref="SpatialAudioMath"/>).
    /// </summary>
    public bool SpatialEnabled { get; set; } = false;

    /// <summary>Distance within which a spatial source is heard at full volume.</summary>
    public float MinDistance { get; set; } = 50f;

    /// <summary>Distance beyond which a spatial source is silent (also the pan normalization range).</summary>
    public float MaxDistance { get; set; } = 500f;

    private AudioClip? _clip;
    private string? _instanceId;
    private readonly List<string> _oneShotIds = new();
    // Whether this component itself paused the main source on app-pause (so it only resumes what it paused).
    private bool _pausedByAppPause;

    /// <summary>The id of the main playing instance, or null when not playing.</summary>
    public string? InstanceId => _instanceId;

    /// <summary>Whether the main source is currently playing (or paused).</summary>
    public bool IsPlaying => _instanceId != null;

    /// <summary>
    /// Initializes a new instance of the <see cref="AudioSourceComponent"/> class.
    /// </summary>
    public AudioSourceComponent()
    {
    }

    /// <inheritdoc />
    public override void OnAttach()
    {
        base.OnAttach();

        _clip = ResolveClip(SoundAsset);

        if (PlayOnAttach && !string.IsNullOrWhiteSpace(SoundAsset))
            Play();
    }

    /// <inheritdoc />
    public override void OnDetach()
    {
        StopAllInstances();
        base.OnDetach();
    }

    /// <inheritdoc />
    public override void Update(GameTime gameTime)
    {
        if (!SpatialEnabled || _instanceId == null || Owner == null)
            return;

        var listener = AudioListenerComponent.ActiveListener;
        if (listener == null)
            return;

        var lp = listener.ListenerPosition;
        var sp = Owner.Position;
        var pan = SpatialAudioMath.ComputePan(lp.X, sp.X, MaxDistance);
        var distance = SpatialAudioMath.Distance(lp, sp);
        var attenuation = SpatialAudioMath.ComputeAttenuation(distance, MinDistance, MaxDistance);

        SetInstanceParams(_instanceId, Volume * attenuation, Pitch, pan);
    }

    /// <inheritdoc />
    /// <remarks>
    /// Pauses the main source when the application is backgrounded and resumes it on return — the
    /// standard background-audio behavior. Only a source that was playing (not already paused) is
    /// auto-paused, so an intentionally paused source is left alone.
    /// </remarks>
    public override void OnApplicationPause(bool paused)
    {
        if (_instanceId == null)
            return;

        if (paused)
        {
            PauseInstance(_instanceId);
            _pausedByAppPause = true;
        }
        else if (_pausedByAppPause)
        {
            ResumeInstance(_instanceId);
            _pausedByAppPause = false;
        }
    }

    /// <summary>
    /// Starts (or resumes) the main source from <see cref="SoundAsset"/>, applying this component's
    /// volume/pitch/pan. No-op when no asset is set.
    /// </summary>
    public void Play()
    {
        if (_clip == null || string.IsNullOrWhiteSpace(SoundAsset))
            return;

        _clip.Loop = Loop;
        _instanceId = PlayClip(_clip, Channel);
        if (_instanceId != null)
            SetInstanceParams(_instanceId, Volume, Pitch, Pan);
    }

    /// <summary>
    /// Fires a transient one-shot sound (fire-and-forget; the manager prunes it when done). Defaults
    /// to <see cref="SoundAsset"/> when no asset is given. Expects a non-looping SFX asset.
    /// </summary>
    /// <param name="asset">Optional asset name; falls back to <see cref="SoundAsset"/>.</param>
    public void PlayOneShot(string? asset = null)
    {
        var target = string.IsNullOrWhiteSpace(asset) ? SoundAsset : asset;
        if (string.IsNullOrWhiteSpace(target))
            return;

        var id = PlayOneShotClip(target, Channel);
        if (id != null)
            _oneShotIds.Add(id);
    }

    /// <summary>
    /// Parameterless one-shot command target for declarative &lt;Bind&gt; wiring: plays this source's
    /// own <see cref="SoundAsset"/>. This is the method a button's <c>Clicked</c> event binds to, so a
    /// sound button can be declared purely from data (a template with an <see cref="AudioSourceComponent"/>
    /// plus &lt;Bind Event="Clicked" Command="PlayOneShotNow"/&gt;) — no per-button C# needed.
    /// </summary>
    public void PlayOneShotNow() => PlayOneShot();

    /// <summary>Stops the main source.</summary>
    public void Stop()
    {
        if (_instanceId == null)
            return;
        StopInstance(_instanceId);
        _instanceId = null;
    }

    /// <summary>Pauses the main source (can be resumed with <see cref="Resume"/>).</summary>
    public void Pause()
    {
        if (_instanceId != null)
            PauseInstance(_instanceId);
    }

    /// <summary>Resumes a paused main source.</summary>
    public void Resume()
    {
        if (_instanceId != null)
            ResumeInstance(_instanceId);
    }

    private void StopAllInstances()
    {
        if (_instanceId != null)
        {
            StopInstance(_instanceId);
            _instanceId = null;
        }

        foreach (var id in _oneShotIds)
            StopInstance(id);
        _oneShotIds.Clear();
    }

    // ── Test seams ────────────────────────────────────────────────────────────────

    /// <summary>Resolves an asset name to a loaded <see cref="AudioClip"/>; returns null on failure.</summary>
    protected virtual AudioClip? ResolveClip(string asset)
    {
        if (string.IsNullOrWhiteSpace(asset))
            return null;
        try
        {
            return AssetManager.LoadAsset<AudioClip>(asset);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[AudioSourceComponent] Could not load audio asset '{asset}': {ex.Message}");
            return null;
        }
    }

    /// <summary>Plays a clip and returns the manager's instance id.</summary>
    protected virtual string? PlayClip(AudioClip clip, AudioChannel channel)
        => AudioManager.Instance.PlaySound(clip, channel);

    /// <summary>Plays a one-shot by asset name and returns the manager's instance id.</summary>
    protected virtual string? PlayOneShotClip(string asset, AudioChannel channel)
        => AudioManager.Instance.PlayOneShotSound(asset, channel);

    /// <summary>Applies volume/pitch/pan to an active instance.</summary>
    protected virtual void SetInstanceParams(string id, float volume, float pitchRatio, float pan)
    {
        var manager = AudioManager.Instance;
        manager.SetInstanceVolume(id, volume);
        manager.SetInstancePitch(id, pitchRatio);
        manager.SetInstancePan(id, pan);
    }

    /// <summary>Stops an active instance by id.</summary>
    protected virtual void StopInstance(string id)
        => AudioManager.Instance.StopSound(id);

    /// <summary>Pauses an active instance by id.</summary>
    protected virtual void PauseInstance(string id)
        => AudioManager.Instance.PauseSound(id);

    /// <summary>Resumes an active instance by id.</summary>
    protected virtual void ResumeInstance(string id)
        => AudioManager.Instance.ResumeSound(id);
}
