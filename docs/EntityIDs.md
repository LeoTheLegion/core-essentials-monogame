# Entity IDs & References 🔖

Unique identifiers and cross-entity linking for XML-driven scenes.

## Overview

Entity IDs provide a human-readable way to identify and look up entities at runtime. Combined with `EntityReference`, they enable clean cross-entity linking without tight coupling.

## Setting an ID

Assign a unique ID to any entity using `SetId()`:

```csharp
var player = system.CreateEntity<PlayerEntity>();
player.SetId("hero");
```

IDs are **case-insensitive** — `"Hero"`, `"hero"`, and `"HERO"` are treated as the same ID.

### Auto-Generated IDs

If you don't assign an ID, one is auto-generated on demand **internally** by the
framework (for example, when an entity is serialized or referenced). The public
`EnsureId()` method is `internal` to CoreEssentials, so you can't call it directly
from game code — assign IDs explicitly with `SetId()` when you need a stable lookup key.

Auto-generated IDs follow the pattern `{TypeName}_{8-char-GUID}`.

## Finding Entities by ID

Look up entities by their ID using `FindById()`:

```csharp
var hero = system.FindById("hero");
if (hero != null)
{
    // Do something with the hero
}
```

Returns `null` if no entity with that ID exists.

## Duplicate IDs

Each entity must have a **unique** ID. Attempting to assign a duplicate throws an `InvalidOperationException`:

```csharp
var entity1 = system.CreateEntity<EnemyEntity>();
entity1.SetId("guard");

var entity2 = system.CreateEntity<EnemyEntity>();
entity2.SetId("guard"); // ❌ Throws InvalidOperationException
```

## Entity References

Use `EntityReference` for **deferred** entity lookups. This is useful when an entity needs to reference another that may not exist yet (e.g., during scene loading).

### Creating a Reference

```csharp
var reference = new EntityReference("targetPlatform");
```

### Resolving a Reference

After all entities are loaded, resolve the reference:

```csharp
bool resolved = reference.Resolve(entityDictionary);
if (resolved)
{
    Entity target = reference.GetEntity();
}
```

### Auto-Resolution

Call `ResolveReferences()` on the `EntitySystem` to automatically resolve all references held by entities implementing `IEntityReferenceHolder`:

```csharp
// After loading your scene...
int resolvedCount = system.ResolveReferences();
Console.WriteLine($"Resolved {resolvedCount} references");
```

### Implementing IEntityReferenceHolder

```csharp
public class PlatformEntity : Entity, IEntityReferenceHolder
{
    public EntityReference TargetPlatform { get; set; }

    public int ResolveReferences(Dictionary<string, Entity> entities)
    {
        return TargetPlatform.Resolve(entities) ? 1 : 0;
    }
}
```

### Implicit Conversion

Resolved references can be implicitly converted to their entity:

```csharp
Entity? target = reference; // Null if not resolved
```

## Best Practices

- **Naming conventions**: Use lowercase with underscores (e.g., `"hero"`, `"chest_1"`, `"exit_door"`)
- **Set IDs early**: Assign IDs before entities are added to the scene
- **Check resolution**: Always check `IsResolved` before accessing `GetEntity()`
- **Handle missing refs**: Gracefully handle cases where a referenced entity doesn't exist

## API Summary

| Member | Type | Description |
|--------|------|-------------|
| `Entity.Id` | Property | Gets the entity's unique ID |
| `Entity.SetId(string)` | Method | Assigns a unique ID |
| `Entity.EnsureId()` | Method (`internal`) | Returns or generates an ID (framework-internal) |
| `EntitySystem.FindById(string)` | Method | Finds entity by ID |
| `EntitySystem.ResolveReferences()` | Method | Resolves all pending references |
| `EntityReference` | Class | Deferred entity lookup |
| `IEntityReferenceHolder` | Interface | Auto-resolvable reference holder |
