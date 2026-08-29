# Game State Serialization

The Game State Serialization system allows you to save and load the complete state of your game entities to XML files. This is perfect for implementing save games, checkpoints, and level persistence.

## Overview

The serialization system captures:
- Entity positions, rotations, **and scale**
- Entity tags and sort order
- Entity hierarchies (parent-child relationships)
- Active/inactive state
- **Custom entity state** via `ISaveableEntity` interface

> **Opt-in approach**: Only entities implementing `ISaveableEntity` are saved and loaded. This gives you full control over what persists and makes serialization explicit and testable.

## Quick Start

### Saving Game State

```csharp
// Save the entire entity system state
entitySystem.SaveState("saves/game_save.xml");

// Or using the serializer directly
GameStateSerializer.SaveState(entitySystem, "saves/game_save.xml");
```

### Loading Game State

```csharp
// Load state - replaces all ISaveableEntity instances with what's in the save file
// Entities not implementing ISaveableEntity are unaffected
entitySystem.LoadState("saves/game_save.xml");

// After loading, any ISaveableEntity instances NOT in the save file will be automatically removed
// This ensures the game state exactly matches what was saved
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
- Only entities implementing `ISaveableEntity` are affected
- Entities with matching IDs in the save file are updated in place
- ISaveableEntity instances NOT in the save file are automatically removed after loading
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

## ISaveableEntity Interface

Serialization is **opt-in** — only entities implementing `ISaveableEntity` are saved and loaded. This gives you full control over what persists and makes serialization explicit.

### The ISaveableEntity Interface

```csharp
public interface ISaveableEntity
{
    XElement SaveState();
    void LoadState(XElement element);
}
```

### How Loading Works

When loading state, the serializer follows an ID-based replace flow:

1. **Collect IDs** — gather all entity IDs from the save file (including nested children)
2. **Load entities** — for each saved entity:
   - If an entity with that ID exists → update it in place via `LoadState()`
   - Otherwise → create new entity and call `LoadState()`
3. **Cleanup** — remove any ISaveableEntity instances whose ID wasn't in the save file

This ensures the game state **exactly matches** what was saved.

### Example: Entity with Custom State

```csharp
public class Player : Entity, ISaveableEntity
{
    public int Score { get; set; }
    public float Health { get; set; }

    public XElement SaveState()
    {
        return new XElement("PlayerState",
            new XAttribute("Score", Score),
            new XAttribute("Health", Health)
        );
    }

    public void LoadState(XElement element)
    {
        var playerState = element.Element("PlayerState");
        if (playerState != null)
        {
            if (int.TryParse(playerState.Attribute("Score")?.Value, out int score))
                Score = score;
            if (float.TryParse(playerState.Attribute("Health")?.Value,
                    NumberStyles.Any, CultureInfo.InvariantCulture,
                    out float health))
                Health = health;
        }
    }
}
```

### Example: Physics Entity (Ball)

```csharp
public class Ball : Entity, ISaveableEntity
{
    private RigidbodyComponent? _rigidbody;
    private SpriteComponent? _sprite;

    public override void OnStart()
    {
        base.OnStart();
        _sprite = new SpriteComponent(AssetManager.LoadAsset<Sprite>("ball.png"));
        AddComponent(_sprite);
        _rigidbody = new RigidbodyComponent(RigidbodyType.Dynamic);
        AddComponent(_rigidbody);
    }

    public XElement SaveState()
    {
        var state = new XElement("BallState",
            new XAttribute("Color", _sprite?.Color.ToArgb() ?? 0)
        );

        // The RigidbodyComponent exposes velocity directly (no need to reach into the body).
        if (_rigidbody != null && _rigidbody.IsBodyCreated)
        {
            state.Add(new XElement("Physics",
                new XAttribute("LinearVelocityX", _rigidbody.LinearVelocity.X),
                new XAttribute("LinearVelocityY", _rigidbody.LinearVelocity.Y),
                new XAttribute("AngularVelocity", _rigidbody.AngularVelocity)
            ));
        }

        return state;
    }

