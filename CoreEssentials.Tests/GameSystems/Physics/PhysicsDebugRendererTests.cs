#nullable enable
using System;
using CoreEssentials.GameSystems.Physics.Engines.Aether;
using Microsoft.Xna.Framework.Graphics;
using nkast.Aether.Physics2D.Diagnostics;
using Xunit;

namespace CoreEssentials.Tests.GameSystems.Physics
{
    /// <summary>
    /// Device-free tests for <see cref="PhysicsDebugRenderer"/>: constructor wiring, flag defaults, the
    /// dispose lifecycle, and the guard rails (no-engine / no-game throws). The Aether <c>RenderDebugData</c>
    /// call in <c>Draw</c> is left uncovered — it needs a loaded font + graphics device.
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
    }
}
