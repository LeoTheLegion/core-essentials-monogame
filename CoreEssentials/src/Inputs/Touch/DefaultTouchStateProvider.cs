// CoreEssentials/src/inputs/Touch/DefaultTouchStateProvider.cs
using System.Collections.Generic;
using Microsoft.Xna.Framework.Input.Touch;

namespace CoreEssentials.Inputs
{
    /// <summary>
    /// Provides the current touch state using the default MonoGame <see cref="TouchPanel"/> implementation.
    /// </summary>
    public class DefaultTouchStateProvider : ITouchStateProvider
    {
        /// <summary>
        /// Gets the current collection of active touch locations from the touch input device.
        /// </summary>
        /// <returns>The current touch locations (empty if no touches are active or the
        /// touch panel is not initialized, e.g. in a headless environment).</returns>
        public IReadOnlyList<TouchLocation> GetState()
        {
            // TouchPanel.GetState() throws NullReferenceException when no touch device /
            // graphics device is available (e.g. headless test environments). Treat that
            // as "no touches" so the Input system remains usable in such contexts.
            try
            {
                return new List<TouchLocation>(TouchPanel.GetState());
            }
            catch (NullReferenceException)
            {
                return new List<TouchLocation>();
            }
        }
    }
}
