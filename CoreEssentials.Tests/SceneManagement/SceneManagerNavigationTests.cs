using System;
using System.Collections;
using System.IO;
using Microsoft.Xna.Framework;
using Xunit;
using CoreEssentials.Assets;
using CoreEssentials.GameSystems;
using CoreEssentials.Scenes;
using CoreEssentials.Tests.Coroutines;

namespace CoreEssentials.Tests.SceneManagement
{
    /// <summary>
    /// Sprint 3 — ordered navigation on SceneManager. NextScene()/PreviousScene() move ±1 through the
    /// manifest's &lt;GameScenes&gt; list (clamped at both ends), route through the normal transition path
    /// (so per-scene loading screens apply), are no-ops when untracked, and fire SceneAdvanced/SceneRetreated
    /// once the new scene is current.
    /// </summary>
    public class SceneManagerNavigationTests
    {
        // A trivial zero-system scene used for object-based (untracked) state.
        private class BlankScene : Scene
        {
            protected override IEnumerator OnStartCoroutine()
            {
                UpdateLoadingProgress(1.0f, "Loading complete");
                yield break;
            }

            protected override GameSystem[] LoadGameSystems() => new GameSystem[0];
        }

        // ──────────────────────────── Happy paths ────────────────────────────

        [Fact]
        public void NextScene_FromFirst_MovesToSecond_AndFiresSceneAdvanced()
        {
            WriteScenes("NavA.xml", "NavB.xml");
            AssetManager.Init(new MockContentManager());
            var manager = new SceneManager();
            manager.SetManifest(SceneManifestFixture.Build(
                new[] { new SceneManifestFixture.GameScene("NavA.xml"), new SceneManifestFixture.GameScene("NavB.xml") }));

            var helper = new CoroutineTestHelper();
            try
            {
                LoadToCompletion(manager, "NavA.xml", helper);

                string? advancedWith = null;
                manager.SceneAdvanced += name => advancedWith = name;

                // Act
                manager.NextScene();
                Assert.True(manager.IsTransitioning);
                TickUntilSettled(manager, helper);

                // Assert — moved to the second entry and the event fired with its name
                Assert.False(manager.IsTransitioning);
                Assert.Equal("NavB.xml", CurrentAssetName(manager));
                Assert.Equal("NavB.xml", advancedWith);
            }
            finally
            {
                helper.Cleanup();
            }
        }

        [Fact]
        public void PreviousScene_FromLast_MovesBack_AndFiresSceneRetreated()
        {
            WriteScenes("NavP1.xml", "NavP2.xml");
            AssetManager.Init(new MockContentManager());
            var manager = new SceneManager();
            manager.SetManifest(SceneManifestFixture.Build(
                new[] { new SceneManifestFixture.GameScene("NavP1.xml"), new SceneManifestFixture.GameScene("NavP2.xml") }));

            var helper = new CoroutineTestHelper();
            try
            {
                LoadToCompletion(manager, "NavP2.xml", helper); // start at the last entry

                string? retreatedWith = null;
                manager.SceneRetreated += name => retreatedWith = name;

                // Act
                manager.PreviousScene();
                Assert.True(manager.IsTransitioning);
                TickUntilSettled(manager, helper);

                // Assert — moved back to the first entry and the event fired with its name
                Assert.False(manager.IsTransitioning);
                Assert.Equal("NavP1.xml", CurrentAssetName(manager));
                Assert.Equal("NavP1.xml", retreatedWith);
            }
            finally
            {
                helper.Cleanup();
            }
        }

        [Fact]
        public void NextScene_ThenPreviousScene_RoundTripsThroughMiddle()
        {
            WriteScenes("NavR1.xml", "NavR2.xml", "NavR3.xml");
            AssetManager.Init(new MockContentManager());
            var manager = new SceneManager();
            manager.SetManifest(SceneManifestFixture.Build(
                new[]
                {
                    new SceneManifestFixture.GameScene("NavR1.xml"),
                    new SceneManifestFixture.GameScene("NavR2.xml"),
                    new SceneManifestFixture.GameScene("NavR3.xml"),
                }));

            var helper = new CoroutineTestHelper();
            try
            {
                LoadToCompletion(manager, "NavR1.xml", helper);

                manager.NextScene();
                TickUntilSettled(manager, helper);
                Assert.Equal("NavR2.xml", CurrentAssetName(manager));

                manager.PreviousScene();
                TickUntilSettled(manager, helper);
                Assert.Equal("NavR1.xml", CurrentAssetName(manager));
            }
            finally
            {
                helper.Cleanup();
            }
        }

