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
│   ├── adapters/interfaces/     # Interface definitions
│   └── adapters/implementations/# Adapter classes wrapping Aether
│       └── ShapeAdapters/       # Shape adapter implementations
├── docs/physics_refactor/sprints/    # These sprint files
```

**Sprint 0 creates and validates the project:** The `CoreEssentials.Physics` folder and `.csproj` file are created in Sprint 0, then built to ensure everything compiles before moving forward. This ensures we have a working foundation for subsequent sprints.

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
| 📋 [0](Sprint_0_Planning.md) | Create Project + Interfaces | 2 | In Progress | Create CoreEssentials.Physics project, define adapter interfaces, build successfully |
| 🔧 [1](Sprint_1_CoreInterfaces.md) | Core Interface Definitions | 1 | Not Started | Create IPhysicsBody, IFixture, ISpatialShape, etc. |
| ⚙️ [2](Sprint_2_AdapterImplementations.md) | Adapter Implementations - Core Classes | 1 | Not Started | Implement PhysicsBodyAdapter, FixtureAdapter, WorldAdapter |
| 📐 [3](Sprint_3_ShapeAdapters.md) | Spatial Shape Adapters | 1 | Not Started | CircleShape, RectangleShape, PolygonShape adapters |
| 🔨 [4](Sprint_4_Factories_Helpers.md) | Factory Classes & Helpers | 1 | Not Started | PhysicsFactory, SpatialShapeFactory, BodyPoolAdapter |
| 🔄 [5](Sprint_5_MigrateExistingCode.md) | Migrate Existing Code to Adapters | 1 | Not Started | Update PhysicsEngine, WorldPool, DebugRenderer |
| ✅ [6](Sprint_6_Testing_Documentation.md) | Testing & Documentation | 1 | Not Started | Unit tests, integration tests, API docs |
| 🚀 [7](Sprint_7_Review_Deploy.md) | Review, Polish & Release Prep | 1 | Not Started | Code review, performance optimization, NuGet release |

---

## Sprint Point Summary

- **Total Points:** 9 points across 8 sprints
- **Average Per Sprint:** ~1.1 points (conservative estimation)
- **Timeline Estimate:** 8 weeks (one sprint per week)

---

## Key Workflow

1. **Sprint 0 (2 pts)** - Creates `CoreEssentials.Physics` project and makes it buildable ✓
2. **Sprints 1-4 (1 pt each)** - Build core physics engine with adapter pattern
3. **Sprint 5 (1 pt)** - Migrate existing code to use new adapters in CoreEssentials.Physics
4. **Sprint 6 (1 pt)** - Testing and documentation  
5. **Sprint 7 (1 pt)** - Final review and NuGet release

---

## How to Use These Sprints

### For Developers Starting a New Sprint:

1. Open the corresponding `.md` file for your assigned sprint
2. Review tasks and mark them as `[x]` when complete
3. Check acceptance criteria before moving to next sprint
4. Update status in sprint header if needed

### Reference Documents:

- [`PhysicsSystemRefactor.md`](../PhysicsSystemRefactor.md) - Full technical specification
- [`PhysicsSystemRefactor_SUMMARY.md`](../PhysicsSystemRefactor_SUMMARY.md) - Updated summary with key learnings

---

## Sprint Point Calculation Guide

Tasks are sized based on complexity and risk:

| Points | Complexity | Risk | Examples |
|--------|-----------|------|----------|
| 1 | Low | Low | Interface definition, simple adapter |
| 2 | Medium | Low-Medium | Complex interface with multiple methods |
| 5 | High | High | Major refactoring, migration of existing code |

---

## Sprint Completion Checklist

Before closing a sprint:
- [ ] All tasks marked as `[x]` complete
- [ ] Acceptance criteria verified
- [ ] Code reviewed by team member
- [ ] Tests passing (if applicable)
- [ ] Documentation updated
- [ ] Next sprint tasks visible and ready

---

*Last Updated: 2026-07-13*  
*Project: CoreEssentials Physics Adapter Pattern Refactoring*
