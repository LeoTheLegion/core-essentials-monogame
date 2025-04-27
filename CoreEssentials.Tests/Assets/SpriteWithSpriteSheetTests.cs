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
        
        [Fact(Skip = "Requires mocking of sealed Texture2D class")]
        public void Sprite_Constructor_LoadsSpriteSheetData()
        {
            // Skip - requires a real texture
        }
        
        [Fact]
        public void SpriteConstructor_WithInvalidExtension_ThrowsException()
        {
            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => new Sprite("invalid_sprite_no_extension"));
        }
        
        [Fact(Skip = "Requires mocking of sealed Texture2D class")]
        public void GetSize_WithSpriteSheet_ReturnsFrameSize()
        {
            // Skip - requires a real texture
        }
        
        [Fact(Skip = "Requires mocking of sealed Texture2D class")]
        public void Draw_WithSpriteSheet_DrawsDefaultFrame()
        {
            // Skip - requires a real texture
        }
        
        [Fact(Skip = "Requires mocking of sealed Texture2D class")]
        public void Draw_WithSpriteSheetAndFrameIndex_DrawsSpecificFrame()
        {
            // Skip - requires a real texture
        }
        
        [Fact(Skip = "Requires mocking of sealed Texture2D class")]
        public void Sprite_WithSpriteSheet_HasCorrectDefaultFrame()
        {
            // Skip - requires a real texture
        }
    }
}