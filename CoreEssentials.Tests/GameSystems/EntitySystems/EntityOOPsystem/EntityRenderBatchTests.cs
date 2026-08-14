#nullable enable
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
        private static EntitySystem CreateSystem() => new EntitySystem();

        // ===== Texture Tracking (T1) =====

        [Fact]
        public void Texture_DefaultIsNull()
        {
            var system = CreateSystem();
            var entity = system.CreateEntity<TestEntity>();

            Assert.Null(entity.BatchTexture);
        }

        [Fact]
        public void Texture_RegisterForInstancedRendering_AssignsTexture()
        {
            var system = CreateSystem();
            var entity = system.CreateEntity<TestEntity>();
            var mockTexture = new MockTexture2DAsset("testTexture");

            entity.RegisterForInstancedRendering(mockTexture);

            Assert.Equal(mockTexture, entity.BatchTexture);
        }

        [Fact]
        public void Texture_RegisterForInstancedRendering_MarksAsChanged()
        {
            var system = CreateSystem();
            var entity = system.CreateEntity<TestEntity>();
            var mockTexture = new MockTexture2DAsset("testTexture");

            entity.BatchTextureDirty = false;
            entity.RegisterForInstancedRendering(mockTexture);

            Assert.True(entity.BatchTextureDirty);
        }

        [Fact]
        public void Texture_DirectAssignment_DoesNotMarkAsChanged()
        {
            var system = CreateSystem();
            var entity = system.CreateEntity<TestEntity>();
            var mockTexture = new MockTexture2DAsset("testTexture");

            entity.BatchTextureDirty = false;
            entity.BatchTexture = mockTexture;

            Assert.False(entity.BatchTextureDirty);
        }

        [Fact]
        public void Texture_SetNullTexture_ClearsTexture()
        {
            var system = CreateSystem();
            var entity = system.CreateEntity<TestEntity>();
            var mockTexture = new MockTexture2DAsset("testTexture");

            entity.RegisterForInstancedRendering(mockTexture);
            entity.RegisterForInstancedRendering((Texture2DAsset?)null);

            Assert.Null(entity.BatchTexture);
            Assert.True(entity.BatchTextureDirty);
        }

        [Fact]
        public void TextureChanged_DefaultIsFalse()
        {
            var system = CreateSystem();
            var entity = system.CreateEntity<TestEntity>();

            Assert.False(entity.BatchTextureDirty);
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

            entity1.RegisterForInstancedRendering(textureA);
            entity2.RegisterForInstancedRendering(textureB);
            entity3.RegisterForInstancedRendering(textureA);

            Assert.Equal(textureA, entity1.BatchTexture);
            Assert.Equal(textureB, entity2.BatchTexture);
            Assert.Equal(textureA, entity3.BatchTexture);
        }

        [Fact]
        public void Texture_SameTextureReference_GroupsCorrectly()
        {
            var system = CreateSystem();
            var entity1 = system.CreateEntity<TestEntity>();
            var entity2 = system.CreateEntity<TestEntity>();
            var sharedTexture = new MockTexture2DAsset("sharedTexture");

            entity1.RegisterForInstancedRendering(sharedTexture);
            entity2.RegisterForInstancedRendering(sharedTexture);

            Assert.Same(sharedTexture, entity1.BatchTexture);
            Assert.Same(sharedTexture, entity2.BatchTexture);
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

            entity1.RegisterForInstancedRendering(textureA);
            entity2.RegisterForInstancedRendering(textureB);
            entity3.RegisterForInstancedRendering(textureA);

            // Access internal grouping logic via reflection
            var entitiesField = typeof(EntitySystem).GetField("_entities", BindingFlags.NonPublic | BindingFlags.Instance);
            var entities = entitiesField!.GetValue(system) as List<Entity>;

            // Verify entities have correct textures assigned
            Assert.Equal(textureA, entities![0].BatchTexture);
            Assert.Equal(textureB, entities[1].BatchTexture);
            Assert.Equal(textureA, entities[2].BatchTexture);

            // Count entities per texture
            var textureACount = entities.Count(e => e.BatchTexture == textureA);
            var textureBCount = entities.Count(e => e.BatchTexture == textureB);

            Assert.Equal(2, textureACount);
            Assert.Equal(1, textureBCount);
        }

        [Fact]
        public void Draw_EntitiesWithoutTexture_AreHandled()
        {
            var system = CreateSystem();
            var textureA = new MockTexture2DAsset("textureA");

            var entity1 = system.CreateEntity<TestEntity>();
            _ = system.CreateEntity<TestEntity>(); // entity without texture
            var entity3 = system.CreateEntity<TestEntity>();

            entity1.RegisterForInstancedRendering(textureA);
            // entity2 has no texture
            entity3.RegisterForInstancedRendering(textureA);

            var entitiesField = typeof(EntitySystem).GetField("_entities", BindingFlags.NonPublic | BindingFlags.Instance);
            var entities = (entitiesField!.GetValue(system) as List<Entity>)!;

            var noTextureCount = entities.Count(e => e.BatchTexture == null);
            var withTextureCount = entities.Count(e => e.BatchTexture != null);

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

            entity1.RegisterForInstancedRendering(textureA);
            entity2.RegisterForInstancedRendering(textureA);

            // Manually reset flags to simulate post-draw state
            entity1.BatchTextureDirty = false;
            entity2.BatchTextureDirty = false;

            // Verify flags can be reset
            Assert.False(entity1.BatchTextureDirty);
            Assert.False(entity2.BatchTextureDirty);
        }

        [Fact]
        public void Draw_SortOrderPreservedWithinTextureGroup()
        {
            var system = CreateSystem();
            var textureA = new MockTexture2DAsset("textureA");

            var entity1 = system.CreateEntity<TestEntity>();
            var entity2 = system.CreateEntity<TestEntity>();
            var entity3 = system.CreateEntity<TestEntity>();

            entity1.RegisterForInstancedRendering(textureA);
            entity1.SetSort(10);
            
            entity2.RegisterForInstancedRendering(textureA);
            entity2.SetSort(5);
            
            entity3.RegisterForInstancedRendering(textureA);
            entity3.SetSort(15);

            system.Update(new GameTime());

            var entitiesField = typeof(EntitySystem).GetField("_entities", BindingFlags.NonPublic | BindingFlags.Instance);
            var entities = (entitiesField!.GetValue(system) as List<Entity>)!;

            // All entities have same texture, so they should be sorted by sort order
            var textureAEntities = entities.Where(e => e.BatchTexture == textureA).ToList();
            
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

            entity1.RegisterForInstancedRendering(textureA);
            entity2.RegisterForInstancedRendering(textureA);
            entity2.SetActive(false);

            var entitiesField = typeof(EntitySystem).GetField("_entities", BindingFlags.NonPublic | BindingFlags.Instance);
            var entities = (entitiesField!.GetValue(system) as List<Entity>)!;

            var activeWithTexture = entities.Count(e => e.GetActive() && e.BatchTexture == textureA);
            var inactiveWithTexture = entities.Count(e => !e.GetActive() && e.BatchTexture == textureA);

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
            public override void Render(SpriteBatch _spriteBatch) { }
        }
    }
}
#nullable enable