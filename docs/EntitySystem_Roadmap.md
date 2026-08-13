# Entity System Roadmap

Future enhancements and feature ideas for the Entity System in CoreEssentials-MonoGame.

## Current State

The Entity System is an **OOP-based architecture** with:

- `Entity` base class providing Position, Rotation, Sort order, Active/Destroyed lifecycle
- `EntitySystem` managing entities via `List<Entity>` with reflection-based `CreateEntity<T>()`
- Simple sort-based render ordering
- No hierarchy, tagging, pooling, or event system

---

## Quick Wins (Low Effort, High Impact)

### 1. Entity Tags

Add string-based tagging for easy grouping and lookup without type-checking.

**API Sketch:**
```csharp
entity.SetTag("enemy");
entity.SetTag("projectile");

var enemies = entitySystem.GetEntitiesByTag("enemy");
var projectiles = entitySystem.GetEntitiesByTag("projectile");
```

**Benefits:** Cleaner scene logic, no `is`/`as` casts, runtime-friendly grouping.

**MonoGame.Extended Coverage:** ❌ Not covered. Their ECS uses components for grouping instead of tags.

---

### 2. Entity Query API

Convenient lookup methods on `EntitySystem`.

**API Sketch:**
```csharp
// Find entities by type
var balls = entitySystem.FindByType<Ball>();

// Find entities near a position
var nearby = entitySystem.FindNearby(player.Position, 500f);

// Find entities by tag
var enemies = entitySystem.FindByTag("enemy");
```

**Benefits:** Reduces boilerplate loops and manual distance checks.

**MonoGame.Extended Coverage:** ❌ Not covered. Their ECS has system queries but no spatial/nearby lookups.

---

### 3. Entity Pooling

Recycle destroyed entities instead of letting GC collect them.

**API Sketch:**
```csharp
// Pool-aware creation (returns pooled instance if available)
var bullet = entitySystem.CreatePooled<Bullet>(position);

// Return to pool instead of destroying
entitySystem.ReleasePooled<Bullet>(bullet);
```

**Benefits:** Massive performance win for high-spawn-rate entities (projectiles, particles, etc.).

**MonoGame.Extended Coverage:** ❌ Not covered. No object pooling utilities in the library.

---

## Medium Lift

### 4. Parent-Child Hierarchy

Transform inheritance so child entities follow their parent.

**API Sketch:**
```csharp
var character = entitySystem.CreateEntity<CharacterEntity>(position);
var weapon = entitySystem.CreateEntity<WeaponEntity>(Vector2.Zero); // local offset

character.AddChild(weapon);
// weapon now inherits character's Position, Rotation, and Scale
```

**Benefits:** Clean composition for character + accessories, UI groups, effect chains.

**MonoGame.Extended Coverage:** ❌ Not covered. Their ECS has no parent-child transform hierarchy.

---

### 5. Event System

Decoupled publish/subscribe so entities don't need direct references.

**API Sketch:**
```csharp
// Subscribe
entity.Subscribe("OnHit", (data) => { /* react */ });

// Publish
entity.Publish("OnHit", new { Damage = 10, Source = attacker });
```

**Benefits:** Loose coupling, easier testing, cleaner entity design.

**MonoGame.Extended Coverage:** ❌ Not covered. Their ECS uses data-driven systems, not publish/subscribe events.

---

### 6. Render Batching by Texture

Group draw calls by active texture instead of one `SpriteBatch.Begin/End` per system.

**Current:** All entities render in a single batch regardless of texture.
**Future:** Sort entities by texture before drawing to minimize state changes.

**Benefits:** Better GPU utilization, especially with many unique textures.

**MonoGame.Extended Coverage:** ❌ Not covered. No render batching utilities for SpriteBatch.

---

## Bigger Features

### 7. Lightweight Components

Mixin-style behaviors without going full ECS.

