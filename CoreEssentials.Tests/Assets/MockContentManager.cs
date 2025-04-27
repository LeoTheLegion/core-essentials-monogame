using System;
using System.Collections.Generic;
using System.Reflection;
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
        private readonly Dictionary<string, TextureDimensions> _textureDimensions = new Dictionary<string, TextureDimensions>();
        
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
            
            // Register some default texture dimensions
            _textureDimensions["default_texture"] = new TextureDimensions(100, 100);
            _textureDimensions["ball"] = new TextureDimensions(64, 64);
            _textureDimensions["characterSheet"] = new TextureDimensions(300, 200);
        }
        
        /// <summary>
        /// Register a mock asset to be returned by this ContentManager
        /// </summary>
        public void RegisterMockAsset<T>(string assetName, T asset)
        {
            _mockAssets[assetName] = asset;
        }
        
        /// <summary>
        /// Register dimensions for a texture asset
        /// </summary>
        public void RegisterTestTexture(string assetName, int width, int height)
        {
            _textureDimensions[assetName] = new TextureDimensions(width, height);
        }
        
        /// <summary>
        /// Get dimensions for a registered texture
        /// </summary>
        public TextureDimensions GetTextureDimensions(string assetName)
        {
            if (_textureDimensions.TryGetValue(assetName, out var dimensions))
            {
                return dimensions;
            }
            return new TextureDimensions(100, 100); // Default size
        }
        
        /// <summary>
        /// Override Load to return dummy objects instead of loading from disk
        /// </summary>
        public override T Load<T>(string assetName)
        {
            // Special handling for Texture2D - we can't create these without a GraphicsDevice
            if (typeof(T) == typeof(Texture2D))
            {
                // Instead of trying to create a real Texture2D, return a mock object
                var mock = new Mock<Texture2D>();
                
                // Unfortunately we can't mock Width/Height as they're not virtual
                // The SpriteSheet class will need to be modified for testability
                
                return (T)(object)mock.Object;
            }

            if (_mockAssets.TryGetValue(assetName, out var mockAsset))
            {
                // If we have a direct conversion, use it
                if (mockAsset is T typedAsset)
                {
                    return typedAsset;
                }
                
                // If we stored a Type marker, create a special dummy object
                if (mockAsset is Type && typeof(T).IsAssignableFrom((Type)mockAsset))
                {
                    // For basic types, return default
                    return default;
                }
            }
            
            // For non-registered assets, try to create a mock
            if (typeof(T).IsInterface || typeof(T).IsAbstract)
            {
                var mockType = typeof(Mock<>).MakeGenericType(typeof(T));
                var mock = Activator.CreateInstance(mockType);
                var objectProperty = mockType.GetProperty("Object");
                return (T)objectProperty.GetValue(mock);
            }
            
            // Try to create an instance using default constructor
            try
            {
                return Activator.CreateInstance<T>();
            }
            catch
            {
                // If all else fails
                throw new ContentLoadException($"No mock registered for asset '{assetName}' of type {typeof(T).Name}");
            }
        }
        
        /// <summary>
        /// Override Unload to handle our mock assets
        /// </summary>
        public override void Unload()
        {
            _mockAssets.Clear();
            _textureDimensions.Clear();
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
            
            if (_textureDimensions.ContainsKey(assetName))
            {
                _textureDimensions.Remove(assetName);
            }
        }
    }
    
    /// <summary>
    /// Simple structure to store texture dimensions
    /// </summary>
    public struct TextureDimensions
    {
        public int Width { get; }
        public int Height { get; }
        
        public TextureDimensions(int width, int height)
        {
            Width = width;
            Height = height;
        }
    }
}