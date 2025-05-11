using CoreEssentials.Inputs;
using Microsoft.Xna.Framework;
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
            Assert.IsType<KeyboardListener>(Input.Keyboard);
            Assert.IsType<MouseListener>(Input.Mouse);
        }

        [Fact]
        public void Update_CallsUpdateOnAllListeners()
        {
            // Arrange - create mock GameTime
            var gameTime = new GameTime();

            // Use reflection to replace the input listeners with mocks
            var touchMock = new Mock<TouchListener>();
            var keyboardMock = new Mock<KeyboardListener>();
            var mouseMock = new Mock<MouseListener>();

            // Get field info for each listener using reflection
            var touchField = typeof(Input).GetProperty("Touch", BindingFlags.Public | BindingFlags.Static);
            var keyboardField = typeof(Input).GetProperty("Keyboard", BindingFlags.Public | BindingFlags.Static);
            var mouseField = typeof(Input).GetProperty("Mouse", BindingFlags.Public | BindingFlags.Static);

            // Store original listeners to restore later
            var originalTouch = Input.Touch;
            var originalKeyboard = Input.Keyboard;
            var originalMouse = Input.Mouse;

            try
            {
                // Replace listeners with mocks
                SetPropertyValue(touchField, null, touchMock.Object);
                SetPropertyValue(keyboardField, null, keyboardMock.Object);
                SetPropertyValue(mouseField, null, mouseMock.Object);

                // Act
                Input.Update(gameTime);

                // Assert - verify that Update was called on each listener
                touchMock.Verify(t => t.Update(gameTime), Times.Once);
                keyboardMock.Verify(k => k.Update(gameTime), Times.Once);
                mouseMock.Verify(m => m.Update(gameTime), Times.Once);
            }
            finally
            {
                // Restore original listeners
                SetPropertyValue(touchField, null, originalTouch);
                SetPropertyValue(keyboardField, null, originalKeyboard);
                SetPropertyValue(mouseField, null, originalMouse);
            }
        }

        // Helper method to set static properties via reflection
        private void SetPropertyValue(PropertyInfo prop, object obj, object value)
        {
            prop.SetValue(obj, value);
        }
    }
}
