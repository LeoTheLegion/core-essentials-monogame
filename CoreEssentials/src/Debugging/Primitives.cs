using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace CoreEssentials.Debugging
{
    /// <summary>
    /// Provides functionality for drawing simple geometric shapes and debug visualizations.
    /// Useful for rendering collision bounds, pathfinding information, and other debug elements.
    /// </summary>
    public class Primitives
    {
        private Texture2D? _texture;

        /// <summary>
        /// Gets the texture used for drawing primitives.
        /// </summary>
        /// <param name="spriteBatch">The SpriteBatch used for drawing.</param>
        /// <returns>The texture used for drawing primitives.</returns>
        private Texture2D GetTexture(SpriteBatch spriteBatch)
        {
            if (_texture == null)
            {
                _texture = new Texture2D(spriteBatch.GraphicsDevice, 1, 1, false, SurfaceFormat.Color);
                _texture.SetData(new[] { Color.White });
            }

            return _texture;
        }

        /// <summary>
        /// Draws a rectangle.
        /// </summary>
        /// <param name="spriteBatch">The SpriteBatch used for drawing.</param>
        /// <param name="rectangle">The rectangle to draw.</param>
        /// <param name="color">The color of the rectangle.</param>
        /// <param name="thickness">The thickness of the rectangle's lines in pixels.</param>
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

        /// <summary>
        /// Draws a line between two points.
        /// </summary>
        /// <param name="spriteBatch">The SpriteBatch used for drawing.</param>
        /// <param name="point1">The starting point of the line.</param>
        /// <param name="point2">The ending point of the line.</param>
        /// <param name="color">The color of the line.</param>
        /// <param name="thickness">The thickness of the line in pixels.</param>
        public void DrawLine(SpriteBatch spriteBatch, Vector2 point1, Vector2 point2, Color color, float thickness = 1f)
        {
            var distance = Vector2.Distance(point1, point2);
            var angle = (float)Math.Atan2(point2.Y - point1.Y, point2.X - point1.X);
            DrawLine(spriteBatch, point1, distance, angle, color, thickness);
        }

        /// <summary>
        /// Draws a line with specified length and angle.
        /// </summary>
        /// <param name="spriteBatch">The SpriteBatch used for drawing.</param>
        /// <param name="point">The starting point of the line.</param>
        /// <param name="length">The length of the line in pixels.</param>
        /// <param name="angle">The angle of the line in radians.</param>
        /// <param name="color">The color of the line.</param>
        /// <param name="thickness">The thickness of the line in pixels.</param>
        public void DrawLine(SpriteBatch spriteBatch, Vector2 point, float length, float angle, Color color, float thickness = 1f)
        {
            var origin = new Vector2(0f, 0.5f);
            var scale = new Vector2(length, thickness);
            spriteBatch.Draw(GetTexture(spriteBatch), point, null, color, angle, origin, scale, SpriteEffects.None, 0);
        }

        /// <summary>
        /// Draws a circle.
        /// </summary>
        /// <param name="spriteBatch">The SpriteBatch used for drawing.</param>
        /// <param name="center">The center point of the circle.</param>
        /// <param name="radius">The radius of the circle in pixels.</param>
        /// <param name="color">The color of the circle.</param>
        /// <param name="segments">The number of line segments used to approximate the circle.</param>
        /// <param name="thickness">The thickness of the circle's outline in pixels.</param>
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
