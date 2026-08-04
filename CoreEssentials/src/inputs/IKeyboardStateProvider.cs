// CoreEssentials/src/inputs/IKeyboardStateProvider.cs
using Microsoft.Xna.Framework.Input;

namespace CoreEssentials.Inputs
{
    /// <summary>
    /// Defines an interface for providing the current state of the keyboard.
    /// </summary>
    public interface IKeyboardStateProvider
    {
        /// <summary>
        /// Gets the current state of the keyboard.
        /// </summary>
        /// <returns>The current KeyboardState.</returns>
        KeyboardState GetState();
    }
}
