using System;
using System.Xml.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using CoreEssentials.Assets;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Components;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Serialization;

namespace CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Components.BuiltIn;

/// <summary>
/// Component that provides sprite-based rendering for an entity.
/// In the hybrid rendering model, this component provides an additional draw path
/// alongside the existing Entity.Render() method.
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
    /// Gets or sets an explicit MonoGame <see cref="Effect"/> to render this sprite with.
    /// When set, it takes precedence over <see cref="EffectAsset"/> (same precedence as an explicit
    /// <see cref="Sprite"/> over <see cref="SpriteAsset"/>). In MonoGame an effect is applied at
    /// <c>SpriteBatch.Begin</c>, so the render pipeline groups entities by effect and opens a
    /// dedicated Begin/End for each distinct effect — leaving the no-effect case unchanged.
    /// </summary>
    public Effect? Effect { get; set; }

    /// <summary>
    /// Gets or sets the asset name of an <see cref="Effect"/> to load via the <see cref="AssetManager"/>
    /// (e.g. "Effects/glow.xml"). Resolved once in <see cref="OnAttach"/> and assigned to
    /// <see cref="Effect"/> — but only when no explicit <see cref="Effect"/> was set already, so an
    /// effect assigned in code always wins. This lets data-driven (XML) entities declare a shader with
    /// a plain string property instead of needing a per-game loader component to bridge the gap.
    /// </summary>
    public string EffectAsset { get; set; } = "";

    /// <summary>
    /// Gets the effective <see cref="Effect"/> for this sprite: the explicit <see cref="Effect"/> when
    /// set, otherwise the effect resolved from <see cref="EffectAsset"/>. Returns null when neither is
    /// set, in which case the sprite renders with the SpriteBatch's default (no shader) — preserving
    /// current behavior and batching exactly.
    /// </summary>
    public Effect? EffectiveEffect => Effect ?? _resolvedEffect;

    private Effect? _resolvedEffect;

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

        // Resolve the declarative effect, unless an explicit Effect was already assigned in code.
        if (!string.IsNullOrWhiteSpace(EffectAsset) && Effect == null)
        {
            try
            {
                _resolvedEffect = AssetManager.LoadAsset<EffectAsset>(EffectAsset).Effect;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SpriteComponent] Could not load effect asset '{EffectAsset}': {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Clears the resolved <see cref="EffectAsset"/> so a re-attached component can resolve it again.
    /// An explicitly assigned <see cref="Effect"/> is left untouched (it is code-owned).
    /// </summary>
    public override void OnDetach()
    {
        base.OnDetach();
        _resolvedEffect = null;
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
