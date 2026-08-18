using System;
using CoreEssentials.Assets;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Xunit;

namespace CoreEssentials.Tests.Asset
{
    /// <summary>
    /// Tests for the unified <see cref="Sprite"/> type (Sprint 15.5).
    /// A single Sprite now handles both a single-frame <c>texture2d</c> source and an
    /// N-frame <c>spritesheet</c> source. These tests drive the type through the internal
    /// test seams so no GraphicsDevice or AssetManager is required.
    /// </summary>
    public class UnifiedSpriteTests
    {
        // ===== Frame sequence building =====

        [Fact]
        public void BuildFrameSequence_Texture2D_SingleFrame()
        {
            var sprite = new Sprite("t.xml");
            sprite.TestMetaData = new Sprite.SpriteMeta { SourceType = "texture2d" };

            sprite.TestBuildFrameSequence();

            Assert.Equal(new[] { 0 }, sprite.TestFrames);
        }

        [Fact]
        public void BuildFrameSequence_SpriteSheet_ExplicitFrames()
        {
            var sprite = new Sprite("s.xml");
            sprite.TestMetaData = new Sprite.SpriteMeta { SourceType = "spritesheet", Frames = "36,37,38,39" };

            sprite.TestBuildFrameSequence();

            Assert.Equal(new[] { 36, 37, 38, 39 }, sprite.TestFrames);
        }

        [Fact]
        public void BuildFrameSequence_SpriteSheet_SingleFrame()
        {
            var sprite = new Sprite("s.xml");
            sprite.TestMetaData = new Sprite.SpriteMeta { SourceType = "spritesheet", Frame = 5 };

            sprite.TestBuildFrameSequence();

            Assert.Equal(new[] { 5 }, sprite.TestFrames);
        }

        [Fact]
        public void BuildFrameSequence_SpriteSheet_NoFrames_DefaultsToZero()
        {
            var sprite = new Sprite("s.xml");
            sprite.TestMetaData = new Sprite.SpriteMeta { SourceType = "spritesheet" };

            sprite.TestBuildFrameSequence();

            Assert.Equal(new[] { 0 }, sprite.TestFrames);
        }

        [Fact]
        public void BuildFrameSequence_ConvertsFpsToSecondsPerFrame()
        {
            var sprite = new Sprite("s.xml");
            sprite.TestMetaData = new Sprite.SpriteMeta { SourceType = "spritesheet", Frames = "0,1", FrameRate = "11" };

            sprite.TestBuildFrameSequence();

            Assert.Equal(1f / 11f, sprite.FrameRate, precision: 5);
        }

        [Fact]
        public void BuildFrameSequence_InvalidFrameRate_FallsBackToTenFps()
        {
            var sprite = new Sprite("s.xml");
            sprite.TestMetaData = new Sprite.SpriteMeta { SourceType = "spritesheet", Frames = "0,1", FrameRate = "0" };

            sprite.TestBuildFrameSequence();

            Assert.Equal(1f / 10f, sprite.FrameRate, precision: 5);
        }

        // ===== GetSize =====

        [Fact]
        public void GetSize_Texture2D_ReturnsMetadataSize()
        {
            var sprite = new Sprite("t.xml");
            sprite.TestMetaData = new Sprite.SpriteMeta
            {
                SourceType = "texture2d",
                Size = new Sprite.Size { Width = 64, Height = 64 }
            };

            Assert.Equal(new Vector2(64, 64), sprite.GetSize());
        }

        [Fact]
        public void GetSize_SpriteSheet_ReturnsFrameSize()
        {
            var sprite = new Sprite("s.xml");
            sprite.TestMetaData = new Sprite.SpriteMeta { SourceType = "spritesheet" };
            sprite.TestSpriteSheet = new FakeSpriteSheet();

            Assert.Equal(new Vector2(32, 32), sprite.GetSize());
        }

        [Fact]
        public void GetSize_ThrowsWhenMetadataNotLoaded()
        {
            var sprite = new Sprite("s.xml");

            Assert.Throws<InvalidOperationException>(() => sprite.GetSize());
        }

        // ===== FrameCount / DrawFrame =====

        [Fact]
        public void FrameCount_ReflectsFrames()
        {
            var sprite = new Sprite("s.xml");
            sprite.TestFrames = new[] { 0, 1, 2 };

            Assert.Equal(3, sprite.FrameCount);
        }

        [Fact]
        public void DrawFrame_ThrowsIfFrameIndexOutOfRange()
        {
            var sprite = new Sprite("s.xml");
            sprite.TestMetaData = new Sprite.SpriteMeta { SourceType = "spritesheet" };
            sprite.TestSpriteSheet = new FakeSpriteSheet();
            sprite.TestFrames = new[] { 0 };

            Assert.Throws<IndexOutOfRangeException>(
                () => sprite.DrawFrame(null, Vector2.Zero, 1, Color.White));
        }

        // ===== Fake helpers =====

        /// <summary>A SpriteSheet that reports fixed frame geometry without a texture.</summary>
        private class FakeSpriteSheet : SpriteSheet
        {
            public FakeSpriteSheet() : base("fake_sheet.xml") { }

            public override Vector2 GetFrameSize() => new Vector2(32, 32);

            public override Rectangle GetFrame(int index) => new Rectangle(index * 32, 0, 32, 32);

            public override Vector2 FrameOrigin => new Vector2(16, 16);
        }
    }
}
