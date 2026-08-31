# Sprint 1 — Scene Format Parser 📜

**Points:** 5 | **Status:** Not Started | **Goal:** Strict parser for the new self-describing `<Scene>` format.

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

- [ ] T1 🔒 `SceneDefinition` model (systems, per-system prefab registrations + entity definitions)
- [ ] T2 ⭐ Strict parser: root must be `<Scene>` → `<GameSystems>`; unknown elements/attributes = parse error naming the element
- [ ] T3 🔒 System resolution: built-in type-name table (`EntitySystem`, `PhysicsEngine`) + reflection fallback for custom systems
- [ ] T4 🔒 Flat-attribute override parsing (resolve by component property name across prefab components; ambiguous → error) + precise `<Overrides><Component Type=...>` form
- [ ] T5 🔁 Parser tests: full doc, partial sections, ambiguity errors, unknown elements, Type XOR Source violations

## Acceptance Criteria

- Every existing playground scene file expressible in the new format
- Invalid states unwriteable (entities/prefabs only exist inside a `<System>`)
- Build clean, all tests passing

---
*Created: 2026-08-31 | Part of Scene-as-Data Project*
