using CoreEssentials.GUI.Fonts;
using Xunit;

namespace CoreEssentials.Tests.GUI;

/// <summary>
/// Tests for the GUI TTF font loader. The full render path requires a live graphics device, so these
/// focus on the argument-validation contract and cache management that are safe to exercise headless.
/// </summary>
public class GuiFontLoaderTests
{
    [Fact]
    public void Load_EmptyName_Throws()
    {
        Assert.Throws<System.ArgumentException>(() => GuiFontLoader.Load("", 20));
    }

    [Fact]
    public void Load_WhitespaceName_Throws()
    {
        Assert.Throws<System.ArgumentException>(() => GuiFontLoader.Load("   ", 20));
    }

    [Fact]
    public void ClearCache_DoesNotThrow()
    {
        // Should be a safe no-op even when the cache is already empty.
        GuiFontLoader.ClearCache();
    }
}
