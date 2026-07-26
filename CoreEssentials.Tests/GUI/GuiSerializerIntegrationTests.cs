using System;
using Xunit;
using Microsoft.Xna.Framework;
using CoreEssentials.GUI;
using CoreEssentials.GUI.Types;
using CoreEssentials.GUI.Factory;
using CoreEssentials.Assets;

namespace CoreEssentials.Tests.GUI;

public class GuiSerializerIntegrationTests : IDisposable
{
    public GuiSerializerIntegrationTests()
    {
        WidgetFactory.Instance = new FakeWidgetFactory();
    }

    public void Dispose()
    {
        WidgetFactory.Instance = new DefaultWidgetFactory();
    }

    [Fact]
    public void LoadFromXml_DetectsAndLoadsCorrectType_Label()
    {
        // Arrange
        string xml = @"<Label Text=""Integration Label"" />";

        // Act
        var widget = GuiSerializer.LoadFromXml(xml);

        // Assert
        Assert.IsAssignableFrom<ILabel>(widget);
        Assert.Equal("Integration Label", ((ILabel)widget).Text);
    }

    [Fact]
    public void LoadFromXml_DetectsAndLoadsCorrectType_Button()
    {
        // Arrange
        string xml = @"<Button Text=""Integration Button"" />";

        // Act
        var widget = GuiSerializer.LoadFromXml(xml);

        // Assert
        Assert.IsAssignableFrom<IButton>(widget);
        Assert.Equal("Integration Button", ((IButton)widget).Text);
    }

    [Fact]
    public void LoadFromXml_DetectsAndLoadsCorrectType_Panel()
    {
        // Arrange
        string xml = @"<Panel Width=""100"" />";

        // Act
        var widget = GuiSerializer.LoadFromXml(xml);

        // Assert
        Assert.IsAssignableFrom<IPanel>(widget);
        Assert.Equal(100f, widget.Width);
    }

    [Fact]
    public void LoadFromXml_DetectsAndLoadsCorrectType_Grid()
    {
        // Arrange
        string xml = @"<Grid RowSpacing=""10"" />";

        // Act
        var widget = GuiSerializer.LoadFromXml(xml);

        // Assert
        Assert.IsAssignableFrom<IGrid>(widget);
        Assert.Equal(10f, ((IGrid)widget).RowSpacing);
    }

    [Fact]
    public void LoadFromXml_UnsupportedElement_ThrowsFormatException()
    {
        // Arrange
        string xml = @"<UnknownWidget Text=""Error"" />";

        // Act & Assert
        Assert.Throws<FormatException>(() => GuiSerializer.LoadFromXml(xml));
    }

    [Fact]
    public void LoadFromXml_EmptyXml_ThrowsFormatException()
    {
        // Arrange
        string xml = @"";

        // Act & Assert
        Assert.Throws<FormatException>(() => GuiSerializer.LoadFromXml(xml));
    }
}
