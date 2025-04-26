using System;
using System.Collections;
using CoreEssentials.Coroutines;
using CoreEssentials.GameSystems;
using CoreEssentials.SceneManagement;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Xunit;

namespace CoreEssentials.Tests.SceneManagement
{
    public class SceneLoadingTests
    {
        private class TestScene : Scene
        {
            public string[] StatusUpdates { get; } = new string[4];
            public int StatusUpdateCount { get; private set; } = 0;
            
            protected override GameSystem[] LoadGameSystems()
            {
                return new GameSystem[0];
            }

            protected override void onStart()
            {
                // No implementation needed
            }
            
            protected override IEnumerator OnStartCoroutine()
            {
                // This will be called after the base coroutine has already set progress to 0.5
                UpdateLoadingProgress(0.6f, "First stage");
                RecordStatusUpdate();
                yield return null;
                
                UpdateLoadingProgress(0.7f, "Second stage");
                RecordStatusUpdate();
                yield return null;
                
                UpdateLoadingProgress(0.8f, "Third stage");
                RecordStatusUpdate();
                yield return null;
                
                UpdateLoadingProgress(1.0f, "Complete");
                RecordStatusUpdate();
            }
            
            private void RecordStatusUpdate()
            {
                if (StatusUpdateCount < StatusUpdates.Length)
                {
                    StatusUpdates[StatusUpdateCount] = LoadingStatus;
                    StatusUpdateCount++;
                }
            }
        }
        
        [Fact]
        public void LoadingStatus_IsUpdatedProperly()
        {
            // Arrange
            var scene = new TestScene();
            var gameTime = new GameTime();
            
            // Act - Start loading process
            scene.Load();
            
            // First updates will be for setting up the Scene basics 
            // (LoadGameSystems, registering, etc.)
            for (int i = 0; i < 10; i++)
            {
                CoroutineManager.Update(gameTime);
            }
            
            // Assert - Verify the status messages were recorded correctly
            Assert.Equal(4, scene.StatusUpdateCount);
            Assert.Equal("First stage", scene.StatusUpdates[0]);
            Assert.Equal("Second stage", scene.StatusUpdates[1]);
            Assert.Equal("Third stage", scene.StatusUpdates[2]);
            Assert.Equal("Complete", scene.StatusUpdates[3]);
            
            // Verify loading completed
            Assert.True(scene.IsLoaded);
            Assert.False(scene.IsLoading);
            Assert.Equal(1.0f, scene.LoadingProgress);
        }
        
        [Fact]
        public void GetLoadingProgressPercentage_ReturnsCorrectValue()
        {
            // Arrange
            var scene = new TestScene();
            
            // Act
            scene.Load();
            
            // Need multiple updates to get to the point where OnStartCoroutine starts
            for (int i = 0; i < 5; i++)
            {
                CoroutineManager.Update(new GameTime()); 
            }
            // This will set progress to 0.6 from "First stage"
            CoroutineManager.Update(new GameTime());
            
            // Assert
            Assert.Equal(60, scene.GetLoadingProgressPercentage());
        }
    }
}