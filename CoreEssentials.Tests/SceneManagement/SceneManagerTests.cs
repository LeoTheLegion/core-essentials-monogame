using System;
using Xunit;
using Microsoft.Xna.Framework;
using CoreEssentials.SceneManagement;
using CoreEssentials.GameSystems;
using Microsoft.Xna.Framework.Graphics;

namespace CoreEssentials.Tests.GameSystems.SceneManagement
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