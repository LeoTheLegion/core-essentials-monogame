using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Reflection;
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
            
            // For tests, we need a SpriteBatch where we can call both Begin and Draw
            // We'll use reflection to set internal fields to make it think it's properly initialized
            
            try
            {
                // Set a field to indicate that Begin has been called
                // The exact field name may vary by MonoGame version
                SetPrivateField(spriteBatch, "_beginCalled", true);
                
                // Some versions use _spriteEffect instead
                SetPrivateField(spriteBatch, "_spriteEffect", new BasicEffect(MockGraphicsDevice.CreateTestGraphicsDevice()));
            }
            catch (Exception)
            {
                // Ignore errors if we can't modify the fields
                // This method may need updates for different MonoGame versions
            }
            
            return spriteBatch;
        }
        
        /// <summary>
        /// Helper method to set private fields via reflection
        /// </summary>
        private static void SetPrivateField(object obj, string fieldName, object value)
        {
            try
            {
                var field = obj.GetType().GetField(fieldName, 
                    BindingFlags.NonPublic | BindingFlags.Instance);
                    
                if (field != null)
                {
                    field.SetValue(obj, value);
                }
            }
            catch
            {
                // Field not found or can't set, just ignore
            }
        }
    }
}