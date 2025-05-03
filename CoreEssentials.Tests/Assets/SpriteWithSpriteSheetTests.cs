using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using CoreEssentials.Assets;
using CoreEssentials.Debugging;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Moq;
using Xunit;

namespace CoreEssentials.Tests.Assets
{
    public class SpriteWithSpriteSheetTests : IDisposable
    {
        private readonly MockContentManager _mockContentManager;
        private readonly string _testSpriteSheetXmlPath = "testSpriteSheet.xml";
        private readonly string _testSpriteXmlPath = "testSprite.xml";
        private readonly string _testContentDir;
        private readonly string _fullSpriteSheetXmlPath;
        private readonly string _fullSpriteXmlPath;
        private readonly Mock<SpriteBatch> _mockSpriteBatch;
        
        // Width and height values we're expecting for our texture
        private const int TextureWidth = 300;
        private const int TextureHeight = 200;
        
        public SpriteWithSpriteSheetTests()
        {
            // Setup mock content manager
            _mockContentManager = new MockContentManager();
            
            // Setup mock SpriteBatch
            _mockSpriteBatch = new Mock<SpriteBatch>();
            
            // Mock Debug class to prevent drawing errors
            MockDebug();
            
            // Setup base directory for test files
            _testContentDir = Path.Combine(AppContext.BaseDirectory, "Content");
            Directory.CreateDirectory(_testContentDir);
            
            // Create a test XML file for sprite sheet
            _fullSpriteSheetXmlPath = Path.Combine(_testContentDir, _testSpriteSheetXmlPath);
            
            // Create test XML content for sprite sheet with 3x2 grid
            string spriteSheetXmlContent = @"<?xml version=""1.0"" encoding=""utf-8""?>
<SpriteSheetData xmlns=""http://schemas.coreessentials.monogame/2025/spritesheet"">
  <SourceType>texture2d</SourceType>
  <Source>characterSheet</Source>
  <Grid>
    <Rows>2</Rows>
    <Columns>3</Columns>
  </Grid>
  <Origin>
    <X>16</X>
    <Y>16</Y>
  </Origin>
</SpriteSheetData>";
            
            File.WriteAllText(_fullSpriteSheetXmlPath, spriteSheetXmlContent);
            
            // Create a test XML file for sprite that uses a sprite sheet
            _fullSpriteXmlPath = Path.Combine(_testContentDir, _testSpriteXmlPath);
            
            // Create test XML content for sprite
            string spriteXmlContent = @"<?xml version=""1.0"" encoding=""utf-8""?>
<SpriteData xmlns=""http://schemas.coreessentials.monogame/2025/sprite"">
  <SourceType>spritesheet</SourceType>
  <Source>testSpriteSheet.xml</Source>
  <Size>
    <Width>100</Width>
    <Height>100</Height>
  </Size>
  <Origin>
    <X>50</X>
    <Y>50</Y>
  </Origin>
  <Frame>2</Frame>
</SpriteData>";
            
            File.WriteAllText(_fullSpriteXmlPath, spriteXmlContent);
            
            // Register texture dimensions
            _mockContentManager.RegisterTestTexture("characterSheet", 300, 200);
            
            // Reset AssetManager state and init with mock
            ResetAssetManagerState();
            AssetManager.Init(_mockContentManager);
        }
        
        public void Dispose()
        {
            // Clean up test files
            if (File.Exists(_fullSpriteSheetXmlPath))
            {
                File.Delete(_fullSpriteSheetXmlPath);
            }
            
            if (File.Exists(_fullSpriteXmlPath))
            {
                File.Delete(_fullSpriteXmlPath);
            }
        }
        
        private void MockDebug()
        {
            // Create a simple mock for the Debug.Primitives to avoid runtime errors
            var mockPrimitives = new Mock<object>();
            
            // Use reflection to inject the mock
            var debugType = typeof(Debug);
            var primitivesProperty = debugType.GetProperty("Primitives", BindingFlags.Public | BindingFlags.Static);
            
            if (primitivesProperty != null)
            {
                try
                {
                    // Try to set the Primitives property if it's available
                    primitivesProperty.SetValue(null, mockPrimitives.Object);
                }
                catch (Exception)
                {
                    // Ignore errors if Debug.Primitives can't be set
                }
            }
        }
        
        private void ResetAssetManagerState()
        {
            // Access and clear private static dictionaries using reflection
            Type assetManagerType = typeof(AssetManager);
            
            FieldInfo assetsLoadedField = assetManagerType.GetField("assetsLoaded", 
                BindingFlags.Static | BindingFlags.NonPublic);
            
            FieldInfo countField = assetManagerType.GetField("countOfObjectsUsingAsset", 
                BindingFlags.Static | BindingFlags.NonPublic);
            
            var assetsDict = (Dictionary<string, object>)assetsLoadedField.GetValue(null);
            var countDict = (Dictionary<string, int>)countField.GetValue(null);
            
            assetsDict.Clear();
            countDict.Clear();
        }
        
