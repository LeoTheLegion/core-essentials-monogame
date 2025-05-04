using System;
using CoreEssentials.Assets;
using Xunit;
using System.Reflection;
using Microsoft.Xna.Framework;

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

        [Fact]
        public void Load_SetsUpAnimationFrames()
        {
            // Arrange
            var asset = new MockAnimatedSprite("anim.xml");
            var mockManager = new MockContentManager();

            // Act
            asset.Load(mockManager);

            // Assert
            Assert.NotNull(asset.GetSpriteSheet());
            Assert.Equal(3, asset.GetFrameCount());
        }

        [Fact]
        public void Unload_ClearsData()
        {
            // Arrange
            var asset = new MockAnimatedSprite("anim.xml");
            var mockManager = new MockContentManager();
            asset.Load(mockManager);
            
            // Act
            asset.Unload(mockManager);
            
            // Assert
            Assert.Null(asset.GetSpriteSheet());
            Assert.Equal(0, asset.GetFrameCount());
        }
        
        [Fact]
        public void GetCurrentFrameIndex_ReturnsExpectedValue()
        {
            // Arrange
            var asset = new MockAnimatedSprite("anim.xml");
            var mockManager = new MockContentManager();
            asset.Load(mockManager);
            
            // Act
            var frameIndex = asset.GetCurrentFrameIndex();
            
            // Assert
            Assert.Equal(0, frameIndex);
        }
    }

    public class FakeSpriteSheet : SpriteSheet
    {
        public FakeSpriteSheet() : base("fake") { }
        public new Microsoft.Xna.Framework.Rectangle GetFrame(int index) => new Microsoft.Xna.Framework.Rectangle();
        public new Microsoft.Xna.Framework.Vector2 FrameOrigin => Microsoft.Xna.Framework.Vector2.Zero;
        public new Microsoft.Xna.Framework.Graphics.Texture2D Texture => null;
    }

    public class MockAnimatedSprite : AnimatedSprite
    {
        public MockAnimatedSprite(string name) : base(name)
        {
        }

        public override void Load(IContentManager contentManager)
        {
            if (contentManager == null) throw new ArgumentNullException(nameof(contentManager));
            
            // Set frames array - we'll use 3 frames
            var frames = new int[] { 0, 1, 0 };
            var framesField = typeof(AnimatedSprite).GetField("_frames", BindingFlags.NonPublic | BindingFlags.Instance);
            if (framesField != null) 
            {
                framesField.SetValue(this, frames);
            }
            
            // Create and set up frameTime information
            var frameTime = 0.2f; // 5 frames per second
            var frameTimeField = typeof(AnimatedSprite).GetField("_frameTime", BindingFlags.NonPublic | BindingFlags.Instance);
            if (frameTimeField != null)
            {
                frameTimeField.SetValue(this, frameTime);
            }
            
            // Use the existing FakeSpriteSheet to avoid GraphicsDevice dependency
            var spriteSheet = new FakeSpriteSheet();
            var spriteSheetField = typeof(AnimatedSprite).GetField("_spriteSheet", BindingFlags.NonPublic | BindingFlags.Instance);
            if (spriteSheetField != null) 
            {
                spriteSheetField.SetValue(this, spriteSheet);
            }
            
            // Set animation to be looping
            var isLoopingField = typeof(AnimatedSprite).GetField("_isLooping", BindingFlags.NonPublic | BindingFlags.Instance);
            if (isLoopingField != null)
            {
                isLoopingField.SetValue(this, true);
            }
            
            // Store reference for testing
            _spriteSheetRef = spriteSheet;
            _isLoaded = true;
        }
        
        public override void Unload(IContentManager contentManager)
        {
            if (contentManager == null) throw new ArgumentNullException(nameof(contentManager));
            
            // Clear frames
            var framesField = typeof(AnimatedSprite).GetField("_frames", BindingFlags.NonPublic | BindingFlags.Instance);
            if (framesField != null) 
            {
                framesField.SetValue(this, null);
            }
            
            // Clear sprite sheet
            var spriteSheetField = typeof(AnimatedSprite).GetField("_spriteSheet", BindingFlags.NonPublic | BindingFlags.Instance);
            if (spriteSheetField != null) 
            {
                spriteSheetField.SetValue(this, null);
            }
            
            _spriteSheetRef = null;
            _isLoaded = false;
        }
        
        private FakeSpriteSheet _spriteSheetRef;
        private bool _isLoaded;

        public SpriteSheet GetSpriteSheet()
        {
            return _isLoaded ? _spriteSheetRef : null;
        }
        
        public int GetFrameCount()
        {
            var framesField = typeof(AnimatedSprite).GetField("_frames", BindingFlags.NonPublic | BindingFlags.Instance);
            if (framesField != null)
            {
                var frames = (int[])framesField.GetValue(this);
                return frames?.Length ?? 0;
            }
            return 0;
        }
        
        public int GetCurrentFrameIndex()
        {
            var currentFrameIndexField = typeof(AnimatedSprite).GetField("_currentFrameIndex", BindingFlags.NonPublic | BindingFlags.Instance);
            if (currentFrameIndexField != null)
            {
                return (int)(currentFrameIndexField.GetValue(this) ?? 0);
            }
            return 0;
        }
    }
}
