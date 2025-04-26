using System;
using System.Collections;
using CoreEssentials.Coroutines;
using Microsoft.Xna.Framework;

namespace CoreEssentials.Tests.Coroutines
{
    /// <summary>
    /// Helper class for testing coroutines in a sequential environment
    /// </summary>
    public class CoroutineTestHelper
    {
        private readonly CoroutineOwner _owner;
        private double _currentTime;
        
        public CoroutineTestHelper()
        {
            _owner = new CoroutineOwner();
            _currentTime = 0.0;
            
            // Clean up any coroutines from previous tests
            CoroutineManager.StopAllCoroutines();
        }
        
        /// <summary>
        /// Starts a coroutine using the test owner
        /// </summary>
        public Guid StartCoroutine(IEnumerator routine)
        {
            return _owner.StartCoroutine(routine);
        }
        
        /// <summary>
        /// Advances time by the specified amount and updates coroutines
        /// </summary>
        public void AdvanceTime(float seconds)
        {
            // Create game time with incremental time
            _currentTime += seconds;
            var gameTime = new GameTime(
                TimeSpan.FromSeconds(_currentTime),
                TimeSpan.FromSeconds(seconds)
            );
            
            // Update coroutines with this game time
            CoroutineManager.Update(gameTime);
        }
        
        /// <summary>
        /// Updates coroutines without advancing time (for processing yield return null)
        /// </summary>
        public void Tick()
        {
            var gameTime = new GameTime(
                TimeSpan.FromSeconds(_currentTime),
                TimeSpan.FromSeconds(0.016) // One frame at ~60 FPS
            );
            
            CoroutineManager.Update(gameTime);
        }
        
        /// <summary>
        /// Cleans up all coroutines used by this helper
        /// </summary>
        public void Cleanup()
        {
            _owner.StopAllCoroutines();
        }
    }
}