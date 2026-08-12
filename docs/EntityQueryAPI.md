# Entity Query API

The Entity Query API provides powerful methods for finding and filtering entities in your game. Use these methods for gameplay logic, collision detection, and spatial queries.

## Overview

The query API offers:
- Type-based queries for finding entities by class
- Spatial queries for finding entities near a position
- Performance-optimized queries using spatial partitioning
- Flexible filtering for complex gameplay scenarios

## API Reference

### FindByType<T>()

Finds all active entities of a specific type.

```csharp
public List<T> FindByType<T>() where T : Entity
```

**Returns:** List of active entities of type T

**Example:**
```csharp
// Find all enemies
var enemies = entitySystem.FindByType<EnemyEntity>();

// Find all projectiles
var projectiles = entitySystem.FindByType<ProjectileEntity>();

// Process each enemy
foreach (var enemy in enemies)
{
    enemy.UpdateAI();
}
```

**Performance:** O(n) where n is total entities. Only returns active entities.

### FindNearby(Vector2 position, float radius)

Finds all active entities within a radius of a position.

```csharp
public List<Entity> FindNearby(Vector2 position, float radius)
```

**Parameters:**
- `position` — Center point for search
- `radius` — Search radius in world units

**Returns:** List of entities within radius

**Example:**
```csharp
// Find entities near player
var nearby = entitySystem.FindNearby(player.Position, 100f);

foreach (var entity in nearby)
{
    if (entity.HasTag("Enemy"))
    {
        enemy.Aggro(player);
    }
}
```

### FindNearby<T>(Vector2 position, float radius)

Finds all active entities of a specific type within a radius.

```csharp
public List<T> FindNearby<T>(Vector2 position, float radius) where T : Entity
```

**Parameters:**
- `position` — Center point for search
- `radius` — Search radius in world units

**Returns:** List of entities of type T within radius

**Example:**
```csharp
// Find enemies near player
var nearbyEnemies = entitySystem.FindNearby<EnemyEntity>(player.Position, 150f);

foreach (var enemy in nearbyEnemies)
{
    enemy.SetAlert(true);
}
```

### FindInBounds(Rectangle bounds)

Finds all active entities within a rectangular area.

```csharp
public List<Entity> FindInBounds(Rectangle bounds)
```

**Parameters:**
- `bounds` — Rectangle to search within

**Returns:** List of entities within bounds

**Example:**
```csharp
// Find entities in camera view
var cameraBounds = new Rectangle(
    (int)camera.Position.X,
    (int)camera.Position.Y,
    camera.ViewportWidth,
    camera.ViewportHeight
);

var visibleEntities = entitySystem.FindInBounds(cameraBounds);
```

### FindClosest(Vector2 position, float radius)

Finds the closest active entity within a radius.

```csharp
public Entity? FindClosest(Vector2 position, float radius)
```

**Parameters:**
- `position` — Center point for search
- `radius` — Maximum search radius

**Returns:** Closest entity, or null if none found

**Example:**
```csharp
// Find closest enemy to player
var closestEnemy = entitySystem.FindClosest(player.Position, 200f);

if (closestEnemy != null && closestEnemy.HasTag("Enemy"))
{
    player.TargetEnemy(closestEnemy);
}
```

## Usage Examples

### Spatial Queries for AI

```csharp
public class EnemyAI
{
    public void Update(GameTime gameTime)
    {
        // Find player within detection range
        var player = entitySystem.FindByType<PlayerEntity>().FirstOrDefault();
        if (player == null) return;

        var distance = Vector2.Distance(Position, player.Position);
        
        if (distance < 150f)
        {
            // Find nearby allies for group behavior
            var allies = entitySystem.FindNearby<EnemyEntity>(Position, 100f);
            allies.Remove(this); // Remove self
            
            if (allies.Count > 0)
            {
                // Coordinate with allies
                FormGroup(allies);
            }
        }
    }
}
```

### Collision Detection

