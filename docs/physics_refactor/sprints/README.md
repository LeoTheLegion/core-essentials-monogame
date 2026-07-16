# Physics Refactor - Scrum Sprints 🚀

This folder contains sprint plans for the physics system refactoring project using an agile/Scrum approach. Each file represents one sprint with tasks estimated in story points (1, 2, or 5 points).

## Project Structure

⚠️ **Important:** The physics system will live in a **separate NuGet-published project** (`CoreEssentials.Physics`) within the solution:

```
core-essentials-monogame.sln
├── CoreEssentials/              # Main game systems library (existing)
├── CoreEssentials.Playground/   # Integration examples (existing)
├── CoreEssentials.Tests/        # Tests for main library (existing)
├── CoreEssentials.Physics/      # NEW: Physics engine package (created in Sprint 0!)
│   ├── types/                   # Pure interface abstractions (NO Aether refs)
│   │   ├── IPhysicsBody.cs     # ⭐ ONLY user-facing physics object interface
│   │   ├── IFixture.cs         # 🔒 Internal use only by PhysicsBody
│   │   ├── IShape.cs           # 🔒 Internal use only by PhysicsBody/Factory
│   │   ├── IConstraint.cs      # 🔒 Internal use only by Factory
│   │   └── IPhysicsWorld.cs    # 🔒 Internal use ONLY (completely hidden!)
│   │
│   ├── engines/aether/          # Aether engine implementations
│   │   ├── PhysicsEngine.cs    # ⭐ Wraps world + implements IFixedUpdateGameSystem
│   │   ├── PhysicsBody.cs      # Implements IPhysicsBody, wraps Aether.Body
│   │   ├── Fixture.cs          # 🔒 Implements IFixture (internal only)
│   │   └── Shapes/             # 🔒 CircleShape, RectangleShape, PolygonShape
│   │
│   └── factory/                 # Factory classes for creating physics objects
│       ├── PhysicsFactory.cs   # 🔒 Creates bodies via interfaces
│       └── SpatialShapeFactory.cs # 🔒 Shape creation factory
├── docs/physics_refactor/sprints/    # These sprint files
```

**Key Design Decision:** Users interact ONLY through `IPhysicsBody` and the `PhysicsEngine` GameSystem. The world type (`IPhysicsWorld`) is **COMPLETELY HIDDEN** from users — it's managed internally by `PhysicsEngine` with no public API exposure. All other types (`IFixture`, `IShape`, `IConstraint`) are also internal-only 🔒.

**Sprint 0 creates and validates the project:** The `CoreEssentials.Physics` folder structure, `.csproj` file, and build configuration are set up in Sprint 0 to ensure everything compiles before moving forward.

This structure allows users to install `CoreEssentials-MonoGame` NuGet package and get physics engine automatically integrated as a GameSystem.

---

## Sprint Structure

Each sprint is designed to be approximately **5 total points** worth of work, following standard Scrum principles:
- **1 point** = Small task (30 min - 2 hours)
- **2 points** = Medium task (2-4 hours)  
- **5 points** = Large task (1 full day or more)

---

## Sprint Roadmap

