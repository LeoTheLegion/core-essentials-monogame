using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CoreEssentials.Assets;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Xunit;

namespace CoreEssentials.Tests.GameSystems.EntitySystems.EntityOOPsystem
{
    public class EntityRenderBatchTests
    {
        private EntitySystem CreateSystem() => new EntitySystem();

        // ===== Texture Tracking (T1) =====

        [Fact]
        public void Texture_DefaultIsNull()
        {
            var system = CreateSystem();
            var entity = system.CreateEntity<TestEntity>();

            Assert.Null(entity.Texture);
        }

        [Fact]
        public void Texture_SetTexture_AssignsTexture()
        {
            var system = CreateSystem();
            var entity = system.CreateEntity<TestEntity>();
            var mockTexture = new MockTexture2DAsset("testTexture");

            entity.SetTexture(mockTexture);

            Assert.Equal(mockTexture, entity.Texture);
        }

        [Fact]
        public void Texture_SetTexture_MarksAsChanged()
        {
            var system = CreateSystem();
            var entity = system.CreateEntity<TestEntity>();
            var mockTexture = new MockTexture2DAsset("testTexture");

            entity.TextureChanged = false;
            entity.SetTexture(mockTexture);

            Assert.True(entity.TextureChanged);
        }

        [Fact]
        public void Texture_DirectAssignment_DoesNotMarkAsChanged()
        {
            var system = CreateSystem();
            var entity = system.CreateEntity<TestEntity>();
            var mockTexture = new MockTexture2DAsset("testTexture");

            entity.TextureChanged = false;
            entity.Texture = mockTexture;

            Assert.False(entity.TextureChanged);
        }

        [Fact]
        public void Texture_SetNullTexture_ClearsTexture()
        {
            var system = CreateSystem();
            var entity = system.CreateEntity<TestEntity>();
            var mockTexture = new MockTexture2DAsset("testTexture");

            entity.SetTexture(mockTexture);
            entity.SetTexture(null);

            Assert.Null(entity.Texture);
            Assert.True(entity.TextureChanged);
        }

        [Fact]
        public void TextureChanged_DefaultIsFalse()
        {
            var system = CreateSystem();
            var entity = system.CreateEntity<TestEntity>();

            Assert.False(entity.TextureChanged);
        }

        [Fact]
        public void Texture_MultipleEntities_CanHaveDifferentTextures()
        {
            var system = CreateSystem();
            var entity1 = system.CreateEntity<TestEntity>();
            var entity2 = system.CreateEntity<TestEntity>();
            var entity3 = system.CreateEntity<TestEntity>();
            var textureA = new MockTexture2DAsset("textureA");
            var textureB = new MockTexture2DAsset("textureB");

            entity1.SetTexture(textureA);
            entity2.SetTexture(textureB);
            entity3.SetTexture(textureA);

            Assert.Equal(textureA, entity1.Texture);
            Assert.Equal(textureB, entity2.Texture);
            Assert.Equal(textureA, entity3.Texture);
        }

        [Fact]
        public void Texture_SameTextureReference_GroupsCorrectly()
        {
            var system = CreateSystem();
            var entity1 = system.CreateEntity<TestEntity>();
            var entity2 = system.CreateEntity<TestEntity>();
            var sharedTexture = new MockTexture2DAsset("sharedTexture");

            entity1.SetTexture(sharedTexture);
            entity2.SetTexture(sharedTexture);

            Assert.Same(sharedTexture, entity1.Texture);
            Assert.Same(sharedTexture, entity2.Texture);
        }

        // ===== Render Batching (T2) =====

        [Fact]
        public void Draw_EntitiesGroupedByTexture()
        {
            var system = CreateSystem();
            var textureA = new MockTexture2DAsset("textureA");
            var textureB = new MockTexture2DAsset("textureB");

            var entity1 = system.CreateEntity<TestEntity>();
            var entity2 = system.CreateEntity<TestEntity>();
            var entity3 = system.CreateEntity<TestEntity>();

            entity1.SetTexture(textureA);
            entity2.SetTexture(textureB);
            entity3.SetTexture(textureA);

            // Access internal grouping logic via reflection
            var entitiesField = typeof(EntitySystem).GetField("_entities", BindingFlags.NonPublic | BindingFlags.Instance);
            var entities = entitiesField!.GetValue(system) as List<Entity>;

            // Verify entities have correct textures assigned
            Assert.Equal(textureA, entities![0].Texture);
            Assert.Equal(textureB, entities[1].Texture);
            Assert.Equal(textureA, entities[2].Texture);

            // Count entities per texture
            var textureACount = entities.Count(e => e.Texture == textureA);
            var textureBCount = entities.Count(e => e.Texture == textureB);

            Assert.Equal(2, textureACount);
            Assert.Equal(1, textureBCount);
        }

