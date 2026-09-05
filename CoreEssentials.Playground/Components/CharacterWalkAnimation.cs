using System;
using System.Linq;
using CoreEssentials.Assets;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Components;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Components.BuiltIn;
using Microsoft.Xna.Framework;

namespace CoreEssentials.Playground.Components;

/// <summary>
/// Plays a named walk-cycle animation on the owning entity — the behavior that used to live inline
/// in the deleted <c>AnimatedCharacterEntity</c>. It loads the animated sprite asset and, on the first
/// <see cref="Update"/>, hands it to two pre-declared siblings: the <see cref="SpriteComponent"/>
/// (which owns rendering) and the built-in <see cref="AnimationComponent"/> (a pure controller that
/// advances frames into the sprite component). A component must never <em>create</em> a sibling from
/// <c>OnAttach</c>/<c>Update</c> — the entity iterates its live component collection during both
/// passes, so adding one throws. Both siblings are therefore declared on the entity in XML; this
/// component only configures them (property sets + method calls). If either is missing it is skipped.
/// </summary>
public class CharacterWalkAnimation : EntityComponent
{
    /// <summary>Asset name of the animated sprite (e.g. "Sprites/character_anim_walk.xml").</summary>
    public string SpriteAsset { get; set; } = "";

    /// <summary>Name registered on the AnimationComponent and played immediately (default "walk").</summary>
    public string AnimationName { get; set; } = "walk";

    private Sprite? _sprite;
    private bool _wired;

    /// <inheritdoc />
    public override void OnAttach()
    {
        if (string.IsNullOrWhiteSpace(SpriteAsset))
            return;

        try
        {
            _sprite = AssetManager.LoadAsset<Sprite>(SpriteAsset);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[CharacterWalkAnimation] Could not load sprite '{SpriteAsset}': {ex.Message}");
        }
    }

    /// <inheritdoc />
    public override void Update(GameTime gameTime)
    {
        if (_wired || Owner == null || _sprite == null)
            return;

        // Configure the pre-declared SpriteComponent for rendering (property set only).
        var spriteComponent = Owner.GetComponent<SpriteComponent>();
        if (spriteComponent != null)
            spriteComponent.Sprite = _sprite;

        // Register + play the named animation on the pre-declared built-in controller.
        var animation = Owner.GetComponent<AnimationComponent>();
        if (animation != null && !animation.Animations.Contains(AnimationName))
        {
            animation.AddAnimation(AnimationName, _sprite);
            animation.Play(AnimationName);
        }

        _wired = true;
    }
}
