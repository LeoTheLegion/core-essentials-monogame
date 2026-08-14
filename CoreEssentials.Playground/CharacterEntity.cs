#nullable enable

using CoreEssentials.Assets;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem;
using CoreEssentials.Tweening;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace CoreEssentials.Playground;

/// <summary>
/// A simple entity that displays a character from a sprite sheet.
/// </summary>
public class CharacterEntity : Entity
{
    private Sprite? _sprite;
    private TweenComponent? _tweenComponent;
    private TweenFloat? _yOffsetTween;
    private float _originalY;
    private bool _initialized;

    // Parameterless constructor for XML-based entity loading
    public CharacterEntity()
    {
    }

    public CharacterEntity(Vector2 position)
    {
        Position = position;
    }
    
    public override void OnStart()
    {
        base.OnStart();
        
        // Load the character sprite that references the sprite sheet
        _sprite = AssetManager.LoadAsset<Sprite>("character_sprite.xml");
        
        // Add tween component for animations
        _tweenComponent = AddComponent(new TweenComponent());
        
        // Start a Y offset tween: bounce up and down (looping with ease)
        // Note: XML position is applied AFTER OnStart, so we capture it on first Update
        _yOffsetTween = _tweenComponent.TweenToFloat(
            0f, -50f, 1.5f,
            EasingFunctions.InOutSine, // Smooth slow-in/slow-out bounce
            loop: true, reverse: true  // Ping-pong: go up, come back down, repeat
        );
        
        Console.WriteLine("Character entity created!");
    }
    
    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);

        // Capture original Y on first frame (XML position is set after OnStart)
        if (!_initialized && _yOffsetTween != null)
        {
            _originalY = Position.Y;
            _initialized = true;
        }

        // Apply the tweened Y offset relative to original spawn position
        if (_yOffsetTween != null && _initialized)
        {
            Position = new Vector2(
                Position.X,
                _originalY + _yOffsetTween.GetValue()
            );
        }
    }
    
    public override void Render(SpriteBatch spriteBatch)
    {
        // Draw the character with the current frame
        _sprite?.Draw(
            spriteBatch, 
            _position, 
            Color.White, 
            0f, 
            SpriteEffects.None, 
            0f
        );
    }
}