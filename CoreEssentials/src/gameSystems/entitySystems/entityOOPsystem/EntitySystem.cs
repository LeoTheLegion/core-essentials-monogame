using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem;

public class EntitySystem : GameSystem, IUpdateGameSystem, IDrawGameSystem, ILoadAssetGameSystem
{
    public void LoadAssets()
    {
        // Load assets for the entity system here.
        // This could include loading textures, sounds, etc.
    }

    public void Update(GameTime gameTime)
    {
        // Update the entity system here.
        // This could include updating the state of entities, handling input, etc.
    }

    public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
    {
        // Draw the entities here.
        // This could include drawing sprites, UI elements, etc.
    }
}
