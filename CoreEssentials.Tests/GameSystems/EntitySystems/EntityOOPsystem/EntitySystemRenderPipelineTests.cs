#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CoreEssentials.Assets;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Components.BuiltIn;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Xunit;

namespace CoreEssentials.Tests.GameSystems.EntitySystems.EntityOOPsystem
{
    /// <summary>
    /// Drives <see cref="EntitySystem"/>'s render pipeline through the <see cref="IEntityDrawTarget"/> seam
    /// with a recording fake, asserting the grouping / effect-partition / Begin-End ordering without a
    /// graphics device. Also covers the pure-logic entity-management methods (create/clear) that live in the
    /// same file.
    /// </summary>
    public class EntitySystemRenderPipelineTests
    {
        private static EntitySystem CreateSystem() => new EntitySystem();

        // ────────────────────────── GroupEntitiesByZLayer ──────────────────────────

        [Fact]
        public void Group_ByZLayer_ReturnsAscendingLayersBackToFront()
        {
            var system = CreateSystem();
            var texA = new MockTexture2DAsset("a");
            var texB = new MockTexture2DAsset("b");

            var eFront = system.CreateEntity<TestEntity>(); eFront.RegisterForInstancedRendering(texA); eFront.SetZLayer(2);
            var eBack  = system.CreateEntity<TestEntity>(); eBack.RegisterForInstancedRendering(texB);  eBack.SetZLayer(0);
            var eMid   = system.CreateEntity<TestEntity>(); eMid.RegisterForInstancedRendering(texA);  eMid.SetZLayer(1);

            var (zLayers, noTexture) = system.GroupEntitiesByZLayer();

            Assert.Equal(new[] { 0, 1, 2 }, zLayers.Select(l => l.Key).ToArray());
            Assert.Empty(noTexture);
        }

        [Fact]
        public void Group_SameTextureInLayer_CoalescesIntoOneGroup()
        {
            var system = CreateSystem();
            var shared = new MockTexture2DAsset("shared");

            for (int i = 0; i < 3; i++)
            {
                var e = system.CreateEntity<TestEntity>();
                e.RegisterForInstancedRendering(shared);
                e.SetZLayer(5);
            }

            var (zLayers, noTexture) = system.GroupEntitiesByZLayer();

            var layer = zLayers.Single(l => l.Key == 5);
            var group = layer.Value[shared];
            Assert.Equal(3, group.Count);
        }

        [Fact]
        public void Group_InactiveEntity_IsExcluded()
        {
            var system = CreateSystem();
            var texA = new MockTexture2DAsset("a");

            var e1 = system.CreateEntity<TestEntity>(); e1.RegisterForInstancedRendering(texA); e1.SetZLayer(0);
            var e2 = system.CreateEntity<TestEntity>(); e2.RegisterForInstancedRendering(texA); e2.SetZLayer(0); e2.SetActive(false);

            var (zLayers, noTexture) = system.GroupEntitiesByZLayer();

            Assert.Single(zLayers.Single(l => l.Key == 0).Value[texA]); // only the active one
        }

        [Fact]
        public void Group_NoTextureEntity_GoesToNoTextureBucket()
        {
            var system = CreateSystem();
            var texA = new MockTexture2DAsset("a");

            var withTex = system.CreateEntity<TestEntity>(); withTex.RegisterForInstancedRendering(texA);
            var noTex = system.CreateEntity<TestEntity>(); // BatchTexture stays null

            var (zLayers, noTexture) = system.GroupEntitiesByZLayer();

            Assert.Same(noTex, Assert.Single(noTexture));
        }

        // ────────────────────────── PartitionByEffect ──────────────────────────

        [Fact]
        public void Partition_AllNullEffect_FormsSingleRun()
        {
            var system = CreateSystem();
            var list = new List<Entity>
            {
                system.CreateEntity<TestEntity>(),
                system.CreateEntity<TestEntity>(),
                system.CreateEntity<TestEntity>(),
            };

            var runs = EntitySystem.PartitionByEffect(list);

            Assert.Single(runs);
            Assert.Null(runs[0].Effect);
            Assert.Equal(3, runs[0].Entities.Count);
        }

