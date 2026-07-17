using System;
using System.Collections;
using CoreEssentials.Assets;
using CoreEssentials.Coroutines;
using CoreEssentials.Debugging;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem;
using CoreEssentials.GameSystems.Physics.Engines.Aether;
using CoreEssentials.GameSystems.Physics.Types;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace CoreEssentials.Playground;

public class Ball : Entity
{    private Sprite _sprite;
    private IPhysicsBody _body;
    private IFixture _collisionFixture;
    private float _radius;
    private float _scale = 1.0f; // Add a scale field

    static Random _random = new Random();

    private CoroutineOwner _coroutineOwner;

    public IPhysicsBody Body => _body;    

    // Add a Scale property
    public float Scale
    {
        get => _scale;
        set
        {
            _scale = value;
            if (_body != null && _collisionFixture != null)
            {
                // Update the physics body when scale changes
                UpdatePhysicsBody();
            }
        }
    }

    public Ball(Vector2 position)
    {
        Position = position;
        sort = 0;
        
        // Randomize the scale between 0.5 and 1.5
        _scale = (float)(_random.NextDouble() + 0.5f);
    }    public override void OnStart()
    {
        base.OnStart();

        this._sprite = AssetManager.LoadAsset<Sprite>("ball_sprite.xml");

        // I hate this but I have to do it this way for now
        _radius = this._sprite.GetSize().X / 2; // Assuming the sprite is a circle, use half the width as the radius

        PhysicsEngine physicsEngine = EntitySystem.GetGameSystem<PhysicsEngine>();
        
        // Create the physics body with appropriate scale
        CreatePhysicsBody(physicsEngine);

        _coroutineOwner = new CoroutineOwner();
        _coroutineOwner.StartCoroutine(RandomMovementCoroutine());
    }

    private void CreatePhysicsBody(PhysicsEngine physicsEngine)
    {
        this._body = physicsEngine.CreateDynamic(Position);
        this._body.FixedRotation = false; // Allow rotation
        this._body.Mass = 1f * _scale * _scale; // Scale the mass proportionally to the area (scale squared)

        // Create a circle fixture with the scaled radius
        Vector2 offset = new Vector2(0, 1); // Note: API takes offset as second param for CreateCircle
        _collisionFixture = this._body.CreateCircle(_radius * _scale, offset);
        _collisionFixture.Restitution = 1f; // Bounciness
    }

    private void UpdatePhysicsBody()
    {
        if (_body != null && _collisionFixture != null)
        {
            // Remove the old fixture and create new one with updated scale
            _body.RemoveFixture(_collisionFixture);
            
            // Create a new fixture with the updated scale
            Vector2 offset = new Vector2(0, 1);
            _collisionFixture = _body.CreateCircle(_radius * _scale, offset);
            _collisionFixture.Restitution = 1f;
            
            // Update mass based on scale
            _body.Mass = 1f * _scale * _scale;
        }
    }

    public override void Update(GameTime gameTime)
    {
        if (_body != null)
        {
            Position = _body.WorldPosition;
            Rotation = _body.Rotation;
        }
    }

    private IEnumerator RandomMovementCoroutine()
    {
        while (true)
        {
            // Generate random force
            float randomX = (float)(_random.NextDouble() * 2 - 1); // Random value between -1 and 1
            float randomY = (float)(_random.NextDouble() * 2 - 1); // Random value between -1 and 1

            // Apply the random impulse to the body (center of mass)
            var impulseStrength = 500000f;
            _body.ApplyImpulse(new Vector2(randomX, randomY) * impulseStrength);

            // Wait for a short duration before applying the next force
            yield return new WaitForSeconds(_random.Next(1, 5)); // Random wait time between 1 and 5 seconds
        }
    }    public override void Render(SpriteBatch _spriteBatch)
    {
        float rotation = _body.Rotation; // Get the rotation from the physics body
        
        // Use the new Draw method with scale
        _sprite.Draw(_spriteBatch, Position, Color.White, rotation, _scale, SpriteEffects.None, 0f);
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
