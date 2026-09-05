using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Xunit;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Components;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Components.BuiltIn;
using CoreEssentials.GUI;
using CoreEssentials.GUI.Internal;
using CoreEssentials.GUI.Types;
using CoreEssentials.Playground;
using CoreEssentials.Playground.Entities;
using CoreEssentials.Playground.Components;

namespace CoreEssentials.Tests.Playground;

/// <summary>
/// Sprint 5d — unit tests for the new playground components that port per-frame scene logic out of
/// C# scene subclasses so scenes can be driven purely from data: OrbitPanelComponent (elliptical
/// orbit), CameraFollowToggleComponent (follow toggle + info refresh), HudLabelRefreshComponent
/// (measured-size label refresh) and LabelAlignmentDebugOverlayComponent (canvas/label discovery).
/// External side effects are captured through the components' virtual seams, so no live camera,
/// canvas or graphics device is required for the pure tests.
/// </summary>
public class Sprint5dPureComponentTests
{
    private class TestEntity : Entity
    {
        public override void Update(GameTime gameTime) { }
        public override void Render(SpriteBatch spriteBatch) { }
    }

    /// <summary>A GameTime with the given seconds elapsed (the components read ElapsedGameTime).</summary>
    private static GameTime Seconds(float s) => new(TimeSpan.FromSeconds(s), TimeSpan.FromSeconds(s));

    // ── OrbitPanelComponent ───────────────────────────────────────────────────────

    private class OrbitProbe : OrbitPanelComponent
    {
        /// <summary>Exposes the protected trajectory math for direct assertion.</summary>
        public Vector2 Probe(float time) => base.ComputePosition(time);
    }

    [Fact]
    public void Orbit_ComputePosition_FollowsEllipse()
    {
        var comp = new OrbitProbe { CenterX = 100f, CenterY = 50f, RadiusX = 10f, RadiusY = 5f, Speed = 1f };

        // t=0 → rightmost point of the ellipse.
        Assert.Equal(new Vector2(110f, 50f), comp.Probe(0f));

        // t=π/2 (speed 1) → top of the ellipse. cos(π/2)≈0, sin(π/2)=1.
        var top = comp.Probe((float)(Math.PI / 2.0));
        Assert.Equal(100f, top.X, 3);
        Assert.Equal(55f, top.Y, 3);

        // t=π → leftmost point.
        var left = comp.Probe((float)Math.PI);
        Assert.Equal(90f, left.X, 3);
        Assert.Equal(50f, left.Y, 3);
    }

    [Fact]
    public void Orbit_Update_MovesOwnerAndAccumulatesTime()
    {
        var system = new EntitySystem();
        var entity = system.CreateEntity<TestEntity>();
        var comp = (OrbitProbe)entity.AddComponent(
            new OrbitProbe { CenterX = 100f, CenterY = 50f, RadiusX = 10f, RadiusY = 5f, Speed = 1f });
        try
        {
            comp.Update(Seconds(1f));

            Assert.Equal(1f, comp.ElapsedTime, 3);
            var expected = new Vector2(100f + (float)Math.Cos(1f) * 10f, 50f + (float)Math.Sin(1f) * 5f);
            Assert.Equal(expected, entity.Position);
        }
        finally { entity.RemoveComponent<OrbitProbe>(); }
    }

    // ── CameraFollowToggleComponent ───────────────────────────────────────────────

    private class RecordingFollow : CameraFollowToggleComponent
    {
        public int ToggleCalls;
        protected override void DoToggle() => ToggleCalls++;
    }

    private class FollowProbe : CameraFollowToggleComponent
    {
        /// <summary>Drives the real info-text substitution against a live TextEntity label.</summary>
        public void ProbeUpdate(bool following) => base.UpdateInfo(following);
    }

    [Fact]
    public void Follow_TriggerKey_Toggles_WrongKeyDoesNothing()
    {
        var entity = new TestEntity();
        var comp = (RecordingFollow)entity.AddComponent(new RecordingFollow());
        try
        {
            comp.HandleKey(Keys.A);
            Assert.Equal(0, comp.ToggleCalls);

            comp.HandleKey(comp.ToggleKey);
            Assert.Equal(1, comp.ToggleCalls);
        }
        finally { entity.RemoveComponent<RecordingFollow>(); }
    }

