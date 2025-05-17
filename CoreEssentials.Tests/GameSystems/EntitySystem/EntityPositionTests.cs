using System;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Xunit;

namespace CoreEssentials.Tests.GameSystems.EntitySystems.EntityOOPSystem
{
    public class EntityPositionTests
    {
        /// <summary>
        /// Test entity class for unit testing
        /// </summary>
        private class TestEntity : Entity
        {
            public TestEntity(Vector2 position)
            {
                _position = position;
            }
            
            public TestEntity()
            {
                // Uses default position
            }
            
            public override void Render(SpriteBatch spriteBatch)
            {
                // Test implementation
            }
        }
        
        [Fact]
        public void Position_Get_ShouldReturnCorrectPosition()
        {
            // Arrange
            var expectedPosition = new Vector2(100, 200);
            var entity = new TestEntity(expectedPosition);
            
            // Act
            var position = entity.Position;
            
            // Assert
            Assert.Equal(expectedPosition, position);
        }
        
        [Fact]
        public void Position_Set_ShouldUpdatePosition()
        {
            // Arrange
            var entity = new TestEntity();
            var newPosition = new Vector2(300, 400);
            
            // Act
            entity.Position = newPosition;
            
            // Assert
            Assert.Equal(newPosition, entity.Position);
        }
        
        [Fact]
        public void Rotation_Get_ShouldReturnCorrectRotation()
        {
            // Arrange
            var entity = new TestEntity();
            var expectedRotation = 1.5f;
            
            // Use reflection to set the protected field
            typeof(Entity).GetField("_rotation", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
                .SetValue(entity, expectedRotation);
            
            // Act
            var rotation = entity.Rotation;
            
            // Assert
            Assert.Equal(expectedRotation, rotation);
        }
        
        [Fact]
        public void Rotation_Set_ShouldUpdateRotation()
        {
            // Arrange
            var entity = new TestEntity();
            var newRotation = 2.5f;
            
            // Act
            entity.Rotation = newRotation;
            
            // Assert
            Assert.Equal(newRotation, entity.Rotation);
        }
    }
}
