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
    private string _soundAssetName = "";
    private string _buttonText = "";
    private bool _configured;

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
    /// The asset-name string of the one-shot sound to play when the button is clicked.
    /// Settable from scene XML via &lt;EntityOverrides&gt;; wired up in <see cref="OnStart"/>.
    /// </summary>
    public string SoundAsset
    {
        get => _soundAssetName;
        set => _soundAssetName = value;
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
    /// Configures the button with sound asset and text.
    /// </summary>
    public void Configure(string soundAssetName, string buttonText)
    {
        _soundAssetName = soundAssetName;
        _buttonText = buttonText;
        _configured = true;

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

    /// <summary>
    /// Wires up the button when loaded from data (Scene-as-Data, Sprint 5d): if a sound asset and
    /// text were set via &lt;EntityOverrides&gt; before OnStart, configure the widget now. Entities
    /// constructed with the (position, asset, text) constructor are already configured, so this is a
    /// no-op for them.
    /// </summary>
    public override void OnStart()
    {
        base.OnStart();

        // Only configure here when the values arrived via <EntityOverrides> (data-driven load).
        // Constructor-created entities already ran Configure and are flagged _configured.
        if (!_configured && !string.IsNullOrEmpty(_soundAssetName))
            Configure(_soundAssetName, _buttonText);
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
