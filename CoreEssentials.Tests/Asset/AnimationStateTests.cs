using System;
using CoreEssentials.Assets;
using Xunit;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Reflection;

namespace CoreEssentials.Tests
{
    public class AnimationStateTests
    {
        [Fact]
        public void Constructor_SetsSprite()
        {
            var fakeSprite = new FakeSprite();
            var state = new AnimationState(fakeSprite);
            Assert.Equal(fakeSprite, state.Sprite);
        }

        [Fact]
        public void Constructor_ThrowsIfNull()
        {
            Assert.Throws<ArgumentNullException>(() => new AnimationState(null));
        }

        [Fact]
        public void SetFrame_ThrowsIfOutOfRange()
        {
            var fakeSprite = new FakeSprite { FrameCountValue = 2 };
            var state = new AnimationState(fakeSprite);
            Assert.Throws<ArgumentOutOfRangeException>(() => state.SetFrame(-1));
            Assert.Throws<ArgumentOutOfRangeException>(() => state.SetFrame(2));
        }
    }

    public class FakeSprite : Sprite
    {
        public int FrameCountValue = 1;
        public FakeSprite() : base("fake") { }
        public new int FrameCount => FrameCountValue;
    }
}
