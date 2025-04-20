using System;
using CoreEssentials.GameSystems;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem;
using CoreEssentials.GameSystems.Physics;
using CoreEssentials.Inputs;
using CoreEssentials.SceneManagement;
using Microsoft.Xna.Framework;

namespace CoreEssentials.Playground;

public class PhysicsEntityScene : Scene
{
    private Random random = new Random();
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

    protected override void onStart()
    {
        // Run your startup code here.

        GraphicsDeviceManager graphics = SceneManager.Game.Graphics;

        graphics.PreferredBackBufferWidth = 1280;
        graphics.PreferredBackBufferHeight = 720;
        graphics.ApplyChanges();

        EntitySystem entitySystem = GetGameSystem<EntitySystem>();


        for (int i = 0; i < graphics.PreferredBackBufferWidth; i += 10)
        {

            // Create a random y bettween 0 and 720
            int padding = 32;
            int y = random.Next(padding, graphics.PreferredBackBufferHeight - padding);

            //Ball ball = new Ball(new Vector2(i, y));
            Ball ball = entitySystem.CreateEntity<Ball>(new Vector2(i, y));
            // add Random force to the ball
            ball.Body.ApplyLinearImpulse(new Vector2((float)(random.NextDouble() * 10 - 5), (float)(random.NextDouble() * 10 - 5)));
        }

        // Create a world border
        WorldBorder worldBorder = entitySystem.CreateEntity<WorldBorder>(new Vector2(0, 0), new Vector2(graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight));

        Input.Keyboard.KeyReleased += Reset();
    }

    private static EventHandler<MonoGame.Extended.Input.InputListeners.KeyboardEventArgs> Reset()
    {
        return (sender, args) =>
        {
            if (args.Key == Microsoft.Xna.Framework.Input.Keys.Right)
            {
                SceneManager.LoadScene(new PhysicsEntityScene());
            }
        };
    }
}
