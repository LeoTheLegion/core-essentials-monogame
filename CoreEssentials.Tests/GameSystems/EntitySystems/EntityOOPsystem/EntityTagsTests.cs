using System;
using System.Collections.Generic;
using Xunit;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem;

namespace CoreEssentials.Tests.GameSystems.EntitySystems.EntityOOPsystem
{
    public class EntityTagsTests
    {
        private EntitySystem CreateEntitySystem() => new EntitySystem();
        
        private TestEntity CreateTaggedEntity(EntitySystem system, params string[] tags)
        {
            var entity = system.CreateEntity<TestEntity>();
            foreach (var tag in tags)
                entity.SetTag(tag);
            return entity;
        }

        // T1 Tests - Entity tag management
        [Fact]
        public void SetTag_AddsTagToEntity()
        {
            var system = CreateEntitySystem();
            var entity = system.CreateEntity<TestEntity>();
            
            entity.SetTag("enemy");
            
            Assert.Contains("enemy", entity.Tags);
        }

        [Fact]
        public void SetTag_IsCaseInsensitive()
        {
            var system = CreateEntitySystem();
            var entity = system.CreateEntity<TestEntity>();
            
            entity.SetTag("Enemy");
            entity.SetTag("enemy");
            
            Assert.Single(entity.Tags);
        }

        [Fact]
        public void SetTag_ThrowsOnNullTag()
        {
            var system = CreateEntitySystem();
            var entity = system.CreateEntity<TestEntity>();
            
            Assert.Throws<ArgumentNullException>(() => entity.SetTag(null));
        }

        [Fact]
        public void SetTag_ThrowsOnWhitespaceTag()
        {
            var system = CreateEntitySystem();
            var entity = system.CreateEntity<TestEntity>();
            
            Assert.Throws<ArgumentNullException>(() => entity.SetTag("   "));
        }

        [Fact]
        public void RemoveTag_RemovesTagFromEntity()
        {
            var system = CreateEntitySystem();
            var entity = system.CreateEntity<TestEntity>();
            entity.SetTag("enemy");
            
            var result = entity.RemoveTag("enemy");
            
            Assert.True(result);
            Assert.DoesNotContain("enemy", entity.Tags);
        }

        [Fact]
        public void RemoveTag_ReturnsFalseWhenTagNotFound()
        {
            var system = CreateEntitySystem();
            var entity = system.CreateEntity<TestEntity>();
            
            var result = entity.RemoveTag("nonexistent");
            
            Assert.False(result);
        }

        [Fact]
        public void RemoveTag_ReturnsFalseForNullTag()
        {
            var system = CreateEntitySystem();
            var entity = system.CreateEntity<TestEntity>();
            
            var result = entity.RemoveTag(null);
            
            Assert.False(result);
        }

        [Fact]
        public void HasTag_ReturnsTrueWhenEntityHasTag()
        {
            var system = CreateEntitySystem();
            var entity = system.CreateEntity<TestEntity>();
            entity.SetTag("enemy");
            
            Assert.True(entity.HasTag("enemy"));
        }

        [Fact]
        public void HasTag_ReturnsFalseWhenEntityDoesntHaveTag()
        {
            var system = CreateEntitySystem();
            var entity = system.CreateEntity<TestEntity>();
            
            Assert.False(entity.HasTag("enemy"));
        }

        [Fact]
        public void HasTag_IsCaseInsensitive()
        {
            var system = CreateEntitySystem();
            var entity = system.CreateEntity<TestEntity>();
            entity.SetTag("Enemy");
            
            Assert.True(entity.HasTag("enemy"));
            Assert.True(entity.HasTag("ENEMY"));
        }

        [Fact]
        public void HasTag_ReturnsFalseForNullTag()
        {
            var system = CreateEntitySystem();
            var entity = system.CreateEntity<TestEntity>();
            
            Assert.False(entity.HasTag(null));
        }

        // T2 Tests - EntitySystem tag lookup
        [Fact]
        public void GetEntitiesByTag_ReturnsEntitiesWithTag()
        {
            var system = CreateEntitySystem();
            var enemy1 = CreateTaggedEntity(system, "enemy");
            var enemy2 = CreateTaggedEntity(system, "enemy");
            var player = CreateTaggedEntity(system, "player");
            
            var enemies = system.GetEntitiesByTag("enemy");
            
            Assert.Contains(enemy1, enemies);
            Assert.Contains(enemy2, enemies);
            Assert.DoesNotContain(player, enemies);
            Assert.Equal(2, enemies.Count);
        }

        [Fact]
        public void GetEntitiesByTag_IsCaseInsensitive()
        {
            var system = CreateEntitySystem();
            var entity = CreateTaggedEntity(system, "Enemy");
            
            var entities = system.GetEntitiesByTag("enemy");
            
            Assert.Contains(entity, entities);
        }

        [Fact]
        public void GetEntitiesByTag_ReturnsEmptyListWhenNoEntitiesHaveTag()
        {
            var system = CreateEntitySystem();
            system.CreateEntity<TestEntity>();
            
            var entities = system.GetEntitiesByTag("nonexistent");
            
            Assert.Empty(entities);
        }

        [Fact]
        public void GetEntitiesByTag_ReturnsEmptyListForNullTag()
        {
            var system = CreateEntitySystem();
            
            var entities = system.GetEntitiesByTag(null);
            
            Assert.Empty(entities);
        }

        [Fact]
        public void FindByTag_ReturnsFirstEntityWithTag()
        {
            var system = CreateEntitySystem();
            var first = CreateTaggedEntity(system, "enemy");
            var second = CreateTaggedEntity(system, "enemy");
            
            var found = system.FindByTag("enemy");
            
            Assert.Equal(first, found);
        }

        [Fact]
        public void FindByTag_ReturnsNullWhenNoEntitiesHaveTag()
        {
            var system = CreateEntitySystem();
            system.CreateEntity<TestEntity>();
            
            var found = system.FindByTag("nonexistent");
            
            Assert.Null(found);
        }

        [Fact]
        public void FindByTag_ReturnsNullForNullTag()
        {
            var system = CreateEntitySystem();
            
            var found = system.FindByTag(null);
            
            Assert.Null(found);
        }

        [Fact]
        public void TagIndex_UpdatesWhenEntityDestroyed()
        {
            var system = CreateEntitySystem();
            var entity = CreateTaggedEntity(system, "enemy");
            
            entity.Destroy();
            system.Update(null); // Process destruction
            
            var enemies = system.GetEntitiesByTag("enemy");
            
            Assert.DoesNotContain(entity, enemies);
        }

        [Fact]
        public void Entity_StartsWithEmptyTags()
        {
            var system = CreateEntitySystem();
            var entity = system.CreateEntity<TestEntity>();
            
            Assert.Empty(entity.Tags);
        }

        private class TestEntity : Entity
        {
            public override void Update(GameTime gameTime) { }
            public override void Render(SpriteBatch spriteBatch) { }
        }
    }
}
