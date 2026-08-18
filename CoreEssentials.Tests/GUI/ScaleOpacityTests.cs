using System;
using CoreEssentials.GUI;
using CoreEssentials.GUI.Factory;
using CoreEssentials.GUI.Types;
using Microsoft.Xna.Framework;
using Xunit;

namespace CoreEssentials.Tests.GUI;

/// <summary>
/// Tests for Scale and Opacity properties on IWidget interface.
/// These tests verify Issue #29: Add Scale and Opacity to ILabel/IWidget interface.
/// </summary>
public class ScaleOpacityTests : IDisposable
{
    private readonly Game1 _mockGame;
    private bool _disposed = false;

    public ScaleOpacityTests()
    {
        _mockGame = new Game1();
        GUIManager.Init(_mockGame, 800, 600);
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _mockGame?.Dispose();
            var engine = CoreEssentials.GUI.Internal.EngineResolver.GetEngine();
            engine.Shutdown();
            _disposed = true;
        }
    }
    /// <summary>
    /// Verifies that Scale defaults to Vector2.One (1, 1) — no scaling.
    /// </summary>
    [Fact]
    public void Label_Scale_DefaultIsOne()
    {
        // Arrange & Act
        ILabel label = WidgetFactory.CreateLabel("Test");

        // Assert
        Assert.Equal(Vector2.One, label.Scale);
    }

    /// <summary>
    /// Verifies that Opacity defaults to 1.0 — fully opaque.
    /// </summary>
    [Fact]
    public void Label_Opacity_DefaultIsOne()
    {
        // Arrange & Act
        ILabel label = WidgetFactory.CreateLabel("Test");

        // Assert
        Assert.Equal(1.0f, label.Opacity);
    }

    /// <summary>
    /// Verifies that Scale can be set and retrieved with custom values.
    /// </summary>
    [Fact]
    public void Label_Scale_SetAndGet_ReturnsCorrectValues()
    {
        // Arrange
        ILabel label = WidgetFactory.CreateLabel("Test");

        // Act
        Vector2 customScale = new(2.0f, 1.5f);
        label.Scale = customScale;

        // Assert
        Assert.Equal(customScale.X, label.Scale.X);
        Assert.Equal(customScale.Y, label.Scale.Y);
    }

    /// <summary>
    /// Verifies that Opacity can be set and retrieved with values in 0.0–1.0 range.
    /// </summary>
    [Fact]
    public void Label_Opacity_SetAndGet_ReturnsCorrectValues()
    {
        // Arrange
        ILabel label = WidgetFactory.CreateLabel("Test");

        // Act
        float customOpacity = 0.5f;
        label.Opacity = customOpacity;

        // Assert
        Assert.Equal(customOpacity, label.Opacity);
    }

    /// <summary>
    /// Verifies Scale works on buttons (tests that IButton : IWidget inheritance works).
    /// </summary>
    [Fact]
    public void Button_Scale_SetAndGet_ReturnsCorrectValues()
    {
        // Arrange & Act
        IButton button = WidgetFactory.CreateTextButton("Click me");
        Vector2 scale = new(0.8f, 1.2f);
        button.Scale = scale;

        // Assert
        Assert.Equal(scale.X, button.Scale.X);
        Assert.Equal(scale.Y, button.Scale.Y);
    }

    /// <summary>
    /// Verifies Opacity works on buttons (tests that IButton : IWidget inheritance works).
    /// </summary>
    [Fact]
    public void Button_Opacity_SetAndGet_ReturnsCorrectValues()
    {
        // Arrange & Act
        IButton button = WidgetFactory.CreateTextButton("Click me");
        float opacity = 0.75f;
        button.Opacity = opacity;

        // Assert
        Assert.Equal(opacity, button.Opacity);
    }

    /// <summary>
    /// Verifies Scale works on panels (tests that IPanel : IContainer : IWidget inheritance works).
    /// </summary>
    [Fact]
    public void Panel_Scale_SetAndGet_ReturnsCorrectValues()
    {
        // Arrange & Act
        IPanel panel = WidgetFactory.CreatePanel();
        Vector2 scale = new(1.5f, 1.5f);
        panel.Scale = scale;

        // Assert
        Assert.Equal(scale.X, panel.Scale.X);
        Assert.Equal(scale.Y, panel.Scale.Y);
    }

    /// <summary>
    /// Verifies Opacity works on panels (tests that IPanel : IContainer : IWidget inheritance works).
    /// </summary>
    [Fact]
    public void Panel_Opacity_SetAndGet_ReturnsCorrectValues()
    {
        // Arrange & Act
        IPanel panel = WidgetFactory.CreatePanel();
        float opacity = 0.3f;
        panel.Opacity = opacity;

        // Assert
        Assert.Equal(opacity, panel.Opacity);
    }

    /// <summary>
    /// Verifies that Scale can be set to zero (widget collapses).
    /// </summary>
    [Fact]
    public void Widget_Scale_SetToZero_DoesNotThrow()
    {
        // Arrange & Act
        ILabel label = WidgetFactory.CreateLabel("Test");
        Vector2 zeroScale = Vector2.Zero;

        // Should not throw
        label.Scale = zeroScale;
        Assert.Equal(Vector2.Zero, label.Scale);
    }

    /// <summary>
    /// Verifies that Opacity can be set to 0.0 (fully transparent).
    /// </summary>
    [Fact]
    public void Widget_Opacity_SetToZero_IsFullyTransparent()
    {
        // Arrange & Act
        ILabel label = WidgetFactory.CreateLabel("Test");
        label.Opacity = 0.0f;

        // Assert
        Assert.Equal(0.0f, label.Opacity);
    }

    /// <summary>
    /// Verifies that Opacity can be set to 1.0 (fully opaque).
    /// </summary>
    [Fact]
    public void Widget_Opacity_SetToOne_IsFullyOpaque()
    {
        // Arrange & Act
        ILabel label = WidgetFactory.CreateLabel("Test");
        label.Opacity = 1.0f;

        // Assert
        Assert.Equal(1.0f, label.Opacity);
    }
}