    public void LoadState(XElement element)
    {
        var ballState = element.Element("BallState");
        if (ballState != null && _sprite != null)
        {
            var colorAttr = ballState.Attribute("Color")?.Value;
            if (colorAttr != null && int.TryParse(colorAttr, out int argb))
                _sprite.Color = new Color(argb);
        }

        var physics = element.Element("Physics");
        if (physics != null && _rigidbody != null && _rigidbody.IsBodyCreated)
        {
            if (float.TryParse(physics.Attribute("LinearVelocityX")?.Value,
                    NumberStyles.Any, CultureInfo.InvariantCulture, out float velX) &&
                float.TryParse(physics.Attribute("LinearVelocityY")?.Value,
                    NumberStyles.Any, CultureInfo.InvariantCulture, out float velY))
            {
                // Setting LinearVelocity directly restores the saved velocity.
                _rigidbody.LinearVelocity = new Vector2(velX, velY);
            }

            if (float.TryParse(physics.Attribute("AngularVelocity")?.Value,
                    NumberStyles.Any, CultureInfo.InvariantCulture, out float angVel))
            {
                _rigidbody.AngularVelocity = angVel;
            }
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

### Physics Entity Example (Ball)
```xml
<Entity Id="vip_ball_blue" Type="CoreEssentials.Playground.Ball" Rotation="-2.4139123" Sort="0" Active="true">
  <Position X="583.62" Y="250.98" />
  <Scale X="2" Y="2" />
  <Tags>
    <Tag Name="Ball" />
    <Tag Name="Physical" />
  </Tags>
  <Physics LinearVelocityX="-51.93" LinearVelocityY="-108.18" AngularVelocity="-2.34" />
  <Sprite Color="4294901760" />
</Entity>
```

## Entity Cleanup on Load

When loading state, any ISaveableEntity instances **not present in the save file** will be automatically removed. This ensures the loaded game state exactly matches what was saved.

### How It Works

```csharp
// Create some entities
var ball1 = entitySystem.CreateEntity<Ball>();
ball1.SetId("ball_1");
ball1.Implements(ISaveableEntity);

var ball2 = entitySystem.CreateEntity<Ball>();
ball2.SetId("ball_2");
ball2.Implements(ISaveableEntity);

// UI elements don't implement ISaveableEntity, so they're unaffected
var hud = entitySystem.CreateEntity<HUDElement>();
hud.SetId("hud");

// Save state (both balls are saved)
entitySystem.SaveState("saves/game.xml");

// Create another ball at runtime
var ball3 = entitySystem.CreateEntity<Ball>();
ball3.SetId("ball_3");

// Load state - ball3 will be removed since it's not in the save file
entitySystem.LoadState("saves/game.xml");

// Result: ball1, ball2 exist (from save), ball3 is removed, hud still exists (not saveable)
```

### Preserving Runtime Entities

To keep entities like UI elements, cameras, or debug overlays across save/load cycles, simply **don't implement ISaveableEntity**:

```csharp
// This entity won't be saved or affected by LoadState
public class CameraEntity : Entity
{
    // No ISaveableEntity implementation
    // Entity persists across all save/load operations
}
```

### Controlling What Gets Saved

| Interface Implemented | Saved? | Removed on Load if not in file? |
|-----------------------|--------|----------------------------------|
| `ISaveableEntity`     | Yes    | Yes                              |
| None                  | No     | No                               |

## Best Practices

### 1. Always Assign IDs
Entities must have unique IDs to be saved and loaded properly:
```csharp
entity.SetId("player_character");
```

### 2. Implement ISaveableEntity for Serializable Entities
Only entities that need to persist should implement the interface:
```csharp
public class Player : Entity, ISaveableEntity
{
    public XElement SaveState() { /* ... */ }
    public void LoadState(XElement element) { /* ... */ }
}
```

### 3. Don't Implement ISaveableEntity for Runtime-Only Entities
UI elements, cameras, and debug overlays should NOT implement `ISaveableEntity`:
```csharp
// This entity persists across all save/load operations
public class CameraEntity : Entity
{
    // No ISaveableEntity - unaffected by serialization
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
- Make sure your entity implements `ISaveableEntity`
- Check that components are created in `OnStart()` before `LoadState()` is called
- Verify component type names are preserved in XML (use `GetType().FullName` not `GetType().Name`)

### Physics State Not Persisting
- Make sure RigidbodyComponent and ColliderComponent are added before saving
- Velocity is only saved if the physics body has been created (check `IsBodyCreated`)
- On load, velocity is restored after the body is recreated in `OnStart()`

### Scale Issues After Loading
- Entity.Scale is now the single source of truth for scale
- Don't store scale in both Entity and components - SpriteComponent reads from Owner.Scale
- Check that old save files with Scale in SpriteComponent are migrated

### Entities Disappearing After Load
- Only ISaveableEntity instances are affected by loading
- If an entity implements ISaveableEntity but isn't in the save file, it will be removed
- To preserve runtime entities (UI, cameras), don't implement ISaveableEntity

## Built-In Serializable Components

The following components implement `ISerializableComponent` and are automatically saved/loaded:

### SpriteComponent
Saves visual properties of the sprite.
```xml
<SpriteState 
  ColorR="255" ColorG="0" ColorB="0" ColorA="255"
  OriginX="0.5" OriginY="0.5"
  Effects="None" LayerDepth="0" />
```

**Note:** Scale is now stored on the `Entity` base class, not in SpriteComponent.

### RigidbodyComponent
Saves physics body properties and velocity.
```xml
<RigidbodyState 
  Type="Dynamic"
  Mass="1.0" FixedRotation="false"
  SyncFromPhysics="true"
  LinearVelocityX="10.5" LinearVelocityY="-20.3"
  AngularVelocity="0.75" />
```

### ColliderComponent
Saves collider shape and material properties.
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
- [Entity Templates](EntityTemplates.md)
- [Physics System](PhysicsSystem.md)
