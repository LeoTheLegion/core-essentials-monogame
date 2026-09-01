using System;
using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Xunit;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Components;
using CoreEssentials.Inputs;
using CoreEssentials.Playground;

namespace CoreEssentials.Tests.Playground;

/// <summary>
/// Tests for the data-driven behavior components added to the playground so scenes can be driven
/// purely from XML. Each component routes its external side effect (scene load, audio, debug)
/// through a virtual seam that these recording subclasses capture, so no real audio or scene
/// transition is required. Attach/detach wiring against the static Input.Keyboard event is also
/// verified by firing KeyReleased directly.
/// </summary>
public class PlaygroundBehaviorComponentTests
{
    private class TestEntity : Entity
    {
        public override void Update(GameTime gameTime) { }
        public override void Render(SpriteBatch spriteBatch) { }
    }

    // ── Recording subclasses (capture the virtual seams) ─────────────────────────

    private class RecordingNavigate : NavigateOnKeyComponent
    {
        public string? LastLoaded;
        protected override void LoadScene(string sceneAssetName) => LastLoaded = sceneAssetName;
    }

    private class RecordingSound : SoundKeyComponent
    {
        public string? LastPlayed;
        protected override void PlaySound(string soundAsset) => LastPlayed = soundAsset;
    }

    private class RecordingVolume : VolumeKeyComponent
    {
        public float? LastVolume;
        protected override void SetVolume(float volume) => LastVolume = volume;
    }

    private class RecordingDebug : DebugToggleComponent
    {
        public bool Applied;
        public bool Bounds, Ids, Tags, Hierarchy, Position;
        protected override void ApplyDebugConfig(EntitySystem system)
        {
            Applied = true;
            Bounds = ShowEntityBounds;
            Ids = ShowEntityIds;
            Tags = ShowEntityTags;
            Hierarchy = ShowEntityHierarchy;
            Position = ShowEntityPosition;
        }
    }

    private class RecordingMusic : MusicComponent
    {
        public string? PlayedAsset;
        public string? PausedId;
        public string? ResumedId;
        public string? StoppedId;
        protected override string PlayMusic(string musicAsset) { PlayedAsset = musicAsset; return "music-id"; }
        protected override void PauseMusic(string soundId) => PausedId = soundId;
        protected override void ResumeMusic(string soundId) => ResumedId = soundId;
        protected override void StopMusic(string soundId) => StoppedId = soundId;
    }

    /// <summary>
    /// Reads a component's private key-release handler field to verify subscribe/unsubscribe wiring.
    /// </summary>
    private static EventHandler<KeyboardEventArgs>? GetHandler(EntityComponent component)
    {
        // Fields are not inherited in reflection, so walk up the type hierarchy.
        for (var t = component.GetType(); t != null; t = t.BaseType)
        {
            var field = t.GetField("_onKeyReleased", BindingFlags.NonPublic | BindingFlags.Instance);
            if (field != null)
                return (EventHandler<KeyboardEventArgs>?)field.GetValue(component);
        }
        return null;
    }

    // ── NavigateOnKeyComponent ────────────────────────────────────────────────────

    [Fact]
    public void Navigate_TriggerKey_LoadsTargetScene()
    {
        var entity = new TestEntity();
        var comp = (RecordingNavigate)entity.AddComponent(new RecordingNavigate());
        try
        {
            comp.TargetSceneAsset = "PhysicsEntityScene.xml";
            comp.HandleKey(comp.TriggerKey);

            Assert.Equal("PhysicsEntityScene.xml", comp.LastLoaded);
        }
        finally { entity.RemoveComponent<RecordingNavigate>(); }
    }

    [Fact]
    public void Navigate_WrongKey_DoesNothing()
    {
        var entity = new TestEntity();
        var comp = (RecordingNavigate)entity.AddComponent(new RecordingNavigate());
        try
        {
            comp.TargetSceneAsset = "PhysicsEntityScene.xml";
            comp.HandleKey(Keys.A);

            Assert.Null(comp.LastLoaded);
        }
        finally { entity.RemoveComponent<RecordingNavigate>(); }
    }

    [Fact]
    public void Navigate_SubscribesOnAttach_UnsubscribesOnDetach()
    {
        var entity = new TestEntity();
        var comp = (RecordingNavigate)entity.AddComponent(new RecordingNavigate());

        // Attached → a key-release handler is wired up.
        Assert.NotNull(GetHandler(comp));

        // Detached → the handler is unwired.
        entity.RemoveComponent<RecordingNavigate>();
        Assert.Null(GetHandler(comp));
    }

    // ── SoundKeyComponent ─────────────────────────────────────────────────────────

    [Fact]
    public void Sound_TriggerKey_PlaysAsset()
    {
        var entity = new TestEntity();
        var comp = (RecordingSound)entity.AddComponent(new RecordingSound());
        try
        {
            comp.SoundAsset = "footstep1_sound.xml";
            comp.HandleKey(comp.TriggerKey);

            Assert.Equal("footstep1_sound.xml", comp.LastPlayed);
        }
        finally { entity.RemoveComponent<RecordingSound>(); }
    }

