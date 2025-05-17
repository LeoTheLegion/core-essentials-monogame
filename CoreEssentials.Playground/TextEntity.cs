using System;
using CoreEssentials.Assets;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace CoreEssentials.Playground;

public class TextEntity : Entity
{
    private FontAsset _font;
    private string _text;
    private Color _color;
    private Vector2 _offset;
    private TextAlignment _alignment;
    
    public enum TextAlignment
    {
        Left,
        Center,
        Right
    }
      // Constructor that matches what's being passed in CharacterScene.cs
    // Called by entitySystem.CreateEntity<TextEntity>(position, text, color, alignment)
    public TextEntity(Vector2 position, string text, Color color, TextAlignment alignment) : base()
    {
        Position = position;
        _text = text;
        _color = color;
        _alignment = alignment;
        _offset = Vector2.Zero;
    }
    
    public override void OnStart()
    {
        base.OnStart();
        
        // Load the font asset
        _font = AssetManager.LoadAsset<FontAsset>("base");
    }
    
    public string Text 
    {
        get => _text;
        set => _text = value;
    }
    
    public Color Color
    {
        get => _color;
        set => _color = value;
    }
      public override void Render(SpriteBatch spriteBatch)
    {
        base.Render(spriteBatch);
        
        if (_font == null || _font.Font == null)
            return;
            
        Vector2 textSize = _font.MeasureStringVector(_text);
        Vector2 drawPosition = Position + _offset;
        
        // Apply alignment
        switch (_alignment)
        {
            case TextAlignment.Center:
                drawPosition.X -= textSize.X / 2;
                break;
            case TextAlignment.Right:
                drawPosition.X -= textSize.X;
                break;
        }
        
        spriteBatch.DrawString(_font.Font, _text, drawPosition, _color);
    }
}