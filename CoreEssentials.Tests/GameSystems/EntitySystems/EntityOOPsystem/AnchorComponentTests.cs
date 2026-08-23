using System;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Components.BuiltIn;
using CoreEssentials.GUI;
using CoreEssentials.GUI.Internal;
using Microsoft.Xna.Framework;
using Xunit;

namespace CoreEssentials.Tests.GameSystems.EntitySystems.EntityOOPsystem;

public class AnchorComponentTests : IDisposable
{
    private readonly Game _mockGame = null!;
    private bool _disposed;

    public AnchorComponentTests()
    {
        // Create a real Game instance and initialize the GUI engine so that
        // GUIManager.Width / Height report a known screen rect (800 x 600).
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

            // Shutdown the engine to clean up internal state.
            var engine = EngineResolver.GetEngine();
            engine.Shutdown();
        }
        _disposed = true;
    }

    private class TestEntity : Entity
    {
        public override void Update(GameTime gameTime) { }
        public override void Render(Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch) { }
    }

    // ===== Construction =====

    [Fact]
    public void Constructor_Default_HasSensibleDefaults()
    {
        var component = new AnchorComponent();

        Assert.Equal(AnchorPreset.MiddleCenter, component.Preset);
        Assert.Equal(new Vector2(0.5f, 0.5f), component.Anchor);
        Assert.Equal(Vector2.Zero, component.Offset);
        Assert.True(component.Active);
    }

    [Fact]
    public void Constructor_WithPresetAndOffset_AppliesBoth()
    {
        var component = new AnchorComponent(AnchorPreset.TopLeft, new Vector2(10, 20));

        Assert.Equal(AnchorPreset.TopLeft, component.Preset);
        Assert.Equal(Vector2.Zero, component.Anchor);
        Assert.Equal(new Vector2(10, 20), component.Offset);
    }

    // ===== Preset / Anchor mapping =====

    [Theory]
    [InlineData(AnchorPreset.TopLeft, 0f, 0f)]
    [InlineData(AnchorPreset.TopCenter, 0.5f, 0f)]
    [InlineData(AnchorPreset.TopRight, 1f, 0f)]
    [InlineData(AnchorPreset.MiddleLeft, 0f, 0.5f)]
    [InlineData(AnchorPreset.MiddleCenter, 0.5f, 0.5f)]
    [InlineData(AnchorPreset.MiddleRight, 1f, 0.5f)]
    [InlineData(AnchorPreset.BottomLeft, 0f, 1f)]
    [InlineData(AnchorPreset.BottomCenter, 0.5f, 1f)]
    [InlineData(AnchorPreset.BottomRight, 1f, 1f)]
    public void ToVector2_MapsEveryPresetToExpectedPoint(AnchorPreset preset, float x, float y)
    {
        Assert.Equal(new Vector2(x, y), AnchorComponent.ToVector2(preset));
    }

    [Fact]
    public void SettingPreset_UpdatesAnchor()
    {
        var component = new AnchorComponent();

        component.Preset = AnchorPreset.TopRight;

        Assert.Equal(new Vector2(1f, 0f), component.Anchor);
    }

    [Fact]
    public void SettingAnchor_DoesNotChangePreset()
    {
        var component = new AnchorComponent();

        component.Anchor = new Vector2(0.25f, 0.75f);

        Assert.Equal(AnchorPreset.MiddleCenter, component.Preset);
        Assert.Equal(new Vector2(0.25f, 0.75f), component.Anchor);
    }

    // ===== ResolvePosition math =====

    [Fact]
    public void ResolvePosition_Center_NoOffset_IsScreenCenter()
    {
        var rect = new Rectangle(0, 0, 800, 600);

        var pos = AnchorComponent.ResolvePosition(new Vector2(0.5f, 0.5f), Vector2.Zero, rect);

        Assert.Equal(new Vector2(400, 300), pos);
    }

    [Fact]
    public void ResolvePosition_TopLeft_WithOffset_IsOffset()
    {
        var rect = new Rectangle(0, 0, 800, 600);

        var pos = AnchorComponent.ResolvePosition(Vector2.Zero, new Vector2(15, 25), rect);

        Assert.Equal(new Vector2(15, 25), pos);
    }

    [Fact]
    public void ResolvePosition_BottomRight_NoOffset_IsBottomRightCorner()
    {
        var rect = new Rectangle(0, 0, 800, 600);

        var pos = AnchorComponent.ResolvePosition(new Vector2(1f, 1f), Vector2.Zero, rect);

        Assert.Equal(new Vector2(800, 600), pos);
    }

    // ===== Update behavior =====

    [Fact]
    public void Update_WithScreenCanvas_DrivesOwnerPosition()
    {
        var entity = new TestEntity();
        entity.AddComponent(new CanvasComponent()); // screen-space by default
        var anchor = entity.AddComponent(new AnchorComponent(AnchorPreset.TopLeft, new Vector2(10, 20)));

        anchor.Update(new GameTime());

        Assert.Equal(new Vector2(10, 20), entity.Position);
    }

    [Fact]
    public void Update_WithAncestorScreenCanvas_DrivesOwnerPosition()
    {
        var root = new TestEntity();
        var child = new TestEntity();
        root.AddChild(child);
        root.AddComponent(new CanvasComponent());
        var anchor = child.AddComponent(new AnchorComponent(AnchorPreset.MiddleCenter, Vector2.Zero));

        anchor.Update(new GameTime());

        // Child world position is driven to screen center.
        Assert.Equal(new Vector2(400, 300), child.Position);
    }

    [Fact]
    public void Update_WithoutAnyCanvas_DoesNotMoveOwner()
    {
        var entity = new TestEntity();
        entity.Position = new Vector2(123, 456);
        var anchor = entity.AddComponent(new AnchorComponent(AnchorPreset.TopLeft, Vector2.Zero));

        anchor.Update(new GameTime());

        Assert.Equal(new Vector2(123, 456), entity.Position);
    }

    [Fact]
    public void Update_WhenInactive_DoesNotMoveOwner()
    {
        var entity = new TestEntity();
        entity.Position = new Vector2(9, 9);
        entity.AddComponent(new CanvasComponent());
        var anchor = entity.AddComponent(new AnchorComponent(AnchorPreset.TopLeft, Vector2.Zero));
        anchor.Active = false;

        anchor.Update(new GameTime());

        Assert.Equal(new Vector2(9, 9), entity.Position);
    }

    [Fact]
    public void Update_AfterAnchorChange_ReResolvesPosition()
    {
        var entity = new TestEntity();
        entity.AddComponent(new CanvasComponent());
        var anchor = entity.AddComponent(new AnchorComponent(AnchorPreset.TopLeft, Vector2.Zero));

        anchor.Update(new GameTime());
        Assert.Equal(Vector2.Zero, entity.Position);

        anchor.Anchor = new Vector2(1f, 1f);
        anchor.Update(new GameTime());

        Assert.Equal(new Vector2(800, 600), entity.Position);
    }

    // ===== World-space canvas behavior =====

    [Fact]
    public void Update_WithWorldCanvas_ResolvesAgainstCanvasSize()
    {
        var entity = new TestEntity();
        var canvasComponent = entity.AddComponent(new CanvasComponent(isScreenSpace: false));
        canvasComponent.Canvas.Width = 200;
        canvasComponent.Canvas.Height = 100;
        var anchor = entity.AddComponent(new AnchorComponent(AnchorPreset.BottomRight, Vector2.Zero));

        anchor.Update(new GameTime());

        // Bottom-right of the 200x100 world-space canvas.
        Assert.Equal(new Vector2(200, 100), entity.Position);
    }

    [Fact]
    public void Update_WithWorldCanvasChild_StaysPinnedInsideMovingPanel()
    {
        var root = new TestEntity();
        root.Position = new Vector2(500, 300); // panel lives somewhere in the world
        var canvasComponent = root.AddComponent(new CanvasComponent(isScreenSpace: false));
        canvasComponent.Canvas.Width = 200;
        canvasComponent.Canvas.Height = 100;

        var child = new TestEntity();
        root.AddChild(child);
        var anchor = child.AddComponent(new AnchorComponent(AnchorPreset.MiddleCenter, Vector2.Zero));

        anchor.Update(new GameTime());

        // Canvas-relative position is the panel center; world position follows the panel.
        Assert.Equal(new Vector2(100, 50), child.LocalPosition);
        Assert.Equal(new Vector2(600, 350), child.Position);

        // Move the panel around the world — the anchored widget must follow it.
        root.Position = new Vector2(-40, 800);
        anchor.Update(new GameTime());

        Assert.Equal(new Vector2(100, 50), child.LocalPosition);
        Assert.Equal(new Vector2(60, 850), child.Position);
    }

    [Fact]
    public void Update_WithWorldCanvasWithoutSize_FallsBackToViewport()
    {
        var entity = new TestEntity();
        entity.AddComponent(new CanvasComponent(isScreenSpace: false)); // no explicit size
        var anchor = entity.AddComponent(new AnchorComponent(AnchorPreset.TopLeft, new Vector2(10, 20)));

        anchor.Update(new GameTime());

        Assert.Equal(new Vector2(10, 20), entity.Position);
    }
}
