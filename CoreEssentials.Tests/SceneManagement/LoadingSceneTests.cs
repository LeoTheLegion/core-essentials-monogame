#nullable enable
using CoreEssentials.GameSystems;
using CoreEssentials.Scenes;
using Microsoft.Xna.Framework;
using Xunit;

namespace CoreEssentials.Tests.SceneManagement
{
    /// <summary>
    /// Device-free tests for <see cref="LoadingScene"/>: constructors, property defaults, the (empty)
    /// system load, the start-coroutine progress jump, and the <see cref="ColorExtensions.WithAlpha"/>
    /// helper. The <c>Draw</c> body is intentionally left uncovered — it needs a real graphics device.
    /// </summary>
    public class LoadingSceneTests
    {
        private sealed class ExposedLoadingScene : LoadingScene
        {
            // Let the test drive the protected start coroutine device-free.
            public System.Collections.IEnumerator Start() => OnStartCoroutine();
        }

        [Fact]
        public void DefaultConstructor_AppliesDocumentedDefaults()
        {
            var scene = new LoadingScene();

            Assert.Equal("Loading...", scene.LoadingText);
            Assert.Equal(Color.Black, scene.BackgroundColor);
            Assert.Equal(Color.White, scene.LoadingBarColor);
            Assert.Equal(Color.White, scene.TextColor);
        }

        [Fact]
        public void ParameterizedConstructor_AppliesAllFourValues()
        {
            var scene = new LoadingScene("Preparing…", Color.CornflowerBlue, Color.Gold, Color.OrangeRed);

            Assert.Equal("Preparing…", scene.LoadingText);
            Assert.Equal(Color.CornflowerBlue, scene.BackgroundColor);
            Assert.Equal(Color.Gold, scene.LoadingBarColor);
            Assert.Equal(Color.OrangeRed, scene.TextColor);
        }

        [Fact]
        public void LoadGameSystems_ReturnsEmptyArray()
        {
            var scene = new ExposedLoadingScene();
            // LoadGameSystems is protected; drive it through a scene that exposes it.
            var systems = SceneSystemAccessor.Load(scene);

            Assert.Empty(systems);
        }

        [Fact]
        public void StartCoroutine_SetsProgressToFull()
        {
            var scene = new ExposedLoadingScene();

            var e = scene.Start();
            bool completed = e.MoveNext(); // runs to the progress assignment, then `yield break`

            Assert.Equal(1.0f, scene.LoadingProgress);
            Assert.False(completed); // already ended (no further frames to yield)
        }

        [Fact]
        public void WithAlpha_KeepsRgbAndScalesAlpha()
        {
            var c = new Color(10, 20, 30, 255).WithAlpha(0.5f);

            Assert.Equal(10, c.R);
            Assert.Equal(20, c.G);
            Assert.Equal(30, c.B);
            Assert.Equal((byte)(255 * 0.5f), c.A);
        }

        [Fact]
        public void WithAlpha_FullAndZero()
        {
            var full = new Color(1, 2, 3, 4).WithAlpha(1.0f);
            Assert.Equal(255, full.A);

            var none = new Color(1, 2, 3, 255).WithAlpha(0.0f);
            Assert.Equal(0, none.A);
        }

        // LoadGameSystems is protected; expose it for the empty-array assertion.
        private static class SceneSystemAccessor
        {
            public static GameSystem[] Load(Scene scene)
            {
                var m = typeof(Scene).GetMethod("LoadGameSystems", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;
                return (GameSystem[])m.Invoke(scene, null)!;
            }
        }
    }
}
