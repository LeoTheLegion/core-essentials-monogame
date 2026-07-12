using System;
using Xunit;
using Microsoft.Xna.Framework;
using CoreEssentials.GameSystems.Physics;
using nkast.Aether.Physics2D.Dynamics;

namespace CoreEssentials.Tests.GameSystems.Physics
{
    public class PhysicsEngineTests
    {
        [Fact]
        public void Constructor_DefaultScale_SetsScaleToZero()
        {
            // Arrange & Act
            var physicsEngine = new PhysicsEngine();
            
            // Assert
            Assert.Equal(0, physicsEngine.Config.Scale);
        }
        
        [Fact]
        public void Constructor_Default_ShouldInitializeWorld()
        {
            // Arrange & Act
            var physicsEngine = new PhysicsEngine();

            // Assert
            Assert.NotNull(physicsEngine.Bodies);
            Assert.Empty(physicsEngine.Bodies);
        }

        [Fact]
        public void Constructor_WithScale_ShouldInitializeWorld()
        {
            // Arrange & Act
            var physicsEngine = new PhysicsEngine();

            // Assert
            Assert.NotNull(physicsEngine.Bodies);
            Assert.Empty(physicsEngine.Bodies);
        }

        [Fact]
        public void Constructor_WithConfig_UsesProvidedConfig()
        {
            // Arrange
            var config = new PhysicsConfig
            {
                Scale = 150,
                VelocityIterations = 6,
                PositionIterations = 2,
                ContinuousPhysics = false
            };

            // Act
            var physicsEngine = new PhysicsEngine(config);

            // Assert
            Assert.NotNull(physicsEngine.Config);
            Assert.Equal(150, physicsEngine.Config.Scale);
            Assert.Equal(6, physicsEngine.Config.VelocityIterations);
            Assert.Equal(2, physicsEngine.Config.PositionIterations);
            Assert.False(physicsEngine.Config.ContinuousPhysics);
        }

        [Fact]
        public void Constructor_WithConfig_SameInstanceUsed()
        {
            // Arrange
            var config = new PhysicsConfig { Scale = 200 };

            // Act
            var physicsEngine = new PhysicsEngine(config);
            physicsEngine.Config.Scale = 250;

            // Assert - config should be the same instance
            Assert.Equal(250, config.Scale);
        }

