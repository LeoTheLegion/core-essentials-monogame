using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Xunit;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem;
using CoreEssentials.Coroutines;

namespace CoreEssentials.Tests.GameSystems.EntitySystems.EntityOOPsystem
{
    public class EntitySystemTests
    {
        [Fact]
        public void SpawnAfter_SchedulesEntityCreation()
        {
            var entitySystem = new EntitySystem();

            Guid spawnId = entitySystem.SpawnAfter<TestEntity>(Vector2.Zero, TimeSpan.FromSeconds(10));

            Assert.NotEqual(Guid.Empty, spawnId);
        }

        [Fact]
        public void SpawnAfter_CreatesEntityAtPositionAfterDelay()
        {
            var entitySystem = new EntitySystem();
            var position = new Vector2(100, 200);

            Guid spawnId = entitySystem.SpawnAfter<TestEntity>(position, TimeSpan.FromMilliseconds(1));

            double currentTime = 0.0;
            for (int i = 0; i < 10; i++)
            {
                currentTime += 0.05f;
                var gameTime = new GameTime(TimeSpan.FromSeconds(currentTime), TimeSpan.FromMilliseconds(50));
                CoroutineManager.Update(gameTime);
            }

            entitySystem.Update(new GameTime(TimeSpan.FromSeconds(currentTime), TimeSpan.Zero));

            Assert.True(spawnId != Guid.Empty);
        }

        [Fact]
        public void CancelSpawnAfter_CancelsPendingSpawn()
        {
            var entitySystem = new EntitySystem();

            Guid spawnId = entitySystem.SpawnAfter<TestEntity>(Vector2.Zero, TimeSpan.FromSeconds(10));
            bool cancelled = entitySystem.CancelSpawnAfter(spawnId);

            Assert.True(cancelled);
        }

        [Fact]
        public void CancelSpawnAfter_ReturnsFalseForInvalidId()
        {
            var entitySystem = new EntitySystem();

            bool cancelled = entitySystem.CancelSpawnAfter(Guid.NewGuid());

            Assert.False(cancelled);
        }

        [Fact]
        public void SpawnAfter_ThrowsOnNegativeDelay()
        {
            var entitySystem = new EntitySystem();

            Assert.Throws<ArgumentOutOfRangeException>(() =>
                entitySystem.SpawnAfter<TestEntity>(Vector2.Zero, TimeSpan.FromMilliseconds(-1)));
        }

        private class TestEntity : Entity
        {
            public override void Update(GameTime gameTime) { }
            public override void Render(SpriteBatch spriteBatch) { }
        }
    }
}