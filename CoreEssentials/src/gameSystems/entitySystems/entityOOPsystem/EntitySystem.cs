using CoreEssentials.GameSystems.EntitySystems.EntityOOPsystem;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem;

public class EntitySystem : GameSystem, IUpdateGameSystem, IDrawGameSystem
{
    public void Update(GameTime gameTime)
    {
        // Update the entity system here.
        // This could include updating the state of entities, handling input, etc.
        EntityManagementSystem.Update(ref gameTime);
    }

    public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
    {
        // Draw the entities here.
        // This could include drawing sprites, UI elements, etc.
        EntityManagementSystem.Render(ref spriteBatch);
    }
}
