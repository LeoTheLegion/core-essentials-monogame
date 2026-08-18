# Entity Pooling

Entity pooling reduces garbage collection pressure by reusing entities instead of creating and destroying them. Essential for high-frequency spawn scenarios like bullets, particles, and effects.

## Overview

Pooling provides:
- Reduced garbage collection pauses
- Faster entity creation (reuse vs allocation)
- Predictable memory usage
- Better performance for spawn-heavy games

## API Reference

### IPooledEntity Interface

Entities that support pooling must implement `IPooledEntity`:

```csharp
public interface IPooledEntity
{
    void Reset();
    void Activate(Vector2 position);
}
```

#### Reset()

Resets the entity to its initial state for reuse.

```csharp
public void Reset()
```

Called when entity is acquired from pool. Override to reset custom state.

**Example:**
```csharp
public class BulletEntity : Entity, IPooledEntity
{
    private float _speed = 500f;
    private int _damage = 10;
    
    public void Reset()
    {
        // Reset custom state (Entity has no virtual Reset to call)
        Velocity = Vector2.Zero;
        Damage = _damage;
        Lifetime = 3f;
    }
    
    public void Activate(Vector2 position)
    {
        Position = position;
        SetActive(true);
    }
}
```

#### Activate(Vector2 position)

Activates the entity at the specified position.

```csharp
public void Activate(Vector2 position)
```

Called by pool when entity is acquired.

### EntitySystem Pooling Methods

#### CreatePooled<T>(Vector2 position)

Creates a pooled entity of type T.

```csharp
public T CreatePooled<T>(Vector2 position = default, params object[] args) 
    where T : Entity, IPooledEntity, new()
```

**Parameters:**
- `position` — Position to activate entity at
- `args` — Constructor arguments

**Returns:** Active pooled entity

**Example:**
```csharp
// Create a bullet from pool
var bullet = entitySystem.CreatePooled<BulletEntity>(player.Position);
bullet.Velocity = direction * bullet.Speed;
```

#### ReleasePooled<T>(T entity)

Returns a pooled entity to the pool.

```csharp
public void ReleasePooled<T>(T entity) 
    where T : Entity, IPooledEntity, new()
```

**Parameters:**
- `entity` — Entity to release

**Example:**
```csharp
// Instead of destroying, release to pool
bullet.OnLifetimeExpired += () => 
{
    entitySystem.ReleasePooled(bullet);
};
```

### EntityPool<T> Class

Direct pool management for advanced scenarios.

```csharp
public class EntityPool<T> where T : Entity, IPooledEntity, new()
{
    public int TotalCount { get; }
    public int AvailableCount { get; }
    public int ActiveCount { get; }
    
    public EntityPool(int initialCapacity = 10, int maxSize = 100)
    public T Acquire(Vector2 position = default)
    public void Release(T entity)
}
```

## Usage Examples

### Bullet Pooling

```csharp
public class Weapon
{
    private EntitySystem _entitySystem;
    
    public void Fire(Vector2 position, Vector2 direction)
    {
        // Acquire bullet from pool
        var bullet = _entitySystem.CreatePooled<BulletEntity>(position);
        bullet.Velocity = direction * 500f;
        bullet.Direction = direction;
        
        // Auto-release after lifetime
        CoroutineManager.StartCoroutine(ReleaseAfterDelay(bullet, 3f));
    }
    
    private IEnumerator ReleaseAfterDelay(BulletEntity bullet, float delay)
    {
        yield return new WaitForSeconds(delay);
        _entitySystem.ReleasePooled(bullet);
    }
}

public class BulletEntity : Entity, IPooledEntity
{
    public Vector2 Velocity { get; set; }
    public int Damage { get; set; } = 10;
    
    public void Reset()
    {
        // Reset custom state (Entity has no virtual Reset to call)
        Velocity = Vector2.Zero;
        Damage = 10;
        SetActive(false);
    }
    
    public void Activate(Vector2 position)
    {
        Position = position;
        SetActive(true);
    }
    
    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);
        Position += Velocity * (float)gameTime.ElapsedGameTime.TotalSeconds;
    }
}
```

### Particle Effects

