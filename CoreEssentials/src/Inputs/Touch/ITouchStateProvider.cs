// CoreEssentials/src/inputs/Touch/ITouchStateProvider.cs
using System.Collections.Generic;
using Microsoft.Xna.Framework.Input.Touch;

namespace CoreEssentials.Inputs
{
    /// <summary>
    /// Defines an interface for providing the current state of touch input.
    /// This allows the <see cref="Touch"/> wrapper to be unit tested without
    /// depending on real hardware input.
    /// </summary>
    public interface ITouchStateProvider
    {
        /// <summary>
        /// Gets the current collection of active touch locations.
        /// </summary>
        /// <returns>The current touch locations (empty if no touches are active).</returns>
        IReadOnlyList<TouchLocation> GetState();
    }
}
