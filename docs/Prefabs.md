# Prefabs

A **prefab** is a reusable blueprint for an entity. Define it once in XML (or build it in code) and instantiate it as many times as you need — each instance gets its own components, state, and optional per-instantiation overrides. This replaces the older "entity template" terminology; `Prefab` is now the canonical name across the API and the file format.

## Overview

Prefabs give you:

- **Reusable entity definitions** — one file, many instances
- **XML-based configuration** — components, tags, sort, rotation, active state
- **Fast instantiation** — parsed once, cloned per spawn
- **Hierarchical prefabs** — nested `<Prefab>` children build a whole subtree
- **Declarative command binding** — `<Bind>` wiring baked into the prefab
- **Per-instantiation overrides** — vary text/color/scale/etc. per spawn without a wrapper factory

## API Reference

### RegisterPrefab(string name, string assetName)

Registers a prefab from an XML asset (a file whose root element is `<Prefab>`). Re-registering the same name replaces the previous definition and logs a warning — registration is idempotent by design.

```csharp
public void RegisterPrefab(string name, string assetName)
```

**Parameters:**
- `name` — The unique name to instantiate the prefab by (case-insensitive lookup).
- `assetName` — The XML asset name in the `AssetManager` (e.g., `"templates/enemy_goblin.xml"`).

**Example:**
```csharp
entitySystem.RegisterPrefab("EnemyGoblin", "templates/enemy_goblin.xml");
entitySystem.RegisterPrefab("Player", "templates/player.xml");
```

### RegisterPrefab(string name, Prefab prefab)

Registers an already-constructed prefab under the given name — useful when the prefab was parsed from a raw XML string (`EntityPrefabLoader.LoadFromXml`) or built in code.

```csharp
public void RegisterPrefab(string name, Prefab prefab)
```

**Example:**
```csharp
var prefab = EntityPrefabLoader.LoadFromXml(xmlString);
entitySystem.RegisterPrefab("Popup", prefab);
```

> `RegisterTemplate(...)` still exists but is marked `[Obsolete]` and simply forwards to `RegisterPrefab`. Use `RegisterPrefab` in new code.

### HasPrefab(string name) / TryGetPrefab(string name, out Prefab?)

Check registration without instantiating. `TryGetPrefab` returns the **live** registered instance — treat it as read-only (clone before mutating).

```csharp
public bool HasPrefab(string name)
public bool TryGetPrefab(string name, out Prefab? prefab)
```

### InstantiateFromAsset(string assetName, Vector2 position[, overrides][, entityOverrides])

Instantiates a prefab straight from a Content XML asset with **zero registration calls**. On first use the asset is loaded and cached under its base name (the file name without extension), so subsequent instantiations reuse the parsed prefab. An explicit `RegisterPrefab` call for the same name always wins over the lazy cache.

```csharp
public Entity InstantiateFromAsset(string assetName, Vector2 position)
public Entity InstantiateFromAsset(string assetName, Vector2 position,
    IReadOnlyDictionary<string, Dictionary<string, string>>? overrides)
public Entity InstantiateFromAsset(string assetName, Vector2 position,
    IReadOnlyDictionary<string, Dictionary<string, string>>? overrides,
    IReadOnlyDictionary<string, string>? entityOverrides)
```

### Instantiate(string prefabName, Vector2 position[, overrides][, entityOverrides])

Instantiates an entity from a **registered** prefab at the specified position. The registered prefab is never mutated — overrides are merged into a clone before any component is attached or started, so components and the entity see the final values in `OnStart`/`OnAttach`.

```csharp
public Entity Instantiate(string prefabName, Vector2 position)
public Entity Instantiate(string prefabName, Vector2 position,
    IReadOnlyDictionary<string, Dictionary<string, string>>? componentOverrides = null,
    IReadOnlyDictionary<string, string>? entityOverrides = null);
```

**Example:**
```csharp
var enemy = entitySystem.Instantiate("EnemyGoblin", new Vector2(100, 200));
```

### Per-Instantiation Overrides

A single prefab can spawn many instances that differ per spawn — a floating score popup with different text/color/scale, a button with different labels — without writing a wrapper factory or one prefab per variant. `Instantiate` (and `InstantiateFromAsset`) accept two optional override maps:

- **`componentOverrides`** — `component type name → property name → value string`. A key matching an existing prefab component merges into it; a key matching none adds a new component. Component keys may be short names or fully-qualified type names.
- **`entityOverrides`** — `property name → value string`, applied to the entity itself (not a component). Use this when state lives directly on the entity, e.g. an entity's own `Text`, `Color`, `CameraSpeed`, or `Scale`.

