using System;
using System.IO;
using Microsoft.Xna.Framework;
using Xunit;
using CoreEssentials.Assets;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Components;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Components.BuiltIn;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Serialization;
using CoreEssentials.Scenes;
using CoreEssentials.Tests.Coroutines;

namespace CoreEssentials.Tests.SceneManagement
{
    /// <summary>
    /// Tests for running a full scene from a data file: DataDrivenScene load order
    /// (systems → prefabs → entities), a complete transition through a data-driven
    /// loading screen, and the progress component mirroring TransitionProgress.
    /// </summary>
    public class DataDrivenSceneTests : IDisposable
    {
        // ──────────────────────────── Fixtures ────────────────────────────

        /// <summary>Entity fixture with a settable Entity reference for &lt;Reference&gt; tests.</summary>
        private class DDSEntity : Entity
        {
            public Entity? Other { get; set; }
            public override void Render(Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch) { }
        }

        /// <summary>Plain component with one writable string property — target for flat overrides.</summary>
        private class DDSComponent : EntityComponent
        {
            private string _base = "unset";
            public string Base { get => _base; set => _base = value; }
        }

        // ──────────────────────────── T5: load order + content ────────────────────────────

        [Fact]
        public void DataDrivenScene_Loads_SystemsThenPrefabsThenEntities()
        {
            // Arrange — one entity system, one prefab registration, two entities
            WriteContentAsset("DataDrivenLoadOrderProbe.xml", @"<Prefab Type=""DDSEntity"">
                <Components>
                    <Component Type=""DDSComponent"" />
                </Components>
            </Prefab>");

            var xml = @"<Scene>
  <GameSystems>
    <System Type=""EntitySystem"">
      <Prefabs><Prefab Name=""probe"" Asset=""DataDrivenLoadOrderProbe.xml"" /></Prefabs>
      <Entities>
        <EntityDefinition Source=""probe"" Id=""target"" Base=""hi"">
          <Position X=""10"" Y=""20"" />
          <Children>
            <EntityDefinition Type=""DDSEntity"" Id=""nested"" />
          </Children>
        </EntityDefinition>
        <EntityDefinition Type=""DDSEntity"" Id=""plain"" Base=""flat"">
          <Tags><Tag Name=""actor"" /></Tags>
          <Components><Component Type=""DDSComponent"" /></Components>
          <References><Reference Name=""Other"" TargetId=""target"" /></References>
        </EntityDefinition>
      </Entities>
    </System>
  </GameSystems>
</Scene>";

            // Helper first: its constructor stops stray coroutines, and scene.Load() starts one.
            var helper = new CoroutineTestHelper();
            try
            {
                AssetManager.Init(new MockContentManager());
                var scene = new DataDrivenScene(SceneParser.Parse(xml));

                // Act — drive the load coroutine to completion
                scene.Load();
                Assert.True(scene.IsLoading);
                for (int i = 0; i < 30 && !scene.IsLoaded; i++)
                    helper.Tick();

                // Assert — the scene fully loaded from data alone
                Assert.True(scene.IsLoaded);
                var entitySystem = scene.GetGameSystem<EntitySystem>();

                // Prefabs were registered before entities instantiated
                Assert.True(entitySystem.HasPrefab("probe"));

                // Entities loaded: prefab instance with flat override, position, and nested child
                var target = entitySystem.FindById("target");
                Assert.NotNull(target);
                Assert.Equal(new Vector2(10, 20), target.Position);
                Assert.Single(target.Children);
                Assert.Equal("nested", target.Children[0].Id);
                Assert.Equal("hi", target.GetComponent<DDSComponent>()!.Base);

                // Plain-class entity: tags, declared component with flat override, reference resolved
                var plain = entitySystem.FindById("plain");
                Assert.NotNull(plain);
                Assert.Contains("actor", plain.Tags);
                Assert.Equal("flat", plain.GetComponent<DDSComponent>()!.Base);
                Assert.Same(target, ((DDSEntity)plain).Other);
            }
            finally
            {
                helper.Cleanup();
            }
        }

        // ──────────────────────── T5: full transition through a data-driven loading screen ────────────────────────

        [Fact]
        public void Transition_ThroughDataDrivenLoadingScreen_CompletesAndSwapsScenes()
        {
            // Arrange — a data-driven loading screen (progress component) and a data-driven target scene
            WriteTransitionAssets("A");

            var helper = new CoroutineTestHelper();
            try
            {
                AssetManager.Init(new MockContentManager());
                var manager = new SceneManager();
                manager.SetLoadingScene("DataDrivenLoadingScreen_A.xml");
                manager.LoadScene("DataDrivenTargetScene_A.xml");

                Assert.True(manager.IsTransitioning);

                // Act — drive the transition to completion
                for (int i = 0; i < 40 && manager.IsTransitioning; i++)
                {
                    helper.Tick();
                    manager.Update(new GameTime(TimeSpan.FromSeconds(i * 0.016), TimeSpan.FromSeconds(0.016)));
                }

                // Assert — transition completed and the target data scene is now current
                Assert.False(manager.IsTransitioning);
                Assert.Null(manager.NextScene);
                var current = manager.CurrentScene as DataDrivenScene;
                Assert.NotNull(current);
                var entitySystem = current.GetGameSystem<EntitySystem>();
                var hero = entitySystem.FindById("hero");
                Assert.NotNull(hero);
                Assert.Equal(new Vector2(5, 6), hero.Position);

                // The loading screen's progress component observed the whole transition up to 1.0
                var loadingScreen = manager.LoadingScene as DataDrivenScene;
                Assert.NotNull(loadingScreen);
                Assert.Equal(1.0f, ProgressOf(loadingScreen!));
            }
            finally
            {
                helper.Cleanup();
            }
        }

        // ──────────────────────── T5: progress mirrors TransitionProgress ────────────────────────

        [Fact]
        public void TransitionProgressComponent_MirrorsTransitionProgress()
        {
            // Arrange — a data-driven loading screen whose entity carries the progress component
            WriteTransitionAssets("B");

            var helper = new CoroutineTestHelper();
            try
            {
                AssetManager.Init(new MockContentManager());
                var manager = new SceneManager();
                manager.SetLoadingScene("DataDrivenLoadingScreen_B.xml");
                manager.LoadScene("DataDrivenTargetScene_B.xml");

                var loadingScreen = manager.LoadingScene as DataDrivenScene;
                Assert.NotNull(loadingScreen);

                // Act — tick through the transition, sampling the component's progress each frame
                // (only once the loading screen itself has finished loading its systems)
                float last = 0f;
                bool monotonic = true;
                for (int i = 0; i < 40 && manager.IsTransitioning; i++)
                {
                    helper.Tick();
                    manager.Update(new GameTime(TimeSpan.FromSeconds(i * 0.016), TimeSpan.FromSeconds(0.016)));

                    if (!loadingScreen!.IsLoaded) continue;

                    float progress = ProgressOf(loadingScreen!);
                    if (progress < last - 0.0001f)
                        monotonic = false;
                    last = progress;
                }

                // Assert — the component tracked a monotonically rising value ending at 1.0
                Assert.True(monotonic, "Progress should only rise during a transition.");
                Assert.Equal(1.0f, last);
            }
            finally
            {
                helper.Cleanup();
            }
        }

        // ──────────────────────── Deferred-parse constructor (boot before Run()) ────────────────────────

        [Fact]
        public void DataDrivenScene_FromAssetName_DoesNotParseUntilLoad()
        {
            // Arrange — a scene requested by asset name, the way Program.cs does it right after
            // construction and BEFORE MainGame.LoadContent() has initialized the AssetManager. The
            // asset file is intentionally NOT written yet: if construction parsed eagerly it would
            // throw here (missing asset / uninitialized AssetManager). Deferral means it doesn't.
            var scene = new DataDrivenScene("DeferredBootProbe.xml");

            // Act — now that assets are available (as they are once LoadContent has run), load the scene.
            WriteContentAsset("DeferredBootProbe.xml", @"<Scene>
  <GameSystems>
    <System Type=""EntitySystem"">
      <Entities>
        <EntityDefinition Type=""DDSEntity"" Id=""booted"" />
      </Entities>
    </System>
  </GameSystems>
</Scene>");

            var helper = new CoroutineTestHelper();
            try
            {
                AssetManager.Init(new MockContentManager());

                scene.Load();
                for (int i = 0; i < 30 && !scene.IsLoaded; i++)
                    helper.Tick();

                // Assert — the deferred definition resolved during load and instantiated its entity.
                Assert.True(scene.IsLoaded);
                var entitySystem = scene.GetGameSystem<EntitySystem>();
                Assert.NotNull(entitySystem.FindById("booted"));
            }
            finally
            {
                helper.Cleanup();
            }
        }

        [Fact]
        public void DataDrivenScene_FromAssetName_NullOrEmpty_Throws()
        {
            string? nullName = null;
            Assert.Throws<ArgumentNullException>(() => new DataDrivenScene(nullName));
            Assert.Throws<ArgumentNullException>(() => new DataDrivenScene("   "));
        }

        // ──────────────────────────── Helpers ────────────────────────────

        private static float ProgressOf(DataDrivenScene scene)
        {
            var entitySystem = scene.GetGameSystem<EntitySystem>();
            var entity = entitySystem.FindById("ui");
            return entity!.GetComponent<TransitionProgressComponent>()!.Progress;
        }

        /// <summary>Writes the data-driven loading screen and target scene assets used by the
        /// transition tests. The suffix keeps each test's asset names unique.</summary>
        private static void WriteTransitionAssets(string suffix)
        {
            WriteContentAsset($"DataDrivenLoadingScreen_{suffix}.xml", @"<Scene>
  <GameSystems>
    <System Type=""EntitySystem"">
      <Entities>
        <EntityDefinition Type=""DDSEntity"" Id=""ui"">
          <Components><Component Type=""TransitionProgressComponent"" /></Components>
        </EntityDefinition>
      </Entities>
    </System>
  </GameSystems>
</Scene>");

            WriteContentAsset($"DataDrivenTargetScene_{suffix}.xml", @"<Scene>
  <GameSystems>
    <System Type=""EntitySystem"">
      <Entities>
        <EntityDefinition Type=""DDSEntity"" Id=""hero"">
          <Position X=""5"" Y=""6"" />
        </EntityDefinition>
      </Entities>
    </System>
  </GameSystems>
</Scene>");
        }

        public void Dispose() { }

        private static void WriteContentAsset(string fileName, string xml)
        {
            var contentDir = Path.Combine(AppContext.BaseDirectory, "Content");
            Directory.CreateDirectory(contentDir);
            File.WriteAllText(Path.Combine(contentDir, fileName), xml);
        }
    }
}
