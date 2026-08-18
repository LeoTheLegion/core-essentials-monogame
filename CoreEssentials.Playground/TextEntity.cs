using System;
using CoreEssentials.Assets;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace CoreEssentials.Playground;

public class TextEntity : Entity
{
    private FontAsset _font;
    private Vector2 _offset;

    public enum TextAlignment
    {
        Left,
        Center,
        Right
    }

    /// <summary>Gets or sets the text to render.</summary>
    public string Text { get; set; } = "";

    /// <summary>Gets or sets the color to render the text with.</summary>
    public Color Color { get; set; } = Color.White;

    /// <summary>Gets or sets the text alignment.</summary>
    public TextAlignment Alignment { get; set; } = TextAlignment.Left;

    // Parameterless constructor for XML-based entity loading
    public TextEntity() : base()
    {
        _offset = Vector2.Zero;
    }
    
    // Constructor that matches what's being passed in CharacterScene.cs
    public TextEntity(Vector2 position, string text, Color color, TextAlignment alignment) : base()
    {
        Position = position;
        Text = text;
        Color = color;
        Alignment = alignment;
        _offset = Vector2.Zero;
    }
    
    public override void OnStart()
    {
        base.OnStart();
        
        // Load the font asset
        _font = AssetManager.LoadAsset<FontAsset>("base");
    }

    public override void Render(SpriteBatch _spriteBatch)
    {
        base.Render(_spriteBatch);
        
        if (_font == null || _font.Font == null)
            return;
            
        Vector2 textSize = _font.MeasureStringVector(Text);
        Vector2 drawPosition = Position + _offset;
        
        // Apply alignment
        switch (Alignment)
        {
            case TextAlignment.Center:
                drawPosition.X -= textSize.X / 2;
                break;
            case TextAlignment.Right:
                drawPosition.X -= textSize.X;
                break;
        }
        
        _spriteBatch.DrawString(_font.Font, Text, drawPosition, Color);
    }
}