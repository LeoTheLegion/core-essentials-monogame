using System;
using CoreEssentials.Assets;
using Xunit;
using System.Reflection;

namespace CoreEssentials.Tests
{
    public class AssetManagerTests
    {
        [Fact]
        public void LoadAsset_ThrowsIfAssetNameNullOrEmpty()
        {
            AssetManager.Init(new MockContentManager());
            Assert.Throws<ArgumentNullException>(() => AssetManager.LoadAsset<AssetManagerTests.FakeAsset>(null));
            Assert.Throws<ArgumentNullException>(() => AssetManager.LoadAsset<AssetManagerTests.FakeAsset>(""));
        }

        [Fact]
        public void LoadAsset_ThrowsIfTypeNotAsset()
        {
            AssetManager.Init(new MockContentManager());
            Assert.Throws<ArgumentException>(() => AssetManager.LoadAsset<string>("foo"));
        }

        public class FakeAsset : Asset
        {
            public FakeAsset(string name) : base(name) { }
            public override void Load(IContentManager contentManager) { }
            public override void Unload(IContentManager contentManager) { }
        }
    }
}
