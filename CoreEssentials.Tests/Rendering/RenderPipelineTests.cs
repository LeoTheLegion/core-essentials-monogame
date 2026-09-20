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
    /// Tests for the static <see cref="RenderPipeline"/>: the post-pass registry, the pre-pass registry,
    /// and the render-to-target opt-in flag. The full-screen quad drawing and the actual render-target
    /// bind/blit need a live GraphicsDevice and are exercised by the playground smoke-run; here we verify
    /// the registry contracts, ordering, the no-op guards that keep the default game loop untouched, and
    /// the PostPass mode/parameter metadata.
    /// </summary>
    public class RenderPipelineTests : IDisposable
    {
        // The pipeline is a static registry; isolate tests by resetting all state before/after each one.
        public void Dispose() => RenderPipeline.ResetForTesting();

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
            Assert.Same(a, passes[0].Effect);
            Assert.Same(b, passes[1].Effect);
            Assert.Same(c, passes[2].Effect);
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
            Assert.Same(b, passes[0].Effect);
            Assert.Same(a, passes[1].Effect);

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
            (snapshot as List<PostPass>)?.Clear(); // defensive: only meaningful if exposed directly

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

        // ===== PostPass mode / parameter metadata =====

        [Fact]
        public void PostPass_Default_IsAdditive_WithDefaultInputParameter()
        {
            var pass = new PostPass(CreateFakeEffect());

            Assert.False(pass.SamplesSceneTarget);
            Assert.Equal(PostPass.DefaultInputParameterName, pass.InputParameterName);
        }

        [Fact]
        public void PostPass_SamplingMode_HonorsCustomInputParameter()
        {
            var pass = new PostPass(CreateFakeEffect(), samplesSceneTarget: true, inputParameterName: "SceneTex");

            Assert.True(pass.SamplesSceneTarget);
            Assert.Equal("SceneTex", pass.InputParameterName);
        }

        [Fact]
        public void PostPass_SamplingMode_WithBlankInputParameter_FallsBackToDefault()
        {
            var pass = new PostPass(CreateFakeEffect(), samplesSceneTarget: true, inputParameterName: "  ");

            Assert.True(pass.SamplesSceneTarget);
            Assert.Equal(PostPass.DefaultInputParameterName, pass.InputParameterName);
        }

        [Fact]
        public void AddPostPass_AdditiveByDefault()
        {
            RenderPipeline.AddPostPass(CreateFakeEffect());

            Assert.False(RenderPipeline.PostPasses[0].SamplesSceneTarget);
        }

        // ===== Pre passes =====

        [Fact]
        public void AddPrePass_PreservesRegistrationOrder_AndRunsInOrder()
        {
            var order = new List<int>();
            Action<GameTime> first = _ => order.Add(1);
            Action<GameTime> second = _ => order.Add(2);

            RenderPipeline.AddPrePass(first);
            RenderPipeline.AddPrePass(second);

            Assert.Equal(2, RenderPipeline.PrePassCount);

            RenderPipeline.DrawPrePasses(new GameTime());

            Assert.Equal(new List<int> { 1, 2 }, order);
        }

        [Fact]
        public void AddPrePass_Null_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => RenderPipeline.AddPrePass(null!));
        }

        [Fact]
        public void RemovePrePass_RemovesMatching_ReturnsTrue()
        {
            var order = new List<int>();
            Action<GameTime> first = _ => order.Add(1);
            Action<GameTime> second = _ => order.Add(2);

            RenderPipeline.AddPrePass(first);
            RenderPipeline.AddPrePass(second);

            Assert.True(RenderPipeline.RemovePrePass(first));
            Assert.Equal(1, RenderPipeline.PrePassCount);

            RenderPipeline.DrawPrePasses(new GameTime());
            Assert.Equal(new List<int> { 2 }, order);
        }

        [Fact]
        public void RemovePrePass_NotRegistered_ReturnsFalse()
        {
            Assert.False(RenderPipeline.RemovePrePass(_ => { }));
        }

        [Fact]
        public void ClearPrePasses_EmptiesRegistry()
        {
            RenderPipeline.AddPrePass(_ => { });
            RenderPipeline.AddPrePass(_ => { });

            RenderPipeline.ClearPrePasses();

            Assert.Equal(0, RenderPipeline.PrePassCount);
        }

        [Fact]
        public void DrawPrePasses_NoneRegistered_IsNoOp()
        {
            // No throw when nothing is registered (empty-pipeline fast path).
            RenderPipeline.DrawPrePasses(new GameTime());
        }

        // ===== Render-to-target opt-in flag =====

        [Fact]
        public void RenderToTarget_DefaultIsDisabled()
        {
            Assert.False(RenderPipeline.RenderToTargetEnabled);
        }

        [Fact]
        public void EnableRenderToTarget_TogglesFlag()
        {
            RenderPipeline.EnableRenderToTarget(true);
            Assert.True(RenderPipeline.RenderToTargetEnabled);

            RenderPipeline.EnableRenderToTarget(false);
            Assert.False(RenderPipeline.RenderToTargetEnabled);
        }

        [Fact]
        public void SceneTarget_Null_UntilProcessPassRuns()
        {
            // With no device and the process pass not yet run, the scene target is null.
            Assert.Null(RenderPipeline.SceneTarget);
        }

        [Fact]
        public void ResetForTesting_ClearsAllState()
        {
            RenderPipeline.AddPostPass(CreateFakeEffect());
            RenderPipeline.AddPrePass(_ => { });
            RenderPipeline.EnableRenderToTarget(true);

            RenderPipeline.ResetForTesting();

            Assert.Equal(0, RenderPipeline.PostPassCount);
            Assert.Equal(0, RenderPipeline.PrePassCount);
            Assert.False(RenderPipeline.RenderToTargetEnabled);
            Assert.Null(RenderPipeline.SceneTarget);
        }
    }
}