Both are merged into a **clone** of the prefab before any component is attached or started. Values are parsed with the same rules as XML properties (`int`, `float`, `bool`, `string`, `Vector2`, `Color`, and enums).

```csharp
// A "popup" prefab whose text, color and lifetime vary per spawn:
var popup = entitySystem.Instantiate("popup", position,
    componentOverrides: new Dictionary<string, Dictionary<string, string>>
    {
        ["FloatingPopUpComponent"] = new() { ["Scale"] = "1.5", ["Lifetime"] = "0.8" }
    },
    entityOverrides: new Dictionary<string, string>
    {
        ["Text"] = "×2",
        ["Color"] = "Gold"
    });
```

**Scene XML equivalent.** In a data-driven scene file, the same capability is expressed declaratively on an `<EntityDefinition>`: flat attributes and `<Overrides>` target *component* properties, while an `<EntityOverrides>` element targets the entity itself. See [Scene-as-Data](./SceneAsData.md#flat-and-precise-overrides).

### Prefab-Style Convenience Methods

Entities and components can spawn a registered prefab directly — Unity-style prefab instantiation — without reaching for the system:

```csharp
// On Entity (spawns in this entity's system):
Entity popup = InstantiatePrefab("popup", position);

// On EntityComponent (spawns in the owning entity's system):
Entity popup = InstantiatePrefab("popup", position);
```

Both return `null` when the caller is not attached to a system. They pair with `CreateGameObject<T>()` and `Destroy()` / `DestroyOwner()` — see [SendMessage](./SendMessage.md#unity-style-entity-management-one-liners) for the full set of one-liners.

> `InstantiateTemplate(...)` still exists but is marked `[Obsolete]` and forwards to `InstantiatePrefab`. Use `InstantiatePrefab` in new code.

## Prefab XML Schema

The root element is `<Prefab>` (the old `<EntityTemplate>` root was renamed). A prefab file contains exactly one `<Prefab>` element.

### Basic Prefab

```xml
<Prefab Type="EnemyEntity" Rotation="0" Sort="0" Active="true">
    <Tags>
        <Tag Name="Enemy" />
        <Tag Name="GroundUnit" />
    </Tags>
    <Components>
        <Component Type="SpriteComponent">
            <Properties>
                <Property Name="Color" Value="255,255,255,255" />
                <Property Name="LayerDepth" Value="0.5" />
            </Properties>
        </Component>
    </Components>
</Prefab>
```

### Attributes

| Attribute | Type | Required | Description |
|-----------|------|----------|-------------|
| `Type` | string | **Yes** | Entity class name (short or fully-qualified) |
| `Rotation` | float | No | Initial rotation (radians), default `0` |
| `Sort` | int | No | Render sort order, default `0` |
| `Active` | bool | No | Active by default, default `true` |

### Tags Element

```xml
<Tags>
    <Tag Name="Enemy" />
    <Tag Name="GroundUnit" />
</Tags>
```

### Components Element

```xml
<Components>
    <Component Type="SpriteComponent">
        <Properties>
            <Property Name="Color" Value="255,255,255,255" />
            <Property Name="LayerDepth" Value="0.5" />
        </Properties>
    </Component>
</Components>
```

> **Note:** The `Sprite` itself is a complex asset and is assigned in code (e.g. `new SpriteComponent(AssetManager.LoadAsset<Sprite>("goblin_sprite.xml"))`), not via a string XML property. Prefabs set the simple, settable properties such as `Color`, `Origin`, `Effects`, and `LayerDepth`.

> **Note on `RigidbodyComponent`:** The rigidbody *type* (`Static`, `Dynamic`, `Kinematic`) is chosen via the constructor — `new RigidbodyComponent(RigidbodyType.Dynamic)` — and is **not** a settable XML property. `RigidbodyComponent` has no parameterless constructor, so a prefab cannot create it on its own: add it in your entity's `OnStart()` first, and the prefab will then apply the settable properties (`Mass`, `FixedRotation`, `SyncFromPhysics`) via reflection.

### Children Element

Nested prefabs build a whole subtree. A child's `<Position>` is an **offset from its parent**, not a world position.

```xml
<Prefab Type="Spaceship">
    <Children>
        <Prefab Type="Engine">
            <Position X="-50" Y="0" />
        </Prefab>
        <Prefab Type="Engine">
            <Position X="50" Y="0" />
        </Prefab>
    </Children>
</Prefab>
```

### Bind Element

Prefabs support the same declarative `<Bind>` event-to-command wiring as scene entity definitions. Binds are parsed from the prefab and applied to **every** entity instantiated from it (recursively, for child prefabs), so a prefab can be fully data-driven:

```xml
<Prefab Type="ScoreButtonEntity">
    <Components>
        <Component Type="ButtonComponent">
            <Properties><Property Name="Label" Value="+10" /></Properties>
        </Component>
    </Components>
    <!-- Clicked on ButtonComponent -> ScoreKeeperComponent.AddTen() -->
    <Bind Event="Clicked" Command="AddTen" />
</Prefab>
```

Each instantiation gets its own wiring — re-instantiating the same prefab never shares or mutates state between instances. See [Declarative Command Binding](./XMLEntityDefinitions.md#declarative-command-binding) for the bind forms and resolution rules.

## Usage Examples

### Basic Prefab Usage

```csharp
// Register prefab
entitySystem.RegisterPrefab("EnemyGoblin", "templates/enemy_goblin.xml");

// Instantiate multiple enemies
for (int i = 0; i < 10; i++)
{
    var position = new Vector2(
        Random.Range(0, 800),
        Random.Range(0, 600)
    );

    var enemy = entitySystem.Instantiate("EnemyGoblin", position);
}
```

### Enemy Waves

```csharp
public class WaveSpawner
{
    public void SpawnWave(int count, string prefabName)
    {
        for (int i = 0; i < count; i++)
        {
            var position = GetSpawnPosition();
            var enemy = entitySystem.Instantiate(prefabName, position);

            // Customize instance
            enemy.SetTag($"Wave_{currentWave}");
        }
    }
}
```

### Prefab with Overrides

```csharp
public class PrefabManager
{
    public Entity CreateCustomEnemy(string prefabName, Vector2 position,
        string[] additionalTags, float scale)
    {
        var enemy = entitySystem.Instantiate(prefabName, position);

        // Add additional tags
        foreach (var tag in additionalTags)
            enemy.SetTag(tag);

        // Modify the entity's scale (SpriteComponent has no Scale property)
        enemy.Scale = new Vector2(scale, scale);

        return enemy;
    }
}
```

### Hierarchical Prefabs

```xml
<Prefab Type="Spaceship" Rotation="0" Sort="0">
    <Tags>
        <Tag Name="Vehicle" />
        <Tag Name="Player" />
    </Tags>
    <Components>
        <Component Type="SpriteComponent">
            <Properties>
                <Property Name="Color" Value="255,255,255,255" />
                <Property Name="LayerDepth" Value="1.0" />
            </Properties>
        </Component>
    </Components>
    <Children>
        <Prefab Type="Engine">
            <Position X="-50" Y="0" />
            <Components>
                <Component Type="SpriteComponent">
                    <Properties>
                        <Property Name="Color" Value="128,128,128,255" />
                    </Properties>
                </Component>
            </Components>
        </Prefab>
        <Prefab Type="Engine">
            <Position X="50" Y="0" />
            <Components>
                <Component Type="SpriteComponent">
                    <Properties>
                        <Property Name="Color" Value="128,128,128,255" />
                    </Properties>
                </Component>
            </Components>
        </Prefab>
    </Children>
</Prefab>
```

```csharp
// Instantiate spaceship with engines
var ship = entitySystem.Instantiate("Spaceship", new Vector2(400, 300));
// Engines automatically created as children
```

## Prefab Loading

### From Asset

```csharp
entitySystem.RegisterPrefab("Enemy", "templates/enemy.xml");
```

### From XML String

```csharp
var prefab = EntityPrefabLoader.LoadFromXml(xml);
entitySystem.RegisterPrefab("Enemy", prefab);
```

`EntityPrefabLoader.Instantiate(prefab, system, position)` is the lower-level call that `Instantiate` and `InstantiateFromAsset` both funnel through — it builds the entity tree, links children, and attaches components pre-order (parents before children).

## Migration from Entity Templates

If you are upgrading from a version that used the "entity template" name:

| Old (0.19.x) | New (0.20.0) |
|--------------|--------------|
| `<EntityTemplate>` XML root | `<Prefab>` XML root |
| `RegisterTemplate(name, asset)` | `RegisterPrefab(name, asset)` |
| `RegisterTemplate(name, EntityTemplate)` | `RegisterPrefab(name, Prefab)` |
| `InstantiateTemplate(name, pos)` (on `Entity`/`EntityComponent`) | `InstantiatePrefab(name, pos)` |
| `EntityTemplate` type | `Prefab` type |
| `EntityTemplateLoader` | `EntityPrefabLoader` |

The old names (`RegisterTemplate`, `InstantiateTemplate`, the `EntityTemplate` class) are kept as `[Obsolete]` shims for one release so existing code keeps compiling — but they forward to the prefab API and will be removed in a future release. See [Scene-as-Data → Breaking Changes](./SceneAsData.md#breaking-changes-019--020) for the full 0.19 → 0.20 migration note.
