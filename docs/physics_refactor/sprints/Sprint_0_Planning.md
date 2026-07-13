# Sprint 0: Planning & Interface Definition

**Points:** 2  
**Status:** In Progress  
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

- [ ] **Create CoreEssentials.Physics project** - Set up NuGet-published package project
  ```xml
  <!-- CoreEssentials.Physics/CoreEssentials.Physics.csproj -->
  <Project Sdk="Microsoft.NET.Sdk">
    <PropertyGroup>
      <TargetFramework>net8.0</TargetFramework>
      <PackageId>CoreEssentials-MonoGame</PackageId>
      <Version>0.15.0</Version>
      <Authors>LeoTheLegion</Authors>
      <Description>Physics engine with adapter pattern integration for CoreEssentials</Description>
    </PropertyGroup>
    <!-- References to Aether and CoreEssentials dll -->
  </Project>
  ```

- [ ] **Analyze existing codebase** - Review PhysicsEngine.cs, WorldPool.cs to understand current Aether dependencies
  - Reference: `docs/PhysicsSystemRefactor.md`
  
- [ ] **Define adapter interface contracts** - Create all public interfaces without exposing Aether types
  - IPhysicsBodyAdapter.cs in `adapters/interfaces/`
  - IFixtureAdapter.cs in `adapters/interfaces/`
  - ISpatialShapeAdapter.cs (with ShapeType enum) in `adapters/interfaces/`
  - IPhysicsWorldAdapter.cs (with SolverConfig class) in `adapters/interfaces/`
  - IConstraintAdapter.cs in `adapters/interfaces/`
  - IPhysicsFactory.cs in `adapters/interfaces/`
  
- [ ] **Plan implementation strategy** - Map each interface to corresponding Aether implementation
  - Document internal wrapping approach for PhysicsEngineAdapter, BodyAdapter, etc.

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

- [x] CoreEssentials.Physics project created in `CoreEssentials.Physics/` folder
- [ ] Project builds successfully with no errors (dotnet build)
- [ ] NuGet package metadata configured correctly
- [ ] Zero `nkast.Aether.*` references in interface files
- [ ] Implementation plan documented for Phase 2 (adapter implementations)
- [ ] File structure created: 
  - `CoreEssentials.Physics/adapters/interfaces/`
  - `CoreEssentials.Physics/adapters/implementations/ShapeAdapters/`
  - `CoreEssentials.Physics/factory/`
- [ ] Sprint 1 tasks ready to execute (interfaces in correct folders)

---

## Related Documents

- [`docs/PhysicsSystemRefactor.md`](c:\repo\core-essentials-monogame\docs\PhysicsSystemRefactor.md) - Full refactoring specification
- [`docs/PhysicsSystemRefactor_SUMMARY.md`](c:\repo\core-essentials-monogame\docs\PhysicsSystemRefactor_SUMMARY.md) - Updated summary with plugin approach

---

*Target Completion: Week of July 13, 2026*  
*Sprint Points: 2 | Remaining Sprints: 7 (total 7 points)*
