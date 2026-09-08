using System;
using System.Collections;
using Microsoft.Xna.Framework;
using Xunit;
using CoreEssentials.GameSystems;
using CoreEssentials.Scenes;
using CoreEssentials.Tests.Coroutines;

namespace CoreEssentials.Tests.SceneManagement
{
    /// <summary>
    /// Sprint 2 — the core enforces the scene manifest. Name-based loads require a configured manifest
    /// and must reference registered entries; object-based loads remain an escape hatch. The transition
    /// coroutine is unfailable, so a missing/invalid manifest or an unregistered scene propagates (errors out).
    /// </summary>
    public class SceneManagerEnforcementTests
    {
        // A trivial zero-system scene used as the object-based escape hatch and as a loading screen.
        private class BlankScene : Scene
        {
            protected override IEnumerator OnStartCoroutine()
            {
                UpdateLoadingProgress(1.0f, "Loading complete");
                yield break;
            }

            protected override GameSystem[] LoadGameSystems() => new GameSystem[0];
        }

        [Fact]
        public void LoadSceneByName_NoManifest_ThrowsSynchronously()
        {
            var manager = new SceneManager();

            var ex = Assert.Throws<InvalidOperationException>(() => manager.LoadScene("HomeScene.xml"));
            Assert.Contains("No scene manifest", ex.Message);
        }

        [Fact]
        public void SetLoadingSceneByName_NoManifest_ThrowsSynchronously()
        {
            var manager = new SceneManager();

            var ex = Assert.Throws<InvalidOperationException>(() => manager.SetLoadingScene("loading.xml"));
            Assert.Contains("No scene manifest", ex.Message);
        }

        [Fact]
        public void LoadSceneByName_UnregisteredScene_ThrowsOnTransition()
        {
            // A manifest that does NOT include the requested scene. The asset file need not exist:
            // membership is enforced before the (deferred) scene parse, so it throws first.
            var manager = new SceneManager();
            manager.SetManifest(SceneManifestFixture.Build(
                new[] { new SceneManifestFixture.GameScene("HomeScene.xml") }));

            var helper = new CoroutineTestHelper();
            try
            {
                manager.LoadScene("NotRegistered.xml");
                Assert.True(manager.IsTransitioning);

                var ex = Record.Exception(() =>
                {
                    for (int i = 0; i < 5 && manager.IsTransitioning; i++)
                        helper.Tick();
                });

                // The unfailable transition coroutine rethrows the membership violation.
                Assert.NotNull(ex);
                Assert.Contains("NotRegistered.xml", ex!.Message);
                Assert.Contains("not registered", ex.Message);
            }
            finally
            {
                helper.Cleanup();
            }
        }

        [Fact]
        public void LoadSceneByName_ObjectBased_IsEscapeHatch_NoManifestNeeded()
        {
            // Object-based loads carry no asset name, so they are not gated by the manifest.
            var manager = new SceneManager();
            var scene = new BlankScene();

            var helper = new CoroutineTestHelper();
            try
            {
                manager.LoadScene(scene); // must not throw despite no manifest
                Assert.True(manager.IsTransitioning);

                for (int i = 0; i < 20 && manager.IsTransitioning; i++)
                {
                    helper.Tick();
                    manager.Update(new GameTime(TimeSpan.FromSeconds(i * 0.016), TimeSpan.FromSeconds(0.016)));
                }

                Assert.False(manager.IsTransitioning);
                Assert.Same(scene, manager.CurrentScene);
            }
            finally
            {
                helper.Cleanup();
            }
        }

        [Fact]
        public void PerSceneLoadingScreen_ExplicitAttribute_ResolvesThatScreen()
        {
            // Scene "T.xml" names its own loading screen "L1.xml". After the first transition tick, the
            // manager's loading scene is resolved to L1 (before any asset is parsed).
            var manager = new SceneManager();
            manager.SetManifest(SceneManifestFixture.Build(
                new[] { new SceneManifestFixture.GameScene("T.xml", LoadingScreen: "L1.xml") },
                defaultLoadingScreen: "L0.xml",
                extraLoadingScreens: new[] { "L1.xml" }));

            var helper = new CoroutineTestHelper();
            try
            {
                manager.LoadScene("T.xml");
                helper.Tick(); // runs step 0: resolve manifest + enforce + resolve loading screen

                Assert.IsType<DataDrivenScene>(manager.LoadingScene);
                Assert.Equal("L1.xml", ((DataDrivenScene)manager.LoadingScene!).AssetName);
            }
            finally
            {
                helper.Cleanup();
            }
        }

        [Fact]
        public void PerSceneLoadingScreen_NoAttribute_FallsBackToDefault()
        {
            var manager = new SceneManager();
            manager.SetManifest(SceneManifestFixture.Build(
                new[] { new SceneManifestFixture.GameScene("T.xml") },
                defaultLoadingScreen: "L0.xml"));

            var helper = new CoroutineTestHelper();
            try
            {
                manager.LoadScene("T.xml");
                helper.Tick();

                Assert.IsType<DataDrivenScene>(manager.LoadingScene);
                Assert.Equal("L0.xml", ((DataDrivenScene)manager.LoadingScene!).AssetName);
            }
            finally
            {
                helper.Cleanup();
            }
        }

        [Fact]
        public void PerSceneLoadingScreen_NoAttributeAndNoDefault_DoesDirectTransition()
        {
            // No loading screen declared anywhere for this scene → direct transition, no loading scene assigned.
            var manager = new SceneManager();
            manager.SetManifest(SceneManifestFixture.Build(
                new[] { new SceneManifestFixture.GameScene("T.xml") }));

            var helper = new CoroutineTestHelper();
            try
            {
                manager.LoadScene("T.xml");
                helper.Tick();

                Assert.Null(manager.LoadingScene);
            }
            finally
            {
                helper.Cleanup();
            }
        }

        [Fact]
        public void SetManifest_Null_Throws()
        {
            var manager = new SceneManager();
            Assert.Throws<ArgumentNullException>(() => manager.SetManifest(null!));
        }

        [Fact]
        public void SetManifestAsset_EmptyName_Throws()
        {
            var manager = new SceneManager();
            Assert.Throws<ArgumentNullException>(() => manager.SetManifestAsset("   "));
        }
    }
}
