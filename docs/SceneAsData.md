# Scene-as-Data

Scenes are **data**. A scene file is a strict, self-describing XML document that declares which game systems it needs and exactly which entities (and their components) to create — no per-scene C# subclass required. The framework parses the file into a `SceneDefinition`, wraps it in a `DataDrivenScene`, and instantiates everything for you.

This is the only way to load scene content from XML as of 0.20.0 — the legacy flat-`<Scene>` loader was removed (see [Breaking Changes](#breaking-changes-019--020)).

## The One-Line Mental Model

```
XML file ──▶ SceneParser ──▶ SceneDefinition ──▶ DataDrivenScene ──▶ live entities
```

`SceneManager.LoadScene("MyScene.xml")` does all of that for you.

## Anatomy of a Scene File

The root is `<Scene>`, which contains exactly one `<GameSystems>` element. Every prefab registration and entity definition lives inside a `<System>`. Unknown elements or attributes are **parse errors** that name the offending element, so typos fail fast instead of being silently ignored.

```xml
<Scene>
    <GameSystems>
        <System Type="EntitySystem">
            <!-- Optional: register prefabs this scene can instantiate by Source -->
            <Prefabs>
                <Prefab Name="Text" Asset="TextTemplate.xml" />
            </Prefabs>

            <!-- One or more entity definitions (roots) -->
            <Entities>
                <EntityDefinition Type="CoreEssentials.Playground.TextEntity" Id="title">
                    <Position X="100" Y="200" />
                    <Text>Hello from data</Text>
                </EntityDefinition>

                <!-- Same thing, but built from a registered prefab with overrides -->
                <EntityDefinition Source="Text" Id="subtitle">
                    <Position X="100" Y="240" />
                    <Text>From a prefab</Text>
                </EntityDefinition>
            </Entities>
        </System>
    </GameSystems>
</Scene>
```

### `<System>`

Declares a game system the scene needs. `Type` is resolved from a built-in table first (`EntitySystem`, `PhysicsEngine`), then by reflection for any custom `GameSystem` subclass in a loaded assembly. An optional `Config` attribute points at a configuration asset (e.g. a physics config).

| Attribute | Required | Description |
|-----------|----------|-------------|
| `Type` | **Yes** | System type name (built-in short name, or a custom `GameSystem` class name) |
| `Config` | No | Configuration asset name loaded via the system's single-argument constructor |

A `<System>` may contain a `<Prefabs>` element and/or an `<Entities>` element — nothing else.

### `<Prefab>` (registration)

Registers a prefab file for use as a `Source` target within this system. Both attributes are required, and names must be unique within the system. The referenced asset is loaded eagerly at parse time.

```xml
<Prefabs>
    <Prefab Name="Text" Asset="TextTemplate.xml" />
</Prefabs>
```

## Entity Definitions: `Type=` vs `Source=`

Every `<EntityDefinition>` sets **exactly one** of two attributes:

- **`Type="..."`** — build the entity from a concrete class, with any components you declare inline in `<Components>`. Use this for one-off entities that don't come from a shared prefab.
- **`Source="..."`** — instantiate a registered prefab (by its `<Prefab Name>`), then apply per-instance overrides on top.

Setting both is an error; setting neither is an error.

| Attribute | Type | Required | Description |
|-----------|------|----------|-------------|
| `Type` | string | One of Type/Source | Entity class name (short or fully-qualified) |
| `Source` | string | One of Type/Source | Registered prefab name to instantiate from |
| `Id` | string | No | Unique identifier, used by `<Reference>` resolution. Must be unique across the whole scene. |
| `Rotation` | float | No | Initial rotation (radians) |
| `Sort` | int | No | Render sort order |
| `Active` | bool | No | Active by default |

### Child Hierarchy

Nest `<EntityDefinition>` elements inside a `<Children>` element to build a parent/child tree. A child's `<Position>` is an **offset from its parent**, not a world position. Components attach pre-order (parents before children), so hierarchy-dependent components — e.g. a child `LabelComponent` finding its ancestor `CanvasComponent` — resolve correctly.

```xml
<EntityDefinition Type="GameObjectEntity" Id="hud">
    <Position X="0" Y="0" />
    <Components>
        <Component Type="CanvasComponent" />
    </Components>
    <Children>
        <EntityDefinition Type="CoreEssentials.Playground.TextEntity" Id="score">
            <Position X="16" Y="16" />
            <Text>Score: 0</Text>
        </EntityDefinition>
    </Children>
</EntityDefinition>
```

## Flat and Precise Overrides

There are three ways to set a component's property from an entity definition, in increasing order of explicitness:

### 1. Flat attributes (the shorthand)

Any attribute on `<EntityDefinition>` that is *not* one of the known attributes (`Type`, `Source`, `Id`, `Rotation`, `Sort`, `Active`) is treated as a **flat override** — it must resolve to exactly one writable component property with that name, or parsing fails.

```xml
<EntityDefinition Type="CoreEssentials.Playground.TextEntity" Id="title">
    <Text>Hello</Text>          <!-- resolves to LabelComponent/TextEntity.Text -->
    <TextColor>Gold</TextColor> <!-- resolves to the single component exposing TextColor -->
</EntityDefinition>
```

Flat overrides are validated at parse time: a name matching **zero** components, or **more than one**, is an error — use `<Overrides>` to disambiguate.

### 2. Precise `<Overrides>` (target a specific component)

When several components expose the same property name, target one explicitly by type:

```xml
<EntityDefinition Source="Button">
    <Overrides>
        <Component Type="ButtonComponent">
            <Property Name="Label" Value="Start Game" />
        </Component>
    </Overrides>
</EntityDefinition>
```

### 3. `<EntityOverrides>` (target the entity itself)

Some entities keep state directly on themselves with no component to target — e.g. `TextEntity.Text`, or an entity's own `CameraSpeed`/`Scale`. Target those via `<EntityOverrides>`, which applies property → value pairs to the **entity** before `OnStart`/`OnAttach`:

```xml
<EntityDefinition Type="CoreEssentials.Playground.CameraEntity" Id="cam">
    <EntityOverrides>
        <Property Name="CameraSpeed" Value="300" />
    </EntityOverrides>
</EntityDefinition>
```

All three forms parse values with the same rules as XML properties: `int`, `float`, `bool`, `string` (verbatim), `Vector2` (`x,y`), `Color` (named or `R,G,B[,A]`), and enums (case-insensitive).

## Binds and References

### `<Bind>` — declarative command wiring

Wire a public event to a handler method directly in the scene file. See [Declarative Command Binding](./XMLEntityDefinitions.md#declarative-command-binding) for the full forms (`Command` vs `Target`+`Member`, `Source`, resolution order, fail-safe behavior). Binds may be direct children of an `<EntityDefinition>` or nested inside its `<Components>`.

### `<References>` — link entities by Id

Resolve cross-entity links once every entity exists. A reference is first applied to the entity itself; if the entity has no matching settable member, it falls back to the **first component** exposing a settable property or public field of an assignable `Entity` type.

```xml
<EntityDefinition Type="GameObjectEntity" Id="keeper">
    <Components>
        <Component Type="ScoreKeeperComponent" />
    </Components>
    <References>
        <!-- ScoreLabel is a public Entity? member on ScoreKeeperComponent -->
        <Reference Name="ScoreLabel" TargetId="scoreText" />
    </References>
</EntityDefinition>
```

## DataDrivenScene & Loading

`DataDrivenScene` is the concrete `Scene` subclass that materializes a `SceneDefinition`. You almost never construct it directly — the string overloads on `SceneManager` do:

```csharp
// Load a data-driven scene from an XML asset (wraps it in a DataDrivenScene + transitions).
public void LoadScene(string sceneAssetName)

// Set a data-driven loading screen parsed from a scene XML asset.
public void SetLoadingScene(string sceneAssetName)
```

Both parse the file with `SceneParser`, wrap it in a `DataDrivenScene`, and hand it to the existing transition machinery. The same file can serve as either a regular scene or the loading screen.

The scene's `OnStartCoroutine` registers each system's prefabs, then instantiates its entities (building nested `<Children>` subtrees), then resolves references — reporting progress through the 50%→100% phase of the transition.

## The Data-Driven Loading Screen

A loading screen is just another scene file. Use a `TransitionProgressComponent` on a label to mirror `SceneManager.TransitionProgress` each frame and keep the label in sync as a live percentage (0% → 100%):

```xml
<Scene>
    <GameSystems>
        <System Type="EntitySystem">
            <Entities>
                <EntityDefinition Type="CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.GameObjectEntity" Id="loadingLabel">
                    <Position X="0" Y="0" />
                    <Components>
                        <Component Type="CanvasComponent" />
                        <Component Type="LabelComponent">
                            <Properties>
                                <Property Name="Text" Value="Loading..." />
                                <Property Name="TextColor" Value="White" />
                                <Property Name="HorizontalAlignment" Value="Center" />
                                <Property Name="VerticalAlignment" Value="Center" />
                            </Properties>
                        </Component>
                        <Component Type="TransitionProgressComponent" />
                    </Components>
                </EntityDefinition>
            </Entities>
        </System>
    </GameSystems>
</Scene>
```

Then boot purely from files in your `Program.cs` — no C# loading scene or scene subclass:

```csharp
game.SceneManager.SetLoadingScene("loading.xml");
game.SceneManager.LoadScene("HomeScene.xml");
```

## Complete Example

A small menu scene with a shared HUD prefab, a bound button, and an entity-level override:

```xml
<Scene>
    <GameSystems>
        <System Type="EntitySystem">
            <Prefabs>
                <Prefab Name="Button" Asset="ButtonTemplate.xml" />
            </Prefabs>
            <Entities>
                <EntityDefinition Type="CoreEssentials.Playground.CameraEntity" Id="cam">
                    <Position X="640" Y="360" />
                    <EntityOverrides>
                        <Property Name="CameraSpeed" Value="300" />
                    </EntityOverrides>
                </EntityDefinition>

                <EntityDefinition Source="Button" Id="startButton">
                    <Position X="560" Y="400" />
                    <Overrides>
                        <Component Type="ButtonComponent">
                            <Property Name="Label" Value="Start Game" />
                        </Component>
                    </Overrides>
                    <!-- Clicked -> StartGame() resolved on the entity/components -->
                    <Bind Event="Clicked" Command="StartGame" />
                </EntityDefinition>
            </Entities>
        </System>
    </GameSystems>
</Scene>
```

```csharp
// That's it — no scene subclass.
game.SceneManager.LoadScene("MainMenu.xml");
```

## Breaking Changes (0.19 → 0.20)

This release removes the legacy scene-load path and renames the prefab terminology. If you are upgrading from 0.19.x:

### 1. The flat `<Scene>` loader is gone

`EntitySerializer.LoadSceneFromXml`, `EntitySerializer.LoadSceneFromFile`, and `Scene.LoadEntitiesFromXml` have been **removed**. The old format — a `<Scene>` root with bare, flat `<EntityDefinition>` / `<Template Source=>` children directly under it (no `<GameSystems>`/`<System>` wrapper) — no longer parses.

**Fix:** migrate each scene file to the strict format and load it through `SceneManager.LoadScene(string)` / `SetLoadingScene(string)`. Wrap your entities in `<GameSystems><System Type="EntitySystem">`, move prefab registrations into a `<Prefabs>` element, and replace `<Template Source=>` with `<EntityDefinition Source=>`:

```xml
<!-- 0.19 (old — no longer supported) -->
<Scene>
    <EntityDefinition Type="PlayerEntity" Id="player"> ... </EntityDefinition>
    <Template Source="ButtonTemplate.xml" X="100" Y="200" Id="btn" />
</Scene>

<!-- 0.20 (strict) -->
<Scene>
    <GameSystems>
        <System Type="EntitySystem">
            <Prefabs>
                <Prefab Name="Button" Asset="ButtonTemplate.xml" />
            </Prefabs>
            <Entities>
                <EntityDefinition Type="PlayerEntity" Id="player"> ... </EntityDefinition>
                <EntityDefinition Source="Button" Id="btn">
                    <Position X="100" Y="200" />
                </EntityDefinition>
            </Entities>
        </System>
    </GameSystems>
</Scene>
```

### 2. The `<EntityTemplate>` root was renamed to `<Prefab>`

Prefab files now use a `<Prefab>` root element. `EntityPrefabLoader` expects `<Prefab>`; the old `<EntityTemplate>` root is not parsed by it.

**Fix:** rename the root element of each prefab file from `<EntityTemplate>` to `<Prefab>` (and nested `<EntityTemplate>` children to `<Prefab>`).

### 3. "Template" API names were renamed to "Prefab"

| Old (0.19.x) | New (0.20.0) |
|--------------|--------------|
| `RegisterTemplate(name, asset)` | `RegisterPrefab(name, asset)` |
| `RegisterTemplate(name, EntityTemplate)` | `RegisterPrefab(name, Prefab)` |
| `InstantiateTemplate(name, pos)` (`Entity`/`EntityComponent`) | `InstantiatePrefab(name, pos)` |
| `EntityTemplate` type | `Prefab` type |
| `EntityTemplateLoader` | `EntityPrefabLoader` |

The old names are kept as `[Obsolete]` shims for this release so existing code keeps compiling — they forward to the prefab API and will be removed in a future release. Update your call sites to silence the warnings.

### What did **not** change

- Single-entity serialization: `EntitySerializer.LoadEntity<T>`, `LoadEntityFromFile<T>`, `SaveEntity`, `SaveEntityToString` are untouched.
- Game-state save/load (`EntitySystem.SaveState` / `LoadState`) is a separate concern and is unaffected.
- Component discovery (custom components referenced by simple name) works exactly as before.