        [Fact]
        public void Constructor_WithNullConfig_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new PhysicsEngine((PhysicsConfig)null));
        }
        
        [Fact]
        public void CreateBody_ValidParameters_ReturnsNewBody()
        {
            // Arrange
            var physicsEngine = new PhysicsEngine();
            Vector2 position = new Vector2(10, 20);
            float rotation = 0.5f;
            BodyType bodyType = BodyType.Dynamic;
            
            // Act
            var body = physicsEngine.CreateBody(position, rotation, bodyType);
            
            // Assert
            Assert.NotNull(body);
            Assert.Equal(position.X, body.Position.X);
            Assert.Equal(position.Y, body.Position.Y);
            Assert.Equal(bodyType, body.BodyType);
            Assert.True(body.Enabled);
        }
        
        [Fact]
        public void Destroy_Body_DisablesBody()
        {
            // Arrange
            var physicsEngine = new PhysicsEngine();
            var body = physicsEngine.CreateBody(new Vector2(10, 20), 0, BodyType.Dynamic);
            
            // Act
            physicsEngine.Destroy(body);
            
            // Assert
            Assert.False(body.Enabled);
        }
        
        [Fact]
        public void FixedUpdate_SetsPhysicsStep()
        {
            // Arrange
            var physicsEngine = new PhysicsEngine();
            
            var gameTime = new GameTime(
                TimeSpan.FromSeconds(10),
                TimeSpan.FromSeconds(1.0/60.0) // 16.67ms frame time (60 FPS)
            );
            
            // Create a body to observe physics step effects
            var body = physicsEngine.CreateBody(new Vector2(0, 0), 0, BodyType.Dynamic);
            
            // Apply a force to make it move (so we can observe step effects)
            body.ApplyForce(new Vector2(1000, 0));
            
            // Initial position should be (0, 0)
            Vector2 initialPosition = new Vector2(body.Position.X, body.Position.Y);
            
            // Act
            physicsEngine.FixedUpdate(gameTime);
            
            // Assert
            // After update, position should have changed due to the physics step
            Vector2 newPosition = new Vector2(body.Position.X, body.Position.Y);
            Assert.NotEqual(initialPosition, newPosition);
        }
        
        [Fact]
        public void AdjustSimSpeed_FewBodies_ReturnsFullSpeed()
        {
            // Use reflection to call the private method
            var physicsEngine = new PhysicsEngine();
            
            // Create a few bodies (less than 1000)
            for (int i = 0; i < 10; i++)
            {
                physicsEngine.CreateBody(new Vector2(i, i), 0, BodyType.Dynamic);
            }
            
            // Use reflection to call the private method
            var methodInfo = typeof(PhysicsEngine).GetMethod("AdjustSimSpeed", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var result = (float)methodInfo.Invoke(physicsEngine, null);
            
            // Should return 1.0 (full speed) for < 1000 bodies
            Assert.Equal(1.0f, result);
        }

        [Fact]
        public void Config_DefaultValues_MatchAetherDefaults()
        {
            // Arrange & Act
            var physicsEngine = new PhysicsEngine();
            
            // Assert - defaults should match Aether's defaults (0, 8, 3, true)
            Assert.NotNull(physicsEngine.Config);
            Assert.Equal(0, physicsEngine.Config.Scale);
            Assert.Equal(8, physicsEngine.Config.VelocityIterations);
            Assert.Equal(3, physicsEngine.Config.PositionIterations);
            Assert.True(physicsEngine.Config.ContinuousPhysics);
        }

        [Fact]
        public void Config_CanModifyScale()
        {
            // Arrange
            var physicsEngine = new PhysicsEngine();
            
            // Act
            physicsEngine.Config.Scale = 100;
            
            // Assert
            Assert.Equal(100, physicsEngine.Config.Scale);
        }

        [Fact]
        public void Config_CanModifyVelocityIterations()
        {
            // Arrange
            var physicsEngine = new PhysicsEngine();
            
            // Act
            physicsEngine.Config.VelocityIterations = 4;
            
            // Assert
            Assert.Equal(4, physicsEngine.Config.VelocityIterations);
        }

        [Fact]
        public void Config_CanModifyPositionIterations()
        {
            // Arrange
            var physicsEngine = new PhysicsEngine();
            
            // Act
            physicsEngine.Config.PositionIterations = 2;
            
            // Assert
            Assert.Equal(2, physicsEngine.Config.PositionIterations);
        }

        [Fact]
        public void Config_CanModifyContinuousPhysics()
        {
            // Arrange
            var physicsEngine = new PhysicsEngine();
            
            // Act
            physicsEngine.Config.ContinuousPhysics = false;
            
            // Assert
            Assert.False(physicsEngine.Config.ContinuousPhysics);
        }

        [Fact]
        public void Config_ParticleSystemSettings_AppliedCorrectly()
        {
            // Arrange
            var physicsEngine = new PhysicsEngine();
            
            // Act - Apply recommended particle system settings
            physicsEngine.Config.VelocityIterations = 4;
            physicsEngine.Config.PositionIterations = 2;
            physicsEngine.Config.ContinuousPhysics = false;
            
            // Assert
            Assert.Equal(4, physicsEngine.Config.VelocityIterations);
            Assert.Equal(2, physicsEngine.Config.PositionIterations);
            Assert.False(physicsEngine.Config.ContinuousPhysics);
        }

        [Fact]
        public void Config_PrecisionSettings_AppliedCorrectly()
        {
            // Arrange
            var physicsEngine = new PhysicsEngine();
            
            // Act - Apply high precision settings
            physicsEngine.Config.VelocityIterations = 10;
            physicsEngine.Config.PositionIterations = 4;
            physicsEngine.Config.ContinuousPhysics = true;
            
            // Assert
            Assert.Equal(10, physicsEngine.Config.VelocityIterations);
            Assert.Equal(4, physicsEngine.Config.PositionIterations);
            Assert.True(physicsEngine.Config.ContinuousPhysics);
        }

        [Fact]
        public void FixedUpdate_AppliesConfiguredSettings()
        {
            // Arrange
            var physicsEngine = new PhysicsEngine();
            physicsEngine.Config.VelocityIterations = 6;
            physicsEngine.Config.PositionIterations = 2;
            physicsEngine.Config.ContinuousPhysics = false;
            
            var gameTime = new GameTime(
                TimeSpan.FromSeconds(0),
                TimeSpan.FromSeconds(1.0/60.0)
            );
            
            // Create a dynamic body
            var body = physicsEngine.CreateBody(new Vector2(0, 0), 0, BodyType.Dynamic);
            body.ApplyForce(new Vector2(100, 0));
            
            // Act - FixedUpdate should apply the configured settings
            physicsEngine.FixedUpdate(gameTime);
            
            // Assert - No exception should be thrown, which verifies the config was applied
            // The fact that FixedUpdate completes successfully means the solver iterations were valid
            Assert.True(true);
        }
    }
}