| Sprint | Name | Points | Status | Description |
|--------|------|--------|--------|-------------|
| 📋 [0](Sprint_0_Project_Setup.md) | Project Setup & Build Validation | 2 | Not Started | Create `CoreEssentials.Physics` project structure, configure csproj with Aether + CoreEssentials references, verify clean build |
| 🔧 [1](Sprint_1_Core_Types.md) | Core Type Definitions | 3 | Not Started | Define all pure interfaces: `IPhysicsBody`, `IFixture`, `IShape`, `IConstraint`, `IPhysicsWorld` — zero Aether references |
| ⚙️ [2](Sprint_2_Engine_Body_Fixture.md) | Engine Implementations - Body & Fixture | 5 | Not Started | Implement `PhysicsBody` (wraps Aether.Body), `Fixture` (wraps Aether.Fixture), and `PhysicsEngine` GameSystem with IFixedUpdateGameSystem |
| 📐 [3](Sprint_3_Shape_Implementations.md) | Shape Implementations | 2 | Not Started | Implement `CircleShape`, `RectangleShape`, `PolygonShape` wrapping Aether shapes, plus joint implementations (`RevoluteJoint`, `WeldJoint`) |
| 🔨 [4](Sprint_4_Factories.md) | Factory Classes & Body Pooling | 3 | Not Started | `PhysicsFactory` and `SpatialShapeFactory` for creation via interfaces; migrate `WorldPool` to new pooling implementation |
| 🔄 [5](Sprint_5_Migrate_Code.md) | Migrate Existing Physics Code | 3 | Not Started | Update old `PhysicsEngine.cs`, `PhysicsDebugRenderer.cs` in CoreEssentials to use new type abstractions from CoreEssentials.Physics |
| ✅ [6](Sprint_6_Testing.md) | Testing & Playground Migration | 5 | Not Started | Unit tests for all implementations, integration tests verifying GameSystem + Factory patterns, update Playground examples |
| 📚 [7](Sprint_7_Documentation.md) | Documentation Updates | 2 | Not Started | Update `docs/PhysicsSystem.md`, create migration guide (`old API → new API`), add XML docs to all interfaces/implementations |
| 🚀 [8](Sprint_8_Release.md) | Code Review & Release Prep | 3 | Not Started | Final code review, performance profiling, NuGet package configuration, publish `CoreEssentials-MonoGame` package |

---

## Sprint Point Summary

- **Total Points:** 26 points across 9 sprints
- **Average Per Sprint:** ~2.9 points
- **Timeline Estimate:** 9 weeks (one sprint per week) or compressed to 4-5 weeks with parallel work on lower-risk sprints

---

## Key Workflow Phases

**Foundation (Sprint 0–1):** Set up project structure and define all pure interfaces — no Aether references allowed in `types/` folder.

**Core Implementation (Sprint 2–4):** Implement all adapters wrapping Aether types, build the factory system for creation via interfaces, implement body pooling.

**Migration (Sprint 5):** Update existing physics code in CoreEssentials to depend on new abstractions from CoreEssentials.Physics.

**Quality Gate (Sprint 6–8):** Test everything thoroughly, update all documentation, prepare NuGet package for release.

---

## How to Use These Sprints

### For Developers Starting a New Sprint:

1. Open the corresponding `.md` file for your assigned sprint
2. Review tasks and mark them as `[x]` when complete
3. Check acceptance criteria before moving to next sprint
4. Update status in sprint header if needed
5. Run build and test commands after each task to verify nothing is broken

### Reference Documents:

- [`PhysicsSystemRefactor.md`](../PhysicsSystemRefactor.md) - Full technical specification with code examples
- [`PhysicsSystemRefactor_SUMMARY.md`](../PhysicsSystemRefactor_SUMMARY.md) - High-level summary, project structure, key design decisions

---

## Sprint Point Calculation Guide

Tasks are sized based on complexity and risk:

| Points | Complexity | Risk | Examples |
|--------|-----------|------|----------|
| 1 | Low | Low | Simple interface definition, minor config change |
| 2 | Medium | Low-Medium | Complex interface with multiple methods, simple implementation |
| 3 | Medium-High | Medium | Multiple related implementations, factory class |
| 5 | High | High | Major refactoring, migration of existing code, testing suite |

---

## Sprint Dependencies

```
Sprint 0 (Project Setup)
    └── Sprint 1 (Core Types)
        ├── Sprint 2 (Body & Fixture)
        │   └── Sprint 3 (Shapes & Joints)
        │       └── Sprint 4 (Factories)
        │           └── Sprint 5 (Migrate Code)
        │               └── Sprint 6 (Testing)
        │                   └── Sprint 7 (Documentation)
        │                       └── Sprint 8 (Release)
```

**NOTE:** Sprints 2-4 have some parallel potential once interfaces are stable, but Body→Fixture→Shapes has a natural dependency chain.

---

## Sprint Completion Checklist

Before closing a sprint:
- [ ] All tasks marked as `[x]` complete
- [ ] Acceptance criteria verified
- [ ] Code builds cleanly (`dotnet build`)
- [ ] Tests passing (`dotnet test`)
- [ ] Documentation updated
- [ ] Next sprint tasks visible and ready

---

*Last Updated: 2026-07-16*  
*Project: CoreEssentials Physics System Refactoring (Clean Type Names)*
