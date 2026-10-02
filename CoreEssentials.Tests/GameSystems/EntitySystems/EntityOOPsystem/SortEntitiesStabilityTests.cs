using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Xunit;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem;

namespace CoreEssentials.Tests.GameSystems.EntitySystems.EntityOOPsystem
{
    /// <summary>
    /// Regression tests for stable entity sorting.
    ///
    /// Background: <c>EntitySystem.SortEntities()</c> previously relied on the unstable
    /// <see cref="List{T}.Sort(Comparison{T})"/> method. Entities that share a sort value
    /// (the default is <c>0</c>) were permuted into an arbitrary, frame-varying order.
    /// For overlapping translucent sprites within the same ZLayer this produced a visible
    /// per-frame brightness shimmer because premultiplied-alpha blending is order-dependent.
    ///
    /// These tests encode the desired contract: entities with equal sort values must keep
    /// their relative insertion order, and that order must remain deterministic across
    /// repeated sorts and as the entity set changes over time.
    /// </summary>
    public class SortEntitiesStabilityTests
    {
        [Fact]
        public void SortEntities_PreservesInsertionOrder_ForEqualSortValues()
        {
            var system = new EntitySystem();
            const int count = 32;

            var created = new List<Entity>();
            for (int i = 0; i < count; i++)
                created.Add(system.CreateEntity<StableTestEntity>()); // all default to sort = 0

            system.SortEntities();

            // Reference-equality comparison: the same instances, in creation order.
            Assert.Equal(created, system.GetEntities());
        }

        [Fact]
        public void SortEntities_IsIdempotent_ForEqualSortValues()
        {
            // Use > 16 entities to exercise the unstable quicksort path in List<T>.Sort.
            var system = new EntitySystem();
            const int count = 32;

            for (int i = 0; i < count; i++)
                system.CreateEntity<StableTestEntity>();

            system.SortEntities();
            List<Entity> firstPass = new List<Entity>(system.GetEntities());

            // Simulate the per-frame re-sort that triggered the flicker.
            for (int i = 0; i < 10; i++)
                system.SortEntities();

            Assert.Equal(firstPass, system.GetEntities());
        }

        [Fact]
        public void SortEntities_KeepsHigherSortFirst_AndStableWithinTies()
        {
            // Use > 16 entities total to exercise the unstable quicksort path.
            var system = new EntitySystem();

            // Create 20 entities: 15 at default sort=0 (interspersed) and 5 at distinct higher values.
            var zeroSort = new List<Entity>();
            var highSort = new List<Entity>();
            int highValue = 1;

            for (int i = 0; i < 20; i++)
            {
                Entity e = system.CreateEntity<StableTestEntity>();
                if (i % 4 == 3) // every 4th entity gets a distinct higher sort value
                {
                    e.SetSort(highValue++);
                    highSort.Add(e);
                }
                else
                {
                    zeroSort.Add(e); // default sort = 0
                }
            }

            system.SortEntities();

            List<Entity> result = system.GetEntities();

            // Higher sort values come first (descending), in creation order among themselves.
            for (int i = 0; i < highSort.Count; i++)
                Assert.Same(highSort[highSort.Count - 1 - i], result[i]);

            // All zero-sort entities follow, preserving their relative creation order.
            for (int i = 0; i < zeroSort.Count; i++)
                Assert.Same(zeroSort[i], result[highSort.Count + i]);
        }

        [Fact]
        public void SortEntities_RemainsStable_AsEntitySetChanges()
        {
            // Use > 16 baseline entities to exercise the unstable quicksort path,
            // matching the real-world bug scenario where a scene has many default-sort sprites.
            var system = new EntitySystem();
            const int baselineCount = 24;

            var baseline = new List<Entity>();
            for (int i = 0; i < baselineCount; i++)
                baseline.Add(system.CreateEntity<StableTestEntity>()); // all default to sort = 0

            system.SortEntities();
            List<Entity> baselineOrder = new List<Entity>(system.GetEntities());

            // Simulate churn over several frames: transient entities spawn and die,
            // triggering per-frame re-sorts of a large, mostly-equal-key list.
            for (int frame = 0; frame < 5; frame++)
            {
                Entity transient = system.CreateEntity<StableTestEntity>();
                system.SortEntities();
                system.RemoveEntity(transient);
                system.SortEntities();

                // After each churn cycle, the baseline entities must retain their order.
                Assert.Equal(baselineOrder, system.GetEntities());
            }
        }

        [Fact]
        public void SortEntities_PreservesStability_WhenBaselineIsReshuffledBeforeSort()
        {
            // Stress test: manually permute the entity list before sorting to simulate
            // an arbitrary pre-sort state, then verify the stable sort always recovers
            // the correct relative order for equal-key entities.
            var system = new EntitySystem();
            const int count = 32;

            var baseline = new List<Entity>();
            for (int i = 0; i < count; i++)
                baseline.Add(system.CreateEntity<StableTestEntity>()); // all sort = 0

            system.SortEntities();
            List<Entity> expectedOrder = new List<Entity>(system.GetEntities());

            // Reverse the list to create a maximally different pre-sort arrangement.
            var entities = system.GetEntities();
            entities.Reverse();
            system.SortEntities();

            // A stable sort must recover the original relative order regardless of input permutation.
            Assert.Equal(expectedOrder, system.GetEntities());
        }

        private class StableTestEntity : Entity
        {
            public override void Update(GameTime gameTime) { }
            public override void Render(SpriteBatch spriteBatch) { }
        }
    }
}
