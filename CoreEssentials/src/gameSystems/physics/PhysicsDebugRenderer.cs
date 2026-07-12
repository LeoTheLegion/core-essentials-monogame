using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Input.InputListeners;
using nkast.Aether.Physics2D.Collision.Shapes;
using nkast.Aether.Physics2D.Dynamics;
using CoreEssentials.GameSystems;
using CoreEssentials.Inputs;
using CoreEssentials.Debugging;

namespace CoreEssentials.GameSystems.Physics
{
    /// <summary>
    /// Debug renderer for physics bodies that visualizes physics shapes and colliders.
    /// Provides visual representation of the physics entities in the game world.
    /// </summary>
    public class PhysicsDebugRenderer : GameSystem, IDrawGameSystem
    {
        private PhysicsEngine _physicsEngine;
        private bool _drawDebug;

        /// <summary>
        /// Initializes a new instance of the PhysicsDebugRenderer class.
        /// </summary>
        /// <param name="_physicsEngine">The physics engine containing the bodies to debug render.</param>
        public PhysicsDebugRenderer(PhysicsEngine _physicsEngine)
        {
            this._physicsEngine = _physicsEngine;
            _drawDebug = false;

            Input.Keyboard.KeyPressed += OnKeyPressed;
        }

        /// <summary>
        /// Handles key press events to toggle debug rendering.
        /// Press 'P' to toggle physics visualization.
        /// </summary>
        /// <param name="sender">The source of the event. May be <see langword="null" />.</param>
        /// <param name="arg">Event arguments containing key information.</param>
        private void OnKeyPressed(object? sender, KeyboardEventArgs arg)
        {
            if(arg.Key == Microsoft.Xna.Framework.Input.Keys.P)
            {
                _drawDebug = !_drawDebug;
            }
        }

        /// <summary>
        /// Draws debug visualizations for all physics bodies.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        /// <param name="spriteBatch">The SpriteBatch used for drawing.</param>
        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            if (!_drawDebug)
                return;

            spriteBatch.Begin();

            for (int i = 0; i < _physicsEngine.Bodies.Count; i++)
            {
                var body = _physicsEngine.Bodies[i];
                var position = body.Position;
                var rotation = body.Rotation;

                if (body.FixtureList.Count == 0)
                    continue;

                foreach (var fixture in body.FixtureList)
                {
                    DrawFixture(spriteBatch, fixture, position, rotation);
                }
            }

            spriteBatch.End();
        }

        /// <summary>
        /// Draws a physics fixture based on its shape type.
        /// </summary>
        /// <param name="spriteBatch">The SpriteBatch used for drawing.</param>
        /// <param name="fixture">The physics fixture to draw.</param>
        /// <param name="position">The position of the body containing the fixture.</param>
        /// <param name="rotation">The rotation of the body containing the fixture.</param>
        private void DrawFixture(SpriteBatch spriteBatch, Fixture fixture, Vector2 position, float rotation)
        {
            var shape = fixture.Shape;
            if (shape is nkast.Aether.Physics2D.Collision.Shapes.PolygonShape polygonShape)
            {
                DrawPolygon(spriteBatch, polygonShape, position, rotation);
            }
            else if (shape is nkast.Aether.Physics2D.Collision.Shapes.CircleShape circleShape)
            {
                DrawCircle(spriteBatch, circleShape, position, rotation);
            }
        }

        /// <summary>
        /// Draws a circular physics shape.
        /// </summary>
        /// <param name="spriteBatch">The SpriteBatch used for drawing.</param>
        /// <param name="circleShape">The circle shape to draw.</param>
        /// <param name="position">The position of the body containing the shape.</param>
        /// <param name="rotation">The rotation of the body containing the shape.</param>
        private void DrawCircle(SpriteBatch spriteBatch, CircleShape circleShape, Vector2 position, float rotation)
        {
            var center = position + circleShape.Position;
            var radius = circleShape.Radius;

            float f_scale = 0b0001 << _physicsEngine.Config.Scale;

            center *= f_scale;
            radius *= f_scale;

            Debug.Primitives.DrawCircle(spriteBatch, center, radius, Color.Green, 16);
        }

        /// <summary>
        /// Draws a polygon physics shape.
        /// </summary>
        /// <param name="spriteBatch">The SpriteBatch used for drawing.</param>
        /// <param name="polygonShape">The polygon shape to draw.</param>
        /// <param name="position">The position of the body containing the shape.</param>
        /// <param name="rotation">The rotation of the body containing the shape.</param>
        private void DrawPolygon(SpriteBatch spriteBatch, PolygonShape polygonShape, Vector2 position, float rotation)
        {
            var vertices = polygonShape.Vertices;

            // Draw rectangle if it is a rectangle
            if(vertices.Count == 4)
            {
                DrawRectangle(spriteBatch, polygonShape, position, rotation);
                return;
            }

            for (int j = 0; j < vertices.Count; j++)
            {
                var v1 = vertices[j];
                var v2 = vertices[(j + 1) % vertices.Count];

                var p1 = position + v1;
                var p2 = position + v2;

                float f_scale = 0b0001 << _physicsEngine.Config.Scale;

                p1 *= f_scale;
                p2 *= f_scale;

                Debug.Primitives.DrawLine(spriteBatch, p1, p2, Color.Green, 2);
            }
        }

        /// <summary>
        /// Draws a rectangular physics shape.
        /// </summary>
        /// <param name="spriteBatch">The SpriteBatch used for drawing.</param>
        /// <param name="polygonShape">The polygon shape representing a rectangle.</param>
        /// <param name="position">The position of the body containing the shape.</param>
        /// <param name="rotation">The rotation of the body containing the shape.</param>
        private void DrawRectangle(SpriteBatch spriteBatch, PolygonShape polygonShape, Vector2 position, float rotation)
        {
            var vertices = polygonShape.Vertices;

            Vector2 v1 = new Vector2(vertices[2].X, vertices[2].Y);
            Vector2 v2 = new Vector2(vertices[0].X, vertices[0].Y);

            v1 += position;
            v2 += position;

            float f_scale = 0b0001 << _physicsEngine.Config.Scale;

            v1 *= f_scale;
            v2 *= f_scale;

            Debug.Primitives.DrawLine(spriteBatch, v1, v2, Color.Green, 2);
            Rectangle rectangle = new Rectangle(
                (int)(v1.X),
                (int)(v1.Y),
                (int)(v2.X - v1.X),
                (int)(v2.Y - v1.Y)
                );

            Debug.Primitives.DrawRectangle(spriteBatch, rectangle, Color.Green, 2);
        }
    }
}
