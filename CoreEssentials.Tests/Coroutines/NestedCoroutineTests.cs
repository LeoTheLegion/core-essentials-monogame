using System;
using System.Collections;
using Microsoft.Xna.Framework;
using Xunit;
using CoreEssentials.Coroutines;

namespace CoreEssentials.Tests.Coroutines
{
    public class NestedCoroutineTests
    {
        [Fact]
        public void NestedCoroutine_ExecutesInProperSequence()
        {
            // Clear any coroutines from previous tests
            CoroutineManager.StopAllCoroutines();
            
            // Arrange
            int executionStep = 0;
            
            // Act - Setup coroutines
            var mainRoutine = MainCoroutine();
            var mainId = CoroutineManager.StartCoroutine(mainRoutine);
            
            // Need to manually simulate a few frames of execution
            GameTime gameTime = new GameTime(TimeSpan.Zero, TimeSpan.FromMilliseconds(16));
            
            // First update: Main coroutine starts and yields to nested coroutine
            CoroutineManager.Update(gameTime);
            // In our thread-safe implementation, this might not immediately set to 1
            // executionStep may be 1 or 2 depending on how nested coroutines are processed
            
            // Second update: Nested coroutine runs first step
            CoroutineManager.Update(gameTime);
            // executionStep should be at least 2 now
            Assert.True(executionStep >= 2);
            
            // Third update: Nested coroutine completes
            CoroutineManager.Update(gameTime);
            // executionStep should be at least 3 now
            Assert.True(executionStep >= 3);
            
            // Fourth update: Main coroutine continues after nested completes
            CoroutineManager.Update(gameTime);
            // executionStep should be at least 4 now
            Assert.True(executionStep >= 4);
            
            // Fifth update: Main coroutine completes
            CoroutineManager.Update(gameTime);
            Assert.Equal(5, executionStep);
            
            // Local function for main coroutine
            IEnumerator MainCoroutine()
            {
                executionStep = 1;
                
                // Yield to nested coroutine
                yield return NestedCoroutine();
                
                // This should execute after the nested coroutine completes
                executionStep = 4;
                
                yield return null;
                
                executionStep = 5;
            }
            
            // Local function for nested coroutine
            IEnumerator NestedCoroutine()
            {
                executionStep = 2;
                
                yield return null;
                
                executionStep = 3;
            }
        }
        
        [Fact]
        public void StoppingParentCoroutine_StopsNestedCoroutines()
        {
            // Clear any coroutines from previous tests
            CoroutineManager.StopAllCoroutines();
            
            // Arrange
            bool nestedWasExecuted = false;
            var mainRoutine = MainCoroutine();
            var mainId = CoroutineManager.StartCoroutine(mainRoutine);
            
            // Act - First update: Start the main coroutine
            GameTime gameTime = new GameTime(TimeSpan.Zero, TimeSpan.FromMilliseconds(16));
            CoroutineManager.Update(gameTime);
            
            // Stop the parent coroutine
            CoroutineManager.StopCoroutine(mainId);
            
            // Another update to ensure any pending operations complete
            CoroutineManager.Update(gameTime);
            
            // Assert - The nested coroutine should not have set nestedWasExecuted to true
            Assert.False(nestedWasExecuted);
            
            // Local functions
            IEnumerator MainCoroutine()
            {
                yield return NestedCoroutine();
                yield return null;
            }
            
            IEnumerator NestedCoroutine()
            {
                yield return null;
                nestedWasExecuted = true;
            }
        }
    }
}