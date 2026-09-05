using System;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using Xunit;
using CoreEssentials.Assets;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Serialization;
using CoreEssentials.GameSystems.Physics.Engines.Aether;
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
    /// Sprint 5c — proves the physics demo scene runs entirely from data: the real shipping
    /// PhysicsEntityScene.xml parses in the strict format (three systems, in document order), and
    /// loads as a DataDrivenScene that creates the Aether engine from its config asset, registers the
    /// ball prefab, spawns balls + world border, wires the save/load GUI, exposes the F1 debug
    /// overlay, and instantiates the parameterless PhysicsDebugRenderer — all with no C# subclass.
    /// </summary>
    public class Sprint5cPhysicsDataSceneTests : IDisposable
    {
        private readonly Game _mockGame;

        public Sprint5cPhysicsDataSceneTests()
        {
            // SaveLoadButtonsComponent creates GUI widgets on attach — the GUI engine must be up.
            _mockGame = new Game1();
            GUIManager.Init(_mockGame, 1280, 720);
        }

        [Fact]
        public void PhysicsEntityScene_Parses_AsStrictScene_WithThreeSystemsInOrder()
        {
            var xml = ReadSourceContentFile("Scenes/PhysicsEntityScene.xml");

            var scene = SceneParser.Parse(xml);

            // Three systems, in document order: engine (from config), entity system, debug renderer.
            Assert.Equal(3, scene.Systems.Count);

            Assert.Equal(typeof(PhysicsEngine), scene.Systems[0].SystemType);
            Assert.Equal("Config/PhysicsConfig.xml", scene.Systems[0].ConfigAsset);

            Assert.Equal(typeof(EntitySystem), scene.Systems[1].SystemType);

            // The debug renderer is created parameterless (no config).
            Assert.Equal(typeof(PhysicsDebugRenderer), scene.Systems[2].SystemType);
            Assert.Null(scene.Systems[2].ConfigAsset);
        }

        [Fact]
        public void PhysicsEntityScene_Parses_BallPrefabAndComponentKnobs()
        {
            var xml = ReadSourceContentFile("Scenes/PhysicsEntityScene.xml");

            var scene = SceneParser.Parse(xml);
            var entitySystem = scene.Systems[1];

            // The ball prefab is registered from its template asset.
            Assert.Single(entitySystem.Prefabs);
            Assert.Equal("BallPrefab", entitySystem.Prefabs[0].Name);
            Assert.Equal("Templates/BallTemplate.xml", entitySystem.Prefabs[0].Asset);

            // The spawner declares every knob explicitly (self-documenting data).
            var spawn = FindById(entitySystem.Entities, "ballSpawner");
            Assert.NotNull(spawn);
            var spawnComp = spawn!.DeclaredComponents.Find(c => c.Type.Contains("PhysicsSpawnComponent"));
            Assert.NotNull(spawnComp);
            Assert.Equal("BallPrefab", spawnComp!.Properties["BallPrefabName"]);
            Assert.Equal("5", spawnComp.Properties["RegularBallCount"]);
            Assert.Equal("Player", spawnComp.Properties["RegularCategory"]);
            Assert.Equal("vip_ball_blue,vip_ball_green,vip_ball_red", spawnComp.Properties["VipBallIds"]);
            Assert.Equal("true", spawnComp.Properties["CreateWorldBorder"]);

            // The save/load buttons declare their file path.
            var buttons = FindById(entitySystem.Entities, "saveLoadButtons");
            Assert.NotNull(buttons);
            var btnComp = buttons!.DeclaredComponents.Find(c => c.Type.Contains("SaveLoadButtonsComponent"));
            Assert.NotNull(btnComp);
            Assert.Equal("PhysicsScene_Save.xml", btnComp!.Properties["SaveFilePath"]);

            // The debug overlay declares its toggle key.
            var overlay = FindById(entitySystem.Entities, "debugOverlay");
            Assert.NotNull(overlay);
            Assert.Contains("F1", overlay!.DeclaredComponents.Find(c => c.Type.Contains("PhysicsDebugOverlayComponent"))!.Properties["ToggleKey"]);

            // Navigation target is a scene asset-name string (no C# Type reference).
            var nav = FindById(entitySystem.Entities, "navCamera");
            Assert.NotNull(nav);
            Assert.Equal("Scenes/CameraScene.xml", NavTarget(nav!));
        }

        [Fact]
        public void PhysicsEntityScene_Loads_AsDataDrivenScene_WithEngineBallsAndGui()
        {
            StageContentFile("Scenes/PhysicsEntityScene.xml");
            StageContentFile("Templates/BallTemplate.xml");
            StageContentFile("Config/PhysicsConfig.xml");
            StageContentFile("Sprites/ball_sprite.xml");

            var helper = new CoroutineTestHelper();
            try
            {
                AssetManager.Init(new MockContentManager());
                var scene = new DataDrivenScene(SceneParser.LoadFromAsset("Scenes/PhysicsEntityScene.xml"));

                scene.Load();
                for (int i = 0; i < 60 && !scene.IsLoaded; i++)
                    helper.Tick();

                Assert.True(scene.IsLoaded);

                // The engine was created from its config asset: gravity + named categories resolve.
                var engine = scene.GetGameSystem<PhysicsEngine>();
                Assert.NotNull(engine.Config);
                Assert.Equal(new Vector2(0, 1000), engine.Config!.Gravity);
                Assert.True(engine.Config.Resolve("Player") != 0);
                Assert.True(engine.Config.Resolve("Vip") != 0);

                // The parameterless debug renderer resolved its sibling engine and is present.
                Assert.NotNull(scene.GetGameSystem<PhysicsDebugRenderer>());

                var entitySystem = scene.GetGameSystem<EntitySystem>();

                // The ball prefab was registered before entities were instantiated.
                Assert.True(entitySystem.HasPrefab("BallPrefab"));

                // The three shells carried their behavior components.
                Assert.NotNull(entitySystem.FindById("ballSpawner")!.GetComponent<PhysicsSpawnComponent>());
                Assert.NotNull(entitySystem.FindById("saveLoadButtons")!.GetComponent<SaveLoadButtonsComponent>());
                Assert.NotNull(entitySystem.FindById("debugOverlay")!.GetComponent<PhysicsDebugOverlayComponent>());

                // The spawner actually spawned: 5 regular + 3 VIP balls, plus a world border.
                var vipBlue = entitySystem.FindById("vip_ball_blue");
                Assert.NotNull(vipBlue);
                Assert.IsType<Ball>(vipBlue);
                var totalBalls = AllEntities(entitySystem).Count(e => e is Ball);
                Assert.Equal(8, totalBalls);

                // The world border was created and configured against the engine's config.
                var border = AllEntities(entitySystem).FirstOrDefault(e => e is WorldBorder);
                Assert.NotNull(border);
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
            var filePath = Path.Combine(AppContext.BaseDirectory, "Content", fileName);
            Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
            File.WriteAllText(filePath, xml);
        }

        /// <summary>Returns every entity the system currently knows about (top-level + spawned children).</summary>
        private static System.Collections.Generic.List<Entity> AllEntities(EntitySystem system)
            => system.GetEntities().ToList();
    }
}
