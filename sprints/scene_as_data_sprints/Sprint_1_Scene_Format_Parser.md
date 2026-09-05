# Sprint 1 — Scene Format Parser 📜

**Points:** 5 | **Status:** ✅ Done (2026-08-31) | **Goal:** Strict parser for the new self-describing `<Scene>` format.

## Target Schema

```xml
<Scene>
  <GameSystems>
    <System Type="EntitySystem">
      <Prefabs>
        <Prefab Name="Text" Asset="TextTemplate.xml" />
      </Prefabs>
      <Entities>
        <!-- Type= plain class | Source= prefab instance (Type XOR Source) -->
        <EntityDefinition Type="...CharacterEntity" Id="staticCharacter"> ... </EntityDefinition>
        <EntityDefinition Source="Text" Id="infoText" Text="Score: 100"> ... </EntityDefinition>
      </Entities>
    </System>
    <System Type="PhysicsEngine" />
  </GameSystems>
</Scene>
```

## Tasks

- [x] T1 🔒 `SceneDefinition` model (systems, per-system prefab registrations + entity definitions)
- [x] T2 ⭐ Strict parser: root must be `<Scene>` → `<GameSystems>`; unknown elements/attributes = parse error naming the element
- [x] T3 🔒 System resolution: built-in type-name table (`EntitySystem`, `PhysicsEngine`) + reflection fallback for custom systems
- [x] T4 🔒 Flat-attribute override parsing (resolve by component property name across prefab components; ambiguous → error) + precise `<Overrides><Component Type=...>` form
- [x] T5 🔁 Parser tests: full doc, partial sections, ambiguity errors, unknown elements, Type XOR Source violations

## Acceptance Criteria

- Every existing playground scene file expressible in the new format
- Invalid states unwriteable (entities/prefabs only exist inside a `<System>`)
- Build clean, all tests passing

## Notes

- **Files:** `SceneDefinition.cs` (model: `SceneDefinition` → `SystemDefinition` → `PrefabRegistration` / `EntityDefinition`) and `SceneParser.cs` (strict parser), both in `Serialization/`. `EntityPrefabLoader.ResolveEntityType(string)` was added (public) so the parser can validate `Type=` at parse time, mirroring the existing `ResolveComponentType`. The loader file/class were renamed `EntityTemplateLoader` → `EntityPrefabLoader` as part of this sprint's prefab terminology push.
- **Strictness:** every element level has an allow-list; unknown elements or attributes throw `FormatException` naming the offending node. `<Scene>` must contain exactly one `<GameSystems>`; entities/prefabs outside a `<System>` are unparseable by construction.
- **Flat overrides** resolve against the source prefab's components (or the definition's declared `<Components>` for plain-class definitions) by writable property name. Zero matches or multiple matches → parse error pointing at the precise `<Overrides>` form. Flat keys land in `ResolvedOverrides` under the component's FullName; precise `<Overrides>` keys keep their written form — both are consumable as-is by `EntitySystem.Instantiate(name, position, overrides)`.
- **Binds/References:** `<Bind>` is captured both directly on the definition and nested inside `<Components>` (matching the existing scene files). `<Reference Name= TargetId=>` elements are captured for Sprint 2's post-load resolution pass.
- **Prefab assets load eagerly at parse time** (`EntityPrefabLoader.LoadFromAsset`) so flat-override validation can see the prefab's components; failures surface as `FormatException`/asset errors at scene-parse time, not instantiation time.
- **Tests:** 12 new tests in `SceneParserTests.cs`. Full suite: **1032 passed / 0 failed / 3 skipped** (baseline 1020, +12).

---
*Created: 2026-08-31 | Part of Scene-as-Data Project*
