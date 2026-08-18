using System;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem;
using CoreEssentials.GUI;
using CoreEssentials.Audio;
using CoreEssentials.GUI.Factory;
using Microsoft.Xna.Framework;

namespace CoreEssentials.Playground;

/// <summary>
/// An entity that displays volume control buttons using the Canvas wrapper for Myra UI.
/// </summary>
public class VolumeButtonEntity : Entity
{
    private Canvas _canvas;
    private float _volumeLevel;
    private string _buttonText;

    // Parameterless constructor for XML/template loading
    public VolumeButtonEntity()
    {
        _canvas = new Canvas();
    }

    public VolumeButtonEntity(Vector2 position, float volumeLevel, string buttonText)
    {
        _position = position;
        Configure(volumeLevel, buttonText);
    }

    /// <summary>
    /// Configures the button with volume level and text.
    /// </summary>
    public void Configure(float volumeLevel, string buttonText)
    {
        _volumeLevel = volumeLevel;
        _buttonText = buttonText;
        
        // Create a button for setting the volume via factory (returns IButton interface)
        var button = WidgetFactory.CreateTextButton(_buttonText);
        
        // Add button click handler
        button.Clicked += (b) => 
        {
            // Set the volume level when button is clicked
            AudioManager.Instance.SetMasterVolume(_volumeLevel);
            Console.WriteLine($"Volume set to {_volumeLevel * 100}%");
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
