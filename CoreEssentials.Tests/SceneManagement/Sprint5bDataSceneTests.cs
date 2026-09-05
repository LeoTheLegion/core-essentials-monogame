using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Xunit;
using CoreEssentials.Assets;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Components.BuiltIn;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Serialization;
using CoreEssentials.GUI;
using CoreEssentials.GUI.Internal;
using CoreEssentials.Playground;
using CoreEssentials.Playground.Entities;
using CoreEssentials.Playground.Components;
using CoreEssentials.Scenes;
using CoreEssentials.Tests.Coroutines;

namespace CoreEssentials.Tests.SceneManagement
{
    /// <summary>
    /// Sprint 5b — proves the two migrated "easy" demo scenes run entirely from data: the real
    /// shipping XML files parse in the strict format, load as a DataDrivenScene, and expose their
    /// entities/components (including camera speed + declarative navigation targets) with no C# subclass.
    /// </summary>
    public class Sprint5bDataSceneTests : IDisposable
    {
        private readonly Game _mockGame;

        public Sprint5bDataSceneTests()
        {
            // GuiAnchorDemo carries Canvas/Label/Button components, which require the GUI engine to be
            // initialized (the real game does this at startup). Use a mock game with a known screen rect.
            _mockGame = new Game1();
            GUIManager.Init(_mockGame, 1280, 720);
        }

        [Fact]
        public void GuiAnchorDemo_Parses_AsStrictScene_WithCameraAndNavigation()
        {
            var xml = ReadSourceContentFile("GuiAnchorDemo.xml");

            var scene = SceneParser.Parse(xml);

            // Strict format: exactly one EntitySystem.
            Assert.Single(scene.Systems);
            Assert.Equal(typeof(EntitySystem), scene.Systems[0].SystemType);

            // The camera is a data entity with its speed overridden (default 1 is imperceptible).
            var camera = FindById(scene.Systems[0].Entities, "camera");
            Assert.NotNull(camera);
            Assert.Contains("CameraSpeed", camera!.EntityOverrides.Keys);
            Assert.Equal("300", camera.EntityOverrides["CameraSpeed"]);

            // Navigation targets are scene asset-name strings (no C# Type references).
            var navPhysics = FindById(scene.Systems[0].Entities, "navPhysics");
            var navSend = FindById(scene.Systems[0].Entities, "navSendMessage");
            Assert.NotNull(navPhysics);
            Assert.NotNull(navSend);
            Assert.Equal("PhysicsEntityScene.xml", NavTarget(navPhysics!));
            Assert.Equal("SendMessageDemoScene.xml", NavTarget(navSend!));
        }

        [Fact]
        public void GuiAnchorDemo_Loads_AsDataDrivenScene_WithHudAndCamera()
        {
            StageContentFile("GuiAnchorDemo.xml");

            var helper = new CoroutineTestHelper();
            try
            {
                AssetManager.Init(new MockContentManager());
                var scene = new DataDrivenScene(SceneParser.LoadFromAsset("GuiAnchorDemo.xml"));

                scene.Load();
                for (int i = 0; i < 40 && !scene.IsLoaded; i++)
                    helper.Tick();

                Assert.True(scene.IsLoaded);
                var entitySystem = scene.GetGameSystem<EntitySystem>();

                // HUD root carries the canvas + score state.
                var hud = entitySystem.FindById("hud");
                Assert.NotNull(hud);
                Assert.NotNull(hud!.GetComponent<CanvasComponent>());
                Assert.Equal(new Vector2(0, 0), hud.Position);

                // The camera loaded as a real CameraEntity with the overridden speed.
                var camera = entitySystem.FindById("camera") as CameraEntity;
                Assert.NotNull(camera);
                Assert.Equal(300f, camera!.CameraSpeed);
            }
            finally
            {
                helper.Cleanup();
            }
        }

