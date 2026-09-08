using System;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using CoreEssentials.Assets;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Components;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Components.BuiltIn;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Serialization;
using CoreEssentials.GameSystems.Physics.Types;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace CoreEssentials.Playground.Components;

/// <summary>
/// The playground's save component for the physics-demo ball. Implements <see cref="ISaveableComponent"/>
/// so a plain <c>GameObjectEntity</c> is saveable purely by having this component attached — no entity
/// subclass required. It explicitly serializes exactly what a ball needs to restore: the owner's
/// transform (position/rotation/scale/sort/active), its tags, and the public properties of the sibling
/// components it cares about — sprite color/asset (<c>&lt;SpriteState/&gt;</c>), rigidbody mass + velocity
/// (<c>&lt;RigidbodyState/&gt;</c>), and collider settings (<c>&lt;ColliderState/&gt;</c>). Each value is read by
/// name from the component's public properties; there is no per-component serialization interface.
/// <para>
/// Declared purely from data:
/// </para>
/// <code>
/// &lt;Component Type="BallSaveComponent" /&gt;
/// </code>
/// </summary>
public class BallSaveComponent : EntityComponent, ISaveableComponent
{
    /// <summary>
    /// Saves the owner's state: transform, tags, and the specific state of the components a ball needs
    /// to restore — sprite color/asset (<c>&lt;SpriteState/&gt;</c>), rigidbody mass + velocity
    /// (<c>&lt;RigidbodyState/&gt;</c>), and collider settings (<c>&lt;ColliderState/&gt;</c>). Each value is read by
    /// name from the component's public properties. Returns the full <c>&lt;Entity&gt;</c> element; the
    /// framework serializer stamps the authoritative <c>Prefab</c> attribute and appends children.
    /// </summary>
    public XElement SaveState()
    {
        var owner = Owner;
        var element = new XElement("Entity",
            new XAttribute("Id", owner.Id ?? string.Empty),
            new XAttribute("Type", owner.GetType().FullName ?? string.Empty),
            new XAttribute("Rotation", owner.Rotation.ToString(CultureInfo.InvariantCulture)),
            new XAttribute("Sort", owner.GetSort()),
            new XAttribute("Active", owner.GetActive()),
            new XElement("Position",
                new XAttribute("X", owner.Position.X.ToString(CultureInfo.InvariantCulture)),
                new XAttribute("Y", owner.Position.Y.ToString(CultureInfo.InvariantCulture))
            ),
            new XElement("Scale",
                new XAttribute("X", owner.Scale.X.ToString(CultureInfo.InvariantCulture)),
                new XAttribute("Y", owner.Scale.Y.ToString(CultureInfo.InvariantCulture))
            ),
            new XElement("Tags",
                owner.Tags.Select(tag => new XElement("Tag", new XAttribute("Name", tag)))
            )
        );

        // Explicit per-component serialization: each component a ball needs to restore contributes its
        // own element, read by name from the component's public properties.
        if (owner.TryGetComponent<SpriteComponent>(out var sprite) && sprite != null)
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

        if (owner.TryGetComponent<RigidbodyComponent>(out var rigidbody) && rigidbody != null)
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

        if (owner.TryGetComponent<ColliderComponent>(out var collider) && collider != null)
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
    /// Restores the owner's state from XML: transform, tags, and the specific component state (sprite
    /// color/asset, rigidbody mass + velocity, collider settings). Called by the serializer after
    /// instantiation from the prefab, so the sibling components are guaranteed to exist. The rigidbody
    /// body is created at the RESTORED position before its velocity is applied, so the physics world
    /// re-anchors to the saved state (and the sync snapshot is seeded correctly, avoiding a false
    /// "external move").
    /// </summary>
    public void LoadState(XElement element)
    {
        RestorePosition(element);
        RestoreRotation(element);
        RestoreScale(element);
        RestoreSortOrder(element);
        RestoreActiveState(element);
        RestoreTags(element);

        // Explicit per-component deserialization. The rigidbody body is created at the RESTORED position
        // before its velocity is applied, so the physics world re-anchors to the saved state.
        RestoreSpriteState(element);
        RestoreRigidbodyState(element);
        RestoreColliderState(element);

        // A deserialized sprite component carries its asset name but not the loaded sprite — load it now
        // so the entity renders (and the collider's per-frame auto-size can measure it).
        ReloadSpriteIfMissing();
    }

    /// <summary>Restores the owner's <see cref="SpriteComponent"/> color, origin, effects and layout from XML.</summary>
    private void RestoreSpriteState(XElement element)
    {
        var spriteEl = element.Element("SpriteState");
        if (spriteEl == null || !Owner.TryGetComponent<SpriteComponent>(out var sprite) || sprite == null)
            return;

        byte r = byte.Parse(spriteEl.Attribute("ColorR")?.Value ?? "255");
        byte g = byte.Parse(spriteEl.Attribute("ColorG")?.Value ?? "255");
        byte b = byte.Parse(spriteEl.Attribute("ColorB")?.Value ?? "255");
        byte a = byte.Parse(spriteEl.Attribute("ColorA")?.Value ?? "255");
        sprite.Color = new Color(r, g, b, a);

        float ox = float.Parse(spriteEl.Attribute("OriginX")?.Value ?? "0.5");
        float oy = float.Parse(spriteEl.Attribute("OriginY")?.Value ?? "0.5");
        sprite.Origin = new Vector2(ox, oy);

        string effectsStr = spriteEl.Attribute("Effects")?.Value ?? "";
        if (!string.IsNullOrEmpty(effectsStr) && Enum.TryParse<SpriteEffects>(effectsStr, out var fx))
            sprite.Effects = fx;

        string ldStr = spriteEl.Attribute("LayerDepth")?.Value ?? "0";
        if (!string.IsNullOrEmpty(ldStr))
            sprite.LayerDepth = float.Parse(ldStr);

        string soStr = spriteEl.Attribute("SortOrderOverride")?.Value ?? "-1";
        sprite.SortOrderOverride = int.TryParse(soStr, out int so) && so >= 0 ? so : null;

        string afStr = spriteEl.Attribute("AnimationFrame")?.Value ?? "0";
        if (!string.IsNullOrEmpty(afStr))
            sprite.AnimationFrame = int.Parse(afStr);

        sprite.SpriteAsset = spriteEl.Attribute("SpriteAsset")?.Value ?? "";
    }

    /// <summary>Restores the owner's <see cref="RigidbodyComponent"/> mass, flags and velocity from XML.</summary>
    private void RestoreRigidbodyState(XElement element)
    {
        var rbEl = element.Element("RigidbodyState");
        if (rbEl == null || !Owner.TryGetComponent<RigidbodyComponent>(out var rigidbody) || rigidbody == null)
            return;

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

        string? lvx = rbEl.Attribute("LinearVelocityX")?.Value;
        string? lvy = rbEl.Attribute("LinearVelocityY")?.Value;
        if (!string.IsNullOrEmpty(lvx) && !string.IsNullOrEmpty(lvy))
            rigidbody.SetLinearVelocity(new Vector2(float.Parse(lvx), float.Parse(lvy)));

        string? av = rbEl.Attribute("AngularVelocity")?.Value;
        if (!string.IsNullOrEmpty(av))
            rigidbody.AngularVelocity = float.Parse(av);
    }

    /// <summary>Restores the owner's <see cref="ColliderComponent"/> material, categories and shape from XML.</summary>
    private void RestoreColliderState(XElement element)
    {
        var colEl = element.Element("ColliderState");
        if (colEl == null || !Owner.TryGetComponent<ColliderComponent>(out var collider) || collider == null)
            return;

        string fricStr = colEl.Attribute("Friction")?.Value ?? "0.5";
        if (!string.IsNullOrEmpty(fricStr))
            collider.Friction = float.Parse(fricStr);

        string restStr = colEl.Attribute("Restitution")?.Value ?? "0.5";
        if (!string.IsNullOrEmpty(restStr))
            collider.Restitution = float.Parse(restStr);

        string? catStr = colEl.Attribute("Categories")?.Value;
        if (!string.IsNullOrWhiteSpace(catStr) && Enum.TryParse<CollisionCategory>(catStr, ignoreCase: true, out var cats))
            collider.Categories = cats;

        string? cwStr = colEl.Attribute("CollidesWith")?.Value;
        if (!string.IsNullOrWhiteSpace(cwStr) && Enum.TryParse<CollisionCategory>(cwStr, ignoreCase: true, out var cws))
            collider.CollidesWith = cws;

        float offX = float.Parse(colEl.Attribute("OffsetX")?.Value ?? "0");
        float offY = float.Parse(colEl.Attribute("OffsetY")?.Value ?? "0");
        collider.Offset = new Vector2(offX, offY);

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
            collider.Size = new Vector2(sx, sy);
        }
    }