```csharp
public void CheckCollisions()
{
    var projectiles = entitySystem.FindByType<ProjectileEntity>();
    
    foreach (var projectile in projectiles)
    {
        // Find enemies near projectile
        var nearbyEnemies = entitySystem.FindNearby<EnemyEntity>(
            projectile.Position, 
            projectile.CollisionRadius
        );
        
        foreach (var enemy in nearbyEnemies)
        {
            if (projectile.Owner != enemy)
            {
                enemy.TakeDamage(projectile.Damage);
                projectile.Destroy();
                break;
            }
        }
    }
}
```

### Area of Effect

```csharp
public void Explode(Vector2 position, float radius, int damage)
{
    // Find all entities in explosion radius
    var affected = entitySystem.FindNearby(position, radius);
    
    foreach (var entity in affected)
    {
        if (entity.HasTag("Enemy") || entity.HasTag("Player"))
        {
            var distance = Vector2.Distance(position, entity.Position);
            var falloff = 1f - (distance / radius);
            var actualDamage = (int)(damage * falloff);
            
            entity.TakeDamage(actualDamage);
        }
    }
}
```

### Proximity Triggers

```csharp
public class ProximityTrigger
{
    private float _triggerRadius = 50f;
    private bool _triggered = false;
    
    public void Update(GameTime gameTime)
    {
        if (_triggered) return;
        
        // Check for player entering trigger zone
        var player = entitySystem.FindByType<PlayerEntity>().FirstOrDefault();
        if (player != null)
        {
            var distance = Vector2.Distance(Position, player.Position);
            if (distance < _triggerRadius)
            {
                OnTriggerEnter(player);
                _triggered = true;
            }
        }
    }
}
```

## Performance Considerations

### Spatial Partitioning

When `SpatialPartitioningEnabled` is true (default), spatial queries use a grid-based spatial partition for O(1) average lookup:

```csharp
// Enable spatial partitioning (default)
entitySystem.SpatialPartitioningEnabled = true;

// Configure cell size (default: 100)
entitySystem.SpatialCellSize = 100f;
```

**Benefits:**
- Faster queries in large scenes
- Automatic position tracking
- Reduced linear search overhead

**Trade-offs:**
- Slight memory overhead for grid
- Position updates cost more

### Query Optimization

**Do:**
- Cache query results when possible
- Use type-specific queries (`FindNearby<T>`) for better performance
- Query with reasonable radii
- Use spatial partitioning for large scenes

**Don't:**
- Query every frame for static data
- Use very large radii
- Chain multiple queries unnecessarily

### Performance Comparison

| Query Type | Without Spatial Partitioning | With Spatial Partitioning |
|------------|------------------------------|---------------------------|
| FindByType | O(n) | O(n) |
| FindNearby (small radius) | O(n) | O(1) avg |
| FindNearby (large radius) | O(n) | O(n) |
| FindInBounds | O(n) | O(k) where k << n |

## Advanced Usage

### Combining Queries

```csharp
// Find active enemies near player
var player = entitySystem.FindByType<PlayerEntity>().FirstOrDefault();
if (player != null)
{
    var nearbyEnemies = entitySystem.FindNearby<EnemyEntity>(player.Position, 200f);
    
    // Filter further
    var aggressiveEnemies = nearbyEnemies
        .Where(e => e.HasTag("Aggressive"))
        .Where(e => e.GetActive())
        .ToList();
}
```

### Custom Filtering

```csharp
public List<Entity> FindEntitiesWithPredicate(Func<Entity, bool> predicate)
{
    var allEntities = entitySystem.GetEntities();
    return allEntities.Where(predicate).ToList();
}

// Usage
var filtered = FindEntitiesWithPredicate(e => 
    e.HasTag("Enemy") && 
    e.GetActive() && 
    Vector2.Distance(e.Position, player.Position) < 100f
);
```

## See Also

- [Entity Tags](./EntityTags.md) — Tag-based entity filtering
- [Spatial Partitioning](./SpatialPartitioning.md) — Grid-based optimization
- [Entity System](./EntitySystem.md) — Core entity management
