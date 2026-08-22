using System;
using System.Collections.Generic;
using CoreEssentials.Inputs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input.Touch;
using Xunit;

namespace CoreEssentials.Tests.Inputs
{
    /// <summary>
    /// Tests for the <see cref="CoreEssentials.Inputs.Touch"/> wrapper.
    /// </summary>
    public class TouchTests
    {
        private sealed class MockTouchStateProvider : ITouchStateProvider
        {
            private readonly List<TouchLocation> _state;

            public MockTouchStateProvider(List<TouchLocation> state = null)
            {
                _state = state ?? new List<TouchLocation>();
            }

            public void SetState(params TouchLocation[] locations)
            {
                _state.Clear();
                foreach (var location in locations)
                    _state.Add(location);
            }

            public IReadOnlyList<TouchLocation> GetState() => _state;
        }

        private static TouchLocation CreateLocation(int id = 1, Vector2? position = null, TouchLocationState state = TouchLocationState.Pressed)
        {
            return new TouchLocation(id, state, position ?? Vector2.Zero);
        }

        [Fact]
        public void ActiveTouchCount_ReturnsZero_WhenNoTouches()
        {
            // Arrange
            var provider = new MockTouchStateProvider();
            var touch = new Touch(provider);

            // Act
            int count = touch.ActiveTouchCount;

            // Assert
            Assert.Equal(0, count);
        }

        [Fact]
        public void ActiveTouchCount_ReturnsCount_WhenTouchesActive()
        {
            // Arrange
            var provider = new MockTouchStateProvider();
            provider.SetState(CreateLocation(1), CreateLocation(2), CreateLocation(3));
            var touch = new Touch(provider);

            // Act
            int count = touch.ActiveTouchCount;

            // Assert
            Assert.Equal(3, count);
        }

        [Fact]
        public void HasActiveTouches_ReturnsFalse_WhenNoTouches()
        {
            // Arrange
            var provider = new MockTouchStateProvider();
            var touch = new Touch(provider);

            // Act
            bool hasTouches = touch.HasActiveTouches;

            // Assert
            Assert.False(hasTouches);
        }

        [Fact]
        public void HasActiveTouches_ReturnsTrue_WhenTouchesActive()
        {
            // Arrange
            var provider = new MockTouchStateProvider();
            provider.SetState(CreateLocation(1));
            var touch = new Touch(provider);

            // Act
            bool hasTouches = touch.HasActiveTouches;

            // Assert
            Assert.True(hasTouches);
        }

        [Fact(Skip = "TouchListener.Update calls TouchPanel.GetState() internally, which is null in a headless test environment.")]
        public void Update_RefreshesCurrentState()
        {
            // Arrange
            var provider = new MockTouchStateProvider();
            var touch = new Touch(provider);
            var gameTime = new GameTime();

            // Act
            touch.Update(gameTime);
            provider.SetState(CreateLocation(1), CreateLocation(2));
            touch.Update(gameTime);

            // Assert
            Assert.Equal(2, touch.ActiveTouchCount);
        }

        [Fact]
        public void TouchEventArgs_ContainsIdPositionAndState()
        {
            // Arrange
            var time = TimeSpan.FromSeconds(1.5f);
            var position = new Vector2(10, 20);

            // Act
            var args = new TouchEventArgs(time, id: 7, position, TouchLocationState.Pressed);

            // Assert
            Assert.Equal(time, args.Time);
            Assert.Equal(7, args.Id);
            Assert.Equal(position, args.Position);
            Assert.Equal(TouchLocationState.Pressed, args.State);
        }
    }
}
