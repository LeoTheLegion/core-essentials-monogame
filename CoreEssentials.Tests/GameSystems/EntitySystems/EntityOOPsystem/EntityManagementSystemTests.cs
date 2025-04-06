using Xunit;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPsystem;
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
            var entity = new TestEntity();
            Assert.Contains(entity, EntityManagementSystem.GetEntities());

            entity.Destroy();
            Assert.DoesNotContain(entity, EntityManagementSystem.GetEntities());
        }

        [Fact]
        public void SortsEntitiesCorrectly()
        {
            var entity1 = new TestEntity().SetSort(1);
            var entity2 = new TestEntity().SetSort(2);

            EntityManagementSystem.SortEntities();
            List<Entity> sortedEntities = EntityManagementSystem.GetEntities();
            
            // check entity 2 is found before entity 1
            Assert.True(sortedEntities.IndexOf(entity2) < sortedEntities.IndexOf(entity1));
        }

        [Fact]
        public void UpdatesEntitiesCorrectly()
        {
            var entity = new TestEntity();
            EntityManagementSystem.Register(entity);

            var gameTime = new GameTime();
            EntityManagementSystem.Update(ref gameTime);

            Assert.True(entity.Updated);
        }

        private class TestEntity : Entity
        {
            public bool Updated { get; private set; }

            public override void LoadAssets() { }

            public override void Update(ref GameTime gameTime)
            {
                Updated = true;
            }

            public override void Render(ref SpriteBatch _spriteBatch) { }
        }
    }
}