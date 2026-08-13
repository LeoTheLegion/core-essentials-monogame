# Sprint 14 — Entity Tweening 🎬

**Points:** 4.5  
**Status:** Not Started  
**Sprint Goal:** Built-in animation for position, rotation, scale, and custom properties using MonoGame.Extended.

**Dependencies:** Sprint 13 (GameStateSerialization)

**MonoGame.Extended Coverage:** ✅ Covered — wrapping MonoGame.Extended Tweening system

**Existing Entity Properties (from Sprint 13):**
- `Entity.Position` — Vector2, tweenable
- `Entity.Rotation` — float (radians), tweenable
- `Entity.Scale` — Vector2, moved to Entity base class in Sprint 13 (was previously only on SpriteComponent)
- For physics bodies: use `RigidbodyComponent.Position` / `.Rotation` directly

---

## Tasks

- [ ] **T1: Create `EntityTween` wrapper class (1.5 pts)** ⭐ User-facing
  - `TweenPosition(Vector2 target, TimeSpan duration, Easing easing)` method
  - `TweenRotation(float target, TimeSpan duration, Easing easing)` method
  - `TweenScale(float target, TimeSpan duration, Easing easing)` method
  - Wrap MonoGame.Extended `Tween<T>` for entity properties

- [ ] **T2: Add fluent tween builder (1.5 pts)** ⭐ User-facing
  - `TweenBuilder` for chaining tweens
  - `ThenTweenPosition()`, `ThenTweenRotation()`, etc.
  - `Start()` to begin tween sequence
  - `Cancel()` to stop current tween

- [ ] **T3: Add tween management to `Entity` (0.5 pt)** ⭐ User-facing
  - `ActiveTweens` collection
  - `CancelTweens()` method
  - Auto-update tweens in `Update(GameTime)`

- [ ] **T4: Write unit tests (1 pt)** 🔁 Validation
  - Test position tweening
  - Test rotation tweening
  - Test tween chaining
  - Test tween cancellation

- [ ] **T5: Create user documentation (0.5 pt)** 📚 User-facing
  - Create `docs/EntityTweening.md` user guide
  - Document tween API
  - Document easing functions
  - Provide animation examples

---

## Acceptance Criteria

- [ ] Entities can tween position, rotation, and scale
- [ ] Tweens support easing functions
- [ ] Tweens can be chained
- [ ] Tweens can be cancelled
- [ ] Project builds cleanly — **0 errors, 0 warnings**
- [ ] All existing tests pass + new tween tests added

---

## Deliverables

| File | Type | Visibility | Notes |
|------|------|------------|-------|
| `Tweening/EntityTween.cs` | New | ⭐ PUBLIC | Entity tween wrapper |
| `Tweening/TweenBuilder.cs` | New | ⭐ PUBLIC | Fluent tween API |
| `Entity.cs` | Modified | ⭐ PUBLIC | Add tween management |
| `EntityTweenTests.cs` | New | 🔒 Internal | Unit tests for tweening |
| `docs/EntityTweening.md` | New | ⭐ PUBLIC | User guide for entity tweening |

---

## Notes & Risks

- **Low risk** — MonoGame.Extended already has tweening
- Need to handle tween updates with entity lifecycle
- Performance consideration for many concurrent tweens

---

*Created: 2026-08-07 | Part of Entity System Enhancements Project*
