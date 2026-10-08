using Microsoft.Xna.Framework;
using Xunit;
using CoreEssentials.GUI.Internal;

namespace CoreEssentials.Tests.GUI;

public class ColorAdapterTests
{
    [Fact]
    public void ToMyraBrush_ReturnsNonNullBrush()
    {
        var brush = ColorAdapter.ToMyraBrush(Color.Red);
        Assert.NotNull(brush);
    }

    [Fact]
    public void AsBrush_Extension_ReturnsNonNullBrush()
    {
        var brush = Color.Green.AsBrush();
        Assert.NotNull(brush);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(128)]
    [InlineData(255)]
    public void WithAlpha_SetsAlphaPreservingRgb(byte alpha)
    {
        var original = new Color(10, 20, 30);
        var result = original.WithAlpha(alpha);

        Assert.Equal(original.R, result.R);
        Assert.Equal(original.G, result.G);
        Assert.Equal(original.B, result.B);
        Assert.Equal(alpha, result.A);
    }

    [Fact]
    public void WithAlpha_DoesNotMutateOriginal()
    {
        var original = new Color(1, 2, 3);
        _ = original.WithAlpha(50);
        Assert.Equal(255, original.A);
    }
}
