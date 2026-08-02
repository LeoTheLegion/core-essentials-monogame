using System;
using System.Collections;
using CoreEssentials.GameSystems;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem;
using CoreEssentials.Scenes;
using CoreEssentials.Inputs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using CoreEssentials.Audio;
using CoreEssentials.Assets;
using Microsoft.Xna.Framework.Graphics;

namespace CoreEssentials.Playground;

/// <summary>
/// A scene to demonstrate the SpriteSheet functionality with a character display.
/// </summary>
public class CharacterScene : Scene
{
    private string songID;
    private const string InfoText = "Press Q, W, E for sound effects | Z, X to change volume | Right Arrow for next scene | Or use the buttons on the left";
    private const string CharacterInfo = "Static Character (Left) | Animated Character (Right)";
    
    protected override GameSystem[] LoadGameSystems()
    {
        // Only need the entity system for this demo
        return new GameSystem[]
        {
            new EntitySystem()
        };
    }
    
    protected override IEnumerator OnStartCoroutine()
    {
        UpdateLoadingProgress(0.1f, "Initializing character scene...");
        yield return null;
        
        GraphicsDeviceManager graphics = SceneManager.Game.Graphics;

        // Ensure window size is appropriate
        graphics.PreferredBackBufferWidth = 1280;
        graphics.PreferredBackBufferHeight = 720;
        graphics.ApplyChanges();
        
        UpdateLoadingProgress(0.5f, "Creating characters...");
        yield return null;
        
        // Get access to the entity system
        EntitySystem entitySystem = GetGameSystem<EntitySystem>();
        
        // Create a static character entity at the left side of the screen
        entitySystem.CreateEntity<CharacterEntity>(
            new Vector2(graphics.PreferredBackBufferWidth / 4, graphics.PreferredBackBufferHeight / 2)
        );

        // Create an animated character entity at the right side of the screen
        entitySystem.CreateEntity<AnimatedCharacterEntity>(
            new Vector2(graphics.PreferredBackBufferWidth * 3 / 4, graphics.PreferredBackBufferHeight / 2)
        );
        
        // Create text entities for UI information
        entitySystem.CreateEntity<TextEntity>(
            new Vector2(graphics.PreferredBackBufferWidth / 2, 20),
            InfoText,
            Color.White,
            TextEntity.TextAlignment.Center
        );
        
        entitySystem.CreateEntity<TextEntity>(
            new Vector2(graphics.PreferredBackBufferWidth / 2, graphics.PreferredBackBufferHeight - 40),
            CharacterInfo,
            Color.LightGreen,
            TextEntity.TextAlignment.Center
        );

        // Create sound button entities
        entitySystem.CreateEntity<SoundButtonEntity>(
            new Vector2(100, 100),
            "footstep1_sound.xml",
            "Footstep 1"
        );
        
        entitySystem.CreateEntity<SoundButtonEntity>(
            new Vector2(100, 150),
            "footstep2_sound.xml",
            "Footstep 2"
        );
        
        entitySystem.CreateEntity<SoundButtonEntity>(
            new Vector2(100, 200),
            "footstep3_sound.xml",
            "Footstep 3"
        );
        
        // Create volume control buttons
        entitySystem.CreateEntity<VolumeButtonEntity>(
            new Vector2(100, 250),
            0.1f,
            "Volume: 10%"
        );
        
        entitySystem.CreateEntity<VolumeButtonEntity>(
            new Vector2(100, 300),
            1.0f,
            "Volume: 100%"
        );

        // Register input handler
        Input.Keyboard.KeyReleased += Reset();
        Input.Keyboard.KeyReleased += PlaySound();
        
        UpdateLoadingProgress(1.0f, "Scene ready!");
        Console.WriteLine("Character scene loaded successfully!");

        songID = AudioManager.Instance.PlaySound("song1_sound.xml");
    }

    public override void Unload()
    {
        base.Unload();
        Input.Keyboard.KeyReleased -= Reset();
        Input.Keyboard.KeyReleased -= PlaySound();
    }

    private EventHandler<MonoGame.Extended.Input.InputListeners.KeyboardEventArgs> Reset()
    {
        return (sender, args) =>
        {
            if (args.Key == Microsoft.Xna.Framework.Input.Keys.Add || args.Key == Keys.OemPlus)
            {
                AudioManager.Instance.StopSound(songID);
                // Use SceneManager property directly here to get the current reference at the time of the event
                SceneManager.LoadScene(new PhysicsEntityScene());
            }
        };
    }

    private static EventHandler<MonoGame.Extended.Input.InputListeners.KeyboardEventArgs> PlaySound()
    {
        return (sender, args) =>
        {
            if (args.Key == Microsoft.Xna.Framework.Input.Keys.Q)
            {
                // Play the sound effect
                var id = AudioManager.Instance.PlayOneShotSound("footstep1_sound.xml");
                Console.WriteLine($"Sound played with ID: {id}");
            }

            if (args.Key == Microsoft.Xna.Framework.Input.Keys.W)
            {
                // Play the sound effect
                var id = AudioManager.Instance.PlayOneShotSound("footstep2_sound.xml");
                Console.WriteLine($"Sound played with ID: {id}");
            }

            if (args.Key == Microsoft.Xna.Framework.Input.Keys.E)
            {
                // Play the sound effect
                var id = AudioManager.Instance.PlayOneShotSound("footstep3_sound.xml");
                Console.WriteLine($"Sound played with ID: {id}");
            }

            if (args.Key == Microsoft.Xna.Framework.Input.Keys.Z)
            {
                AudioManager.Instance.SetMasterVolume(0.1f);
                Console.WriteLine("Volume set to 10%");
            }

            if (args.Key == Microsoft.Xna.Framework.Input.Keys.X)
            {
                AudioManager.Instance.SetMasterVolume(1.0f);
                Console.WriteLine("Volume set to 100%");
            }
        };
    }
}
