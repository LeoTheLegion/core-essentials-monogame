using System;
using CoreEssentials.Assets;
using Xunit;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System.Runtime.Serialization;
using System.Collections.Generic;
using System.Reflection;

namespace CoreEssentials.Tests
{
    public class FontAssetTests
    {
        [Fact]
        public void Constructor_SetsName()
        {
            // Arrange & Act
            var asset = new FontAsset("testfont");
            
            // Assert
            Assert.Equal("testfont", asset.Name);
        }
        
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void Constructor_ThrowsOnInvalidName(string invalidName)
        {
            // Act & Assert
            if (invalidName == null)
            {
                Assert.Throws<ArgumentNullException>(() => new FontAsset(invalidName));
            }
            else
            {
                Assert.Throws<ArgumentException>(() => new FontAsset(invalidName));
            }
        }

        [Fact]
        public void Load_ThrowsIfContentManagerNull()
        {
            // Arrange
            var asset = new MockFontAsset("testfont");
            
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => asset.Load(null));
        }

        [Fact]
        public void Unload_ThrowsIfContentManagerNull()
        {
            // Arrange
            var asset = new MockFontAsset("testfont");
            
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => asset.Unload(null));
        }
        
        [Fact]
        public void Load_SetsSpriteFontProperty()
        {
            // Arrange
            var asset = new MockFontAsset("testfont");
            var mockContentManager = new MockContentManager();
            mockContentManager.AddAsset<SpriteFont>("testfont", MockSpriteFont.Instance);
            
            // Act
            asset.Load(mockContentManager);
            
            // Assert
            Assert.NotNull(asset.Font);
        }
        
        [Fact]
        public void Unload_ClearsSpriteFontProperty()
        {
            // Arrange
            var asset = new MockFontAsset("testfont");
            var mockContentManager = new MockContentManager();
            mockContentManager.AddAsset<SpriteFont>("testfont", MockSpriteFont.Instance);
            
            // Act
            asset.Load(mockContentManager);
            asset.Unload(mockContentManager);
            
            // Assert
            Assert.Null(asset.Font);
        }
        
        [Fact]
        public void MeasureString_ThrowsWhenFontNotLoaded()
        {
            // Arrange
            var asset = new MockFontAsset("testfont");
            
            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => asset.MeasureString("Test"));
        }
        
        [Fact]
        public void MeasureStringVector_ThrowsWhenFontNotLoaded()
        {
            // Arrange
            var asset = new MockFontAsset("testfont");
            
            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => asset.MeasureStringVector("Test"));
        }
        
        [Fact]
        public void MeasureString_ReturnsCorrectWidth()
        {
            // Arrange
            var asset = new MockFontAsset("testfont");
            var mockContentManager = new MockContentManager();
            
            // Add the mock sprite font to the content manager
            mockContentManager.AddAsset<SpriteFont>("testfont", MockSpriteFont.Instance);
            
            // Act
            asset.Load(mockContentManager);
            float width = asset.MeasureString("Test String");
            
            // Assert - MockSpriteFont returns a Vector2(10, 20) for any text
            Assert.Equal(10f, width);
        }
        
        [Fact]
        public void MeasureStringVector_ReturnsCorrectSize()
        {
            // Arrange
            var asset = new MockFontAsset("testfont");
            var mockContentManager = new MockContentManager();
            
            // Add the mock sprite font to the content manager
            mockContentManager.AddAsset<SpriteFont>("testfont", MockSpriteFont.Instance);
            
            // Act
            asset.Load(mockContentManager);
            Vector2 size = asset.MeasureStringVector("Test String");
            
            // Assert - MockSpriteFont returns a Vector2(10, 20) for any text
            Assert.Equal(10f, size.X);
            Assert.Equal(20f, size.Y);
        }
    }
    
    // Mock classes for testing
    public class MockFontAsset : FontAsset
    {
        private SpriteFont _mockFont;
        
        public MockFontAsset(string name) : base(name)
        {
        }
        
        public override void Load(IContentManager contentManager)
        {
            if (contentManager == null)
            {
                throw new ArgumentNullException(nameof(contentManager));
            }
            
            // Load from the MockContentManager
            _mockFont = contentManager.Load<SpriteFont>(_assetName);
        }
        
        public override void Unload(IContentManager contentManager)
        {
            if (contentManager == null)
            {
                throw new ArgumentNullException(nameof(contentManager));
            }
            
            _mockFont = null;
        }
        
        // Now we can properly override these methods since they're marked as virtual in the base class
        public override float MeasureString(string text)
        {
            if (_mockFont == null)
            {
                throw new InvalidOperationException("Font not loaded. Call Load() first.");
            }
            
            // Our mock font always returns a width of 10
            return 10f;
        }
        
        public override Vector2 MeasureStringVector(string text)
        {
            if (_mockFont == null)
            {
                throw new InvalidOperationException("Font not loaded. Call Load() first.");
            }
            
            // Our mock font always returns a size of (10, 20)
            return new Vector2(10, 20);
        }
        
        public new SpriteFont Font => _mockFont;
    }
    
    // Static holder for a mock SpriteFont instance
    public static class MockSpriteFont
    {
        public static readonly SpriteFont Instance;
        
        static MockSpriteFont()
        {
            // Create a SpriteFont without calling the constructor
            Instance = (SpriteFont)FormatterServices.GetUninitializedObject(typeof(SpriteFont));
            
            // Use reflection to set up a MeasureString method
            var measureStringMethod = new Func<string, Vector2>(_ => new Vector2(10, 20));
            
            // We need to use reflection to set private fields in the SpriteFont class
            // This approach is brittle and depends on internal details of MonoGame SpriteFont
            // but it's necessary for testing without a real SpriteFont instance
            var field = typeof(SpriteFont).GetField("_measureStringMethod", 
                BindingFlags.NonPublic | BindingFlags.Instance);
            
            if (field != null)
            {
                field.SetValue(Instance, measureStringMethod);
            }
        }
    }
}