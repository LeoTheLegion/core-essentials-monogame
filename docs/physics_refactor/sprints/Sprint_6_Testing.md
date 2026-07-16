# Sprint 6 — Testing & Playground Migration ✅

**Points:** 5  
**Status:** Not Started (depends on Sprint 5)  
**Sprint Goal:** Comprehensive test coverage for all new physics types, plus migrate Playground examples to use the clean new API.

---

## Tasks

- [ ] **T1: Unit tests — IPhysicsBody / PhysicsBody (1 pt)**
  - `CreateCircle_ShouldReturnActiveFixture()`
  - `ApplyForce_ShouldChangeVelocity()`
  - `SetLinearVelocity_ShouldOverrideCurrentVelocity()`
  - `Type_ChangeDynamicToStatic_ShouldUpdateMass()`
  - `Dispose_ShouldRemoveFromWorld()`

- [ ] **T2: Unit tests — Shapes (1 pt)**
  - `CircleShape_PointContains_InsidePoint_ReturnsTrue()`
  - `RectangleShape_Vertices_CountEquals4()`
  - `PolygonShape_BoundingRadius_CorrectValue()`
  - Shape translation/rotation transforms verified

- [ ] **T3: Unit tests — PhysicsEngine GameSystem (1 pt)**
  - `CreateDynamic_ShouldReturnIPhysicsBody_NotAetherType()`
  - `Update_StepsWorldOncePerFrame()`
  - `Destroy_ShouldRemoveBodyFromWorld()`
  - Gravity changes propagate to world

- [ ] **T4: Integration tests — End-to-end workflow (1 pt)**
  - Create two dynamic bodies → verify they collide under gravity
  - Apply impulse → verify velocity change matches expectation
  - Joint creation test: connect two bodies with RevoluteJoint, verify constrained motion

- [ ] **T5: Update Playground examples (1 pt)** 🔧
  - Update `PhysicsEntityScene.cs` to use new API (`GetGameSystem<PhysicsEngine>()`, `.CreateDynamic()`)
  - Verify playground still runs and physics behaves correctly
  - Create a simple demo scene showcasing the clean API

---

## Acceptance Criteria

- [ ] All unit tests pass (`dotnet test CoreEssentials.Tests`)
- [ ] Minimum 15 test cases covering Body, Shape, Engine, and integration scenarios
- [ ] Playground compiles and runs with new physics API
- [ ] No Aether types visible in Playground code — only `IPhysicsBody`, `IFixture`, etc.

---

## Deliverables

| File | Purpose |
|------|---------|
| `CoreEssentials.Tests/Physics/PhysicsBodyTests.cs` | Unit tests for PhysicsBody |
| `CoreEssentials.Tests/Physics/ShapeTests.cs` | Unit tests for shape implementations |
| `CoreEssentials.Tests/Physics/PhysicsEngineTests.cs` | GameSystem + engine tests |
| `CoreEssentials.Tests/Physics/IntegrationTests.cs` | End-to-end workflow tests |
| `CoreEssentials.Playground/PhysicsEntityScene.cs` | Updated to use new API |

---

## Notes & Risks

- **Mocking strategy:** For unit tests, mock the Aether world/body so tests don't depend on actual physics simulation. Use Moq or hand-rolled mocks.
- Integration tests DO run against real Aether engine — these verify the full pipeline works correctly.
- If existing `CoreEssentials.Tests/` has old physics tests, migrate them to use new interfaces (or mark `[Obsolete]` and create replacements).

---

*Created: 2026-07-16 | Part of Physics System Refactoring Project*
