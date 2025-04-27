using System;
using CoreEssentials.Assets;
using Xunit;
using Moq;

namespace CoreEssentials.Tests.Assets
{
    public class AssetTests
    {
        private class TestAsset : Asset
        {
            public TestAsset(string name) : base(name)
            {
            }
        }
        
        [Fact]
        public void Asset_Constructor_SetsName()
        {
            // Arrange
            string testName = "test_asset";
            
            // Act
            var asset = new TestAsset(testName);
            
            // Assert
            Assert.Equal(testName, asset.Name);
        }
        
        [Fact]
        public void Asset_Constructor_WithNullName_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new TestAsset(null));
        }
        
        [Fact]
        public void Asset_Constructor_WithEmptyName_ThrowsArgumentException()
        {
            // Act & Assert
            // The string.IsNullOrEmpty check in Asset will match an empty string after checking for null
            Assert.Throws<ArgumentException>(() => new TestAsset(string.Empty));
        }
    }
}