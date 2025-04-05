using System;
using CoreEssentials.Assets;
using CoreEssentials.Debugging;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem;
using CoreEssentials.GameSystems.Physics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using nkast.Aether.Physics2D.Dynamics;

namespace CoreEssentials.Playground;

public class Ball : Entity
{
    private Sprite _sprite;
    private Body _body;
    private Fixture _collisionFixture;
    private float _radius;

    public Body Body => _body;

    public Ball(Vector2 position)
    {
        _position = position;
        sort = 0;

        _radius = 16f;

        this._body = PhysicsEngine.Instance.CreateBody(_position,0, BodyType.Dynamic);

        this._body.FixedRotation = false; // Allow rotation
        this._body.Mass = 1f; // Set mass to 1 kg

        _collisionFixture = this._body.CreateCircle(_radius,1);

        _collisionFixture.Restitution = 1f; // Bounciness
    }

    public override void LoadAssets()
    {
        this._sprite = AssetManager.LoadAsset<Sprite>("Ball");
    }

    public override void Update(ref GameTime gameTime)
    {
        if (_body != null)
        {
            _position = _body.Position;
        }
    }

    public override void Render(ref SpriteBatch _spriteBatch)
    {
        _spriteBatch.Begin();
        Vector2 spriteSize = _sprite.GetSize();
        Vector2 targetSize = new Vector2(_radius * 2, _radius * 2);
        Vector2 targetCenter = new Vector2(_radius, _radius);
        Vector2 scale = new Vector2(targetSize.X / spriteSize.X, targetSize.Y / spriteSize.Y);
        Vector2 origin = new Vector2(spriteSize.X / 2, spriteSize.Y / 2);
        float rotation = _body.Rotation; // Get the rotation from the physics body

        _sprite.Draw(_spriteBatch, _position, Color.White, rotation , origin, scale, SpriteEffects.None, 0f);

        Rectangle targetRectangle = new Rectangle((int)(_position.X - targetCenter.X), (int)(_position.Y - targetCenter.X), (int)(targetSize.X), (int)(targetSize.Y));

        Debug.Primitives.DrawRectangle(_spriteBatch, targetRectangle, Color.Red, 1f);
        _spriteBatch.End();
    }
}
