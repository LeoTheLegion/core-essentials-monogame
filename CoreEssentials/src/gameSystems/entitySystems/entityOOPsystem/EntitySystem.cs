using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using CoreEssentials.Assets;
using CoreEssentials.Camera;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Spatial;

namespace CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem;

/// <summary>
/// Manages game entities in an object-oriented architecture.
/// Handles the creation, updating, rendering, and destruction of entities.
/// </summary>
public class EntitySystem : GameSystem, IUpdateGameSystem, IDrawGameSystem, IDisposable
{
    /// <summary>
    /// The list of all entities managed by this system.
    /// </summary>
    private List<Entity> _entities = new List<Entity>();

    /// <summary>
    /// Dictionary for O(1) tag-based entity lookups.
    /// Maps tag names to lists of entities with that tag.
    /// </summary>
    private Dictionary<string, List<Entity>> _tagIndex = new Dictionary<string, List<Entity>>(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Dictionary of entity pools, keyed by type name.
    /// </summary>
    private Dictionary<Type, object> _pools = new Dictionary<Type, object>();

    /// <summary>
    /// The spatial grid for fast spatial queries.
    /// </summary>
    private SpatialGrid? _spatialGrid;

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

        for (int i = 0; i < _entities.Count; i++)
        {
            if (_entities[i].GetActive())
                _entities[i].Update(gameTime);
        }

        // Auto-update spatial grid for entities that have moved
        if (SpatialPartitioningEnabled && _spatialGrid != null)
        {
            for (int i = 0; i < _entities.Count; i++)
            {
                if (_entities[i].GetActive())
                    _spatialGrid.UpdatePosition(_entities[i]);
            }
        }

        for (int i = _entities.Count - 1; i >= 0; i--)
        {
            if (_entities[i].Destroyed)
            {
                UpdateTagIndexForEntity(_entities[i], false);
                UpdateSpatialGridForEntity(_entities[i], false);
                _entities[i].OnDestroy();
                _entities.RemoveAt(i);
            }
        }
    }

    /// <summary>
    /// Renders all active entities using texture-based batching.
    /// Entities are grouped by their active texture asset to minimize SpriteBatch begin/end calls.
    /// Within each texture group, entities maintain their sort order.
    /// </summary>
    /// <param name="gameTime">Provides a snapshot of timing values.</param>
    /// <param name="spriteBatch">The SpriteBatch used for drawing entities.</param>
    public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
    {
        var camera = Camera.Camera.MainCamera;
        var hasCamera = camera != null;

        // Group entities by texture asset while preserving sort order
        var textureGroups = new Dictionary<Texture2DAsset, List<Entity>>();
        var noTextureEntities = new List<Entity>(); // Group for entities without texture

        for (int i = 0; i < _entities.Count; i++)
        {
            var entity = _entities[i];
            if (entity.GetActive())
            {
                var texture = entity.BatchTexture;
                if (texture == null)
                {
                    noTextureEntities.Add(entity);
                }
                else
                {
                    if (!textureGroups.ContainsKey(texture))
                        textureGroups[texture] = new List<Entity>();
                    textureGroups[texture].Add(entity);
                }
            }
        }

        // Render entities without texture first
        if (noTextureEntities.Count > 0)
        {
            spriteBatch.Begin(
                SpriteSortMode.Deferred,
                BlendState.AlphaBlend,
                SamplerState.PointClamp,
                DepthStencilState.None,
                RasterizerState.CullNone,
                null,
                hasCamera ? camera!.ViewMatrix : null
            );

            foreach (var entity in noTextureEntities)
            {
                entity.Render(spriteBatch);
            }

            spriteBatch.End();
        }

        // Render each texture group
        foreach (var textureGroup in textureGroups)
        {
            spriteBatch.Begin(
                SpriteSortMode.Deferred,
                BlendState.AlphaBlend,
                SamplerState.PointClamp,
                DepthStencilState.None,
                RasterizerState.CullNone,
                null,
                hasCamera ? camera!.ViewMatrix : null
            );

            foreach (var entity in textureGroup.Value)
            {
                entity.Render(spriteBatch);
            }

            spriteBatch.End();
        }

        // Reset texture changed flags for next frame
        for (int i = 0; i < _entities.Count; i++)
        {
            _entities[i].BatchTextureDirty = false;
        }
    }

    /// <summary>
    /// Creates and initializes a new entity of the specified type.
    /// </summary>
    /// <typeparam name="T">The type of entity to create.</typeparam>
    /// <param name="args">Constructor arguments for the entity.</param>
    /// <returns>The newly created entity.</returns>
    public T CreateEntity<T>(params object[] args) where T : Entity
    {
        T entity = (T)(Activator.CreateInstance(typeof(T), args) ?? throw new InvalidOperationException($"Failed to create entity of type {typeof(T)}."));
        entity.SetGameSystem(this);
        _entities.Add(entity);
        UpdateTagIndexForEntity(entity, true);
        UpdateSpatialGridForEntity(entity, true);
        entity.OnStart();
        return entity;
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
        _entities.Add(entity);
        UpdateTagIndexForEntity(entity, true);
        UpdateSpatialGridForEntity(entity, true);
        entity.OnStart();
        return entity;
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
        var results = new List<Entity>();
        foreach (var entity in _entities)
        {
            if (entity.GetActive() && Vector2.DistanceSquared(entity.Position, position) <= squaredRadius)
                results.Add(entity);
        }
        return results;
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
    /// Finds all active entities of the specified type within the specified radius of a position.
    /// </summary>
    /// <typeparam name="T">The type of entity to find.</typeparam>
    /// <param name="position">The center position to search around.</param>
    /// <param name="radius">The search radius.</param>
    /// <returns>A list of all active entities of type T within the radius.</returns>
    public List<T> FindNearby<T>(Vector2 position, float radius) where T : Entity
    {
        var squaredRadius = radius * radius;
        var results = new List<T>();
        foreach (var entity in _entities)
        {
            if (entity is T typed && entity.GetActive() && Vector2.DistanceSquared(entity.Position, position) <= squaredRadius)
                results.Add(typed);
        }
        return results;
    }

    /// <summary>
    /// Removes all entities from the system.
    /// </summary>
    public void ClearEntities()
    {
        for (int i = _entities.Count - 1; i >= 0; i--)
        {
            _entities[i].OnDestroy();
            _entities.RemoveAt(i);
        }
    }

    /// <summary>
    /// Releases all resources used by the <see cref="EntitySystem"/>.
    /// Implements <see cref="IDisposable.Dispose"/>.
    /// </summary>
    public void Dispose()
    {
        ClearEntities();
        _entities = null!;
        _tagIndex.Clear();
        _pools.Clear();
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
    /// Updates the tag index when an entity's tags change or when an entity is created/destroyed.
    /// </summary>
    /// <param name="entity">The entity whose tags have changed.</param>
    /// <param name="adding">True to add the entity to the index, false to remove it.</param>
    private void UpdateTagIndexForEntity(Entity entity, bool adding)
    {
        foreach (var tag in entity.Tags.ToList())
        {
            if (adding)
            {
                if (!_tagIndex.TryGetValue(tag, out var list))
                {
                    list = new List<Entity>();
                    _tagIndex[tag] = list;
                }
                if (!list.Contains(entity))
                    list.Add(entity);
            }
            else
            {
                if (_tagIndex.TryGetValue(tag, out var list))
                {
                    list.Remove(entity);
                    if (list.Count == 0)
                        _tagIndex.Remove(tag);
                }
            }
        }
    }
}
