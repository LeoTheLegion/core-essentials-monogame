using System;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Components.BuiltIn;
using CoreEssentials.GUI;
using CoreEssentials.GUI.Internal;
using CoreEssentials.GUI.Types;
using Microsoft.Xna.Framework;
using Xunit;

namespace CoreEssentials.Tests.GameSystems.EntitySystems.EntityOOPsystem;

public class LabelComponentTests : IDisposable
{
    private readonly Game _mockGame = null!;
    private bool _disposed;

    public LabelComponentTests()
    {
        // Create a real Game instance and initialize the GUI engine (MyraEnvironment setup).
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
        var component = new LabelComponent();

        Assert.Equal("", component.Text);
        Assert.Equal(Color.White, component.TextColor);
        Assert.Equal(Vector2.One, component.Scale);
        Assert.True(component.Visible);
        Assert.Equal(1.0f, component.Opacity);
    }

    [Fact]
    public void Constructor_WithText_SetsText()
    {
        var component = new LabelComponent("Hello");

        Assert.Equal("Hello", component.Text);
    }

    // ===== Attach behavior =====

    [Fact]
    public void OnAttach_AddsLabelToOwnCanvas()
    {
        var entity = new TestEntity();
        var canvas = entity.AddComponent(new CanvasComponent());
        entity.AddComponent(new LabelComponent("Score: 0"));

        Assert.Single(canvas.Canvas.Children);
        var label = Assert.IsAssignableFrom<ILabel>(canvas.Canvas.Children[0]);
        Assert.Equal("Score: 0", label.Text);
    }

    [Fact]
    public void OnAttach_AddsLabelToAncestorCanvas()
    {
        var root = new TestEntity();
        var child = new TestEntity();
        root.AddChild(child);
        var canvas = root.AddComponent(new CanvasComponent());

        child.AddComponent(new LabelComponent("Child label"));

        Assert.Single(canvas.Canvas.Children);
        var label = Assert.IsAssignableFrom<ILabel>(canvas.Canvas.Children[0]);
        Assert.Equal("Child label", label.Text);
    }

    [Fact]
    public void OnAttach_ApppliesTextColorToWidget()
    {
        var entity = new TestEntity();
        var canvas = entity.AddComponent(new CanvasComponent());
        entity.AddComponent(new LabelComponent("T") { TextColor = Color.Red });

        var label = Assert.IsAssignableFrom<ILabel>(canvas.Canvas.Children[0]);
        Assert.Equal(Color.Red, label.TextColor);
    }

    [Fact]
    public void OnAttach_ApppliesScaleOpacityAndVisibleToWidget()
    {
        var entity = new TestEntity();
        var canvas = entity.AddComponent(new CanvasComponent());
        entity.AddComponent(new LabelComponent("T")
        {
            Scale = new Vector2(2, 2),
            Opacity = 0.5f,
            Visible = false
        });

        var label = Assert.IsAssignableFrom<ILabel>(canvas.Canvas.Children[0]);
        Assert.Equal(new Vector2(2, 2), label.Scale);
        Assert.Equal(0.5f, label.Opacity);
        Assert.False(label.Visible);
    }

    [Fact]
    public void OnAttach_Throws_WhenNoCanvasInHierarchy()
    {
        var entity = new TestEntity();

        var ex = Assert.Throws<InvalidOperationException>(
            () => entity.AddComponent(new LabelComponent("Orphan")));

        Assert.Contains("CanvasComponent", ex.Message);
    }

    // ===== Per-frame position sync =====

    [Fact]
    public void Update_KeepsLabelAtEntityPosition_RelativeToCanvas()
    {
        var root = new TestEntity();
        root.Position = new Vector2(100, 50);
        root.AddComponent(new CanvasComponent());

        var child = new TestEntity();
        root.AddChild(child);
        child.LocalPosition = new Vector2(20, 10); // world position (120, 60)
        var label = child.AddComponent(new LabelComponent("T"));

        label.Update(new GameTime());

        var widget = Assert.IsAssignableFrom<ILabel>(root.GetComponent<CanvasComponent>()!.Canvas.Children[0]);
        Assert.Equal(new Vector2(20, 10), widget.Position);
    }

    [Fact]
    public void Update_FollowsEntityMovement()
    {
        var root = new TestEntity();
        var canvas = root.AddComponent(new CanvasComponent());

        var child = new TestEntity();
        root.AddChild(child);
        var label = child.AddComponent(new LabelComponent("T"));

        child.LocalPosition = Vector2.Zero;
        label.Update(new GameTime());
        Assert.Equal(Vector2.Zero, GetWidget(canvas).Position);

        // Move the child entity; the widget should follow.
        child.LocalPosition = new Vector2(30, -15);
        label.Update(new GameTime());
        Assert.Equal(new Vector2(30, -15), GetWidget(canvas).Position);
    }

    // ===== Detach behavior =====

    [Fact]
    public void OnDetach_RemovesLabelFromCanvas()
    {
        var entity = new TestEntity();
        var canvas = entity.AddComponent(new CanvasComponent());
        entity.AddComponent(new LabelComponent("T"));
        Assert.NotEmpty(canvas.Canvas.Children);

        entity.RemoveComponent<LabelComponent>();

        Assert.Empty(canvas.Canvas.Children);
    }

    private static IWidget GetWidget(CanvasComponent canvas) => canvas.Canvas.Children[0];
}