        // ──────────────────────────── Clamping ────────────────────────────

        [Fact]
        public void NextScene_OnLastScene_IsNoOp()
        {
            WriteScenes("NavL1.xml", "NavL2.xml");
            AssetManager.Init(new MockContentManager());
            var manager = new SceneManager();
            manager.SetManifest(SceneManifestFixture.Build(
                new[] { new SceneManifestFixture.GameScene("NavL1.xml"), new SceneManifestFixture.GameScene("NavL2.xml") }));

            var helper = new CoroutineTestHelper();
            try
            {
                LoadToCompletion(manager, "NavL2.xml", helper);

                // Act — already at the end: clamped no-op, no transition started, nothing thrown
                Assert.Null(Record.Exception(() => manager.NextScene()));

                Assert.False(manager.IsTransitioning);
                Assert.Equal("NavL2.xml", CurrentAssetName(manager));
            }
            finally
            {
                helper.Cleanup();
            }
        }

        [Fact]
        public void PreviousScene_OnFirstScene_IsNoOp()
        {
            WriteScenes("NavF1.xml", "NavF2.xml");
            AssetManager.Init(new MockContentManager());
            var manager = new SceneManager();
            manager.SetManifest(SceneManifestFixture.Build(
                new[] { new SceneManifestFixture.GameScene("NavF1.xml"), new SceneManifestFixture.GameScene("NavF2.xml") }));

            var helper = new CoroutineTestHelper();
            try
            {
                LoadToCompletion(manager, "NavF1.xml", helper);

                // Act — already at the start: clamped no-op
                Assert.Null(Record.Exception(() => manager.PreviousScene()));

                Assert.False(manager.IsTransitioning);
                Assert.Equal("NavF1.xml", CurrentAssetName(manager));
            }
            finally
            {
                helper.Cleanup();
            }
        }

        // ──────────────────────────── No-op guards ────────────────────────────

        [Fact]
        public void NextScene_NoManifest_IsNoOp()
        {
            var manager = new SceneManager();

            // Act — no manifest configured at all: console note, not an exception
            Assert.Null(Record.Exception(() => manager.NextScene()));

            Assert.False(manager.IsTransitioning);
            Assert.Null(manager.CurrentScene);
        }

        [Fact]
        public void NextScene_CurrentSceneNotTracked_IsNoOp_UntilListedSceneBecomesCurrent()
        {
            WriteScenes("NavU1.xml", "NavU2.xml");
            AssetManager.Init(new MockContentManager());
            var manager = new SceneManager();
            manager.SetManifest(SceneManifestFixture.Build(
                new[] { new SceneManifestFixture.GameScene("NavU1.xml"), new SceneManifestFixture.GameScene("NavU2.xml") }));

            var helper = new CoroutineTestHelper();
            try
            {
                // An object-based (untracked) scene is current — navigation must be a no-op.
                manager.LoadScene(new BlankScene());
                TickUntilSettled(manager, helper);
                Assert.False(manager.IsTransitioning);

                Assert.Null(Record.Exception(() => manager.NextScene()));
                Assert.False(manager.IsTransitioning);

                // Once a listed scene becomes current, navigation works again (untracked → tracked).
                LoadToCompletion(manager, "NavU1.xml", helper);
                manager.NextScene();
                TickUntilSettled(manager, helper);
                Assert.Equal("NavU2.xml", CurrentAssetName(manager));
            }
            finally
            {
                helper.Cleanup();
            }
        }

