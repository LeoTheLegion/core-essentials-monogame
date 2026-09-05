using System.Collections;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using CoreEssentials.Assets;
using CoreEssentials.Coroutines;
using CoreEssentials.Debugging;
using CoreEssentials.Utils;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Components.BuiltIn;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Serialization;
using CoreEssentials.GameSystems.Physics.Engines.Aether;
using CoreEssentials.GameSystems.Physics.Types;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

#nullable enable

namespace CoreEssentials.Playground.Entities;

public class Ball : Entity, ISaveableEntity
{
    private SpriteComponent? _spriteComponent;
    private RigidbodyComponent? _rigidbodyComponent;
    private float _radius;

    private CoroutineOwner? _coroutineOwner;

    /// <summary>
    /// Gets the rigidbody component for this ball.
    /// </summary>
    public RigidbodyComponent? RigidbodyComponent => _rigidbodyComponent;

    public Ball(Vector2 position, string? id = null)
    {
        Position = position;
        sort = 0;
        float randomScale = GameRandom.NextFloat(0.5f, 1.5f);
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
            var sprite = AssetManager.LoadAsset<Sprite>("Sprites/ball_sprite.xml");
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
                _spriteComponent.Sprite = AssetManager.LoadAsset<Sprite>("Sprites/ball_sprite.xml");
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

        // Register sprite for instanced rendering if available
        var renderSprite = _spriteComponent?.Sprite;
        if (renderSprite != null)
        {
            RegisterForInstancedRendering(renderSprite);
            _radius = renderSprite.GetSize().X / 2;
        }

        // Add collider component only if not already present
        var colliderComponent = GetComponent<ColliderComponent>();
        if (colliderComponent == null)
        {
            var colliderRadius = _radius * Scale.X;
            var colliderOffset = new Vector2(0, 1);
            colliderComponent = new ColliderComponent(colliderRadius, colliderOffset)
            {
                Restitution = 1f
            };
            AddComponent(colliderComponent);
        }

        // Start movement coroutine (base.OnStart() double-start guard prevents this from running twice on loaded entities)
        _coroutineOwner ??= new CoroutineOwner();
        _coroutineOwner.StartCoroutine(RandomMovementCoroutine());
    }

    // (Update handled by RigidbodyComponent)

    private IEnumerator RandomMovementCoroutine()
    {
        while (!Destroyed)
        {
            var direction = GameRandom.RandomVector2();
            float randomX = direction.X;
            float randomY = direction.Y;

            var impulseStrength = 500000f;
            _rigidbodyComponent?.ApplyImpulse(new Vector2(randomX, randomY) * impulseStrength);

            // Add some spin so rotation is visible
            _rigidbodyComponent?.ApplyAngularImpulse(GameRandom.NextSignedFloat() * 5f);

            yield return new WaitForSeconds(GameRandom.Next(1, 5));
        }
    }

    public override void Render(SpriteBatch _spriteBatch)
    {
        if (_spriteComponent == null) return;
        _spriteComponent.Draw(_spriteBatch);
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
            new XAttribute("Type", GetType().FullName ?? string.Empty),
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
        if (_rigidbodyComponent != null && _rigidbodyComponent.IsBodyCreated)
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
        RestorePosition(element);
        RestoreRotation(element);
        RestoreScale(element);
        RestoreSortOrder(element);
        RestoreActiveState(element);
        RestoreTags(element);

        // Restoring the entity transform is enough: RigidbodyComponent detects that the entity
        // moved externally (save/load) and adopts it as the physics source of truth on the next
        // Update, so the body integrates from the saved position. No explicit sync needed here.
        RestorePhysicsVelocity(element);
        RestoreSpriteColor(element);
    }

    private void RestorePosition(XElement element)
    {
        var positionElement = element.Element("Position");
        if (positionElement != null &&
            float.TryParse(positionElement.Attribute("X")?.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out float x) &&
            float.TryParse(positionElement.Attribute("Y")?.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out float y))
        {
            Position = new Vector2(x, y);
        }
    }

    private void RestoreRotation(XElement element)
    {
        if (float.TryParse(element.Attribute("Rotation")?.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out float rotation))
        {
            Rotation = rotation;
        }
    }

    private void RestoreScale(XElement element)
    {
        var scaleElement = element.Element("Scale");
        if (scaleElement != null &&
            float.TryParse(scaleElement.Attribute("X")?.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out float scaleX) &&
            float.TryParse(scaleElement.Attribute("Y")?.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out float scaleY))
        {
            Scale = new Vector2(scaleX, scaleY);
        }
    }

    private void RestoreSortOrder(XElement element)
    {
        if (int.TryParse(element.Attribute("Sort")?.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out int sortOrder))
        {
            SetSort(sortOrder);
        }
    }

    private void RestoreActiveState(XElement element)
    {
        if (bool.TryParse(element.Attribute("Active")?.Value, out bool active))
        {
            SetActive(active);
        }
    }

    private void RestoreTags(XElement element)
    {
        var tagsElement = element.Element("Tags");
        if (tagsElement == null) return;

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

    private void RestorePhysicsVelocity(XElement element)
    {
        var physics = element.Element("Physics");
        if (physics == null || _rigidbodyComponent == null || !_rigidbodyComponent.IsBodyCreated) return;

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

    private void RestoreSpriteColor(XElement element)
    {
        var sprite = element.Element("Sprite");
        if (sprite == null || _spriteComponent == null) return;

        var colorAttr = sprite.Attribute("Color")?.Value;
        if (colorAttr != null && uint.TryParse(colorAttr, out uint argb))
        {
            _spriteComponent.Color = new Microsoft.Xna.Framework.Color(argb);
        }
    }
}
