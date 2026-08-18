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
    /// <summary>
    /// Tests for z-order render layers (Sprint 20).
    /// Verifies that entities are grouped by z-layer first, then by texture within
    /// each layer, and that layers are rendered back-to-front (low to high z-layer).
    /// </summary>
    public class EntityZOrderTests
    {
        private static EntitySystem CreateSystem() => new EntitySystem();

        // ===== ZLayer Property (T1) =====

        [Fact]
        public void ZLayer_DefaultIsZero()
        {
            var system = CreateSystem();
            var entity = system.CreateEntity<TestEntity>();

            Assert.Equal(0, entity.GetZLayer());
            Assert.Equal(0, entity.ZLayer);
        }

        [Fact]
        public void ZLayer_Property_SetAndGet()
        {
            var system = CreateSystem();
            var entity = system.CreateEntity<TestEntity>();

            entity.ZLayer = 5;

            Assert.Equal(5, entity.GetZLayer());
            Assert.Equal(5, entity.ZLayer);
        }

        [Fact]
        public void ZLayer_SetZLayer_ReturnsEntity()
        {
            var system = CreateSystem();
            var entity = system.CreateEntity<TestEntity>();

            var result = entity.SetZLayer(3);

            Assert.Same(entity, result);
            Assert.Equal(3, entity.GetZLayer());
        }

        [Fact]
        public void ZLayer_SupportsNegativeValues()
        {
            var system = CreateSystem();
            var entity = system.CreateEntity<TestEntity>();

            entity.SetZLayer(-1);

            Assert.Equal(-1, entity.GetZLayer());
        }

        // ===== Z-Aware Grouping (T2) =====

        [Fact]
        public void Group_ByZLayer_EntitiesInDifferentLayersAreSeparated()
        {
            var system = CreateSystem();
            var textureA = new MockTexture2DAsset("textureA");

            var entity1 = system.CreateEntity<TestEntity>();
            var entity2 = system.CreateEntity<TestEntity>();

            entity1.RegisterForInstancedRendering(textureA);
            entity1.SetZLayer(0);
            entity2.RegisterForInstancedRendering(textureA);
            entity2.SetZLayer(1);

            var (zLayers, noTexture) = InvokeGroupEntitiesByZLayer(system);

            Assert.Empty(noTexture);
            Assert.Equal(2, zLayers.Count);

            // Layers in ascending order (back to front)
            Assert.Equal(0, zLayers[0].Key);
            Assert.Equal(1, zLayers[1].Key);

            Assert.Single(zLayers[0].Value[textureA]);
            Assert.Single(zLayers[1].Value[textureA]);
        }

        [Fact]
        public void Group_ByZLayer_SameLayer_BatchedByTexture()
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
            entity1.SetZLayer(2);
            entity2.SetZLayer(2);
            entity3.SetZLayer(2);

            var (zLayers, noTexture) = InvokeGroupEntitiesByZLayer(system);

            Assert.Empty(noTexture);
            Assert.Single(zLayers);
            Assert.Equal(2, zLayers[0].Key);

            var layerTextures = zLayers[0].Value;
            Assert.Equal(2, layerTextures[textureA].Count);
            Assert.Single(layerTextures[textureB]);
        }

        [Fact]
        public void Group_ByZLayer_InterleavedTextures_GroupedByLayerThenTexture()
        {
            // Reproduces the Sprint 20 motivating example:
            //   A1 (Texture A, layer 0) -> behind
            //   B1 (Texture B, layer 1) -> middle
            //   A2 (Texture A, layer 2) -> in front
            var system = CreateSystem();
            var textureA = new MockTexture2DAsset("textureA");
            var textureB = new MockTexture2DAsset("textureB");

            var a1 = system.CreateEntity<TestEntity>();
            var b1 = system.CreateEntity<TestEntity>();
            var a2 = system.CreateEntity<TestEntity>();

            a1.RegisterForInstancedRendering(textureA);
            a1.SetZLayer(0);
            b1.RegisterForInstancedRendering(textureB);
            b1.SetZLayer(1);
            a2.RegisterForInstancedRendering(textureA);
            a2.SetZLayer(2);

            var (zLayers, noTexture) = InvokeGroupEntitiesByZLayer(system);

            Assert.Empty(noTexture);
            Assert.Equal(3, zLayers.Count);

            // Layer 0: only A1 (texture A)
            Assert.Equal(0, zLayers[0].Key);
            Assert.Same(a1, zLayers[0].Value[textureA].Single());

            // Layer 1: only B1 (texture B)
            Assert.Equal(1, zLayers[1].Key);
            Assert.Same(b1, zLayers[1].Value[textureB].Single());

            // Layer 2: only A2 (texture A)
            Assert.Equal(2, zLayers[2].Key);
            Assert.Same(a2, zLayers[2].Value[textureA].Single());
        }

        [Fact]
        public void Group_ByZLayer_LayersReturnedInAscendingOrder()
        {
            var system = CreateSystem();
            var textureA = new MockTexture2DAsset("textureA");

            // Insert in non-sorted order to verify the grouping sorts by layer.
            var e3 = system.CreateEntity<TestEntity>();
            var e1 = system.CreateEntity<TestEntity>();
            var e2 = system.CreateEntity<TestEntity>();

            e3.RegisterForInstancedRendering(textureA);
            e3.SetZLayer(30);
            e1.RegisterForInstancedRendering(textureA);
            e1.SetZLayer(10);
            e2.RegisterForInstancedRendering(textureA);
            e2.SetZLayer(20);

            var (zLayers, noTexture) = InvokeGroupEntitiesByZLayer(system);

            Assert.Empty(noTexture);
            Assert.Equal(3, zLayers.Count);
            Assert.Equal(10, zLayers[0].Key);
            Assert.Equal(20, zLayers[1].Key);
            Assert.Equal(30, zLayers[2].Key);
        }

        [Fact]
        public void Group_ByZLayer_EntitiesWithoutTexture_AreExcludedFromLayers()
        {
            var system = CreateSystem();
            var textureA = new MockTexture2DAsset("textureA");

            var entity1 = system.CreateEntity<TestEntity>();
            var entity2 = system.CreateEntity<TestEntity>();

            entity1.RegisterForInstancedRendering(textureA);
            entity1.SetZLayer(0);
            // entity2 has no texture

            var (zLayers, noTexture) = InvokeGroupEntitiesByZLayer(system);

            Assert.Single(noTexture);
            Assert.Same(entity2, noTexture[0]);
            Assert.Single(zLayers);
            Assert.Equal(0, zLayers[0].Key);
            Assert.Single(zLayers[0].Value[textureA]);
        }

        [Fact]
        public void Group_ByZLayer_InactiveEntitiesNotIncluded()
        {
            var system = CreateSystem();
            var textureA = new MockTexture2DAsset("textureA");

            var entity1 = system.CreateEntity<TestEntity>();
            var entity2 = system.CreateEntity<TestEntity>();

            entity1.RegisterForInstancedRendering(textureA);
            entity1.SetZLayer(0);
            entity2.RegisterForInstancedRendering(textureA);
            entity2.SetZLayer(1);
            entity2.SetActive(false);

            var (zLayers, noTexture) = InvokeGroupEntitiesByZLayer(system);

            Assert.Empty(noTexture);
            Assert.Single(zLayers);
            Assert.Equal(0, zLayers[0].Key);
            Assert.Single(zLayers[0].Value[textureA]);
        }

        [Fact]
        public void Group_ByZLayer_SortOrderPreservedWithinLayerTexture()
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

            // All in the same z-layer (default 0)
            system.Update(new GameTime());

            var (zLayers, noTexture) = InvokeGroupEntitiesByZLayer(system);

            Assert.Empty(noTexture);
            Assert.Single(zLayers);
            Assert.Equal(0, zLayers[0].Key);

            // Within the same layer + texture, sort order is preserved (higher first)
            var ordered = zLayers[0].Value[textureA];
            Assert.Equal(3, ordered.Count);
            Assert.Equal(15, ordered[0].GetSort());
            Assert.Equal(10, ordered[1].GetSort());
            Assert.Equal(5, ordered[2].GetSort());
        }

        [Fact]
        public void Group_ByZLayer_BackwardCompatible_DefaultLayerZero()
        {
            // Entities that never set a z-layer all default to layer 0,
            // preserving the old texture-only batching behavior.
            var system = CreateSystem();
            var textureA = new MockTexture2DAsset("textureA");
            var textureB = new MockTexture2DAsset("textureB");

            var entity1 = system.CreateEntity<TestEntity>();
            var entity2 = system.CreateEntity<TestEntity>();

            entity1.RegisterForInstancedRendering(textureA);
            entity2.RegisterForInstancedRendering(textureB);

            var (zLayers, noTexture) = InvokeGroupEntitiesByZLayer(system);

            Assert.Empty(noTexture);
            Assert.Single(zLayers);
            Assert.Equal(0, zLayers[0].Key);
            Assert.Single(zLayers[0].Value[textureA]);
            Assert.Single(zLayers[0].Value[textureB]);
        }

        // ===== Helpers =====

        private static (List<KeyValuePair<int, Dictionary<Texture2DAsset, List<Entity>>>> zLayers, List<Entity> noTexture)
            InvokeGroupEntitiesByZLayer(EntitySystem system)
        {
            var method = typeof(EntitySystem).GetMethod(
                "GroupEntitiesByZLayer",
                BindingFlags.NonPublic | BindingFlags.Instance);

            var result = method!.Invoke(system, null)!;
            var tuple = (ValueTuple<
                List<KeyValuePair<int, Dictionary<Texture2DAsset, List<Entity>>>>,
                List<Entity>>)result;

            return (tuple.Item1, tuple.Item2);
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
