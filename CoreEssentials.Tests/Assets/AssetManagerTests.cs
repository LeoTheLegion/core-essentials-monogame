using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using CoreEssentials.Assets;
using CoreEssentials.Debugging;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Moq;
using Xunit;

namespace CoreEssentials.Tests.Assets
{
    public class AssetManagerTests : IDisposable
    {
        private readonly MockContentManager _mockContentManager;
        private readonly string _testTextAssetPath = "testTextAsset.txt";
        private readonly string _testAssetPath;
        
        public AssetManagerTests()
        {
            // Create the mock ContentManager
            _mockContentManager = new MockContentManager();
            
            // Mock Debug class to prevent drawing errors
            MockDebug();
            
            // Setup the base directory for test files
            string testContentDir = Path.Combine(AppContext.BaseDirectory, "Content");
            Directory.CreateDirectory(testContentDir);
            
            // Create a test file for string loading tests
            _testAssetPath = Path.Combine(testContentDir, _testTextAssetPath);
            File.WriteAllText(_testAssetPath, "Test content data");
            
            // Reset the AssetManager state before each test
            ResetAssetManagerState();
            
            // Initialize AssetManager with our mock
            AssetManager.Init(_mockContentManager);
        }
        
        public void Dispose()
        {
            // Clean up test files
            if (File.Exists(_testAssetPath))
            {
                File.Delete(_testAssetPath);
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
        
        [Fact(Skip = "Requires real GraphicsDevice")]
        public void LoadAsset_TextureType_CallsContentManager()
        {
            // This test requires a real GraphicsDevice to load textures
        }
        
        [Fact]
        public void LoadAsset_StringType_LoadsFromFile()
        {
            // Act
            var result = AssetManager.LoadAsset<string>(_testTextAssetPath);
            
            // Assert
            Assert.Equal("Test content data", result);
            
            // Verify asset is cached
            Type assetManagerType = typeof(AssetManager);
            FieldInfo assetsLoadedField = assetManagerType.GetField("assetsLoaded", 
                BindingFlags.Static | BindingFlags.NonPublic);
            var assetsDict = (Dictionary<string, object>)assetsLoadedField.GetValue(null);
            
            Assert.True(assetsDict.ContainsKey($"{_testTextAssetPath}_String"));
        }
        
        [Fact(Skip = "Requires real GraphicsDevice")]
        public void LoadAsset_SameAssetTwice_IncrementsReferenceCount()
        {
            // This test requires a real GraphicsDevice to load textures
        }
        
        [Fact(Skip = "Requires real GraphicsDevice")]
        public void UnloadAsset_LastReference_RemovesAsset()
        {
            // This test requires a real GraphicsDevice to load textures
        }
        
        [Fact(Skip = "Requires real GraphicsDevice")]
        public void UnloadAsset_MultipleReferences_DecrementsCount()
        {
            // This test requires a real GraphicsDevice to load textures
        }
        
        [Fact]
        public void LoadAsset_WithNullAssetName_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => AssetManager.LoadAsset<Texture2D>(null));
        }
        
        [Fact]
        public void LoadAsset_WithEmptyAssetName_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => AssetManager.LoadAsset<Texture2D>(""));
        }
    }
}