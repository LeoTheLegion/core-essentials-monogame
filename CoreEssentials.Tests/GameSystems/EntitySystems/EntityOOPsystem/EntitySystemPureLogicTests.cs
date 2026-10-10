#nullable enable
using System;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem;
using Microsoft.Xna.Framework;
using Xunit;

namespace CoreEssentials.Tests.GameSystems.EntitySystems.EntityOOPsystem
{
    /// <summary>
    /// Device-free tests for the pure-logic paths of <see cref="EntitySystem"/> that need no production
    /// change to reach: single-entity removal (including the null guard), subtree detach/adopt with ID and
    /// game-system-reference bookkeeping, the linear fallback of the closest-entity query when spatial
    /// partitioning is disabled, and the CreateEntity error path when an entity's OnStart throws. The
    /// remaining uncovered blocks in this class are the debug-overlay draw (gated behind DebugMode) and the
    /// per-effect render branch — both device-bound and covered by the playground smoke-run.
    /// </summary>
    public class EntitySystemPureLogicTests
    {
        private static EntitySystem CreateSystem() => new EntitySystem();

        // ──────────────── Test entity fixtures ────────────────

        private class TestEntity : Entity
        {
            public override void Render(Microsoft.Xna.Framework.Graphics.SpriteBatch _spriteBatch) { }
        }

        private sealed class TrackDestroyEntity : TestEntity
        {
            public bool WasDestroyed;
            public override void OnDestroy() { base.OnDestroy(); WasDestroyed = true; }
        }

        private sealed class ThrowingStartEntity : TestEntity
        {
            public override void OnStart() => throw new InvalidOperationException("boom in OnStart");
        }

        // ──────────────── RemoveEntity ────────────────

        [Fact]
        public void RemoveEntity_Null_IsANoOp()
        {
            var system = CreateSystem();

            var ex = Record.Exception(() => system.RemoveEntity(null!));
            Assert.Null(ex);
        }

        [Fact]
        public void RemoveEntity_RemovesFromSystem_AndFiresOnDestroy()
        {
            var system = CreateSystem();
            var entity = (TrackDestroyEntity)system.CreateEntity(typeof(TrackDestroyEntity));
            Assert.Contains(entity, system.GetEntities());

            system.RemoveEntity(entity);

            Assert.DoesNotContain(entity, system.GetEntities());
            Assert.True(entity.WasDestroyed);
        }

        // ──────────────── FindClosest (linear fallback) ────────────────

        [Fact]
        public void FindClosest_SpatialDisabled_ReturnsNearestWithinRadius()
        {
            var system = CreateSystem();
            system.SpatialPartitioningEnabled = false;   // force the linear-search fallback

            var near = (TestEntity)system.CreateEntity(typeof(TestEntity));
            near.Position = new Vector2(10f, 0f);
            var far = (TestEntity)system.CreateEntity(typeof(TestEntity));
            far.Position = new Vector2(50f, 0f);

            var result = system.FindClosest(Vector2.Zero, radius: 60f);

            Assert.Same(near, result);
        }

        [Fact]
        public void FindClosest_NothingWithinRadius_ReturnsNull()
        {
            var system = CreateSystem();
            system.SpatialPartitioningEnabled = false;

            var entity = (TestEntity)system.CreateEntity(typeof(TestEntity));
            entity.Position = new Vector2(100f, 0f);

            Assert.Null(system.FindClosest(Vector2.Zero, radius: 10f));
        }

        [Fact]
        public void FindClosest_EmptySystem_ReturnsNull()
        {
            var system = CreateSystem();
            system.SpatialPartitioningEnabled = false;

            Assert.Null(system.FindClosest(Vector2.Zero, radius: 100f));
        }

        // ──────────────── DetachEntity / AdoptEntity ────────────────

        [Fact]
        public void DetachEntity_Null_IsANoOp()
        {
            var system = CreateSystem();

            var ex = Record.Exception(() => system.DetachEntity(null!));
            Assert.Null(ex);
        }

        [Fact]
        public void AdoptEntity_Null_IsANoOp()
        {
            var system = CreateSystem();

            var ex = Record.Exception(() => system.AdoptEntity(null!));
            Assert.Null(ex);
        }

        [Fact]
        public void DetachEntity_RemovesSubtreeAndClearsGameSystemRefs()
        {
            var system = CreateSystem();
            var root = (TestEntity)system.CreateEntity(typeof(TestEntity));
            var child = (TestEntity)system.CreateEntity(typeof(TestEntity));
            root.AddChild(child);   // link the subtree; both stay registered with the system

            Assert.Equal(2, system.GetEntities().Count);

            system.DetachEntity(root);

            Assert.Empty(system.GetEntities());
            Assert.Null(root.GetEntitySystem());   // subtree is now inert
            Assert.Null(child.GetEntitySystem());
        }

        [Fact]
        public void AdoptEntity_ReRegistersSubtreeWithIdAndGameSystemRef()
        {
            var system = CreateSystem();
            var root = new TestEntity();      // raw, already "started" elsewhere — not yet registered
            var child = new TestEntity();
            root.AddChild(child);

            system.AdoptEntity(root);

            Assert.Equal(2, system.GetEntities().Count);
            Assert.Same(system, root.GetEntitySystem());
            Assert.Same(system, child.GetEntitySystem());
            Assert.False(string.IsNullOrWhiteSpace(root.Id));   // EnsureId assigned an ID
        }

        [Fact]
        public void AdoptEntity_IsTheInverseOfDetach_RoundTripsSubtree()
        {
            var source = CreateSystem();
            var target = CreateSystem();
            var root = (TestEntity)source.CreateEntity(typeof(TestEntity));
            var child = (TestEntity)source.CreateEntity(typeof(TestEntity));
            root.AddChild(child);

            source.DetachEntity(root);
            Assert.Empty(source.GetEntities());

            target.AdoptEntity(root);

            Assert.Equal(2, target.GetEntities().Count);
            Assert.Same(target, root.GetEntitySystem());
            Assert.Same(target, child.GetEntitySystem());
        }

        // ──────────────── CreateEntity error path ────────────────

        [Fact]
        public void CreateEntity_WhenOnStartThrows_Rethrows()
        {
            var system = CreateSystem();

            var ex = Assert.Throws<InvalidOperationException>(
                () => system.CreateEntity(typeof(ThrowingStartEntity)));
            Assert.Contains("boom in OnStart", ex.Message);
        }
    }
}
