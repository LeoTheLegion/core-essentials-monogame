using System;
using CoreEssentials.Assets;
using Xunit;

namespace CoreEssentials.Tests.Assets
{
    public class AssetTests
    {
        [Fact]
        public void Constructor_SetsName()
        {
            var asset = new TestAsset("foo");
            Assert.Equal("foo", asset.Name);
        }

        [Fact]
        public void Constructor_ThrowsIfNameNull()
        {
            Assert.Throws<ArgumentNullException>(() => new TestAsset(null));
        }

        [Fact]
        public void Constructor_ThrowsIfNameEmpty()
        {
            Assert.Throws<ArgumentException>(() => new TestAsset(""));
        }

        // Minimal concrete implementation for testing
        private class TestAsset : CoreEssentials.Assets.Asset
        {
            public TestAsset(string name) : base(name) { }
            public override void Load(IContentManager contentManager) { }
            public override void Unload(IContentManager contentManager) { }
        }
    }
}
