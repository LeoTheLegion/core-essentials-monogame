using System;
using CoreEssentials.Assets;
using Xunit;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Reflection;

namespace CoreEssentials.Tests
{
    public class Texture2DAssetTests
    {
        [Fact]
        public void Constructor_SetsName()
        {
            var asset = new Texture2DAsset("test.png");
            Assert.Equal("test.png", asset.Name);
        }

        [Fact]
        public void Load_ThrowsIfContentManagerNull()
        {
            var asset = new Texture2DAsset("test.png");
            Assert.Throws<ArgumentNullException>(() => asset.Load(null));
        }

        [Fact]
        public void Unload_ThrowsIfContentManagerNull()
        {
            var asset = new Texture2DAsset("test.png");
            Assert.Throws<ArgumentNullException>(() => asset.Unload(null));
        }

        // Skip the problematic tests that require MonoGame's GraphicsDevice
        // These tests should be moved to integration tests
    }
}
