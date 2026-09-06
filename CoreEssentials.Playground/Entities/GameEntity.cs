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
using CoreEssentials.GameSystems.Physics.Types;
using CoreEssentials.Utils;

#nullable enable

namespace CoreEssentials.Playground.Entities;

/// <summary>
/// A thin, behavior-light entity that implements <see cref="ISaveableEntity"/> with EXPLICIT
/// serialization: it saves its transform (position/rotation/scale/sort/active), its tags, and the
/// specific state of the components a ball needs to restore — sprite color/asset, rigidbody mass +
/// velocity, and collider settings. Each value is read by name from the component's public
/// properties; there is no per-component serialization interface.
/// <para>
/// This exists because the framework's own <see cref="GameObjectEntity"/> cannot be extended without
/// modifying framework code; the physics-demo ball is declared as this type so its state (including
/// physics velocity and sprite color) survives save/load through the explicit path.
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
    /// Saves this entity's state: transform, tags, and the specific state of the components a ball
    /// needs to restore — sprite color/asset (<c>&lt;SpriteState/&gt;</c>), rigidbody mass + velocity
    /// (<c>&lt;RigidbodyState/&gt;</c>), and collider settings (<c>&lt;ColliderState/&gt;</c>). Each
    /// value is read by name from the component's public properties.
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

        // Explicit per-component serialization: each component a ball needs to restore contributes
        // its own element, read by name from the component's public properties.
        if (TryGetComponent<SpriteComponent>(out var sprite) && sprite != null)
        {
            element.Add(new XElement("SpriteState",
                new XAttribute("ColorR", sprite.Color.R),
                new XAttribute("ColorG", sprite.Color.G),
                new XAttribute("ColorB", sprite.Color.B),
                new XAttribute("ColorA", sprite.Color.A),
                new XAttribute("OriginX", sprite.Origin.X.ToString(CultureInfo.InvariantCulture)),
                new XAttribute("OriginY", sprite.Origin.Y.ToString(CultureInfo.InvariantCulture)),
                new XAttribute("Effects", sprite.Effects.ToString()),
                new XAttribute("LayerDepth", sprite.LayerDepth.ToString(CultureInfo.InvariantCulture)),
                new XAttribute("SortOrderOverride", sprite.SortOrderOverride.HasValue ? sprite.SortOrderOverride.Value.ToString(CultureInfo.InvariantCulture) : "-1"),
                new XAttribute("AnimationFrame", sprite.AnimationFrame),
                new XAttribute("SpriteAsset", sprite.SpriteAsset ?? "")
            ));
        }

        if (TryGetComponent<RigidbodyComponent>(out var rigidbody) && rigidbody != null)
        {
            element.Add(new XElement("RigidbodyState",
                new XAttribute("Type", rigidbody.Type.ToString()),
                new XAttribute("Mass", rigidbody.Mass.ToString(CultureInfo.InvariantCulture)),
                new XAttribute("FixedRotation", rigidbody.FixedRotation),
                new XAttribute("SyncFromPhysics", rigidbody.SyncFromPhysics),
                new XAttribute("LinearVelocityX", rigidbody.LinearVelocity.X.ToString(CultureInfo.InvariantCulture)),
                new XAttribute("LinearVelocityY", rigidbody.LinearVelocity.Y.ToString(CultureInfo.InvariantCulture)),
                new XAttribute("AngularVelocity", rigidbody.AngularVelocity.ToString(CultureInfo.InvariantCulture))
            ));
        }

        if (TryGetComponent<ColliderComponent>(out var collider) && collider != null)
        {
            element.Add(new XElement("ColliderState",
                new XAttribute("ShapeType", collider.ShapeType.ToString()),
                new XAttribute("Friction", collider.Friction.ToString(CultureInfo.InvariantCulture)),
                new XAttribute("Restitution", collider.Restitution.ToString(CultureInfo.InvariantCulture)),
                new XAttribute("Categories", collider.Categories.ToString()),
                new XAttribute("CollidesWith", collider.CollidesWith.ToString()),
                new XAttribute("OffsetX", collider.Offset.X.ToString(CultureInfo.InvariantCulture)),
                new XAttribute("OffsetY", collider.Offset.Y.ToString(CultureInfo.InvariantCulture)),
                collider.ShapeType == ColliderShapeType.Circle ? new XAttribute("Radius", collider.Radius.ToString(CultureInfo.InvariantCulture)) : null,
                collider.ShapeType == ColliderShapeType.Rectangle ? new XAttribute("SizeX", collider.Size.X.ToString(CultureInfo.InvariantCulture)) : null,
                collider.ShapeType == ColliderShapeType.Rectangle ? new XAttribute("SizeY", collider.Size.Y.ToString(CultureInfo.InvariantCulture)) : null
            ));
        }

        return element;
    }

    /// <summary>
    /// Restores this entity's state from XML: transform, tags, and the specific component state
    /// (sprite color/asset, rigidbody mass + velocity, collider settings). Called by the serializer
    /// after <see cref="OnStart"/>, so the built-in components are guaranteed to exist.
    /// </summary>
    public void LoadState(XElement element)
    {
        RestorePosition(element);
        RestoreRotation(element);
        RestoreScale(element);
        RestoreSortOrder(element);
        RestoreActiveState(element);
        RestoreTags(element);

        // Explicit per-component deserialization. The rigidbody body is created at the RESTORED
        // position before its velocity is applied, so the physics world re-anchors to the saved
        // state (and the sync snapshot is seeded correctly, avoiding a false "external move").
        var spriteEl = element.Element("SpriteState");
        if (spriteEl != null && TryGetComponent<SpriteComponent>(out var sprite) && sprite != null)
        {
            byte r = byte.Parse(spriteEl.Attribute("ColorR")?.Value ?? "255");
            byte g = byte.Parse(spriteEl.Attribute("ColorG")?.Value ?? "255");
            byte b = byte.Parse(spriteEl.Attribute("ColorB")?.Value ?? "255");
            byte a = byte.Parse(spriteEl.Attribute("ColorA")?.Value ?? "255");
            sprite.Color = new Microsoft.Xna.Framework.Color(r, g, b, a);

            float ox = float.Parse(spriteEl.Attribute("OriginX")?.Value ?? "0.5");
            float oy = float.Parse(spriteEl.Attribute("OriginY")?.Value ?? "0.5");
            sprite.Origin = new Microsoft.Xna.Framework.Vector2(ox, oy);

            string effectsStr = spriteEl.Attribute("Effects")?.Value ?? "";
            if (!string.IsNullOrEmpty(effectsStr) && Enum.TryParse<Microsoft.Xna.Framework.Graphics.SpriteEffects>(effectsStr, out var fx))
                sprite.Effects = fx;

            string ldStr = spriteEl.Attribute("LayerDepth")?.Value ?? "0";
            if (!string.IsNullOrEmpty(ldStr))
                sprite.LayerDepth = float.Parse(ldStr);

            string soStr = spriteEl.Attribute("SortOrderOverride")?.Value ?? "-1";
            if (int.TryParse(soStr, out int so) && so >= 0)
                sprite.SortOrderOverride = so;
            else
                sprite.SortOrderOverride = null;

            string afStr = spriteEl.Attribute("AnimationFrame")?.Value ?? "0";
            if (!string.IsNullOrEmpty(afStr))
                sprite.AnimationFrame = int.Parse(afStr);

            sprite.SpriteAsset = spriteEl.Attribute("SpriteAsset")?.Value ?? "";
        }

        var rbEl = element.Element("RigidbodyState");
        if (rbEl != null && TryGetComponent<RigidbodyComponent>(out var rigidbody) && rigidbody != null)
        {
            string massStr = rbEl.Attribute("Mass")?.Value ?? "1.0";
            if (!string.IsNullOrEmpty(massStr))
                rigidbody.Mass = float.Parse(massStr);

            string frStr = rbEl.Attribute("FixedRotation")?.Value ?? "false";
            if (!string.IsNullOrEmpty(frStr))
                rigidbody.FixedRotation = bool.Parse(frStr);

            string sfStr = rbEl.Attribute("SyncFromPhysics")?.Value ?? "true";
            if (!string.IsNullOrEmpty(sfStr))
                rigidbody.SyncFromPhysics = bool.Parse(sfStr);

            // Ensure the body exists at the restored position before velocity is applied.
            if (!rigidbody.IsBodyCreated)
                rigidbody.CreateBody();

            string lvx = rbEl.Attribute("LinearVelocityX")?.Value;
            string lvy = rbEl.Attribute("LinearVelocityY")?.Value;
            if (!string.IsNullOrEmpty(lvx) && !string.IsNullOrEmpty(lvy))
                rigidbody.SetLinearVelocity(new Microsoft.Xna.Framework.Vector2(float.Parse(lvx), float.Parse(lvy)));

            string av = rbEl.Attribute("AngularVelocity")?.Value;
            if (!string.IsNullOrEmpty(av))
                rigidbody.AngularVelocity = float.Parse(av);
        }

        var colEl = element.Element("ColliderState");
        if (colEl != null && TryGetComponent<ColliderComponent>(out var collider) && collider != null)
        {
            string fricStr = colEl.Attribute("Friction")?.Value ?? "0.5";
            if (!string.IsNullOrEmpty(fricStr))
                collider.Friction = float.Parse(fricStr);

            string restStr = colEl.Attribute("Restitution")?.Value ?? "0.5";
            if (!string.IsNullOrEmpty(restStr))
                collider.Restitution = float.Parse(restStr);

            string catStr = colEl.Attribute("Categories")?.Value;
            if (!string.IsNullOrWhiteSpace(catStr) && Enum.TryParse<CollisionCategory>(catStr, ignoreCase: true, out var cats))
                collider.Categories = cats;

            string cwStr = colEl.Attribute("CollidesWith")?.Value;
            if (!string.IsNullOrWhiteSpace(cwStr) && Enum.TryParse<CollisionCategory>(cwStr, ignoreCase: true, out var cws))
                collider.CollidesWith = cws;

            float offX = float.Parse(colEl.Attribute("OffsetX")?.Value ?? "0");
            float offY = float.Parse(colEl.Attribute("OffsetY")?.Value ?? "0");
            collider.Offset = new Microsoft.Xna.Framework.Vector2(offX, offY);

            if (collider.ShapeType == ColliderShapeType.Circle)
            {
                string radStr = colEl.Attribute("Radius")?.Value ?? "1";
                if (!string.IsNullOrEmpty(radStr))
                    collider.Radius = float.Parse(radStr);
            }
            else if (collider.ShapeType == ColliderShapeType.Rectangle)
            {
                float sx = float.Parse(colEl.Attribute("SizeX")?.Value ?? "1");
                float sy = float.Parse(colEl.Attribute("SizeY")?.Value ?? "1");
                collider.Size = new Microsoft.Xna.Framework.Vector2(sx, sy);
            }
        }

        // A deserialized sprite component carries its asset name but not the loaded sprite — load it
        // now so the entity renders (and the collider's per-frame auto-size can measure it).
        if (TryGetComponent<SpriteComponent>(out var spriteComp) && spriteComp != null
            && spriteComp.Sprite == null && !string.IsNullOrWhiteSpace(spriteComp.SpriteAsset))
        {
            try
            {
                spriteComp.Sprite = AssetManager.LoadAsset<Sprite>(spriteComp.SpriteAsset);
                RegisterForInstancedRendering(spriteComp.Sprite);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GameEntity] Could not load sprite asset '{spriteComp.SpriteAsset}': {ex.Message}");
            }
        }
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
