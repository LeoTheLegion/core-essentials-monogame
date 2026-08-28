# XML Entity Definitions

Load entities and entire scenes from declarative XML files, eliminating boilerplate code and enabling rapid level design.

## Overview

The `EntitySerializer` provides static methods to serialize and deserialize entities from XML. This follows the same pattern as `GuiSerializer` for UI widgets, ensuring consistency across the framework.

## Quick Start

```csharp
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Serialization;

// Load a single entity from an XML string
string xml = @"
    <Entity>
        <Position X=""100"" Y=""200"" />
        <Tags><Tag Name=""Enemy"" /></Tags>
    </Entity>";

var enemy = EntitySerializer.LoadEntity<MyEnemyEntity>(xml, entitySystem);

// Load a scene from a file
var scene = EntitySerializer.LoadSceneFromFile("levels/level1.xml", entitySystem);
```

## Loading Entities

### From XML String

```csharp
var player = EntitySerializer.LoadEntity<PlayerEntity>(xmlString, entitySystem);
```

### From File

```csharp
var player = EntitySerializer.LoadEntityFromFile<PlayerEntity>("entities/player.xml", entitySystem);
```

The generic type parameter `<T>` specifies the concrete entity type to instantiate.

## Saving Entities

### To File

```csharp
EntitySerializer.SaveEntity(player, "entities/player.xml");
```

### To String

```csharp
string xml = EntitySerializer.SaveEntityToString(player);
```

## Loading Scenes

Scenes contain multiple entities with hierarchical relationships and cross-references.

```csharp
// Load from XML string
var entities = EntitySerializer.LoadSceneFromXml(sceneXml, entitySystem);

// Load from file
var entities = EntitySerializer.LoadSceneFromFile("scenes/boss_fight.xml", entitySystem);
```

Returns a list of root entities. Child entities are automatically linked via `AddChild()`.

## Entity XML Schema

### Basic Structure

```xml
<Entity Rotation="1.5" Sort="10">
    <Position X="100" Y="200" />
    <Tags>
        <Tag Name="Player" />
        <Tag Name="Collidable" />
    </Tags>
</Entity>
```

### Attributes

| Attribute | Type | Required | Description |
|-----------|------|----------|-------------|
| `Rotation` | float | No | Entity rotation in radians |
| `Sort` | int | No | Render sort order |

### Position Element

```xml
<Position X="100" Y="200" />
```

Sets the entity's X and Y coordinates.

### Tags Element

```xml
<Tags>
    <Tag Name="Player" />
    <Tag Name="Collidable" />
</Tags>
```

Adds one or more tags for categorization and filtering.

## Component Loading

Attach components to entities via the `<Components>` section:

```xml
<Entity>
    <Position X=""0"" Y=""0"" />
    <Components>
        <Component Type=""SpriteComponent"">
            <Properties>
                <Property Name=""Scale"" Value=""2,2"" />
                <Property Name=""Color"" Value=""Red"" />
            </Properties>
        </Component>
        <Component Type=""RigidbodyComponent"" />
    </Components>
</Entity>
```

### Built-In Components

The following components are registered by default:

| Component | Description |
|-----------|-------------|
| `SpriteComponent` | Sprite-based rendering |
| `AnimationComponent` | Named sprite-frame animations |
| `RigidbodyComponent` | Physics body (Dynamic, Static, Kinematic) |
| `ColliderComponent` | Collision detection |
| `CanvasComponent` | GUI canvas owner (screen- or world-space) |
| `LabelComponent` | GUI text label bound to the nearest canvas |
| `ButtonComponent` | GUI clickable button bound to the nearest canvas |
| `AnchorComponent` | Unity-style anchor + offset positioning within a canvas |
| `CameraComponent` | Anchors a camera to the owning entity (synced in `LateUpdate`) |

### Supported Property Types

Properties are parsed via reflection. Supported types:

- **int** — Integer values (`Value="5"`)
- **float** — Decimal values (`Value="3.14"`)
- **bool** — Boolean values (`Value="true"`)
- **string** — Text values (`Value="Hello"`)
- **Vector2** — Two floats separated by comma (`Value="1,2"`), or a bare scalar for uniform scaling (`Value="1.5"` → `(1.5, 1.5)`)
- **Color** — Named colors (`Value="Red"`, `Value="LightGreen"`) or numeric `R,G,B[,A]` strings (`Value="100,255,100"`, `Value="100,255,100,128"`)
- **enum** — Any enum type (`Value="Static"`)

## Scene XML Schema

### Basic Structure

```xml
<Scene>
    <EntityDefinition Type="PlayerEntity" Id="hero">
        <Position X="100" Y="200" />
        <Tags><Tag Name="Player" /></Tags>
    </EntityDefinition>
    
    <EntityDefinition Type="EnemyEntity" Id="villain">
        <Position X="400" Y="300" />
        <Tags><Tag Name="Enemy" /></Tags>
    </EntityDefinition>
</Scene>
```

