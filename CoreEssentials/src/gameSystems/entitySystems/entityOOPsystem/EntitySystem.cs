using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem;

public class EntitySystem : GameSystem, IUpdateGameSystem, IDrawGameSystem
{
    private List<Entity> _entities = new List<Entity>();

    public EntitySystem()
    {
    }

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
                _entities.RemoveAt(i);
            }
        }
    }

    public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
    {
        for (int i = 0; i < _entities.Count; i++)
        {
            if (_entities[i].GetActive())
                _entities[i].Render(spriteBatch);
        }
    }
    public T CreateEntity<T>( params object[] args ) where T : Entity
    {
        T entity = (T)Activator.CreateInstance(typeof(T),args);
        entity.SetGameSystem(this);
        _entities.Add(entity);
        entity.OnStart();
        return entity;
    }
    public void SortEntities()
    {
        _entities.Sort(
            (x, y) => y.GetSort().CompareTo(x.GetSort())
            );
    }

    public List<Entity> GetEntities()
    {
        return _entities;
    }

    public void ClearEntities()
    {
        for (int i = _entities.Count - 1 ; i < 0; i--)
        {
            _entities[i].Destroy();
        }
    }
}
