using System;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem;
using CoreEssentials.GUI;
using CoreEssentials.Audio;
using CoreEssentials.GUI.Factory;
using Microsoft.Xna.Framework;

namespace CoreEssentials.Playground.Entities;

/// <summary>
/// An entity that displays volume control buttons using the Canvas wrapper for Myra UI.
/// </summary>
public class VolumeButtonEntity : Entity
{
    private Canvas _canvas;
    private float _volumeLevel;
    private string _buttonText = "";
    private bool _configured;

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
    /// The master volume (0.0–1.0) the button sets when clicked. Settable from scene XML via
    /// &lt;EntityOverrides&gt;; wired up in <see cref="OnStart"/>.
    /// </summary>
    public float VolumeLevel
    {
        get => _volumeLevel;
        set => _volumeLevel = value;
    }

    /// <summary>
    /// The button's display text. Settable from scene XML via &lt;EntityOverrides&gt;;
    /// wired up in <see cref="OnStart"/>.
    /// </summary>
    public string ButtonText
    {
        get => _buttonText;
        set => _buttonText = value;
    }

    /// <summary>
    /// Configures the button with volume level and text.
    /// </summary>
    public void Configure(float volumeLevel, string buttonText)
    {
        _volumeLevel = volumeLevel;
        _buttonText = buttonText;
        _configured = true;

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

    /// <summary>
    /// Wires up the button when loaded from data (Scene-as-Data, Sprint 5d): if a volume level and
    /// text were set via &lt;EntityOverrides&gt; before OnStart, configure the widget now. Entities
    /// constructed with the (position, volume, text) constructor are already configured, so this is a
    /// no-op for them.
    /// </summary>
    public override void OnStart()
    {
        base.OnStart();

        // Only configure here when the values arrived via <EntityOverrides> (data-driven load).
        // Constructor-created entities already ran Configure and are flagged _configured.
        if (!_configured && !string.IsNullOrEmpty(_buttonText))
            Configure(_volumeLevel, _buttonText);
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
