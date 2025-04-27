using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Moq;

namespace CoreEssentials.Tests.Assets
{
    /// <summary>
    /// A simplified test implementation of ContentManager that returns mock assets
    /// </summary>
    public class MockContentManager : ContentManager
    {
        private readonly Dictionary<string, object> _mockAssets = new Dictionary<string, object>();
        
        public MockContentManager() : base(CreateMockServiceProvider())
        {
            // Initialize with some common mock assets
            SetupDefaultMocks();
        }
        
        /// <summary>
        /// Creates a mock service provider that doesn't try to mock GraphicsDevice
        /// </summary>
        private static IServiceProvider CreateMockServiceProvider()
        {
            var mockServiceProvider = new Mock<IServiceProvider>();
            
            // Return null for any service - we'll skip the parts that need actual services
            mockServiceProvider
                .Setup(sp => sp.GetService(It.IsAny<Type>()))
                .Returns(null);
            
            return mockServiceProvider.Object;
        }
        
        private void SetupDefaultMocks()
        {
            // Instead of creating mock Texture2D objects (which require GraphicsDevice),
            // we just store type information and handle it specially in Load<T>
            _mockAssets.Add("default_texture", typeof(Texture2D));
            _mockAssets.Add("ball", typeof(Texture2D));
        }
        
        /// <summary>
        /// Register a mock asset to be returned by this ContentManager
        /// </summary>
        public void RegisterMockAsset<T>(string assetName, T asset)
        {
            _mockAssets[assetName] = asset;
        }
        
        /// <summary>
        /// Override Load to return dummy objects instead of loading from disk
        /// </summary>
        public override T Load<T>(string assetName)
        {
            // Special handling for different types since we can't mock them all
            if (typeof(T) == typeof(Texture2D))
            {
                // For Texture2D, return a dummy object - we'll just check for null in tests
                return (T)(object)null;
            }

            if (_mockAssets.TryGetValue(assetName, out var mockAsset))
            {
                // If we have a direct conversion, use it
                if (mockAsset is T typedAsset)
                {
                    return typedAsset;
                }
                
                // If we stored a Type marker, create a special dummy object
                if (mockAsset is Type type && type == typeof(T))
                {
                    return default;
                }
            }
            
            throw new ContentLoadException($"No mock registered for asset '{assetName}' of type {typeof(T).Name}");
        }
        
        /// <summary>
        /// Override Unload to handle our mock assets
        /// </summary>
        public override void Unload()
        {
            _mockAssets.Clear();
            SetupDefaultMocks();
        }
        
        /// <summary>
        /// Simulates unloading a specific asset
        /// </summary>
        public new void UnloadAsset(string assetName)
        {
            if (_mockAssets.ContainsKey(assetName))
            {
                _mockAssets.Remove(assetName);
            }
        }
    }
}