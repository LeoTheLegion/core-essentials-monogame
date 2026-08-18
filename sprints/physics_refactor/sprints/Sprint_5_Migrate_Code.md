# Sprint 5 — Migrate Existing Physics Code 🔄

**Points:** 3  
**Status:** ✅ Completed  
**Sprint Goal:** Consolidate all physics code from CoreEssentials.Physics into CoreEssentials and update Playground/tests to use new abstraction layer.

---

## Architecture Change ⚠️

**Original Plan:** CoreEssentials references CoreEssentials.Physics via project reference.  
**Actual Approach:** All physics code consolidated directly into `CoreEssentials/src/gameSystems/physics/` due to .NET SDK circular dependency blocker (MSB4006).

### Why the change?
- CoreEssentials already referenced Aether Physics2D.MG
- CoreEssentials.Physics also referenced the same package
- Creating a bidirectional reference caused circular dependency in the .NET SDK build graph
- Solution: Move all physics code into single `CoreEssentials` project with proper namespace structure

### Namespace Structure (Actual)
```
CoreEssentials/src/gameSystems/physics/
├── types/              # Interfaces: IPhysicsBody, IFixture, IShape, etc.
├── engines/aether/     # Implementations: PhysicsEngine, PhysicsBody, Fixture, etc.
└── factory/            # Factory classes (if needed)
```

---

## Tasks Completed ✅

- [x] **T1: Identify and resolve circular dependency (0.5 pt)**
  - Discovered .NET SDK MSB4006 error when attempting bidirectional project references
  - Decision: Consolidate all physics code into CoreEssentials directly
  
- [x] **T2: Migrate types/interfaces to CoreEssentials (1 pt)**
  - Copied all interface files from `CoreEssentials.Physics/types/` → `CoreEssentials/src/gameSystems/physics/types/`
  - Updated namespaces: `CoreEssentials.Physics.*` → `CoreEssentials.GameSystems.Physics.*`
  - Files migrated: IPhysicsBody, IFixture, IShape, IPHysicsWorld, etc.

- [x] **T3: Migrate implementations to CoreEssentials (1 pt)**
  - Copied all implementation files from `CoreEssentials.Physics/engines/aether/` → `CoreEssentials/src/gameSystems/physics/engines/aether/`
  - Updated namespaces across 21 physics source files via PowerShell regex replacement
  - Added missing interface implementations:
    - `Fixture.cs`: Added `Friction`, `Restitution` properties (were in interface but not implemented)
    - `PhysicsBody.cs`: Made `Mass` settable, added IPhysicsWorld members (`AddBody`, `RemoveBody`, `ClearAllBodies`, `Step`)

- [x] **T4: Create/update PhysicsDebugRenderer (0.5 pt)**
  - Deleted old raw Aether version from root physics folder
  - Created new version in `engines/aether/PhysicsDebugRenderer.cs` using abstraction layer
  - Uses `IWorld.Bodies`, `IFixture.Shape`, `IShape.GetShapeType()` instead of concrete Aether types
  - Made class extend `GameSystem` for scene registration

- [x] **T5: Delete old physics code (0.5 pt)**
  - Removed old `PhysicsEngine.cs`, `PhysicsConfig.cs`, `WorldPool.cs` from root physics folder
  - Deleted duplicate test files

---

## Acceptance Criteria ✅

- [x] CoreEssentials project builds with zero Aether type leaks in public API surface
- [x] `PhysicsEngine` implements `IPhysicsWorld` interface (internal use only)
- [x] `PhysicsDebugRenderer` uses `IShape.GetShapeType()` instead of Aether shape types for rendering
- [x] Existing test suite compiles and passes: **337 tests** (335 passed, 2 skipped)
- [x] Single CoreEssentials project builds cleanly — no cross-project references needed

---

## Playground Migration 🎮

Updated `CoreEssentials.Playground` to use new abstraction layer API:

| File | Changes |
|------|---------|
| `Ball.cs` | Changed `Body` → `IPhysicsBody`, `Fixture` → `IFixture`; updated method calls (`ApplyLinearImpulse` → `ApplyImpulse`) |
| `WorldBorder.cs` | Changed `Body[]` → `IPhysicsBody[]`; updated to use `CreateStatic()` and new Rectangle API |
| `PhysicsEntityScene.cs` | Added using directive for `CoreEssentials.GameSystems.Physics.Engines.Aether;`; updated debug renderer constructor |

---

## Key API Changes

### IPhysicsBody.Mass — Now Settable
```csharp
// Old: read-only getter
float Mass { get; }

// New: settable property
float Mass { get; set; }  // Setting to 0 makes body static
```

### IFixture — Added Material Properties
```csharp
public interface IFixture : IDisposable
{
    float Friction { get; set; }     // NEW: 0 = slippery, 1 = sticky
    float Restitution { get; set; }  // NEW: 0 = no bounce, 1 = full bounce
}
```

### PhysicsEngine — Implements IPhysicsWorld (Internal)
```csharp
public class PhysicsEngine : GameSystem, IFixedUpdateGameSystem, IPhysicsWorld, IDisposable
{
    IReadOnlyList<IPhysicsBody> IPhysicsWorld.Bodies { get; }  // Explicit interface
}
```

---

## Deliverables 📦

| File | Status | Change |
|------|--------|--------|
| `CoreEssentials/src/gameSystems/physics/types/*.cs` | ✅ Migrated | Interfaces from CoreEssentials.Physics, updated namespaces |
| `CoreEssentials/src/gameSystems/physics/engines/aether/*.cs` | ✅ Migrated | Implementations migrated and fixed |
| `CoreEssentials.Playground/Ball.cs` | ✅ Updated | Uses IPhysicsBody/IFixture abstractions |
| `CoreEssentials.Playground/WorldBorder.cs` | ✅ Updated | Uses IPhysicsBody[] instead of raw Body[] |
| `CoreEssentials.Playground/PhysicsEntityScene.cs` | ✅ Updated | Correct using directives and constructor calls |

---

## Lessons Learned 📚

1. **.NET SDK Circular Dependencies:** Bidirectional project references cause MSB4006 errors that are hard to diagnose. Consider consolidation when dependencies conflict.
2. **Interface Completeness:** Always verify implementations match interface contracts before updating consumers — missing `Friction`/`Restitution` on Fixture caused cascading errors.
3. **Namespace Strategy:** Using `CoreEssentials.GameSystems.Physics.*` (rather than just `Physics.*`) prevents conflicts with other systems and makes the codebase more navigable.
4. **Playground Migration Pattern:** When changing public APIs, update all Playground consumers in one pass to avoid partial states that are hard to debug.

---

*Created: 2026-07-16 | Completed: 2026-07-17 | Part of Physics System Refactoring Project*
