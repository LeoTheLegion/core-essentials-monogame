# Entity Lifecycle

Manage entity lifecycles with delayed destruction, spawning, and respawning. Perfect for temporary power-ups, delayed spawns, and respawn mechanics.

## Overview

The lifecycle system provides:
- Delayed destruction with `DestroyAfter()`
- Delayed spawning with `SpawnAfter()`
- Respawn mechanics with `RespawnAt()`
- Automatic cleanup and cancellation

## API Reference

### DestroyAfter(TimeSpan delay)

Schedules entity for destruction after a delay.

```csharp
public void DestroyAfter(TimeSpan delay)
```

**Parameters:**
- `delay` — Time to wait before destruction

**Example:**
```csharp
// Destroy explosion after 2 seconds
explosion.DestroyAfter(TimeSpan.FromSeconds(2));

// Destroy temporary power-up after 10 seconds
powerUp.DestroyAfter(TimeSpan.FromSeconds(10));
```

**Throws:**
- `ArgumentOutOfRangeException` — If delay is negative

### CancelDestroyAfter()

Cancels pending delayed destruction.

```csharp
public bool CancelDestroyAfter()
```

**Returns:** `true` if destruction was cancelled

**Example:**
```csharp
// Player picks up power-up before it expires
if (powerUp.CancelDestroyAfter())
{
    // Power-up saved
}
```

### RespawnAt(Vector2 position, TimeSpan delay)

Configures entity to respawn after destruction.

```csharp
public void RespawnAt(Vector2 position, TimeSpan delay)
```

**Parameters:**
- `position` — Position to respawn at
- `delay` — Time to wait before respawn

**Example:**
```csharp
// Enemy respawns after 5 seconds
enemy.RespawnAt(spawnPoint, TimeSpan.FromSeconds(5));

// Health pack respawns after 30 seconds
healthPack.RespawnAt(originalPosition, TimeSpan.FromSeconds(30));
```

### CancelRespawnAt()

Cancels pending respawn.

```csharp
public bool CancelRespawnAt()
```

**Returns:** `true` if respawn was cancelled

**Example:**
```csharp
// Prevent respawn if level is ending
enemy.CancelRespawnAt();
```

### HasPendingRespawn Property

Checks if entity has pending respawn.

```csharp
public bool HasPendingRespawn { get; }
```

**Example:**
```csharp
if (enemy.HasPendingRespawn)
{
    // Show respawn timer
    ShowRespawnTimer(enemy);
}
```

## Usage Examples

### Temporary Power-Ups

```csharp
public class PowerUpEntity : Entity
{
    public void Activate()
    {
        // Power-up lasts 10 seconds
        DestroyAfter(TimeSpan.FromSeconds(10));
        
        // Visual effect
        PlayPickupEffect();
    }
    
    public void OnPlayerPickup(PlayerEntity player)
    {
        // Cancel destruction if picked up
        CancelDestroyAfter();
        
        // Apply power-up effect
        player.ApplyPowerUp(this);
        
        // Destroy immediately
        Destroy();
    }
}
```

### Delayed Spawns

```csharp
public class WaveSpawner
{
    public void SpawnWave(int enemyCount)
    {
        for (int i = 0; i < enemyCount; i++)
        {
            // Stagger spawns
            var delay = TimeSpan.FromSeconds(i * 0.5f);
            
            CoroutineManager.StartCoroutine(SpawnAfterDelay(
                spawnPoints[i % spawnPoints.Length],
                delay
            ));
        }
    }
    
    private IEnumerator SpawnAfterDelay(Vector2 position, TimeSpan delay)
    {
        yield return new WaitForSeconds((float)delay.TotalSeconds);
        
        var enemy = entitySystem.CreateEntity<EnemyEntity>(position);
        enemy.SetTag("Enemy");
    }
}
```

### Respawn Mechanics

```csharp
public class RespawnableEnemy : Entity
{
    private Vector2 _spawnPosition;
    
    public void Initialize(Vector2 spawnPosition)
    {
        _spawnPosition = spawnPosition;
        
        // Configure respawn
        RespawnAt(spawnPosition, TimeSpan.FromSeconds(5));
    }
    
    public override void OnDestroy()
    {
        base.OnDestroy();
        
        // Play death effect
        PlayDeathEffect();
        
        // Respawn will happen automatically
    }
}
```

### Temporary Effects

```csharp
public class ExplosionEntity : Entity
{
    public void Explode(Vector2 position, float radius)
    {
        Position = position;
        
        // Damage nearby entities
        var nearby = entitySystem.FindNearby(position, radius);
        foreach (var entity in nearby)
        {
            if (entity.HasTag("Enemy"))
            {
                entity.TakeDamage(50);
            }
        }
        
        // Destroy after animation completes
        DestroyAfter(TimeSpan.FromSeconds(1.5f));
    }
}
```

