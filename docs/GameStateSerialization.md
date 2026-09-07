# Game State Serialization

The Game State Serialization system allows you to save and load the complete state of your game entities to XML files. This is perfect for implementing save games, checkpoints, and level persistence.

## Overview

The serialization system captures:
- Entity positions, rotations, **and scale**
- Entity tags and sort order
- Entity hierarchies (parent-child relationships)
- Active/inactive state
- **Custom entity state** via an `ISaveableComponent` attached to the entity

> **Opt-in approach**: An entity is saveable iff it has a component implementing `ISaveableComponent`. This gives you full control over what persists and makes serialization explicit and testable — no entity subclassing required.
>
> **Prefab-driven:** saves carry a `Prefab="..."` attribute recording the prefab each entity was instantiated from, and loading recreates entities via `EntitySystem.Instantiate`. Prefab-declared components (sprite, rigidbody, collider) come back automatically; the save component then restores exactly the state it cares about.
>
> **Legacy:** the older `ISaveableEntity` interface still works for old saves and unmigrated entities, but is marked `[Obsolete]`. New code should attach an `ISaveableComponent` instead.

## Quick Start

### Attaching a Save Component

```csharp
// Make any entity saveable by attaching a component that implements ISaveableComponent.
// The component decides exactly what to save (typically transform + tags + the public
// properties of whichever sibling components matter).
entity.AddComponent(new MySaveComponent());
```

### Saving Game State

```csharp
// Save the entire entity system state
entitySystem.SaveState("saves/game_save.xml");

// Or using the serializer directly
GameStateSerializer.SaveState(entitySystem, "saves/game_save.xml");
```

### Loading Game State

```csharp
// Load state - recreates saveable entities from their saved Prefab and restores their state.
// Entities with no save component are unaffected.
entitySystem.LoadState("saves/game_save.xml");

// After loading, any saveable entity NOT in the save file will be automatically removed
// This ensures the game state exactly matches what was saved.
// NOTE: the prefabs referenced by the save must be registered (e.g. their scene loaded) first.
```

## API Reference

### EntitySystem Methods

#### SaveState
```csharp
public void SaveState(string filePath)
```
Saves the complete entity system state to an XML file.

**Parameters:**
- `filePath`: Path where the save file will be created

#### LoadState
```csharp
public void LoadState(string filePath)
```
Loads game state from an XML file.

**Parameters:**
- `filePath`: Path to the save file

**Behavior:**
- Only entities carrying an `ISaveableComponent` (or legacy `ISaveableEntity`) are affected
- Entities with matching IDs in the save file are updated in place
- Saveable entities NOT in the save file are automatically removed after loading
- Non-saveable entities (UI, cameras, etc.) are unaffected

### GameStateSerializer Methods

#### SaveState
```csharp
public static void SaveState(EntitySystem system, string filePath)
```
Saves entity system state to XML.

#### LoadState
```csharp
public static void LoadState(EntitySystem system, string filePath)
```
Loads entity system state from XML file.

#### LoadStateFromXml
```csharp
public static void LoadStateFromXml(EntitySystem system, string xmlData)
```
Loads entity system state from XML string.

## ISaveableComponent Interface

