using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace CoreEssentials.Tests.Assets
{
    /// <summary>
    /// A test replacement for Texture2D that can be used in unit tests
    /// </summary>
    public class TextureWrapper
    {
        // Static cache of wrapped textures to prevent duplicates
        private static readonly Dictionary<string, object> _cachedTextures = new Dictionary<string, object>();

        /// <summary>
        /// Width of the texture
        /// </summary>
        public int Width { get; }

        /// <summary>
        /// Height of the texture
        /// </summary>
        public int Height { get; }

        /// <summary>
        /// Creates a new TextureWrapper with the specified dimensions
        /// </summary>
        /// <param name="width">Width of the texture</param>
        /// <param name="height">Height of the texture</param>
        public TextureWrapper(int width, int height)
        {
            Width = width;
            Height = height;
        }

        /// <summary>
        /// Creates and returns a test substitute for Texture2D
        /// </summary>
        public static Texture2D CreateTestTexture(int width, int height)
        {
            // Create a unique key for this texture dimensions
            string key = $"texture_{width}x{height}";

            // Check if we've already created this texture
            if (_cachedTextures.TryGetValue(key, out var existingTexture))
            {
                return (Texture2D)existingTexture;
            }

            // For test purposes, create a minimal test double that only has Width and Height
            var wrapper = new TextureWrapper(width, height);
            
            // Use RuntimeHelpers to create an uninitialized object, bypassing constructors
            var texture = FormatterServices.GetUninitializedObject(typeof(Texture2D));

            // Set the Width/Height fields through reflection
            try
            {
                SetField(texture, "width", width);
                SetField(texture, "height", height);
                // Add other necessary fields to make it work in tests
                
                // Cache the created texture
                _cachedTextures[key] = texture;
                
                return (Texture2D)texture;
            }
            catch
            {
                // If reflection fails, return null for the test
                return null;
            }
        }

        // Helper to set private fields via reflection
        private static void SetField(object obj, string fieldName, object value)
        {
            // Try various naming conventions for private fields
            var fieldNames = new[]
            {
                fieldName,
                $"_{fieldName}",
                $"m_{fieldName}",
                fieldName.ToLower(),
                $"_{fieldName.ToLower()}",
                $"m_{fieldName.ToLower()}"
            };

            Type type = obj.GetType();
            foreach (var name in fieldNames)
            {
                var field = type.GetField(name, 
                    BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                
                if (field != null)
                {
                    field.SetValue(obj, value);
                    return;
                }
            }
        }
    }
}