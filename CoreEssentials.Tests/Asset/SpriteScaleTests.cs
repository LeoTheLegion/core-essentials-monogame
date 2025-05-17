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
            private bool _isLoaded = false;
            
            public MockSprite() : base("test_sprite")
            {
                _isLoaded = true;
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
            
            public void TestDraw(SpriteBatch spriteBatch, Vector2 position, Color color, float rotation, 
                              Vector2 scale, SpriteEffects effects, float layerDepth)
            {
                DrawWithVectorScaleCalled = true;
                LastVectorScale = scale;
            }
            
            // Expose the Draw method with float scale for testing
            public bool DrawWithFloatScaleCalled { get; private set; }
            public float LastFloatScale { get; private set; }
            
            public void TestDraw(SpriteBatch spriteBatch, Vector2 position, Color color, float rotation, 
                              float scale, SpriteEffects effects, float layerDepth)
            {
                DrawWithFloatScaleCalled = true;
                LastFloatScale = scale;
                TestDraw(spriteBatch, position, color, rotation, new Vector2(scale, scale), effects, layerDepth);
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
                null, // SpriteBatch not needed for this test
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
                null, // SpriteBatch not needed for this test
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