```csharp
public class ParticleSystem
{
    private EntitySystem _entitySystem;
    
    public void EmitBurst(Vector2 position, int count)
    {
        for (int i = 0; i < count; i++)
        {
            var particle = _entitySystem.CreatePooled<ParticleEntity>(position);
            particle.Velocity = RandomDirection() * Random.Range(100f, 300f);
            particle.Lifetime = Random.Range(0.5f, 2f);
            particle.Color = Color.White;
        }
    }
}

public class ParticleEntity : Entity, IPooledEntity
{
    public Vector2 Velocity { get; set; }
    public float Lifetime { get; set; }
    private float _maxLifetime;
    
    public void Reset()
    {
        // Reset custom state (Entity has no virtual Reset to call)
        Velocity = Vector2.Zero;
        Lifetime = 0f;
        _maxLifetime = 0f;
        SetActive(false);
    }
    
    public void Activate(Vector2 position)
    {
        Position = position;
        Lifetime = _maxLifetime;
        SetActive(true);
    }
    
    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);
        Position += Velocity * (float)gameTime.ElapsedGameTime.TotalSeconds;
        Lifetime -= (float)gameTime.ElapsedGameTime.TotalSeconds;
        
        if (Lifetime <= 0f)
        {
            _entitySystem.ReleasePooled(this);
        }
    }
}
```

### Object Pool Configuration

```csharp
public class GameScene
{
    protected override void OnStart()
    {
        var entitySystem = GetGameSystem<EntitySystem>();
        
        // Pre-create pools for common entities
        var bulletPool = entitySystem.GetOrCreatePool<BulletEntity>(
            initialCapacity: 50,
            maxSize: 200
        );
        
        var particlePool = entitySystem.GetOrCreatePool<ParticleEntity>(
            initialCapacity: 100,
            maxSize: 500
        );
        
        // Pools are now ready for fast acquisition
    }
}
```

## Performance Benefits

### Before Pooling

```csharp
// Creates new entity each time
public void Fire()
{
    var bullet = new BulletEntity();
    entitySystem.AddEntity(bullet);
    // ... later ...
    bullet.Destroy(); // Garbage collection
}
```

**Problems:**
- Frequent allocations
- Garbage collection pauses
- Slower creation

### After Pooling

```csharp
// Reuses existing entities
public void Fire()
{
    var bullet = entitySystem.CreatePooled<BulletEntity>(position);
    // ... later ...
    entitySystem.ReleasePooled(bullet); // Reuse
}
```

**Benefits:**
- No allocations after warm-up
- No garbage collection
- Faster creation

## Best Practices

### When to Use Pooling

**Use pooling for:**
- Bullets and projectiles
- Particle effects
- Enemy spawns (if frequent)
- UI elements that appear/disappear
- Temporary effects

**Don't pool:**
- Unique entities (bosses, player)
- Rarely created entities
- Entities with complex initialization

### Pool Sizing

```csharp
// Conservative sizing
var pool = entitySystem.GetOrCreatePool<BulletEntity>(
    initialCapacity: 20,  // Pre-create 20
    maxSize: 100          // Max 100 total
);

// Aggressive sizing for intense action
var pool = entitySystem.GetOrCreatePool<ParticleEntity>(
    initialCapacity: 200,
    maxSize: 1000
);
```

**Guidelines:**
- Initial capacity: Expected concurrent entities
- Max size: Peak usage + buffer
- Monitor `ActiveCount` to tune sizes

### Reset Implementation

Always reset all state in `Reset()`:

```csharp
public void Reset()
{
    // Reset all mutable state (Entity has no virtual Reset to call)
    
    // Reset position
    Position = Vector2.Zero;
    
    // Reset velocity
    Velocity = Vector2.Zero;
    
    // Reset health
    Health = MaxHealth;
    
    // Reset timers
    Lifetime = 0f;
    
    // Reset flags
    IsActive = false;
    HasCollided = false;
    
    // Clear references
    Target = null;
}
```

### Avoid Common Pitfalls

**Don't:**
- Leave references to old data
- Assume entity is at default position
- Modify pool directly

**Do:**
- Reset all mutable state
- Test pool exhaustion
- Monitor pool statistics
- Profile performance

## Pool Statistics

Monitor pool usage:

```csharp
var pool = entitySystem.GetOrCreatePool<BulletEntity>();
Console.WriteLine($"Total: {pool.TotalCount}");
Console.WriteLine($"Available: {pool.AvailableCount}");
Console.WriteLine($"Active: {pool.ActiveCount}");
```

## See Also

- [Entity System](./EntitySystem.md) — Core entity management
- [Entity Query API](./EntityQueryAPI.md) — Finding entities
- [Entity Lifecycle](./EntityLifecycle.md) — Delayed destruction and respawn
