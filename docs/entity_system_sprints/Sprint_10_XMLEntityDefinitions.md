# Sprint 10 — XML Entity Definitions 📄

**Points:** 7  
**Status:** Complete ✅ (T1 ✅, T2 ✅, T3 ✅, T4 ✅)  
**Sprint Goal:** Declarative entity definitions using XML, matching existing asset and GUI layout patterns.

**Dependencies:** Sprint 0 (Tags), Sprint 6 (Components), Sprint 12 (IDs)

---

## Tasks

- [x] **T1: Create `EntitySerializer` class (2 pts)** ⭐ User-facing ✅ COMPLETE
  - `LoadEntity<T>(string xmlData, EntitySystem system)` method
  - `LoadEntityFromFile<T>(string filePath, EntitySystem system)` method
  - `SaveEntity(Entity entity, string filePath)` and `SaveEntityToString(Entity entity)` methods
  - Parse `<EntityType>`, `<Position>`, `<Rotation>`, `<Sort>`, `<Tag>`, `<Active>` elements
  - Reuse `XDocument`/`XElement` parsing from `GuiSerializer`

- [x] **T2: Add component loading (2 pts)** ⭐ User-facing ✅ COMPLETE
  - Parse `<Components>` section with `<Component Type="...">` elements
  - Parse `<Properties><Property Name="..." Value="...">` for simple types
  - Support int, float, string, bool, Vector2, Color, enum via reflection-based `SetProperty()`
  - Component registration via `IComponentFactory` interface + `DefaultComponentFactory` class
  - Factory uses `Register(string, Func<EntityComponent>)` for components without parameterless constructors

- [x] **T3: Add scene loading (1.5 pts)** ⭐ User-facing ✅ COMPLETE
  - `LoadSceneFromFile(string filePath, EntitySystem system)` method
  - `LoadSceneFromXml(string xmlData, EntitySystem system)` method
  - Parse multiple `<EntityDefinition Type="...">` elements with two-pass loading
  - Support `<Children>` for nested parent-child hierarchy (recursive)
  - Support `<References><Reference Name="..." TargetId="..."/>` for entity linking by Id
  - Forward-compatible `SetReference()` via reflection for future entity subclasses

- [x] **T4: Write unit tests (1.5 pts)** 🔁 Validation ✅ COMPLETE
  - 29 total tests passing (16 T1 + 6 T2 component tests + 7 T3 scene tests)
  - Test load single entity from XML with SpriteComponent, RigidbodyComponent
  - Test load entity with Vector2/Color property parsing via reflection
  - Test load scene with multiple entities and children hierarchy
  - Test round-trip save/load preservation
  - Test invalid XML / missing file error handling

---

## Acceptance Criteria

- [x] Entities can be loaded from XML files
- [x] Components are parsed and attached (SpriteComponent, RigidbodyComponent, ColliderComponent)
- [x] Scenes can be loaded from XML with multiple entities
- [x] Parent-child hierarchy is supported in XML via `<Children>` element
- [x] Project builds cleanly — **0 errors** (5 pre-existing warnings in RigidbodyComponent.cs)
- [x] All existing tests pass + new XML serialization tests added (29 tests passing)

---

## Deliverables

| File | Type | Visibility | Notes |
|------|------|------------|-------|
| `Serialization/EntitySerializer.cs` | New | ⭐ PUBLIC | XML serialization |
| `Serialization/IComponentFactory.cs` | New | ⭐ PUBLIC | Component factory interface |
| `EntitySystem.cs` | Modified | ⭐ PUBLIC | Add `LoadEntityFromXml`, `LoadSceneFromXml` |
| `EntitySerializerTests.cs` | New | 🔒 Internal | Unit tests for XML serialization |

---

## Notes & Risks

- **High risk** — complex parsing logic with many edge cases
- XML schema should be versioned for future compatibility
- Error handling for missing components or invalid properties

---

## Implementation Notes

### Completed: T1 + T2 + T3 + T4 (2026-08-09)

**EntitySerializer.cs** (`Serialization/EntitySerializer.cs`):
- Static utility class following `GuiSerializer` pattern
- `LoadEntity<T>()` / `LoadEntityFromFile<T>()` — parse XML into Entity instances
- `SaveEntity()` / `SaveEntityToString()` — serialize entities back to XML
- `LoadSceneFromXml()` / `LoadSceneFromFile()` — load complete scenes with multiple entities
- `CreateEntityByTypeName()` — reflection-based entity creation across all loaded assemblies (including test assemblies)
- `ApplyEntityProperties()` — parse position, rotation, sort, tags, active state
- `LoadComponents()` — attach components from `<Components>` section via factory
- `SetProperty()` — reflection-based property setter (int, float, bool, string, Vector2, Color, enum)
- `ParseColor()` — handles both static fields and properties for MonoGame Color names (Red, Blue, etc.)
- `ResolveReferences()` / `SetReference()` — two-pass reference linking by entity Id

**IComponentFactory + DefaultComponentFactory** (merged into EntitySerializer.cs):
- Factory pattern for component instantiation
- `DefaultComponentFactory` uses dictionary-based registration
- Supports `Register(string, Func<EntityComponent>)` for components w/o parameterless constructors
- Gracefully handles missing/unregistered component types (skips silently)

**Entity.cs** (`EntityOOPSystem/Entity.cs`):
- Added non-generic `AddComponent(EntityComponent)` overload — stores by runtime type (`component.GetType()`)
- Required for factory-created components where compile-time type is `EntityComponent` but runtime type is specific subclass

**EntitySerializerTests.cs** — 29 unit tests all passing:
- Load/save entity from XML string and file (16 tests)
- Position, rotation, sort, tags, active property parsing
- Round-trip serialization/deserialization
- Invalid XML error handling
- Component attachment: SpriteComponent, RigidbodyComponent, multiple components
- Component properties: Vector2 Scale, Color tint via reflection
- Custom component factory usage
- Missing component type graceful degradation
- Scene loading: valid scene, empty scene, children hierarchy, tags + components combined
- File-based scene loading and missing file error handling

### XML Format — Entity

```xml
<Entity Rotation="1.5" Sort="10">
    <Position X="100" Y="200" />
    <Tags>
        <Tag Name="Enemy" />
    </Tags>
    <Components>
        <Component Type="SpriteComponent">
            <Properties>
                <Property Name="Scale" Value="2,2" />
                <Property Name="Color" Value="Red" />
            </Properties>
        </Component>
    </Components>
</Entity>
```

---

*Created: 2026-08-07 | Updated: 2026-08-09 | Part of Entity System Enhancements Project*

---

## Scene XML Example

```xml
<Scene>
    <EntityDefinition Type="PlayerEntity" Id="player">
        <Position X="100" Y="200" />
        <Tags>
            <Tag Name="Player" />
            <Tag Name="Controllable" />
        </Tags>
        <Components>
            <Component Type="SpriteComponent">
                <Properties>
                    <Property Name="Scale" Value="2,2" />
                    <Property Name="Color" Value="Red" />
                </Properties>
            </Component>
            <Component Type="RigidbodyComponent" />
        </Components>
        <Children>
            <EntityDefinition Type="WeaponEntity" Id="weapon">
                <Position X="10" Y="-20" />
            </EntityDefinition>
        </Children>
    </EntityDefinition>
</Scene>
```
