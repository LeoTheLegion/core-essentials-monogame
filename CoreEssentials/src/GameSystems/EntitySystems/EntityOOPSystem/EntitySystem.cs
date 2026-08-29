using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using CoreEssentials.Assets;
using CoreEssentials.Camera;
using CoreEssentials.Debugging;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Spatial;
using CoreEssentials.Coroutines;

namespace CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem;

/// <summary>
/// Manages game entities in an object-oriented architecture.
/// Handles the creation, updating, rendering, and destruction of entities.
/// </summary>
public class EntitySystem : GameSystem, IUpdateGameSystem, IDrawGameSystem, IFixedUpdateGameSystem, IPausableGameSystem, IDisposable
{
    /// <summary>
    /// The list of all entities managed by this system.
    /// </summary>
    private readonly List<Entity> _entities = new();

    /// <summary>
    /// Dictionary for O(1) tag-based entity lookups.
    /// Maps tag names to lists of entities with that tag.
    /// </summary>
    private readonly Dictionary<string, List<Entity>> _tagIndex = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Dictionary for O(1) ID-based entity lookups.
    /// Maps entity IDs to entities.
    /// </summary>
    private readonly Dictionary<string, Entity> _idIndex = new Dictionary<string, Entity>(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Dictionary of entity pools, keyed by type name.
    /// </summary>
    private readonly Dictionary<Type, object> _pools = new();

    /// <summary>
    /// Cache of registered entity templates for fast instantiation.
    /// Maps template names to their corresponding EntityTemplate blueprint.
    /// </summary>
    private readonly Dictionary<string, Serialization.EntityTemplate> _templates = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// The spatial grid for fast spatial queries.
    /// </summary>
    private SpatialGrid? _spatialGrid;

    /// <summary>
    /// Dictionary of pending spawns keyed by unique spawn ID for cancellation support.
    /// Maps spawn IDs to the scheduled spawn action.
    /// </summary>
    private readonly Dictionary<Guid, Action> _pendingSpawns = new();

    /// <summary>
    /// Gets or sets whether spatial partitioning is enabled.
    /// When enabled, entities are automatically tracked in the spatial grid for fast spatial queries.
    /// </summary>
    public bool SpatialPartitioningEnabled { get; set; } = true;

    /// <summary>
    /// Gets the cell size used by the spatial grid (default: 100).
    /// </summary>
    public float SpatialCellSize { get; set; } = 100f;

    /// <summary>
    /// Gets or sets whether debug mode is enabled.
    /// When enabled, debug overlays are rendered after entity drawing.
    /// </summary>
    public bool DebugMode { get; set; }

    /// <summary>
    /// Gets the debug configuration for controlling which overlays are displayed.
    /// </summary>
    public DebugConfig DebugConfig { get; } = new DebugConfig();

    /// <summary>
    /// Gets or sets the optional font asset used for rendering text debug overlays.
    /// If null, text overlays (IDs, tags) will be skipped.
    /// </summary>
    public FontAsset? DebugFont { get; set; }

    private EntityDebugDraw? _debugDraw;

    /// <summary>
    /// Gets the entity debug draw helper (lazy-initialized when debug mode is enabled).
    /// </summary>
    private EntityDebugDraw DebugDraw => _debugDraw ??= new EntityDebugDraw(DebugConfig);

    /// <summary>
    /// Initializes a new instance of the EntitySystem class.
    /// </summary>
    public EntitySystem()
    {
    }

    /// <summary>
    /// Updates all active entities and removes destroyed entities.
    /// </summary>
    /// <param name="gameTime">Provides a snapshot of timing values.</param>
    public void Update(GameTime gameTime)
    {
        SortEntities();
        UpdateActiveEntities(gameTime);
        UpdateLateActiveEntities(gameTime);
        UpdateSpatialGridPositions();
        RemoveDestroyedEntities();
    }

    /// <summary>
    /// Updates all active entities.
    /// </summary>
    private void UpdateActiveEntities(GameTime gameTime)
    {
        for (int i = 0; i < _entities.Count; i++)
        {
            Entity entity = _entities[i];
            if (entity.GetActive())
                entity.Update(gameTime);
        }
    }

    /// <summary>
    /// Updates all active entities on the fixed timestep, calling <see cref="Entity.OnFixedUpdate"/>.
    /// </summary>
    /// <param name="gameTime">Provides a snapshot of timing values.</param>
    public void FixedUpdate(GameTime gameTime)
    {
        for (int i = 0; i < _entities.Count; i++)
        {
            Entity entity = _entities[i];
            if (entity.GetActive())
                entity.OnFixedUpdate(gameTime);
        }
    }

    /// <summary>
    /// Updates all active entities' late-update hook, calling <see cref="Entity.OnLateUpdate"/>.
    /// Runs after the regular update pass so late-update logic sees the final state of the frame.
    /// </summary>
    /// <param name="gameTime">Provides a snapshot of timing values.</param>
    private void UpdateLateActiveEntities(GameTime gameTime)
    {
        for (int i = 0; i < _entities.Count; i++)
        {
            Entity entity = _entities[i];
            if (entity.GetActive())
                entity.OnLateUpdate(gameTime);
        }
    }

    /// <summary>
    /// Called app-wide when the application is paused or resumed (e.g. window loses/regains focus).
    /// Forwards the call to <see cref="Entity.OnApplicationPause"/> on every active entity.
    /// </summary>
    /// <param name="paused">True when the application is being paused, false when resuming.</param>
    public void OnApplicationPause(bool paused)
    {
        for (int i = 0; i < _entities.Count; i++)
        {
            Entity entity = _entities[i];
            if (entity.GetActive())
                entity.OnApplicationPause(paused);
        }
    }

    /// <summary>
    /// Auto-updates spatial grid positions for entities that have moved.
    /// </summary>
    private void UpdateSpatialGridPositions()
    {
        if (!SpatialPartitioningEnabled || _spatialGrid == null)
            return;

        for (int i = 0; i < _entities.Count; i++)
        {
            Entity entity = _entities[i];
            if (entity.GetActive())
                _spatialGrid.UpdatePosition(entity);
        }
    }

    /// <summary>
    /// Removes destroyed entities from the system, cleaning up indexes and calling OnDestroy.
    /// </summary>
    private void RemoveDestroyedEntities()
    {
        for (int i = _entities.Count - 1; i >= 0; i--)
        {
            if (!_entities[i].Destroyed)
                continue;

            var destroyedEntity = _entities[i];
            HandlePendingRespawn(destroyedEntity);
            UpdateTagIndexForEntity(destroyedEntity, false);
            UpdateIdIndexForEntity(destroyedEntity, false);
            UpdateSpatialGridForEntity(destroyedEntity, false);
            destroyedEntity.OnDestroy();
            _entities.RemoveAt(i);
        }
    }

    /// <summary>
    /// Checks for and handles pending respawn for a destroyed entity.
    /// </summary>
    private void HandlePendingRespawn(Entity destroyedEntity)
    {
        if (!destroyedEntity.HasPendingRespawn)
            return;

        Type entityType = destroyedEntity.GetType();
        Vector2 respawnPos = destroyedEntity._respawnPosition ?? Vector2.Zero;
        TimeSpan respawnDelay = destroyedEntity._respawnDelay ?? TimeSpan.Zero;
        CoroutineManager.StartCoroutine(RespawnRoutine(entityType, respawnPos, respawnDelay));
    }

    /// <summary>
    /// Renders all active entities using z-aware texture batching.
    /// Entities are grouped by z-layer first, then by texture within each layer,
    /// to minimize SpriteBatch begin/end calls while preserving correct render order.
    /// Layers are rendered back-to-front (low to high z-layer); within each layer,
    /// entities sharing a texture are batched together and maintain their sort order.
    /// </summary>
    /// <param name="gameTime">Provides a snapshot of timing values.</param>
    /// <param name="spriteBatch">The SpriteBatch used for drawing entities.</param>
    public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
    {
        var (zLayers, noTextureEntities) = GroupEntitiesByZLayer();
        RenderNoTextureEntities(noTextureEntities, spriteBatch);
        RenderZLayers(zLayers, spriteBatch);
        ResetTextureDirtyFlags();

        // Render debug overlays on top of everything
        if (DebugMode)
        {
            DrawDebugOverlays(spriteBatch);
        }
    }

    /// <summary>
    /// Groups active entities by z-layer and, within each layer, by texture asset
    /// for efficient batched rendering. Layers are returned in ascending order
    /// (back-to-front) so they can be rendered in the correct sequence.
    /// </summary>
    private (List<KeyValuePair<int, Dictionary<Texture2DAsset, List<Entity>>>> zLayers, List<Entity> noTextureEntities) GroupEntitiesByZLayer()
    {
        var layerMap = new SortedDictionary<int, Dictionary<Texture2DAsset, List<Entity>>>();
        var noTextureEntities = new List<Entity>();

        for (int i = 0; i < _entities.Count; i++)
        {
            var entity = _entities[i];
            if (!entity.GetActive())
                continue;

            var texture = entity.BatchTexture;
            if (texture == null)
            {
                noTextureEntities.Add(entity);
                continue;
            }

            int layer = entity.GetZLayer();
            if (!layerMap.TryGetValue(layer, out var textureGroups))
            {
                textureGroups = new Dictionary<Texture2DAsset, List<Entity>>();
                layerMap[layer] = textureGroups;
            }

            if (!textureGroups.ContainsKey(texture))
                textureGroups[texture] = new List<Entity>();
            textureGroups[texture].Add(entity);
        }

        var zLayers = new List<KeyValuePair<int, Dictionary<Texture2DAsset, List<Entity>>>>();
        foreach (var layer in layerMap)
            zLayers.Add(layer);

        return (zLayers, noTextureEntities);
    }

    /// <summary>
    /// Renders entities that don't have an associated texture.
    /// </summary>
    private static void RenderNoTextureEntities(List<Entity> noTextureEntities, SpriteBatch spriteBatch)
    {
        if (noTextureEntities.Count == 0)
            return;

        var cameraView = GetCameraViewMatrix();
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
            SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone,
            null, cameraView);

        foreach (var entity in noTextureEntities)
        {
            entity.Render(spriteBatch);
        }

        spriteBatch.End();
    }

