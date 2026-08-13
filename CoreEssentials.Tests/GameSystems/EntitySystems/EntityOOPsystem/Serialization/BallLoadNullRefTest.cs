using System;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
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
            public SpriteComponent? SpriteComp { get; private set; }

            public override void OnStart()
            {
                base.OnStart();
                // Simulate Ball.OnStart - create sprite component with asset load
                // In real scenario, AssetManager.LoadAsset would return a Sprite
                // For test, we create a component without Sprite set to simulate load failure
                SpriteComp = new SpriteComponent();
                AddComponent(SpriteComp);
            }
        }

        [Fact]
        public void LoadState_BallEntity_WithSpriteComponent_SpriteIsNull_DoesNotThrow()
        {
            // Arrange
            var system = new EntitySystem();
            var entity = system.CreateEntity<TestBallEntity>();
            entity.Position = new Vector2(100, 200);
            entity.Rotation = 0.5f;
            entity.SetId("test_ball_1");
            
            // Save state
            var tempFile = Path.GetTempFileName();
            try
            {
                GameStateSerializer.SaveState(system, tempFile);
                
                // Act - Load into new system
                var newSystem = new EntitySystem();
                GameStateSerializer.LoadState(newSystem, tempFile, mergeExisting: false);
                
                // Assert - Should load without throwing, and entity should exist
                var entities = newSystem.GetEntities();
                Assert.Single(entities);
                var loaded = entities.First();
                Assert.Equal("test_ball_1", loaded.Id);
                Assert.Equal(new Vector2(100, 200), loaded.Position);
                Assert.Equal(0.5f, loaded.Rotation, 0.01f);
                
                // Verify SpriteComponent exists but Sprite is null (since OnStart created it without asset)
                var spriteComp = loaded.GetComponent<SpriteComponent>();
                Assert.NotNull(spriteComp);
                // Sprite should be null because we didn't set it
                Assert.Null(spriteComp!.Sprite);
            }
            finally
            {
                if (File.Exists(tempFile))
                    File.Delete(tempFile);
            }
        }

        [Fact]
        public void LoadState_BallEntity_CreatesEntityViaReflection_CallsOnStart()
        {
            // Arrange
            var system = new EntitySystem();
            var entity = system.CreateEntity<TestBallEntity>();
            entity.Position = new Vector2(50, 60);
            entity.SetId("ball_test");
            
            var tempFile = Path.GetTempFileName();
            try
            {
                GameStateSerializer.SaveState(system, tempFile);
                
                // Act
                var newSystem = new EntitySystem();
                GameStateSerializer.LoadState(newSystem, tempFile, mergeExisting: false);
                
                // Assert
                var entities = newSystem.GetEntities();
                Assert.Single(entities);
                var loaded = entities.First();
                
                // OnStart should have been called during CreateEntity
                var spriteComp = loaded.GetComponent<SpriteComponent>();
                Assert.NotNull(spriteComp);
                
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
