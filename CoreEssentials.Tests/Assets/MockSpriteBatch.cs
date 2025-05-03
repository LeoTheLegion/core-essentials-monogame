using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Runtime.Serialization;

namespace CoreEssentials.Tests.Assets
{
    /// <summary>
    /// A test wrapper for SpriteBatch that can be used in unit tests
    /// </summary>
    public class MockSpriteBatch
    {
        /// <summary>
        /// Creates a test substitute for SpriteBatch to use in tests
        /// </summary>
        public static SpriteBatch CreateTestSpriteBatch()
        {
            // Use FormatterServices to create an uninitialized object, bypassing constructors
            var spriteBatch = FormatterServices.GetUninitializedObject(typeof(SpriteBatch)) as SpriteBatch;
            
            // For tests, we just need an instance that doesn't throw exceptions when used
            // The actual drawing functionality won't be used in tests
            
            return spriteBatch;
        }
    }
}