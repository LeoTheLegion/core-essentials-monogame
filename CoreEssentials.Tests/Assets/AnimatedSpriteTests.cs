using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using CoreEssentials.Assets;
using CoreEssentials.Debugging;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Moq;
using Xunit;

namespace CoreEssentials.Tests.Assets
{
    public class AnimatedSpriteTests : IDisposable
    {
        private readonly MockContentManager _mockContentManager;
        private readonly string _testAnimatedSpriteXmlPath = "testAnimatedSprite.xml";
        private readonly string _testSpriteSheetXmlPath = "testSpriteSheet.xml";
        private readonly string _testContentDir;
        private readonly string _animatedSpriteFullXmlPath;
        private readonly string _spriteSheetFullXmlPath;
        private readonly Mock<SpriteBatch> _mockSpriteBatch;
        
        // Constants for testing
        private const int TextureWidth = 300;
        private const int TextureHeight = 200;
        
        public AnimatedSpriteTests()
        {
            // Setup mock content manager
            _mockContentManager = new MockContentManager();
            
            // Setup mock SpriteBatch
            _mockSpriteBatch = new Mock<SpriteBatch>();
            
            // Setup Debug.Primitives replacement
            SetupDebugPrimitives();
            
            // Setup base directory for test files
            _testContentDir = Path.Combine(AppContext.BaseDirectory, "Content");
            Directory.CreateDirectory(_testContentDir);
            
            // Create test XML files
            _animatedSpriteFullXmlPath = Path.Combine(_testContentDir, _testAnimatedSpriteXmlPath);
            _spriteSheetFullXmlPath = Path.Combine(_testContentDir, _testSpriteSheetXmlPath);
            
            // Create sprite sheet XML content
            string spriteSheetXml = @"<?xml version=""1.0"" encoding=""utf-8""?>
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
            
            // Create animated sprite XML content
            string animatedSpriteXml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<AnimatedSpriteData xmlns=""http://schemas.coreessentials.monogame/2025/sprite"">
  <SourceType>spritesheet</SourceType>
  <Source>testSpriteSheet.xml</Source>
  <Size>
    <Width>64</Width>
    <Height>64</Height>
  </Size>
  <Frames>0,1,2,3,4,5</Frames>
  <FrameRate>8</FrameRate>
</AnimatedSpriteData>";
            
            // Write the XML files
            File.WriteAllText(_spriteSheetFullXmlPath, spriteSheetXml);
            File.WriteAllText(_animatedSpriteFullXmlPath, animatedSpriteXml);
            
            // Register test texture
            _mockContentManager.RegisterTestTexture("characterSheet", TextureWidth, TextureHeight);
            
            // Register the XML content as strings so AssetManager.LoadAsset<string> can find them
            _mockContentManager.RegisterMockAsset<string>(_testAnimatedSpriteXmlPath, animatedSpriteXml);
            _mockContentManager.RegisterMockAsset<string>(_testSpriteSheetXmlPath, spriteSheetXml);
            
            // Initialize AssetManager with our mock
            ResetAssetManagerState();
            AssetManager.Init(_mockContentManager);
        }
        
        public void Dispose()
        {
            // Clean up test files
            if (File.Exists(_animatedSpriteFullXmlPath))
                File.Delete(_animatedSpriteFullXmlPath);
                
            if (File.Exists(_spriteSheetFullXmlPath))
                File.Delete(_spriteSheetFullXmlPath);
                
            // Reset AssetManager to clean state
            ResetAssetManagerState();
        }
        
        private void SetupDebugPrimitives()
        {
            // Create a test implementation of the Debug.Primitives functionality
            var debugPrimitives = new TestDebugPrimitives();
            
            // Use reflection to get the Primitives property
            var debugType = typeof(Debug);
            var primitivesField = debugType.GetField("Primitives", 
                BindingFlags.Public | BindingFlags.Static | BindingFlags.NonPublic);
            
            if (primitivesField != null)
            {
                try {
                    primitivesField.SetValue(null, debugPrimitives);
                }
                catch (Exception ex) {
                    // If we can't set it, it's not critical - tests will still run
                    System.Console.WriteLine($"Warning: Could not set Debug.Primitives: {ex.Message}");
                }
            }
        }
        
        /// <summary>
        /// A simple implementation that fulfills the Debug.Primitives interface
        /// This avoids having to mock a potentially sealed or internal class
        /// </summary>
        private class TestDebugPrimitives
        {
            // Implement the DrawRectangle method that's used in AnimatedSprite
            public void DrawRectangle(SpriteBatch spriteBatch, Rectangle rectangle, Color color, float thickness)
            {
                // Do nothing - this is just a test implementation
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
            
            assetsDict?.Clear();
            countDict?.Clear();
        }
        
        [Fact]
        public void AnimatedSprite_Constructor_WithInvalidExtension_ThrowsException()
        {
            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => new AnimatedSprite("invalid_sprite_no_extension"));
        }
        
