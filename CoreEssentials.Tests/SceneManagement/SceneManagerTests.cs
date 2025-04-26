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
    public class SceneManagerTests
    {
        [Fact]
        public void Constructor_InitializesCorrectly()
        {
            // Arrange & Act
            var sceneManager = new SceneManager();
            
            // Assert
            Assert.NotNull(sceneManager);
            Assert.Null(sceneManager.CurrentScene);
            Assert.Null(sceneManager.NextScene);
        }
        
        [Fact]
        public void AddScene_AddsSceneToCollection()
        {
            // Arrange
            var sceneManager = new SceneManager();
            var scene = new MockScene();
            
            // Act
            sceneManager.LoadScene(scene);
            
            // Assert
            Assert.Equal(scene, sceneManager.NextScene);
        }
        
        // Simple test scene that loads instantly
        private class FastLoadScene : Scene
        {
            public bool WasUnloaded { get; private set; } = false;
            
            protected override GameSystem[] LoadGameSystems()
            {
                return new GameSystem[0];
            }

            protected override void onStart() { }
            
            protected override IEnumerator OnStartCoroutine()
            {
                // Complete loading immediately
                _loadingProgress = 1.0f;
                yield break;
            }
            
            public override void Unload()
            {
                base.Unload();
                WasUnloaded = true;
            }
        }
        
        // Test scene that loads in multiple steps
        private class SlowLoadScene : Scene
        {
            protected override GameSystem[] LoadGameSystems()
            {
                return new GameSystem[0];
            }

            protected override void onStart() { }
            
            protected override IEnumerator OnStartCoroutine()
            {
                LoadingStatus = "Loading slowly...";
                _loadingProgress = 0.5f;
                yield return null;
                
                LoadingStatus = "Almost done...";
                _loadingProgress = 0.9f;
                yield return null;
                
                LoadingStatus = "Complete";
                _loadingProgress = 1.0f;
                yield break;
            }
        }
        
        [Fact]
        public void DirectSceneTransition_WorksCorrectly()
        {
            // Arrange
            SceneManager sceneManager = new SceneManager();
            FastLoadScene initialScene = new FastLoadScene();
            FastLoadScene targetScene = new FastLoadScene();
            
            // Act
            // Set initial scene
            sceneManager.LoadScene(initialScene);
            
            // Process updates to complete the first scene transition
            SimulateUpdates(sceneManager, 3);
            
            // Verify initial scene is loaded
            Assert.Equal(initialScene, sceneManager.CurrentScene);
            Assert.False(sceneManager.IsTransitioning);
            
            // Start transition to target scene
            sceneManager.LoadScene(targetScene);
            
            // Process updates to complete the second scene transition
            SimulateUpdates(sceneManager, 3);
            
            // Assert
            Assert.Equal(targetScene, sceneManager.CurrentScene);
            Assert.True(initialScene.WasUnloaded);
            Assert.False(sceneManager.IsTransitioning);
            Assert.Null(sceneManager.NextScene);
        }
        
        [Fact]
        public void TransitionWithLoadingScreen_WorksCorrectly()
        {
            // Arrange
            SceneManager sceneManager = new SceneManager();
            FastLoadScene initialScene = new FastLoadScene();
            SlowLoadScene targetScene = new SlowLoadScene();
            FastLoadScene loadingScene = new FastLoadScene();
            
            // Set initial scene
            sceneManager.LoadScene(initialScene);
            SimulateUpdates(sceneManager, 3);
            
            // Set loading screen
            sceneManager.SetLoadingScene(loadingScene);
            
            // Act - Start transition to target scene
            sceneManager.LoadScene(targetScene);
            
            // After a few updates, the loading screen should be active
            SimulateUpdates(sceneManager, 3);
            Assert.Equal(loadingScene, sceneManager.CurrentScene);
            Assert.True(sceneManager.IsTransitioning);
            Assert.Equal(targetScene, sceneManager.NextScene);
            
            // After more updates, the target scene should be loaded
            SimulateUpdates(sceneManager, 5);
            
            // Assert
            Assert.Equal(targetScene, sceneManager.CurrentScene);
            Assert.False(sceneManager.IsTransitioning);
            Assert.Null(sceneManager.NextScene);
        }
        
        [Fact]
        public void IsTransitioning_PreventsConcurrentSceneLoads()
        {
            // Arrange
            SceneManager sceneManager = new SceneManager();
            SlowLoadScene scene1 = new SlowLoadScene();
            SlowLoadScene scene2 = new SlowLoadScene();
            
            // Act
            sceneManager.LoadScene(scene1);
            
            // Should be in transition now
            Assert.True(sceneManager.IsTransitioning);
            
            // Try to load another scene during transition
            sceneManager.LoadScene(scene2);
            
            // Assert - scene2 should not have been loaded
            Assert.Equal(scene1, sceneManager.NextScene);
        }
        
        // Helper method to simulate multiple game updates
        private void SimulateUpdates(SceneManager sceneManager, int count)
        {
            var gameTime = new GameTime();
            for (int i = 0; i < count; i++)
            {
                CoroutineManager.Update(gameTime);
                sceneManager.Update(gameTime);
            }
        }
        
        // Helper class for testing Scene update/draw calls
        private class MockScene : Scene
        {
            public bool UpdateWasCalled { get; private set; }
            public bool DrawWasCalled { get; private set; }

            protected override GameSystem[] LoadGameSystems()
            {
                throw new NotImplementedException();
            }

            protected override void onStart()
            {
                throw new NotImplementedException();
            }
        }
    }
}