# Sprint 5 — Migrate Existing Physics Code 🔄

**Points:** 3  
**Status:** Not Started (depends on Sprint 4)  
**Sprint Goal:** Update existing physics code in CoreEssentials to use new type abstractions from CoreEssentials.Physics, removing direct Aether dependencies.

---

## Tasks

- [ ] **T1: Add project reference (0.5 pt)**
  - Add `<ProjectReference>` from `CoreEssentials.csproj` → `CoreEssentials.Physics.csproj`
  - Verify both projects build together

- [ ] **T2: Migrate `PhysicsEngine.cs` in CoreEssentials (1.5 pts)**
  - The old `CoreEssentials/src/gameSystems/physics/PhysicsEngine.cs` directly exposes Aether.World types
  - Options:
    - **A:** Replace entirely with a thin wrapper that delegates to the new `CoreEssentials.Physics.engines.aether.PhysicsEngine`
    - **B:** Refactor in-place to use `IPhysicsBody` / `IFixture` instead of raw Aether types
  - Remove any public API surface that leaks Aether types (`World`, `Body`, `FixtureList`, etc.)

- [ ] **T3: Migrate `PhysicsDebugRenderer.cs` (0.5 pt)**
  - Currently iterates over Aether fixtures/shapes directly via `fixture.Shape`
  - Update to iterate using `IFixture`/`IShape` interfaces instead
  - Render logic should work with our shape abstractions (`CircleShape`, `PolygonShape`, etc.)

- [ ] **T4: Handle `WorldPool.cs` migration (0.5 pt)**
  - Old `WorldPool.cs` is superseded by new pooling in Sprint 4
  - Either remove entirely or keep as a thin compatibility shim that delegates to the new pool

---

## Acceptance Criteria

- [ ] CoreEssentials project builds with zero Aether type leaks in public API surface
- [ ] `PhysicsEngine.cs` no longer exposes `Aether.World` publicly
- [ ] `PhysicsDebugRenderer.cs` uses `IShape` instead of Aether shape types for rendering
- [ ] Existing test suite still compiles (may need minor updates)
- [ ] Both CoreEssentials and CoreEssentials.Physics build cleanly together

---

## Deliverables

| File | Change |
|------|--------|
| `CoreEssentials/CoreEssentials.csproj` | Added project reference to CoreEssentials.Physics |
| `CoreEssentials/src/gameSystems/physics/PhysicsEngine.cs` | Migrated to use new abstractions |
| `CoreEssentials/src/gameSystems/physics/PhysicsDebugRenderer.cs` | Uses IShape interfaces for rendering |
| `CoreEssentials/src/gameSystems/physics/WorldPool.cs` | Removed or deprecated (replaced by new pool) |

---

## Notes & Risks

- **Highest risk sprint:** This is where things can break. Existing playground code and tests may depend on old API surface.
- Strategy: Keep the old `PhysicsEngine.cs` as a compatibility layer if the migration is too invasive — mark it `[Obsolete]` with a message pointing to new API.
- Run full test suite after each file migration to catch regressions early.

---

*Created: 2026-07-16 | Part of Physics System Refactoring Project*