        [Fact]
        public void AnimatedSprite_Constructor_WithInvalidSourceType_ThrowsException()
        {
            // Arrange - Create XML content with invalid source type
            string invalidXmlPath = "invalid_source_type.xml";
            string invalidXml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<AnimatedSpriteData xmlns=""http://schemas.coreessentials.monogame/2025/sprite"">
  <SourceType>invalid</SourceType>
  <Source>testSpriteSheet.xml</Source>
  <Size>
    <Width>64</Width>
    <Height>64</Height>
  </Size>
  <Frames>0,1,2,3</Frames>
  <FrameRate>10</FrameRate>
</AnimatedSpriteData>";
            
            // Write the invalid XML to a physical file
            string invalidXmlFullPath = Path.Combine(_testContentDir, invalidXmlPath);
            File.WriteAllText(invalidXmlFullPath, invalidXml);
            
            // Register the XML content
            _mockContentManager.RegisterMockAsset<string>(invalidXmlPath, invalidXml);
            
            try {
                // Act & Assert
                Assert.Throws<InvalidOperationException>(() => new AnimatedSprite(invalidXmlPath));
            }
            finally {
                // Clean up
                if (File.Exists(invalidXmlFullPath))
                    File.Delete(invalidXmlFullPath);
            }
        }
        
        [Fact]
        public void AnimatedSprite_WithEmptyFrames_CreatesDefaultFrame()
        {
            // Arrange - Create XML content with empty frames
            string emptyFramesPath = "empty_frames.xml";
            string emptyFramesXml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<AnimatedSpriteData xmlns=""http://schemas.coreessentials.monogame/2025/sprite"">
  <SourceType>spritesheet</SourceType>
  <Source>testSpriteSheet.xml</Source>
  <Size>
    <Width>64</Width>
    <Height>64</Height>
  </Size>
  <Frames></Frames>
  <FrameRate>10</FrameRate>
</AnimatedSpriteData>";
            
            // Write the XML to a physical file
            string emptyFramesFullPath = Path.Combine(_testContentDir, emptyFramesPath);
            File.WriteAllText(emptyFramesFullPath, emptyFramesXml);
            
            // Register the XML content
            _mockContentManager.RegisterMockAsset<string>(emptyFramesPath, emptyFramesXml);
            
            try {
                // Act
                AnimatedSprite sprite = null;
                Exception exception = Record.Exception(() => { sprite = new AnimatedSprite(emptyFramesPath); });
                
                // Assert
                Assert.Null(exception); // No exception should be thrown
                Assert.NotNull(sprite);
                Assert.Equal(1, sprite.FrameCount); // Should have one default frame
            }
            finally {
                // Clean up
                if (File.Exists(emptyFramesFullPath))
                    File.Delete(emptyFramesFullPath);
            }
        }
        
        [Fact]
        public void AnimatedSprite_WithInvalidFrameRate_UsesDefaultFrameRate()
        {
            // Arrange - Create XML content with negative frame rate
            string negativeFrameRatePath = "negative_frame_rate.xml";
            string negativeFrameRateXml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<AnimatedSpriteData xmlns=""http://schemas.coreessentials.monogame/2025/sprite"">
  <SourceType>spritesheet</SourceType>
  <Source>testSpriteSheet.xml</Source>
  <Size>
    <Width>64</Width>
    <Height>64</Height>
  </Size>
  <Frames>0,1,2</Frames>
  <FrameRate>-5</FrameRate>
</AnimatedSpriteData>";
            
            // Write the XML to a physical file
            string negativeFrameRateFullPath = Path.Combine(_testContentDir, negativeFrameRatePath);
            File.WriteAllText(negativeFrameRateFullPath, negativeFrameRateXml);
            
            // Register the XML content
            _mockContentManager.RegisterMockAsset<string>(negativeFrameRatePath, negativeFrameRateXml);
            
            try {
                // Act
                AnimatedSprite sprite = null;
                Exception exception = Record.Exception(() => { sprite = new AnimatedSprite(negativeFrameRatePath); });
                
                // Assert
                Assert.Null(exception); // No exception should be thrown
                Assert.NotNull(sprite);
                Assert.Equal(0.1f, sprite.FrameRate, 0.001f); // Should use default frame rate (1/10 = 0.1 seconds per frame)
            }
            finally {
                // Clean up
                if (File.Exists(negativeFrameRateFullPath))
                    File.Delete(negativeFrameRateFullPath);
            }
        }
        
        [Fact(Skip = "Requires real GraphicsDevice")]
        public void AnimatedSprite_Constructor_LoadsXmlData()
        {
            // This test requires a real GraphicsDevice to load textures
        }
        
        [Fact(Skip = "Requires mocking of sealed Texture2D class")]
        public void DrawFrame_WithValidFrameIndex_DoesNotThrowException()
        {
            // This test would need a more complete mock of SpriteBatch and Texture2D
        }
        
        [Fact(Skip = "Requires mocking of sealed Texture2D class")]
        public void DrawFrame_WithInvalidFrameIndex_ThrowsException()
        {
            // This test would need a more complete mock of SpriteBatch and Texture2D
        }
    }
}