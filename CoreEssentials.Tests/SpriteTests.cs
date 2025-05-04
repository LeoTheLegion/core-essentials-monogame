using System;
using CoreEssentials.Assets;
using Xunit;
using Microsoft.Xna.Framework.Graphics;
using System.Reflection;

namespace CoreEssentials.Tests
{
    public class SpriteTests
    {
        [Fact]
        public void Constructor_SetsName()
        {
            var asset = new Sprite("sprite.xml");
            Assert.Equal("sprite.xml", asset.Name);
        }

        [Fact]
        public void GetSize_ReturnsSizeFromMetaData()
        {
            var asset = new Sprite("sprite.xml");
            // Simulate loaded metadata
            var metaType = asset.GetType().GetNestedType("SpriteMeta", BindingFlags.NonPublic);
            var meta = Activator.CreateInstance(metaType);
            metaType.GetProperty("SourceType")!.SetValue(meta, "texture2d");
            var sizeType = metaType.GetProperty("Size")!.PropertyType;
            var size = Activator.CreateInstance(sizeType);
            sizeType.GetProperty("Width")!.SetValue(size, 10f);
            sizeType.GetProperty("Height")!.SetValue(size, 20f);
            metaType.GetProperty("Size")!.SetValue(meta, size);
            asset.GetType().GetField("_metaData", BindingFlags.NonPublic | BindingFlags.Instance)!.SetValue(asset, meta);
            var result = asset.GetSize();
            Assert.Equal(10f, result.X);
            Assert.Equal(20f, result.Y);
        }
    }
}
