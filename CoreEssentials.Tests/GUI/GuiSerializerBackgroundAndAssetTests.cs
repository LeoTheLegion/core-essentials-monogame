#pragma warning disable CS8618 // Non-nullable field must contain null-free value

#nullable enable

using System;
using System.Reflection;
using Xunit;
using Microsoft.Xna.Framework;
using CoreEssentials.GUI;
using CoreEssentials.GUI.Types;
using CoreEssentials.GUI.Factory;
using CoreEssentials.Assets;

namespace CoreEssentials.Tests.GUI;

/// <summary>
/// Covers the device-free surface of <see cref="GuiSerializer"/> that the primary suite does not
/// reach: (a) the background/brush parsing helpers (hex RGB, hex ARGB, named colors, opacity, and
/// the unknown-color error path), and (b) the <see cref="XMLAsset"/>-overload success returns for
/// each widget type. Uses the shared fake widget factory so no graphics device is required.
/// </summary>
public class GuiSerializerBackgroundAndAssetTests : IDisposable
{
    private bool _disposed;

    public GuiSerializerBackgroundAndAssetTests() => WidgetFactory.Instance = new FakeWidgetFactory();

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
                WidgetFactory.Instance = new DefaultWidgetFactory();
            _disposed = true;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    // ── Background / brush parsing ────────────────────────────────────────────

    [Fact]
    public void LoadPanel_BackgroundHexRgb_ParsesOpaqueColor()
    {
        var panel = (FakePanel)GuiSerializer.LoadPanelFromXml(@"<Panel Background=""#FF8040"" />");

        Assert.NotNull(panel.Background);
        Assert.Equal(new Color(255, 128, 64, 255), panel.Background!.Color);
        Assert.Equal(1.0f, panel.Background.Opacity);
    }

    [Fact]
    public void LoadGrid_BackgroundHexArgb_ParsesAlphaChannel()
    {
        // #80FF0000 → A=0x80 (128), R=255, G=0, B=0.
        var grid = (FakeGrid)GuiSerializer.LoadGridFromXml(@"<Grid Background=""#80FF0000"" />");

        Assert.NotNull(grid.Background);
        Assert.Equal(new Color(255, 0, 0, 128), grid.Background!.Color);
    }

    [Theory]
    [InlineData("BLACK", 0, 0, 0)]
    [InlineData("WHITE", 255, 255, 255)]
    [InlineData("RED", 255, 0, 0)]
    [InlineData("GREEN", 0, 128, 0)]
    [InlineData("BLUE", 0, 0, 255)]
    [InlineData("YELLOW", 255, 255, 0)]
    [InlineData("GRAY", 128, 128, 128)]
    public void LoadPanel_BackgroundNamedColor_ParsesEachName(string name, int r, int g, int b)
    {
        var panel = (FakePanel)GuiSerializer.LoadPanelFromXml($@"<Panel Background=""{name}"" />");

        Assert.Equal(new Color(r, g, b, 255), panel.Background!.Color);
    }

    [Fact]
    public void LoadGrid_BackgroundWithOpacity_AppliesOpacity()
    {
        var grid = (FakeGrid)GuiSerializer.LoadGridFromXml(@"<Grid Background=""RED"" Opacity=""0.5"" />");

        Assert.Equal(0.5f, grid.Background!.Opacity);
    }

    [Fact]
    public void LoadPanel_NoBackgroundAttribute_LeavesBrushNull()
    {
        var panel = (FakePanel)GuiSerializer.LoadPanelFromXml(@"<Panel Width=""10"" />");

        Assert.Null(panel.Background);
    }

    [Fact]
    public void LoadPanel_BackgroundUnknownColor_ThrowsFormatException()
    {
        Assert.Throws<FormatException>(() => GuiSerializer.LoadPanelFromXml(@"<Panel Background=""Purple"" />"));
    }

    // ── XMLAsset overload success returns ─────────────────────────────────────

    [Fact]
    public void LoadLabelFromXml_AssetWithContent_ReturnsLabel()
    {
        var asset = WithContent(new XMLAsset("a.xml"), @"<Label Text=""Hi"" />");

        var label = GuiSerializer.LoadLabelFromXml(asset);

        Assert.Equal("Hi", label.Text);
    }

    [Fact]
    public void LoadButtonFromXml_AssetWithContent_ReturnsButton()
    {
        var asset = WithContent(new XMLAsset("a.xml"), @"<Button Text=""Go"" />");

        var button = GuiSerializer.LoadButtonFromXml(asset);

        Assert.Equal("Go", button.Text);
    }

    [Fact]
    public void LoadPanelFromXml_AssetWithContent_ReturnsPanel()
    {
        var asset = WithContent(new XMLAsset("a.xml"), @"<Panel />");

        var panel = GuiSerializer.LoadPanelFromXml(asset);

        Assert.NotNull(panel);
    }

    [Fact]
    public void LoadGridFromXml_AssetWithContent_ReturnsGrid()
    {
        var asset = WithContent(new XMLAsset("a.xml"), @"<Grid />");

        var grid = GuiSerializer.LoadGridFromXml(asset);

        Assert.NotNull(grid);
    }

    [Fact]
    public void LoadFromXml_AssetWithContent_DispatchesByRoot()
    {
        var asset = WithContent(new XMLAsset("a.xml"), @"<Label Text=""ViaAsset"" />");

        var widget = GuiSerializer.LoadFromXml(asset);

        Assert.Equal("ViaAsset", ((ILabel)widget).Text);
    }

    // ── Reflection helper: drive private _xmlContent (codebase convention) ────

    private static XMLAsset WithContent(XMLAsset asset, string xml)
    {
        typeof(XMLAsset).GetField("_xmlContent", BindingFlags.NonPublic | BindingFlags.Instance)!
            .SetValue(asset, xml);
        return asset;
    }
}