        [Fact]
        public void Partition_DifferentSignature_SplitsRun()
        {
            var system = CreateSystem();
            var e1 = system.CreateEntity<TestEntity>(); AddShader(e1, "x", 1f);
            var e2 = system.CreateEntity<TestEntity>(); AddShader(e2, "x", 2f); // different value → different signature
            var e3 = system.CreateEntity<TestEntity>(); AddShader(e3, "x", 1f);

            var runs = EntitySystem.PartitionByEffect(new List<Entity> { e1, e2, e3 });

            // Three distinct contiguous signatures (1,2,1) yield three runs.
            Assert.Equal(3, runs.Count);
            Assert.All(runs, r => Assert.Single(r.Entities));
        }

        [Fact]
        public void Partition_IdenticalAdjacentEffectCoalesces()
        {
            var system = CreateSystem();
            var e1 = system.CreateEntity<TestEntity>(); AddShader(e1, "x", 7f);
            var e2 = system.CreateEntity<TestEntity>(); AddShader(e2, "x", 7f); // same value → same signature

            var runs = EntitySystem.PartitionByEffect(new List<Entity> { e1, e2 });

            Assert.Single(runs);
            Assert.Equal(2, runs[0].Entities.Count);
        }

        [Fact]
        public void Partition_NullThenNonShader_BothFormNullRunsButDistinctOnlyBySignature()
        {
            // A shader-less entity has Signature "" ; an empty-set shader also "". They coalesce when adjacent.
            var system = CreateSystem();
            var bare = system.CreateEntity<TestEntity>();                       // no ShaderComponent
            var emptyShader = system.CreateEntity<TestEntity>(); AddShader(emptyShader, null!, 0f);

            var runs = EntitySystem.PartitionByEffect(new List<Entity> { bare, emptyShader });

            Assert.Single(runs); // both resolve to (null effect, "") and merge
        }

        // ────────────────────── RenderEntities (recording fake) ──────────────────────

        [Fact]
        public void RenderEntities_OpenCloseBalancedOneBeginPerRun()
        {
            var system = CreateSystem();
            var texA = new MockTexture2DAsset("a");
            for (int i = 0; i < 4; i++) { var e = system.CreateEntity<TestEntity>(); e.RegisterForInstancedRendering(texA); }

            var target = new RecordingTarget();
            system.RenderEntities(target);

            Assert.Equal(1, target.BeginCount);       // one texture group, one null-effect run
            Assert.Equal(1, target.EndCount);
            Assert.Equal(4, target.DrawCount);        // all four entities drawn in the run
            Assert.True(target.LastOpIsEnd());         // batch closed after drawing
        }

        [Fact]
        public void RenderEntities_NoTextureEntitiesDrawnInOwnRun()
        {
            var system = CreateSystem();
            _ = system.CreateEntity<TestEntity>(); // no texture → no-texture run
            _ = system.CreateEntity<TestEntity>();

            var target = new RecordingTarget();
            system.RenderEntities(target);

            Assert.Equal(2, target.DrawCount);
            Assert.True(target.BeginCount >= 1 && target.EndCount == target.BeginCount);
        }

        [Fact]
        public void RenderEntities_DifferentSignaturesProduceMultipleBatches()
        {
            var system = CreateSystem();
            var texA = new MockTexture2DAsset("a");
            var e1 = system.CreateEntity<TestEntity>(); e1.RegisterForInstancedRendering(texA); AddShader(e1, "x", 1f);
            var e2 = system.CreateEntity<TestEntity>(); e2.RegisterForInstancedRendering(texA); AddShader(e2, "x", 9f);

            var target = new RecordingTarget();
            system.RenderEntities(target);

            Assert.Equal(2, target.BeginCount); // two signatures → two Begin/End runs
            Assert.Equal(2, target.EndCount);
        }

        // ────────────────────── Pure-logic: CreateEntity / Unstarted / Clear ──────────────────────

        [Fact]
        public void CreateEntity_Type_CallsOnStart()
        {
            var system = CreateSystem();
            var e = (RecordingEntity)system.CreateEntity(typeof(RecordingEntity));

            Assert.True(e.Awoken);
            Assert.True(e.Started);
        }

        [Fact]
        public void CreateEntity_Typed_ReturnsConfiguredEntity()
        {
            var system = CreateSystem();
            var e = system.CreateEntity<RecordingEntity>();

            Assert.IsType<RecordingEntity>(e);
            Assert.True(e.Started);
        }

        [Fact]
        public void CreateEntityUnstarted_DoesNotCallOnStart()
        {
            var system = CreateSystem();
            var e = (RecordingEntity)system.CreateEntityUnstarted(typeof(RecordingEntity));

            Assert.True(e.Awoken);
            Assert.False(e.Started); // configure now, start later
        }

