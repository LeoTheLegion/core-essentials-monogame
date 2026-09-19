using System;
using System.Xml.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using CoreEssentials.Assets;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Components;

namespace CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Components.BuiltIn;

/// <summary>
/// Component that provides sprite-based rendering for an entity. It owns only the *sprite* (texture,
/// origin, tint, flip, depth); all shader state — the effect and its uniforms — lives on a companion
/// <see cref="ShaderComponent"/>. This component guarantees that such a sibling exists: at attach it looks
/// for one and, when missing, auto-creates a basic one (no effect = the SpriteBatch default batch). The
/// render pipeline then resolves the shader via the entity's <see cref="ShaderComponent"/>.
/// </summary>
public class SpriteComponent : EntityComponent, IDrawableComponent
{
    /// <summary>
    /// Gets or sets the sprite to render.
    /// </summary>
    public Sprite? Sprite { get; set; }

    /// <summary>
    /// Gets or sets the asset name of a sprite to load via the <see cref="AssetManager"/> (e.g. "Sprites/hero.xml").
    /// Resolved once in <see cref="OnAttach"/> and assigned to <see cref="Sprite"/> — but only when no
    /// explicit <see cref="Sprite"/> was set already, so a sprite assigned in code always wins. This lets
    /// data-driven (XML) entities declare their visual with a plain string property instead of needing a
    /// per-game loader component to bridge the string → asset gap.
    /// </summary>
    public string SpriteAsset { get; set; } = "";

    /// <summary>
    /// Gets or sets the origin point for rotation and positioning, as a fraction of the sprite size.
    /// (0, 0) = top-left, (0.5, 0.5) = center, (1, 1) = bottom-right.
    /// Default is (0.5, 0.5) for center-origin.
    /// </summary>
    public Vector2 Origin { get; set; } = new Vector2(0.5f, 0.5f);

    /// <summary>
    /// Gets or sets the color tint applied to the sprite. Default is white (no tint).
    /// </summary>
    public Color Color { get; set; } = Color.White;

    /// <summary>
    /// Gets or sets the sprite effects (flip). Default is None.
    /// </summary>
    public SpriteEffects Effects { get; set; } = SpriteEffects.None;

    /// <summary>
    /// Gets or sets the layer depth for 3D sorting. Default is 0.
    /// </summary>
    public float LayerDepth { get; set; } = 0f;

    /// <summary>
    /// Gets or sets an optional sort order override. When set, this value is used
    /// instead of the entity's default sort order for render ordering.
    /// </summary>
    public int? SortOrderOverride { get; set; }

    /// <summary>
    /// Gets or sets the animation frame index (only applicable when Sprite uses a SpriteSheet).
    /// </summary>
    public int AnimationFrame { get; set; } = 0;

    /// <summary>
    /// Initializes a new instance of the <see cref="SpriteComponent"/> class.
    /// </summary>
    public SpriteComponent()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SpriteComponent"/> class with a sprite.
    /// </summary>
    /// <param name="sprite">The sprite to render.</param>
    public SpriteComponent(Sprite sprite)
    {
        Sprite = sprite;
    }

    /// <summary>
    /// Resolves <see cref="SpriteAsset"/> (if set) through the <see cref="AssetManager"/> and assigns it
    /// to <see cref="Sprite"/> — unless a sprite was already assigned explicitly. Runs once on attach,
    /// which is the earliest point at which XML-declared properties are final. Failures are logged and
    /// swallowed so a missing asset never breaks entity attachment.
    /// </summary>
    public override void OnAttach()
    {
        base.OnAttach();

        // Resolve the declarative sprite, unless one was already assigned in code.
        if (!string.IsNullOrWhiteSpace(SpriteAsset) && Sprite == null)
        {
            try
            {
                Sprite = AssetManager.LoadAsset<Sprite>(SpriteAsset);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SpriteComponent] Could not load sprite asset '{SpriteAsset}': {ex.Message}");
            }
        }

        // Guarantee a companion shader. On the code path (no deferred-attach window) this creates it inline;
        // on the prefab/scene path creation is deferred to the loader's finish pass — see EnsureShaderComponent.
        if (Owner != null && !Owner.DeferringComponentAttach)
            EnsureShaderComponent(Owner);
    }

    /// <summary>
    /// Idempotently guarantees this entity has a companion <see cref="ShaderComponent"/>. If one is already
    /// present it is returned untouched; otherwise a basic one (no effect = the SpriteBatch default batch) is
    /// created and attached. Safe to call from both the code path and the prefab/scene finish pass — the
    /// guard makes it a no-op when the shader already exists, so a user-declared <see cref="ShaderComponent"/>
    /// is never duplicated.
    /// </summary>
    internal static ShaderComponent EnsureShaderComponent(Entity owner)
    {
        if (owner.TryGetComponent<ShaderComponent>(out var existing) && existing != null)
            return existing;

        Console.WriteLine($"[SpriteComponent] Entity '{owner.Id}' has a SpriteComponent but no ShaderComponent — " +
            $"auto-created a basic one (no effect = the default batch). Declare <Component Type=\"ShaderComponent\"> " +
            $"(in XML or code) to set an effect or uniforms.");

        var created = new ShaderComponent();
        owner.AddComponent(created);
        return created;
    }

    /// <summary>
    /// Draws the sprite using the entity's transform.
    /// Call this method from Entity.Render() or EntitySystem.Draw() to render this component.
    /// </summary>
    /// <param name="spriteBatch">The SpriteBatch used for drawing.</param>
    public void Draw(SpriteBatch spriteBatch)
    {
        if (Sprite == null || Owner == null)
            return;

        // Draw the current animation frame (frame 0 for single-frame texture2d sprites).
        Sprite.DrawFrame(
            spriteBatch,
            Owner.Position,
            AnimationFrame,
            Color,
            Owner.Rotation,
            Owner.Scale,
            Effects,
            LayerDepth
        );
    }

    /// <summary>
    /// Gets the effective sort order for this component.
    /// Returns the SortOrderOverride if set, otherwise falls back to the entity's sort order.
    /// </summary>
    public int GetEffectiveSortOrder()
    {
        return SortOrderOverride ?? Owner.GetSort();
    }
}
