using Microsoft.Xna.Framework;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem;

namespace CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Components;

/// <summary>
/// Base class for all entity components in the mixin-style component system.
/// Components provide composable behavior that can be attached to any Entity.
/// </summary>
public abstract class EntityComponent
{
    /// <summary>
    /// The Entity that owns this component.
    /// Set automatically when the component is attached to an entity.
    /// </summary>
    public Entity Owner { get; internal set; } = null!;

    /// <summary>
    /// Called when the component is attached to an entity.
    /// Override to perform initialization that requires access to the owning entity.
    /// </summary>
    public virtual void OnAttach()
    {
    }

    /// <summary>
    /// Called when the component is detached from an entity.
    /// Override to perform cleanup or resource disposal.
    /// </summary>
    public virtual void OnDetach()
    {
    }

    /// <summary>
    /// Called every frame while the component is attached to an active entity.
    /// Override to implement per-frame update logic.
    /// </summary>
    /// <param name="gameTime">Provides timing information.</param>
    public virtual void Update(GameTime gameTime)
    {
    }
}
