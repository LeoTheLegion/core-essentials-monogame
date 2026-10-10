using System;
using CoreEssentials.Assets;
using CoreEssentials.GUI;
using Xunit;

namespace CoreEssentials.Tests.GUI;

/// <summary>
/// Covers the device-free XML-validation branches of <see cref="GuiSerializer"/> that the primary
/// suite does not reach: wrong/malformed root elements and null-asset content for every widget type.
/// These throw before any widget factory or graphics work, so no device is required.
/// </summary>
public class GuiSerializerValidationTests
{
    [Fact]
    public void LoadButtonFromXml_MalformedXml_ThrowsFormatException()
        => Assert.Throws<FormatException>(() => GuiSerializer.LoadButtonFromXml("<Button><oops>"));

    [Fact]
    public void LoadButtonFromXml_WrongRootElement_ThrowsFormatException()
        => Assert.Throws<FormatException>(() => GuiSerializer.LoadButtonFromXml("<Label/>"));

    [Fact]
    public void LoadPanelFromXml_MalformedXml_ThrowsFormatException()
        => Assert.Throws<FormatException>(() => GuiSerializer.LoadPanelFromXml("<Panel><oops>"));

    [Fact]
    public void LoadPanelFromXml_WrongRootElement_ThrowsFormatException()
        => Assert.Throws<FormatException>(() => GuiSerializer.LoadPanelFromXml("<Grid/>"));

    [Fact]
    public void LoadGridFromXml_MalformedXml_ThrowsFormatException()
        => Assert.Throws<FormatException>(() => GuiSerializer.LoadGridFromXml("<Grid><oops>"));

    [Fact]
    public void LoadGridFromXml_WrongRootElement_ThrowsFormatException()
        => Assert.Throws<FormatException>(() => GuiSerializer.LoadGridFromXml("<Button/>"));

    [Theory]
    [InlineData("null-content-asset")]
    public void NullContentAsset_LoadersThrowArgumentException(string assetName)
    {
        var empty = new XMLAsset(assetName); // XMLContent is null by construction

        Assert.Throws<ArgumentException>(() => GuiSerializer.LoadLabelFromXml(empty));
        Assert.Throws<ArgumentException>(() => GuiSerializer.LoadButtonFromXml(empty));
        Assert.Throws<ArgumentException>(() => GuiSerializer.LoadPanelFromXml(empty));
        Assert.Throws<ArgumentException>(() => GuiSerializer.LoadGridFromXml(empty));
    }
}
