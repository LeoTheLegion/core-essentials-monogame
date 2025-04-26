using System;
using System.Collections;
using Microsoft.Xna.Framework;
using Xunit;
using CoreEssentials.Coroutines;

namespace CoreEssentials.Tests.Coroutines
{
    public class YieldInstructionTests
    {
        [Fact]
        public void WaitForSeconds_DelaysCoroutineExecution()
        {
            // Clear any coroutines from previous tests
            CoroutineManager.StopAllCoroutines();
            
            // Arrange
            bool completed = false;
            GameTime initialGameTime = new GameTime(TimeSpan.Zero, TimeSpan.FromSeconds(0));
            
            // Act - Start coroutine with WaitForSeconds
            CoroutineManager.StartCoroutine(TestRoutine());
            
            // First update - starts the coroutine
            CoroutineManager.Update(initialGameTime);
            Assert.False(completed);
            
            // Second update - still waiting (0.5 seconds passed)
            CoroutineManager.Update(new GameTime(TimeSpan.FromSeconds(0.5), TimeSpan.FromSeconds(0.5)));
            Assert.False(completed);
            
            // Third update - still waiting (1.0 seconds passed)
            CoroutineManager.Update(new GameTime(TimeSpan.FromSeconds(1.0), TimeSpan.FromSeconds(0.5)));
            Assert.False(completed);
            
            // Fourth update - coroutine should complete now (1.5 seconds passed > 1.2 seconds wait)
            CoroutineManager.Update(new GameTime(TimeSpan.FromSeconds(1.5), TimeSpan.FromSeconds(0.5)));
            
            // Assert
            Assert.True(completed);
            
            // Local function
            IEnumerator TestRoutine()
            {
                yield return new WaitForSeconds(1.2f); // Wait for 1.2 seconds
                completed = true;
            }
        }
        
        [Fact]
        public void WaitUntil_DelaysCoroutineUntilConditionMet()
        {
            // Clear any coroutines from previous tests
            CoroutineManager.StopAllCoroutines();
            
            // Arrange
            bool completed = false;
            bool condition = false;
            GameTime gameTime = new GameTime();
            
            // Act - Start coroutine with WaitUntil
            CoroutineManager.StartCoroutine(TestRoutine());
            
            // First update - starts the coroutine
            CoroutineManager.Update(gameTime);
            Assert.False(completed);
            
            // Second update - condition still not met
            CoroutineManager.Update(gameTime);
            Assert.False(completed);
            
            // Set condition to true
            condition = true;
            
            // Third update - condition is now met, coroutine should complete
            CoroutineManager.Update(gameTime);
            
            // Assert
            Assert.True(completed);
            
            // Local function
            IEnumerator TestRoutine()
            {
                yield return new WaitUntil(() => condition); // Wait until condition is true
                completed = true;
            }
        }
        
        [Fact]
        public void MultipleYieldInstructions_WorkInSequence()
        {
            // Clear any coroutines from previous tests
            CoroutineManager.StopAllCoroutines();
            
            // Arrange
            int step = 0;
            bool condition = false;
            GameTime initialGameTime = new GameTime(TimeSpan.Zero, TimeSpan.FromSeconds(0));
            
            // Act - Start coroutine with multiple yield instructions
            CoroutineManager.StartCoroutine(TestRoutine());
            
            // Run through updates with time progression
            CoroutineManager.Update(initialGameTime);
            Assert.Equal(1, step);
            
            // This update should get past the WaitForSeconds
            CoroutineManager.Update(new GameTime(TimeSpan.FromSeconds(1.0), TimeSpan.FromSeconds(1.0)));
            Assert.Equal(2, step);
            
            // This update should not progress (condition still false)
            CoroutineManager.Update(initialGameTime);
            Assert.Equal(2, step);
            
            // Set condition to true
            condition = true;
            
            // This update should get past the WaitUntil
            CoroutineManager.Update(initialGameTime);
            Assert.Equal(3, step);
            
            // This update should complete the coroutine
            CoroutineManager.Update(initialGameTime);
            
            // Assert
            Assert.Equal(4, step);
            
            // Local function
            IEnumerator TestRoutine()
            {
                step = 1;
                yield return new WaitForSeconds(0.5f); // Wait for 0.5 seconds
                
                step = 2;
                yield return new WaitUntil(() => condition); // Wait until condition is true
                
                step = 3;
                yield return null; // Wait one frame
                
                step = 4;
            }
        }
        
        [Fact]
        public void CombinedNestedCoroutinesAndYieldInstructions_WorkCorrectly()
        {
            // Clear any coroutines from previous tests
            CoroutineManager.StopAllCoroutines();
            
            // Arrange
            int step = 0;
            GameTime initialGameTime = new GameTime(TimeSpan.Zero, TimeSpan.FromSeconds(0));
            
            // Act - Start complex coroutine with nesting and yield instructions
            CoroutineManager.StartCoroutine(MainRoutine());
            
            // First update - starts main coroutine and begins nested
            CoroutineManager.Update(initialGameTime);
            Assert.Equal(1, step);
            
            // Second update - waits in nested
            CoroutineManager.Update(initialGameTime);
            Assert.Equal(2, step);
            
            // Third update - completes wait in nested
            CoroutineManager.Update(new GameTime(TimeSpan.FromSeconds(1.0), TimeSpan.FromSeconds(1.0)));
            Assert.Equal(3, step);
            
            // Fourth update - back to main coroutine
            CoroutineManager.Update(initialGameTime);
            Assert.Equal(4, step);
            
            // Fifth update - completes main coroutine
            CoroutineManager.Update(initialGameTime);
            Assert.Equal(5, step);
            
            // Local functions
            IEnumerator MainRoutine()
            {
                step = 1;
                yield return NestedRoutine();
                
                step = 4;
                yield return null;
                
                step = 5;
            }
            
            IEnumerator NestedRoutine()
            {
                step = 2;
                yield return new WaitForSeconds(0.5f);
                
                step = 3;
                yield return null;
            }
        }
    }
}