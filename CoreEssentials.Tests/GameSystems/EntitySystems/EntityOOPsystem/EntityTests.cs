using Xunit;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem;
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

        private class TestEntity : Entity
        {
            public override void Update(GameTime gameTime) { }
            public override void Render(SpriteBatch spriteBatch) { }
        }
    }
}