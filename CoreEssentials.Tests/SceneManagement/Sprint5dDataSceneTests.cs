using System;
using System.IO;
using System.Linq;
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
    /// Sprint 5d — proves the three "hard" demo scenes run entirely from data. Each real shipping XML
    /// parses in the strict format (systems, prefabs, entity knobs, references and navigation targets),
    /// and loads as a DataDrivenScene with no C# scene subclass:
    ///   • CharacterScene — characters + templated buttons, key-driven audio/volume/debug/navigation.
    ///   • CameraScene — camera + player + follow toggle (declarative &lt;Reference&gt; links).
    ///   • LabelAlignmentDemoScene — screen-space HUD labels + an orbiting world-space panel + overlay.
    ///
    /// The CharacterScene load test uses a music-stripped variant of the file: MusicComponent plays on
    /// attach, and audio playback throws headlessly (the mock content returns a null SoundEffect). The
    /// parse test still asserts the real file carries the music shell.
    /// </summary>
    public class Sprint5dDataSceneTests : IDisposable
    {
        private readonly Game _mockGame;

        public Sprint5dDataSceneTests()
        {
            // Buttons/labels/canvases create GUI widgets on attach — the GUI engine must be up.
            _mockGame = new Game1();
            GUIManager.Init(_mockGame, 1280, 720);
        }

        // ═══════════════════════ CharacterScene ═══════════════════════

        [Fact]
        public void CharacterScene_Parses_AsStrictScene_WithPrefabsAndKnobs()
        {
            // Parsing a <System> with <Prefab Asset=.../> loads each template through AssetManager,
            // so the content manager must be up and the templates staged.
            StageContentFile("TextTemplate.xml");
            StageContentFile("SoundButtonTemplate.xml");
            StageContentFile("VolumeButtonTemplate.xml");
            AssetManager.Init(new MockContentManager());

            var scene = SceneParser.Parse(ReadSourceContentFile("Scenes/CharacterScene.xml"));

            Assert.Single(scene.Systems);
            Assert.Equal(typeof(EntitySystem), scene.Systems[0].SystemType);
            var sys = scene.Systems[0];

            // Three templates are registered as prefabs.
            Assert.Equal(3, sys.Prefabs.Count);
            Assert.Contains(sys.Prefabs, p => p.Name == "TextPrefab" && p.Asset == "TextTemplate.xml");
            Assert.Contains(sys.Prefabs, p => p.Name == "SoundButtonPrefab" && p.Asset == "SoundButtonTemplate.xml");
            Assert.Contains(sys.Prefabs, p => p.Name == "VolumeButtonPrefab" && p.Asset == "VolumeButtonTemplate.xml");

            // Characters are typed entities with their tags.
            var staticChar = FindById(sys.Entities, "staticCharacter");
            Assert.NotNull(staticChar);
            Assert.Equal("CoreEssentials.Playground.Entities.CharacterEntity", staticChar!.Type);
            Assert.Contains("Static", staticChar.Tags);
            var animated = FindById(sys.Entities, "animatedCharacter");
            Assert.NotNull(animated);
            Assert.Contains("Animated", animated!.Tags);

            // Text instances are prefab-based and configured via EntityOverrides.
            var info = FindById(sys.Entities, "infoText");
            Assert.Equal("TextPrefab", info!.Source);
            Assert.Equal("Center", info.EntityOverrides["Alignment"]);
            Assert.Contains("Press Q, W, E", info.EntityOverrides["Text"]);

            // Sound buttons are prefab-based with sound + text overrides.
            var fs1 = FindById(sys.Entities, "footstep1Button");
            Assert.Equal("SoundButtonPrefab", fs1!.Source);
            Assert.Equal("footstep1_sound.xml", fs1.EntityOverrides["SoundAsset"]);
            Assert.Equal("Footstep 1", fs1.EntityOverrides["ButtonText"]);

            // Volume buttons carry a level + text.
            var volLow = FindById(sys.Entities, "volumeLowButton");
            Assert.Equal("VolumeButtonPrefab", volLow!.Source);
            Assert.Equal("0.1", volLow.EntityOverrides["VolumeLevel"]);

            // Music shell (present in the real file; stripped from the load variant below).
            var music = FindById(sys.Entities, "music");
            Assert.NotNull(music);
            Assert.Equal("song1_sound.xml", music!.DeclaredComponents.First(c => c.Type.Contains("MusicComponent")).Properties["MusicAsset"]);

            // Debug toggle starts enabled with its font.
            var debug = FindById(sys.Entities, "debugToggle");
            var debugComp = debug!.DeclaredComponents.First(c => c.Type.Contains("DebugToggleComponent"));
            Assert.Equal("true", debugComp.Properties["StartEnabled"]);
            Assert.Equal("base", debugComp.Properties["DebugFontAsset"]);

            // Key-driven audio: three sound keys + two volume keys.
            var q = FindById(sys.Entities, "soundKeyQ")!;
            Assert.Equal("footstep1_sound.xml", q.DeclaredComponents.First(c => c.Type.Contains("SoundKeyComponent")).Properties["SoundAsset"]);
            var z = FindById(sys.Entities, "volumeKeyZ")!;
            Assert.Equal("0.1", z.DeclaredComponents.First(c => c.Type.Contains("VolumeKeyComponent")).Properties["Volume"]);

            // Navigation targets are scene asset-name strings.
            Assert.Equal("Scenes/PhysicsEntityScene.xml", NavTarget(FindById(sys.Entities, "navPhysics")!));
            Assert.Equal("Scenes/SendMessageDemoScene.xml", NavTarget(FindById(sys.Entities, "navSendMessage")!));
        }

        [Fact]
        public void CharacterScene_Loads_AsDataDrivenScene_WithCharactersAndButtons()
        {
            StageContentFile("Scenes/CharacterScene.xml");
            StageContentFile("TextTemplate.xml");
            StageContentFile("SoundButtonTemplate.xml");
            StageContentFile("VolumeButtonTemplate.xml");
            // Character / player sprites load headlessly (0×0 frames) via this chain.
            StageContentFile("character_sprite.xml");
            StageContentFile("character_anim_walk.xml");
            StageContentFile("character_sheet.xml");

            var helper = new CoroutineTestHelper();
            try
            {
                // Music playback throws headlessly (null SoundEffect), so load a music-stripped copy.
                var stripped = StripEntity(ReadSourceContentFile("Scenes/CharacterScene.xml"), "music");
                WriteContentAsset("CharacterScene_LoadVariant.xml", stripped);

                var content = new MockContentManager();
                content.AddAsset<SpriteFont>("base", CoreEssentials.Tests.MockSpriteFont.Instance);
                AssetManager.Init(content);
                var scene = new DataDrivenScene(SceneParser.LoadFromAsset("CharacterScene_LoadVariant.xml"));

                scene.Load();
                for (int i = 0; i < 60 && !scene.IsLoaded; i++)
                    helper.Tick();

                Assert.True(scene.IsLoaded);
                var entitySystem = scene.GetGameSystem<EntitySystem>();

                // Characters instantiated with their tags.
                var staticChar = entitySystem.FindById("staticCharacter");
                Assert.NotNull(staticChar);
                Assert.IsType<CharacterEntity>(staticChar);
                Assert.True(staticChar!.HasTag("Static"));
                var animated = entitySystem.FindById("animatedCharacter");
                Assert.IsType<AnimatedCharacterEntity>(animated);

                // Prefab-based buttons resolved to their concrete types.
                Assert.IsType<TextEntity>(entitySystem.FindById("infoText"));
                Assert.IsType<SoundButtonEntity>(entitySystem.FindById("footstep1Button"));
                Assert.IsType<VolumeButtonEntity>(entitySystem.FindById("volumeLowButton"));

                // The debug toggle attached to its shell.
                Assert.NotNull(entitySystem.FindById("debugToggle")!.GetComponent<DebugToggleComponent>());
            }
            finally
            {
                helper.Cleanup();
            }
        }

        // ═══════════════════════ CameraScene ═══════════════════════

        [Fact]
        public void CameraScene_Parses_AsStrictScene_WithCameraPlayerAndFollowToggle()
        {
            var scene = SceneParser.Parse(ReadSourceContentFile("Scenes/CameraScene.xml"));

            Assert.Single(scene.Systems);
            Assert.Equal(typeof(EntitySystem), scene.Systems[0].SystemType);
            var sys = scene.Systems[0];

            // Camera + player are typed entities.
            var camera = FindById(sys.Entities, "camera");
            Assert.Equal("CoreEssentials.Playground.Entities.CameraEntity", camera!.Type);
            var player = FindById(sys.Entities, "player");
            Assert.Equal("CoreEssentials.Playground.Entities.PlayerEntity", player!.Type);

            // The info text is a typed TextEntity with multi-line text (newline preserved via &#10;).
            var info = FindById(sys.Entities, "cameraInfoText");
            Assert.Equal("CoreEssentials.Playground.Entities.TextEntity", info!.Type);
            Assert.Contains("\n", info.EntityOverrides["Text"]);

            // The follow toggle declares its three references.
            var follow = FindById(sys.Entities, "followToggle");
            Assert.NotNull(follow);
            Assert.Equal("camera", RefTarget(follow!, "Camera"));
            Assert.Equal("player", RefTarget(follow, "FollowTarget"));
            Assert.Equal("cameraInfoText", RefTarget(follow, "InfoLabel"));

            // Navigation target.
            Assert.Equal("Scenes/CharacterScene.xml", NavTarget(FindById(sys.Entities, "navCharacter")!));
        }

        [Fact]
        public void CameraScene_Loads_AsDataDrivenScene_WithReferencesResolved()
        {
            StageContentFile("Scenes/CameraScene.xml");
            StageContentFile("character_sprite.xml");
            StageContentFile("character_anim_walk.xml");
            StageContentFile("character_sheet.xml");

            var helper = new CoroutineTestHelper();
            try
            {
                var content = new MockContentManager();
                content.AddAsset<SpriteFont>("base", CoreEssentials.Tests.MockSpriteFont.Instance);
                AssetManager.Init(content);
                var scene = new DataDrivenScene(SceneParser.LoadFromAsset("Scenes/CameraScene.xml"));

                scene.Load();
                for (int i = 0; i < 60 && !scene.IsLoaded; i++)
                    helper.Tick();

                Assert.True(scene.IsLoaded);
                var entitySystem = scene.GetGameSystem<EntitySystem>();

                // The camera registered its inner Camera instance as the main camera on attach.
                var camera = entitySystem.FindById("camera") as CameraEntity;
                Assert.NotNull(camera);
                Assert.Same(camera!.Camera, CoreEssentials.Camera.Camera.MainCamera);

                // The player instantiated at its authored position.
                var player = entitySystem.FindById("player");
                Assert.NotNull(player);
                Assert.Equal(new Vector2(400, 300), player!.Position);

                // The follow toggle's <Reference> links resolved to the live entities.
                var follow = entitySystem.FindById("followToggle")!.GetComponent<CameraFollowToggleComponent>();
                Assert.NotNull(follow);
                Assert.Same(camera, follow!.Camera);
                Assert.Same(player, follow.FollowTarget);
                Assert.Same(entitySystem.FindById("cameraInfoText"), follow.InfoLabel);
            }
            finally
            {
                helper.Cleanup();
            }
        }

        // ═══════════════════════ LabelAlignmentDemoScene ═══════════════════════

        [Fact]
        public void LabelAlignmentDemo_Parses_AsStrictScene_WithHudPanelAndOverlay()
        {
            var scene = SceneParser.Parse(ReadSourceContentFile("Scenes/LabelAlignmentDemoScene.xml"));

            Assert.Single(scene.Systems);
            Assert.Equal(typeof(EntitySystem), scene.Systems[0].SystemType);
            var sys = scene.Systems[0];

            // Camera speed is overridden (the default 1 unit/s is imperceptible).
            var camera = FindById(sys.Entities, "camera");
            Assert.Equal("300", camera!.EntityOverrides["CameraSpeed"]);

            // The screen-space HUD root has four children (three labels + info).
            var hud = FindById(sys.Entities, "hudRoot");
            Assert.NotNull(hud);
            var hudCanvas = hud!.DeclaredComponents.First(c => c.Type.Contains("CanvasComponent"));
            Assert.Equal("true", hudCanvas.Properties["IsScreenSpace"]);
            Assert.Equal(4, hud.Children.Count);

            // Each label host carries a LabelComponent + a HudLabelRefreshComponent.
            var leftHost = hud.Children[0];
            Assert.Contains(leftHost.DeclaredComponents, c => c.Type.Contains("LabelComponent"));
            Assert.Contains(leftHost.DeclaredComponents, c => c.Type.Contains("HudLabelRefreshComponent"));

            // The world-space panel has a pinned-size canvas + an orbit component + two label children.
            var panel = FindById(sys.Entities, "panel");
            Assert.NotNull(panel);
            var panelCanvas = panel!.DeclaredComponents.First(c => c.Type.Contains("CanvasComponent"));
            Assert.Equal("false", panelCanvas.Properties["IsScreenSpace"]);
            Assert.Equal("280", panelCanvas.Properties["Width"]);
            var orbit = panel.DeclaredComponents.First(c => c.Type.Contains("OrbitPanelComponent"));
            Assert.Equal("640", orbit.Properties["CenterX"]);
            Assert.Equal(2, panel.Children.Count);

            // The overlay is a plain component on its shell.
            var overlay = FindById(sys.Entities, "debugOverlay");
            Assert.NotNull(overlay);
            Assert.Contains(overlay!.DeclaredComponents, c => c.Type.Contains("LabelAlignmentDebugOverlayComponent"));

            // Navigation target.
            Assert.Equal("Scenes/SendMessageDemoScene.xml", NavTarget(FindById(sys.Entities, "navSendMessage")!));
        }

        [Fact]
        public void LabelAlignmentDemo_Loads_AsDataDrivenScene_WithHudPanelAndOrbit()
        {
            StageContentFile("Scenes/LabelAlignmentDemoScene.xml");

            var helper = new CoroutineTestHelper();
            try
            {
                var content = new MockContentManager();
                content.AddAsset<SpriteFont>("base", CoreEssentials.Tests.MockSpriteFont.Instance);
                AssetManager.Init(content);
                var scene = new DataDrivenScene(SceneParser.LoadFromAsset("Scenes/LabelAlignmentDemoScene.xml"));

                scene.Load();
                for (int i = 0; i < 60 && !scene.IsLoaded; i++)
                    helper.Tick();

                Assert.True(scene.IsLoaded);
                var entitySystem = scene.GetGameSystem<EntitySystem>();

                // Camera speed override applied.
                var camera = entitySystem.FindById("camera") as CameraEntity;
                Assert.Equal(300f, camera!.CameraSpeed);

                // HUD root: screen-space canvas with four child hosts.
                var hud = entitySystem.FindById("hudRoot");
                var hudCanvas = hud!.GetComponent<CanvasComponent>();
                Assert.NotNull(hudCanvas);
                Assert.True(hudCanvas.IsScreenSpace);
                Assert.Equal(4, hud.Children.Count);

                // Panel: world-space canvas at its authored position with a pinned size + orbit.
                var panel = entitySystem.FindById("panel");
                Assert.NotNull(panel);
                var panelCanvas = panel!.GetComponent<CanvasComponent>();
                Assert.False(panelCanvas.IsScreenSpace);
                Assert.Equal(280f, panelCanvas.Width);
                Assert.Equal(new Vector2(640, 360), panel.Position);
                var orbit = panel.GetComponent<OrbitPanelComponent>();
                Assert.NotNull(orbit);
                Assert.Equal(0.6f, orbit!.Speed);

                // All six labels attached (three HUD + info + two on the panel) across two canvases.
                var overlay = entitySystem.FindById("debugOverlay")!.GetComponent<LabelAlignmentDebugOverlayComponent>();
                Assert.Equal(6, overlay!.DiscoverLabels(entitySystem).Count);
                Assert.Equal(2, overlay.DiscoverCanvases(entitySystem).Count);

                // The orbit actually moves the panel on update.
                var before = panel.Position;
                var frame = new TimeSpan(0, 0, 0, 0, 16); // 16 ms
                for (int i = 0; i < 30; i++)
                    entitySystem.Update(new GameTime(frame, frame));
                Assert.NotEqual(before, panel.Position);
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
            var comp = def.DeclaredComponents.First(c => c.Type.Contains("NavigateOnKeyComponent"));
            return comp.Properties["TargetSceneAsset"];
        }

        /// <summary>Reads the TargetId of a &lt;Reference Name=.../&gt; on a definition.</summary>
        private static string RefTarget(EntityDefinition def, string name)
        {
            var reference = def.References.First(r => r.Attribute("Name")?.Value == name);
            return reference.Attribute("TargetId")!.Value;
        }

        /// <summary>Removes the &lt;EntityDefinition Id="id"&gt; block (and its comment line) from scene XML.</summary>
        private static string StripEntity(string xml, string id)
        {
            var marker = $"Id=\"{id}\"";
            var idx = xml.IndexOf(marker, StringComparison.Ordinal);
            if (idx < 0) return xml;

            // Walk back to the start of the enclosing element line (skipping a preceding comment line).
            int start = xml.LastIndexOf("<EntityDefinition", idx);
            var prevNewline = xml.LastIndexOf('\n', start - 1);
            if (prevNewline >= 0 && xml.Substring(prevNewline + 1, start - prevNewline - 1).TrimStart().StartsWith("<!--"))
            {
                var commentLineStart = xml.LastIndexOf('\n', prevNewline - 1);
                start = commentLineStart < 0 ? 0 : commentLineStart + 1;
            }

            // Walk forward past the closing </EntityDefinition> and its trailing newline.
            int end = xml.IndexOf("</EntityDefinition>", idx, StringComparison.Ordinal) + "</EntityDefinition>".Length;
            if (end < xml.Length && xml[end] == '\n') end++;

            return xml.Substring(0, start) + xml.Substring(end);
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
    }
}
