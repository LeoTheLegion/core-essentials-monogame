using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using CoreEssentials.Camera;

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

        for (int i = _entities.Count - 1; i >= 0; i--)
        {
            if (_entities[i].Destroyed)
            {
                UpdateTagIndexForEntity(_entities[i], false);
                _entities[i].OnDestroy();
                _entities.RemoveAt(i);
            }
        }
    }

    /// <summary>
    /// Renders all active entities.
    /// </summary>
    /// <param name="gameTime">Provides a snapshot of timing values.</param>
    /// <param name="spriteBatch">The SpriteBatch used for drawing entities.</param>
    public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
    {
        var camera = Camera.Camera.MainCamera;
        if (camera == null)
        {
            spriteBatch.Begin();
        }
        else
        {
            spriteBatch.Begin(
                SpriteSortMode.Deferred,
                BlendState.AlphaBlend,
                SamplerState.PointClamp,
                DepthStencilState.None,
                RasterizerState.CullNone,
                null,
                camera.ViewMatrix
            );
        }

        for (int i = 0; i < _entities.Count; i++)
        {
            if (_entities[i].GetActive())
                _entities[i].Render(spriteBatch);
        }
        spriteBatch.End();
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
        entity.OnStart();
        return entity;
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
