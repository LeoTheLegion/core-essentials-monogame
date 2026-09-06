using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Xunit;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Components;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Components.BuiltIn;
using CoreEssentials.Audio;
using CoreEssentials.GUI;
using CoreEssentials.GUI.Types;
using CoreEssentials.Playground.Components;

namespace CoreEssentials.Tests.Playground;

/// <summary>
/// Sprint 3 (playground entity → components) — unit tests for the new GUI-button and camera
/// components that replace the hand-written SoundButtonEntity / VolumeButtonEntity / CameraEntity:
/// VolumeButtonComponent (a thin Clicked subscriber onto a pre-declared ButtonComponent that now
/// routes through the built-in AudioListenerComponent) and CameraFollowComponent (follow lerp + toggle), plus the follow-gating in
/// CameraInputComponent and the rewired CameraFollowToggleComponent. External side effects are
/// captured through the components' virtual seams; button clicks are raised via the component's
/// Clicked event field, so no live Myra input pipeline is required.
/// </summary>
public class Sprint3GuiAndCameraComponentTests : IDisposable
{
    private readonly Game _mockGame = null!;
    private bool _disposed;

    public Sprint3GuiAndCameraComponentTests()
    {
        // ButtonComponent.OnAttach builds a real widget via WidgetFactory, which needs the GUI
        // engine (MyraEnvironment) initialized — same fixture as ButtonComponentTests.
        _mockGame = new Game1();
        GUIManager.Init(_mockGame, 800, 600);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed) return;
        if (disposing)
        {
            _mockGame?.Dispose();

            var engine = CoreEssentials.GUI.Internal.EngineResolver.GetEngine();
            engine.Shutdown();
        }
        _disposed = true;
    }

    private class TestEntity : Entity
    {
        public override void Update(GameTime gameTime) { }
        public override void Render(SpriteBatch spriteBatch) { }
    }

    /// <summary>A GameTime with the given seconds elapsed (the components read ElapsedGameTime).</summary>
    private static GameTime Seconds(float s) => new(TimeSpan.FromSeconds(s), TimeSpan.FromSeconds(s));

    // ── Recording subclasses (capture the virtual seams) ─────────────────────────

    private class RecordingSource : AudioSourceComponent
    {
        public int OneShots;
        public string? LastAsset;
        protected override string? PlayOneShotClip(string asset, AudioChannel channel) { OneShots++; LastAsset = asset; return "oneshot-id"; }
    }

    private class RecordingVolumeButton : VolumeButtonComponent
    {
        public int Sets;
        public float? LastVolume;
        protected override void SetVolume(float volumeLevel) { Sets++; LastVolume = volumeLevel; }
    }

    private class GatedCameraInput : CameraInputComponent
    {
        public HashSet<Keys> Held = new();
        protected override bool IsKeyHeld(Keys key) => Held.Contains(key);
    }

    /// <summary>
    /// Raises the component's own Clicked event via its backing field, simulating a user click
    /// without driving Myra's input pipeline (a plain C# event compiles to a private instance
    /// field of delegate type, so it is located by type rather than by name).
    /// </summary>
    private static bool TryRaiseClicked(ButtonComponent component)
    {
        var field = component.GetType()
            .GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance)
            .FirstOrDefault(f => f.FieldType == typeof(Action));

        if (field == null) return false;
        var handler = (Action?)field.GetValue(component);
        if (handler == null) return false;

        handler.Invoke();
        return true;
    }

    // ── AudioSourceComponent (data-driven sound-button bind target) ───────────────

    [Fact]
    public void AudioSource_PlayOneShotNow_PlaysConfiguredAsset()
    {
        var comp = new RecordingSource { SoundAsset = "Audio/footstep1_sound.xml" };
        comp.PlayOneShotNow();
        Assert.Equal(1, comp.OneShots);
        Assert.Equal("Audio/footstep1_sound.xml", comp.LastAsset);
    }

    [Fact]
    public void AudioSource_PlayOneShotNow_EmptyAsset_DoesNothing()
    {
        var comp = new RecordingSource();
        comp.PlayOneShotNow();
        Assert.Equal(0, comp.OneShots);
        Assert.Null(comp.LastAsset);
    }

    // ── VolumeButtonComponent ─────────────────────────────────────────────────────

    [Fact]
    public void VolumeButton_Click_SetsConfiguredVolume()
    {
        var entity = new TestEntity();
        entity.AddComponent(new CanvasComponent());
        var button = (ButtonComponent)entity.AddComponent(new ButtonComponent("Volume: 10%"));
        var comp = (RecordingVolumeButton)entity.AddComponent(new RecordingVolumeButton
        {
            VolumeLevel = 0.1f
        });
        try
        {
            Assert.True(TryRaiseClicked(button));

            Assert.Equal(1, comp.Sets);
            Assert.Equal(0.1f, comp.LastVolume!.Value, 3);
        }
        finally { entity.RemoveComponent<RecordingVolumeButton>(); }
    }

    [Fact]
    public void VolumeButton_NoButtonComponent_DoesNotThrowAndNeverSets()
    {
        var entity = new TestEntity();
        var comp = (RecordingVolumeButton)entity.AddComponent(new RecordingVolumeButton
        {
            VolumeLevel = 0.5f
        });
        try
        {
            Assert.Equal(0, comp.Sets);
        }
        finally { entity.RemoveComponent<RecordingVolumeButton>(); }
    }

    [Fact]
    public void VolumeButton_Detach_UnsubscribesFromClicked()
    {
        var entity = new TestEntity();
        entity.AddComponent(new CanvasComponent());
        var button = (ButtonComponent)entity.AddComponent(new ButtonComponent("Volume: 10%"));
        var comp = (RecordingVolumeButton)entity.AddComponent(new RecordingVolumeButton
        {
            VolumeLevel = 0.5f
        });

        entity.RemoveComponent<RecordingVolumeButton>();

        // Our subscriber is gone — no handler remains on the component's Clicked event.
        Assert.False(TryRaiseClicked(button));
        Assert.Equal(0, comp.Sets);
    }

    // ── CameraFollowComponent ─────────────────────────────────────────────────────

    [Fact]
    public void Follow_Update_LerpsOwnerTowardTarget()
    {
        var system = new EntitySystem();
        var owner = system.CreateEntity<TestEntity>();
        var target = system.CreateEntity<TestEntity>();
        target.Position = new Vector2(100f, 0f);

        var comp = (CameraFollowComponent)owner.AddComponent(new CameraFollowComponent());
        try
        {
            Assert.False(comp.FollowingTarget);

            // No target → inert.
            comp.Update(Seconds(1f));
            Assert.Equal(Vector2.Zero, owner.Position);

            comp.SetFollowTarget(target);
            Assert.True(comp.FollowingTarget);

            // LerpFactor 0.5 from (0,0) toward (100,0) → (50,0).
            comp.LerpFactor = 0.5f;
            comp.Update(Seconds(1f));
            Assert.Equal(new Vector2(50f, 0f), owner.Position);

            // Next frame continues closing the gap: (75,0).
            comp.Update(Seconds(1f));
            Assert.Equal(new Vector2(75f, 0f), owner.Position);
        }
        finally { owner.RemoveComponent<CameraFollowComponent>(); }
    }

    [Fact]
    public void Follow_ToggleFollow_StartsAndStopsForSameTarget()
    {
        var system = new EntitySystem();
        var owner = system.CreateEntity<TestEntity>();
        var target = system.CreateEntity<TestEntity>();
        var other = system.CreateEntity<TestEntity>();

        var comp = (CameraFollowComponent)owner.AddComponent(new CameraFollowComponent());
        try
        {
            comp.ToggleFollow(target);
            Assert.True(comp.FollowingTarget);
            Assert.Same(target, comp.FollowTarget);

            // Toggling the same target stops following.
            comp.ToggleFollow(target);
            Assert.False(comp.FollowingTarget);
            Assert.Null(comp.FollowTarget);

            // A different target starts following that one.
            comp.ToggleFollow(other);
            Assert.True(comp.FollowingTarget);
            Assert.Same(other, comp.FollowTarget);
        }
        finally { owner.RemoveComponent<CameraFollowComponent>(); }
    }

    [Fact]
    public void Follow_DestroyedTarget_StopsFollowing()
    {
        var system = new EntitySystem();
        var owner = system.CreateEntity<TestEntity>();
        var target = system.CreateEntity<TestEntity>();
        target.Position = new Vector2(100f, 0f);

        var comp = (CameraFollowComponent)owner.AddComponent(new CameraFollowComponent());
        try
        {
            comp.SetFollowTarget(target);
            Assert.True(comp.FollowingTarget);

            target.Destroy();

            // The follow drops instead of lerping toward a destroyed entity.
            comp.Update(Seconds(1f));
            Assert.False(comp.FollowingTarget);
            Assert.Null(comp.FollowTarget);
        }
        finally { owner.RemoveComponent<CameraFollowComponent>(); }
    }

    // ── CameraInputComponent follow gating ────────────────────────────────────────

    [Fact]
    public void CameraInput_PanKeys_IgnoredWhileFollowing()
    {
        var system = new EntitySystem();
        var owner = system.CreateEntity<TestEntity>();
        var target = system.CreateEntity<TestEntity>();

        var follow = (CameraFollowComponent)owner.AddComponent(new CameraFollowComponent());
        var input = (GatedCameraInput)owner.AddComponent(new GatedCameraInput { MoveSpeed = 100f });
        try
        {
            // Not following → pan works.
            input.Held.Add(input.RightKey);
            input.Update(Seconds(1f));
            Assert.Equal(new Vector2(100f, 0f), owner.Position);

            // Following → pan keys are suspended (the follow lerp owns the position).
            owner.Position = Vector2.Zero;
            follow.SetFollowTarget(target);
            input.Update(Seconds(1f));
            Assert.Equal(Vector2.Zero, owner.Position);

            // Stopped following → pan works again.
            follow.SetFollowTarget(null);
            input.Update(Seconds(1f));
            Assert.Equal(new Vector2(100f, 0f), owner.Position);
        }
        finally
        {
            owner.RemoveComponent<GatedCameraInput>();
            owner.RemoveComponent<CameraFollowComponent>();
        }
    }

    // ── CameraFollowToggleComponent (rewired to CameraFollowComponent) ────────────

    private class FollowProbe : CameraFollowToggleComponent
    {
        /// <summary>Drives the real toggle against the live components.</summary>
        public void ProbeToggle() => base.DoToggle();
    }

    [Fact]
    public void FollowToggle_TogglesCameraFollowComponentAndInfoLabel()
    {
        // TextComponent.OnAttach loads the shared font — register a mock so attach works headlessly.
        var content = new MockContentManager();
        content.AddAsset<SpriteFont>("Fonts/base", CoreEssentials.Tests.MockSpriteFont.Instance);
        CoreEssentials.Assets.AssetManager.Init(content);

        var system = new EntitySystem();
        var camera = system.CreateEntity<TestEntity>();
        var player = system.CreateEntity<TestEntity>();
        var label = system.CreateEntity<TestEntity>();
        var text = (TextComponent)label.AddComponent(new TextComponent());
        try
        {
            var follow = (CameraFollowComponent)camera.AddComponent(new CameraFollowComponent());
            var toggleEntity = system.CreateEntity<TestEntity>();
            var toggle = (FollowProbe)toggleEntity.AddComponent(new FollowProbe
            {
                InfoTemplate = "state={state}",
                Camera = camera,
                FollowTarget = player,
                InfoLabel = label
            });
            try
            {
                // First toggle → following ON.
                toggle.ProbeToggle();
                Assert.True(follow.FollowingTarget);
                Assert.Same(player, follow.FollowTarget);
                Assert.Equal("state=ON", text.Text);

                // Second toggle → following OFF.
                toggle.ProbeToggle();
                Assert.False(follow.FollowingTarget);
                Assert.Equal("state=OFF", text.Text);
            }
            finally
            {
                camera.RemoveComponent<CameraFollowComponent>();
                toggleEntity.RemoveComponent<FollowProbe>();
            }
        }
        finally { label.RemoveComponent<TextComponent>(); }
    }

    [Fact]
    public void FollowToggle_CameraWithoutFollowComponent_DoesNothing()
    {
        var system = new EntitySystem();
        var camera = system.CreateEntity<TestEntity>(); // no CameraFollowComponent
        var player = system.CreateEntity<TestEntity>();
        var toggleEntity = system.CreateEntity<TestEntity>();
        var toggle = (FollowProbe)toggleEntity.AddComponent(new FollowProbe
        {
            Camera = camera,
            FollowTarget = player
        });
        try
        {
            toggle.ProbeToggle(); // must not throw
            Assert.Null(toggle.Camera?.GetComponent<CameraFollowComponent>());
        }
        finally { toggleEntity.RemoveComponent<FollowProbe>(); }
    }
}
