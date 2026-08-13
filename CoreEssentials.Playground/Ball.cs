using System;
using System.Collections;
using CoreEssentials.Assets;
using CoreEssentials.Coroutines;
using CoreEssentials.Debugging;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Components.BuiltIn;
using CoreEssentials.GameSystems.Physics.Engines.Aether;
using CoreEssentials.GameSystems.Physics.Types;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace CoreEssentials.Playground;

public class Ball : Entity
{
    private SpriteComponent _spriteComponent;
    private RigidbodyComponent _rigidbodyComponent;
    private ColliderComponent _colliderComponent;
    private float _radius;
    private float _scale = 1.0f;

    static Random _random = new Random();

    private CoroutineOwner _coroutineOwner;

    /// <summary>
    /// Gets the rigidbody component for this ball.
    /// </summary>
    public RigidbodyComponent RigidbodyComponent => _rigidbodyComponent;

    // Add a Scale property
    public float Scale
    {
        get => _scale;
        set
        {
            _scale = value;
            if (_rigidbodyComponent.Body != null && _colliderComponent.IsColliderCreated)
            {
                UpdateCollider();
            }
        }
    }

    public Ball(Vector2 position, string? id = null)
    {
        Position = position;
        sort = 0;
        _scale = (float)(_random.NextDouble() + 0.5f);
        if (id != null)
            SetId(id);
    }

    // Parameterless constructor for XML serialization (Sprint 10)
    public Ball() : this(Vector2.Zero) { }

    public override void OnStart()
    {
        base.OnStart();

        Console.WriteLine($"[Ball.OnStart] Id={Id}, _hasStarted check passed");

        // Add sprite component only if not already present (e.g., from deserialization)
        _spriteComponent = GetComponent<SpriteComponent>();
        if (_spriteComponent == null)
        {
            Console.WriteLine($"[Ball.OnStart] Creating NEW SpriteComponent for {Id}");
            var sprite = AssetManager.LoadAsset<Sprite>("ball_sprite.xml");
            _spriteComponent = new SpriteComponent(sprite);
            AddComponent(_spriteComponent);
        }
        else
        {
            Console.WriteLine($"[Ball.OnStart] Reusing EXISTING SpriteComponent for {Id}, Sprite={_spriteComponent.Sprite?.Name ?? "null"}, Color=({_spriteComponent.Color.R},{_spriteComponent.Color.G},{_spriteComponent.Color.B})");
        }

        // Assign sprite if it's null (e.g., component was created during deserialization without a sprite)
        if (_spriteComponent.Sprite == null)
        {
            Console.WriteLine($"[Ball.OnStart] Assigning sprite to existing component for {Id}");
            _spriteComponent.Sprite = AssetManager.LoadAsset<Sprite>("ball_sprite.xml");
        }
        else
        {
            // Only set default properties if this is a fresh entity (not loaded from save)
            Console.WriteLine($"[Ball.OnStart] Setting default properties for {Id}");
            _spriteComponent.Scale = new Vector2(_scale, _scale);
            _spriteComponent.Origin = new Vector2(0.5f, 0.5f);
            _spriteComponent.Color = Color.White;
            _spriteComponent.Effects = SpriteEffects.None;
            _spriteComponent.LayerDepth = 0f;
        }

        Console.WriteLine($"[Ball.OnStart] Final state for {Id}: Color=({_spriteComponent.Color.R},{_spriteComponent.Color.G},{_spriteComponent.Color.B}), Scale=({_spriteComponent.Scale.X},{_spriteComponent.Scale.Y})");

        // Add rigidbody component only if not already present
        _rigidbodyComponent = GetComponent<RigidbodyComponent>();
        if (_rigidbodyComponent == null)
        {
            _rigidbodyComponent = new RigidbodyComponent(RigidbodyType.Dynamic);
            AddComponent(_rigidbodyComponent);

            // Configure rigidbody properties (body auto-creates on first access)
            _rigidbodyComponent.FixedRotation = false;
            _rigidbodyComponent.Mass = 1f * _scale * _scale;
        }

        RegisterForInstancedRendering(_spriteComponent.Sprite);

        _radius = _spriteComponent.Sprite.GetSize().X / 2;

        // Add collider component only if not already present
        _colliderComponent = GetComponent<ColliderComponent>();
        if (_colliderComponent == null)
        {
            var colliderRadius = _radius * _scale;
            var colliderOffset = new Vector2(0, 1);
            _colliderComponent = new ColliderComponent(colliderRadius, colliderOffset)
            {
                Restitution = 1f
            };
            AddComponent(_colliderComponent);
        }

        // Start movement coroutine (base.OnStart() double-start guard prevents this from running twice on loaded entities)
        _coroutineOwner ??= new CoroutineOwner();
        _coroutineOwner.StartCoroutine(RandomMovementCoroutine());
    }

    private void UpdateCollider()
    {
        // Update collider radius based on new scale
        _colliderComponent.UpdateCircleRadius(_radius * _scale);

        // Update mass based on scale
        _rigidbodyComponent.Mass = 1f * _scale * _scale;
    }

    // (Update handled by RigidbodyComponent)

    private IEnumerator RandomMovementCoroutine()
    {
        while (true)
        {
            float randomX = (float)(_random.NextDouble() * 2 - 1);
            float randomY = (float)(_random.NextDouble() * 2 - 1);

            var impulseStrength = 500000f;
            _rigidbodyComponent.ApplyImpulse(new Vector2(randomX, randomY) * impulseStrength);

            // Add some spin so rotation is visible
            _rigidbodyComponent.ApplyAngularImpulse((float)_random.NextDouble() * 10 - 5);

            yield return new WaitForSeconds(_random.Next(1, 5));
        }
    }

    public override void Render(SpriteBatch spriteBatch)
    {
        if (_spriteComponent == null) return;
        _spriteComponent.Draw(spriteBatch);
    }

    public override void OnDestroy()
    {
        base.OnDestroy();

        // Destroy physics body through component
        _rigidbodyComponent.DestroyBody();

        _coroutineOwner.StopAllCoroutines();
        _coroutineOwner = null;
    }

    ~Ball()
    {
        if (_rigidbodyComponent.Body != null)
        {
            throw new InvalidOperationException("Ball is not destroyed properly. Please call Destroy() method.");
        }

        if (_coroutineOwner != null)
        {
            throw new InvalidOperationException("Coroutine owner is not destroyed properly. Please call StopAllCoroutines() method.");
        }
    }
}
