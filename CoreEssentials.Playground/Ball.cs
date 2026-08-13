using System;
using System.Collections;
using System.Globalization;
using System.Xml.Linq;
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

    // Deferred state for post-OnStart restoration (components don't exist during DeserializeFromXml)
    private XElement? _deferredPhysicsElement;
    private XElement? _deferredSpriteElement;

    static Random _random = new Random();

    private CoroutineOwner _coroutineOwner;

    /// <summary>
    /// Gets the rigidbody component for this ball.
    /// </summary>
    public RigidbodyComponent RigidbodyComponent => _rigidbodyComponent;

    public Ball(Vector2 position, string? id = null)
    {
        Position = position;
        sort = 0;
        float randomScale = (float)(_random.NextDouble() + 0.5f);
        Scale = new Vector2(randomScale, randomScale);
        if (id != null)
            SetId(id);
    }

    // Parameterless constructor for XML serialization (Sprint 10)
    public Ball() : this(Vector2.Zero) { }

    public override void OnStart()
    {
        base.OnStart();

        // Add sprite component only if not already present (e.g., from deserialization)
        _spriteComponent = GetComponent<SpriteComponent>();
        if (_spriteComponent == null)
        {
            var sprite = AssetManager.LoadAsset<Sprite>("ball_sprite.xml");
            _spriteComponent = new SpriteComponent(sprite);
            AddComponent(_spriteComponent);

            // Set default properties for fresh entities
            _spriteComponent.Origin = new Vector2(0.5f, 0.5f);
            _spriteComponent.Color = Color.White;
            _spriteComponent.Effects = SpriteEffects.None;
            _spriteComponent.LayerDepth = 0f;
        }
        else
        {
            // Assign sprite if it's null (e.g., component was created during deserialization without a sprite)
            if (_spriteComponent.Sprite == null)
            {
                _spriteComponent.Sprite = AssetManager.LoadAsset<Sprite>("ball_sprite.xml");
            }
        }

        // Add rigidbody component only if not already present
        _rigidbodyComponent = GetComponent<RigidbodyComponent>();
        if (_rigidbodyComponent == null)
        {
            _rigidbodyComponent = new RigidbodyComponent(RigidbodyType.Dynamic);
            AddComponent(_rigidbodyComponent);

            // Configure rigidbody properties (body auto-creates on first access)
            _rigidbodyComponent.FixedRotation = false;
            _rigidbodyComponent.Mass = 1f * Scale.X * Scale.X;
        }

        RegisterForInstancedRendering(_spriteComponent.Sprite);

        _radius = _spriteComponent.Sprite.GetSize().X / 2;

        // Add collider component only if not already present
        _colliderComponent = GetComponent<ColliderComponent>();
        if (_colliderComponent == null)
        {
            var colliderRadius = _radius * Scale.X;
            var colliderOffset = new Vector2(0, 1);
            _colliderComponent = new ColliderComponent(colliderRadius, colliderOffset)
            {
                Restitution = 1f
            };
            AddComponent(_colliderComponent);
        }

        // Restore deferred state (physics velocity, sprite color) now that components exist
        if (_deferredPhysicsElement != null && _rigidbodyComponent?.Body != null)
        {
            var body = _rigidbodyComponent.Body;
            var physics = _deferredPhysicsElement;

            if (float.TryParse(physics.Attribute("LinearVelocityX")?.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out float velX) &&
                float.TryParse(physics.Attribute("LinearVelocityY")?.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out float velY))
            {
                var targetVelocity = new Vector2(velX, velY);
                var velocityChange = targetVelocity - body.LinearVelocity;
                body.ApplyImpulse(velocityChange * body.Mass);
            }

            if (float.TryParse(physics.Attribute("AngularVelocity")?.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out float angVel))
            {
                body.AngularVelocity = angVel;
            }
        }
        _deferredPhysicsElement = null;

        if (_deferredSpriteElement != null && _spriteComponent != null)
        {
            var colorAttr = _deferredSpriteElement.Attribute("Color")?.Value;
            if (colorAttr != null && uint.TryParse(colorAttr, out uint argb))
            {
                _spriteComponent.Color = new Microsoft.Xna.Framework.Color(argb);
            }
        }
        _deferredSpriteElement = null;

        // Start movement coroutine (base.OnStart() double-start guard prevents this from running twice on loaded entities)
        _coroutineOwner ??= new CoroutineOwner();
        _coroutineOwner.StartCoroutine(RandomMovementCoroutine());
    }

    private void UpdateCollider()
    {
        // Update collider radius based on new scale
        _colliderComponent.UpdateCircleRadius(_radius * Scale.X);

        // Update mass based on scale
        _rigidbodyComponent.Mass = 1f * Scale.X * Scale.X;
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

    /// <summary>
    /// Serializes this ball's state including physics velocity.
    /// </summary>
    public override XElement SerializeToXml()
    {
        // Get base serialization (Position, Scale, Tags, etc.)
        var element = base.SerializeToXml();

        // Explicitly add physics state - Ball decides what matters for gameplay
        if (_rigidbodyComponent?.Body != null)
        {
            var body = _rigidbodyComponent.Body;
            element.Add(new XElement("Physics",
                new XAttribute("LinearVelocityX", body.LinearVelocity.X.ToString(CultureInfo.InvariantCulture)),
                new XAttribute("LinearVelocityY", body.LinearVelocity.Y.ToString(CultureInfo.InvariantCulture)),
                new XAttribute("AngularVelocity", body.AngularVelocity.ToString(CultureInfo.InvariantCulture))
            ));
        }

        // Save sprite color so it survives save/load round-trips
        if (_spriteComponent != null)
        {
            element.Add(new XElement("Sprite",
                new XAttribute("Color", _spriteComponent.Color.PackedValue.ToString())
            ));
        }

        return element;
    }

    /// <summary>
    /// Restores this ball's state including physics velocity.
    /// Defers component restoration to OnStart() since components don't exist yet during deserialization.
    /// </summary>
    public override void DeserializeFromXml(XElement element, bool mergeExisting = false)
    {
        // Restore base state (Position, Scale, Tags, etc.)
        base.DeserializeFromXml(element, mergeExisting);

        // Defer physics and sprite restoration until OnStart() creates the components
        _deferredPhysicsElement = element.Element("Physics");
        _deferredSpriteElement = element.Element("Sprite");
    }
}
