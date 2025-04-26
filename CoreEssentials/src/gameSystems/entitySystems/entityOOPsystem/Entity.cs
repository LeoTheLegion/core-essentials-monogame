using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem;

/// <summary>
/// Base class for all game entities in the object-oriented entity system.
/// Provides core functionality for positioning, rendering, updating, and lifecycle management.
/// </summary>
public abstract class Entity
{
    /// <summary>
    /// The position of the entity in the game world.
    /// </summary>
    protected Vector2 _position;

    /// <summary>
    /// The sort order of the entity, used to determine rendering order.
    /// Higher values are rendered first (further back in the scene).
    /// </summary>
    protected int sort = -1;

    /// <summary>
    /// Flag indicating whether the entity has been destroyed.
    /// </summary>
    protected bool _destroyed;

    /// <summary>
    /// Flag indicating whether the entity is currently active.
    /// Inactive entities are not updated or rendered.
    /// </summary>
    protected bool _active;

    /// <summary>
    /// Flag indicating whether the entity has started.
    /// </summary>
    private bool _hasStarted = false;

    /// <summary>
    /// Gets whether the entity has been destroyed.
    /// </summary>
    public bool Destroyed => _destroyed;

    /// <summary>
    /// Gets whether the entity has started.
    /// </summary>
    public bool HasStarted => _hasStarted;

    /// <summary>
    /// The EntitySystem that manages this entity.
    /// </summary>
    protected EntitySystem EntitySystem;

    /// <summary>
    /// Initializes a new instance of the Entity class.
    /// </summary>
    protected Entity()
    {
        _position = Vector2.Zero;
        sort = -1;
        _destroyed = false;
        _active = true;
    }

    /// <summary>
    /// Sets the parent game system for this entity.
    /// </summary>
    /// <param name="entitySystem">The entity system that will manage this entity.</param>
    public void SetGameSystem(EntitySystem entitySystem)
    {
        EntitySystem = entitySystem;
    }

    /// <summary>
    /// Called when the entity is first created.
    /// Override this method to initialize entity-specific data.
    /// </summary>
    public virtual void OnStart()
    {
        _hasStarted = true;
    }

    /// <summary>
    /// Updates the entity's state.
    /// Called once per frame for active entities.
    /// </summary>
    /// <param name="gameTime">Provides a snapshot of timing values.</param>
    public virtual void Update(GameTime gameTime) { }

    /// <summary>
    /// Renders the entity.
    /// Called once per frame for active entities during the draw phase.
    /// </summary>
    /// <param name="spriteBatch">The SpriteBatch used for drawing.</param>
    public virtual void Render(SpriteBatch _spriteBatch) { }

    /// <summary>
    /// Marks the entity for destruction.
    /// The entity will be removed from the system on the next update.
    /// </summary>
    public void Destroy()
    {
        _destroyed = true;
        _active = false;
    }
    
    /// <summary>
    /// Called by Entity Sytem when the entity is destroyed.
    /// Override this method to implement custom cleanup logic.
    /// </summary>
    public virtual void OnDestroy()
    {
        // Cleanup logic for when the entity is destroyed.
        // Override this method in derived classes to implement custom cleanup.
    }

    /// <summary>
    /// Sets the sort order of the entity.
    /// </summary>
    /// <param name="x">The new sort order value.</param>
    /// <returns>The current entity instance.</returns>
    public virtual Entity SetSort(int x)
    {
        sort = x;
        return this;
    }

    /// <summary>
    /// Gets the current sort order of the entity.
    /// </summary>
    /// <returns>The sort order value.</returns>
    public virtual int GetSort() { return sort; }

    /// <summary>
    /// Sets whether the entity is active.
    /// </summary>
    /// <param name="active">True to activate, false to deactivate.</param>
    public virtual void SetActive(bool active) => _active = active;

    /// <summary>
    /// Gets whether the entity is currently active.
    /// </summary>
    /// <returns>True if the entity is active; otherwise, false.</returns>
    public virtual bool GetActive() { return _active; }
}
