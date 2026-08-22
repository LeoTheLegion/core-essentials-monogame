// CoreEssentials/src/inputs/IMouseStateProvider.cs
using Microsoft.Xna.Framework.Input;

namespace CoreEssentials.Inputs
{
    /// <summary>
    /// Defines an interface for providing the current state of the mouse.
    /// This allows the <see cref="Mouse"/> wrapper to be unit tested without
    /// depending on real hardware input.
    /// </summary>
    public interface IMouseStateProvider
    {
        /// <summary>
        /// Gets the current state of the mouse.
        /// </summary>
        /// <returns>The current <see cref="MouseState"/>.</returns>
        MouseState GetState();
    }
}
