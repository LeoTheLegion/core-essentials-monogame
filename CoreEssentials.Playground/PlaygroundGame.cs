using CoreEssentials.GameSystems;
using CoreEssentials.GameSystems.Physics;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem;

namespace CoreEssentials.Playground;

public class PlaygroundGame : MainGame
{
    protected override GameSystem[] LoadSystems()
    {
        Graphics.PreferredBackBufferWidth = 1280;
        Graphics.PreferredBackBufferHeight = 720;
        Graphics.ApplyChanges();

        // Load all the game systems you want to use in your game here.

        PhysicsEngine physicsEngine = new PhysicsEngine(1);
        PhysicsDebugRenderer physicsDebugRenderer = new PhysicsDebugRenderer(physicsEngine);
        EntitySystem entitySystem = new EntitySystem();

        GameSystem[] systems = new GameSystem[]
        {
            physicsEngine,
            physicsDebugRenderer,
            entitySystem,
        };

        return systems;
    }
}