**API Sketch:**
```csharp
public class HealthComponent
{
    public int Current { get; set; }
    public int Maximum { get; set; }
    public event Action OnDeath;
}

// Add to any entity
entity.AddComponent<HealthComponent>(new HealthComponent { Maximum = 100 });

// Access later
var health = entity.GetComponent<HealthComponent>();
health.Current -= damage;
```

**Benefits:** Composable behavior, keeps OOP simplicity, avoids deep inheritance chains.

**MonoGame.Extended Coverage:** ✅ Partially covered. Their ECS provides components (VelocityComponent, PositionComponent, etc.), but we're taking a mixin approach rather than full ECS adoption.

---

### 11. XML Entity Definitions

Declarative entity definitions using XML, matching the existing asset and GUI layout patterns.

**XML Schema Sketch:**
```xml
<?xml version="1.0" encoding="utf-8"?>
<EntityDefinition xmlns="http://schemas.coreessentials.monogame/2025/entity">
  <EntityType>PlayerEntity</EntityType>
  <Position X="400" Y="300" />
  <Rotation>0</Rotation>
  <Sort>10</Sort>
  <Tag>player</Tag>
  
  <Components>
    <Component Type="HealthComponent">
      <Property Name="Current" Value="100" />
      <Property Name="Maximum" Value="100" />
    </Component>
    
    <Component Type="SpriteComponent">
      <Property Name="SpriteAsset" Value="player_sprite.xml" />
      <Property Name="Scale" Value="1.0" />
    </Component>
    
    <Component Type="PhysicsComponent">
      <Property Name="BodyType" Value="Dynamic" />
      <Property Name="Shape" Value="Circle" />
      <Property Name="Radius" Value="32" />
      <Property Name="Mass" Value="1.0" />
    </Component>
  </Components>
  
  <Children>
    <EntityDefinition Source="weapon.xml" />
  </Children>
</EntityDefinition>
```

**API Sketch:**
```csharp
// Load entity definition from XML file
var player = entitySystem.LoadEntityFromXml("player_entity.xml");

// Load from XMLAsset
var xmlAsset = AssetManager.LoadAsset<XMLAsset>("player_entity.xml");
var player = entitySystem.LoadEntityFromXml(xmlAsset);

// Or load a whole scene from XML
var scene = entitySystem.LoadSceneFromXml("level1_entities.xml");
```

**Serializer Logic:**
```csharp
public static class EntitySerializer
{
    public static TEntity LoadEntity<TEntity>(string xmlData, EntitySystem system) where TEntity : Entity;
    public static TEntity LoadEntity<TEntity>(XMLAsset asset, EntitySystem system) where TEntity : Entity;
    public static Entity LoadEntity(string xmlData, EntitySystem system);
    public static void SaveEntity(Entity entity, string filePath);
}
```

**Benefits:**
- **Designer-friendly** — level designers can author entities without touching code
- **Consistent** with existing XML patterns (assets, GUI layouts)
- **Data-driven** — tweak entity properties without recompiling
- **Reusable** — define templates (`goblin.xml`, `chest.xml`) and instantiate multiple times
- **Version-controllable** — plain text diffs for entity changes
- **Works with Components** — components become declarative XML nodes

**MonoGame.Extended Coverage:** ❌ Not covered. No XML entity definition or scene loading system.

