using System;
using CoreEssentials.GUI.Engines.Myra.Brushes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Xunit;

namespace CoreEssentials.Tests.GUI;

public class TextureBrushTests
{
    private static readonly Texture2D FakeTexture = CoreEssentials.Tests.FakeTexture2D.Instance;

    [Fact]
    public void Constructor_StoresTextureAndDefaultsTintToWhite()
    {
        var brush = new TextureBrush(FakeTexture);

        Assert.Same(FakeTexture, brush.Texture);
        Assert.Equal(Color.White, brush.Tint);
        Assert.Null(brush.Source);
    }

    [Fact]
    public void Constructor_StoresExplicitTintAndSource()
    {
        var source = new Rectangle(4, 8, 16, 16);
        var brush = new TextureBrush(FakeTexture, Color.CornflowerBlue, source);

        Assert.Equal(Color.CornflowerBlue, brush.Tint);
        Assert.Equal(source, brush.Source);
    }

    [Fact]
    public void Constructor_NullTexture_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => new TextureBrush(null!));
    }

    [Fact]
    public void Tint_IsSettable()
    {
        var brush = new TextureBrush(FakeTexture);

        brush.Tint = Color.Tomato;

        Assert.Equal(Color.Tomato, brush.Tint);
    }
}
