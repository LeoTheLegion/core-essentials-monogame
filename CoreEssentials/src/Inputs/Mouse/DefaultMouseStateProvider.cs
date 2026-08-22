// CoreEssentials/src/inputs/DefaultMouseStateProvider.cs
using Microsoft.Xna.Framework.Input;

namespace CoreEssentials.Inputs
{
    /// <summary>
    /// Provides the current mouse state using the default MonoGame <see cref="Mouse"/> implementation.
    /// </summary>
    public class DefaultMouseStateProvider : IMouseStateProvider
    {
        /// <summary>
        /// Gets the current <see cref="MouseState"/> from the mouse input device.
        /// </summary>
        /// <returns>The current <see cref="MouseState"/>.</returns>
        public MouseState GetState()
        {
            return Microsoft.Xna.Framework.Input.Mouse.GetState();
        }
    }
}
