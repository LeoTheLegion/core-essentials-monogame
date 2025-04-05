using CoreEssentials.GameSystems;
using CoreEssentials.GameSystems.Physics;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem;
using Microsoft.Xna.Framework;
using System;

namespace CoreEssentials.Playground;

public class PlaygroundGame : MainGame
{
    private Random random = new Random();
    protected override GameSystem[] LoadSystems()
    {
        Graphics.PreferredBackBufferWidth = 1280;
        Graphics.PreferredBackBufferHeight = 720;
        Graphics.ApplyChanges();

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

        for (int i = 0; i < Graphics.PreferredBackBufferWidth; i += 25){
            Ball ball = new Ball(new Vector2(i, 200));
            // add Random force to the ball
            ball.Body.ApplyLinearImpulse(new Vector2((float)(random.NextDouble() * 10 - 5), (float)(random.NextDouble() * 10 - 5)));
        }

        WorldBorder worldBorder = new WorldBorder(new Vector2(0, 0), new Vector2(Graphics.PreferredBackBufferWidth, Graphics.PreferredBackBufferHeight));

        return systems;
    }
}
