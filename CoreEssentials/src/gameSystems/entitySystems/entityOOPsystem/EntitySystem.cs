using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

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
        var camera = Cameras.Camera.MainCamera;
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
        T entity = (T)Activator.CreateInstance(typeof(T), args);
        entity.SetGameSystem(this);
        _entities.Add(entity);
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

    public void Dispose()
    {
        ClearEntities();
        _entities = null;
    }
}