        [Fact]
        public void SendMessageDemo_Parses_AsStrictScene_WithPrefabAndControls()
        {
            var xml = ReadSourceContentFile("SendMessageDemoScene.xml");

            var scene = SceneParser.Parse(xml);

            Assert.Single(scene.Systems);
            Assert.Equal(typeof(EntitySystem), scene.Systems[0].SystemType);

            // PingPrefab is registered from its template asset.
            var systemDef = scene.Systems[0];
            Assert.Single(systemDef.Prefabs);
            Assert.Equal("PingPrefab", systemDef.Prefabs[0].Name);
            Assert.Equal("PingPrefabTemplate.xml", systemDef.Prefabs[0].Asset);

            // The control component declares the staggered spawn position.
            var control = FindById(systemDef.Entities, "pingControl");
            Assert.NotNull(control);
            var comp = control!.DeclaredComponents.Find(c => c.Type.Contains("PingControlComponent"));
            Assert.NotNull(comp);
            Assert.Equal("640,450", comp!.Properties["SpawnPosition"]);

            // Navigation target is a scene asset-name string.
            var nav = FindById(systemDef.Entities, "navCharacter");
            Assert.NotNull(nav);
            Assert.Equal("CharacterScene.xml", NavTarget(nav!));
        }

        [Fact]
        public void SendMessageDemo_Loads_AsDataDrivenScene_WithReceiversAndNestedChild()
        {
            StageContentFile("SendMessageDemoScene.xml");
            StageContentFile("PingPrefabTemplate.xml");

            var helper = new CoroutineTestHelper();
            try
            {
                // PingReceiverComponent.OnAttach loads the "base" font — register a mock for it.
                var content = new MockContentManager();
                content.AddAsset<SpriteFont>("base", CoreEssentials.Tests.MockSpriteFont.Instance);
                AssetManager.Init(content);
                var scene = new DataDrivenScene(SceneParser.LoadFromAsset("SendMessageDemoScene.xml"));

                scene.Load();
                for (int i = 0; i < 40 && !scene.IsLoaded; i++)
                    helper.Tick();

                Assert.True(scene.IsLoaded);
                var entitySystem = scene.GetGameSystem<EntitySystem>();

                // Prefab registered before entities instantiated.
                Assert.True(entitySystem.HasPrefab("PingPrefab"));

                // Root receiver + its label.
                var root = entitySystem.FindById("rootReceiver");
                Assert.NotNull(root);
                Assert.Equal("root receiver", root!.GetComponent<PingReceiverComponent>()!.Label);

                // Nested child keeps its authored offset from its parent (Gap 1 regression guard).
                var nestedParent = entitySystem.FindById("nestedParent");
                var nested = entitySystem.FindById("nestedReceiver");
                Assert.NotNull(nestedParent);
                Assert.NotNull(nested);
                Assert.Same(nestedParent, nested!.Parent);
                Assert.Equal(new Vector2(80, 0), nested.LocalPosition);
                Assert.Equal(new Vector2(800, 300), nested.Position);

                // The control + navigation components attached to their shells.
                var control = entitySystem.FindById("pingControl");
                Assert.NotNull(control);
                Assert.NotNull(control!.GetComponent<PingControlComponent>());
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

        /// <summary>Finds an entity by Id in a definition tree (recursing into nested children).</summary>
        private static EntityDefinition? FindById(System.Collections.Generic.List<EntityDefinition> entities, string id)
        {
            foreach (var e in entities)
            {
                if (e.Id == id) return e;
                var nested = FindById(e.Children, id);
                if (nested != null) return nested;
            }
            return null;
        }

        /// <summary>Reads the TargetSceneAsset property of a NavigateOnKeyComponent on a definition.</summary>
        private static string NavTarget(EntityDefinition def)
        {
            var comp = def.DeclaredComponents.Find(c => c.Type.Contains("NavigateOnKeyComponent"));
            Assert.NotNull(comp);
            return comp!.Properties["TargetSceneAsset"];
        }

        /// <summary>Copies a real source-tree Content file into the content dir the AssetManager reads.</summary>
        private static void StageContentFile(string name)
        {
            WriteContentAsset(name, ReadSourceContentFile(name));
        }

        /// <summary>Resolves the real source-tree Content file by walking up from the test output directory.</summary>
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
            var contentDir = Path.Combine(AppContext.BaseDirectory, "Content");
            Directory.CreateDirectory(contentDir);
            File.WriteAllText(Path.Combine(contentDir, fileName), xml);
        }
    }
}
