using Xunit;
using CoreEssentials.GameSystems;
using CoreEssentials.Scenes;
using Microsoft.Xna.Framework;
using System.Collections;

namespace CoreEssentials.Tests.GameSystems
{
    // Test GameSystem to verify OnStart is called
    public class TestOnStartGameSystem : GameSystem
    {
        public bool OnStartCalled { get; private set; } = false;

        public override void OnStart()
        {
            base.OnStart();
            OnStartCalled = true;
            System.Console.WriteLine("TestOnStartGameSystem.OnStart Called");
        }
    }

    // Test Scene to load the TestOnStartGameSystem
    public class TestSceneWithOnStartSystem : Scene
    {
        public TestOnStartGameSystem MyTestSystem { get; private set; }

        protected override GameSystem[] LoadGameSystems()
        {
            MyTestSystem = new TestOnStartGameSystem();
            return new GameSystem[] { MyTestSystem };
        }

        protected override IEnumerator OnStartCoroutine()
        {
            // Minimal implementation for testing
            UpdateLoadingProgress(1.0f, "Scene OnStartCoroutine complete");
            yield return null;
        }
    }

    public class GameSystemOnStartTests
    {
        [Fact]
        public void GameSystem_OnStart_IsCalledAfterSceneLoad()
        {
            // Arrange
            CoreEssentials.Coroutines.CoroutineManager.StopAllCoroutines(); // Ensure clean state
            var testScene = new TestSceneWithOnStartSystem();
            var gameTime = new GameTime();

            // Act
            testScene.Load(); // This starts the LoadCoroutine

            // Simulate a few game updates to allow the LoadCoroutine to progress
            // The LoadCoroutine has several 'yield return null' and other yields.
            // We need enough updates to get past the GameSystem registration and OnStart calls.
            for (int i = 0; i < 10; i++) // Increased loop for safety, adjust if needed
            {
                CoreEssentials.Coroutines.CoroutineManager.Update(gameTime);
                if (testScene.IsLoaded) break; // Exit early if loaded
            }

            // Assert
            Assert.True(testScene.IsLoaded, "Scene should be loaded.");
            Assert.NotNull(testScene.MyTestSystem);
            Assert.True(testScene.MyTestSystem.OnStartCalled, "OnStart should have been called on TestOnStartGameSystem.");
        }
    }
}
