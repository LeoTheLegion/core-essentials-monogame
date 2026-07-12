// CoreEssentials/src/inputs/DefaultKeyboardStateProvider.cs
using Microsoft.Xna.Framework.Input;

namespace CoreEssentials.Inputs
{
    /// <summary>
    /// Provides the current keyboard state using the default MonoGame <see cref="Keyboard"/> implementation.
    /// </summary>
    public class DefaultKeyboardStateProvider : IKeyboardStateProvider
    {
        /// <summary>
        /// Gets the current <see cref="KeyboardState"/> from the keyboard input device.
        /// </summary>
        /// <returns>The current <see cref="KeyboardState"/>.</returns>
        public KeyboardState GetState()
        {
            return Microsoft.Xna.Framework.Input.Keyboard.GetState();
        }
    }
}
