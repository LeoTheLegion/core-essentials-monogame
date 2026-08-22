using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Spatial;

/// <summary>
/// Grid-based spatial partitioning for efficient spatial queries.
/// Divides the world into cells of configurable size and tracks entities in each cell.
/// </summary>
public class SpatialGrid
{
    /// <summary>
    /// The size of each cell in the grid (width and height).
    /// </summary>
    private readonly float _cellSize;

    /// <summary>
    /// Maps grid coordinates to the entities occupying that cell.
    /// </summary>
    private readonly Dictionary<Vector2, HashSet<Entity>> _grid;

    /// <summary>
    /// Tracks which cells each entity occupies for efficient removal.
    /// </summary>
    private readonly Dictionary<Entity, HashSet<Vector2>> _entityCells;

    /// <summary>
    /// Initializes a new instance of the SpatialGrid class.
    /// </summary>
    /// <param name="cellSize">The size of each cell in the grid. Larger cells = fewer cells to search, smaller cells = more precise partitioning.</param>
    public SpatialGrid(float cellSize)
    {
        if (cellSize <= 0)
            throw new ArgumentException("Cell size must be positive.", nameof(cellSize));

        _cellSize = cellSize;
        _grid = new Dictionary<Vector2, HashSet<Entity>>();
        _entityCells = new Dictionary<Entity, HashSet<Vector2>>();
    }

    /// <summary>
    /// Gets the cell size used by this grid.
    /// </summary>
    public float CellSize => _cellSize;

    /// <summary>
    /// Gets the total number of entities tracked in the grid.
    /// </summary>
    public int Count => _entityCells.Count;

    /// <summary>
    /// Adds an entity to the grid at its current position.
    /// Entities are placed in all cells that overlap with their bounding box.
    /// </summary>
    /// <param name="entity">The entity to add.</param>
    public void Insert(Entity entity)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        if (_entityCells.ContainsKey(entity))
            Remove(entity); // Re-insert if already present

        var cells = GetOccupiedCells(entity.Position);
        _entityCells[entity] = cells;

        foreach (var cell in cells)
        {
            if (!_grid.ContainsKey(cell))
                _grid[cell] = new HashSet<Entity>();

            _grid[cell].Add(entity);
        }
    }

    /// <summary>
    /// Removes an entity from the grid.
    /// </summary>
    /// <param name="entity">The entity to remove.</param>
    public void Remove(Entity entity)
    {
        if (!_entityCells.TryGetValue(entity, out var cells))
            return;

        foreach (var cell in cells)
        {
            if (_grid.ContainsKey(cell))
                _grid[cell].Remove(entity);

            // Clean up empty cells
            if (_grid[cell].Count == 0)
                _grid.Remove(cell);
        }

        _entityCells.Remove(entity);
    }

    /// <summary>
    /// Updates an entity's position in the grid.
    /// Call this when an entity moves to ensure it's in the correct cells.
    /// </summary>
    /// <param name="entity">The entity to update.</param>
    public void UpdatePosition(Entity entity)
    {
        Remove(entity);
        Insert(entity);
    }

    /// <summary>
    /// Queries for all entities within a rectangular region.
    /// </summary>
    /// <param name="bounds">The rectangle to query.</param>
    /// <returns>A collection of entities within the specified bounds.</returns>
    public HashSet<Entity> Query(Rectangle bounds)
    {
        var results = new HashSet<Entity>();

        var minCell = WorldToGrid(new Vector2(bounds.X, bounds.Y));
        var maxCell = WorldToGrid(new Vector2(bounds.X + bounds.Width, bounds.Y + bounds.Height));

        for (var x = minCell.X; x <= maxCell.X; x++)
        {
            for (var y = minCell.Y; y <= maxCell.Y; y++)
            {
                var cell = new Vector2(x, y);
                if (_grid.TryGetValue(cell, out var entities))
                {
                    foreach (var entity in entities)
                        results.Add(entity);
                }
            }
        }

        return results;
    }

    /// <summary>
    /// Queries for all entities within a circular region.
    /// </summary>
    /// <param name="center">The center of the circle.</param>
    /// <param name="radius">The radius of the circle.</param>
    /// <returns>A collection of entities within the specified radius.</returns>
    public HashSet<Entity> Query(Vector2 center, float radius)
    {
        var results = new HashSet<Entity>();

        // Calculate the bounding box of the circle
        var bounds = new Rectangle(
            (int)(center.X - radius),
            (int)(center.Y - radius),
            (int)(radius * 2),
            (int)(radius * 2)
        );

        var candidates = Query(bounds);

        // Filter to only include entities actually within the radius
        foreach (var entity in candidates)
        {
            var distance = Vector2.Distance(center, entity.Position);
            if (distance <= radius)
                results.Add(entity);
        }

        return results;
    }

    /// <summary>
    /// Clears all entities from the grid.
    /// </summary>
    public void Clear()
    {
        _grid.Clear();
        _entityCells.Clear();
    }

    /// <summary>
    /// Calculates which cells an entity at a given position occupies.
    /// Entities are placed in a single cell based on their position.
    /// </summary>
    private HashSet<Vector2> GetOccupiedCells(Vector2 position)
    {
        var cells = new HashSet<Vector2>();
        var cell = WorldToGrid(position);
        cells.Add(cell);
        return cells;
    }

    /// <summary>
    /// Converts world coordinates to grid coordinates.
    /// </summary>
    private Vector2 WorldToGrid(Vector2 position)
    {
        return new Vector2(
            (int)(position.X / _cellSize),
            (int)(position.Y / _cellSize)
        );
    }
}
