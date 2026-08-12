# Entity Tags

Tags provide a flexible way to categorize and query entities without creating deep inheritance hierarchies. Use tags for gameplay logic, filtering, and organization.

## Overview

The tagging system allows you to:
- Assign multiple tags to any entity
- Query entities by tag for gameplay logic
- Filter entities for rendering or updates
- Organize entities by type, behavior, or gameplay role

Tags are case-insensitive and stored in a `HashSet<string>` for fast lookups.

## API Reference

### Entity Methods

#### SetTag(string tag)

Adds a tag to the entity. Tags are case-insensitive.

```csharp
public void SetTag(string tag)
```

**Parameters:**
- `tag` — The tag name to add. Cannot be null or whitespace.

**Example:**
```csharp
var enemy = entitySystem.CreateEntity<EnemyEntity>(position);
enemy.SetTag("Enemy");
enemy.SetTag("Hostile");
enemy.SetTag("GroundUnit");
```

**Throws:**
- `ArgumentNullException` — When tag is null or whitespace

#### RemoveTag(string tag)

Removes a tag from the entity.

```csharp
public bool RemoveTag(string tag)
```

**Parameters:**
- `tag` — The tag name to remove

**Returns:** `true` if the tag was removed, `false` if not found

**Example:**
```csharp
enemy.RemoveTag("Hostile"); // Enemy becomes neutral
```

#### HasTag(string tag)

Checks if the entity has a specific tag.

```csharp
public bool HasTag(string tag)
```

**Parameters:**
- `tag` — The tag name to check

**Returns:** `true` if the entity has the tag

**Example:**
```csharp
if (entity.HasTag("Collectible"))
{
    player.Score += 100;
}
```

### EntitySystem Methods

#### GetEntitiesByTag(string tag)

Returns all entities with the specified tag.

```csharp
public List<Entity> GetEntitiesByTag(string tag)
```

**Parameters:**
- `tag` — The tag to search for

**Returns:** List of entities with the tag

**Example:**
```csharp
// Find all enemies in the scene
var enemies = entitySystem.GetEntitiesByTag("Enemy");

foreach (var enemy in enemies)
{
    enemy.TakeDamage(10);
}
```

#### FindByTag(string tag)

Returns the first entity with the specified tag.

```csharp
public Entity? FindByTag(string tag)
```

**Parameters:**
- `tag` — The tag to search for

**Returns:** First matching entity, or `null` if not found

**Example:**
```csharp
var player = entitySystem.FindByTag("Player");
if (player != null)
{
    player.Position = spawnPoint;
}
```

## Usage Examples

### Tagging Enemies

```csharp
// Create different enemy types with tags
var goblin = entitySystem.CreateEntity<GoblinEntity>(position);
goblin.SetTag("Enemy");
goblin.SetTag("GroundUnit");
goblin.SetTag("Melee");

var archer = entitySystem.CreateEntity<ArcherEntity>(position);
archer.SetTag("Enemy");
archer.SetTag("Flying");
archer.SetTag("Ranged");

// Query all enemies
var allEnemies = entitySystem.GetEntitiesByTag("Enemy");

// Query specific enemy types
var meleeEnemies = entitySystem.GetEntitiesByTag("Melee");
var flyingEnemies = entitySystem.GetEntitiesByTag("Flying");
```

### Projectile Management

```csharp
// Tag projectiles for collision detection
var bullet = entitySystem.CreateEntity<BulletEntity>(position);
bullet.SetTag("Projectile");
bullet.SetTag("PlayerOwned");

// In collision handler
if (entity.HasTag("Projectile") && entity.HasTag("PlayerOwned"))
{
    // Damage enemies
    var enemies = entitySystem.GetEntitiesByTag("Enemy");
    foreach (var enemy in enemies)
    {
        if (Vector2.Distance(entity.Position, enemy.Position) < 10f)
        {
            enemy.TakeDamage(25);
        }
    }
}
```

### Collectibles

```csharp
// Create collectible items
var coin = entitySystem.CreateEntity<CoinEntity>(position);
coin.SetTag("Collectible");
coin.SetTag("Currency");

// Player collection logic
var collectibles = entitySystem.GetEntitiesByTag("Collectible");
foreach (var item in collectibles)
{
    if (Vector2.Distance(player.Position, item.Position) < 20f)
    {
        player.AddCoins(1);
        item.Destroy();
    }
}
```

### Game State Filtering

```csharp
// Pause game by deactivating non-UI entities
var allEntities = entitySystem.GetEntities();
foreach (var entity in allEntities)
{
    if (!entity.HasTag("UI") && !entity.HasTag("Persistent"))
    {
        entity.SetActive(false);
    }
}

// Resume game
var pausedEntities = entitySystem.GetEntitiesByTag("Paused");
foreach (var entity in pausedEntities)
{
    entity.SetActive(true);
}
```

## XML Integration

Tags can be defined in XML entity definitions:

```xml
<EntityDefinition Type="EnemyEntity" Id="goblin1">
    <Position X="100" Y="200" />
    <Tags>
        <Tag Name="Enemy" />
        <Tag Name="GroundUnit" />
        <Tag Name="Melee" />
    </Tags>
</EntityDefinition>
```

## Best Practices

### Tag Naming Conventions

- Use PascalCase for tag names: `Enemy`, `Player`, `Collectible`
- Be descriptive but concise
- Use consistent naming across your project
- Consider creating a constants file for common tags

```csharp
public static class EntityTags
{
    public const string Enemy = "Enemy";
    public const string Player = "Player";
    public const string Collectible = "Collectible";
    public const string Projectile = "Projectile";
    public const string UI = "UI";
}
```

### Performance Considerations

- Tag lookups are O(1) due to internal indexing
- `GetEntitiesByTag()` returns a copy of the list — avoid calling it every frame
- Cache tag queries when possible
- Use tags for broad categorization, not fine-grained state

### When to Use Tags vs Inheritance

**Use Tags when:**
- Entities share behavior but have different base types
- You need dynamic categorization
- Categories overlap (an entity can be both "Enemy" and "Flying")
- You need runtime categorization

**Use Inheritance when:**
- Entities share common implementation
- Behavior is fixed at compile time
- You need polymorphic methods

## See Also

- [Entity Query API](./EntityQueryAPI.md) — Finding entities by type and position
- [Entity System](./EntitySystem.md) — Core entity management
- [XML Entity Definitions](./XMLEntityDefinitions.md) — Loading entities from XML
