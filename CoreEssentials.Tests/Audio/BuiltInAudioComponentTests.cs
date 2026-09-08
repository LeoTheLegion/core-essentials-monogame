using System;
using CoreEssentials.Assets;
using CoreEssentials.Audio;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Components;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Components.BuiltIn;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Xunit;

namespace CoreEssentials.Tests.Audio;

/// <summary>
/// Tests for the built-in audio component surface added with the per-instance/channel AudioManager
/// upgrades: channel + per-instance volume/pitch/pan on <see cref="AudioManager"/>, the pure 2D
/// spatial math helpers, and the single-active-listener rule on <see cref="AudioListenerComponent"/>.
/// </summary>
public class BuiltInAudioComponentTests
{
    // ── AudioManager: channels + per-instance knobs ───────────────────────────────

    [Fact]
    public void SetChannelVolume_AppliesToInstancesOnThatChannelOnly()
    {
        var manager = new MockAudioManager();

        var musicSfx = new MockSoundEffect();
        var musicClip = new MockAudioClip("music.xml", 1.0f) { SoundEffect = musicSfx };
        var musicId = manager.PlaySound(musicClip, AudioChannel.Music);

        var sfxSfx = new MockSoundEffect();
        var sfxClip = new MockAudioClip("sfx.xml", 1.0f) { SoundEffect = sfxSfx };
        var sfxId = manager.PlaySound(sfxClip, AudioChannel.Sfx);

        // Act: duck the Music channel to half volume.
        manager.SetChannelVolume(AudioChannel.Music, 0.5f);

        // Assert: music instance is halved (1.0 × 0.5), SFX untouched (1.0).
        Assert.Equal(0.5f, musicSfx.LastCreatedInstance!.Volume, 0.001f);
        Assert.Equal(1.0f, sfxSfx.LastCreatedInstance!.Volume, 0.001f);

        manager.StopSound(musicId);
        manager.StopSound(sfxId);
    }

    [Fact]
    public void SetInstanceVolume_PitchAndPan_ReachUnderlyingInstance()
    {
        var manager = new MockAudioManager();
        var sfx = new MockSoundEffect();
        var clip = new MockAudioClip("s.xml", 1.0f) { SoundEffect = sfx };
        var id = manager.PlaySound(clip, AudioChannel.Master);

        // Act: drive the three per-instance knobs through the manager.
        manager.SetInstanceVolume(id, 0.25f);
        manager.SetInstancePitch(id, 2.0f);   // one octave up → +12 semitones
        manager.SetInstancePan(id, 0.75f);

        var underlying = sfx.LastCreatedInstance!;
        Assert.Equal(0.25f, underlying.Volume, 0.001f);
        Assert.Equal(12f, underlying.Pitch, 0.001f);
        Assert.Equal(0.75f, underlying.Pan, 0.001f);

        manager.StopSound(id);
    }

    [Fact]
    public void SetInstancePitch_ConvertsRatioToSemitones()
    {
        var manager = new MockAudioManager();
        var sfx = new MockSoundEffect();
        var clip = new MockAudioClip("s.xml", 1.0f) { SoundEffect = sfx };
        var id = manager.PlaySound(clip, AudioChannel.Master);

        // ratio 1.0 = normal pitch → 0 semitones; a non-positive ratio is clamped to normal.
        manager.SetInstancePitch(id, 1.0f);
        Assert.Equal(0f, sfx.LastCreatedInstance!.Pitch, 0.001f);

        manager.SetInstancePitch(id, 0f);
        Assert.Equal(0f, sfx.LastCreatedInstance!.Pitch, 0.001f);

        // ratio 4.0 = two octaves up → +24 semitones.
        manager.SetInstancePitch(id, 4.0f);
        Assert.Equal(24f, sfx.LastCreatedInstance!.Pitch, 0.001f);

        manager.StopSound(id);
    }

    [Fact]
    public void EffectiveVolume_CombinesClipChannelAndMaster()
    {
        var manager = new MockAudioManager();
        var sfx = new MockSoundEffect();
        // Clip volume 0.5, channel (Music) 0.8, master 1.0 → effective 0.4.
        var clip = new MockAudioClip("m.xml", 0.5f) { SoundEffect = sfx };
        var id = manager.PlaySound(clip, AudioChannel.Music);

        manager.SetChannelVolume(AudioChannel.Music, 0.8f);
        Assert.Equal(0.4f, sfx.LastCreatedInstance!.Volume, 0.001f);

        // Now drop master to 0.5 → effective 0.2.
        manager.SetMasterVolume(0.5f);
        Assert.Equal(0.2f, sfx.LastCreatedInstance!.Volume, 0.001f);

        manager.StopSound(id);
    }

