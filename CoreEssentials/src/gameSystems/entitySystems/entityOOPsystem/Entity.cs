using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Events;

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
    /// Gets or sets the position of the entity in the game world.
    /// When this entity has a parent, the getter returns the world position (parent position + local offset).
    /// The setter always sets the local world position (independent of parent).
    /// Use <see cref="LocalPosition"/> to set the offset relative to the parent.
    /// </summary>
    public Vector2 Position
    {
        get
        {
            if (Parent != null)
                return Parent.Position + LocalPosition;
            return _position;
        }
        set => _position = value;
    }

    /// <summary>
    /// The rotation of the entity in radians.
    /// </summary>
    protected float _rotation = 0f;

    /// <summary>
    /// Gets or sets the rotation of the entity in radians.
    /// When this entity has a parent, the getter returns the world rotation (parent rotation + local offset).
    /// The setter always sets the local rotation value.
    /// Use <see cref="LocalRotation"/> to set the rotation offset relative to the parent.
    /// </summary>
    public float Rotation
    {
        get
        {
            if (Parent != null)
                return Parent.Rotation + LocalRotation;
            return _rotation;
        }
        set => _rotation = value;
    }

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
    /// This is <see langword="null"/> until the entity is added to a system.
    /// </summary>
    protected EntitySystem? EntitySystem;

    /// <summary>
    /// The collection of tags assigned to this entity.
    /// Tags are case-insensitive and provide a simple way to group entities.
    /// </summary>
    public HashSet<string> Tags { get; }

    /// <summary>
    /// The collection of event handlers subscribed by this entity.
    /// Used for auto-cleanup when the entity is destroyed.
    /// </summary>
    private readonly List<(string EventName, EntityEventHandler Handler)> _eventSubscriptions = new();

    /// <summary>
    /// The parent entity of this entity in the hierarchy.
    /// <see langword="null"/> if this entity has no parent.
    /// </summary>
    public Entity? Parent { get; private set; }

    /// <summary>
    /// The collection of child entities in this entity's hierarchy.
    /// </summary>
    public List<Entity> Children { get; } = new();

    /// <summary>
    /// The local position offset relative to the parent entity.
    /// Only meaningful when this entity has a parent.
    /// </summary>
    public Vector2 LocalPosition { get; set; } = Vector2.Zero;

    /// <summary>
    /// The local rotation offset relative to the parent entity in radians.
    /// Only meaningful when this entity has a parent.
    /// </summary>
    public float LocalRotation { get; set; } = 0f;

    /// <summary>
    /// The current texture used for rendering this entity.
    /// Used by the render batching system to group entities by texture for efficient drawing.
    /// </summary>
    public Texture2D? Texture { get; set; }

    /// <summary>
    /// Gets whether the texture has changed since the last render preparation.
    /// Set to <see langword="false"/> during render preparation to indicate the texture has been processed.
    /// </summary>
    public bool TextureChanged { get; set; }

    /// <summary>
    /// Sets the texture for this entity and marks it as changed.
    /// </summary>
    /// <param name="texture">The texture to use for rendering.</param>
    public void SetTexture(Texture2D? texture)
    {
        Texture = texture;
        TextureChanged = true;
    }

    /// <summary>
    /// Adds a tag to this entity.
    /// </summary>
    /// <param name="tag">The tag to add.</param>
    /// <exception cref="ArgumentNullException">Thrown when tag is null or whitespace.</exception>
    public void SetTag(string tag)
    {
        if (string.IsNullOrWhiteSpace(tag))
            throw new ArgumentNullException(nameof(tag), "Tag cannot be null or whitespace.");
        Tags.Add(tag);
        EntitySystem?.OnEntityTagAdded(this, tag);
    }

    /// <summary>
    /// Removes a tag from this entity.
    /// </summary>
    /// <param name="tag">The tag to remove.</param>
    /// <returns>True if the tag was removed; false if the tag was not found.</returns>
    public bool RemoveTag(string tag)
    {
        if (string.IsNullOrWhiteSpace(tag))
            return false;
        var removed = Tags.Remove(tag);
        if (removed)
            EntitySystem?.OnEntityTagRemoved(this, tag);
        return removed;
    }

    /// <summary>
    /// Checks if this entity has the specified tag.
    /// </summary>
    /// <param name="tag">The tag to check for.</param>
    /// <returns>True if the entity has the tag; otherwise, false.</returns>
    public bool HasTag(string tag)
    {
        if (string.IsNullOrWhiteSpace(tag))
            return false;
        return Tags.Contains(tag);
    }

    /// <summary>
    /// Initializes a new instance of the Entity class.
    /// </summary>
    protected Entity()
    {
        _position = Vector2.Zero;
        sort = -1;
        _destroyed = false;
        _active = true;
        Tags = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
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
    /// <param name="_spriteBatch">The SpriteBatch used for drawing.</param>
    public virtual void Render(SpriteBatch _spriteBatch) { }

    /// <summary>
    /// Marks the entity for destruction.
    /// The entity will be removed from the system on the next update.
    /// All children are also destroyed when their parent is destroyed.
    /// </summary>
    public void Destroy()
    {
        _destroyed = true;
        _active = false;
        // Destroy all children recursively
        foreach (var child in Children)
        {
            child.Destroy();
        }
    }
    
    /// <summary>
    /// Called by Entity Sy System when the entity is destroyed.
    /// Override this method to implement custom cleanup logic.
    /// </summary>
    public virtual void OnDestroy()
    {
        // Auto-unsubscribe from all events
        var eventSystem = EntityEventSystem.Instance;
        if (eventSystem != null)
        {
            foreach (var (eventName, handler) in _eventSubscriptions)
            {
                eventSystem.Unsubscribe(this, eventName, handler);
            }
        }
        _eventSubscriptions.Clear();
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
    public virtual void SetActive(bool active)
    {
        _active = active;
        // Propagate activation state to all children
        foreach (var child in Children)
        {
            child.SetActive(active);
        }
    }

    /// <summary>
    /// Gets whether the entity is currently active.
    /// </summary>
    /// <returns>True if the entity is active; otherwise, false.</returns>
    public virtual bool GetActive() { return _active; }

    /// <summary>
    /// Subscribes to an event with a handler. The subscription is automatically removed when this entity is destroyed.
    /// </summary>
    /// <param name="eventName">The name of the event to subscribe to.</param>
    /// <param name="handler">The handler to invoke when the event is raised.</param>
    public void Subscribe(string eventName, EntityEventHandler handler)
    {
        if (EntitySystem == null)
            throw new InvalidOperationException("Cannot subscribe to events before the entity is added to an EntitySystem.");

        var eventSystem = EntityEventSystem.Instance;
        if (eventSystem == null)
            throw new InvalidOperationException("Cannot subscribe to events before the EntityEventSystem is initialized.");

        _eventSubscriptions.Add((eventName, handler));
        eventSystem.Subscribe(this, eventName, handler);
    }

    /// <summary>
    /// Publishes an event from this entity.
    /// </summary>
    /// <param name="eventName">The name of the event to publish.</param>
    /// <param name="args">The event arguments.</param>
    public void Publish(string eventName, EntityEventArgs args)
    {
        if (EntitySystem == null)
            throw new InvalidOperationException("Cannot publish events before the entity is added to an EntitySystem.");

        var eventSystem = EntityEventSystem.Instance;
        if (eventSystem == null)
            throw new InvalidOperationException("Cannot publish events before the EntityEventSystem is initialized.");

        eventSystem.Publish(this, eventName, args);
    }

    /// <summary>
    /// Unsubscribes from an event with a specific handler.
    /// </summary>
    /// <param name="eventName">The name of the event to unsubscribe from.</param>
    /// <param name="handler">The handler to remove.</param>
    public void Unsubscribe(string eventName, EntityEventHandler handler)
    {
        var eventSystem = EntityEventSystem.Instance;
        if (eventSystem == null)
            throw new InvalidOperationException("Cannot unsubscribe from events before the EntityEventSystem is initialized.");

        eventSystem.Unsubscribe(this, eventName, handler);
        _eventSubscriptions.RemoveAll(s => s.EventName == eventName && s.Handler == handler);
    }

    /// <summary>
    /// Adds the specified entity as a child of this entity.
    /// </summary>
    /// <param name="child">The entity to add as a child.</param>
    /// <exception cref="ArgumentNullException">Thrown when child is null.</exception>
    /// <exception cref="ArgumentException">Thrown when attempting to add this entity as its own child or creating a circular reference.</exception>
    public void AddChild(Entity child)
    {
        if (child == null)
            throw new ArgumentNullException(nameof(child), "Child cannot be null.");

        if (child == this)
            throw new ArgumentException("An entity cannot be its own parent.", nameof(child));

        // Prevent circular references: check if child is already an ancestor of this
        Entity? ancestor = this.Parent;
        while (ancestor != null)
        {
            if (ancestor == child)
                throw new ArgumentException("Cannot add entity as its own descendant. This would create a circular reference.", nameof(child));
            ancestor = ancestor.Parent;
        }

        // Prevent circular references: check if this is already an ancestor of child
        ancestor = child.Parent;
        while (ancestor != null)
        {
            if (ancestor == this)
                throw new ArgumentException("Cannot add entity as its own descendant. This would create a circular reference.", nameof(child));
            ancestor = ancestor.Parent;
        }

        // Remove from previous parent if any
        if (child.Parent != null)
            child.Parent.Children.Remove(child);
        child.Parent = this;
        Children.Add(child);
    }

    /// <summary>
    /// Removes the specified entity from this entity's children.
    /// </summary>
    /// <param name="child">The entity to remove as a child.</param>
    /// <returns>True if the child was removed; false if the child was not found.</returns>
    public bool RemoveChild(Entity child)
    {
        if (child == null)
            return false;

        if (!Children.Remove(child))
            return false;

        child.Parent = null;
        return true;
    }
}
