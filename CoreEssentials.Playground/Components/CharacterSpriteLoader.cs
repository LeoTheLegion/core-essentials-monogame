using System;
using CoreEssentials.Assets;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Components;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Components.BuiltIn;
using Microsoft.Xna.Framework;

namespace CoreEssentials.Playground.Components;

/// <summary>
/// Loads a sprite asset and hands it to the owning entity's sibling <see cref="SpriteComponent"/> so
/// a character can be rendered purely from data (no per-entity C#). The asset is loaded in
/// <see cref="OnAttach"/> (once the <c>AssetManager</c> is up) and assigned to the existing
/// <see cref="SpriteComponent"/> on the first <see cref="Update"/>. A component must never
/// <em>create</em> a sibling from <c>OnAttach</c>/<c>Update</c> (the entity iterates its live
/// component collection during both passes, so adding one throws), which is why the
/// <see cref="SpriteComponent"/> is declared on the entity in XML alongside this loader — the loader
/// only configures it. If no <see cref="SpriteComponent"/> is present the loader is a no-op.
/// </summary>
public class CharacterSpriteLoader : EntityComponent
{
    /// <summary>Asset name of the sprite to load (e.g. "Sprites/character_sprite.xml").</summary>
    public string SpriteAsset { get; set; } = "";

    private Sprite? _sprite;
    private bool _mounted;

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
            Console.WriteLine($"[CharacterSpriteLoader] Could not load sprite '{SpriteAsset}': {ex.Message}");
        }
    }

    /// <inheritdoc />
    public override void Update(GameTime gameTime)
    {
        if (_mounted || Owner == null || _sprite == null)
            return;

        // Configure the pre-declared SpriteComponent (property set only — never create a sibling here).
        var spriteComponent = Owner.GetComponent<SpriteComponent>();
        if (spriteComponent != null)
            spriteComponent.Sprite = _sprite;

        _mounted = true;
    }
}
