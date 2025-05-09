using CoreEssentials.Assets;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace CoreEssentials.Playground;

/// <summary>
/// A simple entity that displays a character from a sprite sheet.
/// </summary>
public class CharacterEntity : Entity
{
    private Sprite _sprite;
    
    public CharacterEntity(Vector2 position)
    {
        _position = position;
        
        // Load the character sprite that references the sprite sheet
        _sprite = AssetManager.LoadAsset<Sprite>("character_sprite.xml");
    }
    
    public override void OnStart()
    {
        base.OnStart();
        Console.WriteLine("Character entity created!");
    }
    
    public override void Render(SpriteBatch spriteBatch)
    {
        // Draw the character with the current frame
        _sprite.Draw(
            spriteBatch, 
            _position, 
            Color.White, 
            0f, 
            SpriteEffects.None, 
            0f
        );
    }
}