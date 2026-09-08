using System;
using System.IO;
using Microsoft.Xna.Framework;
using Xunit;
using CoreEssentials.Assets;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Serialization;
using CoreEssentials.GameSystems.Physics.Engines.Aether;
using CoreEssentials.Scenes;
using CoreEssentials.Tests.Coroutines;

namespace CoreEssentials.Tests.SceneManagement
{
    /// <summary>
    /// Tests for the data-driven scene gaps that block the scene-as-data migration:
    /// nested &lt;Children&gt; carrying their own &lt;Position&gt;, and &lt;System&gt; entries
    /// that need a configuration asset (PhysicsEngine) or a parameterless constructor
    /// (PhysicsDebugRenderer).
    /// </summary>
    public class DataDrivenSceneChildPositionAndSystemConfigTests : IDisposable
    {
        // ──────────────────────────── Fixtures ────────────────────────────

        /// <summary>Plain entity fixture resolvable by the prefab loader's name-based reflection.</summary>
        private class ChildPosEntity : Entity
        {
            public override void Render(Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch) { }
        }

        // ──────────────────────── Gap 1: nested child &lt;Position&gt; ────────────────────────

        [Fact]
        public void NestedChild_WithPosition_IsPlacedAtOffsetFromParent()
        {
            var xml = @"<Scene>
  <GameSystems>
    <System Type=""EntitySystem"">
      <Entities>
        <EntityDefinition Type=""ChildPosEntity"" Id=""root"">
          <Position X=""100"" Y=""100"" />
          <Children>
            <EntityDefinition Type=""ChildPosEntity"" Id=""child"">
              <Position X=""20"" Y=""30"" />
            </EntityDefinition>
          </Children>
        </EntityDefinition>
      </Entities>
    </System>
  </GameSystems>
</Scene>";

            var helper = new CoroutineTestHelper();
            try
            {
                AssetManager.Init(new MockContentManager());
                var scene = new DataDrivenScene(SceneParser.Parse(xml));
                scene.Load();
                for (int i = 0; i < 30 && !scene.IsLoaded; i++)
                    helper.Tick();

                Assert.True(scene.IsLoaded);
                var entitySystem = scene.GetGameSystem<EntitySystem>();

                var root = entitySystem.FindById("root");
                var child = entitySystem.FindById("child");
                Assert.NotNull(root);
                Assert.NotNull(child);
                Assert.Same(root, child!.Parent);

                // The child's authored position is an offset from its parent.
                Assert.Equal(new Vector2(20, 30), child.LocalPosition);
                Assert.Equal(new Vector2(120, 130), child.Position);
            }
            finally
            {
                helper.Cleanup();
            }
        }

        [Fact]
        public void NestedChild_WithoutPosition_KeepsZeroLocalOffset()
        {
            var xml = @"<Scene>
  <GameSystems>
    <System Type=""EntitySystem"">
      <Entities>
        <EntityDefinition Type=""ChildPosEntity"" Id=""root"">
          <Position X=""10"" Y=""20"" />
          <Children>
            <EntityDefinition Type=""ChildPosEntity"" Id=""child"" />
          </Children>
        </EntityDefinition>
      </Entities>
    </System>
  </GameSystems>
</Scene>";

            var helper = new CoroutineTestHelper();
            try
            {
                AssetManager.Init(new MockContentManager());
                var scene = new DataDrivenScene(SceneParser.Parse(xml));
                scene.Load();
                for (int i = 0; i < 30 && !scene.IsLoaded; i++)
                    helper.Tick();

                Assert.True(scene.IsLoaded);
                var entitySystem = scene.GetGameSystem<EntitySystem>();
                var child = entitySystem.FindById("child");
                Assert.NotNull(child);
                Assert.Equal(Vector2.Zero, child!.LocalPosition);
                Assert.Equal(new Vector2(10, 20), child.Position);
            }
            finally
            {
                helper.Cleanup();
            }
        }

        // ──────────────── Gap 2: &lt;System Config=...&gt; attribute ────────────────

        [Fact]
        public void SceneParser_Parses_SystemConfigAttribute()
        {
            var xml = @"<Scene>
  <GameSystems>
    <System Type=""PhysicsEngine"" Config=""MyPhysics.xml"" />
  </GameSystems>
</Scene>";

            var definition = SceneParser.Parse(xml);
            Assert.Single(definition.Systems);
            Assert.Equal("MyPhysics.xml", definition.Systems[0].ConfigAsset);
        }

        [Fact]
        public void DataDrivenScene_SystemWithConfigAttribute_CreatesEngineFromConfig()
        {
            WriteContentAsset("ChildPosTestPhysicsConfig.xml", @"<PhysicsConfig>
    <Gravity X=""0"" Y=""1000"" />
    <Solver VelocityIterations=""8"" PositionIterations=""3"" />
    <Categories>
        <Category Name=""Player"" />
        <Category Name=""Vip"" />
    </Categories>
</PhysicsConfig>");

            var xml = @"<Scene>
  <GameSystems>
    <System Type=""PhysicsEngine"" Config=""ChildPosTestPhysicsConfig.xml"" />
    <System Type=""EntitySystem"">
      <Entities>
        <EntityDefinition Type=""ChildPosEntity"" Id=""probe"" />
      </Entities>
    </System>
  </GameSystems>
</Scene>";

            var helper = new CoroutineTestHelper();
            try
            {
                AssetManager.Init(new MockContentManager());
                var scene = new DataDrivenScene(SceneParser.Parse(xml));
                scene.Load();
                for (int i = 0; i < 30 && !scene.IsLoaded; i++)
                    helper.Tick();

                Assert.True(scene.IsLoaded);

                var engine = scene.GetGameSystem<PhysicsEngine>();
                Assert.NotNull(engine.Config);
                // The named categories from the config asset must be resolvable.
                Assert.Equal(new Vector2(0, 1000), engine.Config!.Gravity);
                Assert.True(engine.Config.Resolve("Player") != 0);
                Assert.True(engine.Config.Resolve("Vip") != 0);
            }
            finally
            {
                helper.Cleanup();
            }
        }

        // ──────────────── Gap 3: parameterless PhysicsDebugRenderer ────────────────

        [Fact]
        public void DataDrivenScene_ParameterlessPhysicsDebugRenderer_LoadsAlongsideEngine()
        {
            var xml = @"<Scene>
  <GameSystems>
    <System Type=""PhysicsEngine"" />
    <System Type=""PhysicsDebugRenderer"" />
    <System Type=""EntitySystem"">
      <Entities>
        <EntityDefinition Type=""ChildPosEntity"" Id=""probe"" />
      </Entities>
    </System>
  </GameSystems>
</Scene>";

            var helper = new CoroutineTestHelper();
            try
            {
                AssetManager.Init(new MockContentManager());
                var scene = new DataDrivenScene(SceneParser.Parse(xml));
                scene.Load();
                for (int i = 0; i < 30 && !scene.IsLoaded; i++)
                    helper.Tick();

                Assert.True(scene.IsLoaded);
                Assert.NotNull(scene.GetGameSystem<PhysicsDebugRenderer>());
            }
            finally
            {
                helper.Cleanup();
            }
        }

        // ──────────────────────────── Helpers ────────────────────────────

        public void Dispose() { }

        private static void WriteContentAsset(string fileName, string xml)
        {
            var contentDir = Path.Combine(AppContext.BaseDirectory, "Content");
            Directory.CreateDirectory(contentDir);
            File.WriteAllText(Path.Combine(contentDir, fileName), xml);
        }
    }
}
