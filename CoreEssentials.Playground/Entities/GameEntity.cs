using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using CoreEssentials.Assets;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Components;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Components.BuiltIn;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Serialization;
using CoreEssentials.Utils;

#nullable enable

namespace CoreEssentials.Playground.Entities;

/// <summary>
/// A thin, behavior-light entity that implements <see cref="ISaveableEntity"/> with GENERIC
/// serialization: it saves its transform (position/rotation/scale/sort/active), its tags, and the
/// serialized state of every attached <see cref="ISerializableComponent"/>. Any component that
/// implements <see cref="ISerializableComponent"/> (the built-in sprite/rigidbody/collider components
/// do) round-trips for free — no per-entity save/load code needed.
/// <para>
/// This exists because the framework's own <see cref="GameObjectEntity"/> cannot be extended without
/// modifying framework code; the physics-demo ball is declared as this type so its state (including
/// physics velocity and sprite color) survives save/load through the generic path.
/// </para>
/// <para>
/// It also owns the ball's runtime component hydration in <see cref="OnStart"/> — mirroring the old
/// hand-written ball entity: on the save/load path the serializer creates a bare instance (no
/// prefab), so OnStart must be able to add any missing built-in component itself. Each addition is
/// guarded by "only if not already present". Fresh balls get a random display scale here and a
/// collider sized from the sprite; the built-in <c>ColliderComponent</c> per-frame auto-size keeps
/// the collider in step with any later scale change.
/// </para>
/// </summary>
public class GameEntity : GameObjectEntity, ISaveableEntity
{
    /// <summary>The asset name of the ball sprite, loaded when hydrating a fresh entity.</summary>
    private const string BallSpriteAsset = "Sprites/ball_sprite.xml";

    /// <summary>
    /// Maps each known serializable component type to the element name its
    /// <see cref="ISerializableComponent.SerializeToXml"/> emits, so <see cref="LoadState"/> can find
    /// the matching saved element without re-serializing. Unknown types fall back to a live
    /// <c>SerializeToXml()</c> name probe.
    /// </summary>
    private static readonly Dictionary<Type, string> StateElementNames = new()
    {
        [typeof(SpriteComponent)] = "SpriteState",
        [typeof(RigidbodyComponent)] = "RigidbodyState",
        [typeof(ColliderComponent)] = "ColliderState",
    };

