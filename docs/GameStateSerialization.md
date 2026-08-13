# Game State Serialization

The Game State Serialization system allows you to save and load the complete state of your game entities to XML files. This is perfect for implementing save games, checkpoints, and level persistence.

## Overview

The serialization system captures:
- Entity positions, rotations, **and scale**
- Entity tags and sort order
- Entity hierarchies (parent-child relationships)
- Active/inactive state
- **Custom entity state** via `SerializeToXml()` / `DeserializeFromXml()` overrides

> **Entity-driven approach**: Each entity explicitly declares what to save by overriding virtual methods. This makes serialization transparent, testable, and easy for developers.

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
// Load and replace all entities
entitySystem.LoadState("saves/game_save.xml", mergeExisting: false);

// Or merge with existing entities (preserves runtime-only entities)
entitySystem.LoadState("saves/game_save.xml", mergeExisting: true);
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
public void LoadState(string filePath, bool mergeExisting = false)
```
Loads game state from an XML file.

**Parameters:**
- `filePath`: Path to the save file
- `mergeExisting`: If `true`, merges saved state with existing entities. If `false`, replaces all entities.

### GameStateSerializer Methods

#### SaveState
```csharp
public static void SaveState(EntitySystem system, string filePath)
```
Saves entity system state to XML.

#### LoadState
```csharp
public static void LoadState(EntitySystem system, string filePath, bool mergeExisting = false)
```
Loads entity system state from XML file.

#### LoadStateFromXml
```csharp
public static void LoadStateFromXml(EntitySystem system, string xmlData, bool mergeExisting = false)
```
Loads entity system state from XML string.

## Entity-Driven Serialization

Instead of relying on components to know what to save, **entities explicitly declare their serialization needs** by overriding virtual methods. This approach is simpler, more transparent, and easier to debug.

### How It Works

The `Entity` base class provides two virtual methods:

```csharp
// Called during save - returns XElement with your state
public virtual XElement SerializeToXml()

// Called during load - restore your state from XElement
public virtual void DeserializeFromXml(XElement element, bool mergeExisting = false)
```

### Example: Entity with Custom State

```csharp
public class Player : Entity
{
    public int Score { get; set; }
    public float Health { get; set; }

    public override XElement SerializeToXml()
    {
        // Base saves Id, Type, Position, Rotation, Scale, Sort, Active, Tags
        var element = base.SerializeToXml();

        // Add custom state as a child element
        element.Add(new XElement("PlayerState",
            new XAttribute("Score", Score),
            new XAttribute("Health", Health)
        ));

        return element;
    }

    public override void DeserializeFromXml(XElement element, bool mergeExisting = false)
    {
        // Base restores Position, Rotation, Scale, Tags, etc.
        base.DeserializeFromXml(element, mergeExisting);

        // Restore custom state
        var playerState = element.Element("PlayerState");
        if (playerState != null)
        {
            if (int.TryParse(playerState.Attribute("Score")?.Value, out int score))
                Score = score;
            if (float.TryParse(playerState.Attribute("Health")?.Value,
                    System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture,
                    out float health))
                Health = health;
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

## Merge Mode

Merge mode allows you to load saved state while preserving entities created at runtime.

### When to Use Merge Mode

- **Merge Mode (`true`)**: Load save file but keep existing entities (good for loading additional content, checkpoints)
- **Replace Mode (`false`)**: Clear all entities and load from save (good for loading main save games)

### Example: Merge Mode
```csharp
// Create runtime-only entities
var uiElement = entitySystem.CreateEntity<UIElement>();
uiElement.SetId("hud");

// Load save game but keep UI elements
entitySystem.LoadState("saves/game.xml", mergeExisting: true);

// UI element still exists, game entities restored from save
```

## Best Practices

### 1. Always Assign IDs
Entities must have unique IDs to be saved and loaded properly:
```csharp
entity.SetId("player_character");
```

### 2. Handle Versioning
When changing entity structure, consider save file versioning:
```xml
<GameState Version="2.0" ...>
```
Update your loader to handle different versions.

### 3. Deferred Component Restoration

If your entity creates components in `OnStart()` (like loading sprites from assets), the components **won't exist yet** when `DeserializeFromXml()` is called because it runs *before* `OnStart()`.

**Solution**: Store the XML element during deserialization, then apply it after `OnStart()` creates the component:

```csharp
public class Ball : Entity
{
    private SpriteComponent? _spriteComponent;
    private XElement? _deferredSpriteElement;  // Stored for later

    public override void DeserializeFromXml(XElement element, bool mergeExisting = false)
    {
        base.DeserializeFromXml(element, mergeExisting);
        // Defer - component doesn't exist yet
        _deferredSpriteElement = element.Element("Sprite");
    }

    public override void OnStart()
    {
        base.OnStart();

        // Create component
        _spriteComponent = new SpriteComponent(AssetManager.LoadAsset<Sprite>("ball.png"));
        AddComponent(_spriteComponent);

        // Now restore deferred state
        if (_deferredSpriteElement != null && _spriteComponent != null)
        {
            var colorAttr = _deferredSpriteElement.Attribute("Color")?.Value;
            if (colorAttr != null && uint.TryParse(colorAttr, out uint argb))
            {
                _spriteComponent.Color = new Color(argb);
            }
        }
        _deferredSpriteElement = null;  // Clear
    }
}
```

### 4. Test Round-Trips
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
        
        _entitySystem.LoadState(filePath, mergeExisting: false);
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
- Verify component implements `ISerializableComponent`
- Check that `SerializeToXml()` and `DeserializeFromXml()` are implemented correctly
- Ensure component type name is preserved in XML (use `GetType().FullName` not `GetType().Name`)

### Physics State Not Persisting
- Make sure RigidbodyComponent and ColliderComponent are added before saving
- Velocity is only saved if the physics body has been created (check `IsBodyCreated`)
- On load, velocity is restored after the body is recreated in `OnStart()`

### Scale Issues After Loading
- Entity.Scale is now the single source of truth for scale
- Don't store scale in both Entity and components - SpriteComponent reads from Owner.Scale
- Check that old save files with Scale in SpriteComponent are migrated

### Merge Mode Issues
- Entities with duplicate IDs will be updated, not duplicated
- Runtime tags are preserved in merge mode
- New entities from save file will be created

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

- Sprint 10: XML Entity Definitions
- Sprint 12: Entity IDs
- Entity System Core

## See Also

- [Entity System Documentation](EntitySystem.md)
- [Entity Templates](XMLEntityDefinitions.md)
- [Physics System](PhysicsSystem.md)
