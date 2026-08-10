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

        int totalEntities = graphics.PreferredBackBufferWidth / 10;
        int currentEntity = 0;
        
        // Create balls one by one with progress updates
        for (int i = 0; i < graphics.PreferredBackBufferWidth; i += 10)
        {
            // Create a random y between 0 and 720
            int padding = 32;
            int y = _random.Next(padding, graphics.PreferredBackBufferHeight - padding);

            Ball ball = entitySystem.CreateEntity<Ball>(new Vector2(i, y));
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

        // Sprint 10 Demo: Load extra VIP balls from XML! 🎉
        UpdateLoadingProgress(0.92f, "Loading VIP balls from XML...");
        string vipBallXml = @"
            <Scene>
                <EntityDefinition Type=""CoreEssentials.Playground.Ball"" Id=""vip1"">
                    <Position X=""100"" Y=""100"" />
                    <Component Type=""SpriteComponent"">
                        <Properties>
                            <Property Name=""Color"" Value=""Red"" />
                        </Properties>
                    </Component>
                </EntityDefinition>
                <EntityDefinition Type=""CoreEssentials.Playground.Ball"" Id=""vip2"">
                    <Position X=""500"" Y=""360"" />
                    <Component Type=""SpriteComponent"">
                        <Properties>
                            <Property Name=""Color"" Value=""Green"" />
                        </Properties>
                    </Component>
                </EntityDefinition>
                <EntityDefinition Type=""CoreEssentials.Playground.Ball"" Id=""vip3"">
                    <Position X=""1000"" Y=""600"" />
                    <Component Type=""SpriteComponent"">
                        <Properties>
                            <Property Name=""Color"" Value=""Cyan"" />
                        </Properties>
                    </Component>
                </EntityDefinition>
            </Scene>";

        var xmlBalls = EntitySerializer.LoadSceneFromXml(vipBallXml, entitySystem);
        foreach (var ball in xmlBalls)
        {
            var spriteComp = ball.GetComponent<SpriteComponent>();
            Console.WriteLine($"[PhysicsEntityScene] Ball color AFTER XML load: {spriteComp?.Color}");
            
            ball.GetComponent<RigidbodyComponent>()?.ApplyImpulse(new Vector2(
                (float)(_random.NextDouble() * 15 - 7.5f),
                (float)(_random.NextDouble() * 15 - 7.5f)
            ));
        }

        Console.WriteLine($"Loaded {xmlBalls.Count} VIP balls from XML!");

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
