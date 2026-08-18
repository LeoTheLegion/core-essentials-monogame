using System;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem;
using CoreEssentials.GUI;
using CoreEssentials.Audio;
using CoreEssentials.GUI.Factory;
using Microsoft.Xna.Framework;

namespace CoreEssentials.Playground;

/// <summary>
/// An entity that displays sound control buttons using the Canvas wrapper for Myra UI.
/// </summary>
public class SoundButtonEntity : Entity
{
    private Canvas _canvas;
    private string _soundAssetName;
    private string _buttonText;

    // Parameterless constructor for XML/template loading
    public SoundButtonEntity()
    {
        _canvas = new Canvas();
    }

    public SoundButtonEntity(Vector2 position, string soundAssetName, string buttonText)
    {
        _position = position;
        Configure(soundAssetName, buttonText);
    }

    /// <summary>
    /// Configures the button with sound asset and text.
    /// </summary>
    public void Configure(string soundAssetName, string buttonText)
    {
        _soundAssetName = soundAssetName;
        _buttonText = buttonText;
        
        // Create a button for playing the sound via factory (returns IButton interface)
        var button = WidgetFactory.CreateTextButton(_buttonText);
        
        // Add button click handler
        button.Clicked += (b) => 
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
