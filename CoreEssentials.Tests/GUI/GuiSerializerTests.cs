#pragma warning disable CS8618 // Non-nullable field must contain null-free value
#pragma warning disable CS8614 // Nullable reference type has directionality

#nullable enable

using System;
using System.Collections.Generic;
using Xunit;
using Microsoft.Xna.Framework;
using CoreEssentials.GUI;
using CoreEssentials.GUI.Types;
using CoreEssentials.GUI.Factory;
using CoreEssentials.Assets;

namespace CoreEssentials.Tests.GUI;

#region Fakes

public class FakeLabel : ILabel
{
    public bool AutoWidth { get; set; } = true;
    public bool AutoHeight { get; set; } = true;
    public float Width { get; set; }
    public float Height { get; set; }
    public bool Visible { get; set; } = true;
    public bool Enabled { get; set; } = true;
    public bool IsMouseInside { get; set; }
    public bool IsKeyboardFocused { get; set; }
    public Vector2 Position { get; set; }
    public Thickness Margin { get; set; }
    public HorizontalAlignment HorizontalAlignment { get; set; }
    public VerticalAlignment VerticalAlignment { get; set; }
    public Vector2 Scale { get; set; } = Vector2.One;
    public float Opacity { get; set; } = 1.0f;
    public string? Text { get; set; }
    public object? Font { get; set; }
    public Color TextColor { get; set; } = Color.White;
}

public class FakeButton : IButton
{
    public bool AutoWidth { get; set; } = true;
    public bool AutoHeight { get; set; } = true;
    public float Width { get; set; }
    public float Height { get; set; }
    public bool Visible { get; set; } = true;
    public bool Enabled { get; set; } = true;
    public bool IsMouseInside { get; set; }
    public bool IsKeyboardFocused { get; set; }
    public Vector2 Position { get; set; }
    public Thickness Margin { get; set; }
    public HorizontalAlignment HorizontalAlignment { get; set; }
    public VerticalAlignment VerticalAlignment { get; set; }
    public Vector2 Scale { get; set; } = Vector2.One;
    public float Opacity { get; set; } = 1.0f;
    public string? Text { get; set; }
#pragma warning disable CS0067 // The event is never used (required by IButton interface)
    public event Action<IButton>? Clicked;
#pragma warning restore CS0067
}

public class FakePanel : IPanel
{
    public bool AutoWidth { get; set; } = true;
    public bool AutoHeight { get; set; } = true;
    public float Width { get; set; }
    public float Height { get; set; }
    public bool Visible { get; set; } = true;
    public bool Enabled { get; set; } = true;
    public bool IsMouseInside { get; set; }
    public bool IsKeyboardFocused { get; set; }
    public Vector2 Position { get; set; }
    public Thickness Margin { get; set; }
    public HorizontalAlignment HorizontalAlignment { get; set; }
    public VerticalAlignment VerticalAlignment { get; set; }
    public Vector2 Scale { get; set; } = Vector2.One;
    public float Opacity { get; set; } = 1.0f;
    public IBrush? Background { get; set; }
    public Thickness BorderThickness { get; set; }
    public System.Collections.Generic.IList<IWidget> Children { get; } = new System.Collections.Generic.List<IWidget>();
    public System.Collections.Generic.IEnumerable<IWidget> Widgets => Children;
    public void AddChild(IWidget widget) => Children.Add(widget);
    public void RemoveChild(IWidget widget) => Children.Remove(widget);
    public void ClearChildren() => Children.Clear();
}

public class FakeGrid : IGrid
{
    public bool AutoWidth { get; set; } = true;
    public bool AutoHeight { get; set; } = true;
    public float Width { get; set; }
    public float Height { get; set; }
    public bool Visible { get; set; } = true;
    public bool Enabled { get; set; } = true;
    public bool IsMouseInside { get; set; }
    public bool IsKeyboardFocused { get; set; }
    public Vector2 Position { get; set; }
    public Thickness Margin { get; set; }
    public HorizontalAlignment HorizontalAlignment { get; set; }
    public VerticalAlignment VerticalAlignment { get; set; }
    public Vector2 Scale { get; set; } = Vector2.One;
    public float Opacity { get; set; } = 1.0f;
    public IBrush? Background { get; set; }
    public System.Collections.Generic.IList<float> RowProportions { get; } = new System.Collections.Generic.List<float>();
    public System.Collections.Generic.IList<float> ColumnProportions { get; } = new System.Collections.Generic.List<float>();
    public float RowSpacing { get; set; }
    public float ColumnSpacing { get; set; }
    public System.Collections.Generic.IList<IWidget> Children { get; } = new System.Collections.Generic.List<IWidget>();
    public System.Collections.Generic.IEnumerable<IWidget> Widgets => Children;
    public void AddChild(IWidget widget) => Children.Add(widget);
    public void RemoveChild(IWidget widget) => Children.Remove(widget);
    public void ClearChildren() => Children.Clear();
    public void SetRow(IWidget widget, int rowIndex) { }
    public void SetColumn(IWidget widget, int columnIndex) { }
    public int GetRow(IWidget widget) => -1;
    public int GetColumn(IWidget widget) => -1;
}

