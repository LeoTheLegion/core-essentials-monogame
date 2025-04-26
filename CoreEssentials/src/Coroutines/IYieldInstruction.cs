using System;
using Microsoft.Xna.Framework;

namespace CoreEssentials.Coroutines
{
    /// <summary>
    /// Interface for yield instructions that can be returned from coroutines
    /// to control when they should resume execution.
    /// </summary>
    public interface IYieldInstruction
    {
        /// <summary>
        /// Determines if the yield instruction has completed and the coroutine can resume.
        /// </summary>
        /// <param name="gameTime">Current game time information.</param>
        /// <returns>True if the yield instruction is complete and the coroutine should resume; otherwise, false.</returns>
        bool IsComplete(GameTime gameTime);
    }
}