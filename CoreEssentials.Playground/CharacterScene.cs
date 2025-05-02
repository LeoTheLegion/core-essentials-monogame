using System;
using System.Collections;
using CoreEssentials.Assets;
using CoreEssentials.GameSystems;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem;
using CoreEssentials.SceneManagement;
using CoreEssentials.Debugging;
using CoreEssentials.Inputs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace CoreEssentials.Playground;

/// <summary>
/// A scene to demonstrate the SpriteSheet functionality with a character display.
/// </summary>
public class CharacterScene : Scene
{
    private Random random = new Random();
    
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
        
        UpdateLoadingProgress(1.0f, "Scene ready!");
        Debug.Console.WriteLine("Character scene loaded successfully!");
    }

    public override void Unload()
    {
        base.Unload();
        Input.Keyboard.KeyReleased -= Reset();
    }

    private EventHandler<MonoGame.Extended.Input.InputListeners.KeyboardEventArgs> Reset()
    {
        var sceneManager = this.SceneManager;
        return (sender, args) =>
        {
            if (args.Key == Microsoft.Xna.Framework.Input.Keys.Right)
            {
                sceneManager.LoadScene(new PhysicsEntityScene());
            }
        };
    }
}

/// <summary>
/// A simple entity that displays a character from a sprite sheet.
/// </summary>
public class CharacterEntity : Entity
{
    private Sprite _sprite;
    
    public CharacterEntity(Vector2 position)
    {
        _position = position;
        
        // Load the character sprite that references the sprite sheet
        _sprite = AssetManager.LoadAsset<Sprite>("character_sprite.xml");
    }
    
    public override void OnStart()
    {
        base.OnStart();
        Debug.Console.WriteLine("Character entity created!");
    }
    
    public override void Render(SpriteBatch spriteBatch)
    {
        spriteBatch.Begin();
        // Draw the character with the current frame
        _sprite.Draw(
            spriteBatch, 
            _position, 
            Color.White, 
            0f, 
            SpriteEffects.None, 
            0f
        );
        spriteBatch.End();
    }
}