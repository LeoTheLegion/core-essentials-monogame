using CoreEssentials.Inputs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input; // Added for KeyboardState
using MonoGame.Extended.Input.InputListeners;
using Moq;
using System;
using System.Reflection;
using Xunit;

namespace CoreEssentials.Tests.Inputs
{
    /// <summary>
    /// Tests for the Input class.
    /// </summary>
    public class InputTests
    {
        [Fact]
        public void Constructor_InitializesInputListeners()
        {
            // Act - Input static constructor is already called when we access the class

            // Assert - verify that all input listeners are initialized
            Assert.NotNull(Input.Touch);
            Assert.NotNull(Input.Keyboard);
            Assert.NotNull(Input.Mouse);
            
            // Verify types
            Assert.IsType<TouchListener>(Input.Touch);
            Assert.IsType<CoreEssentials.Inputs.Keyboard>(Input.Keyboard); // Fully qualified name
            Assert.IsType<CoreEssentials.Inputs.Mouse>(Input.Mouse); // Fully qualified name (wrapper, not raw MouseListener)
        }

        // Helper method to set static properties via reflection
        private void SetPropertyValue(PropertyInfo prop, object obj, object value)
        {
            prop.SetValue(obj, value);
        }
    }
}