    public override void OnStart()
    {
        base.OnStart();

        // Balls come in random sizes (matches the old ball entity's constructor behavior). A loaded
        // ball is unaffected: LoadState restores its saved scale after OnStart has run.
        if (Scale == Microsoft.Xna.Framework.Vector2.One)
        {
            float randomScale = GameRandom.NextFloat(0.5f, 1.5f);
            Scale = new Microsoft.Xna.Framework.Vector2(randomScale, randomScale);
        }

        // Hydrate the sprite component only if not already present (e.g., from deserialization). The
        // sprite is loaded synchronously so instanced rendering can be registered and the collider
        // sized at startup (mirrors the old ball entity).
        var spriteComponent = GetComponent<SpriteComponent>();
        if (spriteComponent == null)
        {
            Sprite? loaded = null;
            try
            {
                loaded = AssetManager.LoadAsset<Sprite>(BallSpriteAsset);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GameEntity] Could not load sprite asset '{BallSpriteAsset}': {ex.Message}");
            }

            spriteComponent = new SpriteComponent
            {
                Sprite = loaded,
                SpriteAsset = BallSpriteAsset,
                Origin = new Microsoft.Xna.Framework.Vector2(0.5f, 0.5f),
                Color = Microsoft.Xna.Framework.Color.White
            };
            AddComponent(spriteComponent);
        }
        else if (spriteComponent.Sprite == null)
        {
            // Component existed but carries no sprite (e.g., created during deserialization).
            try
            {
                var asset = string.IsNullOrWhiteSpace(spriteComponent.SpriteAsset) ? BallSpriteAsset : spriteComponent.SpriteAsset;
                spriteComponent.Sprite = AssetManager.LoadAsset<Sprite>(asset);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GameEntity] Could not load sprite asset '{spriteComponent.SpriteAsset}': {ex.Message}");
            }
        }

        var renderSprite = spriteComponent.Sprite;

        // Hydrate the rigidbody component only if not already present. Mass matches the old ball:
        // 1 * scale², evaluated at startup (a prefab's declared Mass property may override it).
        var rigidbodyComponent = GetComponent<RigidbodyComponent>();
        if (rigidbodyComponent == null)
        {
            rigidbodyComponent = new RigidbodyComponent(RigidbodyType.Dynamic);
            AddComponent(rigidbodyComponent);

            rigidbodyComponent.FixedRotation = false;
            rigidbodyComponent.Mass = 1f * Scale.X * Scale.X;
        }

        // Register the sprite for instanced rendering.
        if (renderSprite != null)
        {
            RegisterForInstancedRendering(renderSprite);
        }

        // Hydrate the collider component only if not already present, sized from the sprite like the
        // old ball entity. The built-in ColliderComponent.Update auto-resizes circle colliders to hug
        // the rendered sprite every frame, so any later scale change (VIP config or a restored save)
        // is picked up automatically.
        var colliderComponent = GetComponent<ColliderComponent>();
        if (colliderComponent == null)
        {
            float radius = 1f;
            if (renderSprite != null)
            {
                try
                {
                    radius = renderSprite.GetSize().X / 2f * Scale.X;
                }
                catch (InvalidOperationException)
                {
                    // Sprite metadata not loaded yet — the per-frame auto-size corrects it later.
                }
            }

            colliderComponent = new ColliderComponent(radius, new Microsoft.Xna.Framework.Vector2(0, 1))
            {
                Restitution = 1f
            };
            AddComponent(colliderComponent);
        }
    }

    /// <summary>
    /// Saves this entity's state: transform, tags, and the serialized state of every attached
    /// <see cref="ISerializableComponent"/> (e.g. <c>&lt;SpriteState/&gt;</c>,
    /// <c>&lt;RigidbodyState/&gt;</c>, <c>&lt;ColliderState/&gt;</c>).
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

        // Generic per-component serialization: every serializable component contributes its own element.
        foreach (var component in Components)
        {
            if (component is ISerializableComponent serializable)
            {
                element.Add(serializable.SerializeToXml());
            }
        }

        return element;
    }

    /// <summary>
    /// Restores this entity's state from XML: transform, tags, and each attached
    /// <see cref="ISerializableComponent"/> (matched by the element name its
    /// <c>SerializeToXml</c> emits). Called by the serializer after <see cref="OnStart"/>, so the
    /// built-in components are guaranteed to exist.
    /// </summary>
    public void LoadState(XElement element)
    {
        RestorePosition(element);
        RestoreRotation(element);
        RestoreScale(element);
        RestoreSortOrder(element);
        RestoreActiveState(element);
        RestoreTags(element);

        // Generic per-component deserialization. The rigidbody body is created at the RESTORED
        // position before its velocity is applied, so the physics world re-anchors to the saved
        // state (and the sync snapshot is seeded correctly, avoiding a false "external move").
        foreach (var component in Components)
        {
            if (component is not ISerializableComponent serializable) continue;

            var stateElement = element.Element(StateElementName(component));
            if (stateElement == null) continue;

            if (component is RigidbodyComponent rigidbody && !rigidbody.IsBodyCreated)
            {
                // Ensure the body exists at the restored position before velocity is applied.
                rigidbody.CreateBody();
            }

            serializable.DeserializeFromXml(stateElement);
        }

        // A deserialized sprite component carries its asset name but not the loaded sprite — load it
        // now so the entity renders (and the collider's per-frame auto-size can measure it).
        var spriteComponent = GetComponent<SpriteComponent>();
        if (spriteComponent != null && spriteComponent.Sprite == null && !string.IsNullOrWhiteSpace(spriteComponent.SpriteAsset))
        {
            try
            {
                spriteComponent.Sprite = AssetManager.LoadAsset<Sprite>(spriteComponent.SpriteAsset);
                RegisterForInstancedRendering(spriteComponent.Sprite);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GameEntity] Could not load sprite asset '{spriteComponent.SpriteAsset}': {ex.Message}");
            }
        }
    }

    /// <summary>Resolves the saved-state element name for a component type.</summary>
    private static string StateElementName(EntityComponent component)
    {
        if (StateElementNames.TryGetValue(component.GetType(), out var name))
            return name;

        // Unknown serializable component — probe its live serialization for the element name.
        if (component is ISerializableComponent serializable)
            return serializable.SerializeToXml().Name.LocalName;

        throw new InvalidOperationException($"Component '{component.GetType().Name}' is not serializable.");
    }

    private void RestorePosition(XElement element)
    {
        var positionElement = element.Element("Position");
        if (positionElement != null &&
            float.TryParse(positionElement.Attribute("X")?.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out float x) &&
            float.TryParse(positionElement.Attribute("Y")?.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out float y))
        {
            Position = new Microsoft.Xna.Framework.Vector2(x, y);
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
            Scale = new Microsoft.Xna.Framework.Vector2(scaleX, scaleY);
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
}
