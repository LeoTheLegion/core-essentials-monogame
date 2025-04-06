using System;
using System.Collections.Generic;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace CoreEssentials.GameSystems.EntitySystems.EntityOOPsystem;

public static class EntityManagementSystem
{
    private static List<Entity> _entities = new List<Entity>();

    public static void Register(Entity e)
    {
        _entities.Add(e);
    }
    public static void Unregister(Entity e)
    {
        _entities.Remove(e);
    }

    public static void LoadAssets()
    {
        for (int i = 0; i < _entities.Count; i++)
        {
            _entities[i].LoadAssets();
        }
    }

    public static void Update(ref GameTime _gameTime)
    {
        SortEntities();

        for (int i = 0; i < _entities.Count; i++)
        {
            if (_entities[i].GetActive())
                _entities[i].Update(ref _gameTime);
        }
    }

    public static void Render(ref SpriteBatch _spriteBatch)
    {
        for (int i = 0; i < _entities.Count; i++)
        {
            if (_entities[i].GetActive())
                _entities[i].Render(ref _spriteBatch);
        }
    }

    public static void SortEntities()
    {
        _entities.Sort(
            (x, y) => y.GetSort().CompareTo(x.GetSort())
            );
    }

    public static List<Entity> GetEntities()
    {
        return _entities;
    }

    public static void ClearEntities()
    {
        for (int i = _entities.Count - 1 ; i < 0; i--)
        {
            _entities[i].Destroy();
        }
    }
}