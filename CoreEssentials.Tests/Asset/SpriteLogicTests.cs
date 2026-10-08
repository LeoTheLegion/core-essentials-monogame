using System;
using CoreEssentials.Assets;
using Microsoft.Xna.Framework;
using Xunit;

namespace CoreEssentials.Tests.Asset
{
    /// <summary>
    /// Exercises the pure-logic portions of <see cref="Sprite"/> (metadata-driven size/origin
    /// and frame-sequence building) through its internal test seams, without a GraphicsDevice.
    /// These paths are otherwise uncovered because other tests stub the asset with mocks.
    /// </summary>
    public class SpriteLogicTests
    {
        [Fact]
        public void GetSize_WithoutMetadata_Throws()
        {
            var sprite = new Sprite("sprite.xml");

            Assert.Throws<InvalidOperationException>(() => sprite.GetSize());
        }

        [Fact]
        public void GetSize_Texture2D_ReturnsMetaDataSize()
        {
            var sprite = new Sprite("sprite.xml")
            {
                TestMetaData = new Sprite.SpriteMeta
                {
                    SourceType = "texture2d",
                    Size = new Sprite.Size { Width = 12f, Height = 34f }
                }
            };

            var size = sprite.GetSize();

            Assert.Equal(12f, size.X);
            Assert.Equal(34f, size.Y);
        }

        [Fact]
        public void GetSize_Texture2D_NullSize_Throws()
        {
            var sprite = new Sprite("sprite.xml")
            {
                TestMetaData = new Sprite.SpriteMeta { SourceType = "texture2d", Size = null }
            };

            Assert.Throws<InvalidOperationException>(() => sprite.GetSize());
        }

        [Fact]
        public void GetOrigin_WithoutMetadata_Throws()
        {
            var sprite = new Sprite("sprite.xml");

            Assert.Throws<InvalidOperationException>(() => sprite.GetOrigin());
        }

        [Fact]
        public void GetOrigin_Texture2D_NullOrigin_ReturnsZero()
        {
            var sprite = new Sprite("sprite.xml")
            {
                TestMetaData = new Sprite.SpriteMeta { SourceType = "texture2d", Origin = null }
            };

            Assert.Equal(Vector2.Zero, sprite.GetOrigin());
        }

        [Fact]
        public void GetOrigin_Texture2D_ReturnsMetaDataOrigin()
        {
            var sprite = new Sprite("sprite.xml")
            {
                TestMetaData = new Sprite.SpriteMeta
                {
                    SourceType = "texture2d",
                    Origin = new Sprite.Origin { X = 5f, Y = 7f }
                }
            };

            var origin = sprite.GetOrigin();

            Assert.Equal(5f, origin.X);
            Assert.Equal(7f, origin.Y);
        }

        [Fact]
        public void BuildFrameSequence_Texture2D_SingleFrame()
        {
            var sprite = new Sprite("sprite.xml")
            {
                TestMetaData = new Sprite.SpriteMeta { SourceType = "texture2d", FrameRate = "30" }
            };

            sprite.TestBuildFrameSequence();

            Assert.Equal(new[] { 0 }, sprite.Frames);
            Assert.Equal(1, sprite.FrameCount);
            Assert.Equal(1f / 30f, sprite.FrameRate, FrameRatePrecision);
        }

        [Fact]
        public void BuildFrameSequence_Texture2D_DefaultFrameRate()
        {
            var sprite = new Sprite("sprite.xml")
            {
                TestMetaData = new Sprite.SpriteMeta { SourceType = "texture2d", FrameRate = null }
            };

            sprite.TestBuildFrameSequence();

            Assert.Equal(new[] { 0 }, sprite.Frames);
            // No frame rate supplied -> default 10 fps -> 0.1 s per frame.
            Assert.Equal(0.1f, sprite.FrameRate, FrameRatePrecision);
        }

        [Theory]
        [InlineData("0")]   // zero -> falls back to default 10
        [InlineData("-5")]  // negative -> falls back to default 10
        [InlineData("abc")] // unparseable -> falls back to default 10
        public void BuildFrameSequence_InvalidFrameRate_FallsBackToDefault(string frameRate)
        {
            var sprite = new Sprite("sprite.xml")
            {
                TestMetaData = new Sprite.SpriteMeta { SourceType = "texture2d", FrameRate = frameRate }
            };

            sprite.TestBuildFrameSequence();

            Assert.Equal(0.1f, sprite.FrameRate, FrameRatePrecision);
        }

        [Fact]
        public void BuildFrameSequence_SpriteSheet_ParsesFramesList()
        {
            var sprite = new Sprite("sprite.xml")
            {
                TestMetaData = new Sprite.SpriteMeta { SourceType = "spritesheet", Frames = "0, 1,2" }
            };

            sprite.TestBuildFrameSequence();

            Assert.Equal(new[] { 0, 1, 2 }, sprite.Frames);
            Assert.Equal(3, sprite.FrameCount);
        }

        [Fact]
        public void BuildFrameSequence_SpriteSheet_SkipsUnparseableEntries()
        {
            var sprite = new Sprite("sprite.xml")
            {
                TestMetaData = new Sprite.SpriteMeta { SourceType = "spritesheet", Frames = "1,x,3" }
            };

            sprite.TestBuildFrameSequence();

            Assert.Equal(new[] { 1, 3 }, sprite.Frames);
        }

        [Fact]
        public void BuildFrameSequence_SpriteSheet_NoFrames_UsesExplicitFrame()
        {
            var sprite = new Sprite("sprite.xml")
            {
                TestMetaData = new Sprite.SpriteMeta { SourceType = "spritesheet", Frames = null, Frame = 4 }
            };

            sprite.TestBuildFrameSequence();

            Assert.Equal(new[] { 4 }, sprite.Frames);
        }

        [Fact]
        public void BuildFrameSequence_SpriteSheet_NoFramesNoFrame_UsesZero()
        {
            var sprite = new Sprite("sprite.xml")
            {
                TestMetaData = new Sprite.SpriteMeta { SourceType = "spritesheet", Frames = null, Frame = null }
            };

            sprite.TestBuildFrameSequence();

            Assert.Equal(new[] { 0 }, sprite.Frames);
        }

        [Fact]
        public void BuildFrameSequence_SetsFrameRateFromParsedValue()
        {
            var sprite = new Sprite("sprite.xml")
            {
                TestMetaData = new Sprite.SpriteMeta { SourceType = "spritesheet", Frames = "0", FrameRate = "20" }
            };

            sprite.TestBuildFrameSequence();

            Assert.Equal(1f / 20f, sprite.FrameRate, FrameRatePrecision);
        }

        // Number of decimal places to ignore when comparing computed frame-rate floats.
        private const int FrameRatePrecision = 5;
    }
}
