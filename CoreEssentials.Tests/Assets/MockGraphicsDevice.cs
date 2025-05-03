using Microsoft.Xna.Framework.Graphics;
using System.Runtime.Serialization;

namespace CoreEssentials.Tests.Assets
{
    /// <summary>
    /// A test wrapper for GraphicsDevice that can be used in unit tests
    /// </summary>
    public class MockGraphicsDevice
    {
        /// <summary>
        /// Creates a test substitute for GraphicsDevice to use in tests
        /// </summary>
        public static GraphicsDevice CreateTestGraphicsDevice()
        {
            // Use FormatterServices to create an uninitialized object, bypassing constructors
            var graphicsDevice = FormatterServices.GetUninitializedObject(typeof(GraphicsDevice)) as GraphicsDevice;
            
            // For tests, we just need an instance that doesn't throw exceptions when used
            // The actual functionality won't be exercised in tests
            
            return graphicsDevice;
        }
    }
}