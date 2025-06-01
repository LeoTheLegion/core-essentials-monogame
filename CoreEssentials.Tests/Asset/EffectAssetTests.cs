using System;
using CoreEssentials.Assets;
using Xunit;
using Microsoft.Xna.Framework.Graphics; // Required for Effect
using System.Runtime.Serialization; // Required for FormatterServices

namespace CoreEssentials.Tests.Asset
{
    public class EffectAssetTests
    {
        private const string TestEffectName = "TestEffect";

        [Fact]
        public void Constructor_SetsNameCorrectly()
        {
            // Arrange & Act
            var effectAsset = new EffectAsset(TestEffectName);

            // Assert
            Assert.Equal(TestEffectName, effectAsset.Name);
        }

        [Fact]
        public void Load_NullContentManager_ThrowsArgumentNullException()
        {
            // Arrange
            var effectAsset = new EffectAsset(TestEffectName);

            // Act & Assert
            Assert.Throws<ArgumentNullException>("contentManager", () => effectAsset.Load(null));
        }

        [Fact]
        public void Unload_NullContentManager_ThrowsArgumentNullException()
        {
            // Arrange
            var effectAsset = new EffectAsset(TestEffectName);
            // To test Unload, the asset might need to be "loaded" first,
            // or the Unload method designed to handle an uninitialized effect.
            // For this basic test, we assume Unload can be called.
            // If Load must be called first, this test needs a mock content manager.

            // Act & Assert
            Assert.Throws<ArgumentNullException>("contentManager", () => effectAsset.Unload(null));
        }

        [Fact]
        public void Load_ValidContentManager_SetsEffectProperty()
        {
            // Arrange
            var mockContentManager = new MockContentManager();
            var effectAsset = new EffectAsset(TestEffectName);

            // Act
            effectAsset.Load(mockContentManager);

            // Assert
            Assert.NotNull(effectAsset.Effect);
            Assert.True(mockContentManager.IsLoaded(TestEffectName));
        }

        [Fact]
        public void Unload_ValidContentManager_ClearsEffectProperty()
        {
            // Arrange
            var mockContentManager = new MockContentManager();
            var effectAsset = new EffectAsset(TestEffectName);
            effectAsset.Load(mockContentManager); // Load first

            // Act
            effectAsset.Unload(mockContentManager);

            // Assert
            Assert.Null(effectAsset.Effect);
            Assert.True(mockContentManager.IsUnloaded(TestEffectName));
        }
    }

    // Mock IContentManager for testing purposes
    public class MockContentManager : IContentManager
    {
        private readonly System.Collections.Generic.Dictionary<string, object> _loadedAssets = new();
        private readonly System.Collections.Generic.HashSet<string> _unloadedAssets = new();

        public T Load<T>(string assetName)
        {
            _loadedAssets[assetName] = typeof(T) == typeof(Effect) ? FakeEffect.Instance : default(T);
            return (T)_loadedAssets[assetName];
        }

        public void Unload(string assetName)
        {
            if (_loadedAssets.ContainsKey(assetName))
            {
                _loadedAssets.Remove(assetName);
                _unloadedAssets.Add(assetName);
            }
        }

        public bool IsLoaded(string assetName) => _loadedAssets.ContainsKey(assetName);
        public bool IsUnloaded(string assetName) => _unloadedAssets.Contains(assetName);
    }

    // A static holder for a fake Effect instance that doesn't actually create a real Effect
    // This avoids GraphicsDevice requirements completely for unit testing.
    public static class FakeEffect
    {
        public static readonly Effect Instance;

        static FakeEffect()
        {
            // Use FormatterServices.GetUninitializedObject to create an Effect without calling its constructor.
            // This is a special case for testing only, not for production code.
            // Requires System.Runtime.Serialization.
            Instance = (Effect)FormatterServices.GetUninitializedObject(typeof(Effect));
        }
    }
}
