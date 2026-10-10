#nullable enable
using System;
using System.Collections;
using System.Reflection;
using CoreEssentials.GameSystems;
using CoreEssentials.GameSystems.Physics.Engines.Aether;
using CoreEssentials.Scenes;
using Microsoft.Xna.Framework.Graphics;
using nkast.Aether.Physics2D.Diagnostics;
using Xunit;

namespace CoreEssentials.Tests.GameSystems.Physics
{
    /// <summary>
    /// Device-free tests for <see cref="PhysicsDebugRenderer"/>: constructor wiring, flag defaults, the
    /// dispose lifecycle, the guard rails (no-engine / no-game throws), and — via an injected recording
    /// fake of the device-reach seam (<c>IPhysicsDebugRenderTarget</c>) — the lazy engine resolution,
    /// font-load dispatch, and the enabled draw path that computes the screen-pixel projection.
    /// </summary>
    public class PhysicsDebugRendererTests
    {
        [Fact]
        public void ParameterizedConstructor_NullEngine_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new PhysicsDebugRenderer(null!));
        }

        [Fact]
        public void ParameterizedConstructor_AddsContactPointsFlagAndExposesView()
        {
            using var engine = new PhysicsEngine();

            using var renderer = new PhysicsDebugRenderer(engine);

            Assert.True(renderer.DebugView.Flags.HasFlag(DebugViewFlags.ContactPoints));
            Assert.NotNull(renderer.DebugView);
        }

        [Fact]
        public void IsEnabled_DefaultsToFalse_AndIsSettable()
        {
            using var renderer = new PhysicsDebugRenderer();

            Assert.False(renderer.IsEnabled);
            renderer.IsEnabled = true;
            Assert.True(renderer.IsEnabled);
        }

        [Fact]
        public void DebugView_AccessWithoutEngineOrScene_Throws()
        {
            // No engine passed and no scene attached → the lazy resolve must fail loudly.
            using var renderer = new PhysicsDebugRenderer();

            Assert.Throws<InvalidOperationException>(() => _ = renderer.DebugView);
        }

        [Fact]
        public void Flags_GetsAndSetsThroughView()
        {
            using var engine = new PhysicsEngine();
            using var renderer = new PhysicsDebugRenderer(engine);

            var current = renderer.Flags;
            Assert.True(current.HasFlag(DebugViewFlags.ContactPoints));

            // Round-trip: clearing the flag is observable through the getter.
            renderer.Flags = (DebugViewFlags)0;
            Assert.Equal((DebugViewFlags)0, renderer.Flags);
        }

        [Fact]
        public void Draw_WhenDisabled_IsANoOp()
        {
            // IsEnabled defaults to false → Draw must return before touching the (absent) scene/game.
            using var renderer = new PhysicsDebugRenderer();

            var ex = Record.Exception(() => renderer.Draw(null!));
            Assert.Null(ex);
        }

        [Fact]
        public void LoadContent_BeforeSceneAttached_Throws()
        {
            using var renderer = new PhysicsDebugRenderer();

            Assert.Throws<InvalidOperationException>(() => renderer.LoadContent());
        }

        [Fact]
        public void Dispose_IsIdempotentAndDoesNotThrow()
        {
            using var engine = new PhysicsEngine();
            var renderer = new PhysicsDebugRenderer(engine);

            var ex = Record.Exception(() =>
            {
                renderer.Dispose();
                renderer.Dispose(); // second call must be a no-op
            });
            Assert.Null(ex);
        }

        // ───────────── Device-reach seam (IPhysicsDebugRenderTarget) ─────────────

        private sealed class RecordingTarget : IPhysicsDebugRenderTarget
        {
            public int LoadFontCalls;
            public int RenderCalls;
            public DebugView? LastRendered;

            public void LoadFont(DebugView view) => LoadFontCalls++;
            public void Render(DebugView view) { RenderCalls++; LastRendered = view; }
        }

        // ───────────── Lazy engine resolution (no ctor-injected engine) ─────────────

        [Fact]
        public void EnsureDebugView_NoEngineInCtor_ResolvesSiblingFromScene()
        {
            using var engine = new PhysicsEngine();
            var scene = NewSceneWith(engine);
            using var renderer = new PhysicsDebugRenderer();   // no engine passed
            renderer.SetScene(scene);

            // First access triggers the lazy resolve + DebugView creation.
            var view = renderer.DebugView;

            Assert.NotNull(view);
            Assert.True(view.Flags.HasFlag(DebugViewFlags.ContactPoints));
        }

        private static Scene NewSceneWith(PhysicsEngine engine)
        {
            var scene = new TestScene();
            RegisterSystem(scene, engine);
            return scene;
        }

        // ───────────── LoadContent dispatch through the seam ─────────────

        [Fact]
        public void LoadContent_InjectedTarget_ForwardsFontLoad_OnceOnly()
        {
            using var engine = new PhysicsEngine();
            var target = new RecordingTarget();
            using var renderer = new PhysicsDebugRenderer(engine, target);

            renderer.LoadContent();
            renderer.LoadContent(); // idempotent: _contentLoaded guard blocks a second load

            Assert.Equal(1, target.LoadFontCalls);
        }

        // ───────────── Draw enabled path (cold vs warm content) ─────────────

        [Fact]
        public void Draw_Enabled_ColdContent_LoadsFontThenRenders()
        {
            using var engine = new PhysicsEngine();
            var target = new RecordingTarget();
            using var renderer = new PhysicsDebugRenderer(engine, target);
            renderer.IsEnabled = true;

            renderer.Draw(null!);   // null SpriteBatch is never dereferenced

            Assert.Equal(1, target.LoadFontCalls);
            Assert.Equal(1, target.RenderCalls);
            Assert.Same(renderer.DebugView, target.LastRendered);
        }

        [Fact]
        public void Draw_Enabled_WarmContent_SkipsFontLoad()
        {
            using var engine = new PhysicsEngine();
            var target = new RecordingTarget();
            using var renderer = new PhysicsDebugRenderer(engine, target);
            renderer.IsEnabled = true;
            SetContentLoaded(renderer, true);   // pretend LoadContent already ran

            renderer.Draw(null!);

            Assert.Equal(0, target.LoadFontCalls);
            Assert.Equal(1, target.RenderCalls);
        }

        // ───────────── Production adapter guard (no injected seam) ─────────────

        [Fact]
        public void LoadContent_ProductionTarget_BeforeSceneAttached_Throws()
        {
            using var engine = new PhysicsEngine();
            using var renderer = new PhysicsDebugRenderer(engine);   // no injected seam, no scene

            // EnsureDebugView succeeds (engine present), but the production adapter finds Game == null.
            Assert.Throws<InvalidOperationException>(() => renderer.LoadContent());
        }

        // ───────────── Reflection helpers (test-only) ─────────────

        private sealed class TestScene : Scene
        {
            protected override GameSystem[] LoadGameSystems() => Array.Empty<GameSystem>();
            protected override IEnumerator OnStartCoroutine() { yield break; }
        }

        private static void RegisterSystem(Scene scene, GameSystem system)
        {
            var dict = (System.Collections.Generic.Dictionary<Type, GameSystem>)typeof(Scene)
                .GetField("_gameSystems", BindingFlags.NonPublic | BindingFlags.Instance)!
                .GetValue(scene)!;
            dict[system.GetType()] = system;
        }

        private static void SetContentLoaded(PhysicsDebugRenderer renderer, bool value)
        {
            typeof(PhysicsDebugRenderer)
                .GetField("_contentLoaded", BindingFlags.NonPublic | BindingFlags.Instance)!
                .SetValue(renderer, value);
        }
    }
}