### Delayed Activation

```csharp
public class DelayedSpawnEntity : Entity
{
    public void ScheduleSpawn(Vector2 position, TimeSpan delay)
    {
        // Create inactive entity
        SetActive(false);
        Position = position;
        
        // Activate after delay
        CoroutineManager.StartCoroutine(ActivateAfterDelay(delay));
    }
    
    private IEnumerator ActivateAfterDelay(TimeSpan delay)
    {
        yield return new WaitForSeconds((float)delay.TotalSeconds);
        SetActive(true);
        OnStart();
    }
}
```

## Advanced Patterns

### Chain Reactions

```csharp
public class ChainReactionEntity : Entity
{
    public void TriggerChain()
    {
        // Destroy after delay
        DestroyAfter(TimeSpan.FromSeconds(2));
        
        // Schedule next in chain
        CoroutineManager.StartCoroutine(TriggerNext());
    }
    
    private IEnumerator TriggerNext()
    {
        yield return new WaitForSeconds(1f);
        
        var next = entitySystem.FindByType<ChainReactionEntity>()
            .FirstOrDefault(e => e != this);
        
        next?.TriggerChain();
    }
}
```

### Conditional Destruction

```csharp
public class ConditionalEntity : Entity
{
    private bool _conditionMet = false;
    
    public void WaitForCondition()
    {
        // Destroy after 10 seconds unless condition met
        DestroyAfter(TimeSpan.FromSeconds(10));
        
        // Check condition periodically
        CoroutineManager.StartCoroutine(CheckCondition());
    }
    
    private IEnumerator CheckCondition()
    {
        while (!_conditionMet)
        {
            yield return new WaitForSeconds(1f);
            
            if (CheckGameCondition())
            {
                _conditionMet = true;
                CancelDestroyAfter();
                Destroy();
            }
        }
    }
}
```

### Respawn with Variation

```csharp
public class RandomRespawnEntity : Entity
{
    public void RespawnRandomly(Vector2[] spawnPoints, TimeSpan delay)
    {
        // Pick random spawn point
        var randomPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
        
        // Respawn with variation
        RespawnAt(randomPoint, delay);
    }
}
```

## Best Practices

### When to Use Delayed Destruction

**Use for:**
- Temporary effects (explosions, particles)
- Power-ups with duration
- Timed obstacles
- UI notifications

**Don't use for:**
- Permanent entities
- Entities that need immediate cleanup
- Performance-critical entities

### Respawn Considerations

**Do:**
- Store original spawn position
- Use reasonable respawn delays
- Cancel respawn on level end
- Show respawn indicators

**Don't:**
- Respawn too frequently
- Respawn without player feedback
- Forget to cancel respawn

### Performance Tips

- Use `DestroyAfter()` instead of coroutines for simple delays
- Cancel pending operations when entity is destroyed
- Avoid chaining too many delayed operations
- Profile coroutine usage

## Common Patterns

### Temporary Invulnerability

```csharp
public class PlayerEntity : Entity
{
    public void MakeInvulnerable(float duration)
    {
        SetTag("Invulnerable");
        
        // Remove invulnerability after duration
        CoroutineManager.StartCoroutine(RemoveInvulnerability(duration));
    }
    
    private IEnumerator RemoveInvulnerability(float duration)
    {
        yield return new WaitForSeconds(duration);
        RemoveTag("Invulnerable");
    }
}
```

### Staggered Spawns

```csharp
public void SpawnEnemiesStaggered(Vector2[] positions)
{
    for (int i = 0; i < positions.Length; i++)
    {
        var delay = TimeSpan.FromSeconds(i * 0.3f);
        CoroutineManager.StartCoroutine(SpawnWithDelay(positions[i], delay));
    }
}
```

### Delayed Damage

```csharp
public class DelayedDamageEntity : Entity
{
    public void ApplyDelayedDamage(Entity target, int damage, float delay)
    {
        CoroutineManager.StartCoroutine(DealDamageAfterDelay(target, damage, delay));
    }
    
    private IEnumerator DealDamageAfterDelay(Entity target, int damage, float delay)
    {
        yield return new WaitForSeconds(delay);
        
        if (!target.Destroyed)
        {
            target.TakeDamage(damage);
        }
    }
}
```

## See Also

- [Entity System](./EntitySystem.md) — Core entity management
- [Coroutines](./Coroutines.md) — Time-based operations
- [Entity Pooling](./EntityPooling.md) — Reusing entities
