using System;
using Xunit;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem;
using CoreEssentials.Coroutines;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace CoreEssentials.Tests.GameSystems.EntitySystems.EntityOOPsystem
{
    public class EntityTests
    {
        [Fact]
        public void Entity_SetsActiveToFalse()
        {
            EntitySystem entitySystem = new EntitySystem();
            var entity = entitySystem.CreateEntity<TestEntity>();
            entity.SetActive(false);
            Assert.False(entity.GetActive());
        }

        [Fact]
        public void Entity_SetsSortValue()
        {
            EntitySystem entitySystem = new EntitySystem();
            var entity = entitySystem.CreateEntity<TestEntity>();
            entity.SetSort(5);
            Assert.Equal(5, entity.GetSort());
        }

        [Fact]
        public void DestroyAfter_SchedulesDestruction()
        {
            EntitySystem entitySystem = new EntitySystem();
            var entity = entitySystem.CreateEntity<TestEntity>();

            entity.DestroyAfter(TimeSpan.FromSeconds(10));

            Assert.False(entity.Destroyed);
            Assert.True(entity.GetActive());
        }

        [Fact]
        public void DestroyAfter_DestroysEntityAfterDelay()
        {
            EntitySystem entitySystem = new EntitySystem();
            var entity = entitySystem.CreateEntity<TestEntity>();

            entity.DestroyAfter(TimeSpan.FromMilliseconds(1));

            // Advance total game time past the 1ms delay using increasing TotalGameTime
            double currentTime = 0.0;
            for (int i = 0; i < 10; i++)
            {
                currentTime += 0.05; // 50ms per tick
                var gameTime = new GameTime(TimeSpan.FromSeconds(currentTime), TimeSpan.FromMilliseconds(50));
                CoroutineManager.Update(gameTime);
            }

            entitySystem.Update(new GameTime(TimeSpan.FromSeconds(currentTime), TimeSpan.Zero));

            Assert.True(entity.Destroyed);
            Assert.False(entity.GetActive());
        }

        [Fact]
        public void CancelDestroyAfter_CancelPendingDestruction()
        {
            EntitySystem entitySystem = new EntitySystem();
            var entity = entitySystem.CreateEntity<TestEntity>();

            entity.DestroyAfter(TimeSpan.FromMilliseconds(1));
            bool cancelled = entity.CancelDestroyAfter();

            Assert.True(cancelled);
            Assert.False(entity.Destroyed);
            Assert.True(entity.GetActive());

            // Even after coroutine update, entity should still be alive
            var gameTime = new GameTime(TimeSpan.Zero, TimeSpan.FromMilliseconds(10));
            CoroutineManager.Update(gameTime);
            entitySystem.Update(gameTime);

            Assert.False(entity.Destroyed);
        }

        [Fact]
        public void CancelDestroyAfter_ReturnsFalseWhenNothingPending()
        {
            EntitySystem entitySystem = new EntitySystem();
            var entity = entitySystem.CreateEntity<TestEntity>();

            bool cancelled = entity.CancelDestroyAfter();

            Assert.False(cancelled);
        }

        [Fact]
        public void DestroyAfter_ThrowsOnNegativeDelay()
        {
            EntitySystem entitySystem = new EntitySystem();
            var entity = entitySystem.CreateEntity<TestEntity>();

            Assert.Throws<ArgumentOutOfRangeException>(() => entity.DestroyAfter(TimeSpan.FromMilliseconds(-1)));
        }

        [Fact]
        public void DestroyAfter_CancelsPreviousScheduledDestroy()
        {
            EntitySystem entitySystem = new EntitySystem();
            var entity = entitySystem.CreateEntity<TestEntity>();

            entity.DestroyAfter(TimeSpan.FromMilliseconds(1));
            // Second call should cancel the first
            entity.DestroyAfter(TimeSpan.FromSeconds(10));

            var gameTime = new GameTime(TimeSpan.Zero, TimeSpan.FromMilliseconds(10));
            CoroutineManager.Update(gameTime);
            entitySystem.Update(gameTime);

            Assert.False(entity.Destroyed);
        }

        [Fact]
        public void Destroy_BeforeDelay_CancelsCoroutine()
        {
            EntitySystem entitySystem = new EntitySystem();
            var entity = entitySystem.CreateEntity<TestEntity>();

            entity.DestroyAfter(TimeSpan.FromSeconds(10));
            // Manually destroy before delay expires
            entity.Destroy();

            Assert.True(entity.Destroyed);

            // Coroutines should be cleaned up in OnDestroy, no double-destroy
            var gameTime = new GameTime(TimeSpan.Zero, TimeSpan.FromMilliseconds(1000));
            CoroutineManager.Update(gameTime);

            Assert.False(entity.GetActive());
        }

        [Fact]
        public void RespawnAt_ConfiguresPendingRespawn()
        {
            EntitySystem entitySystem = new EntitySystem();
            var entity = entitySystem.CreateEntity<TestEntity>();
            var position = new Vector2(100, 200);

            entity.RespawnAt(position, TimeSpan.FromSeconds(5));

            Assert.True(entity.HasPendingRespawn);
        }

        [Fact]
        public void RespawnAt_ThrowsOnNegativeDelay()
        {
            EntitySystem entitySystem = new EntitySystem();
            var entity = entitySystem.CreateEntity<TestEntity>();

            Assert.Throws<ArgumentOutOfRangeException>(() =>
                entity.RespawnAt(Vector2.Zero, TimeSpan.FromMilliseconds(-1)));
        }

        [Fact]
        public void CancelRespawnAt_CancelsPendingRespawn()
        {
            EntitySystem entitySystem = new EntitySystem();
            var entity = entitySystem.CreateEntity<TestEntity>();

            entity.RespawnAt(Vector2.Zero, TimeSpan.FromSeconds(5));
            bool cancelled = entity.CancelRespawnAt();

            Assert.True(cancelled);
            Assert.False(entity.HasPendingRespawn);
        }

        [Fact]
        public void CancelRespawnAt_ReturnsFalseWhenNothingPending()
        {
            EntitySystem entitySystem = new EntitySystem();
            var entity = entitySystem.CreateEntity<TestEntity>();

            bool cancelled = entity.CancelRespawnAt();

            Assert.False(cancelled);
        }

        [Fact]
        public void RespawnAt_SpawnsNewEntityAfterDestructionAndDelay()
        {
            EntitySystem entitySystem = new EntitySystem();
            var respawnPosition = new Vector2(100, 200);

            // Create first entity with respawn configured
            var originalEntity = entitySystem.CreateEntity<TestEntity>();
            originalEntity.RespawnAt(respawnPosition, TimeSpan.FromMilliseconds(1));

            // Destroy the entity
            originalEntity.Destroy();
            entitySystem.Update(new GameTime(TimeSpan.Zero, TimeSpan.FromMilliseconds(10)));

            // Entity should be gone
            Assert.Empty(entitySystem.GetEntities());

            // Advance coroutine time past respawn delay
            double currentTime = 0.0;
            for (int i = 0; i < 10; i++)
            {
                currentTime += 0.05f;
                var gt = new GameTime(TimeSpan.FromSeconds(currentTime), TimeSpan.FromMilliseconds(50));
                CoroutineManager.Update(gt);
            }

            // Respawned entity should exist
            Assert.Single(entitySystem.GetEntities());
        }

        private class TestEntity : Entity
        {
            public override void Update(GameTime gameTime) { }
            public override void Render(SpriteBatch spriteBatch) { }
        }
    }
}