using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem;
using CoreEssentials.GUI;
using CoreEssentials.Audio;
using Microsoft.Xna.Framework;
using Myra.Graphics2D.UI;
using System;

namespace CoreEssentials.Playground;

/// <summary>
/// An entity that displays sound control buttons using the Canvas wrapper for Myra UI.
/// </summary>
public class SoundButtonEntity : Entity
{
    private Canvas _canvas;
    private string _soundAssetName;
    private string _buttonText;
      public SoundButtonEntity(Vector2 position, string soundAssetName, string buttonText)
    {
        _position = position;
        _soundAssetName = soundAssetName;
        _buttonText = buttonText;
        
        // Create canvas for Myra UI components
        _canvas = new Canvas();
        
          // Create a button for playing the sound
        var button = Button.CreateTextButton(_buttonText);
        
        // Add button click handler
        button.Click += (s, a) => 
        {
            // Play the sound effect when button is clicked
            var id = AudioManager.Instance.PlayOneShotSound(_soundAssetName);
            Console.WriteLine($"Sound played with ID: {id} from button: {_buttonText}");
        };
        
        // Add button to canvas
        _canvas.AddWidget(button);
    }
    
    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);
        
        // Set canvas position
        _canvas.SetPosition(_position);
        _canvas.Update(gameTime);
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        // Clean up the canvas when the entity is destroyed
        _canvas.CleanUp();
    }
}
