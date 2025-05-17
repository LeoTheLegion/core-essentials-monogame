using System;
using CoreEssentials.Assets;
using Xunit;
using System.Reflection;

namespace CoreEssentials.Tests.Assets
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

        public class FakeAsset : CoreEssentials.Assets.Asset
        {
            public FakeAsset(string name) : base(name) { }
            public override void Load(IContentManager contentManager) { }
            public override void Unload(IContentManager contentManager) { }
        }
    }
}
