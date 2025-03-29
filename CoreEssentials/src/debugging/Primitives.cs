using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace CoreEssentials.Debugging
{
    public class Primitives
    {
        private Texture2D _texture;
        private Texture2D GetTexture(SpriteBatch spriteBatch)
        {
            if (_texture == null)
            {
                _texture = new Texture2D(spriteBatch.GraphicsDevice, 1, 1, false, SurfaceFormat.Color);
                _texture.SetData(new[] { Color.White });
            }

            return _texture;
        }

        public void DrawRectangle(SpriteBatch spriteBatch, Rectangle rectangle, Color color, float thickness = 1f)
        {
            Vector2 topLeft = new Vector2(rectangle.X, rectangle.Y);
            Vector2 topRight = new Vector2(rectangle.X + rectangle.Width, rectangle.Y);
            Vector2 bottomLeft = new Vector2(rectangle.X, rectangle.Y + rectangle.Height);
            Vector2 bottomRight = new Vector2(rectangle.X + rectangle.Width, rectangle.Y + rectangle.Height);


            DrawLine(spriteBatch, topLeft, topRight, color, thickness);
            DrawLine(spriteBatch, topRight, bottomRight, color, thickness);
            DrawLine(spriteBatch, bottomLeft, bottomRight, color, thickness);
            DrawLine(spriteBatch, bottomLeft, topLeft, color, thickness);
        }

        public void DrawLine(SpriteBatch spriteBatch, Vector2 point1, Vector2 point2, Color color, float thickness = 1f)
        {
            var distance = Vector2.Distance(point1, point2);
            var angle = (float)Math.Atan2(point2.Y - point1.Y, point2.X - point1.X);
            DrawLine(spriteBatch, point1, distance, angle, color, thickness);
        }

        public void DrawLine(SpriteBatch spriteBatch, Vector2 point, float length, float angle, Color color, float thickness = 1f)
        {
            var origin = new Vector2(0f, 0.5f);
            var scale = new Vector2(length, thickness);
            spriteBatch.Draw(GetTexture(spriteBatch), point, null, color, angle, origin, scale, SpriteEffects.None, 0);
        }

        public void DrawCircle(SpriteBatch spriteBatch, Vector2 center, float radius, Color color, int segments = 16, float thickness = 1f)
        {
            var step = MathHelper.TwoPi / segments;
            var current = 0f;
            var previous = new Vector2(center.X + radius * (float)Math.Cos(current), center.Y + radius * (float)Math.Sin(current));

            for (var i = 1; i <= segments; i++)
            {
                current = step * i;
                var next = new Vector2(center.X + radius * (float)Math.Cos(current), center.Y + radius * (float)Math.Sin(current));
                DrawLine(spriteBatch, previous, next, color, thickness);
                previous = next;
            }
        }
    }
}