        [Fact]
        public void NextScene_WhileTransitioning_IsNoOp()
        {
            WriteScenes("NavW1.xml", "NavW2.xml");
            AssetManager.Init(new MockContentManager());
            var manager = new SceneManager();
            manager.SetManifest(SceneManifestFixture.Build(
                new[] { new SceneManifestFixture.GameScene("NavW1.xml"), new SceneManifestFixture.GameScene("NavW2.xml") }));

            var helper = new CoroutineTestHelper();
            try
            {
                LoadToCompletion(manager, "NavW1.xml", helper);

                // Start a transition, then try to navigate on top of it.
                manager.LoadScene("NavW2.xml");
                Assert.True(manager.IsTransitioning);

                string? advancedWith = null;
                manager.SceneAdvanced += name => advancedWith = name;
                Assert.Null(Record.Exception(() => manager.NextScene()));

                TickUntilSettled(manager, helper);

                // The original transition completed; the navigation attempt did not fire its event.
                Assert.Equal("NavW2.xml", CurrentAssetName(manager));
                Assert.Null(advancedWith);
            }
            finally
            {
                helper.Cleanup();
            }
        }

        // ──────────────────────── Loading-screen routing ────────────────────────

        [Fact]
        public void NextScene_RoutesThroughPerSceneLoadingScreenResolution()
        {
            // Both loading screens must exist on disk: the default one is used for the initial load of
            // NavS1, and NavS2's own screen is resolved when navigating into it.
            WriteScenes("NavS1.xml", "NavS2.xml", "NavDefaultScreen.xml", "NavS2Screen.xml");
            AssetManager.Init(new MockContentManager());
            var manager = new SceneManager();
            manager.SetManifest(SceneManifestFixture.Build(
                new[]
                {
                    new SceneManifestFixture.GameScene("NavS1.xml"),
                    new SceneManifestFixture.GameScene("NavS2.xml", LoadingScreen: "NavS2Screen.xml"),
                },
                defaultLoadingScreen: "NavDefaultScreen.xml",
                extraLoadingScreens: new[] { "NavS2Screen.xml" }));

            var helper = new CoroutineTestHelper();
            try
            {
                LoadToCompletion(manager, "NavS1.xml", helper);

                // Act — navigating INTO NavS2 must resolve its per-scene loading screen.
                manager.NextScene();
                helper.Tick(); // runs step 0: manifest + membership + loading-screen resolution

                Assert.True(manager.IsTransitioning);
                Assert.IsType<DataDrivenScene>(manager.LoadingScene);
                Assert.Equal("NavS2Screen.xml", ((DataDrivenScene)manager.LoadingScene!).AssetName);

                TickUntilSettled(manager, helper);
                Assert.Equal("NavS2.xml", CurrentAssetName(manager));
            }
            finally
            {
                helper.Cleanup();
            }
        }

        // ──────────────────────────── Helpers ────────────────────────────

        private static string? CurrentAssetName(SceneManager manager)
            => manager.CurrentScene is DataDrivenScene dds ? dds.AssetName : null;

        /// <summary>Loads a named scene to completion, driving the transition coroutine each tick.</summary>
        private static void LoadToCompletion(SceneManager manager, string sceneName, CoroutineTestHelper helper)
        {
            manager.LoadScene(sceneName);
            TickUntilSettled(manager, helper);
            Assert.False(manager.IsTransitioning);
        }

        /// <summary>Ticks the coroutine owner (and the current scene) until no transition is in progress.</summary>
        private static void TickUntilSettled(SceneManager manager, CoroutineTestHelper helper)
        {
            for (int i = 0; i < 40 && manager.IsTransitioning; i++)
            {
                helper.Tick();
                manager.Update(new GameTime(TimeSpan.FromSeconds(i * 0.016), TimeSpan.FromSeconds(0.016)));
            }
        }

        /// <summary>Writes minimal zero-system scene XML assets for the given file names.</summary>
        private static void WriteScenes(params string[] names)
        {
            foreach (var name in names)
                WriteContentAsset(name, "<Scene><GameSystems /></Scene>");
        }

        private static void WriteContentAsset(string fileName, string xml)
        {
            var contentDir = Path.Combine(AppContext.BaseDirectory, "Content");
            Directory.CreateDirectory(contentDir);
            File.WriteAllText(Path.Combine(contentDir, fileName), xml);
        }
    }
}
