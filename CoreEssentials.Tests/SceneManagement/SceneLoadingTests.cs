using System;
using System.Collections;
using System.Reflection;
using Microsoft.Xna.Framework;
using Xunit;
using CoreEssentials.Coroutines;
using CoreEssentials.SceneManagement;
using CoreEssentials.GameSystems;

namespace CoreEssentials.Tests.SceneManagement
{
    public class SceneLoadingTests
    {
        private class TestLoadingScene : Scene
        {
            public int LoadingSteps { get; set; } = 0;
            public int MaxLoadingSteps { get; set; } = 5;
            
            // Internal coroutine for testing - simulates the loading process
            public IEnumerator LoadingCoroutine()
            {
                LoadingStatus = "Initializing...";
                yield return null;
                
                LoadingStatus = "Loading assets...";
                LoadingSteps = 1;
                UpdateLoadingProgress(0.2f, LoadingStatus);
                yield return null;
                
                LoadingStatus = "Creating game objects...";
                LoadingSteps = 2;
                UpdateLoadingProgress(0.4f, LoadingStatus);
                yield return null;
                
                LoadingStatus = "Configuring systems...";
                LoadingSteps = 3;
                UpdateLoadingProgress(0.6f, LoadingStatus);
                yield return null;
                
                LoadingStatus = "Finalizing...";
                LoadingSteps = 4;
                UpdateLoadingProgress(0.8f, LoadingStatus);
                yield return null;
                
                LoadingStatus = "Loading complete";
                LoadingSteps = MaxLoadingSteps;
                UpdateLoadingProgress(1.0f, LoadingStatus);
            }
            
            protected override GameSystem[] LoadGameSystems()
            {
                // No systems to load in this test
                return new GameSystem[0];
            }

            protected override IEnumerator OnStartCoroutine()
            {
                throw new NotImplementedException();
            }
        }
        
        [Fact]
        public void LoadingStatus_IsUpdatedProperly()
        {
            // Arrange
            var scene = new TestLoadingScene();
            var manager = new SceneManager();
            scene.SetSceneManager(manager);
            
            // Act - Begin loading
            var loadingCoroutine = scene.LoadingCoroutine();
            loadingCoroutine.MoveNext(); // Initializing...
            
            // Assert - First status
            Assert.Equal("Initializing...", scene.LoadingStatus);
            
            // Move through loading steps
            loadingCoroutine.MoveNext(); // Move to Loading assets...
            Assert.Equal("Loading assets...", scene.LoadingStatus);
            
            loadingCoroutine.MoveNext(); // Move to Creating game objects...
            Assert.Equal("Creating game objects...", scene.LoadingStatus);
            
            loadingCoroutine.MoveNext(); // Move to Configuring systems...
            Assert.Equal("Configuring systems...", scene.LoadingStatus);
            
            loadingCoroutine.MoveNext(); // Move to Finalizing...
            Assert.Equal("Finalizing...", scene.LoadingStatus);
            
            loadingCoroutine.MoveNext(); // Move to Loading complete
            Assert.Equal("Loading complete", scene.LoadingStatus);
        }
        
        [Fact]
        public void GetLoadingProgressPercentage_ReturnsCorrectValue()
        {
            // Arrange
            var scene = new TestLoadingScene();
            scene.MaxLoadingSteps = 10;
            
            // Set progress to 60%
            typeof(Scene).GetField("_loadingProgress", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(scene, 0.6f);
            
            // Assert - Now using the non-overridable method
            Assert.Equal(60, scene.GetLoadingProgressPercentage());
        }
    }
}