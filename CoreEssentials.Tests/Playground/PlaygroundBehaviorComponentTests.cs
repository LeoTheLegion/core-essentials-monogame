using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Xunit;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Components;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Components.BuiltIn;
using CoreEssentials.GameSystems.Physics.Types;
using CoreEssentials.GUI.Types;
using CoreEssentials.Inputs;
using CoreEssentials.Playground;
using CoreEssentials.Playground.Entities;
using CoreEssentials.Playground.Components;

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

    private class RecordingCameraInput : CameraInputComponent
    {
        public HashSet<Keys> Held = new();
        // Real Update/ResetCamera run (no camera component → pan + position reset are observable).
        protected override bool IsKeyHeld(Keys key) => Held.Contains(key);
    }

    private class RecordingPing : PingControlComponent
    {
        public int Broadcasts;
        public string? BroadcastMessage;
        public Vector2? PrefabPosition;
        public string? PrefabUsed;
        public Vector2? TypedPosition;
        public int Destroys;
        protected override int Broadcast() { Broadcasts++; BroadcastMessage = MessageName; return 1; }
        protected override Entity? SpawnPrefab(Vector2 position) { PrefabPosition = position; PrefabUsed = PrefabName; return null; }
        protected override Entity? SpawnTyped(Vector2 position) { TypedPosition = position; return null; }
        protected override void DestroyLast() => Destroys++;
    }

    /// <summary>A GameTime with 1 second elapsed (the component reads ElapsedGameTime).</summary>
    private static GameTime OneSecond() => new(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1));

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

    // ── CameraInputComponent ──────────────────────────────────────────────────────

    [Fact]
    public void Camera_PanKeys_MoveOwnerEntity()
    {
        var entity = new TestEntity();
        var comp = (RecordingCameraInput)entity.AddComponent(new RecordingCameraInput());
        try
        {
            comp.MoveSpeed = 100f;
            var start = entity.Position;

            // Hold Right → moves +X by MoveSpeed * dt.
            comp.Held.Add(comp.RightKey);
            comp.Update(OneSecond());
            Assert.Equal(start + new Vector2(100f, 0f), entity.Position);

            // Hold Up → moves -Y.
            comp.Held.Clear();
            comp.Held.Add(comp.UpKey);
            var afterRight = entity.Position;
            comp.Update(OneSecond());
            Assert.Equal(afterRight + new Vector2(0f, -100f), entity.Position);

            // No keys held → no movement.
            comp.Held.Clear();
            var stable = entity.Position;
            comp.Update(OneSecond());
            Assert.Equal(stable, entity.Position);
        }
        finally { entity.RemoveComponent<RecordingCameraInput>(); }
    }

    [Fact]
    public void Camera_ResetKey_ResetsOwnerPosition()
    {
        var entity = new TestEntity();
        var comp = (RecordingCameraInput)entity.AddComponent(new RecordingCameraInput());
        try
        {
            entity.Position = new Vector2(50f, 60f);

            // Wrong key → no reset.
            comp.HandleKey(Keys.T);
            Assert.Equal(new Vector2(50f, 60f), entity.Position);

            // Reset key → position back to origin (no camera component present).
            comp.HandleKey(comp.ResetKey);
            Assert.Equal(Vector2.Zero, entity.Position);
        }
        finally { entity.RemoveComponent<RecordingCameraInput>(); }
    }

    [Fact]
    public void Camera_SubscribesOnAttach_UnsubscribesOnDetach()
    {
        var entity = new TestEntity();
        var comp = (RecordingCameraInput)entity.AddComponent(new RecordingCameraInput());
        Assert.NotNull(GetHandler(comp));
        entity.RemoveComponent<RecordingCameraInput>();
        Assert.Null(GetHandler(comp));
    }

    // ── PingControlComponent ──────────────────────────────────────────────────────

    [Fact]
    public void Ping_BroadcastKey_SendsConfiguredMessage()
    {
        var entity = new TestEntity();
        var comp = (RecordingPing)entity.AddComponent(new RecordingPing());
        try
        {
            comp.MessageName = "OnPing";
            comp.HandleKey(comp.BroadcastKey);

            Assert.Equal(1, comp.Broadcasts);
            Assert.Equal("OnPing", comp.BroadcastMessage);
        }
        finally { entity.RemoveComponent<RecordingPing>(); }
    }

    [Fact]
    public void Ping_SpawnPrefabKey_SpawnsAtStaggeredPosition()
    {
        var entity = new TestEntity();
        var comp = (RecordingPing)entity.AddComponent(new RecordingPing());
        try
        {
            comp.PrefabName = "PingPrefab";
            comp.SpawnPosition = new Vector2(640, 450);

            comp.HandleKey(comp.SpawnPrefabKey);
            Assert.Equal("PingPrefab", comp.PrefabUsed);
            // First spawn → offset (1 % 5) * 80 = 80.
            Assert.Equal(new Vector2(720, 450), comp.PrefabPosition);

            comp.HandleKey(comp.SpawnPrefabKey);
            // Second spawn → offset (2 % 5) * 80 = 160.
            Assert.Equal(new Vector2(800, 450), comp.PrefabPosition);
        }
        finally { entity.RemoveComponent<RecordingPing>(); }
    }

    [Fact]
    public void Ping_SpawnTypedKey_SpawnsAtStaggeredPosition()
    {
        var entity = new TestEntity();
        var comp = (RecordingPing)entity.AddComponent(new RecordingPing());
        try
        {
            comp.SpawnPosition = Vector2.Zero;
            comp.HandleKey(comp.SpawnTypedKey);

            // First spawn → offset (1 % 5) * 80 = 80.
            Assert.Equal(new Vector2(80, 0), comp.TypedPosition);
        }
        finally { entity.RemoveComponent<RecordingPing>(); }
    }

    [Fact]
    public void Ping_DestroyKey_TracksAndDestroys()
    {
        var entity = new TestEntity();
        var comp = (RecordingPing)entity.AddComponent(new RecordingPing());
        try
        {
            // No spawn yet → destroy is a no-op but still routed.
            comp.HandleKey(comp.DestroyLastKey);
            Assert.Equal(1, comp.Destroys);

            comp.HandleKey(comp.DestroyLastKey);
            Assert.Equal(2, comp.Destroys);
        }
        finally { entity.RemoveComponent<RecordingPing>(); }
    }

    [Fact]
    public void Ping_WrongKey_DoesNothing()
    {
        var entity = new TestEntity();
        var comp = (RecordingPing)entity.AddComponent(new RecordingPing());
        try
        {
            comp.HandleKey(Keys.J);

            Assert.Equal(0, comp.Broadcasts);
            Assert.Null(comp.PrefabPosition);
            Assert.Null(comp.TypedPosition);
            Assert.Equal(0, comp.Destroys);
        }
        finally { entity.RemoveComponent<RecordingPing>(); }
    }

    [Fact]
    public void Ping_SubscribesOnAttach_UnsubscribesOnDetach()
    {
        var entity = new TestEntity();
        var comp = (RecordingPing)entity.AddComponent(new RecordingPing());
        Assert.NotNull(GetHandler(comp));
        entity.RemoveComponent<RecordingPing>();
        Assert.Null(GetHandler(comp));
    }

    // ── Batch C recording helpers ────────────────────────────────────────────────

    private class FakeButton : IButton
    {
        public bool AutoWidth { get; set; }
        public bool AutoHeight { get; set; }
        public float Width { get; set; }
        public float Height { get; set; }
        public bool Visible { get; set; } = true;
        public bool Enabled { get; set; } = true;
        public bool IsMouseInside => false;
        public bool IsKeyboardFocused => false;
        public Vector2 Position { get; set; }
        public Thickness Margin { get; set; }
        public HorizontalAlignment HorizontalAlignment { get; set; }
        public VerticalAlignment VerticalAlignment { get; set; }
        public Vector2 Scale { get; set; } = Vector2.One;
        public Vector2 TransformOrigin { get; set; }
        public float Opacity { get; set; } = 1f;
        public string? Text { get; set; }
        public event Action<IButton>? Clicked;

        public void RaiseClick() => Clicked?.Invoke(this);
    }

    private class RecordingSpawn : PhysicsSpawnComponent
    {
        public int InstantiateCalls;
        public List<Vector2> Positions = new();
        public List<CollisionCategory?> FilterCategories = new();
        public List<CollisionCategory?> FilterCollidesWith = new();
        public List<Vector2> Impulses = new();
        public List<string> VipIds = new();
        public List<Color> VipColors = new();
        public float? VipScale;
        public bool WorldBorderCreated;

        protected override Entity? InstantiateBall(Vector2 position) { InstantiateCalls++; Positions.Add(position); return new TestEntity(); }
        protected override CollisionCategory? ResolveCategory(string name) => name == "Player" ? CollisionCategory.Cat1 : (CollisionCategory?)CollisionCategory.Cat2;
        protected override void ApplyCollisionFilter(Entity ball, CollisionCategory? categories, CollisionCategory? collidesWith) { FilterCategories.Add(categories); FilterCollidesWith.Add(collidesWith); }
        protected override void ApplyImpulse(Entity ball, Vector2 impulse) => Impulses.Add(impulse);
        protected override void ConfigureVipBall(Entity ball, string id, Color color, float scale) { VipIds.Add(id); VipColors.Add(color); VipScale = scale; }
        protected override void CreateWorldBorderEntity(Vector2 position, Vector2 size) => WorldBorderCreated = true;
    }

    private class RecordingSaveLoad : SaveLoadButtonsComponent
    {
        public FakeButton? SaveBtn;
        public FakeButton? LoadBtn;
        public List<IWidget> Added = new();
        public List<IWidget> Removed = new();
        public int Saves;
        public int Loads;

        protected override IButton? CreateSaveButton() { SaveBtn = new FakeButton { Text = SaveButtonLabel }; return SaveBtn; }
        protected override IButton? CreateLoadButton() { LoadBtn = new FakeButton { Text = LoadButtonLabel }; return LoadBtn; }
        protected override void AddWidget(IWidget widget) => Added.Add(widget);
        protected override void RemoveWidget(IWidget widget) => Removed.Add(widget);
        protected override void Save() => Saves++;
        protected override void Load() => Loads++;
    }

    private class FakeDebugRenderer : IPhysicsDebugRenderer
    {
        public bool IsEnabled { get; set; }
        public int Draws;
        public void Draw(SpriteBatch spriteBatch) => Draws++;
        public void Dispose() { }
    }

    private class RecordingOverlay : PhysicsDebugOverlayComponent
    {
        public FakeDebugRenderer Renderer = new();
        protected override IPhysicsDebugRenderer? GetDebugRenderer() => Renderer;
    }

    // ── PhysicsSpawnComponent (T7) ───────────────────────────────────────────────

    [Fact]
    public void Spawn_SpawnsConfiguredRegularBalls_WithFilterAndImpulse()
    {
        var comp = new RecordingSpawn
        {
            RegularBallCount = 3,
            VipBallIds = "",
            CreateWorldBorder = false
        };

        comp.Spawn();

        Assert.Equal(3, comp.InstantiateCalls);
        // Each regular ball gets a collision filter + an impulse.
        Assert.Equal(3, comp.FilterCategories.Count);
        Assert.Equal(3, comp.Impulses.Count);
        // Regular balls resolve to the "Player" category (Cat1 in the fake).
        Assert.All(comp.FilterCategories, c => Assert.Equal(CollisionCategory.Cat1, c));
        // No VIP balls configured.
        Assert.Empty(comp.VipIds);
    }

    [Fact]
    public void Spawn_SpawnsVipBalls_WithIdColorScaleAndFilter()
    {
        var comp = new RecordingSpawn
        {
            RegularBallCount = 0,
            VipBallIds = "vip_a,vip_b",
            VipBallPositions = "10,20;30,40",
            VipBallColors = "Blue,Green",
            VipBallScale = 2.5f,
            CreateWorldBorder = false
        };

        comp.Spawn();

        // One instantiate per VIP ball.
        Assert.Equal(2, comp.InstantiateCalls);
        Assert.Equal(new[] { "vip_a", "vip_b" }, comp.VipIds);
        Assert.Equal(Color.Blue, comp.VipColors[0]);
        Assert.Equal(Color.Green, comp.VipColors[1]);
        Assert.Equal(2.5f, comp.VipScale);
        // VIP positions are parsed from the semicolon-separated list.
        Assert.Contains(new Vector2(10, 20), comp.Positions);
        Assert.Contains(new Vector2(30, 40), comp.Positions);
        // Each VIP ball resolves to the "Vip" category (Cat2 in the fake).
        Assert.All(comp.FilterCategories, c => Assert.Equal(CollisionCategory.Cat2, c));
    }

    [Fact]
    public void Spawn_CreatesWorldBorder_WhenEnabled()
    {
        var comp = new RecordingSpawn
        {
            RegularBallCount = 0,
            VipBallIds = "",
            CreateWorldBorder = true
        };

        comp.Spawn();

        Assert.True(comp.WorldBorderCreated);
    }

    [Fact]
    public void Spawn_SkipsWorldBorder_WhenDisabled()
    {
        var comp = new RecordingSpawn
        {
            RegularBallCount = 0,
            VipBallIds = "",
            CreateWorldBorder = false
        };

        comp.Spawn();

        Assert.False(comp.WorldBorderCreated);
    }

    [Fact]
    public void Spawn_SpawnsOnAttach()
    {
        var entity = new TestEntity();
        var comp = (RecordingSpawn)entity.AddComponent(new RecordingSpawn { RegularBallCount = 2, VipBallIds = "" });
        try
        {
            // OnAttach ran at AddComponent time → spawn already occurred.
            Assert.Equal(2, comp.InstantiateCalls);
        }
        finally { entity.RemoveComponent<RecordingSpawn>(); }
    }

    // ── SaveLoadButtonsComponent (T8) ────────────────────────────────────────────

    [Fact]
    public void SaveLoad_CreatesAndAddsBothButtons_OnAttach()
    {
        var entity = new TestEntity();
        var comp = (RecordingSaveLoad)entity.AddComponent(new RecordingSaveLoad());
        try
        {
            Assert.NotNull(comp.SaveBtn);
            Assert.NotNull(comp.LoadBtn);
            // Both buttons are added to the GUI root.
            Assert.Equal(2, comp.Added.Count);
            Assert.Contains(comp.SaveBtn, comp.Added);
            Assert.Contains(comp.LoadBtn, comp.Added);
        }
        finally { entity.RemoveComponent<RecordingSaveLoad>(); }
    }

    [Fact]
    public void SaveLoad_SaveButtonClick_SavesState()
    {
        var entity = new TestEntity();
        var comp = (RecordingSaveLoad)entity.AddComponent(new RecordingSaveLoad());
        try
        {
            comp.SaveBtn.RaiseClick();
            Assert.Equal(1, comp.Saves);
            Assert.Equal(0, comp.Loads);
        }
        finally { entity.RemoveComponent<RecordingSaveLoad>(); }
    }

    [Fact]
    public void SaveLoad_LoadButtonClick_LoadsState()
    {
        var entity = new TestEntity();
        var comp = (RecordingSaveLoad)entity.AddComponent(new RecordingSaveLoad());
        try
        {
            comp.LoadBtn.RaiseClick();
            Assert.Equal(1, comp.Loads);
            Assert.Equal(0, comp.Saves);
        }
        finally { entity.RemoveComponent<RecordingSaveLoad>(); }
    }

    [Fact]
    public void SaveLoad_RemovesBothButtons_OnDetach()
    {
        var entity = new TestEntity();
        var comp = (RecordingSaveLoad)entity.AddComponent(new RecordingSaveLoad());

        // Detach removes both widgets from the GUI root.
        entity.RemoveComponent<RecordingSaveLoad>();
        Assert.Equal(2, comp.Removed.Count);
        Assert.Contains(comp.SaveBtn, comp.Removed);
        Assert.Contains(comp.LoadBtn, comp.Removed);
    }

    // ── PhysicsDebugOverlayComponent (T9) ────────────────────────────────────────

    [Fact]
    public void Overlay_ToggleKey_FlipsRendererEnabled()
    {
        var entity = new TestEntity();
        var comp = (RecordingOverlay)entity.AddComponent(new RecordingOverlay());
        try
        {
            Assert.False(comp.Renderer.IsEnabled);
            comp.HandleKey(comp.ToggleKey);
            Assert.True(comp.Renderer.IsEnabled);

            // Second press toggles back off.
            comp.HandleKey(comp.ToggleKey);
            Assert.False(comp.Renderer.IsEnabled);
        }
        finally { entity.RemoveComponent<RecordingOverlay>(); }
    }

    [Fact]
    public void Overlay_WrongKey_DoesNothing()
    {
        var entity = new TestEntity();
        var comp = (RecordingOverlay)entity.AddComponent(new RecordingOverlay());
        try
        {
            comp.HandleKey(Keys.F2);
            Assert.False(comp.Renderer.IsEnabled);
        }
        finally { entity.RemoveComponent<RecordingOverlay>(); }
    }

    [Fact]
    public void Overlay_DrawsOnlyWhenEnabled()
    {
        var entity = new TestEntity();
        var comp = (RecordingOverlay)entity.AddComponent(new RecordingOverlay());
        try
        {
            // Disabled → no draw.
            comp.Draw(null);
            Assert.Equal(0, comp.Renderer.Draws);

            // Enabled → draws through to the renderer.
            comp.Renderer.IsEnabled = true;
            comp.Draw(null);
            Assert.Equal(1, comp.Renderer.Draws);
        }
        finally { entity.RemoveComponent<RecordingOverlay>(); }
    }

    [Fact]
    public void Overlay_SubscribesOnAttach_UnsubscribesOnDetach()
    {
        var entity = new TestEntity();
        var comp = (RecordingOverlay)entity.AddComponent(new RecordingOverlay());
        Assert.NotNull(GetHandler(comp));
        entity.RemoveComponent<RecordingOverlay>();
        Assert.Null(GetHandler(comp));
    }
}
