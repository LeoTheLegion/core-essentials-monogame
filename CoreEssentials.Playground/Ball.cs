#nullable enable

using System;
using System.Collections;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using CoreEssentials.Assets;
using CoreEssentials.Coroutines;
using CoreEssentials.Debugging;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Components.BuiltIn;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Serialization;
using CoreEssentials.GameSystems.Physics.Engines.Aether;
using CoreEssentials.GameSystems.Physics.Types;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace CoreEssentials.Playground;

public class Ball : Entity, ISaveableEntity
{
    private SpriteComponent? _spriteComponent;
    private RigidbodyComponent? _rigidbodyComponent;
    private ColliderComponent? _colliderComponent;
    private float _radius;

    static Random _random = new Random();

    private CoroutineOwner? _coroutineOwner;

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

        RegisterForInstancedRendering(_spriteComponent!.Sprite!);

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

        // Cleanup coroutines (component cleanup like DestroyBody is handled by OnDetach in base.OnDestroy())
        _coroutineOwner?.StopAllCoroutines();
        _coroutineOwner = null;
    }

    /// <summary>
    /// Saves this ball's state including position, transform, tags, physics velocity, and sprite color.
    /// </summary>
    public XElement SaveState()
    {
        var element = new XElement("Entity",
            new XAttribute("Id", Id ?? string.Empty),
            new XAttribute("Type", GetType().FullName),
            new XAttribute("Rotation", Rotation.ToString(CultureInfo.InvariantCulture)),
            new XAttribute("Sort", GetSort()),
            new XAttribute("Active", GetActive()),
            new XElement("Position",
                new XAttribute("X", Position.X.ToString(CultureInfo.InvariantCulture)),
                new XAttribute("Y", Position.Y.ToString(CultureInfo.InvariantCulture))
            ),
            new XElement("Scale",
                new XAttribute("X", Scale.X.ToString(CultureInfo.InvariantCulture)),
                new XAttribute("Y", Scale.Y.ToString(CultureInfo.InvariantCulture))
            ),
            new XElement("Tags",
                Tags.Select(tag => new XElement("Tag", new XAttribute("Name", tag)))
            )
        );

        // Add physics state
        if (_rigidbodyComponent.IsBodyCreated)
        {
            element.Add(new XElement("Physics",
                new XAttribute("LinearVelocityX", _rigidbodyComponent.LinearVelocity.X.ToString(CultureInfo.InvariantCulture)),
                new XAttribute("LinearVelocityY", _rigidbodyComponent.LinearVelocity.Y.ToString(CultureInfo.InvariantCulture)),
                new XAttribute("AngularVelocity", _rigidbodyComponent.AngularVelocity.ToString(CultureInfo.InvariantCulture))
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
    /// Restores this ball's state from XML including position, transform, tags, physics velocity, and sprite color.
    /// Called by GameStateSerializer after OnStart() so components are guaranteed to exist.
    /// </summary>
    public void LoadState(XElement element)
    {
        // Restore position
        var positionElement = element.Element("Position");
        if (positionElement != null)
        {
            if (float.TryParse(positionElement.Attribute("X")?.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out float x) &&
                float.TryParse(positionElement.Attribute("Y")?.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out float y))
            {
                Position = new Vector2(x, y);
            }
        }

        // Restore rotation
        if (float.TryParse(element.Attribute("Rotation")?.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out float rotation))
        {
            Rotation = rotation;
        }

        // Restore scale
        var scaleElement = element.Element("Scale");
        if (scaleElement != null)
        {
            if (float.TryParse(scaleElement.Attribute("X")?.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out float scaleX) &&
                float.TryParse(scaleElement.Attribute("Y")?.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out float scaleY))
            {
                Scale = new Vector2(scaleX, scaleY);
            }
        }

        // Restore sort order
        if (int.TryParse(element.Attribute("Sort")?.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out int sortOrder))
        {
            SetSort(sortOrder);
        }

        // Restore active state
        if (bool.TryParse(element.Attribute("Active")?.Value, out bool active))
        {
            SetActive(active);
        }

        // Restore tags
        var tagsElement = element.Element("Tags");
        if (tagsElement != null)
        {
            foreach (var tag in Tags.ToList())
            {
                RemoveTag(tag);
            }

            foreach (var tagElement in tagsElement.Elements("Tag"))
            {
                var tagName = tagElement.Attribute("Name")?.Value;
                if (!string.IsNullOrWhiteSpace(tagName))
                {
                    SetTag(tagName);
                }
            }
        }

        // Restore physics velocity — body exists since OnStart ran
        // (position/rotation sync happens automatically on first Update via component)
        var physics = element.Element("Physics");
        if (physics != null && _rigidbodyComponent.IsBodyCreated)
        {
            if (float.TryParse(physics.Attribute("LinearVelocityX")?.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out float velX) &&
                float.TryParse(physics.Attribute("LinearVelocityY")?.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out float velY))
            {
                _rigidbodyComponent.SetLinearVelocity(new Vector2(velX, velY));
            }

            if (float.TryParse(physics.Attribute("AngularVelocity")?.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out float angVel))
            {
                _rigidbodyComponent.AngularVelocity = angVel;
            }
        }

        // Restore sprite color — component exists since OnStart ran
        var sprite = element.Element("Sprite");
        if (sprite != null && _spriteComponent != null)
        {
            var colorAttr = sprite.Attribute("Color")?.Value;
            if (colorAttr != null && uint.TryParse(colorAttr, out uint argb))
            {
                _spriteComponent.Color = new Microsoft.Xna.Framework.Color(argb);
            }
        }
    }
}
