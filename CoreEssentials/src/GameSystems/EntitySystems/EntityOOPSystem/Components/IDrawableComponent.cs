using Microsoft.Xna.Framework.Graphics;

namespace CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Components;

/// <summary>
/// Implemented by components that contribute to an entity's rendering.
/// The base <c>Entity.Render</c> method draws every attached component that implements this
/// interface, so entities can render purely from components without overriding <c>Render</c>.
/// </summary>
public interface IDrawableComponent
{
    /// <summary>
    /// Draws this component using its owning entity's transform.
    /// </summary>
    /// <param name="spriteBatch">The SpriteBatch used for drawing.</param>
    void Draw(SpriteBatch spriteBatch);
}
