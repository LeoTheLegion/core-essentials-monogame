using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Pooling;

/// <summary>
/// Generic entity pool for recycling pooled entities.
/// Uses a stack for O(1) acquire/release operations.
/// </summary>
/// <typeparam name="T">The type of pooled entity (must be an <see cref="Entity"/> implementing <see cref="IPooledEntity"/>).</typeparam>
public class EntityPool<T> where T : Entity, IPooledEntity, new()
{
    /// <summary>
    /// Stack of available (inactive) entities ready to be acquired.
    /// </summary>
    private Stack<T> _available;

    /// <summary>
    /// Maximum number of entities the pool can hold.
    /// When exceeded, new entities will be created outside the pool.
    /// </summary>
    private int _maxSize;

    /// <summary>
    /// Current total number of entities (both active and available).
    /// </summary>
    public int TotalCount { get; private set; }

    /// <summary>
    /// Number of entities currently available for acquisition.
    /// </summary>
    public int AvailableCount => _available.Count;

    /// <summary>
    /// Number of entities currently in use (active).
    /// </summary>
    public int ActiveCount => TotalCount - _available.Count;

    /// <summary>
    /// Initializes a new instance of the <see cref="EntityPool{T}"/> class.
    /// </summary>
    /// <param name="initialCapacity">Initial number of entities to pre-create (default: 10).</param>
    /// <param name="maxSize">Maximum pool size before creating new instances (default: 100).</param>
    public EntityPool(int initialCapacity = 10, int maxSize = 100)
    {
        _maxSize = maxSize;
        _available = new Stack<T>(initialCapacity);
        TotalCount = 0;

        // Pre-create entities for initial capacity
        for (int i = 0; i < initialCapacity; i++)
        {
            var entity = new T();
            _available.Push(entity);
            TotalCount++;
        }
    }

    /// <summary>
    /// Acquires an entity from the pool.
    /// If the pool has available entities, returns one of those.
    /// Otherwise, creates a new instance if under the max size limit.
    /// </summary>
    /// <param name="position">The position to activate the entity at.</param>
    /// <returns>An active entity ready for use.</returns>
    public T Acquire(Vector2 position = default)
    {
        T entity;

        if (_available.Count > 0)
        {
            entity = _available.Pop();
        }
        else if (TotalCount < _maxSize)
        {
            entity = new T();
            TotalCount++;
        }
        else
        {
            // Pool exhausted — reuse the last available entity
            // This is a fallback; consider increasing pool size
            entity = new T();
        }

        entity.Reset();
        entity.Activate(position);
        return entity;
    }

    /// <summary>
    /// Releases an entity back to the pool.
    /// The entity becomes inactive and available for reuse.
    /// </summary>
    /// <param name="entity">The entity to release.</param>
    public void Release(T entity)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        entity.SetActive(false);
        _available.Push(entity);
    }
}
