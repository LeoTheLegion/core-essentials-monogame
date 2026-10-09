using System;
using Xunit;
using CoreEssentials.Inputs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace CoreEssentials.Tests.Inputs
{
    /// <summary>
    /// Covers the per-button dispatch in the <see cref="CoreEssentials.Inputs.Mouse"/> wrapper for the
    /// button arms the primary suite does not exercise (XButton1/XButton2, and full down/pressed/released
    /// transitions for every button). Uses the same injected <see cref="IMouseStateProvider"/> seam — no
    /// hardware or graphics device required.
    /// </summary>
    public class MouseButtonMatrixTests
    {
        private readonly MockMouseStateProvider _provider;
        private readonly CoreEssentials.Inputs.Mouse _mouse;
        private readonly GameTime _gameTime = new();

        public MouseButtonMatrixTests()
        {
            _provider = new MockMouseStateProvider();
            _mouse = new CoreEssentials.Inputs.Mouse(_provider);
            _mouse.Update(_gameTime); // establish neutral previous/current state
        }

        [Fact]
        public void IsButtonDown_Left_XButton1_And_XButton2_AreIndependent()
        {
            // XButtons are the arms the primary suite does not exercise; verify each toggles on its own.
            _provider.SetSimulatedState(MockMouseStateProvider.CreateState(leftButton: true));
            _mouse.Update(_gameTime);
            Assert.True(_mouse.IsButtonDown(MouseButton.Left));
            Assert.False(_mouse.IsButtonDown(MouseButton.XButton1));
            Assert.False(_mouse.IsButtonDown(MouseButton.XButton2));

            _provider.SetSimulatedState(MockMouseStateProvider.CreateState(xButton1: true));
            _mouse.Update(_gameTime);
            Assert.True(_mouse.IsButtonDown(MouseButton.XButton1));
            Assert.False(_mouse.IsButtonDown(MouseButton.Left));

            _provider.SetSimulatedState(MockMouseStateProvider.CreateState(xButton2: true));
            _mouse.Update(_gameTime);
            Assert.True(_mouse.IsButtonDown(MouseButton.XButton2));
            Assert.False(_mouse.IsButtonDown(MouseButton.XButton1));
        }

        [Theory]
        [InlineData(MouseButton.Left)]
        [InlineData(MouseButton.XButton1)]
        [InlineData(MouseButton.XButton2)]
        public void IsButtonPressedOnce_TrueOnlyOnTheUpThenDownFrame(MouseButton target)
        {
            // Start up.
            _provider.SetSimulatedState(new MouseState());
            _mouse.Update(_gameTime);

            // Now press: exactly one frame should report "pressed once".
            _provider.SetSimulatedState(StateFor(target));
            _mouse.Update(_gameTime);
            Assert.True(_mouse.IsButtonPressedOnce(target));

            // Held: no longer "once".
            _mouse.Update(_gameTime);
            Assert.False(_mouse.IsButtonPressedOnce(target));
        }

        [Theory]
        [InlineData(MouseButton.Left)]
        [InlineData(MouseButton.XButton1)]
        [InlineData(MouseButton.XButton2)]
        public void IsButtonReleasedOnce_TrueOnlyOnTheDownThenUpFrame(MouseButton target)
        {
            // Start pressed.
            _provider.SetSimulatedState(StateFor(target));
            _mouse.Update(_gameTime);

            // Now release: exactly one frame should report "released once".
            _provider.SetSimulatedState(new MouseState());
            _mouse.Update(_gameTime);
            Assert.True(_mouse.IsButtonReleasedOnce(target));

            // Still up: no longer "once".
            _mouse.Update(_gameTime);
            Assert.False(_mouse.IsButtonReleasedOnce(target));
        }

        [Theory]
        [InlineData(MouseButton.Left)]
        [InlineData(MouseButton.Right)]
        [InlineData(MouseButton.Middle)]
        [InlineData(MouseButton.XButton1)]
        [InlineData(MouseButton.XButton2)]
        public void IsButtonDown_TrueOnlyForTheHeldButton_EveryButtonIsIndependent(MouseButton held)
        {
            _provider.SetSimulatedState(StateFor(held));
            _mouse.Update(_gameTime);

            foreach (var probe in Enum.GetValues<MouseButton>())
            {
                var expected = probe == held;
                Assert.True(_mouse.IsButtonDown(probe) == expected,
                    $"IsButtonDown({probe}) should be {expected} while holding {held}.");
            }
        }

        private static MouseState StateFor(MouseButton button) => button switch
        {
            MouseButton.Left => MockMouseStateProvider.CreateState(leftButton: true),
            MouseButton.Right => MockMouseStateProvider.CreateState(rightButton: true),
            MouseButton.Middle => MockMouseStateProvider.CreateState(middleButton: true),
            MouseButton.XButton1 => MockMouseStateProvider.CreateState(xButton1: true),
            _ => MockMouseStateProvider.CreateState(xButton2: true)
        };
    }
}
