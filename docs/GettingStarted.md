# Getting Started with CoreEssentials-MonoGame

This guide will walk you through the process of setting up a new project using the CoreEssentials-MonoGame framework and implementing your first game scene.

## Prerequisites

- [.NET 8.0 or higher](https://dotnet.microsoft.com/download)
- [Visual Studio 2022 or higher](https://visualstudio.microsoft.com/) (with .NET desktop development workload)
- [MonoGame](https://www.monogame.net/downloads/) (MonoGame 3.8+ recommended)

## Create a New Project

1. Create a new MonoGame Cross Platform Desktop Project in Visual Studio
2. Install the CoreEssentials-MonoGame package from NuGet:

```bash
# Via Package Manager Console
Install-Package CoreEssentials-MonoGame

# Via CLI
dotnet add package CoreEssentials-MonoGame
```

## Set Up Your First Project

### Setting Up the Main Game Class

Replace the default `Game1.cs` with a class that inherits from `MainGame`:

```csharp
using CoreEssentials;
using Microsoft.Xna.Framework;

namespace YourGameNamespace
{
    public class Game1 : MainGame
    {
        public Game1() : base()
        {
            // Configure window properties
            Window.Title = "My First CoreEssentials Game";
            Graphics.PreferredBackBufferWidth = 1280;
            Graphics.PreferredBackBufferHeight = 720;
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            base.Initialize();
            
            // Load your initial scene
            SceneManager.LoadScene(new MainMenuScene());
        }
    }
}
```

### Creating Your First Scene

Create a scene class that inherits from `Scene`:

```csharp
using System.Collections;
using CoreEssentials.GameSystems;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem;
using CoreEssentials.Inputs;
using CoreEssentials.Scenes;
using CoreEssentials.Debugging;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace YourGameNamespace
{
    public class MainMenuScene : Scene
    {
        protected override GameSystem[] LoadGameSystems()
        {
            return new GameSystem[]
            {
                new EntitySystem()
            };
        }
        
        protected override IEnumerator OnStartCoroutine()
        {
            UpdateLoadingProgress(0.5f, "Loading main menu...");
            yield return null;
            
            // Set up the scene
            EntitySystem entitySystem = GetGameSystem<EntitySystem>();
            
            // Create a menu entity
            var menuEntity = entitySystem.CreateEntity<MenuEntity>(
                new Vector2(Graphics.PreferredBackBufferWidth / 2, 
                            Graphics.PreferredBackBufferHeight / 2));
            
            // Register input handler
            Input.Keyboard.KeyReleased += OnKeyReleased();
            
            UpdateLoadingProgress(1.0f, "Ready!");
            Console.WriteLine("Main menu loaded successfully!");
        }
        
        public override void Unload()
        {
            base.Unload();
            Input.Keyboard.KeyReleased -= OnKeyReleased();
        }
        
        private EventHandler<MonoGame.Extended.Input.InputListeners.KeyboardEventArgs> OnKeyReleased()
        {
            return (sender, args) =>
            {
                if (args.Key == Keys.Enter)
                {
                    // Start the game when Enter is pressed
                    SceneManager.LoadScene(new GameplayScene());
                }
            };
        }
    }
}
```

### Creating a Basic Entity

Create an entity class for your menu:

```csharp
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem;
using CoreEssentials.Scenes;
using CoreEssentials.Assets;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace YourGameNamespace
{
    public class MenuEntity : Entity
    {
        private FontAsset _font;
        private string _titleText = "My Awesome Game";
        private string _instructionText = "Press ENTER to start";
        
        public override void OnStart()
        {
            base.OnStart();
            
            // Load font through the asset manager (assumes you've added a SpriteFont to your Content project)
            _font = AssetManager.LoadAsset<FontAsset>("Fonts/MenuFont");
        }
        
        public override void Render(SpriteBatch spriteBatch)
        {
            base.Render(spriteBatch);
            
            // Draw title text
            Vector2 titleSize = _font.MeasureStringVector(_titleText);
            _font.Font.DrawString(spriteBatch, _titleText, 
                Position - new Vector2(titleSize.X / 2, 50), 
                Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
            
            // Draw instruction text
            Vector2 instructSize = _font.MeasureStringVector(_instructionText);
            _font.Font.DrawString(spriteBatch, _instructionText, 
                Position - new Vector2(instructSize.X / 2, -50), 
                Color.Yellow, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
        }
    }
}
```

## Using Physics

### Create a Scene with Physics

To implement a gameplay scene with physics:

```csharp
public class GameplayScene : Scene
{
    protected override GameSystem[] LoadGameSystems()
    {
        // Create the physics engine
        PhysicsEngine physicsEngine = new PhysicsEngine();
        
        // Create physics debug renderer for development
        PhysicsDebugRenderer debugRenderer = new PhysicsDebugRenderer(physicsEngine);
        
        // Create entity system
        EntitySystem entitySystem = new EntitySystem();
        
        return new GameSystem[]
        {
            physicsEngine,
            entitySystem,
            debugRenderer
        };
    }
    
    protected override IEnumerator OnStartCoroutine()
    {
        UpdateLoadingProgress(0.3f, "Setting up gameplay...");
        yield return null;
        
        // Get systems
        EntitySystem entitySystem = GetGameSystem<EntitySystem>();
        
        // Create player
        var player = entitySystem.CreateEntity<PlayerEntity>(
            new Vector2(Graphics.PreferredBackBufferWidth / 2, 
                        Graphics.PreferredBackBufferHeight / 2));
        
        // Create some obstacles
        for (int i = 0; i < 5; i++)
        {
            var obstacle = entitySystem.CreateEntity<ObstacleEntity>(
                new Vector2(
                    Random.Shared.Next(100, Graphics.PreferredBackBufferWidth - 100),
                    Random.Shared.Next(100, Graphics.PreferredBackBufferHeight - 100)
                ));
        }
        
        // Create world border
        var border = entitySystem.CreateEntity<BorderEntity>(
            new Vector2(0, 0), 
            new Vector2(Graphics.PreferredBackBufferWidth, 
                        Graphics.PreferredBackBufferHeight));
        
        UpdateLoadingProgress(1.0f, "Ready!");
    }
}
```

### Create a Physics-Enabled Entity

Physics is added to entities through components. A `RigidbodyComponent` gives the entity a physics body, and a `ColliderComponent` gives it a shape. The rigidbody keeps the entity's `Position`/`Rotation` in sync with the physics body automatically.

```csharp
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Components.BuiltIn;
using CoreEssentials.Assets;
using CoreEssentials.Inputs;

public class PlayerEntity : Entity
{
    private RigidbodyComponent _rigidbody;
    private SpriteComponent _sprite;
    
    public override void OnStart()
    {
        base.OnStart();
        
        // Add a dynamic rigidbody (body is created lazily on first access)
        _rigidbody = new RigidbodyComponent(RigidbodyType.Dynamic);
        _rigidbody.Mass = 1f;
        AddComponent(_rigidbody);
        
        // Add a circle collider
        var collider = new ColliderComponent(radius: 16f);
        collider.Restitution = 0.5f;
        AddComponent(collider);
        
        // Load and add a sprite
        var sprite = AssetManager.LoadAsset<Sprite>("player.xml");
        _sprite = new SpriteComponent(sprite);
        _sprite.Origin = new Vector2(0.5f, 0.5f);
        AddComponent(_sprite);
    }
    
    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);
        
        // Handle input and apply a force through the rigidbody component
        Vector2 force = Vector2.Zero;
        
        if (Input.Keyboard.IsKeyDown(Keys.W)) force.Y -= 1;
        if (Input.Keyboard.IsKeyDown(Keys.S)) force.Y += 1;
        if (Input.Keyboard.IsKeyDown(Keys.A)) force.X -= 1;
        if (Input.Keyboard.IsKeyDown(Keys.D)) force.X += 1;
        
        if (force != Vector2.Zero)
        {
            force.Normalize();
            force *= 10f;
            _rigidbody.ApplyImpulse(force);
        }
        
        // Position/Rotation are synced from the physics body automatically.
    }
    
    public override void Render(SpriteBatch spriteBatch)
    {
        base.Render(spriteBatch);
        
        _sprite.Draw(spriteBatch);
    }
}
```

## Using Audio

Add sound effects to your gameplay through the `AudioManager` singleton:

```csharp
// Inside PlayerEntity.cs
private string _footstepSoundId;

public override void Update(GameTime gameTime)
{
    // ...existing code...
    
    // Play a one-shot sound effect (e.g., footsteps)
    _footstepSoundId = AudioManager.Instance.PlayOneShotSound("footstep_sound.xml");
}

public override void OnDestroy()
{
    base.OnDestroy();
    
    // Stop sounds when the entity is destroyed
    if (_footstepSoundId != null)
        AudioManager.Instance.StopSound(_footstepSoundId);
}
```

Other useful `AudioManager` methods:

```csharp
// Start a looping/controllable sound
string id = AudioManager.Instance.PlaySound("engine_sound.xml");
AudioManager.Instance.PauseSound(id);
AudioManager.Instance.ResumeSound(id);
AudioManager.Instance.StopSound(id);

// Adjust the overall volume
AudioManager.Instance.SetMasterVolume(0.5f);
```

## Running and Testing

1. Build and run your project
2. The game should start with your MainMenuScene
3. Press Enter to start the game and transition to the GameplayScene
4. Use WASD to control the player character

## Next Steps

- Add more entities to your game
- Implement game-specific mechanics
- Create additional scenes for different game states
- Add UI elements using the GUIManager
- Implement sound effects for various game events

## Learn from Playground Examples

The CoreEssentials.Playground project contains several example scenes:

- **CharacterScene**: Demonstrates sprite animation and audio management
- **PhysicsEntityScene**: Shows physics interactions with dynamic bodies

Study these examples to understand how to implement various features in your games.