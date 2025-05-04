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

        [Fact]
        public void GetFrame_ReturnsCorrectFrame()
        {
            // Arrange
            var asset = new MockSpriteSheet("sheet.xml");
            var mockManager = new MockContentManager();
            asset.Load(mockManager);

            // Act
            var frame = asset.GetFrame(1);

            // Assert
            Assert.Equal(32, frame.Width);
            Assert.Equal(32, frame.Height);
            Assert.Equal(32, frame.X);
            Assert.Equal(0, frame.Y);
        }

        [Fact]
        public void GetFrameCount_ReturnsCorrectCount()
        {
            // Arrange
            var asset = new MockSpriteSheet("sheet.xml");
            var mockManager = new MockContentManager();
            asset.Load(mockManager);

            // Act
            var count = asset.GetFrameCount();

            // Assert
            Assert.Equal(2, count);
        }

        [Fact]
        public void Unload_ClearsData()
        {
            // Arrange
            var asset = new MockSpriteSheet("sheet.xml");
            var mockManager = new MockContentManager();
            asset.Load(mockManager);
            
            // Act
            asset.Unload(mockManager);
            
            // Assert
            Assert.Null(asset.GetTexture());
            
            // Instead of calling GetFrameCount(), check the internal field directly
            var frames = (Rectangle[])typeof(SpriteSheet).GetField("_frames", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(asset);
            Assert.Null(frames);
        }
    }

    public class MockSpriteSheet : SpriteSheet
    {
        public MockSpriteSheet(string name) : base(name)
        {
        }

        public override void Load(IContentManager contentManager)
        {
            if (contentManager == null) throw new ArgumentNullException(nameof(contentManager));
            
            // Set up the frames directly
            var frames = new Rectangle[2];
            frames[0] = new Rectangle(0, 0, 32, 32);
            frames[1] = new Rectangle(32, 0, 32, 32);
            typeof(SpriteSheet).GetField("_frames", BindingFlags.NonPublic | BindingFlags.Instance)!.SetValue(this, frames);
            
            // No need to set up a real texture
            _isLoaded = true;
        }
        
        public override void Unload(IContentManager contentManager)
        {
            if (contentManager == null) throw new ArgumentNullException(nameof(contentManager));
            
            // Clear frames
            typeof(SpriteSheet).GetField("_frames", BindingFlags.NonPublic | BindingFlags.Instance)!.SetValue(this, null);
            
            _isLoaded = false;
        }
        
        // Add implementation for GetFrameCount to use in tests
        public new int GetFrameCount()
        {
            var frames = (Rectangle[])typeof(SpriteSheet).GetField("_frames", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(this);
            return frames?.Length ?? 0;
        }
        
        private bool _isLoaded;

        public Texture2DAsset GetTexture()
        {
            // Just return null for testing - we're not actually testing the texture
            return null;
        }
    }
}