### Entity Definition Attributes

| Attribute | Type | Required | Description |
|-----------|------|----------|-------------|
| `Type` | string | **Yes** | Entity class name (must inherit from `Entity`) |
| `Id` | string | No | Unique identifier for references |

### Parent-Child Hierarchy

Use `<Children>` to nest entities:

```xml
<EntityDefinition Type="Spaceship" Id="ship">
    <Position X=""0"" Y=""0"" />
    <Children>
        <EntityDefinition Type="Engine" Id="leftEngine">
            <Position X=""-50"" Y=""0"" />
        </EntityDefinition>
        <EntityDefinition Type="Engine" Id="rightEngine">
            <Position X=""50"" Y=""0"" />
        </EntityDefinition>
    </Children>
</EntityDefinition>
```

Child positions are relative to the parent.

### Entity References

Link entities by `Id` using `<References>`:

```xml
<EntityDefinition Type="Projectile" Id="laser">
    <Position X=""0"" Y=""-100"" />
    <References>
        <Reference Name=""Owner"" TargetId=""ship"" />
    </References>
</EntityDefinition>
```

References are resolved after all entities are created (two-pass loading).

### Component Reference Injection

A `<Reference>` is first applied to the entity itself. If the entity has no matching settable member, the serializer falls back to the **first component** exposing a settable property or public field of an assignable `Entity` type:

```xml
<EntityDefinition Type="GameObjectEntity" Id="hud">
    <Components>
        <Component Type="ScoreKeeperComponent" />
    </Components>
    <References>
        <!-- ScoreLabel is a public Entity? field on ScoreKeeperComponent -->
        <Reference Name="ScoreLabel" TargetId="scoreText" />
    </References>
</EntityDefinition>
```

## Declarative Command Binding

`<Bind>` elements wire a public event to a public handler method directly in the scene file — Unity-style PersistentCall/SendMessage wiring — so interactive scenes (menus, HUDs) need no `FindById` + subscribe code in a scene class.

### The Two Forms

```xml
<!-- SendMessage style: named command resolved by search -->
<Bind Event="Clicked" Command="StartGame" />

<!-- PersistentCall style: explicit target component + member -->
<Bind Event="Clicked" Target="MenuActions" Member="StartGame" />
```

| Attribute | Required | Description |
|-----------|----------|-------------|
| `Event` | **Yes** | Name of the public event to subscribe to. |
| `Command` | One of the forms | Handler method name, resolved by searching (see below). |
| `Target` + `Member` | One of the forms | Restrict resolution to a component (or entity) whose type matches `Target` (short or full name, case-insensitive), and call its `Member` method. |
| `Source` | No | Restrict *event* lookup to a specific component (by short/full type name) or the entity itself. Useful when several components raise same-named events. |

Provide `Command` **or** `Target`+`Member` — not both. Binds may be direct children of `<EntityDefinition>`/`<Entity>`, siblings of `<Component>` elements inside `<Components>`, or nested inside a `<Component>` element.

### Resolution Order

- **Command form**: the handler is searched for in order — the entity itself → its components (attach order) → ancestor entities (nearest first, via `Parent`), each with their components. The first public instance method with that name wins.
- **Target+Member form**: only types matching `Target` are considered; within those, the same owner walk applies and `Member` must exist as a public instance method.

This means a button entity can bind to a handler on its own component *or* on a parent (e.g. the scene root carrying a state-keeping component), keeping behavior co-located with data.

### Supported Event Signatures & Handler Requirements

| Event signature | Payload delivered? |
|-----------------|--------------------|
| `Action` | No — handler takes 0 parameters. |
| `Action<T>` | Yes, if the handler's single parameter is `object` or assignable from `T`. |
| `EventHandler` | Yes, as `EventArgs` (or a derived type) via a 1-parameter handler. |

Handler methods must be **public instance methods, non-generic**, taking **0 or 1** parameters. Any other event signature (e.g. `Func<T,bool>`) is unsupported.

### Fail-Safe Behavior

Binds never throw: missing events, unresolvable handlers, and incompatible signatures each log a `[Serialization] ... — bind skipped.` console warning and are skipped, so a malformed scene degrades gracefully instead of crashing the game loop. Exceptions thrown *by* a handler during an event raise are caught (unwrapped from `TargetInvocationException`) and logged the same way.

### Example: XML-Only Menu Wiring

From the playground's `GuiAnchorDemo.xml` — the score buttons bind to methods on a shared component attached to the HUD root, with zero scene-class wiring code:

