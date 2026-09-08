using System;
using CoreEssentials.Assets;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Moq;
using Xunit;

namespace CoreEssentials.Tests.Assets
{
    public class SpriteScaleTests
    {
        private class MockSprite : Sprite
        {
            public MockSprite() : base("test_sprite")
            {
            }
            
            public override void Load(IContentManager contentManager) 
            {
                // Mock implementation for testing
            }
            
            public override void Unload(IContentManager contentManager)
            {
                // Mock implementation for testing
            }
            
            // Expose the Draw method for testing
            public bool DrawWithVectorScaleCalled { get; private set; }
            public Vector2 LastVectorScale { get; private set; }
            
            // Expose the Draw method with float scale for testing
            public bool DrawWithFloatScaleCalled { get; private set; }
            public float LastFloatScale { get; private set; }

            // The unused parameters mirror the real Sprite.Draw signature.
            public void TestDraw(SpriteBatch _spriteBatch, Vector2 _position, Color _color, float _rotation, 
                              Vector2 scale, SpriteEffects _effects, float _layerDepth)
            {
                DrawWithVectorScaleCalled = true;
                LastVectorScale = scale;
            }
            
            public void TestDraw(SpriteBatch _spriteBatch, Vector2 _position, Color _color, float _rotation, 
                              float scale, SpriteEffects _effects, float _layerDepth)
            {
                DrawWithFloatScaleCalled = true;
                LastFloatScale = scale;
                TestDraw(_spriteBatch, _position, _color, _rotation, new Vector2(scale, scale), _effects, _layerDepth);
            }
        }
        
        [Fact]
        public void Draw_WithVectorScale_ShouldScaleSprite()
        {
            // Arrange
            var mockSprite = new MockSprite();
            Vector2 expectedScale = new Vector2(2.0f, 1.5f);
            
            // Act
            mockSprite.TestDraw(
                null!, // SpriteBatch not needed for this test
                Vector2.Zero,
                Color.White,
                0f,
                expectedScale,
                SpriteEffects.None,
                0f
            );
            
            // Assert
            Assert.True(mockSprite.DrawWithVectorScaleCalled);
            Assert.Equal(expectedScale, mockSprite.LastVectorScale);
        }
        
        [Fact]
        public void Draw_WithFloatScale_ShouldScaleSprite()
        {
            // Arrange
            var mockSprite = new MockSprite();
            float expectedScale = 2.0f;
            
            // Act
            mockSprite.TestDraw(
                null!, // SpriteBatch not needed for this test
                Vector2.Zero,
                Color.White,
                0f,
                expectedScale,
                SpriteEffects.None,
                0f
            );
            
            // Assert
            Assert.True(mockSprite.DrawWithFloatScaleCalled);
            Assert.Equal(expectedScale, mockSprite.LastFloatScale);
            Assert.Equal(new Vector2(expectedScale, expectedScale), mockSprite.LastVectorScale);
        }
    }
}
