using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace CoreEssentials.GameSystems.Physics.Types;

/// <summary>
/// Debug renderer for visualizing physics bodies, fixtures, and shapes.
/// Provides a way to toggle and customize debug visualization during development.
/// </summary>
public interface IPhysicsDebugRenderer : IDisposable
{
    /// <summary>
    /// Gets or sets whether debug rendering is currently enabled.
    /// </summary>
    bool IsEnabled { get; set; }

    /// <summary>
    /// Draws all physics bodies and their shapes to the provided sprite batch.
    /// </summary>
    /// <param name="spriteBatch">The active SpriteBatch for rendering.</param>
    void Draw(SpriteBatch spriteBatch);
}