    // ── SpatialAudioMath (pure helpers) ───────────────────────────────────────────

    [Theory]
    [InlineData(0f, 100f, 500f, 0.2f)]   // source right of listener → pans right
    [InlineData(0f, -100f, 500f, -0.2f)] // source left of listener → pans left
    public void ComputePan_MapsHorizontalOffsetToStereo(float listenerX, float sourceX, float maxDistance, float expected)
        => Assert.Equal(expected, SpatialAudioMath.ComputePan(listenerX, sourceX, maxDistance), 0.001f);

    [Fact]
    public void ComputePan_ClampsBeyondMaxDistance()
    {
        Assert.Equal(1f, SpatialAudioMath.ComputePan(0f, 5000f, 500f), 0.001f);
        Assert.Equal(-1f, SpatialAudioMath.ComputePan(0f, -5000f, 500f), 0.001f);
    }

    [Fact]
    public void ComputeAttenuation_FullInsideMin_SilentBeyondMax_LinearBetween()
    {
        Assert.Equal(1f, SpatialAudioMath.ComputeAttenuation(0f, 50f, 500f), 0.001f);   // inside min
        Assert.Equal(1f, SpatialAudioMath.ComputeAttenuation(50f, 50f, 500f), 0.001f);  // at min
        Assert.Equal(0f, SpatialAudioMath.ComputeAttenuation(500f, 50f, 500f), 0.001f); // at max
        Assert.Equal(0f, SpatialAudioMath.ComputeAttenuation(900f, 50f, 500f), 0.001f); // beyond max

        // Midpoint between min (50) and max (500) → ~0.5.
        Assert.Equal(0.5f, SpatialAudioMath.ComputeAttenuation(275f, 50f, 500f), 0.001f);
    }

    [Fact]
    public void ComputeAttenuation_DegenerateRange_Binary()
    {
        // max <= min: full at/inside min, silent beyond.
        Assert.Equal(1f, SpatialAudioMath.ComputeAttenuation(10f, 50f, 50f), 0.001f);
        Assert.Equal(0f, SpatialAudioMath.ComputeAttenuation(60f, 50f, 50f), 0.001f);
    }

    // ── AudioListenerComponent: single-active-listener rule ────────────────────────

    private class TestEntity : Entity
    {
        public override void Update(GameTime gameTime) { }
        public override void Render(SpriteBatch spriteBatch) { }
    }

    [Fact]
    public void SecondListener_DoesNotStealActiveSlot()
    {
        var first = new TestEntity();
        var second = new TestEntity();

        var listener1 = (AudioListenerComponent)first.AddComponent(new AudioListenerComponent());
        try
        {
            Assert.Same(listener1, AudioListenerComponent.ActiveListener);

            // A second attach must not steal the slot.
            var listener2 = (AudioListenerComponent)second.AddComponent(new AudioListenerComponent());
            Assert.Same(listener1, AudioListenerComponent.ActiveListener);

            second.RemoveComponent<AudioListenerComponent>();
        }
        finally
        {
            if (first.HasComponent<AudioListenerComponent>()) first.RemoveComponent<AudioListenerComponent>();
            if (second.HasComponent<AudioListenerComponent>()) second.RemoveComponent<AudioListenerComponent>();
        }

        // Detaching the active listener clears the slot.
        Assert.Null(AudioListenerComponent.ActiveListener);
    }

    [Fact]
    public void ListenerPosition_TracksOwner()
    {
        var entity = new TestEntity();
        var listener = (AudioListenerComponent)entity.AddComponent(new AudioListenerComponent());
        try
        {
            entity.Position = new Vector2(120f, 340f);
            Assert.Equal(new Vector2(120f, 340f), listener.ListenerPosition);
        }
        finally
        {
            if (entity.HasComponent<AudioListenerComponent>()) entity.RemoveComponent<AudioListenerComponent>();
        }
    }
}