    /// <summary>Loads the sprite asset for a deserialized sprite component that has an asset but no loaded sprite.</summary>
    private void ReloadSpriteIfMissing()
    {
        if (!Owner.TryGetComponent<SpriteComponent>(out var spriteComp) || spriteComp == null
            || spriteComp.Sprite != null || string.IsNullOrWhiteSpace(spriteComp.SpriteAsset))
            return;

        try
        {
            spriteComp.Sprite = AssetManager.LoadAsset<Sprite>(spriteComp.SpriteAsset);
            Owner.RegisterForInstancedRendering(spriteComp.Sprite);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[BallSaveComponent] Could not load sprite asset '{spriteComp.SpriteAsset}': {ex.Message}");
        }
    }

    private void RestorePosition(XElement element)
    {
        var positionElement = element.Element("Position");
        if (positionElement != null &&
            float.TryParse(positionElement.Attribute("X")?.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out float x) &&
            float.TryParse(positionElement.Attribute("Y")?.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out float y))
        {
            Owner.Position = new Vector2(x, y);
        }
    }

    private void RestoreRotation(XElement element)
    {
        if (float.TryParse(element.Attribute("Rotation")?.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out float rotation))
        {
            Owner.Rotation = rotation;
        }
    }

    private void RestoreScale(XElement element)
    {
        var scaleElement = element.Element("Scale");
        if (scaleElement != null &&
            float.TryParse(scaleElement.Attribute("X")?.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out float scaleX) &&
            float.TryParse(scaleElement.Attribute("Y")?.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out float scaleY))
        {
            Owner.Scale = new Vector2(scaleX, scaleY);
        }
    }

    private void RestoreSortOrder(XElement element)
    {
        if (int.TryParse(element.Attribute("Sort")?.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out int sortOrder))
        {
            Owner.SetSort(sortOrder);
        }
    }

    private void RestoreActiveState(XElement element)
    {
        if (bool.TryParse(element.Attribute("Active")?.Value, out bool active))
        {
            Owner.SetActive(active);
        }
    }

    private void RestoreTags(XElement element)
    {
        var tagsElement = element.Element("Tags");
        if (tagsElement == null) return;

        foreach (var tag in Owner.Tags.ToList())
        {
            Owner.RemoveTag(tag);
        }

        foreach (var tagElement in tagsElement.Elements("Tag"))
        {
            var tagName = tagElement.Attribute("Name")?.Value;
            if (!string.IsNullOrWhiteSpace(tagName))
            {
                Owner.SetTag(tagName);
            }
        }
    }
}
