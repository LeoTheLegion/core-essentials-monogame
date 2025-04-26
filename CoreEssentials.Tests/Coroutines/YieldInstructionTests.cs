using System;
using System.Collections;
using Microsoft.Xna.Framework;
using Xunit;
using Xunit.Sdk;
using CoreEssentials.Coroutines;

namespace CoreEssentials.Tests.Coroutines
{
    public class YieldInstructionTests
    {
        [Fact]
        public void WaitForSeconds_DelaysCoroutineExecution()
        {
            // Arrange
            bool completed = false;
            GameTime initialGameTime = new GameTime(TimeSpan.Zero, TimeSpan.FromSeconds(0));
            
            // Create a test owner to isolate this test's coroutines
            var testOwner = new TestCoroutineOwner();
            
            // Act - Start coroutine with WaitForSeconds
            testOwner.StartCoroutine(TestRoutine());
            
            // First update - starts the coroutine
            CoroutineManager.Update(initialGameTime);
            
            // Second update - still waiting (0.5 seconds passed)
            CoroutineManager.Update(new GameTime(TimeSpan.FromSeconds(0.5), TimeSpan.FromSeconds(0.5)));
            
            // Third update - still waiting (1.0 seconds passed)
            CoroutineManager.Update(new GameTime(TimeSpan.FromSeconds(1.0), TimeSpan.FromSeconds(0.5)));
            
            // Fourth update - still waiting (1.5 seconds passed > 1.2 seconds wait)
            CoroutineManager.Update(new GameTime(TimeSpan.FromSeconds(1.5), TimeSpan.FromSeconds(0.5)));
            
            // Additional update to ensure completion
            CoroutineManager.Update(new GameTime(TimeSpan.FromSeconds(2.0), TimeSpan.FromSeconds(0.5)));
            
            // Assert
            Assert.True(completed, "Coroutine should have completed after waiting for seconds");
            
            // Clean up coroutines for this test
            testOwner.StopAllCoroutines();
            
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
            // Arrange
            bool completed = false;
            bool condition = false;
            GameTime gameTime = new GameTime();
            
            // Create a test owner to isolate this test's coroutines
            var testOwner = new TestCoroutineOwner();
            
            // Act - Start coroutine with WaitUntil
            testOwner.StartCoroutine(TestRoutine());
            
            // First update - starts the coroutine
            CoroutineManager.Update(gameTime);
            
            // Second update - condition still not met
            CoroutineManager.Update(gameTime);
            Assert.False(completed);
            
            // Set condition to true
            condition = true;
            
            // Third update - condition is now met, coroutine should complete
            CoroutineManager.Update(gameTime);
            
            // Fourth update - ensure completion is processed
            CoroutineManager.Update(gameTime);
            
            // Assert
            Assert.True(completed, "Coroutine should have completed after condition was met");
            
            // Clean up coroutines for this test
            testOwner.StopAllCoroutines();
            
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
            // Arrange
            int step = 0;
            bool condition = false;
            
            // Create a test helper for reliable coroutine testing
            var helper = new CoroutineTestHelper();
            
            // Act - Start coroutine with multiple yield instructions
            helper.StartCoroutine(TestRoutine());
            
            // First tick - starts the coroutine
            helper.Tick();
            Assert.Equal(1, step);
            
            // This update should NOT progress through the WaitForSeconds yet (not enough time elapsed)
            helper.Tick();
            Assert.Equal(1, step);
            
            // This update SHOULD progress through the WaitForSeconds (advancing 0.6 seconds > 0.5 seconds wait)
            helper.AdvanceTime(0.6f);
            Assert.Equal(2, step);
            
            // This update should not progress (condition still false)
            helper.Tick();
            Assert.Equal(2, step);
            
            // Set condition to true
            condition = true;
            
            // This update should get past the WaitUntil since condition is now true
            helper.Tick();
            Assert.Equal(3, step);
            
            // This update should complete the coroutine
            helper.Tick();
            Assert.Equal(4, step);
            
            // Clean up coroutines for this test
            helper.Cleanup();
            
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
            // Arrange
            int step = 0;
            
            // Create a test helper for reliable coroutine testing
            var helper = new CoroutineTestHelper();
            
            // Act - Start complex coroutine with nesting and yield instructions
            helper.StartCoroutine(MainRoutine());
            
            // First update - starts the main coroutine
            helper.Tick();
            Assert.Equal(1, step);
            
            // Second update - transitions to nested coroutine
            helper.Tick();
            Assert.Equal(2, step);
            
            // This update should NOT progress through the WaitForSeconds yet
            helper.Tick();
            Assert.Equal(2, step);
            
            // This update SHOULD progress through the WaitForSeconds
            helper.AdvanceTime(0.6f);
            Assert.Equal(3, step);
            
            // Update to continue main coroutine after nested returns
            helper.Tick();
            Assert.Equal(4, step);
            
            // Update to complete main coroutine
            helper.Tick();
            Assert.Equal(5, step);
            
            // Clean up coroutines for this test
            helper.Cleanup();
            
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
            }
        }
        
        // Adding TestCoroutineOwner class for test isolation
        private class TestCoroutineOwner : CoreEssentials.Coroutines.CoroutineOwner
        {
        }
    }
}