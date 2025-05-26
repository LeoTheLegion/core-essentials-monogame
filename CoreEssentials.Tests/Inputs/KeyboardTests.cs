// CoreEssentials.Tests/Inputs/KeyboardTests.cs
using Xunit;
using CoreEssentials.Inputs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic; // For List<Keys>
using MonoGame.Extended.Input.InputListeners;

namespace CoreEssentials.Tests.Inputs
{
    // Mock KeyboardStateProvider for testing
    public class MockKeyboardStateProvider : IKeyboardStateProvider
    {
        private KeyboardState _currentKeyboardState;

        public MockKeyboardStateProvider()
        {
            // Initialize with an empty state (all keys up)
            _currentKeyboardState = new KeyboardState();
        }

        public void SetSimulatedState(params Keys[] pressedKeys)
        {
            _currentKeyboardState = new KeyboardState(pressedKeys);
        }

        public void SetSimulatedState(KeyboardState state)
        {
            _currentKeyboardState = state;
        }

        public KeyboardState GetState()
        {
            return _currentKeyboardState;
        }
    }

    public class KeyboardTests
    {
        private CoreEssentials.Inputs.Keyboard _keyboardWrapper;
        private MockKeyboardStateProvider _mockProvider;
        private GameTime _gameTime;

        public KeyboardTests()
        {
            _mockProvider = new MockKeyboardStateProvider();
            _keyboardWrapper = new CoreEssentials.Inputs.Keyboard(_mockProvider);
            _gameTime = new GameTime();
            // Initial update to set initial states within Keyboard wrapper
            _keyboardWrapper.Update(_gameTime);
        }

        [Fact]
        public void IsKeyDown_WhenProviderSaysKeyIsDown_ReturnsTrue()
        {
            _mockProvider.SetSimulatedState(Keys.A);
            _keyboardWrapper.Update(_gameTime);
            Assert.True(_keyboardWrapper.IsKeyDown(Keys.A));

            _mockProvider.SetSimulatedState(); // No keys pressed
            _keyboardWrapper.Update(_gameTime);
            Assert.False(_keyboardWrapper.IsKeyDown(Keys.A));
        }

        [Fact]
        public void IsKeyUp_WhenProviderSaysKeyIsUp_ReturnsTrue()
        {
            _mockProvider.SetSimulatedState(); // No keys pressed
            _keyboardWrapper.Update(_gameTime);
            Assert.True(_keyboardWrapper.IsKeyUp(Keys.A));

            _mockProvider.SetSimulatedState(Keys.A);
            _keyboardWrapper.Update(_gameTime);
            Assert.False(_keyboardWrapper.IsKeyUp(Keys.A));
        }

        [Fact]
        public void IsKeyPressedOnce_WhenKeyWasUpThenDown_ReturnsTrueAndThenFalse()
        {
            // Frame 1: Key is UP
            _mockProvider.SetSimulatedState();
            _keyboardWrapper.Update(_gameTime);
            Assert.False(_keyboardWrapper.IsKeyPressedOnce(Keys.Space), "Key should not be pressed once initially (Up -> Up)");

            // Frame 2: Key is PRESSED (Up -> Down)
            _mockProvider.SetSimulatedState(Keys.Space);
            _keyboardWrapper.Update(_gameTime);
            Assert.True(_keyboardWrapper.IsKeyPressedOnce(Keys.Space), "Key should be pressed once (Up -> Down)");

            // Frame 3: Key is HELD (Down -> Down)
            _mockProvider.SetSimulatedState(Keys.Space); // Keep it pressed
            _keyboardWrapper.Update(_gameTime);
            Assert.False(_keyboardWrapper.IsKeyPressedOnce(Keys.Space), "Key should not be pressed once when held (Down -> Down)");
            Assert.True(_keyboardWrapper.IsKeyDown(Keys.Space), "Key should still be considered down when held");

            // Frame 4: Key is RELEASED (Down -> Up)
            _mockProvider.SetSimulatedState();
            _keyboardWrapper.Update(_gameTime);
            Assert.False(_keyboardWrapper.IsKeyPressedOnce(Keys.Space), "Key should not be pressed once when released (Down -> Up)");
        }

        [Fact]
        public void IsKeyReleasedOnce_WhenKeyWasDownThenUp_ReturnsTrueAndThenFalse()
        {
            // Frame 1: Key is PRESSED (simulating it was pressed before this test sequence starts)
            _mockProvider.SetSimulatedState(Keys.Enter);
            _keyboardWrapper.Update(_gameTime); // This sets the initial _currentState
            _mockProvider.SetSimulatedState(Keys.Enter); // Keep it pressed for the "previous" state of the first check
            _keyboardWrapper.Update(_gameTime); // This sets _previousState = pressed, _currentState = pressed
            Assert.False(_keyboardWrapper.IsKeyReleasedOnce(Keys.Enter), "Key should not be released once initially (Down -> Down)");

            // Frame 2: Key is RELEASED (Down -> Up)
            _mockProvider.SetSimulatedState(); // Release the key
            _keyboardWrapper.Update(_gameTime);
            Assert.True(_keyboardWrapper.IsKeyReleasedOnce(Keys.Enter), "Key should be released once (Down -> Up)");

            // Frame 3: Key is STILL UP (Up -> Up)
            _mockProvider.SetSimulatedState(); // Keep it released
            _keyboardWrapper.Update(_gameTime);
            Assert.False(_keyboardWrapper.IsKeyReleasedOnce(Keys.Enter), "Key should not be released once when still up (Up -> Up)");
            Assert.True(_keyboardWrapper.IsKeyUp(Keys.Enter), "Key should still be considered up");
        }

        [Fact(Skip = "Event testing is unreliable as KeyboardListener uses global XNA state, not the mock provider.")]
        public void KeyPressedEvent_FiresWhenKeyIsPressed_Conceptual()
        {
            bool eventFired = false;
            Keys eventKey = Keys.None;
            _keyboardWrapper.KeyPressed += (sender, args) =>
            {
                eventFired = true;
                eventKey = args.Key;
            };

            // To test this, the actual Microsoft.Xna.Framework.Input.Keyboard.GetState()
            // would need to change between _keyboardWrapper.Update() calls, which our mock doesn't control
            // for the KeyboardListener's internal mechanism.

            // Conceptual simulation:
            // 1. _mockProvider state: B is UP. _keyboardWrapper.Update()
            // 2. _mockProvider state: B is DOWN. _keyboardWrapper.Update()
            // If KeyboardListener used our provider, event would fire.

            // This assertion will likely fail or be flaky.
            // Assert.True(eventFired, "KeyPressed event did not fire as expected.");
            // Assert.Equal(Keys.B, eventKey);
        }

        [Fact(Skip = "Event testing is unreliable as KeyboardListener uses global XNA state, not the mock provider.")]
        public void KeyReleasedEvent_FiresWhenKeyIsReleased_Conceptual()
        {
            bool eventFired = false;
            Keys eventKey = Keys.None;
            _keyboardWrapper.KeyReleased += (sender, args) =>
            {
                eventFired = true;
                eventKey = args.Key;
            };

            // Similar limitations to KeyPressedEvent test.

            // Assert.True(eventFired, "KeyReleased event did not fire as expected.");
        }
    }
}
