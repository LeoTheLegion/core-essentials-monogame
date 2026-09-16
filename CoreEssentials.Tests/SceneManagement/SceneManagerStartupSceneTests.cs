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
    /// The library owns the "no scene supplied → boot the first &lt;GameScenes&gt; entry" rule. A null or
    /// empty name passed to LoadScene(string?) resolves, inside the transition coroutine, to the manifest's
    /// startup scene (its first entry). Callers never hardcode a default scene — the core decides what launches.
    /// </summary>
    public class SceneManagerStartupSceneTests
    {
        // A plain entity so the target scene needs no GUI engine and can transition directly (no loading screen).
        private class StartupEntity : Entity
        {
            public override void Render(Microsoft.Xna.Framework.Graphics.SpriteBatch _spriteBatch) { }
        }

        [Fact]
        public void LoadScene_NullName_BootsFirstManifestEntry()
        {
            // Arrange — two game scenes; the FIRST is the one a null name must resolve to.
            WriteContentAsset("StartupFirst.xml", SceneXml());
            WriteContentAsset("StartupSecond.xml", SceneXml());

            var helper = new CoroutineTestHelper();
            try
            {
                AssetManager.Init(new MockContentManager());
                var manager = new SceneManager();
                manager.SetManifest(SceneManifestFixture.Build(
                    new[]
                    {
                        new SceneManifestFixture.GameScene("StartupFirst.xml"),
                        new SceneManifestFixture.GameScene("StartupSecond.xml")
                    }));

                // Act — no scene name supplied; the core must pick the first entry. The cast disambiguates
                // the string? overload from LoadScene(Scene) for a literal null.
                manager.LoadScene((string?)null);
                Assert.True(manager.IsTransitioning);
                DriveToCompletion(manager, helper);

                // Assert — the current scene is the FIRST manifest entry, not the second.
                var current = manager.CurrentScene as DataDrivenScene;
                Assert.NotNull(current);
                Assert.Equal("StartupFirst.xml", current.AssetName);
            }
            finally { helper.Cleanup(); }
        }

        [Fact]
        public void LoadScene_EmptyName_BootsFirstManifestEntry()
        {
            // Arrange — same two-scene manifest; whitespace must be treated as "no scene".
            WriteContentAsset("StartupFirst.xml", SceneXml());
            WriteContentAsset("StartupSecond.xml", SceneXml());

            var helper = new CoroutineTestHelper();
            try
            {
                AssetManager.Init(new MockContentManager());
                var manager = new SceneManager();
                manager.SetManifest(SceneManifestFixture.Build(
                    new[]
                    {
                        new SceneManifestFixture.GameScene("StartupFirst.xml"),
                        new SceneManifestFixture.GameScene("StartupSecond.xml")
                    }));

                // Act
                manager.LoadScene("   ");
                Assert.True(manager.IsTransitioning);
                DriveToCompletion(manager, helper);

                // Assert
                var current = manager.CurrentScene as DataDrivenScene;
                Assert.NotNull(current);
                Assert.Equal("StartupFirst.xml", current.AssetName);
            }
            finally { helper.Cleanup(); }
        }

        [Fact]
        public void LoadScene_ExplicitName_StillLoadsThatScene()
        {
            // Arrange — the explicit path is unchanged: naming a scene loads exactly that one.
            WriteContentAsset("StartupFirst.xml", SceneXml());
            WriteContentAsset("StartupSecond.xml", SceneXml());

            var helper = new CoroutineTestHelper();
            try
            {
                AssetManager.Init(new MockContentManager());
                var manager = new SceneManager();
                manager.SetManifest(SceneManifestFixture.Build(
                    new[]
                    {
                        new SceneManifestFixture.GameScene("StartupFirst.xml"),
                        new SceneManifestFixture.GameScene("StartupSecond.xml")
                    }));

                // Act — explicitly request the SECOND scene.
                manager.LoadScene("StartupSecond.xml");
                Assert.True(manager.IsTransitioning);
                DriveToCompletion(manager, helper);

                // Assert — the explicit name wins over the first-entry default.
                var current = manager.CurrentScene as DataDrivenScene;
                Assert.NotNull(current);
                Assert.Equal("StartupSecond.xml", current.AssetName);
            }
            finally { helper.Cleanup(); }
        }

        [Fact]
        public void LoadScene_NullName_NoManifest_ThrowsSynchronously()
        {
            // A null name still requires a configured manifest before it can resolve the first entry.
            var manager = new SceneManager();
            Assert.Throws<InvalidOperationException>(() => manager.LoadScene((string?)null));
        }

        // ──────────────────────────── Helpers ────────────────────────────

        private static string SceneXml() => $@"<Scene>
  <GameSystems>
    <System Type=""EntitySystem"">
      <Entities>
        <EntityDefinition Type=""{nameof(StartupEntity)}"" Id=""root"">
          <Position X=""1"" Y=""2"" />
        </EntityDefinition>
      </Entities>
    </System>
  </GameSystems>
</Scene>";

        private static void DriveToCompletion(SceneManager manager, CoroutineTestHelper helper)
        {
            for (var i = 0; i < 40 && manager.IsTransitioning; i++)
            {
                helper.Tick();
                manager.Update(new GameTime(TimeSpan.FromSeconds(i * 0.016), TimeSpan.FromSeconds(0.016)));
            }

            Assert.False(manager.IsTransitioning);
            Assert.Null(manager.PendingScene);
        }

        private static void WriteContentAsset(string fileName, string xml)
        {
            var contentDir = Path.Combine(AppContext.BaseDirectory, "Content");
            Directory.CreateDirectory(contentDir);
            File.WriteAllText(Path.Combine(contentDir, fileName), xml);
        }
    }
}
