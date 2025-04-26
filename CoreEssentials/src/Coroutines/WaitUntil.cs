using System;
using Microsoft.Xna.Framework;

namespace CoreEssentials.Coroutines
{
    /// <summary>
    /// A yield instruction that waits until a specified condition is met.
    /// </summary>
    public class WaitUntil : IYieldInstruction
    {
        /// <summary>
        /// Gets the condition function that determines when to resume execution.
        /// </summary>
        public Func<bool> Condition { get; }
        
        /// <summary>
        /// Initializes a new instance of the WaitUntil class.
        /// </summary>
        /// <param name="condition">The condition function that returns true when the wait should end.</param>
        public WaitUntil(Func<bool> condition)
        {
            Condition = condition ?? throw new ArgumentNullException(nameof(condition));
        }

        /// <summary>
        /// Determines if the condition has been met.
        /// </summary>
        /// <param name="gameTime">Current game time information.</param>
        /// <returns>True if the condition is met; otherwise, false.</returns>
        public bool IsComplete(GameTime gameTime)
        {
            return Condition();
        }
    }
}