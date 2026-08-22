// CoreEssentials.Tests/Inputs/MouseTests.cs
using System;
using Xunit;
using CoreEssentials.Inputs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace CoreEssentials.Tests.Inputs
{
    /// <summary>
    /// Mock mouse state provider for testing the Mouse wrapper without real hardware input.
    /// </summary>
    public class MockMouseStateProvider : IMouseStateProvider
    {
        private MouseState _currentMouseState;

        public MockMouseStateProvider()
        {
            // Initialize with a neutral state (all buttons up, position 0,0)
            _currentMouseState = new MouseState();
        }

        /// <summary>
        /// Sets the simulated mouse state.
        /// </summary>
        public void SetSimulatedState(MouseState state)
        {
            _currentMouseState = state;
        }

        /// <summary>
        /// Creates a simulated mouse state with the given position and button states.
        /// MonoGame's MouseState only exposes a parameterless and an 8-argument constructor
        /// (x, y, scrollWheelValue, then ButtonState per button), so this helper keeps test
        /// call sites terse.
        /// </summary>
        public static MouseState CreateState(
            int x = 0, int y = 0,
            bool leftButton = false, bool rightButton = false, bool middleButton = false,
            int scrollWheelValue = 0, bool xButton1 = false, bool xButton2 = false)
        {
            return new MouseState(
                x, y, scrollWheelValue,
                leftButton ? ButtonState.Pressed : ButtonState.Released,
                rightButton ? ButtonState.Pressed : ButtonState.Released,
                middleButton ? ButtonState.Pressed : ButtonState.Released,
                xButton1 ? ButtonState.Pressed : ButtonState.Released,
                xButton2 ? ButtonState.Pressed : ButtonState.Released);
        }

        public MouseState GetState()
        {
            return _currentMouseState;
        }
    }

    /// <summary>
    /// Tests for the CoreEssentials.Inputs.Mouse wrapper.
    /// </summary>
    public class MouseTests
    {
        private CoreEssentials.Inputs.Mouse _mouseWrapper;
        private MockMouseStateProvider _mockProvider;
        private GameTime _gameTime;

        public MouseTests()
        {
            _mockProvider = new MockMouseStateProvider();
            _mouseWrapper = new CoreEssentials.Inputs.Mouse(_mockProvider);
            _gameTime = new GameTime();
            // Initial update to set initial states within the Mouse wrapper
            _mouseWrapper.Update(_gameTime);
        }

        [Fact]
        public void IsButtonDown_WhenProviderSaysButtonIsDown_ReturnsTrue()
        {
            _mockProvider.SetSimulatedState(MockMouseStateProvider.CreateState(leftButton: true));
            _mouseWrapper.Update(_gameTime);
            Assert.True(_mouseWrapper.IsButtonDown(MouseButton.Left));

            _mockProvider.SetSimulatedState(new MouseState());
            _mouseWrapper.Update(_gameTime);
            Assert.False(_mouseWrapper.IsButtonDown(MouseButton.Left));
        }

        [Fact]
        public void IsButtonDown_ReportsEachButtonIndependently()
        {
            _mockProvider.SetSimulatedState(MockMouseStateProvider.CreateState(rightButton: true, middleButton: true));
            _mouseWrapper.Update(_gameTime);

            Assert.True(_mouseWrapper.IsButtonDown(MouseButton.Right));
            Assert.True(_mouseWrapper.IsButtonDown(MouseButton.Middle));
            Assert.False(_mouseWrapper.IsButtonDown(MouseButton.Left));
            Assert.False(_mouseWrapper.IsButtonDown(MouseButton.XButton1));
        }

        [Fact]
        public void IsButtonUp_WhenProviderSaysButtonIsUp_ReturnsTrue()
        {
            _mockProvider.SetSimulatedState(MockMouseStateProvider.CreateState());
            _mouseWrapper.Update(_gameTime);
            Assert.True(_mouseWrapper.IsButtonUp(MouseButton.Left));

            _mockProvider.SetSimulatedState(MockMouseStateProvider.CreateState(leftButton: true));
            _mouseWrapper.Update(_gameTime);
            Assert.False(_mouseWrapper.IsButtonUp(MouseButton.Left));
        }

        [Fact]
        public void IsButtonPressedOnce_WhenButtonWasUpThenDown_ReturnsTrueAndThenFalse()
        {
            // Frame 1: Button is UP
            _mockProvider.SetSimulatedState(MockMouseStateProvider.CreateState());
            _mouseWrapper.Update(_gameTime);
            Assert.False(_mouseWrapper.IsButtonPressedOnce(MouseButton.Left), "Button should not be pressed once initially (Up -> Up)");

            // Frame 2: Button is PRESSED (Up -> Down)
            _mockProvider.SetSimulatedState(MockMouseStateProvider.CreateState(leftButton: true));
            _mouseWrapper.Update(_gameTime);
            Assert.True(_mouseWrapper.IsButtonPressedOnce(MouseButton.Left), "Button should be pressed once (Up -> Down)");

            // Frame 3: Button is HELD (Down -> Down)
            _mockProvider.SetSimulatedState(MockMouseStateProvider.CreateState(leftButton: true));
            _mouseWrapper.Update(_gameTime);
            Assert.False(_mouseWrapper.IsButtonPressedOnce(MouseButton.Left), "Button should not be pressed once when held (Down -> Down)");
            Assert.True(_mouseWrapper.IsButtonDown(MouseButton.Left), "Button should still be considered down when held");

            // Frame 4: Button is RELEASED (Down -> Up)
            _mockProvider.SetSimulatedState(MockMouseStateProvider.CreateState());
            _mouseWrapper.Update(_gameTime);
            Assert.False(_mouseWrapper.IsButtonPressedOnce(MouseButton.Left), "Button should not be pressed once when released (Down -> Up)");
        }

        [Fact]
        public void IsButtonReleasedOnce_WhenButtonWasDownThenUp_ReturnsTrueAndThenFalse()
        {
            // Frame 1: Button is PRESSED
            _mockProvider.SetSimulatedState(MockMouseStateProvider.CreateState(leftButton: true));
            _mouseWrapper.Update(_gameTime);

            // Frame 2: Button is RELEASED (Down -> Up)
            _mockProvider.SetSimulatedState(MockMouseStateProvider.CreateState());
            _mouseWrapper.Update(_gameTime);
            Assert.True(_mouseWrapper.IsButtonReleasedOnce(MouseButton.Left), "Button should be released once (Down -> Up)");

            // Frame 3: Button is STILL UP (Up -> Up)
            _mockProvider.SetSimulatedState(MockMouseStateProvider.CreateState());
            _mouseWrapper.Update(_gameTime);
            Assert.False(_mouseWrapper.IsButtonReleasedOnce(MouseButton.Left), "Button should not be released once when still up (Up -> Up)");
            Assert.True(_mouseWrapper.IsButtonUp(MouseButton.Left), "Button should still be considered up");
        }

        [Fact]
        public void Position_ReturnsCurrentMousePositionAsVector2()
        {
            _mockProvider.SetSimulatedState(MockMouseStateProvider.CreateState(x: 123, y: 456));
            _mouseWrapper.Update(_gameTime);

            Vector2 position = _mouseWrapper.Position;
            Assert.Equal(new Vector2(123f, 456f), position);
        }

        [Fact]
        public void Position_IsViewportIndependent()
        {
            // Position should be the raw pixel coordinates regardless of any viewport adapter —
            // the wrapper never consults a ViewportAdapter.
            _mockProvider.SetSimulatedState(MockMouseStateProvider.CreateState(x: 10, y: 20));
            _mouseWrapper.Update(_gameTime);

            Assert.Equal(new Vector2(10f, 20f), _mouseWrapper.Position);
        }

        [Fact]
        public void MouseEventArgs_HasViewportIndependentVector2Position()
        {
            // The CoreEssentials MouseEventArgs exposes Position as a Vector2 (not a Point),
            // and no MonoGame.Extended types are required to consume it.
            var args = new MouseEventArgs(
                time: TimeSpan.FromMilliseconds(16),
                previousPosition: new Vector2(1, 2),
                position: new Vector2(3, 4),
                button: MouseButton.Left);

            Assert.Equal(new Vector2(3f, 4f), args.Position);
            Assert.Equal(new Vector2(1f, 2f), args.PreviousPosition);
            Assert.Equal(new Vector2(2f, 2f), args.DeltaMoved);
            Assert.True(args.IsLeftButton);
            Assert.False(args.IsRightButton);
        }

        [Fact]
        public void MouseEventArgs_ScrollWheelProperties_AreExposed()
        {
            var args = new MouseEventArgs(
                time: TimeSpan.Zero,
                previousPosition: Vector2.Zero,
                position: Vector2.Zero,
                scrollWheelValue: 120,
                scrollWheelDelta: 120);

            Assert.Equal(120, args.ScrollWheelValue);
            Assert.Equal(120, args.ScrollWheelDelta);
        }

        [Fact]
        public void Input_Mouse_IsCoreEssentialsMouseWrapper()
        {
            // The static Input.Mouse property should expose the CE wrapper, not the raw listener.
            Assert.IsType<CoreEssentials.Inputs.Mouse>(Input.Mouse);
        }
    }
}
