using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Xunit;
using CoreEssentials.Assets;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Components.BuiltIn;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Serialization;
using CoreEssentials.GUI;
using CoreEssentials.GUI.Internal;
using CoreEssentials.Scenes;
using CoreEssentials.Tests.Coroutines;

namespace CoreEssentials.Tests.SceneManagement
{
    /// <summary>
    /// Sprint 5a — proves the boot-from-files pipeline against the REAL shipping assets
    /// (CoreEssentials.Playground/Content/loading.xml + HomeScene.xml), not inline fixtures:
    /// the loading screen parses in the strict format with a label + progress component, and a
    /// full transition through it into the placeholder home scene completes.
    /// </summary>
    public class BootFromFilesTests : IDisposable
    {
        private readonly Game _mockGame;

        public BootFromFilesTests()
        {
            // The loading/home scenes carry Canvas + Label components, which require the GUI engine
            // to be initialized (the real game does this at startup). Use a mock game with a known
            // screen rect, matching the other component tests.
            _mockGame = new Game1();
            GUIManager.Init(_mockGame, 1280, 720);
        }

        [Fact]
        public void RealLoadingXml_Parses_AsStrictScene_WithLabelAndProgress()
        {
            // Arrange — the actual file that ships in Content/
            var xml = ReadSourceContentFile("Scenes/loading.xml");

            // Act
            var scene = SceneParser.Parse(xml);

            // Assert — strict format: exactly one EntitySystem
            Assert.Single(scene.Systems);
            Assert.Equal(typeof(EntitySystem), scene.Systems[0].SystemType);

            // The label entity carries BOTH a LabelComponent and the TransitionProgressComponent,
            // which is what lets the progress component auto-sync the label text as a percentage.
            var labelEntity = FindEntityWithComponents(
                scene.Systems[0].Entities,
                new[] { "LabelComponent", "TransitionProgressComponent" });
            Assert.NotNull(labelEntity);
        }

        [Fact]
        public void Boot_FromRealFiles_TransitionCompletesAndSwapsScenes()
        {
            // Arrange — stage the REAL loading + home files into the content dir the AssetManager reads
            var loadingXml = ReadSourceContentFile("Scenes/loading.xml");
            var homeXml = ReadSourceContentFile("Scenes/HomeScene.xml");
            WriteContentAsset("Scenes/loading.xml", loadingXml);
            WriteContentAsset("Scenes/HomeScene.xml", homeXml);

            var helper = new CoroutineTestHelper();
            try
            {
                AssetManager.Init(new MockContentManager());
                var manager = new SceneManager();
                // Name-based loads are now gated by a manifest: register the home scene + its loading screen.
                manager.SetManifest(SceneManifestFixture.Build(
                    new[] { new SceneManifestFixture.GameScene("Scenes/HomeScene.xml") },
                    defaultLoadingScreen: "Scenes/loading.xml"));
                manager.SetLoadingScene("Scenes/loading.xml");
                manager.LoadScene("Scenes/HomeScene.xml");

                Assert.True(manager.IsTransitioning);

                // Act — drive the transition to completion, exactly as Program.cs boots the game.
                // Sample the loading screen's progress while it is still live (before the swap unloads it).
                var loadingScreen = manager.LoadingScene as DataDrivenScene;
                Assert.NotNull(loadingScreen);
                float lastProgress = 0f;
                for (int i = 0; i < 40 && manager.IsTransitioning; i++)
                {
                    helper.Tick();
                    manager.Update(new GameTime(TimeSpan.FromSeconds(i * 0.016), TimeSpan.FromSeconds(0.016)));

                    if (loadingScreen!.IsLoaded)
                        lastProgress = ProgressOf(loadingScreen);
                }

                // Assert — the transition completed and the home data scene is now current
                Assert.False(manager.IsTransitioning);
                Assert.Null(manager.PendingScene);
                var current = manager.CurrentScene as DataDrivenScene;
                Assert.NotNull(current);
                var entitySystem = current.GetGameSystem<EntitySystem>();
                var title = entitySystem.FindById("homeTitle");
                Assert.NotNull(title);

                // The data-driven loading screen's progress component observed the whole transition up to 1.0.
                // It is now unloaded (so its canvas stops rendering), but we captured its final live value.
                Assert.Equal(1.0f, lastProgress);
            }
            finally
            {
                helper.Cleanup();
            }
        }

        private bool _disposed;

        public void Dispose()
        {
            if (_disposed) return;
            _mockGame?.Dispose();
            EngineResolver.GetEngine()?.Shutdown();
            _disposed = true;
            GC.SuppressFinalize(this);
        }

        // ──────────────────────────── Helpers ────────────────────────────

        private static float ProgressOf(DataDrivenScene scene)
        {
            var entitySystem = scene.GetGameSystem<EntitySystem>();
            var entity = entitySystem.FindById("loadingLabel");
            return entity!.GetComponent<TransitionProgressComponent>()!.Progress;
        }

        /// <summary>Recursively finds the first entity whose declared components include all of the given types.</summary>
        private static EntityDefinition? FindEntityWithComponents(
            IEnumerable<EntityDefinition> entities, string[] requiredTypes)
        {
            foreach (var entity in entities)
            {
                var declared = new HashSet<string>();
                foreach (var component in entity.DeclaredComponents)
                    declared.Add(component.Type);

                bool hasAll = true;
                foreach (var type in requiredTypes)
                    if (!declared.Contains(type)) { hasAll = false; break; }

                if (hasAll) return entity;

                var nested = FindEntityWithComponents(entity.Children, requiredTypes);
                if (nested != null) return nested;
            }
            return null;
        }

        /// <summary>Resolves the real source-tree Content file by walking up from the test output directory
        /// until it finds CoreEssentials.Playground/Content/{name}.</summary>
        private static string ReadSourceContentFile(string name)
        {
            var dir = new DirectoryInfo(AppContext.BaseDirectory);
            for (int i = 0; i < 12 && dir != null; i++)
            {
                var candidate = Path.Combine(dir.FullName, "CoreEssentials.Playground", "Content", name);
                if (File.Exists(candidate))
                    return File.ReadAllText(candidate);

                dir = dir.Parent;
            }

            throw new FileNotFoundException(
                $"Could not locate source Content file '{name}' under CoreEssentials.Playground/Content.", name);
        }

        private static void WriteContentAsset(string fileName, string xml)
        {
            var filePath = Path.Combine(AppContext.BaseDirectory, "Content", fileName);
            Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
            File.WriteAllText(filePath, xml);
        }
    }
}