    [Fact]
    public void Sound_WrongKey_DoesNothing()
    {
        var entity = new TestEntity();
        var comp = (RecordingSound)entity.AddComponent(new RecordingSound());
        try
        {
            comp.SoundAsset = "footstep1_sound.xml";
            comp.HandleKey(Keys.B);

            Assert.Null(comp.LastPlayed);
        }
        finally { entity.RemoveComponent<RecordingSound>(); }
    }

    // ── VolumeKeyComponent ────────────────────────────────────────────────────────

    [Fact]
    public void Volume_TriggerKey_SetsVolume()
    {
        var entity = new TestEntity();
        var comp = (RecordingVolume)entity.AddComponent(new RecordingVolume());
        try
        {
            comp.Volume = 0.1f;
            comp.HandleKey(comp.TriggerKey);

            Assert.Equal(0.1f, comp.LastVolume);
        }
        finally { entity.RemoveComponent<RecordingVolume>(); }
    }

    [Fact]
    public void Volume_WrongKey_DoesNothing()
    {
        var entity = new TestEntity();
        var comp = (RecordingVolume)entity.AddComponent(new RecordingVolume());
        try
        {
            comp.Volume = 0.1f;
            comp.HandleKey(Keys.C);

            Assert.Null(comp.LastVolume);
        }
        finally { entity.RemoveComponent<RecordingVolume>(); }
    }

    // ── DebugToggleComponent ──────────────────────────────────────────────────────

    [Fact]
    public void Debug_TriggerKey_TogglesAndAppliesConfig()
    {
        var system = new EntitySystem();
        var entity = system.CreateEntity<TestEntity>();
        var comp = (RecordingDebug)entity.AddComponent(new RecordingDebug());
        try
        {
            comp.ShowEntityBounds = true;
            comp.ShowEntityIds = true;

            Assert.False(system.DebugMode);
            comp.HandleKey(comp.TriggerKey);

            Assert.True(system.DebugMode);
            Assert.True(comp.Applied);
            Assert.True(comp.Bounds);
            Assert.True(comp.Ids);
            Assert.False(comp.Tags);

            // Second press toggles off.
            comp.HandleKey(comp.TriggerKey);
            Assert.False(system.DebugMode);
        }
        finally { entity.RemoveComponent<RecordingDebug>(); }
    }

    [Fact]
    public void Debug_WrongKey_DoesNothing()
    {
        var system = new EntitySystem();
        var entity = system.CreateEntity<TestEntity>();
        var comp = (RecordingDebug)entity.AddComponent(new RecordingDebug());
        try
        {
            comp.HandleKey(Keys.F1);

            Assert.False(system.DebugMode);
            Assert.False(comp.Applied);
        }
        finally { entity.RemoveComponent<RecordingDebug>(); }
    }

    // ── MusicComponent ────────────────────────────────────────────────────────────

    [Fact]
    public void Music_PlaysOnAttach_StopsOnDetach()
    {
        var entity = new TestEntity();
        var comp = (RecordingMusic)entity.AddComponent(new RecordingMusic());
        try
        {
            // OnAttach already ran at AddComponent time, but MusicAsset was empty then.
            Assert.Null(comp.PlayedAsset);

            // Simulate a configured track: re-drive attach by detaching/re-attaching with asset set.
            entity.RemoveComponent<RecordingMusic>();
            var comp2 = (RecordingMusic)entity.AddComponent(new RecordingMusic { MusicAsset = "song1_sound.xml" });
            try
            {
                Assert.Equal("song1_sound.xml", comp2.PlayedAsset);

                // Detach stops the track.
                entity.RemoveComponent<RecordingMusic>();
                Assert.Equal("music-id", comp2.StoppedId);
            }
            finally { if (entity.HasComponent<RecordingMusic>()) entity.RemoveComponent<RecordingMusic>(); }
        }
        finally { if (entity.HasComponent<RecordingMusic>()) entity.RemoveComponent<RecordingMusic>(); }
    }

    [Fact]
    public void Music_PauseResume_ForwardedFromEntity()
    {
        var entity = new TestEntity();
        var comp = (RecordingMusic)entity.AddComponent(new RecordingMusic { MusicAsset = "song1_sound.xml" });
        try
        {
            Assert.Equal("song1_sound.xml", comp.PlayedAsset);

            // Entity forwards OnApplicationPause to its components.
            entity.OnApplicationPause(true);
            Assert.Equal("music-id", comp.PausedId);

            entity.OnApplicationPause(false);
            Assert.Equal("music-id", comp.ResumedId);
        }
        finally { entity.RemoveComponent<RecordingMusic>(); }
    }

    [Fact]
    public void Music_EmptyAsset_DoesNotPlay()
    {
        var entity = new TestEntity();
        var comp = (RecordingMusic)entity.AddComponent(new RecordingMusic());
        try
        {
            Assert.Null(comp.PlayedAsset);
            entity.OnApplicationPause(true);
            Assert.Null(comp.PausedId);
        }
        finally { entity.RemoveComponent<RecordingMusic>(); }
    }
}
