#nullable enable
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using System.Xml.Linq;
using Xunit;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Serialization;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Components.BuiltIn;

namespace CoreEssentials.Tests.GameSystems.EntitySystems.EntityOOPsystem.Serialization
{
    public class BallLoadNullRefTest
    {
        public class TestBallEntity : Entity
        {
            public override void OnStart()
            {
                base.OnStart();
                // Simulate Ball.OnStart - create a sprite component with no asset loaded
                // (the real path uses AssetManager.LoadAsset, which we skip to keep the Sprite null).
                AddComponent(new SpriteComponent());
            }
        }

        private static EntitySystem NewSystemWithBallPrefab()
        {
            var system = new EntitySystem();
            system.RegisterPrefab("ball", EntityPrefabLoader.LoadFromXml(
                "<Prefab Type=\"TestBallEntity\"><Components>" +
                "<Component Type=\"TransformSaveComponent\" /></Components></Prefab>"));
            return system;
        }

        [Fact]
        public void LoadState_BallEntity_WithSpriteComponent_SpriteIsNull_DoesNotThrow()
        {
            // Arrange
            var system = NewSystemWithBallPrefab();
            var entity = system.Instantiate("ball", new Vector2(100, 200));
            entity.Rotation = 0.5f;
            entity.SetId("test_ball_1");
            
            // Save state
            var tempFile = Path.GetTempFileName();
            try
            {
                GameStateSerializer.SaveState(system, tempFile);
                
                // Act - Load into new system
                var newSystem = NewSystemWithBallPrefab();
                GameStateSerializer.LoadState(newSystem, tempFile);
                
                // Assert - Should load without throwing, and entity should exist
                var entities = newSystem.GetEntities();
                Assert.Single(entities);
                var loaded = entities.First();
                Assert.Equal("test_ball_1", loaded.Id);
                Assert.Equal(new Vector2(100, 200), loaded.Position);
                Assert.Equal(0.5f, loaded.Rotation, 0.01f);
                
                // Verify SpriteComponent exists but Sprite is null (OnStart created it without an asset)
                var spriteComp = loaded.GetComponent<SpriteComponent>();
                Assert.NotNull(spriteComp);
                Assert.Null(spriteComp!.Sprite);
            }
            finally
            {
                if (File.Exists(tempFile))
                    File.Delete(tempFile);
            }
        }

        [Fact]
        public void LoadState_BallEntity_InstantiatedViaPrefab_CallsOnStart()
        {
            // Arrange
            var system = NewSystemWithBallPrefab();
            var entity = system.Instantiate("ball", new Vector2(50, 60));
            entity.SetId("ball_test");
            
            var tempFile = Path.GetTempFileName();
            try
            {
                GameStateSerializer.SaveState(system, tempFile);
                
                // Act
                var newSystem = NewSystemWithBallPrefab();
                GameStateSerializer.LoadState(newSystem, tempFile);
                
                // Assert
                var entities = newSystem.GetEntities();
                Assert.Single(entities);
                var loaded = entities.First();
                
                // OnStart should have run during instantiation (the SpriteComponent exists and is owned)
                var spriteComp = loaded.GetComponent<SpriteComponent>();
                Assert.NotNull(spriteComp);
                Assert.Same(loaded, spriteComp!.Owner);
                
                // Position should be restored
                Assert.Equal(new Vector2(50, 60), loaded.Position);
            }
            finally
            {
                if (File.Exists(tempFile))
                    File.Delete(tempFile);
            }
        }
    }
}
#nullable enable