```xml
<EntityDefinition Type="GameObjectEntity" Id="hud">
    <Components>
        <Component Type="ScoreKeeperComponent" />
    </Components>
    <References>
        <Reference Name="ScoreLabel" TargetId="scoreText" />
    </References>
    <Children>
        <EntityDefinition Type="GameObjectEntity" Id="addScoreButton">
            <Components>
                <Component Type="ButtonComponent">
                    <Properties><Property Name="Label" Value="+10" /></Properties>
                </Component>
            </Components>
            <!-- Clicked (Action) on ButtonComponent -> ScoreKeeperComponent.AddTen() -->
            <Bind Event="Clicked" Command="AddTen" />
        </EntityDefinition>

        <EntityDefinition Type="GameObjectEntity" Id="resetButton">
            <Components>
                <Component Type="ButtonComponent">
                    <Properties><Property Name="Label" Value="Reset" /></Properties>
                </Component>
            </Components>
            <Bind Event="Clicked" Command="Reset" />
        </EntityDefinition>
    </Children>
</EntityDefinition>
```

The matching C# component (registered on the `IComponentFactory` you pass to the loader) simply exposes public methods and reads its injected label:

```csharp
public class ScoreKeeperComponent : EntityComponent
{
    public Entity? ScoreLabel;          // injected from <Reference Name="ScoreLabel" .../>
    private int _score;

    public void AddTen() => SetScore(_score + 10);
    public void Reset()  => SetScore(0);

    private void SetScore(int value)
    {
        _score = value;
        ScoreLabel?.GetComponent<LabelComponent>()?.Text = $"Score: {value}";
    }
}
```

```csharp
var factory = new DefaultComponentFactory();
factory.Register("ScoreKeeperComponent", () => new ScoreKeeperComponent());
scene.LoadEntitiesFromXml("GuiAnchorDemo.xml", entitySystem, factory);
```

## Custom Component Factory

The serializer resolves `<Component Type="...">` entries through an `IComponentFactory`. If you don't pass one to the loader, it builds a default with only the built-ins registered — so **custom components must be registered on your own factory and passed in**:

```csharp
// 1. Subclass EntityComponent (parameterless constructor shown)
public class HealthComponent : EntityComponent
{
    public int MaxHealth { get; set; } = 100;
}

// 2. Register it and pass the factory to the scene loader
var factory = new DefaultComponentFactory();
factory.RegisterBuiltIns();                       // built-ins first
factory.Register<HealthComponent>("Health");      // your own name

var roots = EntitySerializer.LoadSceneFromFile("scene.xml", entitySystem, factory);
```

Then in XML:

```xml
<Component Type="Health">
    <Properties>
        <Property Name="MaxHealth" Value="150" />
    </Properties>
</Component>
```

### Registration Overloads

| Method | Use when |
|---|---|
| `factory.Register<T>("Name")` | The component has a parameterless constructor. |
| `factory.Register("Name", () => new MyComp(arg))` | The component needs constructor arguments — this is how built-ins like `ColliderComponent` are registered. |

### Fully-Qualified Name Fallback

You can skip registration entirely: if the type name isn't in the factory, `DefaultComponentFactory.Create` falls back to `Type.GetType(typeName)`, so a **fully qualified type name** works directly in XML as long as the assembly is loaded:

```xml
<Component Type="MyGame.Components.HealthComponent" />
```

Registering by short name (recommended) keeps scene files clean and decoupled from your code's namespace layout.

## Error Handling

- **Missing file**: `FileNotFoundException`
- **Invalid XML**: `FormatException`
- **Unknown component type**: Component is silently skipped
- **Invalid property value**: Property is silently skipped

## Complete Example

```csharp
// Scene file: levels/dungeon.xml
string sceneXml = @"
    <Scene>
        <EntityDefinition Type=""PlayerEntity"" Id=""player"">
            <Position X=""100"" Y=""200"" />
            <Tags><Tag Name=""Player"" /></Tags>
            <Components>
                <Component Type=""SpriteComponent"">
                    <Properties>
                        <Property Name=""Scale"" Value=""2,2"" />
                    </Properties>
                </Component>
            </Components>
        </EntityDefinition>

        <EntityDefinition Type=""ChestEntity"" Id=""treasure"">
            <Position X=""500"" Y=""400"" />
            <Tags><Tag Name=""Collectible"" /></Tags>
            <Children>
                <EntityDefinition Type=""KeyItem"" Id=""goldKey"">
                    <Position X=""0"" Y=""-20"" />
                </EntityDefinition>
            </Children>
        </EntityDefinition>
    </Scene>";

var scene = EntitySerializer.LoadSceneFromXml(sceneXml, entitySystem);
foreach (var entity in scene)
{
    Console.WriteLine($"Loaded: {entity.Id} at ({entity.Position.X}, {entity.Position.Y})");
}
```
