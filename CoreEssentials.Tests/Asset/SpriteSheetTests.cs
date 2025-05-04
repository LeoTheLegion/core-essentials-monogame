using System;
using CoreEssentials.Assets;
using Xunit;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Reflection;

namespace CoreEssentials.Tests
{
    public class SpriteSheetTests
    {
        [Fact]
        public void Constructor_SetsName()
        {
            var asset = new SpriteSheet("sheet.xml");
            Assert.Equal("sheet.xml", asset.Name);
        }

        [Fact]
        public void GetFrame_ThrowsIfIndexOutOfRange()
        {
            var asset = new SpriteSheet("sheet.xml");
            // Simulate loaded frames
            typeof(SpriteSheet).GetField("_frames", BindingFlags.NonPublic | BindingFlags.Instance)!.SetValue(asset, new Rectangle[2]);
            Assert.Throws<ArgumentOutOfRangeException>(() => asset.GetFrame(-1));
            Assert.Throws<ArgumentOutOfRangeException>(() => asset.GetFrame(2));
        }
    }
}