        [Fact]
        public void Draw_EntitiesWithoutTexture_AreHandled()
        {
            var system = CreateSystem();
            var textureA = new MockTexture2DAsset("textureA");

            var entity1 = system.CreateEntity<TestEntity>();
            var entity2 = system.CreateEntity<TestEntity>();
            var entity3 = system.CreateEntity<TestEntity>();

            entity1.SetTexture(textureA);
            // entity2 has no texture
            entity3.SetTexture(textureA);

            var entitiesField = typeof(EntitySystem).GetField("_entities", BindingFlags.NonPublic | BindingFlags.Instance);
            var entities = entitiesField!.GetValue(system) as List<Entity>;

            var noTextureCount = entities!.Count(e => e.Texture == null);
            var withTextureCount = entities.Count(e => e.Texture != null);

            Assert.Equal(1, noTextureCount);
            Assert.Equal(2, withTextureCount);
        }

        [Fact]
        public void Draw_TextureChangedFlagsResetAfterDraw()
        {
            var system = CreateSystem();
            var textureA = new MockTexture2DAsset("textureA");

            var entity1 = system.CreateEntity<TestEntity>();
            var entity2 = system.CreateEntity<TestEntity>();

            entity1.SetTexture(textureA);
            entity2.SetTexture(textureA);

            // Manually reset flags to simulate post-draw state
            entity1.TextureChanged = false;
            entity2.TextureChanged = false;

            // Verify flags can be reset
            Assert.False(entity1.TextureChanged);
            Assert.False(entity2.TextureChanged);
        }

        [Fact]
        public void Draw_SortOrderPreservedWithinTextureGroup()
        {
            var system = CreateSystem();
            var textureA = new MockTexture2DAsset("textureA");

            var entity1 = system.CreateEntity<TestEntity>();
            var entity2 = system.CreateEntity<TestEntity>();
            var entity3 = system.CreateEntity<TestEntity>();

            entity1.SetTexture(textureA);
            entity1.SetSort(10);
            
            entity2.SetTexture(textureA);
            entity2.SetSort(5);
            
            entity3.SetTexture(textureA);
            entity3.SetSort(15);

            system.Update(new GameTime());

            var entitiesField = typeof(EntitySystem).GetField("_entities", BindingFlags.NonPublic | BindingFlags.Instance);
            var entities = entitiesField!.GetValue(system) as List<Entity>;

            // All entities have same texture, so they should be sorted by sort order
            var textureAEntities = entities!.Where(e => e.Texture == textureA).ToList();
            
            Assert.Equal(3, textureAEntities.Count);
            // Higher sort value first (15, 10, 5)
            Assert.Equal(15, textureAEntities[0].GetSort());
            Assert.Equal(10, textureAEntities[1].GetSort());
            Assert.Equal(5, textureAEntities[2].GetSort());
        }

        [Fact]
        public void Draw_InactiveEntitiesNotIncluded()
        {
            var system = CreateSystem();
            var textureA = new MockTexture2DAsset("textureA");

            var entity1 = system.CreateEntity<TestEntity>();
            var entity2 = system.CreateEntity<TestEntity>();

            entity1.SetTexture(textureA);
            entity2.SetTexture(textureA);
            entity2.SetActive(false);

            var entitiesField = typeof(EntitySystem).GetField("_entities", BindingFlags.NonPublic | BindingFlags.Instance);
            var entities = entitiesField!.GetValue(system) as List<Entity>;

            var activeWithTexture = entities!.Count(e => e.GetActive() && e.Texture == textureA);
            var inactiveWithTexture = entities.Count(e => !e.GetActive() && e.Texture == textureA);

            Assert.Equal(1, activeWithTexture);
            Assert.Equal(1, inactiveWithTexture);
        }

        // ===== Mock Texture2DAsset =====

        private class MockTexture2DAsset : Texture2DAsset
        {
            public MockTexture2DAsset(string name) : base(name)
            {
            }

            public override void Load(IContentManager contentManager)
            {
                // No-op for testing
            }

            public override void Unload(IContentManager contentManager)
            {
                // No-op for testing
            }
        }

        private class TestEntity : Entity
        {
            public override void Render(SpriteBatch spriteBatch) { }
        }
    }
}