Serialization is **opt-in** — an entity is saveable iff it has a component implementing `ISaveableComponent`. This gives you full control over what persists and makes serialization explicit. The component decides exactly what to save (typically the owner's transform + tags + the public properties of whichever sibling components matter).

### The ISaveableComponent Interface

```csharp
public interface ISaveableComponent
{
    XElement SaveState();
    void LoadState(XElement element);
}
```

- `SaveState()` returns the full `<Entity>` element for the owner. The framework serializer stamps the authoritative `Prefab="..."` attribute on it and appends `<Children>`.
- `LoadState(element)` is called by the serializer **after** the entity has been recreated from its prefab (so sibling components exist), letting you restore exactly what you saved.

### How Loading Works

When loading state, the serializer follows a prefab-driven flow:

1. **Collect IDs** — gather all saveable entity IDs from the save file (including nested children)
2. **Load entities** — for each saved entity:
   - If it carries an `ISaveableComponent` and has a `Prefab` attribute → recreate via `EntitySystem.Instantiate(prefab, position)`, set the saved Id, then call the component's `LoadState(element)`
   - Otherwise (legacy save without a `Prefab`) → fall back to creating the entity by its `Type` and call `LoadState()`
3. **Cleanup** — remove any saveable entity whose ID wasn't in the save file

This ensures the game state **exactly matches** what was saved.

> **Prefab registration is required at load time.** The prefabs referenced by a save must be registered before loading (true for scenes, which register their prefabs before spawning entities). Loading a save that references an unregistered prefab throws an actionable `KeyNotFoundException` (surfaced as the inner exception of an `InvalidOperationException`).

### Example: A Save Component with Custom State

```csharp
public class PlayerSaveComponent : EntityComponent, ISaveableComponent
{
    public int Score { get; set; }
    public float Health { get; set; }

    public XElement SaveState()
    {
        return new XElement("Entity",
            new XAttribute("Id", Owner.Id ?? string.Empty),
            new XAttribute("Score", Score),
            new XAttribute("Health", Health)
        );
    }

    public void LoadState(XElement element)
    {
        if (int.TryParse(element.Attribute("Score")?.Value, out int score))
            Score = score;
        if (float.TryParse(element.Attribute("Health")?.Value,
                NumberStyles.Any, CultureInfo.InvariantCulture,
                out float health))
            Health = health;
    }
}
```

### Example: A Save Component for a Physics Ball

The common case is a component-composed entity whose save component explicitly serializes the
owner's transform + tags and the public properties of whichever sibling components matter. The
playground's ball does exactly this in `BallSaveComponent : EntityComponent, ISaveableComponent` —
it reads each value by name (sprite color/asset, rigidbody mass + velocity, collider settings) with
no per-component serialization interface; the full implementation follows.

```csharp
public class BallSaveComponent : EntityComponent, ISaveableComponent
{
    public XElement SaveState()
    {
        var element = new XElement("Entity",
            new XAttribute("Id", Owner.Id ?? string.Empty),
            new XElement("Position",
                new XAttribute("X", Owner.Position.X.ToString(CultureInfo.InvariantCulture)),
                new XAttribute("Y", Owner.Position.Y.ToString(CultureInfo.InvariantCulture))));

        // Explicit per-component serialization, read by name from each sibling's public properties.
        if (Owner.TryGetComponent<SpriteComponent>(out var sprite) && sprite != null)
            element.Add(new XElement("SpriteState", new XAttribute("ColorR", sprite.Color.R)));

        if (Owner.TryGetComponent<RigidbodyComponent>(out var rb) && rb != null)
            element.Add(new XElement("RigidbodyState",
                new XAttribute("LinearVelocityX", rb.LinearVelocity.X),
                new XAttribute("LinearVelocityY", rb.LinearVelocity.Y)));

        return element; // the serializer stamps Prefab="..." and appends <Children>
    }

    public void LoadState(XElement element)
    {
        var spriteEl = element.Element("SpriteState");
        if (spriteEl != null && Owner.TryGetComponent<SpriteComponent>(out var sprite) && sprite != null)
            sprite.Color = new Color(byte.Parse(spriteEl.Attribute("ColorR")?.Value ?? "255"), 0, 0);

        var rbEl = element.Element("RigidbodyState");
        if (rbEl != null && Owner.TryGetComponent<RigidbodyComponent>(out var rb) && rb != null)
        {
            // Ensure the body exists at the restored position before velocity is applied.
            if (!rb.IsBodyCreated) rb.CreateBody();
            rb.SetLinearVelocity(new Vector2(
                float.Parse(rbEl.Attribute("LinearVelocityX")?.Value ?? "0"),
                float.Parse(rbEl.Attribute("LinearVelocityY")?.Value ?? "0")));
        }
    }
}
```

## XML Schema

### GameState Root
```xml
<GameState Version="1.0" Timestamp="2026-01-01T00:00:00Z">
  <Entities>
    <!-- Entity definitions -->
  </Entities>
</GameState>
```

### Entity Element
```xml
<Entity Id="player_1" Type="PlayerEntity" Rotation="0.785" Sort="10" Active="true">
  <Position X="100" Y="200" />
  <Scale X="1.5" Y="1.5" />
  <Tags>
    <Tag Name="player" />
    <Tag Name="controllable" />
  </Tags>
  <!-- Custom state added by entity overrides -->
  <PlayerState Score="42" Health="85.5" />
  <Children>
    <!-- Child entities -->
  </Children>
</Entity>
```

### Physics Entity Example (Ball — component-based serialization)

The playground's physics ball is a plain `GameObjectEntity` carrying a `BallSaveComponent`. Its save
shape is **explicit**: transform + tags, plus one element for each component the ball needs to
restore (`<SpriteState/>`, `<RigidbodyState/>`, `<ColliderState/>`). Each value is read by name from
the component's public properties — adding a new saved field means adding it to `BallSaveComponent`'s
save/load code. The authoritative `Prefab="..."` attribute records which prefab recreated the entity
on load (the `Type` attribute is kept for legacy readability only).

```xml
<Entity Id="vip_ball_blue" Prefab="BallPrefab" Type="CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.GameObjectEntity" Rotation="-2.4139123" Sort="0" Active="true">
  <Position X="583.62" Y="250.98" />
  <Scale X="2" Y="2" />
  <Tags>
    <Tag Name="Ball" />
    <Tag Name="Physical" />
  </Tags>
  <SpriteState ColorR="255" ColorG="174" ColorB="201" ColorA="255" OriginX="0.5" OriginY="0.5" Effects="" LayerDepth="0" SortOrderOverride="-1" AnimationFrame="0" SpriteAsset="Sprites/ball_sprite.xml" />
  <RigidbodyState Type="Dynamic" Mass="4" FixedRotation="False" SyncFromPhysics="True" LinearVelocityX="-51.93" LinearVelocityY="-108.18" AngularVelocity="-2.34" />
  <ColliderState ShapeType="Circle" Friction="0.2" Restitution="1" Categories="Cat2" CollidesWith="Cat2" OffsetX="0" OffsetY="1" Radius="96" />
</Entity>
```

## Entity Cleanup on Load

When loading state, any saveable entities (those carrying an `ISaveableComponent`) **not present in the save file** will be automatically removed. This ensures the loaded game state exactly matches what was saved.

### How It Works

```csharp
// Create some entities and make them saveable by attaching a save component
var ball1 = entitySystem.CreateEntity<GameObjectEntity>();
ball1.SetId("ball_1");
ball1.AddComponent(new BallSaveComponent());

var ball2 = entitySystem.CreateEntity<GameObjectEntity>();
ball2.SetId("ball_2");
ball2.AddComponent(new BallSaveComponent());

// UI elements have no save component, so they're unaffected
var hud = entitySystem.CreateEntity<HUDElement>();
hud.SetId("hud");

// Save state (both balls are saved)
entitySystem.SaveState("saves/game.xml");

// Create another ball at runtime (also saveable)
var ball3 = entitySystem.CreateEntity<GameObjectEntity>();
ball3.SetId("ball_3");
ball3.AddComponent(new BallSaveComponent());

// Load state - ball3 will be removed since it's not in the save file
entitySystem.LoadState("saves/game.xml");

// Result: ball1, ball2 exist (from save), ball3 is removed, hud still exists (not saveable)
```

### Preserving Runtime Entities

To keep entities like UI elements, cameras, or debug overlays across save/load cycles, simply **don't attach a save component**:

```csharp
// This entity has no ISaveableComponent, so it won't be saved or affected by LoadState
var camera = entitySystem.CreateEntity<CameraEntity>();
// Entity persists across all save/load operations
```

### Controlling What Gets Saved

| Save Component Attached | Saved? | Removed on Load if not in file? |
|-------------------------|--------|----------------------------------|
| `ISaveableComponent`    | Yes    | Yes                              |
| None                    | No     | No                               |

## Best Practices

### 1. Always Assign IDs
Entities must have unique IDs to be saved and loaded properly:
```csharp
entity.SetId("player_character");
```

### 2. Attach a Save Component for Serializable Entities
Only entities that need to persist should carry an `ISaveableComponent`:
```csharp
entity.AddComponent(new MySaveComponent());
```

### 3. Don't Attach a Save Component to Runtime-Only Entities
UI elements, cameras, and debug overlays should NOT carry an `ISaveableComponent`:
```csharp
// This entity persists across all save/load operations
var camera = entitySystem.CreateEntity<CameraEntity>();
// No save component - unaffected by serialization
}
```

### 4. Handle Versioning
When changing entity structure, consider save file versioning:
```xml
<GameState Version="2.0" ...>
```
Update your loader to handle different versions.

### 5. Test Round-Trips
Always test save/load round-trips:
```csharp
// Save
entitySystem.SaveState("test.xml");

// Load into fresh system
var newSystem = new EntitySystem();
GameStateSerializer.LoadState(newSystem, "test.xml");

// Verify state matches
```

## Example: Complete Save Game System

```csharp
public class SaveGameManager
{
    private readonly EntitySystem _entitySystem;
    private readonly string _saveDirectory;

    public SaveGameManager(EntitySystem entitySystem, string saveDirectory)
    {
        _entitySystem = entitySystem;
        _saveDirectory = saveDirectory;
    }

    public void SaveGame(string slotName)
    {
        var filePath = Path.Combine(_saveDirectory, $"{slotName}.xml");
        _entitySystem.SaveState(filePath);
        
        // Also save metadata
        SaveMetadata(slotName);
    }

    public void LoadGame(string slotName)
    {
        var filePath = Path.Combine(_saveDirectory, $"{slotName}.xml");
        
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"Save slot '{slotName}' not found");
        
        _entitySystem.LoadState(filePath);
    }

    public void QuickSave()
    {
        SaveGame("quick_save");
    }

    public void QuickLoad()
    {
        LoadGame("quick_save");
    }

    private void SaveMetadata(string slotName)
    {
        // Save play time, level, etc.
    }
}
```

## Troubleshooting

### Entities Not Saving
- Ensure entities have IDs assigned with `SetId()`
- Check that entities are added to the EntitySystem before saving

### Components Not Restoring
- Make sure your entity carries an `ISaveableComponent`
- Sibling components come back from the prefab on load; the save component restores their saved state in `LoadState()`
- Verify component type names are preserved in XML (use `GetType().FullName` not `GetType().Name`)

### Physics State Not Persisting
- Make sure RigidbodyComponent and ColliderComponent are declared before saving
- Velocity is only saved if the physics body has been created (check `IsBodyCreated`)
- On load, velocity is restored after the body is recreated (the save component calls `CreateBody()` first)

### Scale Issues After Loading
- Entity.Scale is now the single source of truth for scale
- Don't store scale in both Entity and components - SpriteComponent reads from Owner.Scale
- Check that old save files with Scale in SpriteComponent are migrated

### Entities Disappearing After Load
- Only saveable entities (those carrying an `ISaveableComponent`) are affected by loading
- If a saveable entity isn't in the save file, it will be removed
- To preserve runtime entities (UI, cameras), don't attach a save component

## Component State Elements

A save component (e.g. the playground's `BallSaveComponent`) writes these elements by reading each built-in
component's public properties. The element shapes below are what a save component emits and restores:

### SpriteComponent
Visual properties of the sprite.
```xml
<SpriteState 
  ColorR="255" ColorG="0" ColorB="0" ColorA="255"
  OriginX="0.5" OriginY="0.5"
  Effects="None" LayerDepth="0" />
```

**Note:** Scale is now stored on the `Entity` base class, not in SpriteComponent.

### RigidbodyComponent
Physics body properties and velocity.
```xml
<RigidbodyState 
  Type="Dynamic"
  Mass="1.0" FixedRotation="false"
  SyncFromPhysics="true"
  LinearVelocityX="10.5" LinearVelocityY="-20.3"
  AngularVelocity="0.75" />
```

### ColliderComponent
Collider shape and material properties.
```xml
<ColliderState 
  ShapeType="Circle"
  Friction="0.5" Restitution="1.0"
  OffsetX="0" OffsetY="1"
  Radius="25" />
```

## Entity Scale Property

Entity now has a `Scale` property (like `Position` and `Rotation`) that is automatically serialized:

```csharp
// Set entity scale
entity.Scale = new Vector2(2.0f, 2.0f);

// Components like SpriteComponent read from Owner.Scale
// No need to store scale in individual components
```

This eliminates redundancy - previously each entity stored its own scale, now it's a single source of truth on the Entity base class.

## Dependencies

- [XML Entity Definitions](XMLEntityDefinitions.md)
- [Entity IDs](EntityIDs.md)
- Entity System Core

## See Also

- [Entity System Documentation](EntitySystem.md)
- [Prefabs](Prefabs.md)
- [Physics System](PhysicsSystem.md)
