using System;
using System.Collections;
using System.IO;
using CoreEssentials.GameSystems;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Serialization;
using CoreEssentials.GameSystems.Physics.Engines.Aether;
using CoreEssentials.GameSystems.Physics.Types;
using CoreEssentials.Inputs;
using CoreEssentials.Scenes;
using CoreEssentials.Coroutines;
using CoreEssentials.GUI;
using CoreEssentials.GUI.Factory;
using CoreEssentials.GUI.Types;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Components.BuiltIn;

namespace CoreEssentials.Playground;

public class PhysicsEntityScene : Scene
{
    private readonly Random _random = new();
    private IButton _saveButton;
    private IButton _loadButton;
    private PhysicsDebugRenderer _physicsDebugRenderer;
    private PhysicsConfig _physicsConfig;
    private const string SaveFilePath = "PhysicsScene_Save.xml";

    protected override GameSystem[] LoadGameSystems()
    {
        // Load all the game systems you want to use in your game here.
        // Physics settings (gravity, solver iterations) and named collision categories
        // come from a declarative Content/PhysicsConfig.xml file.
        _physicsConfig = PhysicsConfig.LoadFromAsset("PhysicsConfig.xml");
        PhysicsEngine physicsEngine = new PhysicsEngine(_physicsConfig);
        // Aether-backed debug renderer (toggle with F1). Content is loaded lazily
        // in OnStartCoroutine once the graphics device exists.
        _physicsDebugRenderer = new PhysicsDebugRenderer(physicsEngine);
        PhysicsDebugRenderer physicsDebugRenderer = _physicsDebugRenderer;
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

        // Load the physics debug renderer's font now that the graphics device exists.
        _physicsDebugRenderer?.LoadContent();

        UpdateLoadingProgress(0.55f, "Setting up entities...");
        
        EntitySystem entitySystem = GetGameSystem<EntitySystem>();

        // Register the template first — all balls use it
        entitySystem.RegisterTemplate("BallPrefab", "BallTemplate.xml");
        
        int totalEntities = 5;
        int currentEntity = 0;
        
        // Create regular balls from template with progress updates
        for (int i = 0; i < totalEntities; i++)
        {
            // Create a random position within the screen bounds
            int padding = 32;
            int x = _random.Next(padding, graphics.PreferredBackBufferWidth - padding);
            int y = _random.Next(padding, graphics.PreferredBackBufferHeight - padding);

            Ball ball = (Ball)entitySystem.Instantiate("BallPrefab", new Vector2(x, y));
            // ID auto-generated on creation
            // Sprint 19 demo: regular balls only collide with other regular balls.
            // The "Player" category name is resolved from PhysicsConfig.xml.
            SetBallCollisionFilter(ball, _physicsConfig.Resolve("Player"), _physicsConfig.Resolve("Player"));
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

        // Sprint 10, 11 & 13 Demo: Load VIP balls using Templates + persistent IDs! 🎉
        UpdateLoadingProgress(0.92f, "Loading VIP balls from Template...");

        // Instantiate 3 VIP balls from the template at center positions with distinct colors and IDs
        (string Id, Vector2 Position, Color Color)[] vipBalls = {
            ("vip_ball_blue", new Vector2(640, 360), Color.Blue),
            ("vip_ball_green", new Vector2(580, 300), Color.Green),
            ("vip_ball_red", new Vector2(700, 420), Color.Red)
        };

        foreach (var (id, pos, color) in vipBalls)
        {
            Ball ball = (Ball)entitySystem.Instantiate("BallPrefab", pos);
            ball.SetId(id);

            // Sprint 19 demo: VIP balls only collide with other VIP balls.
            // The "Vip" category name is resolved from PhysicsConfig.xml.
            SetBallCollisionFilter(ball, _physicsConfig.Resolve("Vip"), _physicsConfig.Resolve("Vip"));

            // Make VIP balls larger and set their unique color.
            // The ColliderComponent auto-sizes its circle collider to the (now larger)
            // sprite on the next frame, so no manual collider update is needed here.
            ball.Scale = new Vector2(2.0f, 2.0f);
            var spriteComp = ball.GetComponent<SpriteComponent>();
            if (spriteComp != null)
                spriteComp.Color = color;
            
            // Apply random impulse for fun movement
            ball.GetComponent<RigidbodyComponent>()?.ApplyImpulse(new Vector2(
                (float)(_random.NextDouble() * 15 - 7.5f),
                (float)(_random.NextDouble() * 15 - 7.5f)
            ));
            
            Console.WriteLine($"VIP Ball spawned at {pos} with Scale={ball.Scale}, Color={color}, Id={id}");
        }

        Console.WriteLine($"Loaded {vipBalls.Length} VIP balls from Template!");

        // Update progress to 95%
        UpdateLoadingProgress(0.95f, "Setting up world border...");
        yield return new WaitForSeconds(0.1f);

        // Create a world border (ID auto-generated on creation)
        entitySystem.CreateEntity<WorldBorder>(
            new Vector2(0, 0), 
            new Vector2(graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight)
        );
        
        // Create save/load buttons
        CreateSaveLoadButtons();
        
        // Register input handler
        Input.Keyboard.KeyReleased += Reset();
        
        // Final progress update
        UpdateLoadingProgress(1.0f, "Scene ready!");
        
        Console.WriteLine("Physics entity scene initialization complete!");
    }

    /// <summary>
    /// Applies a collision filter to a ball's collider (Sprint 19 demo).
    /// The collider is created during Instantiate, so we set the live ICollider here.
    /// </summary>
    private static void SetBallCollisionFilter(Ball ball, CollisionCategory categories, CollisionCategory collidesWith)
    {
        var colliderComponent = ball.GetComponent<ColliderComponent>();
        if (colliderComponent?.Collider == null)
        {
            Console.WriteLine($"[CollisionFilter] Ball {ball.Id} has no collider — skipping filter");
            return;
        }

        // Keep the component's stored values in sync so save/load round-trips the filter.
        colliderComponent.Categories = categories;
        colliderComponent.CollidesWith = collidesWith;

        colliderComponent.Collider.Categories = categories;
        colliderComponent.Collider.CollidesWith = collidesWith;

        Console.WriteLine($"[CollisionFilter] Ball {ball.Id}: Categories={categories}, CollidesWith={collidesWith}");
    }

    public override void Unload()
    {
        base.Unload();
        Input.Keyboard.KeyReleased -= Reset();
        // Clean up GUI buttons
        if (_saveButton != null)
            GUIManager.RemoveWidget(_saveButton);
        if (_loadButton != null)
            GUIManager.RemoveWidget(_loadButton);
    }

    private void CreateSaveLoadButtons()
    {
        // Create save button
        _saveButton = WidgetFactory.CreateTextButton("Save Physics Scene");
        _saveButton.Position = new Vector2(20, 20);
        _saveButton.Width = 200;
        _saveButton.Height = 50;
        _saveButton.Clicked += (button) => SaveScene();
        GUIManager.AddWidget(_saveButton);

        // Create load button
        _loadButton = WidgetFactory.CreateTextButton("Load Physics Scene");
        _loadButton.Position = new Vector2(20, 80);
        _loadButton.Width = 200;
        _loadButton.Height = 50;
        _loadButton.Clicked += (button) => LoadScene();
        GUIManager.AddWidget(_loadButton);
    }

    private void SaveScene()
    {
        try
        {
            var entitySystem = GetGameSystem<EntitySystem>();
            entitySystem.SaveState(SaveFilePath);
            Console.WriteLine($"Physics scene saved to {SaveFilePath}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to save physics scene: {ex.Message}");
        }
    }

    private void LoadScene()
    {
        Console.WriteLine("[LoadScene] === Starting load process ===");
        try
        {
            if (File.Exists(SaveFilePath))
            {
                Console.WriteLine($"[LoadScene] Save file found: {SaveFilePath}");
                var entitySystem = GetGameSystem<EntitySystem>();
                Console.WriteLine($"[LoadScene] EntitySystem has {entitySystem.GetEntities().Count} entities before load");
                
                Console.WriteLine("[LoadState] === Calling LoadState (ID-based replace mode) ===");
                entitySystem.LoadState(SaveFilePath);
                Console.WriteLine($"[LoadState] === LoadState completed successfully ===");
                Console.WriteLine($"[LoadScene] EntitySystem now has {entitySystem.GetEntities().Count} entities after load");
                
                Console.WriteLine($"Physics scene loaded from {SaveFilePath}");
            }
            else
            {
                Console.WriteLine($"Save file not found: {SaveFilePath}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to load physics scene: {ex.Message}");
            Console.WriteLine($"Exception type: {ex.GetType().FullName}");
            if (ex.InnerException != null)
            {
                Console.WriteLine($"Inner: {ex.InnerException.GetType().Name}: {ex.InnerException.Message}");
            }
            Console.WriteLine($"Stack:\n{ex.StackTrace}");
        }
    }

    private EventHandler<MonoGame.Extended.Input.InputListeners.KeyboardEventArgs> Reset()
    {
        return (sender, args) =>
        {
            if (args.Key == Keys.F1)
            {
                // Toggle physics debug visualization (collider outlines).
                if (_physicsDebugRenderer != null)
                {
                    _physicsDebugRenderer.IsEnabled = !_physicsDebugRenderer.IsEnabled;
                    Console.WriteLine($"[PhysicsDebug] Enabled = {_physicsDebugRenderer.IsEnabled}");
                }
                return;
            }

            if (args.Key == Microsoft.Xna.Framework.Input.Keys.Add || args.Key == Keys.OemPlus)
            {
                // Use SceneManager property directly here to get the current reference at the time of the event
                SceneManager.LoadScene(new CameraScene());
            }
        };
    }

    /// <summary>
    /// Draws the scene, then overlays the physics debug visualization when enabled (F1).
    /// </summary>
    public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
    {
        base.Draw(gameTime, spriteBatch);

        if (_physicsDebugRenderer != null && _physicsDebugRenderer.IsEnabled)
            _physicsDebugRenderer.Draw(spriteBatch);
    }
}
