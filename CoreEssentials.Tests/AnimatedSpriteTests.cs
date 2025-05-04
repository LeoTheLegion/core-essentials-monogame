using System;
using CoreEssentials.Assets;
using Xunit;
using System.Reflection;

namespace CoreEssentials.Tests
{
    public class AnimatedSpriteTests
    {
        [Fact]
        public void Constructor_SetsName()
        {
            var asset = new AnimatedSprite("anim.xml");
            Assert.Equal("anim.xml", asset.Name);
        }

        [Fact]
        public void DrawFrame_ThrowsIfFrameIndexOutOfRange()
        {
            var asset = new AnimatedSprite("anim.xml");
            // Simulate loaded frames and sprite sheet
            typeof(AnimatedSprite).GetField("_frames", BindingFlags.NonPublic | BindingFlags.Instance)!.SetValue(asset, new int[] { 0 });
            typeof(AnimatedSprite).GetField("_spriteSheet", BindingFlags.NonPublic | BindingFlags.Instance)!.SetValue(asset, new FakeSpriteSheet());
            Assert.Throws<IndexOutOfRangeException>(() => asset.DrawFrame(null, Microsoft.Xna.Framework.Vector2.Zero, 1, Microsoft.Xna.Framework.Color.White));
        }
    }

    public class FakeSpriteSheet : SpriteSheet
    {
        public FakeSpriteSheet() : base("fake") { }
        public new Microsoft.Xna.Framework.Rectangle GetFrame(int index) => new Microsoft.Xna.Framework.Rectangle();
        public new Microsoft.Xna.Framework.Vector2 FrameOrigin => Microsoft.Xna.Framework.Vector2.Zero;
        public new Microsoft.Xna.Framework.Graphics.Texture2D Texture => null;
    }
}
