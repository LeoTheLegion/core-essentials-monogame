using System;
using Microsoft.Xna.Framework;

namespace CoreEssentials.Coroutines
{
    /// <summary>
    /// A yield instruction that waits for a specified number of seconds.
    /// </summary>
    public class WaitForSeconds : IYieldInstruction
    {
        /// <summary>
        /// Gets the number of seconds to wait.
        /// </summary>
        public float Seconds { get; }
        
        /// <summary>
        /// The target time when this wait instruction will be complete.
        /// </summary>
        private float _targetTime;
        
        /// <summary>
        /// Flag indicating if the target time has been initialized.
        /// </summary>
        private bool _initialized;

        /// <summary>
        /// Initializes a new instance of the WaitForSeconds class.
        /// </summary>
        /// <param name="seconds">The number of seconds to wait.</param>
        public WaitForSeconds(float seconds)
        {
            Seconds = seconds;
            _initialized = false;
        }

        /// <summary>
        /// Determines if the wait time has elapsed.
        /// </summary>
        /// <param name="gameTime">Current game time information.</param>
        /// <returns>True if the wait time has elapsed; otherwise, false.</returns>
        public bool IsComplete(GameTime gameTime)
        {
            // Initialize the target time on first check
            if (!_initialized)
            {
                _targetTime = (float)gameTime.TotalGameTime.TotalSeconds + Seconds;
                _initialized = true;
                return false;
            }

            // Check if current time has exceeded the target time
            return (float)gameTime.TotalGameTime.TotalSeconds >= _targetTime;
        }
    }
}