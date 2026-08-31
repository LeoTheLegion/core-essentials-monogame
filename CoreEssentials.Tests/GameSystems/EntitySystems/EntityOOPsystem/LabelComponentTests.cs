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
    public void OnAttach_PinsTransformOriginToTopLeft()
    {
        var entity = new TestEntity();
        var canvas = entity.AddComponent(new CanvasComponent());
        entity.AddComponent(new LabelComponent("T") { Scale = new Vector2(2, 2) });

        var label = Assert.IsAssignableFrom<ILabel>(canvas.Canvas.Children[0]);

        // The alignment math assumes top-left scaling; Myra's default origin is center,
        // so the component must pin it before applying scale.
        Assert.Equal(Vector2.Zero, label.TransformOrigin);
    }

    [Fact]
    public void OnAttach_PinsTransformOriginToTopLeft_EvenAtDefaultScale()
    {
        var entity = new TestEntity();
        var canvas = entity.AddComponent(new CanvasComponent());
        entity.AddComponent(new LabelComponent("T"));

        var label = Assert.IsAssignableFrom<ILabel>(canvas.Canvas.Children[0]);

        Assert.Equal(Vector2.Zero, label.TransformOrigin);
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

    // ===== Live property updates after attach (#68) =====

    [Fact]
    public void SetText_AfterAttach_UpdatesWidget()
    {
        var entity = new TestEntity();
        var canvas = entity.AddComponent(new CanvasComponent());
        var label = entity.AddComponent(new LabelComponent("Score: 0"));

        label.Text = "Score: 42";

        var widget = Assert.IsAssignableFrom<ILabel>(canvas.Canvas.Children[0]);
        Assert.Equal("Score: 42", widget.Text);
    }

    [Fact]
    public void SetTextColor_AfterAttach_UpdatesWidget()
    {
        var entity = new TestEntity();
        var canvas = entity.AddComponent(new CanvasComponent());
        var label = entity.AddComponent(new LabelComponent("T"));

        label.TextColor = Color.LimeGreen;

        var widget = Assert.IsAssignableFrom<ILabel>(canvas.Canvas.Children[0]);
        Assert.Equal(Color.LimeGreen, widget.TextColor);
    }

    [Fact]
    public void SetScaleOpacityAndVisible_AfterAttach_UpdateWidget()
    {
        var entity = new TestEntity();
        var canvas = entity.AddComponent(new CanvasComponent());
        var label = entity.AddComponent(new LabelComponent("T"));

        label.Scale = new Vector2(2, 1);
        label.Opacity = 0.25f;
        label.Visible = false;

        var widget = Assert.IsAssignableFrom<ILabel>(canvas.Canvas.Children[0]);
        Assert.Equal(new Vector2(2, 1), widget.Scale);
        Assert.Equal(0.25f, widget.Opacity);
        Assert.False(widget.Visible);
    }

    [Fact]
    public void SetProperties_BeforeAttach_AppliedOnAttach()
    {
        var entity = new TestEntity();
        var canvas = entity.AddComponent(new CanvasComponent());
        var label = new LabelComponent("T")
        {
            TextColor = Color.Red,
            Scale = new Vector2(3, 3),
            Opacity = 0.5f,
            Visible = false
        };
        entity.AddComponent(label);

        var widget = Assert.IsAssignableFrom<ILabel>(canvas.Canvas.Children[0]);
        Assert.Equal(Color.Red, widget.TextColor);
        Assert.Equal(new Vector2(3, 3), widget.Scale);
        Assert.Equal(0.5f, widget.Opacity);
        Assert.False(widget.Visible);
    }

    [Fact]
    public void Reattach_AfterDetach_ReappliesCurrentPropertyValues()
    {
        var entity = new TestEntity();
        var canvas = entity.AddComponent(new CanvasComponent());
        var label = entity.AddComponent(new LabelComponent("T"));
        label.TextColor = Color.Blue;

        entity.RemoveComponent<LabelComponent>();
        Assert.Empty(canvas.Canvas.Children);

        // Re-attach the same component instance: widget must reflect values set while detached too.
        entity.AddComponent(label);

        var widget = Assert.IsAssignableFrom<ILabel>(canvas.Canvas.Children[0]);
        Assert.Equal(Color.Blue, widget.TextColor);
    }

    // ===== Alignment (container semantics: the canvas positions the label) =====

    [Fact]
    public void Constructor_Default_AlignmentIsLeftTop()
    {
        var component = new LabelComponent();

        Assert.Equal(HorizontalAlignment.Left, component.HorizontalAlignment);
        Assert.Equal(VerticalAlignment.Top, component.VerticalAlignment);
    }

    [Fact]
    public void Update_HorizontalCenter_CentersInCanvas()
    {
        var entity = new TestEntity();
        var canvas = entity.AddComponent(new CanvasComponent());
        var label = entity.AddComponent(new LabelComponent("T"));
        label.HorizontalAlignment = HorizontalAlignment.Center;

        label.Update(new GameTime());

        var widget = GetWidget(canvas);
        // The screen-space canvas reports the GUI viewport (800x600 from Init). Myra stores
        // Left/Top as int, so the result is truncated on assignment.
        Assert.Equal((float)MathF.Truncate(canvas.Canvas.Width * 0.5f - widget.Width * 0.5f), widget.Position.X);
        Assert.Equal(0f, widget.Position.Y);
    }

    [Fact]
    public void Update_VerticalCenter_CentersInCanvas()
    {
        var entity = new TestEntity();
        var canvas = entity.AddComponent(new CanvasComponent());
        var label = entity.AddComponent(new LabelComponent("T"));
        label.VerticalAlignment = VerticalAlignment.Center;

        label.Update(new GameTime());

        var widget = GetWidget(canvas);
        Assert.Equal(0f, widget.Position.X);
        Assert.Equal((float)MathF.Truncate(canvas.Canvas.Height * 0.5f - widget.Height * 0.5f), widget.Position.Y);
    }

    [Fact]
    public void Update_RightBottom_EdgesOnCanvasEdges()
    {
        var entity = new TestEntity();
        var canvas = entity.AddComponent(new CanvasComponent());
        var label = entity.AddComponent(new LabelComponent("T"));
        label.HorizontalAlignment = HorizontalAlignment.Right;
        label.VerticalAlignment = VerticalAlignment.Bottom;

        label.Update(new GameTime());

        var widget = GetWidget(canvas);
        Assert.Equal((float)MathF.Truncate(canvas.Canvas.Width - widget.Width), widget.Position.X);
        Assert.Equal((float)MathF.Truncate(canvas.Canvas.Height - widget.Height), widget.Position.Y);
    }

    [Fact]
    public void Update_Centering_IsScaleAware()
    {
        var entity = new TestEntity();
        var canvas = entity.AddComponent(new CanvasComponent());
        var label = entity.AddComponent(new LabelComponent("T"));
        label.Scale = new Vector2(2, 4);
        label.HorizontalAlignment = HorizontalAlignment.Center;
        label.VerticalAlignment = VerticalAlignment.Center;

        label.Update(new GameTime());

        var widget = GetWidget(canvas);
        Assert.Equal((float)MathF.Truncate(canvas.Canvas.Width * 0.5f - widget.Width * 2f * 0.5f), widget.Position.X);
        Assert.Equal((float)MathF.Truncate(canvas.Canvas.Height * 0.5f - widget.Height * 4f * 0.5f), widget.Position.Y);
    }

    [Fact]
    public void Update_DefaultAlignment_TopsLeftCornerOfCanvas()
    {
        var entity = new TestEntity();
        var canvas = entity.AddComponent(new CanvasComponent());
        var label = entity.AddComponent(new LabelComponent("T"));

        label.Update(new GameTime());

        Assert.Equal(Vector2.Zero, GetWidget(canvas).Position);
    }

    [Fact]
    public void Update_EntityOffsetActsAsMarginFromAlignedReference()
    {
        var root = new TestEntity();
        root.Position = new Vector2(100, 50);
        var canvas = root.AddComponent(new CanvasComponent());

        var child = new TestEntity();
        root.AddChild(child);
        child.LocalPosition = new Vector2(40, 20); // margin (40, 20) relative to the canvas entity
        var label = child.AddComponent(new LabelComponent("T"));
        label.HorizontalAlignment = HorizontalAlignment.Right;

        label.Update(new GameTime());

        var widget = GetWidget(canvas);
        Assert.Equal((float)MathF.Truncate(40f + canvas.Canvas.Width - widget.Width), widget.Position.X);
        Assert.Equal(20f, widget.Position.Y);
    }

    private static IWidget GetWidget(CanvasComponent canvas) => canvas.Canvas.Children[0];
}