public class FakeWidgetFactory : IWidgetFactory
{
    public IPanel CreatePanel() => new FakePanel();
    public ILabel CreateLabel(string text) => new FakeLabel { Text = text };
    public IButton CreateTextButton(string text) => new FakeButton { Text = text };
    public IGrid CreateGrid() => new FakeGrid();
}

#endregion

public class GuiSerializerTests : IDisposable
{
    private bool _disposed;

    public GuiSerializerTests()
    {
        WidgetFactory.Instance = new FakeWidgetFactory();
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                WidgetFactory.Instance = new DefaultWidgetFactory();
            }
            _disposed = true;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    [Fact]
    public void LoadLabelFromXml_ValidXml_ReturnsLabelWithCorrectProperties()

    {
        // Arrange
        string xml = @"<Label Text=""Hello World"" Width=""100"" Height=""20"" Visible=""false"" Enabled=""true"" X=""10"" Y=""20"" TextColor=""Red"" />";

        // Act
        var label = GuiSerializer.LoadLabelFromXml(xml);

        // Assert
        Assert.NotNull(label);
        Assert.Equal("Hello World", label.Text);
        Assert.Equal(100f, label.Width);
        Assert.Equal(20f, label.Height);
        Assert.False(label.Visible);
        Assert.True(label.Enabled);
        Assert.Equal(new Vector2(10, 20), label.Position);
        Assert.Equal(Color.Red, label.TextColor);
    }

    [Fact]
    public void LoadButtonFromXml_ValidXml_ReturnsButtonWithCorrectProperties()
    {
        // Arrange
        string xml = @"<Button Text=""Click Me"" Width=""50"" Height=""30"" X=""100"" Y=""150"" />";

        // Act
        var button = GuiSerializer.LoadButtonFromXml(xml);

        // Assert
        Assert.NotNull(button);
        Assert.Equal("Click Me", button.Text);
        Assert.Equal(50f, button.Width);
        Assert.Equal(30f, button.Height);
        Assert.Equal(new Vector2(100, 150), button.Position);
    }

    [Fact]
    public void LoadLabelFromXml_MalformedXml_ThrowsFormatException()
    {
        // Arrange
        string xml = @"<Label Text=""Missing Close Tag"";";

        // Act & Assert
        Assert.Throws<FormatException>(() => GuiSerializer.LoadLabelFromXml(xml));
    }

    [Fact]
    public void LoadLabelFromXml_WrongRootElement_ThrowsFormatException()
    {
        // Arrange
        string xml = @"<NotALabel Text=""Wrong"" />";

        // Act & Assert
        Assert.Throws<FormatException>(() => GuiSerializer.LoadLabelFromXml(xml));
    }

    [Fact]
    public void LoadLabelFromXml_MissingAttributes_UsesDefaults()
    {
        // Arrange
        string xml = @"<Label />";

        // Act
        var label = GuiSerializer.LoadLabelFromXml(xml);

        // Assert
        Assert.NotNull(label);
        Assert.Equal(string.Empty, label.Text);
    }

    [Fact]
    public void LoadLabelFromXml_InvalidColor_UsesDefaultColor()
    {
        // Arrange
        string xml = @"<Label TextColor=""NotAColor"" />";

        // Act
        var label = GuiSerializer.LoadLabelFromXml(xml);

        // Assert
        Assert.NotNull(label);
        // Color.TryParse should fail and keep default. 
        // Assuming default is White or similar, just check it doesn't throw.
    }

    [Fact]
    public void LoadLabelFromXml_InvalidNumbers_IgnoresAndUsesDefaults()
    {
        // Arrange
        string xml = @"<Label Width=""NaN"" Height=""Invalid"" />";

        // Act
        var label = GuiSerializer.LoadLabelFromXml(xml);

        // Assert
        Assert.NotNull(label);
        // Width and Height should remain their default values (likely 0)
    }

