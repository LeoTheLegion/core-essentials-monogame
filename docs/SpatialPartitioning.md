# Spatial Partitioning

Spatial partitioning optimizes spatial queries by dividing the game world into a grid. This dramatically improves performance for finding entities near a position.

## Overview

Spatial partitioning provides:
- O(1) average lookup for spatial queries
- Automatic position tracking
- Configurable cell size
- Seamless integration with EntitySystem

## How It Works

The world is divided into a grid of cells. Each entity is tracked in the cell(s) it occupies. Queries only check relevant cells instead of all entities.

```
World Grid (cell size = 100)
┌─────┬─────┬─────┐
│  0,0 │ 1,0 │ 2,0 │
├─────┼─────┼─────┤
│  0,1 │ 1,1 │ 2,1 │
├─────┼─────┼─────┤
│  0,2 │ 1,2 │ 2,2 │
└─────┴─────┴─────┘
```

## API Reference

### SpatialPartitioningEnabled Property

Enables or disables spatial partitioning.

```csharp
public bool SpatialPartitioningEnabled { get; set; }
```

**Default:** `true`

**Example:**
```csharp
var entitySystem = GetGameSystem<EntitySystem>();
entitySystem.SpatialPartitioningEnabled = true;
```

### SpatialCellSize Property

Sets the size of each grid cell.

```csharp
public float SpatialCellSize { get; set; }
```

**Default:** `100f`

**Example:**
```csharp
entitySystem.SpatialCellSize = 200f; // Larger cells
```

### FindNearby(Vector2 position, float radius)

Finds entities within radius using spatial partitioning.

```csharp
public List<Entity> FindNearby(Vector2 position, float radius)
```

**Performance:** O(k) where k is entities in relevant cells

**Example:**
```csharp
var nearby = entitySystem.FindNearby(player.Position, 100f);
```

### FindInBounds(Rectangle bounds)

Finds entities within rectangle using spatial partitioning.

```csharp
public List<Entity> FindInBounds(Rectangle bounds)
```

**Performance:** O(k) where k is entities in relevant cells

**Example:**
```csharp
var visible = entitySystem.FindInBounds(cameraBounds);
```

### FindClosest(Vector2 position, float radius)

Finds closest entity using spatial partitioning.

```csharp
public Entity? FindClosest(Vector2 position, float radius)
```

**Performance:** O(k) where k is entities in relevant cells

## Usage Examples

### Basic Spatial Query

```csharp
public class EnemyAI
{
    public void Update(GameTime gameTime)
    {
        // Find player within detection range
        var player = entitySystem.FindByType<PlayerEntity>().FirstOrDefault();
        if (player == null) return;
        
        // Spatial partitioning makes this fast
        var nearbyEnemies = entitySystem.FindNearby<EnemyEntity>(
            player.Position, 
            150f
        );
        
        foreach (var enemy in nearbyEnemies)
        {
            enemy.Aggro(player);
        }
    }
}
```

### Camera Culling

```csharp
public class CameraSystem
{
    public void Update(GameTime gameTime)
    {
        var cameraBounds = new Rectangle(
            (int)camera.Position.X,
            (int)camera.Position.Y,
            camera.ViewportWidth,
            camera.ViewportHeight
        );
        
        // Only process visible entities
        var visibleEntities = entitySystem.FindInBounds(cameraBounds);
        
        foreach (var entity in visibleEntities)
        {
            entity.Update(gameTime);
            entity.Render(spriteBatch);
        }
    }
}
```

### Collision Detection

```csharp
public class CollisionSystem
{
    public void CheckCollisions()
    {
        var projectiles = entitySystem.FindByType<ProjectileEntity>();
        
        foreach (var projectile in projectiles)
        {
            // Only check nearby enemies
            var nearbyEnemies = entitySystem.FindNearby<EnemyEntity>(
                projectile.Position,
                projectile.CollisionRadius
            );
            
            foreach (var enemy in nearbyEnemies)
            {
                if (CheckCollision(projectile, enemy))
                {
                    HandleCollision(projectile, enemy);
                }
            }
        }
    }
}
```

## Performance Comparison

### Without Spatial Partitioning

