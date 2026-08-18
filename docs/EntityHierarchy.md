# Entity Hierarchy

Parent-child hierarchies allow entities to be organized in tree structures with transform inheritance. Perfect for character equipment, UI panels, and complex game objects.

## Overview

The hierarchy system provides:
- Parent-child relationships with automatic transform inheritance
- Local vs world position/rotation
- Recursive activation and destruction
- XML support for hierarchical definitions

## API Reference

### Parent Property

Gets or sets the parent entity.

```csharp
public Entity? Parent { get; private set; }
```

**Example:**
```csharp
var parent = entitySystem.CreateEntity<ParentEntity>(position);
var child = entitySystem.CreateEntity<ChildEntity>(position);

parent.AddChild(child);
Console.WriteLine(child.Parent == parent); // true
```

### Children Property

Gets the list of child entities.

```csharp
public List<Entity> Children { get; }
```

**Example:**
```csharp
foreach (var child in parent.Children)
{
    child.Update(gameTime);
}
```

### LocalPosition Property

Gets or sets the position relative to parent.

```csharp
public Vector2 LocalPosition { get; set; }
```

**Example:**
```csharp
// Child positioned 50 units to the right of parent
child.LocalPosition = new Vector2(50, 0);
```

### LocalRotation Property

Gets or sets the rotation relative to parent.

```csharp
public float LocalRotation { get; set; }
```

**Example:**
```csharp
// Child rotated 45 degrees relative to parent
child.LocalRotation = MathHelper.ToRadians(45);
```

### AddChild(Entity child)

Adds an entity as a child.

```csharp
public void AddChild(Entity child)
```

**Parameters:**
- `child` — Entity to add as child

**Throws:**
- `ArgumentNullException` — If child is null
- `ArgumentException` — If adding self or creating circular reference

**Example:**
```csharp
var character = entitySystem.CreateEntity<CharacterEntity>(position);
var weapon = entitySystem.CreateEntity<WeaponEntity>(position);

character.AddChild(weapon);
weapon.LocalPosition = new Vector2(30, 0); // Weapon offset from character
```

### RemoveChild(Entity child)

Removes an entity from children.

```csharp
public bool RemoveChild(Entity child)
```

**Parameters:**
- `child` — Entity to remove

**Returns:** `true` if removed, `false` if not found

**Example:**
```csharp
parent.RemoveChild(child);
Console.WriteLine(child.Parent == null); // true
```

## Transform Inheritance

### World Position

When an entity has a parent, its world position (`Position`) is calculated as:

```
Position = Parent.Position + LocalPosition
```

**Example:**
```csharp
var parent = entitySystem.CreateEntity<ParentEntity>(new Vector2(100, 100));
var child = entitySystem.CreateEntity<ChildEntity>(Vector2.Zero);

parent.AddChild(child);
child.LocalPosition = new Vector2(50, 0);

// Child world position is (150, 100)
Console.WriteLine(child.Position); // Vector2(150, 100)
```

### World Rotation

World rotation (`Rotation`) is calculated as:

```
Rotation = Parent.Rotation + LocalRotation
```

**Example:**
```csharp
parent.Rotation = MathHelper.ToRadians(90);
child.LocalRotation = MathHelper.ToRadians(45);

// Child world rotation is 135 degrees
Console.WriteLine(child.Rotation); // 2.356 radians (135°)
```

## Usage Examples

### Character with Equipment

```csharp
public class CharacterEntity : Entity
{
    public void EquipWeapon(WeaponEntity weapon)
    {
        AddChild(weapon);
        weapon.LocalPosition = new Vector2(30, 0); // Right hand
    }
    
    public void EquipArmor(ArmorEntity armor)
    {
        AddChild(armor);
        armor.LocalPosition = Vector2.Zero; // Centered on character
    }
}

// Usage
var character = entitySystem.CreateEntity<CharacterEntity>(position);
var sword = entitySystem.CreateEntity<SwordEntity>(Vector2.Zero);
character.EquipWeapon(sword);

// Sword moves with character automatically
character.Position += new Vector2(10, 0);
// Sword position updates automatically
```

