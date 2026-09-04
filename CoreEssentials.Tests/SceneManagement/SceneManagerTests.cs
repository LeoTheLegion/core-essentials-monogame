using System;
using System.Collections;
using Xunit;
using Microsoft.Xna.Framework;
using CoreEssentials.Coroutines;
using CoreEssentials.Scenes;
using CoreEssentials.GameSystems;
using CoreEssentials.Tests.Coroutines;

namespace CoreEssentials.Tests.SceneManagement
{
    public class SceneManagerTests
    {
        private class FastLoadScene : Scene
        {
            protected override IEnumerator OnStartCoroutine()
            {
                LoadingStatus = "Initializing...";
                yield return null;

                LoadingStatus = "Loading assets...";
                UpdateLoadingProgress(0.33f, LoadingStatus);
                yield return null;

                LoadingStatus = "Registering game systems...";
                UpdateLoadingProgress(0.66f, LoadingStatus);
                yield return null;

                LoadingStatus = "Loading complete";
                UpdateLoadingProgress(1.0f, LoadingStatus);
            }
            
            protected override GameSystem[] LoadGameSystems()
            {
                // No game systems in test
                return new GameSystem[0];
            }
        }

        private class SlowLoadScene : Scene
        {
            protected override IEnumerator OnStartCoroutine()
            {
                LoadingStatus = "Initializing...";
                yield return null;

                LoadingStatus = "Loading resources...";
                UpdateLoadingProgress(0.25f, LoadingStatus);
                yield return new WaitForSeconds(0.1f);

                LoadingStatus = "Creating game objects...";
                UpdateLoadingProgress(0.5f, LoadingStatus);
                yield return null;

                LoadingStatus = "Setting up physics...";
                UpdateLoadingProgress(0.75f, LoadingStatus);
                yield return new WaitForSeconds(0.1f);

                LoadingStatus = "Finalizing...";
                UpdateLoadingProgress(0.9f, LoadingStatus);
                yield return null;

                LoadingStatus = "Loading complete";
                UpdateLoadingProgress(1.0f, LoadingStatus);
            }
            
            protected override GameSystem[] LoadGameSystems()
            {
                // No game systems in test
                return new GameSystem[0];
            }
        }

        private class LoadingScreenScene : Scene
        {
            private Scene NextScene { get; set; }
            public bool IsNextSceneLoaded { get; private set; }

            public LoadingScreenScene(Scene nextScene)
            {
                NextScene = nextScene;
            }

            protected override IEnumerator OnStartCoroutine()
            {
                LoadingStatus = "Preparing loading screen...";
                UpdateLoadingProgress(0.2f, LoadingStatus);
                yield return null;

                LoadingStatus = "Loading next scene...";
                UpdateLoadingProgress(0.5f, LoadingStatus);

                // Start loading the next scene
                NextScene.Load();
                
                // Wait for next scene to load
                while (NextScene.IsLoading)
                {
                    yield return null;
                }
                
                IsNextSceneLoaded = true;
                UpdateLoadingProgress(1.0f, "Ready to transition");
            }
            
            protected override GameSystem[] LoadGameSystems()
            {
                // No game systems in test
                return new GameSystem[0];
            }
        }

        [Fact]
        public void SetScene_SetsCurrentScene()
        {
            // Create a test helper for reliable coroutine testing
            var helper = new CoroutineTestHelper();
            
            // Arrange
            var manager = new SceneManager();
            var scene = new FastLoadScene();
            
            // Act
            manager.LoadScene(scene);
            
            // Assert - Scene should be set as the next scene
            Assert.Equal(scene, manager.NextScene);
            
            // Clean up
            helper.Cleanup();
        }

