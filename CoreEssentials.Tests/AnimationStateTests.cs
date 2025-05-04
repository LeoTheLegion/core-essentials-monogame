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
        public void Constructor_SetsAnimatedSprite()
        {
            var fakeSprite = new FakeAnimatedSprite();
            var state = new AnimationState(fakeSprite);
            Assert.Equal(fakeSprite, state.AnimatedSprite);
        }

        [Fact]
        public void Constructor_ThrowsIfNull()
        {
            Assert.Throws<ArgumentNullException>(() => new AnimationState(null));
        }

        [Fact]
        public void SetFrame_ThrowsIfOutOfRange()
        {
            var fakeSprite = new FakeAnimatedSprite { FrameCountValue = 2 };
            var state = new AnimationState(fakeSprite);
            Assert.Throws<ArgumentOutOfRangeException>(() => state.SetFrame(-1));
            Assert.Throws<ArgumentOutOfRangeException>(() => state.SetFrame(2));
        }
    }

    public class FakeAnimatedSprite : AnimatedSprite
    {
        public int FrameCountValue = 1;
        public FakeAnimatedSprite() : base("fake") { }
        public new int FrameCount => FrameCountValue;
    }
}
