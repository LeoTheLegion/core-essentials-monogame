using System;
using System.Collections;
using CoreEssentials.GameSystems;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem;
using CoreEssentials.SceneManagement;
using CoreEssentials.Debugging;
using CoreEssentials.Inputs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using CoreEssentials.Audio;

namespace CoreEssentials.Playground;

/// <summary>
/// A scene to demonstrate the SpriteSheet functionality with a character display.
/// </summary>
public class CharacterScene : Scene
{
    private Random random = new Random();

    private string songID;
    
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
        CharacterEntity staticCharacter = entitySystem.CreateEntity<CharacterEntity>(
            new Vector2(graphics.PreferredBackBufferWidth / 4, graphics.PreferredBackBufferHeight / 2)
        );

        // Create an animated character entity at the right side of the screen
        AnimatedCharacterEntity animatedCharacter = entitySystem.CreateEntity<AnimatedCharacterEntity>(
            new Vector2(graphics.PreferredBackBufferWidth * 3 / 4, graphics.PreferredBackBufferHeight / 2)
        );

        // Register input handler
        Input.Keyboard.KeyReleased += Reset();
        Input.Keyboard.KeyReleased += PlaySound();
        
        UpdateLoadingProgress(1.0f, "Scene ready!");
        Debug.Console.WriteLine("Character scene loaded successfully!");

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
            if (args.Key == Microsoft.Xna.Framework.Input.Keys.Right)
            {
                AudioManager.Instance.StopSound(songID);
                // Use SceneManager property directly here to get the current reference at the time of the event
                SceneManager.LoadScene(new PhysicsEntityScene());
            }
        };
    }

    private EventHandler<MonoGame.Extended.Input.InputListeners.KeyboardEventArgs> PlaySound()
    {
        return (sender, args) =>
        {
            if (args.Key == Microsoft.Xna.Framework.Input.Keys.Q)
            {
                // Play the sound effect
                var id = AudioManager.Instance.PlayOneShotSound("footstep1_sound.xml");
                Debug.Console.WriteLine($"Sound played with ID: {id}");
            }

            if (args.Key == Microsoft.Xna.Framework.Input.Keys.W)
            {
                // Play the sound effect
                var id = AudioManager.Instance.PlayOneShotSound("footstep2_sound.xml");
                Debug.Console.WriteLine($"Sound played with ID: {id}");
            }

            if (args.Key == Microsoft.Xna.Framework.Input.Keys.E)
            {
                // Play the sound effect
                var id = AudioManager.Instance.PlayOneShotSound("footstep3_sound.xml");
                Debug.Console.WriteLine($"Sound played with ID: {id}");
            }

            if (args.Key == Microsoft.Xna.Framework.Input.Keys.Z)
            {
                AudioManager.Instance.SetMasterVolume(0.1f);
                Debug.Console.WriteLine("Volume set to 10%");
            }

            if (args.Key == Microsoft.Xna.Framework.Input.Keys.X)
            {
                AudioManager.Instance.SetMasterVolume(1.0f);
                Debug.Console.WriteLine("Volume set to 100%");
            }
        };
    }
}