    /// <summary>
    /// Renders each z-layer back-to-front (low to high). Within a layer, each texture
    /// group is rendered with a single SpriteBatch begin/end pair, preserving batching
    /// while maintaining correct interleaving of textures across z-layers.
    /// </summary>
    private static void RenderZLayers(List<KeyValuePair<int, Dictionary<Texture2DAsset, List<Entity>>>> zLayers, SpriteBatch spriteBatch)
    {
        if (zLayers.Count == 0)
            return;

        var cameraView = GetCameraViewMatrix();

        foreach (var layer in zLayers)
        {
            foreach (var textureGroup in layer.Value)
            {
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                    SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone,
                    null, cameraView);

                foreach (var entity in textureGroup.Value)
                {
                    entity.Render(spriteBatch);
                }

                spriteBatch.End();
            }
        }
    }

    /// <summary>
    /// Gets the current camera view matrix or null if no camera is active.
    /// </summary>
    private static Matrix? GetCameraViewMatrix()
    {
        return Camera.Camera.MainCamera?.ViewMatrix;
    }

    /// <summary>
    /// Resets texture changed flags for all entities after rendering.
    /// </summary>
    private void ResetTextureDirtyFlags()
    {
        for (int i = 0; i < _entities.Count; i++)
        {
            _entities[i].BatchTextureDirty = false;
        }
    }

    /// <summary>
    /// Renders debug overlays for all active entities.
    /// Opens its own SpriteBatch scope since entity rendering batches are already closed.
    /// </summary>
    private void DrawDebugOverlays(SpriteBatch spriteBatch)
    {
        var activeEntities = new List<Entity>();
        for (int i = 0; i < _entities.Count; i++)
        {
            if (_entities[i].GetActive())
                activeEntities.Add(_entities[i]);
        }

        if (activeEntities.Count == 0)
            return;

        var camera = Camera.Camera.MainCamera;
        var hasCamera = camera != null;

        spriteBatch.Begin(
            SpriteSortMode.Deferred,
            BlendState.AlphaBlend,
            SamplerState.PointClamp,
            DepthStencilState.None,
            RasterizerState.CullNone,
            null,
            hasCamera ? camera!.ViewMatrix : null
        );

        DebugDraw.DrawOverlays(activeEntities, spriteBatch, DebugFont);

        spriteBatch.End();
    }

    /// <summary>
    /// Creates and initializes a new entity of the specified type.
    /// </summary>
    /// <param name="type">The Type of entity to create.</param>
    /// <param name="args">Constructor arguments for the entity.</param>
    /// <returns>The newly created entity.</returns>
    public Entity CreateEntity(Type type, params object[] args)
    {
        Console.WriteLine($"[EntitySystem] CreateEntity<{type.Name}>: instantiating...");
        object? instance = CreateInstanceWithOptionalParams(type, args);
        if (instance == null)
            throw new InvalidOperationException($"Failed to create entity of type {type}.");
        Entity entity = (Entity)instance;
        Console.WriteLine($"[EntitySystem]   Instantiated, setting up...");
        entity.SetGameSystem(this);
        entity.EnsureId();
        _entities.Add(entity);
        UpdateTagIndexForEntity(entity, true);
        UpdateIdIndexForEntity(entity, true);
        UpdateSpatialGridForEntity(entity, true);
        entity.OnAwake();
        NotifyAwoken(entity);
        Console.WriteLine($"[EntitySystem]   Calling OnStart for {entity.GetType().Name}...");
        try
        {
            entity.OnStart();
            Console.WriteLine($"[EntitySystem]   OnStart completed for {entity.GetType().Name} (Id={entity.Id})");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[EntitySystem]   ERROR in OnStart for {entity.GetType().Name}: {ex.GetType().Name}: {ex.Message}");
            Console.WriteLine($"[EntitySystem]   Stack: {ex.StackTrace?.Split('\n').Take(5).Aggregate((a, b) => a + "\n" + b)}");
            throw;
        }
        return entity;
    }

    /// <summary>
    /// Creates and initializes a new entity of the specified type.
    /// </summary>
    /// <typeparam name="T">The type of entity to create.</typeparam>
    /// <param name="args">Constructor arguments for the entity.</param>
    /// <returns>The newly created entity.</returns>
    public T CreateEntity<T>(params object[] args) where T : Entity
    {
        return (T)CreateEntity(typeof(T), args);
    }

    /// <summary>
    /// Creates a new entity without calling OnStart(). Use when you need to configure the entity before initialization (e.g., templates).
    /// Call <see cref="Entity.OnStart"/> manually after configuration is complete.
    /// </summary>
    /// <param name="type">The Type of entity to create.</param>
    /// <param name="args">Constructor arguments for the entity.</param>
    /// <returns>The newly created entity (not yet started).</returns>
    public Entity CreateEntityUnstarted(Type type, params object[] args)
    {
        object? instance = CreateInstanceWithOptionalParams(type, args);
        if (instance == null)
            throw new InvalidOperationException($"Failed to create entity of type {type}.");
        Entity entity = (Entity)instance;
        entity.SetGameSystem(this);
        entity.EnsureId();
        _entities.Add(entity);
        UpdateTagIndexForEntity(entity, true);
        UpdateIdIndexForEntity(entity, true);
        UpdateSpatialGridForEntity(entity, true);
        entity.OnAwake();
        NotifyAwoken(entity);
        return entity;
    }

    /// <summary>
    /// Instantiates <paramref name="type"/> with constructor resolution that supports optional parameters.
    /// Finds a public constructor whose required parameter count is at most <c>args.Length</c> and whose total
    /// parameter count is at least <c>args.Length</c>, then fills any omitted trailing arguments from their
    /// declared defaults. This mirrors how C# call sites fill optional parameters at compile time, so entities
    /// with optional-parameter constructors can be created with fewer arguments than the full signature.
    /// </summary>
    /// <param name="type">The type to instantiate.</param>
    /// <param name="args">Positional constructor arguments.</param>
    /// <returns>The newly created instance, or null if invocation produced no value.</returns>
    /// <exception cref="InvalidOperationException">Thrown when no public constructor can accept the given
    /// arguments. The message lists all available constructors to aid diagnosis.</exception>
    private static object? CreateInstanceWithOptionalParams(Type type, object[] args)
    {
        var ctor = type.GetConstructors()
            .Where(c =>
            {
                var ps = c.GetParameters();
                int requiredCount = ps.Count(p => !p.IsOptional);
                return args.Length >= requiredCount && args.Length <= ps.Length;
            })
            // Prefer the tightest match: exact arity first, then fewest filled-in defaults.
            .OrderBy(c => c.GetParameters().Length)
            .FirstOrDefault();

        if (ctor == null)
            throw new InvalidOperationException(
                $"No matching constructor on {type.FullName} for {args.Length} argument(s). " +
                $"Available constructors: {DescribeConstructors(type)}");

        var ps = ctor.GetParameters();
        var fullArgs = new object?[ps.Length];
        for (int i = 0; i < ps.Length; i++)
            fullArgs[i] = i < args.Length ? args[i] : ps[i].DefaultValue;

        return ctor.Invoke(fullArgs);
    }

    /// <summary>
    /// Builds a human-readable list of the public constructors on a type, for error messages.
    /// </summary>
    private static string DescribeConstructors(Type type)
    {
        var ctors = type.GetConstructors();
        if (ctors.Length == 0)
            return "(none)";

        return string.Join(" | ", ctors.Select(c =>
        {
            var ps = c.GetParameters();
            string signature = string.Join(", ", ps.Select(p =>
                p.IsOptional ? $"{p.ParameterType.Name} {p.Name} (optional)" : $"{p.ParameterType.Name} {p.Name}"));
            return $"({signature})";
        }));
    }

    /// <summary>
    /// Creates and initializes a new pooled entity of the specified type.
    /// The entity will be recycled back to the pool when <see cref="ReleasePooled{T}"/> is called.
    /// </summary>
    /// <typeparam name="T">The type of pooled entity to create (must implement <see cref="Pooling.IPooledEntity"/>).</typeparam>
    /// <param name="position">The position to activate the entity at.</param>
    /// <param name="args">Constructor arguments for the entity (if not using default constructor).</param>
    /// <returns>The newly created pooled entity.</returns>
    public T CreatePooled<T>(Vector2 position = default, params object[] args) where T : Entity, Pooling.IPooledEntity, new()
    {
        var pool = GetOrCreatePool<T>();
        var entity = pool.Acquire(position);
        entity.SetGameSystem(this);
        entity.EnsureId();
        _entities.Add(entity);
        UpdateTagIndexForEntity(entity, true);
        UpdateIdIndexForEntity(entity, true);
        UpdateSpatialGridForEntity(entity, true);
        entity.OnAwake();
        NotifyAwoken(entity);
        entity.OnStart();
        return entity;
    }

    /// <summary>
    /// Establishes the initial enabled state for an entity that has just awoken.
    /// If the entity is active, <see cref="Entity.OnEnable"/> is fired. This is called
    /// after <see cref="Entity.OnAwake"/> completes so the derived OnAwake body runs first,
    /// matching Unity's Awake -> OnEnable order.
    /// </summary>
    /// <param name="entity">The entity that has just awoken.</param>
    private static void NotifyAwoken(Entity entity)
    {
        if (entity.GetActive())
            entity.OnEnable();
    }

    /// <summary>
    /// Returns a pooled entity to the pool instead of destroying it.
    /// The entity becomes inactive and available for reuse.
    /// </summary>
    /// <typeparam name="T">The type of pooled entity to release.</typeparam>
    /// <param name="entity">The entity to release back to the pool.</param>
    public void ReleasePooled<T>(T entity) where T : Entity, Pooling.IPooledEntity, new()
    {
        if (entity == null)
            return;

        // Remove from entity list
        _entities.Remove(entity);
        UpdateTagIndexForEntity(entity, false);
        UpdateIdIndexForEntity(entity, false);
        UpdateSpatialGridForEntity(entity, false);

        // Return to pool
        var pool = GetOrCreatePool<T>();
        pool.Release(entity);
    }

    /// <summary>
    /// Gets or creates an entity pool for the specified type.
    /// </summary>
    /// <typeparam name="T">The type of pooled entity.</typeparam>
    /// <param name="initialCapacity">Initial pool capacity (default: 10).</param>
    /// <param name="maxSize">Maximum pool size (default: 100).</param>
    /// <returns>The entity pool for the specified type.</returns>
    public Pooling.EntityPool<T> GetOrCreatePool<T>(int initialCapacity = 10, int maxSize = 100) where T : Entity, Pooling.IPooledEntity, new()
    {
        var type = typeof(T);
        if (!_pools.TryGetValue(type, out var pool))
        {
            pool = new Pooling.EntityPool<T>(initialCapacity, maxSize);
            _pools[type] = pool;
        }
        return (Pooling.EntityPool<T>)pool;
    }

    /// <summary>
    /// Sorts entities based on their sort order.
    /// Entities with higher sort values are drawn first (further back in the scene).
    /// </summary>
    public void SortEntities()
    {
        _entities.Sort(
            (x, y) => y.GetSort().CompareTo(x.GetSort())
            );
    }

    /// <summary>
    /// Gets all entities managed by this system.
    /// </summary>
    /// <returns>The list of all entities.</returns>
    public List<Entity> GetEntities()
    {
        return _entities;
    }

    /// <summary>
    /// Sends a scene-wide message (Unity SendMessage style): every entity managed by this
    /// system, and each of their components, is searched for public instance methods named
    /// <paramref name="message"/> with zero or one parameter, and all matches are invoked.
    /// </summary>
    /// <remarks>
    /// Unlike the declarative &lt;Bind&gt; wiring (which walks a single entity's ancestor chain
    /// and stops at the first match), SendMessage reaches every matching handler in the whole
    /// scene — including entities spawned at runtime from templates. Handler exceptions are
    /// caught and logged so one bad handler cannot take down the game loop.
    /// </remarks>
    /// <param name="message">The name of the handler methods to invoke.</param>
    /// <param name="payload">Optional payload delivered to single-parameter handlers.</param>
    /// <returns>The number of handlers invoked.</returns>
    public int SendMessage(string message, object? payload = null)
    {
        if (string.IsNullOrWhiteSpace(message))
            return 0;

        var invoked = 0;

        // Snapshot so handlers that spawn/destroy entities mid-broadcast can't mutate the list.
        foreach (var root in _entities.ToList())
            InvokeMessageOnSubtree(root, message, payload, ref invoked);

        return invoked;
    }

    /// <summary>
    /// Depth-first visit of an entity and its whole child subtree, invoking matching
    /// handlers on the entity, each of its components, and every descendant — mirroring
    /// how the per-frame update loop reaches children through their roots.
    /// </summary>
    private static void InvokeMessageOnSubtree(Entity entity, string message, object? payload, ref int invoked)
    {
        InvokeMessageOn(entity, message, payload, ref invoked);
        foreach (var component in entity.Components)
            InvokeMessageOn(component, message, payload, ref invoked);

        // Snapshot children too: a handler may attach/detach children mid-broadcast.
        foreach (var child in entity.Children.ToList())
            InvokeMessageOnSubtree(child, message, payload, ref invoked);
    }

    private static void InvokeMessageOn(object target, string message, object? payload, ref int invoked)
    {
        var methods = target.GetType().GetMethods(
            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)
            .Where(m => m.Name == message && !m.IsGenericMethodDefinition
                        && (m.GetParameters().Length == 0 || m.GetParameters().Length == 1))
            .ToList();

        foreach (var method in methods)
        {
            try
            {
                if (method.GetParameters().Length == 1)
                    method.Invoke(target, new[] { payload });
                else
                    method.Invoke(target, null);

                invoked++;
            }
            catch (Exception ex)
            {
                var cause = ex is System.Reflection.TargetInvocationException tie && tie.InnerException != null
                    ? tie.InnerException
                    : ex;
                Console.WriteLine($"[EntitySystem] SendMessage handler '{method.DeclaringType?.Name}.{message}' threw: {cause.Message}");
            }
        }
    }

    /// <summary>
    /// Gets all entities with the specified tag.
    /// </summary>
    /// <param name="tag">The tag to search for.</param>
    /// <returns>A list of entities with the specified tag.</returns>
    public List<Entity> GetEntitiesByTag(string tag)
    {
        if (string.IsNullOrWhiteSpace(tag))
            return new List<Entity>();
        
        if (_tagIndex.TryGetValue(tag, out var entities))
            return new List<Entity>(entities);
        return new List<Entity>();
    }

    /// <summary>
    /// Finds the first entity with the specified tag.
    /// </summary>
    /// <param name="tag">The tag to search for.</param>
    /// <returns>The first entity with the tag, or null if not found.</returns>
    public Entity? FindByTag(string tag)
    {
        if (string.IsNullOrWhiteSpace(tag))
            return null;
        
        if (_tagIndex.TryGetValue(tag, out var entities) && entities.Count > 0)
            return entities[0];
        return null;
    }

    /// <summary>
    /// Finds an entity by its unique ID.
    /// </summary>
    /// <param name="id">The unique identifier of the entity.</param>
    /// <returns>The entity with the specified ID, or null if not found.</returns>
    public Entity? FindById(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            return null;

        return _idIndex.TryGetValue(id, out var entity) ? entity : null;
    }

    /// <summary>
    /// Gets a snapshot of the current ID index for reference resolution.
    /// </summary>
    /// <returns>A dictionary mapping entity IDs to entities.</returns>
    public Dictionary<string, Entity> GetIdIndex() => new(_idIndex);

    /// <summary>
    /// Resolves all pending entity references after scene load.
    /// Iterates over all registered <see cref="Serialization.EntityReference"/> instances and resolves them against the ID index.
    /// </summary>
    /// <returns>The number of successfully resolved references.</returns>
    public int ResolveReferences()
    {
        var entitiesList = new Dictionary<string, Entity>(_idIndex);
        int resolved = 0;

        foreach (var entity in _entities)
        {
            if (entity is Serialization.IEntityReferenceHolder holder)
            {
                resolved += holder.ResolveReferences(entitiesList);
            }
        }

        return resolved;
    }

    /// <summary>
    /// Finds all active entities of the specified type.
    /// </summary>
    /// <typeparam name="T">The type of entity to find.</typeparam>
    /// <returns>A list of all active entities of type T.</returns>
    public List<T> FindByType<T>() where T : Entity
    {
        var results = new List<T>();
        foreach (var entity in _entities)
        {
            if (entity is T typed && entity.GetActive())
                results.Add(typed);
        }
        return results;
    }

    /// <summary>
    /// Finds all active entities within the specified radius of a position.
    /// </summary>
    /// <param name="position">The center position to search around.</param>
    /// <param name="radius">The search radius.</param>
    /// <returns>A list of all active entities within the radius.</returns>
    public List<Entity> FindNearby(Vector2 position, float radius)
    {
        var squaredRadius = radius * radius;
        return _entities.Where(e => e.GetActive() && Vector2.DistanceSquared(e.Position, position) <= squaredRadius).ToList();
    }

    /// <summary>
    /// Finds all active entities of the specified type within the specified radius.
    /// </summary>
    /// <typeparam name="T">The type of entity to find.</typeparam>
    /// <param name="position">The center position to search around.</param>
    /// <param name="radius">The search radius.</param>
    /// <returns>A list of all active entities of type T within the radius.</returns>
    public List<T> FindNearby<T>(Vector2 position, float radius) where T : Entity
    {
        return FindNearby(position, radius).OfType<T>().ToList();
    }

    /// <summary>
    /// Finds all active entities within the specified rectangle.
    /// Uses spatial partitioning for fast queries when enabled.
    /// </summary>
    /// <param name="bounds">The rectangle to search within.</param>
    /// <returns>A list of all active entities within the bounds.</returns>
    public List<Entity> FindInBounds(Rectangle bounds)
    {
        var results = new List<Entity>();

        if (SpatialPartitioningEnabled && _spatialGrid != null)
        {
            var entities = _spatialGrid.Query(bounds);
            foreach (var entity in entities)
            {
                var pos = entity.Position;
                if (entity.GetActive() && bounds.Contains((int)pos.X, (int)pos.Y))
                    results.Add(entity);
            }
        }
        else
        {
            // Fallback to linear search when spatial partitioning is disabled
            foreach (var entity in _entities)
            {
                var pos = entity.Position;
                if (entity.GetActive() && bounds.Contains((int)pos.X, (int)pos.Y))
                    results.Add(entity);
            }
        }

        return results;
    }

    /// <summary>
    /// Finds the closest active entity to a position within the specified radius.
    /// Uses spatial partitioning for fast queries when enabled.
    /// </summary>
    /// <param name="position">The position to search around.</param>
    /// <param name="radius">The maximum search radius.</param>
    /// <returns>The closest entity, or null if no entity is found within the radius.</returns>
    public Entity? FindClosest(Vector2 position, float radius)
    {
        Entity? closest = null;
        var closestDistanceSquared = radius * radius;

        HashSet<Entity> candidates;

        if (SpatialPartitioningEnabled && _spatialGrid != null)
        {
            candidates = _spatialGrid.Query(position, radius);
        }
        else
        {
            // Fallback to linear search when spatial partitioning is disabled
            candidates = new HashSet<Entity>(_entities);
        }

        foreach (var entity in candidates)
        {
            if (!entity.GetActive())
                continue;

            var distanceSquared = Vector2.DistanceSquared(entity.Position, position);
            if (distanceSquared < closestDistanceSquared)
            {
                closest = entity;
                closestDistanceSquared = distanceSquared;
            }
        }

        return closest;
    }

    /// <summary>
    /// Removes a single entity from the system, cleaning up all indexes and calling OnDestroy.
    /// </summary>
    /// <param name="entity">The entity to remove.</param>
    public void RemoveEntity(Entity entity)
    {
        if (entity == null)
            return;

        UpdateTagIndexForEntity(entity, false);
        UpdateIdIndexForEntity(entity, false);
        UpdateSpatialGridForEntity(entity, false);
        entity.OnDestroy();
        _entities.Remove(entity);
    }

    /// <summary>
    /// Removes all entities from the system.
    /// </summary>
    public void ClearEntities()
    {
        Console.WriteLine($"[EntitySystem] ClearEntities: destroying {_entities.Count} entities...");
        for (int i = _entities.Count - 1; i >= 0; i--)
        {
            var entity = _entities[i];
            Console.WriteLine($"[EntitySystem]   Destroying [{i}]: {entity.GetType().Name} (Id={entity.Id})");
            try
            {
                entity.OnDestroy();
                Console.WriteLine($"[EntitySystem]   OnDestroy completed for {entity.GetType().Name}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[EntitySystem]   ERROR in OnDestroy for {entity.GetType().Name} (Id={entity.Id}): {ex.GetType().Name}: {ex.Message}");
                Console.WriteLine($"[EntitySystem]   Stack: {ex.StackTrace?.Split('\n').Take(5).Aggregate((a, b) => a + "\n" + b)}");
                throw;
            }
            _entities.RemoveAt(i);
        }
        // Clear all indexes so saved IDs can be reused on load
        _idIndex.Clear();
        _tagIndex.Clear();
        Console.WriteLine("[EntitySystem] ClearEntities: done (indexes cleared)");
    }

    /// <summary>
    /// Registers an entity template from an XML asset.
    /// </summary>
    /// <param name="name">The unique name to assign to this template.</param>
    /// <param name="assetName">The name of the XML asset containing the <c>&lt;EntityTemplate&gt;</c> definition.</param>
    public void RegisterTemplate(string name, string assetName)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Template name cannot be empty.", nameof(name));
        if (string.IsNullOrWhiteSpace(assetName)) throw new ArgumentException("Asset name cannot be empty.", nameof(assetName));

        var template = Serialization.EntityTemplateLoader.LoadFromAsset(assetName);
        _templates[name] = template;
    }

    /// <summary>
    /// Registers an already-constructed template under the given name (e.g. one parsed with
    /// <see cref="Serialization.EntityTemplateLoader.LoadFromXml"/>).
    /// </summary>
    /// <param name="name">The name to instantiate the template by.</param>
    /// <param name="template">The template to register.</param>
    public void RegisterTemplate(string name, Serialization.EntityTemplate template)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Template name cannot be empty.", nameof(name));
        _templates[name] = template ?? throw new ArgumentNullException(nameof(template));
    }

    /// <summary>
    /// Instantiates an entity from a registered template at the specified position.
    /// </summary>
    /// <param name="templateName">The name of the registered template to use.</param>
    /// <param name="position">The world position to place the instantiated entity.</param>
    /// <returns>The newly created entity.</returns>
    public Entity Instantiate(string templateName, Vector2 position)
    {
        if (!_templates.TryGetValue(templateName, out var template))
            throw new KeyNotFoundException($"Entity template '{templateName}' is not registered.");

        return Serialization.EntityTemplateLoader.Instantiate(template, this, position);
    }

    /// <summary>
    /// Saves the state of all <see cref="Serialization.ISaveableEntity"/> instances to an XML file.
    /// </summary>
    /// <param name="filePath">The path to save the game state file.</param>
    public void SaveState(string filePath)
    {
        Serialization.GameStateSerializer.SaveState(this, filePath);
    }

    /// <summary>
    /// Loads a game state from an XML file and applies it to the entity system.
    /// Entities with matching IDs are updated in place; entities without a match are created.
    /// </summary>
    /// <param name="filePath">The path to the game state file.</param>
    public void LoadState(string filePath)
    {
        Serialization.GameStateSerializer.LoadState(this, filePath);
    }

    /// <summary>
    /// Releases all resources used by the <see cref="EntitySystem"/>.
    /// Implements <see cref="IDisposable.Dispose"/>.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Releases unmanaged and optionally managed resources.
    /// </summary>
    /// <param name="disposing">True to release both managed and unmanaged resources; false to release only unmanaged.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!disposing)
            return;

        ClearEntities();
        _entities.Clear();
        _tagIndex.Clear();
        _idIndex.Clear();
        _pools.Clear();
        _templates.Clear();
        _pendingSpawns.Clear();
        _spatialGrid?.Clear();
    }

    /// <summary>
    /// Ensures the spatial grid is initialized.
    /// </summary>
    private void EnsureSpatialGrid()
    {
        if (_spatialGrid == null)
            _spatialGrid = new SpatialGrid(SpatialCellSize);
    }

    /// <summary>
    /// Updates the spatial grid when an entity is added or removed.
    /// </summary>
    /// <param name="entity">The entity to update in the spatial grid.</param>
    /// <param name="adding">True to add the entity, false to remove it.</param>
    private void UpdateSpatialGridForEntity(Entity entity, bool adding)
    {
        if (!SpatialPartitioningEnabled)
            return;

        EnsureSpatialGrid();

        if (adding)
            _spatialGrid!.Insert(entity);
        else
            _spatialGrid!.Remove(entity);
    }

    /// <summary>
    /// Called by an entity when a tag is added.
    /// </summary>
    /// <param name="entity">The entity that added the tag.</param>
    /// <param name="tag">The tag that was added.</param>
    internal void OnEntityTagAdded(Entity entity, string tag)
    {
        if (!_tagIndex.TryGetValue(tag, out var list))
        {
            list = new List<Entity>();
            _tagIndex[tag] = list;
        }
        if (!list.Contains(entity))
            list.Add(entity);
    }

    /// <summary>
    /// Called by an entity when a tag is removed.
    /// </summary>
    /// <param name="entity">The entity that removed the tag.</param>
    /// <param name="tag">The tag that was removed.</param>
    internal void OnEntityTagRemoved(Entity entity, string tag)
    {
        if (_tagIndex.TryGetValue(tag, out var list))
        {
            list.Remove(entity);
            if (list.Count == 0)
                _tagIndex.Remove(tag);
        }
    }

    /// <summary>
    /// Called by an entity when its ID changes.
    /// </summary>
    /// <param name="entity">The entity whose ID changed.</param>
    /// <param name="newId">The new ID.</param>
    internal void OnEntityIdChanged(Entity entity, string newId)
    {
        // Remove old ID from index if it existed
        var oldId = entity.Id;
        if (!string.IsNullOrEmpty(oldId) && oldId != newId)
            _idIndex.Remove(oldId);

        // Check for duplicate
        if (_idIndex.ContainsKey(newId))
            throw new InvalidOperationException($"Duplicate entity ID '{newId}'. Each entity must have a unique identifier.");

        // Add new ID to index
        _idIndex[newId] = entity;
    }

    /// <summary>
    /// Updates the tag index when an entity's tags change or when an entity is created/destroyed.
    /// </summary>
    /// <param name="entity">The entity whose tags have changed.</param>
    /// <param name="adding">True to add the entity to the index, false to remove it.</param>
    private void UpdateTagIndexForEntity(Entity entity, bool adding)
    {
        if (adding)
            AddEntityTagsToIndex(entity);
        else
            RemoveEntityTagsFromIndex(entity);
    }

    /// <summary>
    /// Adds all tags of an entity to the tag index.
    /// </summary>
    private void AddEntityTagsToIndex(Entity entity)
    {
        foreach (var tag in entity.Tags.ToList())
        {
            if (!_tagIndex.TryGetValue(tag, out var list))
            {
                list = new List<Entity>();
                _tagIndex[tag] = list;
            }
            if (!list.Contains(entity))
                list.Add(entity);
        }
    }

    /// <summary>
    /// Removes all tags of an entity from the tag index.
    /// </summary>
    private void RemoveEntityTagsFromIndex(Entity entity)
    {
        foreach (var tag in entity.Tags.ToList())
        {
            if (!_tagIndex.TryGetValue(tag, out var list))
                continue;

            list.Remove(entity);
            if (list.Count == 0)
                _tagIndex.Remove(tag);
        }
    }

    /// <summary>
    /// Updates the ID index when an entity is created or destroyed.
    /// </summary>
    /// <param name="entity">The entity whose ID should be indexed.</param>
    /// <param name="adding">True to add the entity to the index, false to remove it.</param>
    private void UpdateIdIndexForEntity(Entity entity, bool adding)
    {
        var id = entity.Id;
        if (string.IsNullOrEmpty(id))
            return;

        if (adding)
        {
            // Only add if not already in index (SetId may have already registered it)
            if (!_idIndex.ContainsKey(id))
                _idIndex[id] = entity;
        }
        else
        {
            _idIndex.Remove(id);
        }
    }

    /// <summary>
    /// Schedules creation of a new entity after a specified delay at a given position.
    /// Uses the coroutine system for timing. The delayed spawn can be cancelled before it expires.
    /// </summary>
    /// <typeparam name="T">The type of entity to create.</typeparam>
    /// <param name="position">The position to spawn the entity at.</param>
    /// <param name="delay">The time to wait before spawning the entity.</param>
    /// <param name="args">Constructor arguments for the entity.</param>
    /// <returns>A unique spawn ID that can be used to cancel the pending spawn.</returns>
    public Guid SpawnAfter<T>(Vector2 position, TimeSpan delay, params object[] args) where T : Entity
    {
        if (delay.TotalMilliseconds < 0)
            throw new ArgumentOutOfRangeException(nameof(delay), "Delay cannot be negative.");

        var spawnId = Guid.NewGuid();
        _pendingSpawns[spawnId] = () =>
        {
            var entity = CreateEntity<T>(args);
            ((Entity)entity).Position = position;
        };

        CoroutineManager.StartCoroutine(SpawnAfterRoutine(spawnId, delay));
        return spawnId;
    }

    /// <summary>
    /// Cancels a pending delayed spawn by its spawn ID.
    /// </summary>
    /// <param name="spawnId">The unique spawn ID returned from <see cref="SpawnAfter{T}"/>.</param>
    /// <returns>True if the pending spawn was cancelled; otherwise, false.</returns>
    public bool CancelSpawnAfter(Guid spawnId)
    {
        if (_pendingSpawns.Remove(spawnId))
            return true;
        return false;
    }

    /// <summary>
    /// Coroutine that waits for the specified delay and then spawns an entity.
    /// </summary>
    private IEnumerator SpawnAfterRoutine(Guid spawnId, TimeSpan delay)
    {
        yield return new WaitForSeconds((float)delay.TotalSeconds);
        if (_pendingSpawns.TryGetValue(spawnId, out var createAction))
        {
            createAction();
            _pendingSpawns.Remove(spawnId);
        }
    }

    /// <summary>
    /// Coroutine that waits for the specified delay and then respawns an entity at a given position.
    /// </summary>
    private IEnumerator RespawnRoutine(Type entityType, Vector2 position, TimeSpan delay)
    {
        yield return new WaitForSeconds((float)delay.TotalSeconds);
        // Resolve a constructor accepting no required arguments, filling optional params with their defaults.
        object? instance = CreateInstanceWithOptionalParams(entityType, Array.Empty<object>());
        if (instance == null)
            yield break;

        Entity entity = (Entity)instance;

        entity.SetGameSystem(this);
        entity.Position = position;
        _entities.Add(entity);
        UpdateTagIndexForEntity(entity, true);
        UpdateSpatialGridForEntity(entity, true);
        entity.OnStart();
    }
}
