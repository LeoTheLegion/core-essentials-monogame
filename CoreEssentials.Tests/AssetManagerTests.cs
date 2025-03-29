using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using CoreEssentials.Assets;

namespace CoreEssentials.Tests
{
    [TestClass]
    public class AssetManagerTests
    {
        [TestMethod]
        public void LoadAsset_NullAssetName_ThrowsArgumentNullException()
        {
            Assert.ThrowsException<ArgumentNullException>(() =>
            {
                AssetManager.LoadAsset<object>(null);
            });
        }

        [TestMethod]
        public void LoadAsset_EmptyAssetName_ThrowsArgumentNullException()
        {
            Assert.ThrowsException<ArgumentNullException>(() =>
            {
                AssetManager.LoadAsset<object>("");
            });
        }
    }
}