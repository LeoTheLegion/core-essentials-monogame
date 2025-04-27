using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using CoreEssentials.Assets;
using Xunit;

namespace CoreEssentials.Tests.Assets
{
    /// <summary>
    /// Tests for handling XML content within the Asset system
    /// </summary>
    public class XmlContentTests : IDisposable
    {
        private readonly string _testXmlPath = "testContent.xml";
        private readonly string _fullXmlPath;
        
        public XmlContentTests()
        {
            // Setup test directory
            string testContentDir = Path.Combine(AppContext.BaseDirectory, "Content");
            Directory.CreateDirectory(testContentDir);
            
            // Create the test file
            _fullXmlPath = Path.Combine(testContentDir, _testXmlPath);
            
            // Create sample XML content
            string xmlContent = @"<?xml version=""1.0"" encoding=""utf-8""?>
<TestData xmlns=""http://schemas.coreessentials.monogame/2025/test"">
  <Value>Hello World</Value>
  <Number>42</Number>
</TestData>";
            
            File.WriteAllText(_fullXmlPath, xmlContent);
            
            // Reset AssetManager state
            ResetAssetManagerState();
        }
        
        public void Dispose()
        {
            if (File.Exists(_fullXmlPath))
            {
                File.Delete(_fullXmlPath);
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
        public void LoadAsset_XmlStringType_LoadsCorrectly()
        {
            // Act
            string xmlContent = AssetManager.LoadAsset<string>(_testXmlPath);
            
            // Assert
            Assert.NotNull(xmlContent);
            Assert.Contains("<TestData", xmlContent);
            Assert.Contains("<Value>Hello World</Value>", xmlContent);
            Assert.Contains("<Number>42</Number>", xmlContent);
        }
        
        [Fact]
        public void LoadAsset_XmlContent_IsCached()
        {
            // Act
            AssetManager.LoadAsset<string>(_testXmlPath);
            
            // Assert - verify it's cached
            Type assetManagerType = typeof(AssetManager);
            FieldInfo assetsLoadedField = assetManagerType.GetField("assetsLoaded", 
                BindingFlags.Static | BindingFlags.NonPublic);
            var assetsDict = (Dictionary<string, object>)assetsLoadedField.GetValue(null);
            
            Assert.True(assetsDict.ContainsKey($"{_testXmlPath}_String"));
        }
        
        [Fact]
        public void LoadAsset_XmlTwice_ReturnsSameInstance()
        {
            // Act
            var xml1 = AssetManager.LoadAsset<string>(_testXmlPath);
            var xml2 = AssetManager.LoadAsset<string>(_testXmlPath);
            
            // Assert
            Assert.Same(xml1, xml2); // Should be the same object instance
        }
        
        [Fact]
        public void UnloadAsset_XmlContent_RemovesFromCache()
        {
            // Arrange
            AssetManager.LoadAsset<string>(_testXmlPath);
            
            // Act
            AssetManager.UnloadAsset<string>(_testXmlPath);
            
            // Assert - verify it's no longer in the cache
            Type assetManagerType = typeof(AssetManager);
            FieldInfo assetsLoadedField = assetManagerType.GetField("assetsLoaded", 
                BindingFlags.Static | BindingFlags.NonPublic);
            var assetsDict = (Dictionary<string, object>)assetsLoadedField.GetValue(null);
            
            Assert.False(assetsDict.ContainsKey($"{_testXmlPath}_String"));
        }
    }
}