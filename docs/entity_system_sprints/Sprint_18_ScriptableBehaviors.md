# Sprint 18 — Scriptable Behaviors 📜

**Points:** 6.5  
**Status:** Not Started  
**Sprint Goal:** Attach coroutines or scripts declaratively via XML or API.

**Dependencies:** Sprint 3 (Event System), Sprint 6 (Components)

---

## Tasks

- [ ] **T1: Create `ScriptRegistry` class (1.5 pts)** ⭐ User-facing
  - `Register(string name, Func<Entity, object, IEnumerator> script)` method
  - Store registered scripts in dictionary
  - Script discovery and loading

- [ ] **T2: Create `ScriptComponent` class (2 pts)** ⭐ User-facing
  - Parse `<Script>` elements from XML
  - Execute scripts with parameters
  - Manage script lifecycle (start/stop/pause)
  - Parameter binding from XML or API

- [ ] **T3: Add script management to `Entity` (1.5 pts)** ⭐ User-facing
  - `AddScript(string name, object parameters)` method
  - `RemoveScript(string name)` method
  - Auto-execute scripts on entity start
  - Script cleanup on entity destroy

- [ ] **T4: Write unit tests (1 pt)** 🔁 Validation
  - Test script registration and execution
  - Test script parameters
  - Test script lifecycle
  - Test script cleanup

- [ ] **T5: Create user documentation (0.5 pt)** 📚 User-facing
  - Create `docs/ScriptableBehaviors.md` user guide
  - Document script registration
  - Document ScriptComponent usage
  - Provide XML examples

---

## Acceptance Criteria

- [ ] Scripts can be registered and executed
- [ ] Scripts support parameters
- [ ] Scripts are managed by entity lifecycle
- [ ] Scripts can be loaded from XML
- [ ] Project builds cleanly — **0 errors, 0 warnings**
- [ ] All existing tests pass + new script tests added

---

## Deliverables

| File | Type | Visibility | Notes |
|------|------|------------|-------|
| `Scripting/ScriptRegistry.cs` | New | ⭐ PUBLIC | Script registration |
| `Scripting/ScriptComponent.cs` | New | ⭐ PUBLIC | Script component |
| `Entity.cs` | Modified | ⭐ PUBLIC | Add script management |
| `ScriptableBehaviorTests.cs` | New | 🔒 Internal | Unit tests for scripts |
| `docs/ScriptableBehaviors.md` | New | ⭐ PUBLIC | User guide for scriptable behaviors |

---

## Notes & Risks

- **High risk** — complex integration with coroutines and components
- Script parameter binding needs to be flexible
- Error handling for script execution failures

---

*Created: 2026-08-07 | Part of Entity System Enhancements Project*
