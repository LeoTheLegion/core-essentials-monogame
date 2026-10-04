using System;
using System.IO;
using Microsoft.Xna.Framework;
using Xunit;
using CoreEssentials.Assets;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem;
using CoreEssentials.Scenes;
using CoreEssentials.Tests.Coroutines;

namespace CoreEssentials.Tests.SceneManagement
{
    /// <summary>
    /// Scene-persistent entities: a top-level entity tagged <c>persist</c> in the scene being left is carried
    /// across <see cref="SceneManager.LoadScene"/> and re-adopted into the incoming scene, so it survives the
    /// transition as the same live instance (state such as a looping audio stream is preserved). Entities that
    /// do not carry the tag are torn down with their scene. A persistent root travels with its whole subtree.
    /// </summary>
    public class PersistentEntitiesTests
    {
        // ──────────────────────────── Fixtures ────────────────────────────

        /// <summary>Entity that counts how many times it has been driven by its owning system.</summary>
        private class PersistEntity : Entity
        {
            public int UpdateCalls;

            public override void Update(GameTime gameTime)
            {
                base.Update(gameTime);
                UpdateCalls++;
            }
        }

        // ──────────────────────────── Happy path ────────────────────────────

        [Fact]
        public void PersistTag_EntitySurvivesLoadScene_SameInstanceKeepsTicking()
        {
            WriteScene("PersistA.xml", @"<Scene>
  <GameSystems>
    <System Type=""EntitySystem"">
      <Entities>
        <EntityDefinition Type=""PersistEntity"" Id=""music"">
          <Tags><Tag Name=""persist"" /></Tags>
        </EntityDefinition>
      </Entities>
    </System>
  </GameSystems>
</Scene>");
            // The incoming scene hosts an entity system but declares no entities of its own.
            WriteScene("PersistB.xml", @"<Scene>
  <GameSystems>
    <System Type=""EntitySystem"">
      <Entities />
    </System>
  </GameSystems>
</Scene>");

            AssetManager.Init(new MockContentManager());
            var manager = new SceneManager();
            manager.SetManifest(SceneManifestFixture.Build(
                new[] { new SceneManifestFixture.GameScene("PersistA.xml"), new SceneManifestFixture.GameScene("PersistB.xml") }));

            var helper = new CoroutineTestHelper();
            try
            {
                // Arrange — load scene A and confirm the persistent entity is alive and ticking there.
                LoadToCompletion(manager, "PersistA.xml", helper);
                var systemA = manager.CurrentScene!.GetGameSystems<EntitySystem>()[0];
                var music = (PersistEntity)systemA.FindById("music")!;
                Assert.NotNull(music);
                music.SetActive(true); // guarantee it is driven regardless of the declared active flag

                int before = music.UpdateCalls;
                for (int i = 0; i < 3; i++)
                    manager.Update(new GameTime(TimeSpan.FromSeconds(i * 0.016), TimeSpan.FromSeconds(0.016)));
                Assert.True(music.UpdateCalls > before, "persistent entity should tick in its originating scene");

                // Act — transition to scene B.
                manager.LoadScene("PersistB.xml");
                TickUntilSettled(manager, helper);
                Assert.False(manager.IsTransitioning);

                // Assert — the same instance now lives in scene B's system and keeps ticking from there.
                var systemB = manager.CurrentScene!.GetGameSystems<EntitySystem>()[0];
                Assert.Same(music, systemB.FindById("music"));
                Assert.True(music.GetActive());

                int afterArrival = music.UpdateCalls;
                for (int i = 0; i < 3; i++)
                    manager.Update(new GameTime(TimeSpan.FromSeconds(i * 0.016), TimeSpan.FromSeconds(0.016)));
                Assert.True(music.UpdateCalls > afterArrival, "persistent entity should keep ticking in the new scene");
            }
            finally
            {
                helper.Cleanup();
            }
        }

        [Fact]
        public void PersistTag_EntityIsNotDoubleStartedOnAdoption()
        {
            WriteScene("PersistS1.xml", @"<Scene>
  <GameSystems>
    <System Type=""EntitySystem"">
      <Entities>
        <EntityDefinition Type=""PersistEntity"" Id=""music"">
          <Tags><Tag Name=""persist"" /></Tags>
        </EntityDefinition>
      </Entities>
    </System>
  </GameSystems>
</Scene>");
            WriteScene("PersistS2.xml", @"<Scene>
  <GameSystems>
    <System Type=""EntitySystem"">
      <Entities />
    </System>
  </GameSystems>
</Scene>");

            AssetManager.Init(new MockContentManager());
            var manager = new SceneManager();
            manager.SetManifest(SceneManifestFixture.Build(
                new[] { new SceneManifestFixture.GameScene("PersistS1.xml"), new SceneManifestFixture.GameScene("PersistS2.xml") }));

            var helper = new CoroutineTestHelper();
            try
            {
                LoadToCompletion(manager, "PersistS1.xml", helper);
                var music = (PersistEntity)manager.CurrentScene!.GetGameSystems<EntitySystem>()[0].FindById("music")!;
                Assert.True(music.HasStarted);

                // Act — carrying/adopting must not re-run the one-time lifecycle hooks.
                manager.LoadScene("PersistS2.xml");
                TickUntilSettled(manager, helper);

                // The instance is unchanged and still exactly-once started (no second OnStart fired).
                Assert.True(music.HasStarted);
                Assert.Same(music, manager.CurrentScene!.GetGameSystems<EntitySystem>()[0].FindById("music"));
            }
            finally
            {
                helper.Cleanup();
            }
        }

        // ──────────────────────────── Non-persist teardown ────────────────────────────

        [Fact]
        public void Untagged_EntityIsDestroyedWhenSceneLeaves()
        {
            WriteScene("PersistT1.xml", @"<Scene>
  <GameSystems>
    <System Type=""EntitySystem"">
      <Entities>
        <EntityDefinition Type=""PersistEntity"" Id=""temp"" />
        <EntityDefinition Type=""PersistEntity"" Id=""music"">
          <Tags><Tag Name=""persist"" /></Tags>
        </EntityDefinition>
      </Entities>
    </System>
  </GameSystems>
</Scene>");
            WriteScene("PersistT2.xml", @"<Scene>
  <GameSystems>
    <System Type=""EntitySystem"">
      <Entities />
    </System>
  </GameSystems>
</Scene>");

            AssetManager.Init(new MockContentManager());
            var manager = new SceneManager();
            manager.SetManifest(SceneManifestFixture.Build(
                new[] { new SceneManifestFixture.GameScene("PersistT1.xml"), new SceneManifestFixture.GameScene("PersistT2.xml") }));

            var helper = new CoroutineTestHelper();
            try
            {
                LoadToCompletion(manager, "PersistT1.xml", helper);
                var systemA = manager.CurrentScene!.GetGameSystems<EntitySystem>()[0];
                var temp = (PersistEntity)systemA.FindById("temp")!;
                var music = (PersistEntity)systemA.FindById("music")!;
                Assert.NotNull(temp);
                Assert.NotNull(music);

                // Act
                manager.LoadScene("PersistT2.xml");
                TickUntilSettled(manager, helper);

                // Assert — the untagged entity is gone from the incoming scene; the tagged one remains.
                var systemB = manager.CurrentScene!.GetGameSystems<EntitySystem>()[0];
                Assert.Null(systemB.FindById("temp"));
                Assert.Same(music, systemB.FindById("music"));
            }
            finally
            {
                helper.Cleanup();
            }
        }

        // ──────────────────────────── Subtree carry ────────────────────────────

        [Fact]
        public void PersistTag_RootCarriesWholeSubtree()
        {
            WriteScene("PersistC1.xml", @"<Scene>
  <GameSystems>
    <System Type=""EntitySystem"">
      <Entities>
        <EntityDefinition Type=""PersistEntity"" Id=""root"">
          <Tags><Tag Name=""persist"" /></Tags>
          <Children>
            <EntityDefinition Type=""PersistEntity"" Id=""child"" />
          </Children>
        </EntityDefinition>
      </Entities>
    </System>
  </GameSystems>
</Scene>");
            WriteScene("PersistC2.xml", @"<Scene>
  <GameSystems>
    <System Type=""EntitySystem"">
      <Entities />
    </System>
  </GameSystems>
</Scene>");

            AssetManager.Init(new MockContentManager());
            var manager = new SceneManager();
            manager.SetManifest(SceneManifestFixture.Build(
                new[] { new SceneManifestFixture.GameScene("PersistC1.xml"), new SceneManifestFixture.GameScene("PersistC2.xml") }));

            var helper = new CoroutineTestHelper();
            try
            {
                LoadToCompletion(manager, "PersistC1.xml", helper);
                var systemA = manager.CurrentScene!.GetGameSystems<EntitySystem>()[0];
                var root = (PersistEntity)systemA.FindById("root")!;
                var child = (PersistEntity)systemA.FindById("child")!;
                Assert.NotNull(root);
                Assert.NotNull(child);

                // Act
                manager.LoadScene("PersistC2.xml");
                TickUntilSettled(manager, helper);

                // Assert — both the persistent root and its untagged child survive as the same instances.
                var systemB = manager.CurrentScene!.GetGameSystems<EntitySystem>()[0];
                Assert.Same(root, systemB.FindById("root"));
                Assert.Same(child, systemB.FindById("child"));
            }
            finally
            {
                helper.Cleanup();
            }
        }

        // ──────────────────────────── No host to adopt into ────────────────────────────

        [Fact]
        public void PersistTag_IncomingSceneWithoutEntitySystem_ReleasesCarriedEntities()
        {
            WriteScene("PersistN1.xml", @"<Scene>
  <GameSystems>
    <System Type=""EntitySystem"">
      <Entities>
        <EntityDefinition Type=""PersistEntity"" Id=""music"">
          <Tags><Tag Name=""persist"" /></Tags>
        </EntityDefinition>
      </Entities>
    </System>
  </GameSystems>
</Scene>");
            // The incoming scene declares no game systems at all.
            WriteScene("PersistN2.xml", "<Scene><GameSystems /></Scene>");

            AssetManager.Init(new MockContentManager());
            var manager = new SceneManager();
            manager.SetManifest(SceneManifestFixture.Build(
                new[] { new SceneManifestFixture.GameScene("PersistN1.xml"), new SceneManifestFixture.GameScene("PersistN2.xml") }));

            var helper = new CoroutineTestHelper();
            try
            {
                LoadToCompletion(manager, "PersistN1.xml", helper);
                var music = (PersistEntity)manager.CurrentScene!.GetGameSystems<EntitySystem>()[0].FindById("music")!;
                Assert.NotNull(music);

                // Act — the incoming scene has no entity system to host the carried entity.
                manager.LoadScene("PersistN2.xml");
                TickUntilSettled(manager, helper);
                Assert.False(manager.IsTransitioning);

                // Assert — nothing is leaked or left driving; the transition simply completed cleanly.
                Assert.Equal("PersistN2.xml", CurrentAssetName(manager));
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

        /// <summary>Writes a scene XML content asset under the test Content directory.</summary>
        private static void WriteScene(string fileName, string xml)
        {
            var contentDir = Path.Combine(AppContext.BaseDirectory, "Content");
            Directory.CreateDirectory(contentDir);
            File.WriteAllText(Path.Combine(contentDir, fileName), xml);
        }
    }
}
