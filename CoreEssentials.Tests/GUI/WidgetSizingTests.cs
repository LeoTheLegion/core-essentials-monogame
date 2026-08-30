using System;
using CoreEssentials.GUI;
using CoreEssentials.GUI.Factory;
using CoreEssentials.GUI.Types;
using Microsoft.Xna.Framework;
using Xunit;

namespace CoreEssentials.Tests.GUI;

/// <summary>
/// Tests for AutoWidth/AutoHeight and the Width/Height semantics on IWidget:
/// auto-sized widgets report their content-measured size, explicit sizes are
/// pinned, and toggling auto off pins to the measured size (no visual jump).
/// </summary>
public class WidgetSizingTests : IDisposable
{
    private readonly Game _mockGame = null!;
    private bool _disposed;

    public WidgetSizingTests()
    {
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

    // ===== Defaults =====

    [Fact]
    public void Label_AutoSize_DefaultsToTrue()
    {
        var label = WidgetFactory.CreateLabel("Hello");

        Assert.True(label.AutoWidth);
        Assert.True(label.AutoHeight);
    }

    // ===== Measured size when auto =====

    [Fact]
    public void Label_Width_WhenAuto_ReturnsMeasuredSizeGreaterThanZero()
    {
        var label = WidgetFactory.CreateLabel("Hello World");

        Assert.True(label.Width > 0);
    }

    [Fact]
    public void Label_Height_WhenAuto_ReturnsMeasuredSizeGreaterThanZero()
    {
        var label = WidgetFactory.CreateLabel("Hello World");

        Assert.True(label.Height > 0);
    }

    [Fact]
    public void Label_Width_WhenAuto_GrowsWithTextLength()
    {
        var shortLabel = WidgetFactory.CreateLabel("Hi");
        var longLabel = WidgetFactory.CreateLabel("A considerably longer piece of text");

        Assert.True(longLabel.Width > shortLabel.Width);
    }

    // ===== Setting while auto is a no-op =====

    [Fact]
    public void Label_SetWidth_WhileAuto_IsNoOp()
    {
        var label = WidgetFactory.CreateLabel("Hello World");
        float measured = label.Width;

        label.Width = 5f;

        Assert.True(label.AutoWidth);
        Assert.Equal(measured, label.Width);
    }

    [Fact]
    public void Label_SetHeight_WhileAuto_IsNoOp()
    {
        var label = WidgetFactory.CreateLabel("Hello World");
        float measured = label.Height;

        label.Height = 5f;

        Assert.True(label.AutoHeight);
        Assert.Equal(measured, label.Height);
    }

    // ===== Pinning via AutoWidth/AutoHeight toggle =====

    [Fact]
    public void Label_AutoWidthFalse_PinsToMeasuredSize()
    {
        var label = WidgetFactory.CreateLabel("Hello World");
        float measured = label.Width;

        label.AutoWidth = false;

        Assert.False(label.AutoWidth);
        Assert.Equal(measured, label.Width);
    }

    [Fact]
    public void Label_AutoHeightFalse_PinsToMeasuredSize()
    {
        var label = WidgetFactory.CreateLabel("Hello World");
        float measured = label.Height;

        label.AutoHeight = false;

        Assert.False(label.AutoHeight);
        Assert.Equal(measured, label.Height);
    }

    [Fact]
    public void Label_SetWidth_AfterPinning_SetsExplicitSize()
    {
        var label = WidgetFactory.CreateLabel("Hello World");
        label.AutoWidth = false;

        label.Width = 123f;

        Assert.Equal(123f, label.Width);
    }

    [Fact]
    public void Label_SetHeight_AfterPinning_SetsExplicitSize()
    {
        var label = WidgetFactory.CreateLabel("Hello World");
        label.AutoHeight = false;

        label.Height = 45f;

        Assert.Equal(45f, label.Height);
    }

    [Fact]
    public void Label_AutoWidthTrue_RestoresAutoSizing()
    {
        var label = WidgetFactory.CreateLabel("Hello World");
        float measured = label.Width;
        label.AutoWidth = false;
        label.Width = 10f;
        Assert.Equal(10f, label.Width);

        label.AutoWidth = true;

        Assert.True(label.AutoWidth);
        Assert.Equal(measured, label.Width);
    }

    // ===== Buttons behave the same =====

    [Fact]
    public void Button_AutoSize_DefaultsToTrue_AndReportsMeasuredSize()
    {
        var button = WidgetFactory.CreateTextButton("Click Me");

        Assert.True(button.AutoWidth);
        Assert.True(button.AutoHeight);
        Assert.True(button.Width > 0);
        Assert.True(button.Height > 0);
    }

    // ===== Panels (explicit-size widgets) =====

    [Fact]
    public void Panel_PinnedSize_SetAndGet_RoundTrips()
    {
        var panel = WidgetFactory.CreatePanel();
        panel.AutoWidth = false;
        panel.AutoHeight = false;

        panel.Width = 300f;
        panel.Height = 200f;

        Assert.Equal(300f, panel.Width);
        Assert.Equal(200f, panel.Height);
    }

    // ===== Screen-space canvases report the viewport size while auto =====

    [Fact]
    public void Canvas_ScreenSpace_WhenAuto_ReportsViewportSize()
    {
        var canvas = new Canvas(isScreenSpace: true);

        Assert.True(canvas.AutoWidth);
        Assert.True(canvas.AutoHeight);
        Assert.Equal(GUIManager.Width, canvas.Width);
        Assert.Equal(GUIManager.Height, canvas.Height);
    }

    [Fact]
    public void Canvas_WorldSpace_WhenAuto_ReportsMeasuredSizeNotViewport()
    {
        var canvas = new Canvas(isScreenSpace: false);

        Assert.True(canvas.AutoWidth);
        Assert.True(canvas.AutoHeight);
        // An empty world-space panel has no content, so its measured size is 0 —
        // it must NOT inherit the viewport size like a screen-space canvas does.
        Assert.NotEqual(GUIManager.Width, canvas.Width);
        Assert.NotEqual(GUIManager.Height, canvas.Height);
    }

    [Fact]
    public void Canvas_ScreenSpace_SetWidth_PinsExplicitSize()
    {
        var canvas = new Canvas(isScreenSpace: true);

        canvas.Width = 320f;

        Assert.False(canvas.AutoWidth);
        Assert.Equal(320f, canvas.Width);
    }

    [Fact]
    public void Canvas_ScreenSpace_AutoWidthTrue_RestoresViewportSize()
    {
        var canvas = new Canvas(isScreenSpace: true);
        canvas.Width = 320f;

        canvas.AutoWidth = true;

        Assert.True(canvas.AutoWidth);
        Assert.Equal(GUIManager.Width, canvas.Width);
    }
}
