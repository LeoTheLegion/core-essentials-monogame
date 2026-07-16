# Sprint 7 — Documentation Updates 📚

**Points:** 2  
**Status:** Not Started (depends on Sprint 6)  
**Sprint Goal:** All public API has XML documentation, user-facing docs are updated for the new physics system.

---

## Tasks

- [ ] **T1: Add XML documentation to all interfaces (1 pt)**
  - `IPhysicsBody`: Every property and method gets `<summary>`, `<param>`, `<returns>` tags
  - `IFixture`, `IShape`, `IConstraint`, joint interfaces: Same treatment
  - Include usage examples in doc comments where helpful

- [ ] **T2: Add XML documentation to implementations (0.5 pt)**
  - `PhysicsEngine.cs`: Document all public methods, especially GameSystem integration
  - `PhysicsBody.cs`, `Fixture.cs`: Brief docs on Aether wrapping behavior
  - Shape implementations: Note any shape-specific quirks

- [ ] **T3: Update user-facing documentation (0.5 pt)**
  - Rewrite `docs/PhysicsSystem.md` with new API examples and architecture overview
  - Create migration guide showing "before → after" code snippets for common patterns
  - Update `docs/README.md` to link to new physics docs

---

## Acceptance Criteria

- [ ] Zero XML documentation warnings on build (`<NoWarn>CS1591</NoWarn>` removed or all public members documented)
  - All public interfaces and implementations have complete `<summary>` tags
- [ ] `docs/PhysicsSystem.md` accurately reflects new API (no stale Aether references in user-facing docs)
- [ ] Migration guide exists with concrete code examples showing old → new usage

---

## Deliverables

| File | Change |
|------|--------|
| All `.cs` files in CoreEssentials.Physics | XML doc comments added |
| `docs/PhysicsSystem.md` | Complete rewrite for new API |
| `docs/Migration_Guide_Physics.md` (new) | Old → new API migration examples |
| `docs/README.md` | Updated links to physics docs |

---

## Notes & Risks

- Enable `<GenerateDocumentationFile>true</GenerateDocumentationFile>` in csproj and check for CS1591 warnings.
- **Important:** Internal-only types (`IFixture`, `IShape`, etc.) should have `[Obsolete("Internal use only")]` AND XML docs noting internal usage — so power users understand the architecture without being encouraged to depend on it.

---

*Created: 2026-07-16 | Part of Physics System Refactoring Project*
