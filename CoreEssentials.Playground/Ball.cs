using System;
using System.Collections;
using CoreEssentials.Assets;
using CoreEssentials.Coroutines;
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

    static Random _random = new Random();

    private CoroutineOwner _coroutineOwner;

    public Body Body => _body;

    public Ball(Vector2 position)
    {
        _position = position;
        sort = 0;
    }

    public override void OnStart()
    {
        base.OnStart();

        this._sprite = (Sprite)AssetManager.LoadAsset<Sprite>("ball_sprite.xml");

        // I hate this but I have to do it this way for now
        _radius = this._sprite.GetSize().X / 2; // Assuming the sprite is a circle, use half the width as the radius

        PhysicsEngine physicsEngine = EntitySystem.GetGameSystem<PhysicsEngine>();
        
        this._body = physicsEngine.CreateBody(_position, 0, BodyType.Dynamic);

        this._body.FixedRotation = false; // Allow rotation
        this._body.Mass = 1f; // Set mass to 1 kg

        _collisionFixture = this._body.CreateCircle(_radius, 1);

        _collisionFixture.Restitution = 1f; // Bounciness

        _coroutineOwner = new CoroutineOwner();

        _coroutineOwner.StartCoroutine(RandomMovementCoroutine());
    }

    public override void Update(GameTime gameTime)
    {
        if (_body != null)
        {
            _position = _body.Position;
        }
    }

    private IEnumerator RandomMovementCoroutine()
    {
        while (true)
        {
            // Generate random force
            float randomX = (float)(_random.NextDouble() * 2 - 1); // Random value between -1 and 1
            float randomY = (float)(_random.NextDouble() * 2 - 1); // Random value between -1 and 1

            // Apply the random force to the body
            var forceStr = 500000f;
            _body.ApplyLinearImpulse(new Vector2(randomX, randomY) * forceStr); // Scale the force

            // Wait for a short duration before applying the next force
            yield return new WaitForSeconds(_random.Next(1, 5)); // Random wait time between 1 and 5 seconds
        }
    }

    public override void Render(SpriteBatch _spriteBatch)
    {
        float rotation = _body.Rotation; // Get the rotation from the physics body
        _sprite.Draw(_spriteBatch, _position, Color.White, rotation, SpriteEffects.None, 0f);
    }

    public override void OnDestroy()
    {
        base.OnDestroy();

        PhysicsEngine physicsEngine = EntitySystem.GetGameSystem<PhysicsEngine>();
        physicsEngine.Destroy(_body);
        _body = null; // Set to null to avoid dangling reference

        _coroutineOwner.StopAllCoroutines();
        _coroutineOwner = null; // Clean up the coroutine owner
    }

    ~ Ball()
    {
        // Destructor to clean up resources if needed
        if (_body != null)
        {
            throw new InvalidOperationException("Ball is not destroyed properly. Please call Destroy() method.");
        }

        if (_coroutineOwner != null)
        {
            throw new InvalidOperationException("Coroutine owner is not destroyed properly. Please call StopAllCoroutines() method.");
        }
    }
}