```csharp
// Linear search through all entities
public List<Entity> FindNearby(Vector2 position, float radius)
{
    var results = new List<Entity>();
    foreach (var entity in _entities) // O(n)
    {
        if (Vector2.Distance(entity.Position, position) <= radius)
            results.Add(entity);
    }
    return results;
}
```

**Complexity:** O(n) where n is total entities

### With Spatial Partitioning

```csharp
// Only check relevant cells
public List<Entity> FindNearby(Vector2 position, float radius)
{
    var candidates = _spatialGrid.Query(position, radius); // O(k)
    var results = new List<Entity>();
    foreach (var entity in candidates) // k << n
    {
        if (Vector2.Distance(entity.Position, position) <= radius)
            results.Add(entity);
    }
    return results;
}
```

**Complexity:** O(k) where k is entities in relevant cells

## Cell Size Tuning

### Small Cells (50-100)

**Pros:**
- More precise queries
- Fewer false positives
- Better for dense scenes

**Cons:**
- More cells to manage
- Higher memory overhead
- More updates when entities move

**Best for:** Dense scenes with many small entities

### Large Cells (200-500)

**Pros:**
- Fewer cells to manage
- Lower memory overhead
- Fewer updates when entities move

**Cons:**
- More false positives
- Less precise queries

**Best for:** Sparse scenes with large entities

### Optimal Cell Size

Choose cell size based on:
1. Average entity size
2. Typical query radius
3. Scene density
4. Performance requirements

**Rule of thumb:** Cell size ≈ 2-3x average query radius

```csharp
// For queries with 100 unit radius
entitySystem.SpatialCellSize = 250f; // 2.5x radius
```

## Best Practices

### When to Use Spatial Partitioning

**Use for:**
- Large scenes with many entities
- Frequent spatial queries
- Collision detection
- Proximity-based AI
- Camera culling

**Don't use for:**
- Small scenes (< 100 entities)
- Infrequent queries
- Type-based queries only
- Static scenes

### Optimization Tips

**Do:**
- Enable spatial partitioning by default
- Tune cell size for your game
- Use type-specific queries when possible
- Cache query results when appropriate

**Don't:**
- Disable spatial partitioning without profiling
- Use very small cells (< 25)
- Use very large cells (> 1000)
- Query with huge radii

### Monitoring Performance

```csharp
public void LogSpatialStats()
{
    var grid = entitySystem.GetSpatialGrid();
    Console.WriteLine($"Entities tracked: {grid.Count}");
    Console.WriteLine($"Cell size: {entitySystem.SpatialCellSize}");
    Console.WriteLine($"Partitioning enabled: {entitySystem.SpatialPartitioningEnabled}");
}
```

## Advanced Usage

### Custom Spatial Grid

```csharp
// Access spatial grid directly
var grid = entitySystem.GetSpatialGrid();

// Manual queries
var entities = grid.Query(new Rectangle(0, 0, 100, 100));
```

### Dynamic Cell Size

```csharp
public void AdjustCellSizeForZoom(float zoomLevel)
{
    // Smaller cells when zoomed in
    // Larger cells when zoomed out
    entitySystem.SpatialCellSize = 100f / zoomLevel;
}
```

### Hybrid Queries

```csharp
// Combine spatial and type queries
var nearbyEnemies = entitySystem.FindNearby<EnemyEntity>(position, radius);

// Further filter by tag
var aggressiveEnemies = nearbyEnemies
    .Where(e => e.HasTag("Aggressive"))
    .ToList();
```

## Troubleshooting

### Queries Returning Too Many Results

**Problem:** Cell size too large

**Solution:** Reduce cell size
```csharp
entitySystem.SpatialCellSize = 100f; // Smaller cells
```

### Queries Missing Entities

**Problem:** Entities not updating position in grid

**Solution:** Ensure entities move via Position property
```csharp
// Good - updates spatial grid automatically
entity.Position = newPosition;

// Bad - bypasses spatial grid
entity._position = newPosition;
```

### Performance Degradation

**Problem:** Too many entities per cell

**Solution:** Increase cell size or reduce entity count
```csharp
entitySystem.SpatialCellSize = 200f; // Larger cells
```

## See Also

- [Entity Query API](./EntityQueryAPI.md) — Query methods
- [Entity System](./EntitySystem.md) — Core entity management
- [Entity Tags](./EntityTags.md) — Tag-based filtering
