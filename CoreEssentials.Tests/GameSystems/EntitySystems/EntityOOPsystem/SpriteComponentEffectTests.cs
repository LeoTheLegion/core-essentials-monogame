#nullable enable
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using CoreEssentials.Assets;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Components.BuiltIn;
using Microsoft.Xna.Framework.Graphics;
using Xunit;

namespace CoreEssentials.Tests.GameSystems.EntitySystems.EntityOOPsystem
{
    /// <summary>
    /// Tests for per-sprite <see cref="Effect"/> support on the built-in
    /// <see cref="SpriteComponent"/> and the effect-based render partitioning in
    /// <see cref="EntitySystem"/>. Real MonoGame <see cref="Effect"/> objects cannot be created
    /// without a live GraphicsDevice, so these use uninitialized-effect fakes — which is enough to
    /// assert identity/precedence/resolution behavior headlessly.
    /// </summary>
    public class SpriteComponentEffectTests
    {
        private sealed class TestEntity : Entity
        {
            public override void Render(Microsoft.Xna.Framework.Graphics.SpriteBatch _spriteBatch) { }
        }

        // ===== EffectiveEffect precedence =====

        [Fact]
        public void EffectiveEffect_NothingSet_ReturnsNull()
        {
            var comp = new SpriteComponent();

            Assert.Null(comp.EffectiveEffect);
        }

        [Fact]
        public void EffectiveEffect_ExplicitEffect_WinsOverAsset()
        {
            var explicitEffect = CreateFakeEffect();
            var comp = new SpriteComponent { Effect = explicitEffect, EffectAsset = "Effects/explicit_wins.xml" };

            Assert.Same(explicitEffect, comp.EffectiveEffect);
        }

        [Fact]
        public void EffectiveEffect_OnlyExplicit_ReturnsIt()
        {
            var explicitEffect = CreateFakeEffect();
            var comp = new SpriteComponent { Effect = explicitEffect };

            Assert.Same(explicitEffect, comp.EffectiveEffect);
        }

        // ===== OnAttach resolution of EffectAsset =====

        [Fact]
        public void OnAttach_WithEffectAsset_ResolvesAndExposesEffective()
        {
            AssetManager.Init(new EffectMockContentManager());
            var entity = new TestEntity();

            var comp = entity.AddComponent(new SpriteComponent { EffectAsset = "Effects/glow_attach.xml" });

            Assert.Null(comp.Effect); // resolved, not explicit
            Assert.NotNull(comp.EffectiveEffect);
        }

        [Fact]
        public void OnAttach_MissingEffectAsset_LogsAndLeavesNull()
        {
            AssetManager.Init(new ThrowingContentManager());
            var entity = new TestEntity();

            var comp = entity.AddComponent(new SpriteComponent { EffectAsset = "Effects/does_not_exist.xml" });

            Assert.Null(comp.EffectiveEffect);
        }

        [Fact]
        public void OnDetach_ClearsResolvedEffect()
        {
            AssetManager.Init(new EffectMockContentManager());
            var entity = new TestEntity();

            var comp = entity.AddComponent(new SpriteComponent { EffectAsset = "Effects/glow_detach.xml" });
            Assert.NotNull(comp.EffectiveEffect);

            entity.RemoveComponent<SpriteComponent>();

            // After detach the resolved (asset-derived) effect is cleared; an explicit one would remain.
            Assert.Null(comp.EffectiveEffect);
        }

        // ===== Entity.GetRenderEffect =====

        [Fact]
        public void GetRenderEffect_WithExplicitEffect_ReturnsIt()
        {
            var entity = new TestEntity();
            var effect = CreateFakeEffect();
            entity.AddComponent(new SpriteComponent { Effect = effect });

            Assert.Same(effect, entity.GetRenderEffect());
        }

        [Fact]
        public void GetRenderEffect_WithoutSpriteComponent_ReturnsNull()
        {
            var entity = new TestEntity();

            Assert.Null(entity.GetRenderEffect());
        }

        [Fact]
        public void GetRenderEffect_SpriteComponentWithoutEffect_ReturnsNull()
        {
            var entity = new TestEntity();
            entity.AddComponent(new SpriteComponent());

            Assert.Null(entity.GetRenderEffect());
        }

