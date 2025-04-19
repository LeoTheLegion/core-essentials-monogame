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
    public class PhysicsDebugRenderer : GameSystem, IDrawGameSystem
    {
        private PhysicsEngine _physicsEngine;
        private bool _drawDebug;

        public PhysicsDebugRenderer(MainGame mainGame, PhysicsEngine _physicsEngine): base(mainGame)
        {
            this._physicsEngine = _physicsEngine;
            _drawDebug = false;

            Input.Keyboard.KeyPressed += OnKeyPressed;
        }

        private void OnKeyPressed(object sender, KeyboardEventArgs arg)
        {
            if(arg.Key == Microsoft.Xna.Framework.Input.Keys.P)
            {
                _drawDebug = !_drawDebug;
            }
        }

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

        private void DrawCircle(SpriteBatch spriteBatch, CircleShape circleShape, Vector2 position, float rotation)
        {
            var center = position + circleShape.Position;
            var radius = circleShape.Radius;

            float f_scale = 0b0001 << _physicsEngine.Scale;

            center *= f_scale;
            radius *= f_scale;

            Debug.Primitives.DrawCircle(spriteBatch, center, radius, Color.Green, 16);
        }

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

                float f_scale = 0b0001 << _physicsEngine.Scale;

                p1 *= f_scale;
                p2 *= f_scale;

                Debug.Primitives.DrawLine(spriteBatch, p1, p2, Color.Green, 2);
            }
        }

        private void DrawRectangle(SpriteBatch spriteBatch, PolygonShape polygonShape, Vector2 position, float rotation)
        {
            var vertices = polygonShape.Vertices;

            Vector2 v1 = new Vector2(vertices[2].X, vertices[2].Y);
            Vector2 v2 = new Vector2(vertices[0].X, vertices[0].Y);

            v1 += position;
            v2 += position;

            float f_scale = 0b0001 << _physicsEngine.Scale;

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
