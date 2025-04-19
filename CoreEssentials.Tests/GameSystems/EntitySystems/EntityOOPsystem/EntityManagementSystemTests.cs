using Xunit;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace CoreEssentials.Tests.GameSystems.EntitySystems.EntityOOPsystem
{
    public class EntityManagementSystemTests
    {
        [Fact]
        public void RegistersAndUnregistersEntitiesCorrectly()
        {
            EntitySystem entityManagementSystem = new EntitySystem();
            var entity = entityManagementSystem.CreateEntity<TestEntity>();
            Assert.Contains(entity, entityManagementSystem.GetEntities());

            entity.Destroy();
            entityManagementSystem.Update(new GameTime());
            // After the entity is destroyed, it should no longer be in the list of entities
            Assert.DoesNotContain(entity, entityManagementSystem.GetEntities());
        }

        [Fact]
        public void SortsEntitiesCorrectly()
        {
            EntitySystem entityManagementSystem = new EntitySystem();
            var entity1 = entityManagementSystem.CreateEntity<TestEntity>().SetSort(1);
            var entity2 = entityManagementSystem.CreateEntity<TestEntity>().SetSort(2);

            entityManagementSystem.SortEntities();
            List<Entity> sortedEntities = entityManagementSystem.GetEntities();
            
            // check entity 2 is found before entity 1
            Assert.True(sortedEntities.IndexOf(entity2) < sortedEntities.IndexOf(entity1));
        }

        [Fact]
        public void UpdatesEntitiesCorrectly()
        {
            EntitySystem entityManagementSystem = new EntitySystem();
            var entity = entityManagementSystem.CreateEntity<TestEntity>();

            var gameTime = new GameTime();
            entityManagementSystem.Update(gameTime);

            Assert.True(entity.Updated);
        }

        private class TestEntity : Entity
        {
            public bool Updated { get; private set; }

            public override void Update(GameTime gameTime)
            {
                Updated = true;
            }

            public override void Render(SpriteBatch _spriteBatch) { }
        }
    }
}