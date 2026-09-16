using System;
using System.Linq;
using System.Reflection;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Components.BuiltIn;
using CoreEssentials.GUI;
using CoreEssentials.GUI.Engines.Myra.Widgets;
using CoreEssentials.GUI.Internal;
using CoreEssentials.GUI.Types;
using Microsoft.Xna.Framework;
using MyraButton = Myra.Graphics2D.UI.Button;
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
    public void OnAttach_PinsTransformOriginToTopLeft()
    {
        var entity = new TestEntity();
        var canvas = entity.AddComponent(new CanvasComponent());
        entity.AddComponent(new ButtonComponent("T") { Scale = new Vector2(3, 1) });

        var button = Assert.IsAssignableFrom<IButton>(canvas.Canvas.Children[0]);

        // The alignment math assumes top-left scaling; Myra's default origin is center,
        // so the component must pin it before applying scale.
        Assert.Equal(Vector2.Zero, button.TransformOrigin);
    }

    [Fact]
    public void OnAttach_PinsTransformOriginToTopLeft_EvenAtDefaultScale()
    {
        var entity = new TestEntity();
        var canvas = entity.AddComponent(new CanvasComponent());
        entity.AddComponent(new ButtonComponent("T"));

        var button = Assert.IsAssignableFrom<IButton>(canvas.Canvas.Children[0]);

        Assert.Equal(Vector2.Zero, button.TransformOrigin);
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

    // ===== Alignment =====

    [Fact]
    public void Constructor_Default_AlignmentIsLeftTop()
    {
        var component = new ButtonComponent();

        Assert.Equal(HorizontalAlignment.Left, component.HorizontalAlignment);
        Assert.Equal(VerticalAlignment.Top, component.VerticalAlignment);
    }

    [Fact]
    public void Update_CenterCenter_CentersInCanvas()
    {
        var entity = new TestEntity();
        var canvas = entity.AddComponent(new CanvasComponent());
        var button = entity.AddComponent(new ButtonComponent("T"));
        button.HorizontalAlignment = HorizontalAlignment.Center;
        button.VerticalAlignment = VerticalAlignment.Center;

        button.Update(new GameTime());

        var widget = Assert.IsAssignableFrom<IButton>(canvas.Canvas.Children[0]);
        // The screen-space canvas reports the GUI viewport (800x600 from Init). Myra stores
        // Left/Top as int, so the result is truncated on assignment.
        Assert.Equal((float)MathF.Truncate(canvas.Canvas.Width * 0.5f - widget.Width * 0.5f), widget.Position.X);
        Assert.Equal((float)MathF.Truncate(canvas.Canvas.Height * 0.5f - widget.Height * 0.5f), widget.Position.Y);
    }

    [Fact]
    public void Update_Centering_IsScaleAware()
    {
        var entity = new TestEntity();
        var canvas = entity.AddComponent(new CanvasComponent());
        var button = entity.AddComponent(new ButtonComponent("T"));
        button.Scale = new Vector2(3, 1);
        button.HorizontalAlignment = HorizontalAlignment.Right;

        button.Update(new GameTime());

        var widget = Assert.IsAssignableFrom<IButton>(canvas.Canvas.Children[0]);
        Assert.Equal((float)MathF.Truncate(canvas.Canvas.Width - widget.Width * 3f), widget.Position.X);
        Assert.Equal(0f, widget.Position.Y);
    }

    // ===== Background tint (transparent by default) =====

    [Fact]
    public void OnAttach_NoTint_ClearsAllMyraBackgroundBrushes()
    {
        var entity = new TestEntity();
        var canvas = entity.AddComponent(new CanvasComponent());
        entity.AddComponent(new ButtonComponent("T"));

        var myra = GetMyraButton(canvas.Canvas.Children[0]);
        // Default is transparent: no opaque box in any visual state.
        Assert.Null(myra.Background);
        Assert.Null(myra.OverBackground);
        Assert.Null(myra.PressedBackground);
        Assert.Null(myra.DisabledBackground);
        Assert.Null(myra.FocusedBackground);
    }

    [Fact]
    public void OnAttach_WithTint_AppliesSolidBrushToAllStates()
    {
        var entity = new TestEntity();
        var canvas = entity.AddComponent(new CanvasComponent());
        entity.AddComponent(new ButtonComponent("T") { BackgroundTint = Color.CornflowerBlue });

        var myra = GetMyraButton(canvas.Canvas.Children[0]);
        Assert.NotNull(myra.Background);
        Assert.Same(myra.Background, myra.OverBackground);
        Assert.Same(myra.Background, myra.PressedBackground);
        Assert.Equal(Color.CornflowerBlue, ((Myra.Graphics2D.Brushes.SolidBrush)myra.Background!).Color);
    }

    [Fact]
    public void BackgroundTint_LiveUpdate_RewritesBrushes()
    {
        var entity = new TestEntity();
        var canvas = entity.AddComponent(new CanvasComponent());
        var component = entity.AddComponent(new ButtonComponent("T"));

        // Start transparent, then set a tint live.
        component.BackgroundTint = Color.Tomato;
        var myra = GetMyraButton(canvas.Canvas.Children[0]);
        Assert.NotNull(myra.Background);

        // Clearing the tint restores the transparent default.
        component.BackgroundTint = null;
        Assert.Null(myra.Background);
    }

    [Fact]
    public void BackgroundTint_Default_IsNull()
    {
        var component = new ButtonComponent();
        Assert.Null(component.BackgroundTint);
    }

    // ===== Font (TTF asset name via resolve seam) =====

    private class RecordingButtonFont : ButtonComponent
    {
        public string? LastAsset;
        public int LastSize;
        // Returns null so no real SpriteFontBase (and graphics device) is needed; the test asserts
        // on the recorded resolution arguments rather than the widget's font.
        protected override object? ResolveFont(string assetName, int size)
        {
            LastAsset = assetName;
            LastSize = size;
            return null;
        }
    }

    [Fact]
    public void FontAsset_OnAttach_ResolvesWithConfiguredNameAndSize()
    {
        var entity = new TestEntity();
        var canvas = entity.AddComponent(new CanvasComponent());
        var component = entity.AddComponent(new RecordingButtonFont
        {
            FontAsset = "Fonts/display.ttf",
            FontSize = 32
        });

        Assert.Equal("Fonts/display.ttf", component.LastAsset);
        Assert.Equal(32, component.LastSize);
    }

    [Fact]
    public void FontAsset_Empty_DoesNotResolve()
    {
        var entity = new TestEntity();
        var canvas = entity.AddComponent(new CanvasComponent());
        var component = entity.AddComponent(new RecordingButtonFont());

        Assert.Null(component.LastAsset);
    }

    [Fact]
    public void FontSize_Default_Is20()
    {
        var component = new ButtonComponent();
        Assert.Equal(20, component.FontSize);
    }

    /// <summary>Unwraps a CE widget to its underlying Myra button for direct brush inspection.</summary>
    private static MyraButton GetMyraButton(IWidget widget) => (MyraButton)WidgetWrapper.Unwrap(widget);

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
