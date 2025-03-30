using System;
using System.Reflection;
using Xunit;
using Microsoft.Xna.Framework;
using CoreEssentials.GameSystems.Physics;
using nkast.Aether.Physics2D.Dynamics;

namespace CoreEssentials.Tests.GameSystems.Physics
{
    public class WorldPoolTests
    {
        [Fact]
        public void CreateBody_EmptyPool_CreatesNewBody()
        {
            // Arrange
            var world = new World();
            var worldPool = new WorldPool(world);
            Vector2 position = new Vector2(5, 10);
            float rotation = 0.3f;
            BodyType bodyType = BodyType.Dynamic;
            
            // Act
            var body = worldPool.CreateBody(position, rotation, bodyType);
            
            // Assert
            Assert.NotNull(body);
            Assert.Equal(0, worldPool.Count); // Pool should be empty
            Assert.Equal(position.X, body.Position.X);
            Assert.Equal(position.Y, body.Position.Y);
            Assert.Equal(bodyType, body.BodyType);
        }

        [Fact]
        public void DestroyBody_AddsBodyToPool()
        {
            // Arrange
            var world = new World();
            var worldPool = new WorldPool(world);
            var body = world.CreateBody(new Vector2(1, 2), 0, BodyType.Dynamic);
            
            // Act
            worldPool.DestroyBody(body);
            
            // Assert
            Assert.Equal(1, worldPool.Count); // Pool should have one body
            Assert.False(body.Enabled); // Body should be disabled
        }
        
        [Fact]
        public void CreateBody_WithItemsInPool_ReusesBody()
        {
            // Arrange
            var world = new World();
            var worldPool = new WorldPool(world);
            
            // Create and destroy a body to put it in the pool
            var originalBody = world.CreateBody(new Vector2(1, 2), 0, BodyType.Dynamic);
            worldPool.DestroyBody(originalBody);
            
            // Verify pool has one item
            Assert.Equal(1, worldPool.Count);
            
            // Act - Create a new body, which should reuse from the pool
            Vector2 newPosition = new Vector2(5, 10);
            BodyType newType = BodyType.Static;
            var reuseBody = worldPool.CreateBody(newPosition, 0, newType);
            
            // Assert
            Assert.Same(originalBody, reuseBody); // Should be the same object
            Assert.Equal(0, worldPool.Count); // Pool should be empty again
            Assert.Equal(newPosition.X, reuseBody.Position.X); // Position should be updated
            Assert.Equal(newPosition.Y, reuseBody.Position.Y);
            Assert.Equal(newType, reuseBody.BodyType); // Type should be updated
            Assert.True(reuseBody.Enabled); // Body should be enabled
        }
        
        [Fact]
        public void DestroyBody_RemovesAllFixtures()
        {
            // Arrange
            var world = new World();
            var worldPool = new WorldPool(world);
            var body = world.CreateBody(new Vector2(1, 2), 0, BodyType.Dynamic);
            
            // Add some fixtures to the body
            body.CreateRectangle(2, 2, 1, Vector2.Zero);
            body.CreateCircle(1, 1, Vector2.Zero);
            
            // Verify body has fixtures
            Assert.Equal(2, body.FixtureList.Count);
            
            // Act
            worldPool.DestroyBody(body);
            
            // Assert
            Assert.Empty(body.FixtureList); // No fixtures should remain
        }
        
        [Fact]
        public void Count_ReflectsPoolSize()
        {
            // Arrange
            var world = new World();
            var worldPool = new WorldPool(world);
            
            // Initially should be empty
            Assert.Equal(0, worldPool.Count);
            
            // Add three bodies to the pool
            var body1 = world.CreateBody(Vector2.Zero);
            var body2 = world.CreateBody(Vector2.Zero);
            var body3 = world.CreateBody(Vector2.Zero);
            
            worldPool.DestroyBody(body1);
            worldPool.DestroyBody(body2);
            worldPool.DestroyBody(body3);
            
            // Assert
            Assert.Equal(3, worldPool.Count);
            
            // Take one out
            var reused = worldPool.CreateBody(Vector2.Zero, 0, BodyType.Dynamic);
            
            // Should have 2 left
            Assert.Equal(2, worldPool.Count);
        }
    }
}