        // ===== PartitionByEffect (batcher effect grouping key) =====

        private sealed class EffectEntity : Entity
        {
            public Effect? TestEffect { get; set; }
            public override void Render(Microsoft.Xna.Framework.Graphics.SpriteBatch _spriteBatch) { }
            public override Effect? GetRenderEffect() => TestEffect;
        }

        [Fact]
        public void Partition_AllNullEffects_SingleRun_NoRegression()
        {
            var entities = new List<Entity> { NewEffectEntity(null), NewEffectEntity(null), NewEffectEntity(null) };

            var runs = InvokePartitionByEffect(entities);

            // No effect anywhere -> exactly one null-effect run (the previous single Begin/End behavior).
            Assert.Single(runs);
            Assert.Null(runs[0].Effect);
            Assert.Equal(3, runs[0].Entities.Count);
        }

        [Fact]
        public void Partition_DistinctEffects_SplitIntoRuns_InOrder()
        {
            var a = CreateFakeEffect();
            var b = CreateFakeEffect();
            var entities = new List<Entity>
            {
                NewEffectEntity(a),
                NewEffectEntity(b),
                NewEffectEntity(a)
            };

            var runs = InvokePartitionByEffect(entities);

            Assert.Equal(3, runs.Count);
            Assert.Same(a, runs[0].Effect);
            Assert.Same(b, runs[1].Effect);
            Assert.Same(a, runs[2].Effect);
            Assert.All(runs, r => Assert.Single(r.Entities));
        }

        [Fact]
        public void Partition_AdjacentSameEffect_Coalesce()
        {
            var a = CreateFakeEffect();
            var b = CreateFakeEffect();
            var entities = new List<Entity>
            {
                NewEffectEntity(a),
                NewEffectEntity(a),
                NewEffectEntity(b)
            };

            var runs = InvokePartitionByEffect(entities);

            Assert.Equal(2, runs.Count);
            Assert.Same(a, runs[0].Effect);
            Assert.Equal(2, runs[0].Entities.Count);
            Assert.Same(b, runs[1].Effect);
            Assert.Single(runs[1].Entities);
        }

        [Fact]
        public void Partition_PreservesEntityOrderWithinAndAcrossRuns()
        {
            var a = CreateFakeEffect();
            var e1 = NewEffectEntity(a);
            var e2 = NewEffectEntity(null);
            var e3 = NewEffectEntity(a);

            var runs = InvokePartitionByEffect(new List<Entity> { e1, e2, e3 });

            Assert.Equal(3, runs.Count);
            Assert.Same(e1, runs[0].Entities[0]);
            Assert.Same(e2, runs[1].Entities[0]);
            Assert.Same(e3, runs[2].Entities[0]);
        }

        [Fact]
        public void Partition_EmptyList_NoRuns()
        {
            var runs = InvokePartitionByEffect(new List<Entity>());

            Assert.Empty(runs);
        }

        // ===== Helpers =====

        private static EffectEntity NewEffectEntity(Effect? effect) => new EffectEntity { TestEffect = effect };

        private static Effect CreateFakeEffect() =>
            (Effect)RuntimeHelpers.GetUninitializedObject(typeof(Effect));

        private static List<(Effect? Effect, List<Entity> Entities)> InvokePartitionByEffect(List<Entity> entities)
        {
            var method = typeof(EntitySystem).GetMethod(
                "PartitionByEffect",
                BindingFlags.NonPublic | BindingFlags.Static);

            return (List<(Effect? Effect, List<Entity> Entities)>)method!.Invoke(null, new object[] { entities })!;
        }

        // A content manager that returns a fresh, distinct Effect for every Load<Effect> call.
        private sealed class EffectMockContentManager : IContentManager
        {
            public T Load<T>(string assetName)
            {
                if (typeof(T) == typeof(Effect))
                    return (T)(object)CreateFakeEffect();
                throw new InvalidOperationException($"Unexpected load type {typeof(T)} for '{assetName}'");
            }

            public void Unload(string assetName) { }
        }

        // A content manager that always fails, to exercise the swallowed-resolution path.
        private sealed class ThrowingContentManager : IContentManager
        {
            public T Load<T>(string assetName) => throw new InvalidOperationException($"Asset not found: {assetName}");
            public void Unload(string assetName) { }
        }
    }
}