    [Fact]
    public void Follow_UpdateInfo_SubstitutesStateToken()
    {
        var label = new TextEntity();
        var comp = new FollowProbe { InfoTemplate = "state={state}", InfoLabel = label };

        comp.ProbeUpdate(true);
        Assert.Equal("state=ON", label.Text);

        comp.ProbeUpdate(false);
        Assert.Equal("state=OFF", label.Text);
    }

    // ── HudLabelRefreshComponent ──────────────────────────────────────────────────

    private class RecordingHud : HudLabelRefreshComponent
    {
        public int Refreshes;
        protected override void Refresh() => Refreshes++;
    }

    [Fact]
    public void Hud_Format_SubstitutesMeasuredSize()
    {
        var comp = new RecordingHud();

        Assert.Equal("(W=120 H=24)", comp.Format(120.7f, 24.3f));

        comp.TextTemplate = "LEFT   (W={w} H={h})";
        Assert.Equal("LEFT   (W=80 H=20)", comp.Format(80f, 20f));
    }

    [Fact]
    public void Hud_Update_ThrottlesRefreshByInterval()
    {
        var system = new EntitySystem();
        var entity = system.CreateEntity<TestEntity>();
        var comp = (RecordingHud)entity.AddComponent(new RecordingHud { IntervalSeconds = 0.5f });
        try
        {
            // 0.4s < 0.5s → no refresh yet.
            comp.Update(Seconds(0.4f));
            Assert.Equal(0, comp.Refreshes);

            // +0.6s → accumulated 1.0s ≥ interval → one refresh, accumulator reset.
            comp.Update(Seconds(0.6f));
            Assert.Equal(1, comp.Refreshes);
        }
        finally { entity.RemoveComponent<RecordingHud>(); }
    }

    // ── DebugToggleComponent (Sprint 5d additions) ───────────────────────────────

    private class RecordingDebugStart : DebugToggleComponent
    {
        public string? LoadedFont;
        protected override CoreEssentials.Assets.FontAsset LoadDebugFont(string assetName)
        {
            LoadedFont = assetName;
            return null;
        }
    }

    [Fact]
    public void Debug_StartEnabled_EnablesAndAppliesOnAttach()
    {
        var system = new EntitySystem();
        var entity = system.CreateEntity<TestEntity>();
        var comp = (RecordingDebugStart)entity.AddComponent(new RecordingDebugStart
        {
            StartEnabled = true,
            ShowEntityBounds = true,
            DebugFontAsset = "base"
        });
        try
        {
            // OnAttach ran at AddComponent time: debug mode is on, config applied, font requested.
            Assert.True(system.DebugMode);
            Assert.True(system.DebugConfig.ShowEntityBounds);
            Assert.Equal("base", comp.LoadedFont);
        }
        finally { entity.RemoveComponent<RecordingDebugStart>(); }
    }

    [Fact]
    public void Debug_NotStartEnabled_LeavesDebugOffUntilToggled()
    {
        var system = new EntitySystem();
        var entity = system.CreateEntity<TestEntity>();
        var comp = (RecordingDebugStart)entity.AddComponent(new RecordingDebugStart());
        try
        {
            Assert.False(system.DebugMode);

            comp.HandleKey(comp.TriggerKey);
            Assert.True(system.DebugMode);
        }
        finally { entity.RemoveComponent<RecordingDebugStart>(); }
    }
}

/// <summary>
/// Sprint 5d — tests for LabelAlignmentDebugOverlayComponent. These require a live GUI engine because
/// the overlay discovers real CanvasComponent/LabelComponent widgets and computes measured label bounds.
/// </summary>
public class Sprint5dOverlayComponentTests : IDisposable
{
    private readonly Game _mockGame;
    private bool _disposed;

    public Sprint5dOverlayComponentTests()
    {
        _mockGame = new Game1();
        GUIManager.Init(_mockGame, 800, 600);
    }

    public void Dispose()
    {
        if (_disposed) return;
        _mockGame?.Dispose();
        EngineResolver.GetEngine()?.Shutdown();
        _disposed = true;
        GC.SuppressFinalize(this);
    }