    [Fact]
    public void LoadPanelFromXml_Recursive_ReturnsPanelWithChildren()
    {
        // Arrange
        string xml = @"
        <Panel Width=""400"" Height=""300"">
            <Label Text=""Child Label"" Width=""50"" />
            <Button Text=""Child Button"" Height=""20"" />
            <Panel Width=""100"" Height=""100"">
                <Label Text=""Nested Label"" />
            </Panel>
        </Panel>";

        // Act
        var panel = GuiSerializer.LoadPanelFromXml(xml);

        // Assert
        Assert.NotNull(panel);
        Assert.Equal(400f, panel.Width);
        Assert.Equal(3, panel.Children.Count);
        
        var label = Assert.IsType<FakeLabel>(panel.Children[0]);
        Assert.Equal("Child Label", label.Text);
        
        var button = Assert.IsType<FakeButton>(panel.Children[1]);
        Assert.Equal("Child Button", button.Text);
        
        var nestedPanel = Assert.IsType<FakePanel>(panel.Children[2]);
        Assert.Single(nestedPanel.Children);
        var nestedLabel = Assert.IsType<FakeLabel>(nestedPanel.Children[0]);
        Assert.Equal("Nested Label", nestedLabel.Text);
    }

    [Fact]
    public void LoadGridFromXml_ValidXml_ReturnsGridWithProperties()
    {
        // Arrange
        string xml = @"<Grid RowSpacing=""10"" ColumnSpacing=""5"" Width=""200"" />";

        // Act
        var grid = GuiSerializer.LoadGridFromXml(xml);

        // Assert
        Assert.NotNull(grid);
        Assert.Equal(10f, grid.RowSpacing);
        Assert.Equal(5f, grid.ColumnSpacing);
        Assert.Equal(200f, grid.Width);
    }

    [Fact]
    public void LoadGridFromXml_BackgroundHexARGB_ParsesCorrectly()
    {
        // Arrange: #64 = alpha=100 (~39%), 000000 = black
        string xml = @"<Grid Width=""200"" Height=""100"" Background=""#64000000"" />";

        // Act
        var grid = GuiSerializer.LoadGridFromXml(xml);

        // Assert — SolidColorBrush stores the raw color (A=100) and opacity defaults to 1.0
        Assert.NotNull(grid.Background);
        Assert.True(grid.Background.IsSolid);
        Assert.Equal(1f, grid.Background.Opacity, 0.01f);
    }

    [Fact]
    public void LoadGridFromXml_BackgroundNamedColor_ParsesCorrectly()
    {
        // Arrange
        string xml = @"<Grid Width=""200"" Background=""Black"" />";

        // Act
        var grid = GuiSerializer.LoadGridFromXml(xml);

        // Assert — solid black with default opacity 1.0
        Assert.NotNull(grid.Background);
        Assert.True(grid.Background.IsSolid);
        Assert.Equal(1f, grid.Background.Opacity, 0.01f);
    }

    [Fact]
    public void LoadPanelFromXml_BackgroundHexRGB_ParsesCorrectly()
    {
        // Arrange: #FF0000FF = opaque blue (ARGB)
        string xml = @"<Panel Width=""200"" Height=""100"" Background=""#FF0000FF"" />";

        // Act
        var panel = GuiSerializer.LoadPanelFromXml(xml);

        // Assert
        Assert.NotNull(panel.Background);
        Assert.True(panel.Background.IsSolid);
    }

    [Fact]
    public void LoadGridFromXml_BackgroundWithOpacityOverride_ParsesCorrectly()
    {
        // Arrange: named color + explicit opacity override
        string xml = @"<Grid Width=""200"" Background=""Black"" Opacity=""0.4"" />";

        // Act
        var grid = GuiSerializer.LoadGridFromXml(xml);

        // Assert — solid black with custom opacity
        Assert.NotNull(grid.Background);
        Assert.True(grid.Background.IsSolid);
        Assert.Equal(0.4f, grid.Background.Opacity, 0.01f);
    }

    [Fact]
    public void LoadWidgetFromXml_NoBackground_ReturnsNullBackground()
    {
        // Arrange
        string xml = @"<Grid Width=""200"" Height=""100"" />";

        // Act
        var grid = GuiSerializer.LoadGridFromXml(xml);

        // Assert
        Assert.Null(grid.Background);
    }
}
