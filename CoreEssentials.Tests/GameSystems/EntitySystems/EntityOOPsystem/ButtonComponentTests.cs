using System;
using System.Linq;
using System.Reflection;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Components.BuiltIn;
using CoreEssentials.GUI;
using CoreEssentials.GUI.Internal;
using CoreEssentials.GUI.Types;
using Microsoft.Xna.Framework;
using Xunit;

namespace CoreEssentials.Tests.GameSystems.EntitySystems.EntityOOPsystem;

public class ButtonComponentTests : IDisposable
{
    private readonly Game _mockGame = null!;
    private bool _disposed;

    public ButtonComponentTests()
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
        var component = new ButtonComponent();

        Assert.Equal("", component.Text);
        Assert.Equal(Vector2.One, component.Scale);
        Assert.True(component.Visible);
        Assert.True(component.Enabled);
    }

    [Fact]
    public void Constructor_WithText_SetsText()
    {
        var component = new ButtonComponent("Start");

        Assert.Equal("Start", component.Text);
    }

    // ===== Attach behavior =====

    [Fact]
    public void OnAttach_AddsButtonToOwnCanvas()
    {
        var entity = new TestEntity();
        var canvas = entity.AddComponent(new CanvasComponent());
        entity.AddComponent(new ButtonComponent("Play"));

        Assert.Single(canvas.Canvas.Children);
        var button = Assert.IsAssignableFrom<IButton>(canvas.Canvas.Children[0]);
        Assert.Equal("Play", button.Text);
    }

    [Fact]
    public void OnAttach_AddsButtonToAncestorCanvas()
    {
        var root = new TestEntity();
        var child = new TestEntity();
        root.AddChild(child);
        var canvas = root.AddComponent(new CanvasComponent());

        child.AddComponent(new ButtonComponent("Menu"));

        Assert.Single(canvas.Canvas.Children);
        var button = Assert.IsAssignableFrom<IButton>(canvas.Canvas.Children[0]);
        Assert.Equal("Menu", button.Text);
    }

    [Fact]
    public void OnAttach_ApppliesScaleVisibleAndEnabledToWidget()
    {
        var entity = new TestEntity();
        var canvas = entity.AddComponent(new CanvasComponent());
        entity.AddComponent(new ButtonComponent("T")
        {
            Scale = new Vector2(0.5f, 0.5f),
            Visible = false,
            Enabled = false
        });

        var button = Assert.IsAssignableFrom<IButton>(canvas.Canvas.Children[0]);
        Assert.Equal(new Vector2(0.5f, 0.5f), button.Scale);
        Assert.False(button.Visible);
        Assert.False(button.Enabled);
    }

    [Fact]
    public void OnAttach_Throws_WhenNoCanvasInHierarchy()
    {
        var entity = new TestEntity();

        var ex = Assert.Throws<InvalidOperationException>(
            () => entity.AddComponent(new ButtonComponent("Orphan")));

        Assert.Contains("CanvasComponent", ex.Message);
    }

    // ===== Event bridging =====

    [Fact]
    public void Clicked_Fires_WhenWidgetIsClicked()
    {
        var entity = new TestEntity();
        var canvas = entity.AddComponent(new CanvasComponent());
        var component = entity.AddComponent(new ButtonComponent("Play"));

        int clickCount = 0;
        component.Clicked += () => clickCount++;

        Assert.True(TryRaiseWidgetClick(canvas.Canvas.Children[0]));

        Assert.Equal(1, clickCount);
    }

    [Fact]
    public void Clicked_FiresMultipleTimes_ForMultipleClicks()
    {
        var entity = new TestEntity();
        var canvas = entity.AddComponent(new CanvasComponent());
        var component = entity.AddComponent(new ButtonComponent("Play"));

        int clickCount = 0;
        component.Clicked += () => clickCount++;

        Assert.True(TryRaiseWidgetClick(canvas.Canvas.Children[0]));
        Assert.True(TryRaiseWidgetClick(canvas.Canvas.Children[0]));

        Assert.Equal(2, clickCount);
    }

    [Fact]
    public void OnDetach_UnsubscribesFromWidgetClicked()
    {
        var entity = new TestEntity();
        var canvas = entity.AddComponent(new CanvasComponent());
        var component = entity.AddComponent(new ButtonComponent("Play"));
        var widget = canvas.Canvas.Children[0];

        // Sanity check: while attached, the bridge is subscribed to the widget event.
        Assert.True(TryRaiseWidgetClick(widget));

        entity.RemoveComponent<ButtonComponent>();

        // The bridge unsubscribed on detach, so no handler remains on the (now orphaned) widget.
        Assert.False(TryRaiseWidgetClick(widget));
    }

    [Fact]
    public void OnDetach_RemovesButtonFromCanvas()
    {
        var entity = new TestEntity();
        var canvas = entity.AddComponent(new CanvasComponent());
        entity.AddComponent(new ButtonComponent("T"));
        Assert.NotEmpty(canvas.Canvas.Children);

        entity.RemoveComponent<ButtonComponent>();

        Assert.Empty(canvas.Canvas.Children);
    }

    // ===== Per-frame position sync =====

    [Fact]
    public void Update_KeepsButtonAtEntityPosition_RelativeToCanvas()
    {
        var root = new TestEntity();
        root.Position = new Vector2(100, 50);
        var canvas = root.AddComponent(new CanvasComponent());

        var child = new TestEntity();
        root.AddChild(child);
        child.LocalPosition = new Vector2(40, 20); // world position (140, 70)
        var button = child.AddComponent(new ButtonComponent("T"));

        button.Update(new GameTime());

        var widget = Assert.IsAssignableFrom<IButton>(canvas.Canvas.Children[0]);
        Assert.Equal(new Vector2(40, 20), widget.Position);
    }

    /// <summary>
    /// Raises the widget's own Clicked event via its backing field, simulating a user click
    /// without driving Myra's input pipeline. A plain C# event compiles to a private instance
    /// field of delegate type, so we locate it by type rather than by (compiler-generated) name.
    /// Returns false when no handler is subscribed (e.g. after the component unsubscribed on detach).
    /// </summary>
    private static bool TryRaiseWidgetClick(IWidget widget)
    {
        var field = widget.GetType()
            .GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance)
            .FirstOrDefault(f => f.FieldType == typeof(Action<IButton>));

        if (field == null) return false;

        var handler = (Action<IButton>?)field.GetValue(widget);
        if (handler == null) return false;

        handler.Invoke((IButton)widget);
        return true;
    }
}