    private class TestEntity : Entity
    {
        public override void Update(GameTime gameTime) { }
        public override void Render(SpriteBatch spriteBatch) { }
    }

    [Fact]
    public void Overlay_DiscoverCanvases_FindsRootAndNested()
    {
        var system = new EntitySystem();
        var root = system.CreateEntity<TestEntity>();
        var child = system.CreateEntity<TestEntity>();
        root.AddChild(child);
        root.AddComponent(new CanvasComponent());
        child.AddComponent(new CanvasComponent());

        // An unrelated entity with no canvas is ignored.
        system.CreateEntity<TestEntity>();

        var comp = new LabelAlignmentDebugOverlayComponent();
        Assert.Equal(2, comp.DiscoverCanvases(system).Count);
    }

    [Fact]
    public void Overlay_DiscoverLabels_FindsNestedLabels()
    {
        var system = new EntitySystem();
        var root = system.CreateEntity<TestEntity>();
        var child = system.CreateEntity<TestEntity>();
        root.AddChild(child);
        root.AddComponent(new CanvasComponent());
        root.AddComponent(new LabelComponent("root"));
        child.AddComponent(new LabelComponent("child"));

        var comp = new LabelAlignmentDebugOverlayComponent();
        Assert.Equal(2, comp.DiscoverLabels(system).Count);
    }

    [Fact]
    public void Overlay_ComputeLabelBounds_AppliesAlignmentOffsets()
    {
        var system = new EntitySystem();
        var canvasRoot = system.CreateEntity<TestEntity>();
        canvasRoot.Position = Vector2.Zero;
        var canvas = canvasRoot.AddComponent(new CanvasComponent
        {
            IsScreenSpace = true,
            Width = 100f,
            Height = 50f
        });

        var labelHost = system.CreateEntity<TestEntity>();
        canvasRoot.AddChild(labelHost); // must be under the canvas so OnAttach can resolve it
        // Parented entities derive world position from LocalPosition (the Position setter is ignored).
        labelHost.LocalPosition = new Vector2(10f, 20f); // margin from the canvas origin
        var label = labelHost.AddComponent(new LabelComponent("x"));

        Func<Vector2, Vector2> identity = p => p;
        float canvasW = canvas.Canvas.Width;   // 100
        float canvasH = canvas.Canvas.Height;  // 50
        float sizeW = label.Width * label.Scale.X;
        float sizeH = label.Height * label.Scale.Y;

        // Left/Top (default): the top-left corner sits exactly at the margin.
        var leftTop = LabelAlignmentDebugOverlayComponent.ComputeLabelBounds(label, canvas, identity);
        Assert.Equal(10, leftTop.X);
        Assert.Equal(20, leftTop.Y);

        // The rect is built from integer pixel casts; because the margin (10,20) is integral, each
        // alignment offset below reduces to exactly (int)(offset).
        // Center: shifted from the margin by (half canvas − half label size) on each axis.
        label.HorizontalAlignment = HorizontalAlignment.Center;
        label.VerticalAlignment = VerticalAlignment.Center;
        var center = LabelAlignmentDebugOverlayComponent.ComputeLabelBounds(label, canvas, identity);
        Assert.Equal((int)(canvasW * 0.5f - sizeW * 0.5f), center.X - leftTop.X);
        Assert.Equal((int)(canvasH * 0.5f - sizeH * 0.5f), center.Y - leftTop.Y);

        // Right/Bottom: hugging the canvas's right and bottom edges.
        label.HorizontalAlignment = HorizontalAlignment.Right;
        label.VerticalAlignment = VerticalAlignment.Bottom;
        var rightBottom = LabelAlignmentDebugOverlayComponent.ComputeLabelBounds(label, canvas, identity);
        Assert.Equal((int)(canvasW - sizeW), rightBottom.X - leftTop.X);
        Assert.Equal((int)(canvasH - sizeH), rightBottom.Y - leftTop.Y);

        // The rect's own size always equals the (scaled) measured label size.
        Assert.Equal((int)sizeW, rightBottom.Width);
        Assert.Equal((int)sizeH, rightBottom.Height);
    }
}