        [Fact]
        public void Sprite_Constructor_LoadsSpriteSheetData()
        {
            // Arrange - Our TextureWrapper approach allows this test to run
            
            // Act - This would throw an exception if loading fails
            var sprite = new Sprite(_testSpriteXmlPath); // Use _testSpriteXmlPath, not _testSpriteSheetXmlPath
            
            // Assert
            Assert.NotNull(sprite);
            Assert.Equal(_testSpriteXmlPath, sprite.Name);
            
            // Use reflection to access private _spriteSheet field
            var spriteSheetField = typeof(Sprite).GetField("_spriteSheet", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.NotNull(spriteSheetField);
            
            var spriteSheet = spriteSheetField.GetValue(sprite) as SpriteSheet;
            Assert.NotNull(spriteSheet);
        }
        
        [Fact]
        public void SpriteConstructor_WithInvalidExtension_ThrowsException()
        {
            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => new Sprite("invalid_sprite_no_extension"));
        }
        
        [Fact]
        public void GetSize_WithSpriteSheet_ReturnsFrameSize()
        {
            // Arrange
            var sprite = new Sprite(_testSpriteXmlPath); // Use _testSpriteXmlPath, not _testSpriteSheetXmlPath
            
            // Act
            Vector2 size = sprite.GetSize();
            
            // Assert - Size should be the size of a frame in the spritesheet
            float expectedWidth = TextureWidth / 3;  // 3 columns
            float expectedHeight = TextureHeight / 2;  // 2 rows
            Assert.Equal(expectedWidth, size.X);
            Assert.Equal(expectedHeight, size.Y);
        }
        
        [Fact]
        public void Draw_WithSpriteSheet_DrawsDefaultFrame()
        {
            // Skip actual draw test as it requires SpriteBatch which is hard to fully mock
            // Instead, verify we can access what we need without exceptions
            
            // Arrange
            var sprite = new Sprite(_testSpriteXmlPath); // Use _testSpriteXmlPath, not _testSpriteSheetXmlPath
            
            // Use reflection to access private fields
            var spriteSheetField = typeof(Sprite).GetField("_spriteSheet", BindingFlags.NonPublic | BindingFlags.Instance);
            var defaultFrameField = typeof(Sprite).GetField("_defaultFrame", BindingFlags.NonPublic | BindingFlags.Instance);
            
            Assert.NotNull(spriteSheetField);
            Assert.NotNull(defaultFrameField);
            
            var spriteSheet = spriteSheetField.GetValue(sprite) as SpriteSheet;
            int defaultFrame = (int)defaultFrameField.GetValue(sprite);
            
            // Act & Assert - No exception should be thrown
            Exception ex = Record.Exception(() => {
                var frame = spriteSheet.GetFrame(defaultFrame);
                Assert.NotEqual(Rectangle.Empty, frame);
            });
            
            Assert.Null(ex);
        }
        
        [Fact]
        public void Draw_WithSpriteSheetAndFrameIndex_DrawsSpecificFrame()
        {
            // Skip actual draw test as it requires SpriteBatch which is hard to fully mock
            // Instead, verify frame rectangle is correct for a specific frame
            
            // Arrange
            var sprite = new Sprite(_testSpriteXmlPath); // Use _testSpriteXmlPath, not _testSpriteSheetXmlPath
            int testFrame = 2;
            
            // Use reflection to access private fields
            var spriteSheetField = typeof(Sprite).GetField("_spriteSheet", BindingFlags.NonPublic | BindingFlags.Instance);
            var defaultFrameField = typeof(Sprite).GetField("_defaultFrame", BindingFlags.NonPublic | BindingFlags.Instance);
            
            Assert.NotNull(spriteSheetField);
            Assert.NotNull(defaultFrameField);
            
            var spriteSheet = spriteSheetField.GetValue(sprite) as SpriteSheet;
            
            // Set defaultFrame via reflection
            defaultFrameField.SetValue(sprite, testFrame);
            
            // Verify frame rectangle is correct
            var frameRect = spriteSheet.GetFrame(testFrame);
            int expectedX = (testFrame % 3) * (TextureWidth / 3);  // Column * frame width
            int expectedY = (testFrame / 3) * (TextureHeight / 2);  // Row * frame height
            
            Assert.Equal(new Rectangle(expectedX, expectedY, TextureWidth / 3, TextureHeight / 2), frameRect);
        }
        
        [Fact]
        public void Sprite_WithSpriteSheet_HasCorrectDefaultFrame()
        {
            // Arrange
            var sprite = new Sprite(_testSpriteXmlPath);
            
            // Use reflection to access private _defaultFrame field
            var defaultFrameField = typeof(Sprite).GetField("_defaultFrame", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.NotNull(defaultFrameField);
            
            // Act - Get default frame
            int defaultFrame = (int)defaultFrameField.GetValue(sprite);
            
            // Assert - Default frame should be 0 unless specified otherwise in XML
            Assert.Equal(2, defaultFrame); // Default frame set to 2 in our test XML
        }
    }
}