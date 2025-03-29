using System;
using System.IO;
using System.Reflection;
using Xunit;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using CoreEssentials.Assets;

namespace CoreEssentials.Tests.Assets
{
    public class AssetManagerGameTests : IDisposable
    {
        private readonly Game1 _game;
        
        public AssetManagerGameTests()
        {
            // Initialize the standard game class
            _game = new Game1();
            
            // Initialize AssetManager with the game's content manager
            AssetManager.Init(_game.Content);
            
            // Clear asset manager state between tests
            ClearAssetManagerState();
        }

        public void Dispose()
        {
            _game.Dispose();
        }
        
        private void ClearAssetManagerState()
        {
            // Reset the static dictionaries using reflection
            var assetsLoadedField = typeof(AssetManager).GetField("assetsLoaded", 
                BindingFlags.NonPublic | BindingFlags.Static);
            var countOfObjectsField = typeof(AssetManager).GetField("countOfObjectsUsingAsset", 
                BindingFlags.NonPublic | BindingFlags.Static);
            
            assetsLoadedField.SetValue(null, new System.Collections.Generic.Dictionary<string, object>());
            countOfObjectsField.SetValue(null, new System.Collections.Generic.Dictionary<string, int>());
        }

        [Fact]
        public void LoadAsset_XMMP_File_LoadsTextContent()
        {
            // Arrange
            string testFilePath = Path.Combine(AppContext.BaseDirectory, "Content", "test.xmmp");
            string testContent = "This is test XMMP file content";
            
            // Ensure directory exists
            Directory.CreateDirectory(Path.GetDirectoryName(testFilePath));
            
            // Create test file
            File.WriteAllText(testFilePath, testContent);
            
            try
            {
                // Act
                string result = AssetManager.LoadAsset<string>("test.xmmp");
                
                // Assert
                Assert.Equal(testContent, result);
            }
            finally
            {
                // Clean up
                if (File.Exists(testFilePath))
                {
                    File.Delete(testFilePath);
                }
            }
        }
        
        [Fact]
        public void LoadAsset_SameAssetMultipleTimes_IncreasesReferenceCount()
        {
            // Arrange - Create a test file in Content folder
            string testFilePath = Path.Combine(AppContext.BaseDirectory, "Content", "test.xmmp");
            File.WriteAllText(testFilePath, "Test content");
            
            try
            {
                // Act
                AssetManager.LoadAsset<string>("test.xmmp");
                AssetManager.LoadAsset<string>("test.xmmp");
                AssetManager.LoadAsset<string>("test.xmmp");
                
                // Get reference count via reflection
                var countOfObjectsField = typeof(AssetManager).GetField("countOfObjectsUsingAsset", 
                    BindingFlags.NonPublic | BindingFlags.Static);
                var counts = (System.Collections.Generic.Dictionary<string, int>)countOfObjectsField.GetValue(null);
                
                // Assert
                Assert.Equal(3, counts["test.xmmp"]);
            }
            finally
            {
                // Clean up
                if (File.Exists(testFilePath))
                {
                    File.Delete(testFilePath);
                }
            }
        }
        
        [Fact]
        public void UnloadAsset_DecreasesReferenceCount()
        {
            // Arrange - Create a test file in Content folder
            string testFilePath = Path.Combine(AppContext.BaseDirectory, "Content", "test.xmmp");
            File.WriteAllText(testFilePath, "Test content");
            
            try
            {
                // Load multiple times
                AssetManager.LoadAsset<string>("test.xmmp");
                AssetManager.LoadAsset<string>("test.xmmp");
                
                // Act
                AssetManager.UnloadAsset<string>("test.xmmp");
                
                // Get reference count via reflection
                var countOfObjectsField = typeof(AssetManager).GetField("countOfObjectsUsingAsset", 
                    BindingFlags.NonPublic | BindingFlags.Static);
                var counts = (System.Collections.Generic.Dictionary<string, int>)countOfObjectsField.GetValue(null);
                
                // Assert
                Assert.Equal(1, counts["test.xmmp"]);
            }
            finally
            {
                // Clean up
                if (File.Exists(testFilePath))
                {
                    File.Delete(testFilePath);
                }
            }
        }
        
        [Fact]
        public void LoadAsset_WithInvalidXMMPType_ThrowsException()
        {
            // Arrange
            string testFilePath = Path.Combine(AppContext.BaseDirectory, "Content", "test.xmmp");
            File.WriteAllText(testFilePath, "Test content");
            
            try
            {
                // Act & Assert
                Assert.Throws<InvalidOperationException>(() => 
                    AssetManager.LoadAsset<Texture2D>("test.xmmp"));
            }
            finally
            {
                // Clean up
                if (File.Exists(testFilePath))
                {
                    File.Delete(testFilePath);
                }
            }
        }
    }
}
