using System;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Components.BuiltIn;
using CoreEssentials.GUI;
using CoreEssentials.GUI.Factory;
using CoreEssentials.GUI.Internal;
using Microsoft.Xna.Framework;
using Xunit;

namespace CoreEssentials.Tests.GameSystems.EntitySystems.EntityOOPsystem;

public class CanvasComponentTests : IDisposable
{
    private readonly Game _mockGame = null!;
    private bool _disposed;

    public CanvasComponentTests()
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
    public void Constructor_Default_CreatesScreenSpaceCanvas()
    {
        var component = new CanvasComponent();

        Assert.NotNull(component.Canvas);
        Assert.True(component.IsScreenSpace);
    }

    [Fact]
    public void Constructor_WorldSpace_CreatesWorldSpaceCanvas()
    {
        var component = new CanvasComponent(isScreenSpace: false);

        Assert.NotNull(component.Canvas);
        Assert.False(component.IsScreenSpace);
    }

    /// <summary>
    /// Regression test for the prefab/scene instantiation path. The loader creates components via
    /// Activator.CreateInstance(type) — a TRUE parameterless constructor is required; an
    /// optional-parameter-only constructor (e.g. "CanvasComponent(bool isScreenSpace = true)") does
    /// NOT count and made the loader silently skip creating every canvas, so nested labels failed
    /// with "No CanvasComponent found". This mirrors that exact reflection call.
    /// </summary>
    [Fact]
    public void Constructor_ViaReflection_UsesTrueParameterlessCtor()
    {
        var instance = Activator.CreateInstance(typeof(CanvasComponent));

        Assert.NotNull(instance);
        var component = (CanvasComponent)instance!;
        Assert.True(component.IsScreenSpace);
    }

    // ===== Widget management =====

    [Fact]
    public void AddWidget_AddsToCanvasChildren()
    {
        var component = new CanvasComponent();
        var label = WidgetFactory.CreateLabel("Test");

        component.AddWidget(label);

        Assert.Contains(label, component.Canvas.Children);
    }

    [Fact]
    public void RemoveWidget_RemovesFromCanvasChildren()
    {
        var component = new CanvasComponent();
        var label = WidgetFactory.CreateLabel("Test");
        component.AddWidget(label);

        component.RemoveWidget(label);

        Assert.DoesNotContain(label, component.Canvas.Children);
    }

    // ===== Lifecycle =====

    [Fact]
    public void Update_SyncsCanvasPositionFromOwner()
    {
        var entity = new TestEntity();
        var component = entity.AddComponent(new CanvasComponent());
        entity.Position = new Vector2(100, 50);

        component.Update(new GameTime());

        Assert.Equal(new Vector2(100, 50), component.Canvas.Position);
    }

    [Fact]
    public void Update_FollowsEntityPositionChanges()
    {
        var entity = new TestEntity();
        var component = entity.AddComponent(new CanvasComponent());

        entity.Position = new Vector2(10, 20);
        component.Update(new GameTime());
        entity.Position = new Vector2(300, 400);
        component.Update(new GameTime());

        Assert.Equal(new Vector2(300, 400), component.Canvas.Position);
    }

    [Fact]
    public void OnDetach_CleansUpCanvasWidgets()
    {
        var entity = new TestEntity();
        var component = entity.AddComponent(new CanvasComponent());
        component.AddWidget(WidgetFactory.CreateLabel("Test"));
        Assert.NotEmpty(component.Canvas.Children);

        entity.RemoveComponent<CanvasComponent>();

        Assert.Empty(component.Canvas.Children);
    }

    // ===== Hierarchy lookup (FindCanvas / RequireCanvas) =====

    [Fact]
    public void FindCanvas_ReturnsOwnComponent_WhenPresent()
    {
        var entity = new TestEntity();
        var component = entity.AddComponent(new CanvasComponent());

        Assert.Same(component, CanvasComponent.FindCanvas(entity));
    }

    [Fact]
    public void FindCanvas_ReturnsNearestAncestor_WhenNotOnEntity()
    {
        var root = new TestEntity();
        var child = new TestEntity();
        root.AddChild(child);
        var canvas = root.AddComponent(new CanvasComponent());

        Assert.Same(canvas, CanvasComponent.FindCanvas(child));
    }

    [Fact]
    public void FindCanvas_ReturnsNearest_WhenMultipleInHierarchy()
    {
        var root = new TestEntity();
        var child = new TestEntity();
        root.AddChild(child);
        root.AddComponent(new CanvasComponent());
        var childCanvas = child.AddComponent(new CanvasComponent());

        // The child's own canvas wins over the parent's.
        Assert.Same(childCanvas, CanvasComponent.FindCanvas(child));
    }

    [Fact]
    public void FindCanvas_ReturnsNull_WhenNoCanvasInHierarchy()
    {
        var entity = new TestEntity();

        Assert.Null(CanvasComponent.FindCanvas(entity));
    }

    [Fact]
    public void RequireCanvas_Throws_WhenNoCanvasInHierarchy()
    {
        var entity = new TestEntity();

        var ex = Assert.Throws<InvalidOperationException>(() => CanvasComponent.RequireCanvas(entity));
        Assert.Contains("CanvasComponent", ex.Message);
    }

    [Fact]
    public void RequireCanvas_ReturnsCanvas_WhenPresentOnAncestor()
    {
        var root = new TestEntity();
        var child = new TestEntity();
        root.AddChild(child);
        var canvas = root.AddComponent(new CanvasComponent());

        Assert.Same(canvas, CanvasComponent.RequireCanvas(child));
    }
}
