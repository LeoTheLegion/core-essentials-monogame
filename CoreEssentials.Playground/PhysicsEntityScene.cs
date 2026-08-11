using System;
using System.Collections;
using CoreEssentials.GameSystems;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Serialization;
using CoreEssentials.GameSystems.Physics.Engines.Aether;
using CoreEssentials.Inputs;
using CoreEssentials.Scenes;
using CoreEssentials.Coroutines;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Components.BuiltIn;

namespace CoreEssentials.Playground;

public class PhysicsEntityScene : Scene
{
    private readonly Random _random = new();
    
    protected override GameSystem[] LoadGameSystems()
    {
        // Load all the game systems you want to use in your game here.
        PhysicsEngine physicsEngine = new PhysicsEngine();
        PhysicsDebugRenderer physicsDebugRenderer = new PhysicsDebugRenderer(physicsEngine);
        EntitySystem entitySystem = new EntitySystem();

        GameSystem[] systems = new GameSystem[]
        {
            physicsEngine,
            entitySystem,
            physicsDebugRenderer,
        };

        return systems;
    }
    
    // Override the coroutine version of onStart to demonstrate our new loading status system
    protected override IEnumerator OnStartCoroutine()
    {
        UpdateLoadingProgress(0.5f, "Initializing physics scene...");
        
        // Simulate some initialization work at the beginning
        yield return new WaitForSeconds(0.2f);
        
        GraphicsDeviceManager graphics = SceneManager.Game.Graphics;

        graphics.PreferredBackBufferWidth = 1280;
        graphics.PreferredBackBufferHeight = 720;
        graphics.ApplyChanges();
        
        UpdateLoadingProgress(0.55f, "Setting up entities...");
        
        EntitySystem entitySystem = GetGameSystem<EntitySystem>();

        // Register the template first — all balls use it
        entitySystem.RegisterTemplate("BallPrefab", "BallTemplate.xml");
        
        int totalEntities = graphics.PreferredBackBufferWidth / 10;
        int currentEntity = 0;
        
        // Create regular balls from template with progress updates
        for (int i = 0; i < graphics.PreferredBackBufferWidth; i += 10)
        {
            // Create a random y between 0 and 720
            int padding = 32;
            int y = _random.Next(padding, graphics.PreferredBackBufferHeight - padding);

            Ball ball = (Ball)entitySystem.Instantiate("BallPrefab", new Vector2(i, y));
            // add Random force to the ball
            ball.GetComponent<RigidbodyComponent>().ApplyImpulse(new Vector2(
                (float)(_random.NextDouble() * 10 - 5), 
                (float)(_random.NextDouble() * 10 - 5)
            ));

            // Update progress (from 55% to 90%)
            currentEntity++;
            float progress = 0.55f + 0.35f * (currentEntity / (float)totalEntities);

            // Update loading progress and display entity creation count
            if (i % 50 == 0)
            {
                UpdateLoadingProgress(progress, $"Creating entities: {currentEntity}/{totalEntities} balls");
                yield return null;
            }
            else
            {
                // Just update the progress without changing the status message too often
                _loadingProgress = progress;
            }
        }

        // Sprint 10 & 11 Demo: Load VIP balls using Templates! 🎉
        UpdateLoadingProgress(0.92f, "Loading VIP balls from Template...");

        // Instantiate 3 VIP balls from the template at center positions with distinct colors
        (Vector2 Position, Color Color)[] vipBalls = {
            (new Vector2(640, 360), Color.Blue),
            (new Vector2(580, 300), Color.Green),
            (new Vector2(700, 420), Color.Red)
        };

        foreach (var (pos, color) in vipBalls)
        {
            Ball ball = (Ball)entitySystem.Instantiate("BallPrefab", pos);
            
            // Make VIP balls larger and set their unique color
            ball.Scale = 2.0f;
            var spriteComp = ball.GetComponent<SpriteComponent>();
            if (spriteComp != null)
                spriteComp.Color = color;
            
            // Apply random impulse for fun movement
            ball.GetComponent<RigidbodyComponent>()?.ApplyImpulse(new Vector2(
                (float)(_random.NextDouble() * 15 - 7.5f),
                (float)(_random.NextDouble() * 15 - 7.5f)
            ));
            
            Console.WriteLine($"VIP Ball spawned at {pos} with Scale={ball.Scale}, Color={color}");
        }

        Console.WriteLine($"Loaded {vipBalls.Length} VIP balls from Template!");

        // Update progress to 95%
        UpdateLoadingProgress(0.95f, "Setting up world border...");
        yield return new WaitForSeconds(0.1f);

        // Create a world border
        entitySystem.CreateEntity<WorldBorder>(
            new Vector2(0, 0), 
            new Vector2(graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight)
        );
        
        // Register input handler
        Input.Keyboard.KeyReleased += Reset();
        
        // Final progress update
        UpdateLoadingProgress(1.0f, "Scene ready!");
        
        Console.WriteLine("Physics entity scene initialization complete!");
    }

    public override void Unload()
    {
        base.Unload();
        Input.Keyboard.KeyReleased -= Reset();
    }

    private EventHandler<MonoGame.Extended.Input.InputListeners.KeyboardEventArgs> Reset()
    {
        return (sender, args) =>
        {
            if (args.Key == Microsoft.Xna.Framework.Input.Keys.Add || args.Key == Keys.OemPlus)
            {
                // Use SceneManager property directly here to get the current reference at the time of the event
                SceneManager.LoadScene(new CameraScene());
            }
        };
    }
}