        [Fact]
        public void CreateInstanceWithOptionalParams_FillsOmittedTrailingDefaults()
        {
            var system = CreateSystem();
            // Ctor (int required, float optional=2.5f) — pass only the required arg; default must fill in.
            var e = (OptionalArgEntity)system.CreateEntity(typeof(OptionalArgEntity), 42);

            Assert.Equal(42, e.Required);
            Assert.Equal(2.5f, e.Optional, 3);
        }

        [Fact]
        public void CreateEntity_NoMatchingConstructor_ThrowsWithConstructorList()
        {
            var system = CreateSystem();

            // Needs one non-optional int; we pass none → no ctor matches → describe available ctors.
            var ex = Assert.Throws<InvalidOperationException>(() => system.CreateEntity(typeof(RequiredIntEntity)));
            Assert.Contains("Available constructors", ex.Message);
        }

        [Fact]
        public void ClearEntities_RemovesAllAndCallsOnDestroy()
        {
            var system = CreateSystem();
            var e1 = (RecordingEntity)system.CreateEntity<RecordingEntity>();
            var e2 = (RecordingEntity)system.CreateEntity<RecordingEntity>();

            system.ClearEntities();

            // ClearEntities calls OnDestroy and removes the entities from the system.
            Assert.True(e1.DestroyedNotified);
            Assert.True(e2.DestroyedNotified);
            Assert.Equal(0, EntityCount(system));
        }

        // ────────────────────────── helpers & fixtures ──────────────────────────

        private static ShaderComponent AddShader(Entity e, string? name, float value)
        {
            var s = new ShaderComponent();
            if (name != null) s.SetFloat(name, value);
            return e.AddComponent(s);
        }

        private static int EntityCount(EntitySystem system)
        {
            var field = typeof(EntitySystem).GetField("_entities", BindingFlags.NonPublic | BindingFlags.Instance)!;
            return ((List<Entity>)field.GetValue(system)!).Count;
        }

        /// <summary>Records which draw ops the pipeline issued, in order.</summary>
        private sealed class RecordingTarget : IEntityDrawTarget
        {
            public readonly List<string> Ops = new();
            public int BeginCount => Ops.Count(o => o.StartsWith("begin:"));
            public int EndCount => Ops.Count(o => o == "end");
            public int DrawCount => Ops.Count(o => o.StartsWith("draw:"));

            public void Begin(Effect? effect, Matrix? viewMatrix) => Ops.Add($"begin:{(effect == null ? "null" : "fx")}");
            public void End() => Ops.Add("end");
            public void SyncProjection(Effect? effect) { /* effects are null in these tests */ }
            public void DrawEntity(Entity entity) => Ops.Add("draw:" + entity.Id);

            public bool LastOpIsEnd() => Ops[^1] == "end";
        }

        private sealed class MockTexture2DAsset : Texture2DAsset
        {
            public MockTexture2DAsset(string name) : base(name) { }
            public override void Load(IContentManager contentManager) { }
            public override void Unload(IContentManager contentManager) { }
        }

        private sealed class TestEntity : Entity
        {
            public override void Render(SpriteBatch _spriteBatch) { }
        }

        /// <summary>Records which lifecycle hooks fired so the create/clear paths can be asserted.</summary>
        private sealed class RecordingEntity : Entity
        {
            public bool Awoken { get; private set; }
            public bool Started { get; private set; }
            public bool DestroyedNotified { get; private set; }

            public override void OnAwake() { base.OnAwake(); Awoken = true; }
            public override void OnStart() { base.OnStart(); Started = true; }
            public override void OnDestroy() { base.OnDestroy(); DestroyedNotified = true; }
        }

        /// <summary>Entity with an optional-parameter constructor to exercise default-filling.</summary>
        private sealed class OptionalArgEntity : Entity
        {
            public int Required { get; }
            public float Optional { get; }
            public OptionalArgEntity(int required, float optional = 2.5f)
            {
                Required = required;
                Optional = optional;
            }
            public override void Render(SpriteBatch _spriteBatch) { }
        }

        /// <summary>Entity whose only constructor requires an int — nothing to fill, so zero-arg creation must fail.</summary>
        private sealed class RequiredIntEntity : Entity
        {
            public RequiredIntEntity(int value) { }
            public override void Render(SpriteBatch _spriteBatch) { }
        }
    }
}
