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
        public void SingletonInstance_AlwaysReturnsSameInstance()
        {
            var instance1 = PhysicsEngine.Instance;
            var instance2 = PhysicsEngine.Instance;
            Assert.Same(instance1, instance2);
        }

        [Fact]
        public void Constructor_DefaultScale_SetsScaleToZero()
        {
            // Arrange & Act
            var physicsEngine = PhysicsEngine.Instance;
            
            // Assert
            Assert.Equal(0, physicsEngine.Scale);
        }
        
        [Fact]
        public void Constructor_CustomScale_SetsScaleCorrectly()
        {
            // Arrange & Act
            int expectedScale = 42;
            var physicsEngine = new PhysicsEngine(expectedScale);
            
            // Assert
            Assert.Equal(expectedScale, physicsEngine.Scale);
        }
        
        [Fact]
        public void CreateBody_ValidParameters_ReturnsNewBody()
        {
            // Arrange
            var physicsEngine = PhysicsEngine.Instance;
            physicsEngine.Reset();
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
            var physicsEngine = PhysicsEngine.Instance;
            physicsEngine.Reset();
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
            var physicsEngine = PhysicsEngine.Instance;

            physicsEngine.Reset();
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
            var physicsEngine = PhysicsEngine.Instance;
            physicsEngine.Reset();
            
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
    }
}