### UI Panel with Children

```csharp
public class UIPanelEntity : Entity
{
    public void AddButton(ButtonEntity button, Vector2 offset)
    {
        AddChild(button);
        button.LocalPosition = offset;
    }
}

// Usage
var panel = entitySystem.CreateEntity<UIPanelEntity>(new Vector2(400, 300));
panel.AddButton(okButton, new Vector2(-50, 20));
panel.AddButton(cancelButton, new Vector2(50, 20));

// Moving panel moves all buttons
panel.Position = new Vector2(500, 300);
// Buttons maintain relative positions
```

### Spaceship with Parts

```csharp
var ship = entitySystem.CreateEntity<SpaceshipEntity>(position);

// Add engines
var leftEngine = entitySystem.CreateEntity<EngineEntity>(Vector2.Zero);
leftEngine.LocalPosition = new Vector2(-50, 0);
ship.AddChild(leftEngine);

var rightEngine = entitySystem.CreateEntity<EngineEntity>(Vector2.Zero);
rightEngine.LocalPosition = new Vector2(50, 0);
ship.AddChild(rightEngine);

// Add turrets
var turret = entitySystem.CreateEntity<TurretEntity>(Vector2.Zero);
turret.LocalPosition = new Vector2(0, -30);
ship.AddChild(turret);

// All parts move with ship
ship.Position += velocity;
```

## XML Integration

Hierarchical entities can be defined in XML:

```xml
<EntityDefinition Type="Spaceship" Id="ship">
    <Position X="100" Y="200" />
    <Children>
        <EntityDefinition Type="Engine" Id="leftEngine">
            <Position X="-50" Y="0" />
        </EntityDefinition>
        <EntityDefinition Type="Engine" Id="rightEngine">
            <Position X="50" Y="0" />
        </EntityDefinition>
        <EntityDefinition Type="Turret" Id="mainTurret">
            <Position X="0" Y="-30" />
        </EntityDefinition>
    </Children>
</EntityDefinition>
```

Child positions are relative to parent.

## Advanced Features

### Recursive Activation

Activating/deactivating a parent affects all children:

```csharp
parent.SetActive(false);
// All children become inactive automatically

parent.SetActive(true);
// All children become active automatically
```

### Recursive Destruction

Destroying a parent destroys all children:

```csharp
parent.Destroy();
// All children destroyed recursively
```

### Deep Hierarchy

```csharp
var grandparent = entitySystem.CreateEntity<Entity>(position);
var parent = entitySystem.CreateEntity<Entity>(Vector2.Zero);
var child = entitySystem.CreateEntity<Entity>(Vector2.Zero);

grandparent.AddChild(parent);
parent.AddChild(child);

parent.LocalPosition = new Vector2(50, 0);
child.LocalPosition = new Vector2(25, 0);

// Child world position = grandparent + 50 + 25
```

## Best Practices

### Hierarchy Design

**Do:**
- Use hierarchies for logical grouping
- Keep hierarchies shallow (3-4 levels max)
- Use local positions for relative placement
- Document parent-child relationships

**Don't:**
- Create circular references
- Use deep hierarchies (performance impact)
- Modify parent position directly for child movement
- Forget to handle hierarchy in serialization

### Performance Considerations

- Hierarchy traversal is O(n) for n children
- Deep hierarchies increase update cost
- Consider flattening for performance-critical entities
- Use spatial partitioning for large hierarchies

### Common Patterns

**Character Equipment:**
```csharp
character.AddChild(weapon);
character.AddChild(armor);
character.AddChild(helmet);
```

**UI Layout:**
```csharp
panel.AddChild(title);
panel.AddChild(content);
panel.AddChild(buttons);
```

**Vehicle Parts:**
```csharp
vehicle.AddChild(wheel1);
vehicle.AddChild(wheel2);
vehicle.AddChild(engine);
```

## See Also

- [Entity System](./EntitySystem.md) — Core entity management
- [XML Entity Definitions](./XMLEntityDefinitions.md) — Loading hierarchies from XML
- [Entity Position Rotation](./EntityPositionRotation.md) — Position and rotation details
