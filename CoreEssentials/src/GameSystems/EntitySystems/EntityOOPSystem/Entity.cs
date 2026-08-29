using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using CoreEssentials.Assets;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Components.BuiltIn;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Events;
using CoreEssentials.Coroutines;

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
    /// Gets or sets the scale of the entity. Default is (1, 1).
    /// </summary>
    public virtual Vector2 Scale { get; set; } = Vector2.One;

    /// <summary>
    /// The sort order of the entity, used to determine rendering order.
    /// Higher values are rendered first (further back in the scene).
    /// </summary>
    protected int sort;

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
    /// Protected so derived classes can check startup state if needed.
    /// </summary>
    protected bool _hasStarted = false;

    /// <summary>
    /// Flag indicating whether the entity has awoken (OnAwake has been called).
    /// Guards against double-awake when an entity is re-added to a system.
    /// </summary>
    protected bool _hasAwoken = false;

    /// <summary>
    /// The identifier for the delayed destroy coroutine, if one is active.
    /// </summary>
    private Guid? _destroyCoroutineId;

    /// <summary>
    /// The position where this entity will respawn, if respawn is scheduled.
    /// </summary>
    internal Vector2? _respawnPosition;

    /// <summary>
    /// The delay before this entity respawns, if respawn is scheduled.
    /// </summary>
    internal TimeSpan? _respawnDelay;

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
    /// Gets the EntitySystem that manages this entity.
    /// Public so components can reach system-level services (spawn, destroy, queries,
    /// SendMessage) and, through <c>Game</c>, the MainGame/SceneManager chain.
    /// </summary>
    public EntitySystem? GetEntitySystem() => EntitySystem;

    /// <summary>
    /// Sends a scene-wide message: every entity in this entity's system (and its components)
    /// with a public instance method named <paramref name="message"/> is invoked — Unity SendMessage style.
    /// </summary>
    /// <param name="message">The name of the handler methods to invoke.</param>
    /// <param name="payload">Optional payload delivered to single-parameter handlers.</param>
    /// <returns>The number of handlers invoked, or -1 if the entity is not in a system.</returns>
    public int SendMessage(string message, object? payload = null)
        => EntitySystem?.SendMessage(message, payload) ?? -1;

    /// <summary>
    /// Creates a new entity of the specified type in this entity's system — Unity-style one-liner:
    /// <c>CreateGameObject&lt;Ball&gt;()</c>. Pairs with <see cref="Destroy"/>.
    /// </summary>
    /// <typeparam name="T">The concrete Entity type to create.</typeparam>
    /// <param name="args">Constructor arguments for the entity.</param>
    /// <returns>The newly created entity, or null if this entity is not in a system.</returns>
    public T? CreateGameObject<T>(params object[] args) where T : Entity
        => EntitySystem?.CreateEntity<T>(args);

    /// <summary>
    /// Instantiates a registered template (prefab) at the given position in this entity's system:
    /// <c>InstantiateTemplate("popup", position)</c>. Pairs with <see cref="Destroy"/>.
    /// </summary>
    /// <param name="templateName">The name of the registered template to instantiate.</param>
    /// <param name="position">The world position to place the instantiated entity.</param>
    /// <returns>The newly created entity, or null if this entity is not in a system.</returns>
    public Entity? InstantiateTemplate(string templateName, Vector2 position)
        => EntitySystem?.Instantiate(templateName, position);

    /// <summary>
    /// The unique identifier for this entity.
    /// Used for XML-driven scene loading and cross-entity references.
    /// </summary>
    private string? _id;

    /// <summary>
    /// Gets the unique identifier for this entity.
    /// Returns null if no ID has been assigned.
    /// </summary>
    public string? Id => _id;

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
    /// The dictionary of components attached to this entity.
    /// Keys are component types, values are component instances.
    /// Only one component of each type can be attached to an entity.
    /// </summary>
    private readonly Dictionary<Type, Components.EntityComponent> _components = new();

    /// <summary>
    /// Gets all components attached to this entity.
    /// </summary>
    public IEnumerable<Components.EntityComponent> Components => _components.Values;

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
    /// The current texture asset used for instanced rendering batching.
    /// Entities sharing the same <see cref="BatchTexture"/> are rendered in a single SpriteBatch call.
    /// </summary>
    public Texture2DAsset? BatchTexture { get; set; }

    /// <summary>
    /// Gets whether the batch texture has changed since the last render preparation.
    /// Set to <see langword="false"/> during render preparation to indicate the texture has been processed.
    /// </summary>
    public bool BatchTextureDirty { get; set; }

    /// <summary>
    /// Registers a texture asset for instanced rendering on this entity.
    /// Entities sharing the same texture are batched together for efficient drawing.
    /// </summary>
    /// <param name="texture">The texture asset to use for instanced rendering.</param>
    public void RegisterForInstancedRendering(Texture2DAsset? texture)
    {
        BatchTexture = texture;
        BatchTextureDirty = true;
    }

    /// <summary>
    /// Registers a sprite's texture for instanced rendering on this entity.
    /// If the sprite uses a SpriteSheet, the texture will be null and batching won't apply.
    /// </summary>
    /// <param name="sprite">The sprite to extract the texture from.</param>
    public void RegisterForInstancedRendering(Sprite sprite)
    {
        RegisterForInstancedRendering(sprite.Texture);
    }

    /// <summary>
    /// Sets the unique identifier for this entity.
    /// </summary>
    /// <param name="id">The unique identifier to assign.</param>
    /// <exception cref="ArgumentNullException">Thrown when id is null or whitespace.</exception>
    public void SetId(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentNullException(nameof(id), "ID cannot be null or whitespace.");
        _id = id;
        EntitySystem?.OnEntityIdChanged(this, id);
    }

    /// <summary>
    /// Generates a unique identifier for this entity if one hasn't been assigned.
    /// The generated ID follows the pattern "{TypeName}_xxxxxxxx" where x is 8 chars of a GUID.
    /// </summary>
    /// <returns>The assigned or generated ID.</returns>
    internal string EnsureId()
    {
        if (string.IsNullOrEmpty(_id))
        {
            var typeName = GetType().Name;
            var shortGuid = Guid.NewGuid().ToString("N").Substring(0, 8);
            _id = $"{typeName}_{shortGuid}";
        }
        return _id;
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
    /// Called once when the entity is added to its <see cref="EntitySystem"/>.
    /// Override this method for one-time initialization that must happen before <see cref="OnStart"/>.
    /// This method guards against double-awake — if the entity has already awoken, it returns immediately.
    /// <see cref="OnEnable"/> is fired by the <see cref="EntitySystem"/> after this method fully
    /// completes, so the derived body runs before OnEnable (matching Unity's Awake -> OnEnable order).
    /// </summary>
    public virtual void OnAwake()
    {
        if (_hasAwoken) return;
        _hasAwoken = true;
    }

    /// <summary>
    /// Called when the entity is first created.
    /// Override this method to initialize entity-specific data.
    /// This method guards against double-starts — if the entity has already started, it returns immediately.
    /// </summary>
    public virtual void OnStart()
    {
        if (_hasStarted) return;
        _hasStarted = true;
    }

    /// <summary>
    /// Called when the entity transitions from inactive to active (via <see cref="SetActive"/>).
    /// Override this method to perform work when the entity becomes active.
    /// Only fires on real state transitions — calling <see cref="SetActive"/> with the
    /// current state does not re-trigger this hook.
    /// </summary>
    public virtual void OnEnable()
    {
    }

    /// <summary>
    /// Called when the entity transitions from active to inactive (via <see cref="SetActive"/>).
    /// Override this method to perform cleanup when the entity becomes inactive.
    /// Only fires on real state transitions — calling <see cref="SetActive"/> with the
    /// current state does not re-trigger this hook.
    /// </summary>
    public virtual void OnDisable()
    {
    }

    /// <summary>
    /// Called after <see cref="Update"/> on every frame, for active entities.
    /// By default this drives <see cref="Components.EntityComponent.LateUpdate"/> on every
    /// attached component, so components can react to the final state of the frame (e.g.
    /// camera sync). Override and call <c>base.OnLateUpdate</c> to add entity-level logic.
    /// </summary>
    /// <param name="gameTime">Provides timing information.</param>
    public virtual void OnLateUpdate(GameTime gameTime)
    {
        foreach (var component in _components.Values)
        {
            component.LateUpdate(gameTime);
        }
    }

    /// <summary>
    /// Called on the fixed timestep for active entities.
    /// Override this method for logic that must run at a consistent rate
    /// regardless of frame rate (e.g. physics-adjacent movement).
    /// </summary>
    /// <param name="gameTime">Provides timing information.</param>
    public virtual void OnFixedUpdate(GameTime gameTime)
    {
    }

    /// <summary>
    /// Called app-wide when the application loses or regains focus.
    /// Override this method to pause or resume entity-specific behavior
    /// (e.g. stopping timers, saving state) when the game is backgrounded.
    /// </summary>
    /// <param name="paused">True when the application is being paused, false when resuming.</param>
    public virtual void OnApplicationPause(bool paused)
    {
    }

    /// <summary>
    /// Updates the entity's state.
    /// Called once per frame for active entities.
    /// Also updates all attached components.
    /// </summary>
    /// <param name="gameTime">Provides a snapshot of timing values.</param>
    public virtual void Update(GameTime gameTime)
    {
        foreach (var component in _components.Values)
        {
            component.Update(gameTime);
        }
    }

    /// <summary>
    /// Renders the entity.
    /// Called once per frame for active entities during the draw phase.
    /// By default this draws every attached component that implements
    /// <see cref="Components.IDrawableComponent"/>, so entities can render purely from
    /// components (e.g. a <see cref="SpriteComponent"/>) without an override.
    /// </summary>
    /// <param name="_spriteBatch">The SpriteBatch used for drawing.</param>
    public virtual void Render(SpriteBatch _spriteBatch)
    {
        foreach (var component in _components.Values)
        {
            if (component is Components.IDrawableComponent drawable)
                drawable.Draw(_spriteBatch);
        }
    }

    /// <summary>
    /// Gets the logical size of the entity in pixels, including the current <see cref="Scale"/>.
    /// Resolves the size from the entity's <see cref="SpriteComponent"/> (the single source of
    /// truth for rendering, whether static or driven by an <see cref="AnimationComponent"/>).
    /// Entities that render their own sprite (OOP-style, without a <see cref="SpriteComponent"/>)
    /// should override this method to return their actual rendered size.
    /// </summary>
    /// <returns>The entity size in pixels, or <see cref="Vector2.Zero"/> when no sprite is available.</returns>
    public virtual Vector2 GetSize()
    {
        if (TryGetComponent<SpriteComponent>(out var spriteComponent)
            && spriteComponent?.Sprite != null)
        {
            try
            {
                return spriteComponent.Sprite.GetSize() * Scale;
            }
            catch (InvalidOperationException)
            {
                // Sprite metadata not loaded yet.
            }
        }

        return Vector2.Zero;
    }

    /// <summary>
    /// Gets the pixel origin (pivot) of the entity's rendered sprite, including the current <see cref="Scale"/>.
    /// This is the point that is placed at <see cref="Position"/>, so the top-left corner of the
    /// rendered sprite sits at <c>Position - GetOrigin()</c>.
    /// Resolves the origin from the entity's <see cref="SpriteComponent"/>, mirroring <see cref="GetSize"/>.
    /// </summary>
    /// <returns>The entity origin in pixels, or <see cref="Vector2.Zero"/> when no sprite is available.</returns>
    public virtual Vector2 GetOrigin()
    {
        if (TryGetComponent<SpriteComponent>(out var spriteComponent)
            && spriteComponent?.Sprite != null)
        {
            try
            {
                return spriteComponent.Sprite.GetOrigin() * Scale;
            }
            catch (InvalidOperationException)
            {
                // Sprite metadata not loaded yet.
            }
        }

        return Vector2.Zero;
    }

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
    /// Schedules the entity for destruction after a specified delay.
    /// Uses the coroutine system for timing. The delayed destruction can be cancelled before it expires.
    /// </summary>
    /// <param name="delay">The time to wait before destroying the entity.</param>
    /// <exception cref="ArgumentNullException">Thrown when delay is negative.</exception>
    public void DestroyAfter(TimeSpan delay)
    {
        if (delay.TotalMilliseconds < 0)
            throw new ArgumentOutOfRangeException(nameof(delay), "Delay cannot be negative.");

        // Cancel any existing delayed destroy
        CancelDestroyAfter();

        _destroyCoroutineId = CoroutineManager.StartCoroutine(DestroyAfterRoutine(delay));
    }

    /// <summary>
    /// Cancels a pending delayed destruction. Does nothing if no delayed destruction is scheduled.
    /// </summary>
    /// <returns>True if a delayed destroy was cancelled; otherwise, false.</returns>
    public bool CancelDestroyAfter()
    {
        if (_destroyCoroutineId.HasValue)
        {
            CoroutineManager.StopCoroutine(_destroyCoroutineId.Value);
            _destroyCoroutineId = null;
            return true;
        }
        return false;
    }

    /// <summary>
    /// Coroutine that waits for the specified delay and then destroys the entity.
    /// </summary>
    /// <param name="delay">The time to wait before destroying the entity.</param>
    private IEnumerator DestroyAfterRoutine(TimeSpan delay)
    {
        yield return new WaitForSeconds((float)delay.TotalSeconds);
        _destroyCoroutineId = null;
        Destroy();
    }

    /// <summary>
    /// Configures this entity to automatically respawn at the specified position after destruction and a delay.
    /// Once triggered, this one-time respawn will fire. Call again for multiple respawns.
    /// </summary>
    /// <param name="position">The position to respawn at.</param>
    /// <param name="delay">The time to wait between destruction and respawn.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when delay is negative.</exception>
    public void RespawnAt(Vector2 position, TimeSpan delay)
    {
        if (delay.TotalMilliseconds < 0)
            throw new ArgumentOutOfRangeException(nameof(delay), "Delay cannot be negative.");

        _respawnPosition = position;
        _respawnDelay = delay;
    }

    /// <summary>
    /// Gets whether this entity has a pending respawn scheduled.
    /// </summary>
    public bool HasPendingRespawn => _respawnPosition.HasValue && _respawnDelay.HasValue;

    /// <summary>
    /// Cancels any pending respawn configuration. Does nothing if no respawn is scheduled.
    /// </summary>
    /// <returns>True if a respawn was cancelled; otherwise, false.</returns>
    public bool CancelRespawnAt()
    {
        if (HasPendingRespawn)
        {
            _respawnPosition = null;
            _respawnDelay = null;
            return true;
        }
        return false;
    }

    /// <summary>
    /// Called by Entity System when the entity is destroyed.
    /// Override this method to implement custom cleanup logic.
    /// </summary>
    public virtual void OnDestroy()
    {
        // Cancel any pending delayed destroy coroutine
        CancelDestroyAfter();

        // Detach all components
        foreach (var component in _components.Values)
        {
            component.OnDetach();
            component.Owner = null!;
        }
        _components.Clear();

        // Auto-unsubscribe from all events (legacy system kept working until it is removed).
#pragma warning disable CS0618 // EntityEventSystem is obsolete; this cleanup must keep working for legacy subscribers.
        var eventSystem = EntityEventSystem.Instance;
        if (eventSystem != null)
        {
            foreach (var (eventName, handler) in _eventSubscriptions)
            {
                eventSystem.Unsubscribe(this, eventName, handler);
            }
        }
#pragma warning restore CS0618
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
    /// Gets or sets the z-order layer of the entity.
    /// Entities are rendered layer-by-layer from low to high z-layer (back to front).
    /// Within a single z-layer, entities are batched by texture and ordered by sort order.
    /// Defaults to 0 to preserve backward compatibility with texture-only batching.
    /// </summary>
    public virtual int ZLayer { get; set; }

    /// <summary>
    /// Sets the z-order layer of the entity.
    /// </summary>
    /// <param name="layer">The z-order layer value.</param>
    /// <returns>The current entity instance.</returns>
    public virtual Entity SetZLayer(int layer)
    {
        ZLayer = layer;
        return this;
    }

    /// <summary>
    /// Gets the current z-order layer of the entity.
    /// </summary>
    /// <returns>The z-order layer value.</returns>
    public virtual int GetZLayer() { return ZLayer; }

    /// <summary>
    /// Sets whether the entity is active.
    /// Fires <see cref="OnEnable"/> when transitioning to active and <see cref="OnDisable"/>
    /// when transitioning to inactive. No-op calls (setting the current state) do not fire hooks.
    /// </summary>
    /// <param name="active">True to activate, false to deactivate.</param>
    public virtual void SetActive(bool active)
    {
        if (_active == active)
            return;

        _active = active;

        // Fire lifecycle hooks on the real transition, but only once the entity is awake.
        // This keeps pooled entities (which toggle active during pool acquire before OnAwake)
        // in the correct order: Awake -> Enable -> Start.
        if (_hasAwoken)
        {
            if (active)
                OnEnable();
            else
                OnDisable();
        }

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
    [Obsolete("Use SendMessage for scene-wide messages or declarative <Bind> wiring in XML scenes. The legacy entity event system is being removed.")]
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
    [Obsolete("Use SendMessage for scene-wide messages or declarative <Bind> wiring in XML scenes. The legacy entity event system is being removed.")]
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
    [Obsolete("Use SendMessage for scene-wide messages or declarative <Bind> wiring in XML scenes. The legacy entity event system is being removed.")]
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

    /// <summary>
    /// Adds a component to this entity.
    /// </summary>
    /// <typeparam name="T">The type of component to add.</typeparam>
    /// <param name="component">The component instance to add.</param>
    /// <exception cref="ArgumentNullException">Thrown when component is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when a component of the same type already exists.</exception>
    public T AddComponent<T>(T component) where T : Components.EntityComponent
    {
        if (component == null)
            throw new ArgumentNullException(nameof(component));

        var componentType = typeof(T);
        if (_components.ContainsKey(componentType))
            throw new InvalidOperationException($"Entity already has a component of type '{componentType.Name}'. Use RemoveComponent first.");

        component.Owner = this;
        _components[componentType] = component;
        component.OnAttach();
        return component;
    }

    /// <summary>
    /// Adds a component to this entity using runtime type resolution.
    /// Use this when the component is known only as a base type (e.g., from a factory).
    /// </summary>
    public void AddComponent(Components.EntityComponent component)
    {
        if (component == null)
            throw new ArgumentNullException(nameof(component));

        var componentType = component.GetType();
        if (_components.ContainsKey(componentType))
            throw new InvalidOperationException($"Entity already has a component of type '{componentType.Name}'. Use RemoveComponent first.");

        component.Owner = this;
        _components[componentType] = component;
        component.OnAttach();
    }

    /// <summary>
    /// Gets a component of the specified type from this entity.
    /// </summary>
    /// <typeparam name="T">The type of component to get.</typeparam>
    /// <returns>The component if found; otherwise, null.</returns>
    public T? GetComponent<T>() where T : Components.EntityComponent
    {
        return _components.TryGetValue(typeof(T), out var component) ? (T)component : null;
    }

    /// <summary>
    /// Gets a component of the specified type from this entity.
    /// </summary>
    /// <param name="type">The type of component to get.</param>
    /// <returns>The component if found; otherwise, null.</returns>
    public Components.EntityComponent? GetComponent(Type type)
    {
        return _components.TryGetValue(type, out var component) ? component : null;
    }

    /// <summary>
    /// Gets a component of the specified type from this entity.
    /// </summary>
    /// <typeparam name="T">The type of component to get.</typeparam>
    /// <param name="component">When this method returns, contains the component if found; otherwise, null.</param>
    /// <returns>True if the component was found; otherwise, false.</returns>
    public bool TryGetComponent<T>(out T? component) where T : Components.EntityComponent
    {
        component = GetComponent<T>();
        return component != null;
    }

    /// <summary>
    /// Checks if this entity has a component of the specified type.
    /// </summary>
    /// <typeparam name="T">The type of component to check for.</typeparam>
    /// <returns>True if the entity has the component; otherwise, false.</returns>
    public bool HasComponent<T>() where T : Components.EntityComponent
    {
        return _components.ContainsKey(typeof(T));
    }

    /// <summary>
    /// Removes a component of the specified type from this entity.
    /// </summary>
    /// <typeparam name="T">The type of component to remove.</typeparam>
    /// <returns>The removed component if found; otherwise, null.</returns>
    public T? RemoveComponent<T>() where T : Components.EntityComponent
    {
        if (_components.TryGetValue(typeof(T), out var component))
        {
            component.OnDetach();
            component.Owner = null!;
            _components.Remove(typeof(T));
            return (T)component;
        }
        return null;
    }
}