        [Fact]
        public void DirectSceneTransition_WorksCorrectly()
        {
            // Create a test helper for reliable coroutine testing
            var helper = new CoroutineTestHelper();
            
            // Arrange
            var manager = new SceneManager();
            var newScene = new FastLoadScene();

            // Act - Start transition
            manager.LoadScene(newScene);
            
            // Assert - Transition should be in progress
            Assert.True(manager.IsTransitioning);
            Assert.Equal(newScene, manager.NextScene);
            
            // Process multiple updates to ensure transition completes
            for (int i = 0; i < 20; i++) // Increased number of updates to ensure completion
            {
                // Update coroutines first
                helper.Tick();
                
                // Then update scene manager
                manager.Update(new GameTime(TimeSpan.FromSeconds(i * 0.1), TimeSpan.FromSeconds(0.1)));
                
                // If transition completes, we can break out of the loop
                if (!manager.IsTransitioning)
                    break;
            }
            
            // After sufficient updates, transition should be complete
            Assert.False(manager.IsTransitioning);
            Assert.Null(manager.NextScene);
            
            // The current scene should be set and fully loaded
            Assert.NotNull(manager.CurrentScene);
            Assert.IsType<FastLoadScene>(manager.CurrentScene);
            Assert.Equal(1.0f, manager.CurrentScene.LoadingProgress);
            Assert.Equal("Loading complete", manager.CurrentScene.LoadingStatus);
            
            // Clean up
            helper.Cleanup();
        }

        [Fact]
        public void TransitionWithLoadingScreen_WorksCorrectly()
        {
            // Create a test helper for reliable coroutine testing
            var helper = new CoroutineTestHelper();
            
            // Arrange
            var manager = new SceneManager();
            var targetScene = new SlowLoadScene();
            var loadingScreen = new LoadingScreenScene(targetScene);
            
            // Set loading screen
            manager.SetLoadingScene(loadingScreen);

            // Act - Start transition with loading screen
            manager.LoadScene(targetScene);

            // Assert - Transition should be in progress
            Assert.True(manager.IsTransitioning);
            
            // Process updates with time advances to handle WaitForSeconds
            // Increased number of iterations to ensure the transition completes
            for (int i = 0; i < 30; i++) 
            {
                // Update with advancing time
                helper.AdvanceTime(0.1f);
                manager.Update(new GameTime(TimeSpan.FromSeconds(i * 0.1), TimeSpan.FromSeconds(0.1)));
                
                // If transition completes, we can break out of the loop
                if (!manager.IsTransitioning && manager.CurrentScene is SlowLoadScene && manager.CurrentScene.LoadingProgress >= 0.99f)
                    break;
            }
            
            // After sufficient updates, verify the transition completed
            Assert.False(manager.IsTransitioning);
            Assert.Null(manager.NextScene);
            
            // The current scene should now be the target scene
            Assert.NotNull(manager.CurrentScene);
            Assert.IsType<SlowLoadScene>(manager.CurrentScene);
            Assert.Equal(1.0f, manager.CurrentScene.LoadingProgress);
            Assert.Equal("Loading complete", manager.CurrentScene.LoadingStatus);
            
            // Clean up
            helper.Cleanup();
        }

        [Fact]
        public void TransitionWithLoadingScreen_UnloadsLoadingScreen_AfterSwap()
        {
            // Regression: the loading screen was deliberately kept loaded "for reuse", but its canvas
            // stays registered in the global GUI, so its label kept rendering on top of the new scene.
            // After the swap the loading screen must be unloaded so it stops rendering and can be
            // cleanly reloaded on the next transition.
            var helper = new CoroutineTestHelper();

            var manager = new SceneManager();
            var targetScene = new SlowLoadScene();
            var loadingScreen = new LoadingScreenScene(targetScene);

            manager.SetLoadingScene(loadingScreen);

            // Act — start the transition and drive it to completion.
            manager.LoadScene(targetScene);
            for (int i = 0; i < 30 && manager.IsTransitioning; i++)
            {
                helper.AdvanceTime(0.1f);
                manager.Update(new GameTime(TimeSpan.FromSeconds(i * 0.1), TimeSpan.FromSeconds(0.1)));
            }

            // Assert — the target is now current and fully loaded...
            Assert.False(manager.IsTransitioning);
            Assert.NotNull(manager.CurrentScene);
            Assert.IsType<SlowLoadScene>(manager.CurrentScene);

            // ...and the loading screen has been unloaded (no longer "loaded"), so its canvas can no
            // longer keep rendering. It is retained as the loading scene for reuse on the next call.
            Assert.Same(loadingScreen, manager.LoadingScene);
            Assert.False(loadingScreen.IsLoaded, "Loading screen should be unloaded after the swap.");

            helper.Cleanup();
        }
    }
}