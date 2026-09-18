#nullable enable
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using CoreEssentials.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Xunit;

namespace CoreEssentials.Tests.Rendering
{
    /// <summary>
    /// Tests for the static <see cref="RenderPipeline"/> post-pass registry. The full-screen quad
    /// drawing itself needs a live GraphicsDevice and is exercised by the playground smoke-run; here
    /// we verify the registry contract (ordering, add/remove/clear) and the no-op guard that keeps
    /// the default game loop untouched when nothing is registered.
    /// </summary>
    public class RenderPipelineTests : IDisposable
    {
        // The pipeline is a static registry; isolate tests by clearing before/after each one.
        public void Dispose() => RenderPipeline.ClearPostPasses();

        private static Effect CreateFakeEffect() =>
            (Effect)RuntimeHelpers.GetUninitializedObject(typeof(Effect));

        [Fact]
        public void AddPostPass_PreservesRegistrationOrder()
        {
            var a = CreateFakeEffect();
            var b = CreateFakeEffect();
            var c = CreateFakeEffect();

            RenderPipeline.AddPostPass(a);
            RenderPipeline.AddPostPass(b);
            RenderPipeline.AddPostPass(c);

            Assert.Equal(3, RenderPipeline.PostPassCount);
            var passes = RenderPipeline.PostPasses;
            Assert.Same(a, passes[0]);
            Assert.Same(b, passes[1]);
            Assert.Same(c, passes[2]);
        }

        [Fact]
        public void AddPostPass_Null_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => RenderPipeline.AddPostPass(null!));
        }

        [Fact]
        public void AddPostPass_DuplicatesAllowed_DrawnTwice()
        {
            var a = CreateFakeEffect();

            RenderPipeline.AddPostPass(a);
            RenderPipeline.AddPostPass(a);

            Assert.Equal(2, RenderPipeline.PostPassCount);
        }

        [Fact]
        public void RemovePostPass_RemovesFirstOccurrence()
        {
            var a = CreateFakeEffect();
            var b = CreateFakeEffect();

            RenderPipeline.AddPostPass(a);
            RenderPipeline.AddPostPass(b);
            RenderPipeline.AddPostPass(a);

            Assert.True(RenderPipeline.RemovePostPass(a));
            Assert.Equal(2, RenderPipeline.PostPassCount);
            // First occurrence removed: order is now b, a.
            var passes = RenderPipeline.PostPasses;
            Assert.Same(b, passes[0]);
            Assert.Same(a, passes[1]);

            Assert.True(RenderPipeline.RemovePostPass(a));
            Assert.False(RenderPipeline.RemovePostPass(a)); // already gone
        }

        [Fact]
        public void RemovePostPass_NotRegistered_ReturnsFalse()
        {
            var a = CreateFakeEffect();

            Assert.False(RenderPipeline.RemovePostPass(a));
            Assert.Equal(0, RenderPipeline.PostPassCount);
        }

        [Fact]
        public void ClearPostPasses_EmptiesRegistry()
        {
            RenderPipeline.AddPostPass(CreateFakeEffect());
            RenderPipeline.AddPostPass(CreateFakeEffect());

            RenderPipeline.ClearPostPasses();

            Assert.Equal(0, RenderPipeline.PostPassCount);
            Assert.Empty(RenderPipeline.PostPasses);
        }

        [Fact]
        public void PostPasses_ReturnsSnapshot_NotLiveView()
        {
            var a = CreateFakeEffect();
            RenderPipeline.AddPostPass(a);

            var snapshot = RenderPipeline.PostPasses;
            (snapshot as List<Effect>)?.Clear(); // defensive: only meaningful if exposed directly

            Assert.Equal(1, RenderPipeline.PostPassCount);
        }

        [Fact]
        public void DrawPostPasses_NoPassesRegistered_DoesNotTouchSpriteBatch()
        {
            // No-op guard (acceptance criterion: leaving everything unset keeps current behavior).
            // The pipeline must return before touching the SpriteBatch, so a null batch is safe —
            // if this ever throws, the empty-pipeline fast path has regressed. A throw fails the test.
            RenderPipeline.DrawPostPasses(new GameTime(), null!);
        }
    }
}
