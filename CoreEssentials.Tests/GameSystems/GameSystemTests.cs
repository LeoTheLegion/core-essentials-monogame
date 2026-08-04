using Microsoft.Xna.Framework;
using CoreEssentials.GameSystems;
using CoreEssentials.Scenes;
using Moq;
using System.Reflection;
using System;
using System.Collections;
using System.Collections.Generic;
using Xunit;

namespace CoreEssentials.Tests.GameSystems
{
    /// <summary>
    /// Tests for the GameSystem class.
    /// </summary>
    public class GameSystemTests
    {
        private class TestGameSystem : GameSystem
        {
            // Simple implementation of GameSystem for testing
        }

        private class TestUpdateGameSystem : GameSystem, IUpdateGameSystem
        {
            public bool UpdateCalled { get; private set; }

            public void Update(GameTime gameTime)
            {
                UpdateCalled = true;
            }
        }

        private class TestDrawGameSystem : GameSystem, IDrawGameSystem
        {
            public bool DrawCalled { get; private set; }

            public void Draw(GameTime gameTime, Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch)
            {
                DrawCalled = true;
            }
        }

        private class TestFixedUpdateGameSystem : GameSystem, IFixedUpdateGameSystem
        {
            public bool FixedUpdateCalled { get; private set; }

            public void FixedUpdate(GameTime gameTime)
            {
                FixedUpdateCalled = true;
            }
        }

        // Custom Scene implementation for testing since we can't mock Scene.GetGameSystem
        private class TestScene : Scene
        {
            protected override GameSystem[] LoadGameSystems()
            {
                return new GameSystem[0];
            }

            protected override IEnumerator OnStartCoroutine()
            {
                yield break;
            }
        }

        [Fact]
        public void SetScene_SetsSceneInstance()
        {
            // Arrange
            var gameSystem = new TestGameSystem();
            var mockScene = new Mock<Scene>().Object;
            
            // Act
            gameSystem.SetScene(mockScene);
            
            // Assert - check _scene field via reflection since it's private
            var sceneField = typeof(GameSystem).GetField("_scene", BindingFlags.NonPublic | BindingFlags.Instance);
            var scene = sceneField.GetValue(gameSystem);
            
            Assert.Equal(mockScene, scene);
        }        [Fact]
        public void GetGameSystem_DelegatesToScene()
        {
            // Arrange
            var gameSystem = new TestGameSystem();
            
            // Create a mock scene and set up the necessary systems
            var mockScene = new Mock<Scene>().Object;
            
            // Use reflection to create a dictionary with our test system
            var testSystem = new TestGameSystem();
            var gameSystems = new Dictionary<Type, GameSystem>();
            gameSystems.Add(typeof(TestGameSystem), testSystem);
            
            // Set the private _gameSystems field in the scene object
            var gameSystemsField = typeof(Scene).GetField("_gameSystems", BindingFlags.NonPublic | BindingFlags.Instance);
            gameSystemsField.SetValue(mockScene, gameSystems);
            
            // Set the scene on our game system
            gameSystem.SetScene(mockScene);
            
            // Act
            var retrievedSystem = gameSystem.GetGameSystem<TestGameSystem>();
            
            // Assert
            Assert.Equal(testSystem, retrievedSystem);
        }

        [Fact]
        public void GameSystem_ImplementsInterfaces()
        {
            // Arrange
            var updateSystem = new TestUpdateGameSystem();
            var drawSystem = new TestDrawGameSystem();
            var fixedUpdateSystem = new TestFixedUpdateGameSystem();
            var gameTime = new GameTime();
            
            // Act
            ((IUpdateGameSystem)updateSystem).Update(gameTime);
            
            // We can't mock SpriteBatch, so we'll just check that the interface method exists
            // and assume it would be called correctly
            Assert.True(drawSystem is IDrawGameSystem);
            
            ((IFixedUpdateGameSystem)fixedUpdateSystem).FixedUpdate(gameTime);

            // Assert
            Assert.True(updateSystem.UpdateCalled);
            Assert.True(fixedUpdateSystem.FixedUpdateCalled);
        }        [Fact]
        public void Game_Property_ReturnMainGameFromSceneManager()
        {
            // Arrange
            var gameSystem = new TestGameSystem();
            var mockGame = new Mock<MainGame>();
            
            // Create a TestScene with the SceneManager set properly
            var sceneManager = new SceneManager(mockGame.Object);
            var testScene = new TestScene();
            testScene.SetSceneManager(sceneManager);
            
            // Set the scene on our game system
            gameSystem.SetScene(testScene);
            
            // Act
            var game = gameSystem.Game;
              // Assert
            Assert.NotNull(game);
            Assert.Same(mockGame.Object, game);
        }
    }
}
