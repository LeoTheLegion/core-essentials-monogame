# Getting Started with CoreEssentials-MonoGame

This guide will walk you through the process of setting up a new project using the CoreEssentials-MonoGame framework and implementing your first game scene.

## Prerequisites

- [.NET 5.0 or higher](https://dotnet.microsoft.com/download)
- [Visual Studio 2019 or higher](https://visualstudio.microsoft.com/) (with .NET desktop development workload)
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
using CoreEssentials.SceneManagement;
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
            Debug.Console.WriteLine("Main menu loaded successfully!");
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
using CoreEssentials.SceneManagement;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace YourGameNamespace
{
    public class MenuEntity : Entity
    {
        private SpriteFont _font;
        private string _titleText = "My Awesome Game";
        private string _instructionText = "Press ENTER to start";
        
        public override void Initialize()
        {
            base.Initialize();
            
            // Load font from content (assumes you've added a SpriteFont to your Content project)
            _font = Scene.Game.Content.Load<SpriteFont>("Fonts/MenuFont");
        }
        
        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
            
            // Draw title text
            Vector2 titleSize = _font.MeasureString(_titleText);
            spriteBatch.DrawString(_font, _titleText, 
                Position - new Vector2(titleSize.X / 2, 50), 
                Color.White);
            
            // Draw instruction text
            Vector2 instructSize = _font.MeasureString(_instructionText);
            spriteBatch.DrawString(_font, _instructionText, 
                Position - new Vector2(instructSize.X / 2, -50), 
                Color.Yellow);
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

```csharp
public class PlayerEntity : Entity
{
    public Body Body { get; private set; }
    private Sprite _sprite;
    
    public override void Initialize()
    {
        base.Initialize();
        
        // Get physics system
        PhysicsEngine physics = Scene.GetGameSystem<PhysicsEngine>();
        
        // Create physics body
        Body = physics.CreateCircle(Position, 16f, 1f);
        Body.BodyType = BodyType.Dynamic;
        Body.LinearDamping = 0.5f;
        Body.Restitution = 0.5f;
        Body.Tag = this;
        
        // Load sprite
        SpriteSheet sheet = Scene.AssetManager.LoadSpriteSheet("player.xml");
        _sprite = sheet.GetSprite("default");
        _sprite.Origin = new Vector2(_sprite.Width / 2, _sprite.Height / 2);
    }
    
    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);
        
        // Update position from physics body
        Position = Body.Position;
        Rotation = Body.Rotation;
        
        // Handle input
        Vector2 force = Vector2.Zero;
        
        if (Input.Keyboard.IsKeyDown(Keys.W)) force.Y -= 1;
        if (Input.Keyboard.IsKeyDown(Keys.S)) force.Y += 1;
        if (Input.Keyboard.IsKeyDown(Keys.A)) force.X -= 1;
        if (Input.Keyboard.IsKeyDown(Keys.D)) force.X += 1;
        
        if (force != Vector2.Zero)
        {
            // Normalize and apply force
            force.Normalize();
            force *= 10f;
            Body.ApplyForce(force);
        }
    }
    
    public override void Draw(SpriteBatch spriteBatch)
    {
        base.Draw(spriteBatch);
        
        _sprite.Draw(spriteBatch, Position, Rotation);
    }
}
```

## Using Audio

Add sound effects to your gameplay:

```csharp
// Inside PlayerEntity.cs
private string _engineSoundId;

public override void Initialize()
{
    // ...existing code...
    
    // Play engine sound when moving
    _engineSoundId = AudioManager.Instance.PlaySound("engine_sound.xml");
    AudioManager.Instance.SetSoundVolume(_engineSoundId, 0.0f);
}

public override void Update(GameTime gameTime)
{
    // ...existing code...
    
    // Adjust engine sound volume based on movement
    float speed = Body.LinearVelocity.Length();
    float volume = MathHelper.Clamp(speed / 10f, 0f, 1f);
    AudioManager.Instance.SetSoundVolume(_engineSoundId, volume * 0.5f);
}

public override void OnDestroy()
{
    base.OnDestroy();
    
    // Stop sound when entity is destroyed
    AudioManager.Instance.StopSound(_engineSoundId);
}
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