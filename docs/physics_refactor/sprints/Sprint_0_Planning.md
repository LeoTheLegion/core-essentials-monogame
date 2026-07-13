# Sprint 0: Planning & Interface Definition

**Points:** 2  
**Status:** ✅ **Completed**  
**Date Completed:** July 13, 2026  
**Description:** Plan the adapter pattern implementation and define all interface contracts. No Aether references in interfaces.

---

## Sprint Overview

This sprint establishes the foundation for the entire physics refactoring project by:
1. **Creating the `CoreEssentials.Physics` NuGet package project** - The new standalone project that will house all physics code
2. **Defining clean, high-level interfaces** that abstract away all `nkast.Aether.Physics2D` dependencies

These interfaces will be the user-facing API throughout the library lifecycle, making it easy to swap physics engines in the future without breaking user code.

**Critical:** The project must be buildable after Sprint 0 before moving forward with adapter implementations.

---

## Tasks

- [ ] **Create CoreEssentials.Physics project** ✅
  - Project file: `CoreEssentials.Physics/CoreEssentials.Physics.csproj`
  - Package ID: `CoreEssentials.Physics`, Version: `0.15.0`
  - References: Aether.Physics2D.MG, MonoGame.Framework.DesktopGL, CoreEssentials

- [ ] **Analyze existing codebase** ✅
  - Reviewed PhysicsEngine.cs, WorldPool.cs for Aether dependencies
  - Identified types used: Body, Fixture, World, Vector2, BodyType, SolverIterations
  - Documented usage patterns in repository memory

- [ ] **Define adapter interface contracts** ✅
  - `IPhysicsBodyAdapter.cs` - Body abstraction (USER-FACING ⭐)
  - `IFixtureAdapter.cs` - Fixture abstraction (Internal only 🔒)  
  - `ISpatialShapeAdapter.cs` + `ShapeType` enum - Shape type system (6 types, Internal 🔒)
  - `IConstraintAdapter.cs` - Joint/constraint interfaces (Internal 🔒)
  - `IPhysicsWorldAdapter.cs` + `SolverConfig` class - World/simulation interface (HIDDEN from users! 🔒)
  
**Important:** `IPhysicsWorldAdapter` and all other adapters EXCEPT `IPhysicsBodyAdapter` are INTERNAL ONLY. Users NEVER interact with these directly.

- [ ] **Plan implementation strategy** ✅
  - Implementation plan documented in: `CoreEssentials.Physics/adapters/implementations/ImplementationPlan.md`
  - Maps each interface to Aether implementations
  - Defines Sprint 1-4 execution order

---

## Sprint Goals

1. ✅ CoreEssentials.Physics project created and buildable
2. Zero Aether type references in any interface file
3. All interfaces have proper XML documentation
4. Implementation plan documented for next sprints
5. ShapeType enum defined with all required values (Circle, Rectangle, Polygon, ConvexHull, LineSegment, Unknown)
6. SolverConfig class properly structured for world configuration

---

## Acceptance Criteria

- [ ] CoreEssentials.Physics project created in `CoreEssentials.Physics/` folder
- [ ] Project builds successfully with no errors (`dotnet build`) ✅ **Verified**
- [ ] NuGet package metadata configured correctly (PackageId, Version 0.15.0)
- [ ] Zero `nkast.Aether.*` references in interface files (uses `Microsoft.Xna.Framework` for Vector2/BodyType)
- [ ] Implementation plan documented for Phase 2 (adapter implementations) - See `CoreEssentials.Physics/adapters/implementations/ImplementationPlan.md`
- [ ] File structure created: 
  - ✅ `CoreEssentials.Physics/adapters/interfaces/` (6 interface files)
  - ✅ `CoreEssentials.Physics/adapters/implementations/ShapeAdapters/` (empty, ready for shapes)
  - ✅ `CoreEssentials.Physics/factory/` (empty, ready for factory implementations)
- [ ] Sprint 1 tasks ready to execute (interfaces in correct folders with proper XML docs)

---

## Verification Summary

| Criterion | Status | Notes |
|-----------|--------|-------|
| Project builds | ✅ Pass | `dotnet build` succeeds with no errors |
| Interface files created | ✅ Pass | 6 interfaces + ShapeType enum + SolverConfig class |
| No Aether type exposure | ✅ Pass | Interfaces use only public types (Vector2, BodyType) |
| XML documentation | ✅ Pass | All public members have XML docs |
| Directory structure | ✅ Pass | All required folders created |

---

## Related Documents

- [`docs/PhysicsSystemRefactor.md`](c:\repo\core-essentials-monogame\docs\PhysicsSystemRefactor.md) - Full refactoring specification
- [`docs/PhysicsSystemRefactor_SUMMARY.md`](c:\repo\core-essentials-monogame\docs\PhysicsSystemRefactor_SUMMARY.md) - Updated summary with plugin approach

---

*Target Completion: Week of July 13, 2026*  
*Sprint Points: 2 | Remaining Sprints: 7 (total 7 points)*


## Implementation Roadmap[CoreEssentials.Physics/adapters/implementations/ImplementationPlan.md](file://c:/repo/core-essentials-monogame/CoreEssentials.Physics/adapters/implementations/ImplementationPlan.md) - Detailed plan for Sprints 1-4
