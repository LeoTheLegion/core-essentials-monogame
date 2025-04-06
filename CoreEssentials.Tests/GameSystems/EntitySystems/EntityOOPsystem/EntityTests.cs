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
            var entity = new TestEntity();
            entity.SetActive(false);
            Assert.False(entity.GetActive());
        }

        [Fact]
        public void Entity_SetsSortValue()
        {
            var entity = new TestEntity();
            entity.SetSort(5);
            Assert.Equal(5, entity.GetSort());
        }

        private class TestEntity : Entity
        {
            public override void LoadAssets() { }
            public override void Update(ref GameTime gameTime) { }
            public override void Render(ref SpriteBatch spriteBatch) { }
        }
    }
}