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

        LoadAssets();
    }

    public void LoadAssets()
    {
        this._sprite = AssetManager.LoadAsset<Sprite>("ball_sprite.json");

        // I hate this but I have to do it this way for now
        _radius = this._sprite.GetSize().X / 2; // Assuming the sprite is a circle, use half the width as the radius

        this._body = PhysicsEngine.Instance.CreateBody(_position,0, BodyType.Dynamic);

        this._body.FixedRotation = false; // Allow rotation
        this._body.Mass = 1f; // Set mass to 1 kg

        _collisionFixture = this._body.CreateCircle(_radius,1);

        _collisionFixture.Restitution = 1f; // Bounciness
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
        float rotation = _body.Rotation; // Get the rotation from the physics body
        _sprite.Draw(_spriteBatch, _position, Color.White, rotation , SpriteEffects.None, 0f);
        _spriteBatch.End();
    }
}