**Implementation Notes:**
- Reuse `XDocument`/`XElement` parsing from `GuiSerializer`
- Component registration via a `IComponentFactory` dictionary
- Support `<Property>` for simple types (int, float, string, bool, Vector2, Color)
- Support `<Reference>` for linking to other entities by ID
- Support `<Children>` for parent-child hierarchy (Feature #4)

---

### 8. Spatial Partitioning

Grid or quadtree for fast spatial queries.

**API Sketch:**
```csharp
// Automatic partitioning on EntitySystem
var nearby = entitySystem.FindInBounds(new Rectangle(x, y, width, height));
var closest = entitySystem.FindClosest(position, maxRadius);
```

**Benefits:** O(1) or O(log n) lookups instead of O(n) iteration for collision, AI, etc.

**MonoGame.Extended Coverage:** ❌ Not covered. No quadtree or spatial grid utilities in the library.

---

### 9. Entity Groups/Layers

Logical grouping for independent update/render control.

**API Sketch:**
```csharp
entitySystem.CreateLayer("background");
entitySystem.CreateLayer("foreground");

var bgEntity = entitySystem.CreateEntity<BackgroundEntity>(position, "background");
var fgEntity = entitySystem.CreateEntity<PlayerEntity>(position, "foreground");

// Update only specific layers
entitySystem.UpdateLayer("foreground", gameTime);
```

**Benefits:** Pausing layers, independent render passes, performance tuning.

**MonoGame.Extended Coverage:** ❌ Not covered. No layer/group management for entities.

---

### 10. Delayed Lifecycle

Built-in spawn/destroy/respawn scheduling on coroutines.

**API Sketch:**
```csharp
// Spawn after delay
entitySystem.SpawnAfter<Explosion>(position, TimeSpan.FromSeconds(2f));

// Destroy after delay
entity.DestroyAfter(TimeSpan.FromSeconds(3f));

// Respawn at position
entity.RespawnAt(originalPosition, TimeSpan.FromSeconds(5f));
```

**Benefits:** Cleaner timing logic, no manual coroutine boilerplate.

**MonoGame.Extended Coverage:** ❌ Not covered. No delayed lifecycle utilities.

---

### 12. Entity Templates / Prefabs

Reusable entity blueprints that can be instantiated multiple times from a single XML definition.

**XML Schema Sketch:**
```xml
<!-- goblin.xml (template) -->
<EntityTemplate Name="Goblin" xmlns="...">
  <EntityType>EnemyEntity</EntityType>
  <Tag>enemy</Tag>
  <Components>
    <Component Type="HealthComponent">
      <Property Name="Maximum" Value="50" />
    </Component>
    <Component Type="SpriteComponent">
      <Property Name="SpriteAsset" Value="goblin_sprite.xml" />
    </Component>
  </Components>
</EntityTemplate>

<!-- level1.xml (uses template) -->
<EntityDefinition>
  <Template Source="goblin.xml" Position="100, 200" />
  <Template Source="goblin.xml" Position="300, 400" />
  <Template Source="goblin.xml" Position="500, 200" />
</EntityDefinition>
```

**API Sketch:**
```csharp
// Register a template
entitySystem.RegisterTemplate("goblin", "goblin.xml");

// Instantiate from template
var goblin1 = entitySystem.Instantiate("goblin", new Vector2(100, 200));
var goblin2 = entitySystem.Instantiate("goblin", new Vector2(300, 400));
```

**MonoGame.Extended Coverage:** ❌ Not covered. No prefab/template system.

**Benefits:** DRY scene definitions, consistent enemy/object spawning, easy to tweak one template and propagate.

---

### 13. Entity IDs & References

Unique identifiers and cross-entity linking for XML-driven scenes.

**XML Schema Sketch:**
```xml
<EntityDefinition>
  <EntityType>PlayerEntity</EntityType>
  <Id>hero</Id>
  <Position X="400" Y="300" />
</EntityDefinition>

<EntityDefinition>
  <EntityType>ChestEntity</EntityType>
  <Id>treasure_chest</Id>
  <Position X="800" Y="600" />
  <Components>
    <Component Type="LootComponent">
      <Property Name="TargetEntity" Reference="hero" />
    </Component>
  </Components>
</EntityDefinition>
```

**API Sketch:**
```csharp
// Get entity by ID
var hero = entitySystem.FindById("hero");

// Set up references after loading
entitySystem.ResolveReferences();
```

**MonoGame.Extended Coverage:** ❌ Not covered. No entity ID or reference resolution system.

**Benefits:** Named entity lookups, designer-friendly scene graphs, no fragile type/position-based matching.

---

### 14. Game State Serialization (Save/Load)

Serialize and restore the full entity state for save games.

**XML Schema Sketch:**
```xml
<GameState xmlns="...">
  <Entity Id="hero" Type="PlayerEntity">
    <Position X="400" Y="300" />
    <Rotation>1.57</Rotation>
    <Active>true</Active>
    <Component Type="HealthComponent">
      <Property Name="Current" Value="75" />
      <Property Name="Maximum" Value="100" />
    </Component>
  </Entity>
  <Entity Id="chest1" Type="ChestEntity">
    <Position X="800" Y="600" />
    <Component Type="LootComponent">
      <Property Name="Opened" Value="true" />
    </Component>
  </Entity>
</GameState>
```

**API Sketch:**
```csharp
// Save current entity state
entitySystem.SaveState("savegame.xml");

// Load entity state - replaces ISaveableEntity instances with saved state
// Entities not implementing ISaveableEntity are unaffected
entitySystem.LoadState("savegame.xml");
```

**MonoGame.Extended Coverage:** ❌ Not covered. No save/load serialization for game state.

**Benefits:** Save games, checkpoint systems, replay functionality, scene debugging snapshots.

---

### 15. Entity Tweening

Built-in animation for position, rotation, scale, and custom properties.

**API Sketch:**
```csharp
// Animate position
entity.TweenPosition(new Vector2(800, 600), TimeSpan.FromSeconds(2f), Easing.Linear);

// Animate rotation
entity.TweenRotation(MathF.PI / 2, TimeSpan.FromSeconds(1f), Easing.OutQuad);

// Animate scale
entity.TweenScale(2.0f, TimeSpan.FromSeconds(0.5f), Easing.OutBounce);

// Chain tweens
entity.TweenPosition(pos1, 1s)
      .ThenTweenPosition(pos2, 2s)
      .ThenTweenRotation(angle, 1s);

// Cancel
entity.CancelTweens();
```

**MonoGame.Extended Coverage:** ✅ Covered. The Tweening system (Tween<T>, EasingFunctions, LinearTween) provides position/scale/rotation animation with easing. We'd wrap it for entity convenience methods.

**Benefits:** Smooth animations without manual interpolation, easing curves, chainable sequences.

---

### 16. Entity Debug Visualization

Draw entity metadata in the editor/debug mode.

**API Sketch:**
```csharp
// Enable debug mode
entitySystem.DebugMode = true;

// Configurable overlays
DebugDraw.ShowEntityBounds = true;   // draw bounding boxes
DebugDraw.ShowEntityIds = true;      // draw entity IDs
DebugDraw.ShowEntityTags = true;     // draw tags
DebugDraw.ShowEntityHierarchy = true;// draw parent-child lines

**MonoGame.Extended Coverage:** ❌ Not covered. No entity debug visualization (PrimitiveBatch exists for drawing shapes, but no entity overlays).
DebugDraw.ShowEntityPosition = true; // draw position markers
```

**Benefits:** Visual debugging, rapid iteration, easier to spot issues in scene layout.

---

### 17. Entity Lifecycle Hooks

Additional lifecycle events for fine-grained control.

**API Sketch:**
```csharp
public class MyEntity : Entity
{
    // Called when SetActive(true) is called
    public override void OnEnable() { }

    // Called when SetActive(false) is called
    public override void OnDisable() { }

    // Called when the entity is paused (scene pause)
    public override void OnPause() { }

    // Called when the entity is unpaused
    public override void OnResume() { }

    // Called after the entity is added to a system (before OnStart)
    public override void OnAwake() { }

**MonoGame.Extended Coverage:** ❌ Not covered. Their ECS has no enable/disable/pause lifecycle hooks.
}
```

**Benefits:** Cleaner lifecycle management, pause/resume support, better resource handling.

---

### 18. Entity Relationships

Weak-reference relationships between entities (target, owner, follower, etc.).

**API Sketch:**
```csharp
// Set a relationship
turret.SetRelationship("target", enemy);
enemy.SetRelationship("attacker", turret);

// Get a relationship
var target = turret.GetRelationship<Entity>("target");

// Remove a relationship
turret.RemoveRelationship("target");

// Event when relationship changes

**MonoGame.Extended Coverage:** ❌ Not covered. No entity relationship/link system.
turret.OnRelationshipChanged += (name, oldEntity, newEntity) => { /* react */ };
```

**Benefits:** Named entity links, no tight coupling, easy to serialize to XML.

---

### 19. Scriptable Behaviors

Attach coroutines or scripts declaratively via XML or API.

**XML Schema Sketch:**
```xml
<Component Type="ScriptComponent">
  <Script Name="PatrolBehavior">
    <Parameter Name="PatrolPoints" Value="point1, point2, point3" />
    <Parameter Name="Speed" Value="100" />
  </Script>
  <Script Name="LookAtPlayer">
    <Parameter Name="Target" Reference="hero" />
    <Parameter Name="UpdateRate" Value="0.1" />
  </Script>
</Component>
```

**API Sketch:**
```csharp
// Register a behavior
ScriptRegistry.Register("PatrolBehavior", (entity, args) => PatrolCoroutine(entity, args));

**MonoGame.Extended Coverage:** ❌ Not covered. No scriptable behavior or designer-authored logic system.

// Attach to entity
entity.AddScript("PatrolBehavior", new { PatrolPoints = points, Speed = 100f });
```

**Benefits:** Designer-authored behaviors, no code changes for new patterns, reusable across entities.

---

### 20. Entity Collision Groups

Assign entities to collision groups for filtered interaction.

**API Sketch:**
```csharp
// Define groups
entitySystem.CreateCollisionGroup("projectiles");
entitySystem.CreateCollisionGroup("enemies");
entitySystem.CreateCollisionGroup("player");

// Assign entity to group
bullet.AddToCollisionGroup("projectiles");
enemy.AddToCollisionGroup("enemies");
player.AddToCollisionGroup("player");

// Query collisions
var hits = entitySystem.GetCollidingEntities("projectiles", "enemies");


**MonoGame.Extended Coverage:** ❌ Not covered. Collision2D exists for shape intersection, but no collision group filtering.

---
// Set collision matrix (which groups collide)
entitySystem.SetCollisionEnabled("projectiles", "player", true);
entitySystem.SetCollisionEnabled("projectiles", "projectiles", false);
```

**Benefits:** Filtered collision checks, optimization (skip unnecessary checks), logical grouping.

---

## Prioritization

| Priority | Feature | Effort | Impact |
|----------|---------|--------|--------|
| P0 | Entity Tags | Low | High |
| P0 | Entity Query API | Low | High |
| P1 | Entity Pooling | Medium | High |
| P1 | Event System | Medium | High |
| P1 | XML Entity Definitions | Medium | High |
| P2 | Parent-Child Hierarchy | Medium | Medium |
| P2 | Lightweight Components | Medium | Medium |
| P2 | Entity Templates/Prefabs | Low | High |
| P2 | Entity IDs & References | Low | Medium |
| P2 | Entity Lifecycle Hooks | Low | Medium |
| P3 | Spatial Partitioning | High | High |
| P3 | Render Batching | Medium | Medium |
| P3 | Game State Serialization | Medium | High |
| P3 | Entity Tweening | Medium | Medium |
| P3 | Entity Relationships | Medium | Medium |
| P3 | Collision Groups | Medium | Medium |
| P4 | Entity Groups/Layers | High | Medium |
| P4 | Delayed Lifecycle | Low | Medium |
| P4 | Debug Visualization | Low | Medium |
| P4 | Scriptable Behaviors | Medium | Medium |

---

## Implementation Notes

- Each feature should be implemented on a `feature/entity-xxx` branch
- Tests and documentation are required per project conventions
- Features should be backward-compatible with existing entity code
- Consider the Playground as a living testbed for new features
