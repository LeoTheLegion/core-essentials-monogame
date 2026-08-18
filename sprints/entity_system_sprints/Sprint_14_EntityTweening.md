# Sprint 14 — Entity Tweening 🎬

**Points:** 4.5  
**Status:** Complete ✅  
**Sprint Goal:** Built-in animation for entity properties via simple value-based tween classes and a composable `TweenComponent`.

**Dependencies:** Sprint 13 (GameStateSerialization)

**Architecture:** Simple value interpolation — no expressions, no reflection, no MonoGame.Extended Tweener dependency:
- **`TweenVector2`** — interpolates a Vector2 from start to end over time using an easing function. Call `.GetValue()` each frame for the current eased value. Supports `.Loop` (repeat) and `.Reverse` (ping-pong).
- **`TweenFloat`** — interpolates a float from start to end over time using an easing function. Call `.GetValue()` each frame for the current eased value. Supports `.Loop` (repeat) and `.Reverse` (ping-pong).
- **`EasingFunctions`** — static helper class with 30+ standard easing curves (Linear, Quad, Cubic, Quart, Quint, Sine, Expo, Circ, Elastic, Back, Bounce — each with In/Out/InOut variants). Drop-in replacement for MonoGame.Extended easing API.
- **`TweenComponent`** — entity component that manages multiple active tweens. Auto-advances them each frame, auto-removes completed ones, and handles loop/reverse logic. Exposes `.TweenToVector2()` and `.TweenToFloat()` to create tweens.

**Existing Entity Properties (from Sprint 13):**
- `Entity.Position` — Vector2, tweenable
- `Entity.Rotation` — float (radians), tweenable
- `Entity.Scale` — Vector2, moved to Entity base class in Sprint 13 (was previously only on SpriteComponent)

---

## Tasks

- [X] **T1: Create `TweenVector2` and `TweenFloat` classes (1.5 pts)** ⭐ User-facing ✅ COMPLETE
  - Simple value interpolation: start → end over duration with easing function
  - `.GetValue()` returns current eased interpolated value
  - `.IsComplete` flag for when tween finishes
  - `.Elapsed` tracks time since start
  - `.Loop` — repeat animation when complete (snap back to start)
  - `.Reverse` — ping-pong animation (start → end → start → end...) when combined with Loop
  - No expressions, no target objects — just values

- [X] **T2: Create `TweenComponent` entity component (1 pt)** ⭐ User-facing ✅ COMPLETE
  - Extends `EntityComponent` for composable tween behavior
  - Manages multiple active tweens internally
  - `.TweenToVector2(start, end, duration, easing?)` — creates a Vector2 tween
  - `.TweenToFloat(start, end, duration, easing?)` — creates a float tween
  - Auto-advances all tweens in `Update(GameTime)`
  - Handles loop/reverse: resets or toggles direction instead of removing
  - Removes completed one-shot tweens
  - `.CancelAll()` cancels all active tweens

- [X] **T3: No game system wiring needed (0.5 pt)** ✅ COMPLETE
  - Each `TweenComponent` updates itself via `Update(GameTime)` — EntitySystem already calls component updates automatically
  - No static manager or game system required

- [X] **T4: Write unit tests (1 pt)** 🔁 Validation ✅ COMPLETE
  - Test tween interpolation (start → end over time) — 43 tests total
  - Test easing functions apply correctly (InQuad, sine wave)
  - Test TweenComponent manages multiple tweens
  - Test loop behavior (reset on complete)
  - Test reverse behavior (toggle direction on complete)
  - Test CancelAll clears all active tweens

- [X] **T5: Create user documentation (0.5 pt)** 📚 User-facing ✅ COMPLETE
  - Created `docs/EntityTweening.md` user guide
  - Documented TweenVector2, TweenFloat, and TweenComponent API
  - Documented easing functions (`EasingFunctions.InQuad`, `OutCubic`, etc.)
  - Documented Loop and Reverse properties
  - Provided animation examples

---

## Acceptance Criteria

- [ ] Entities can tween position, rotation, and scale via TweenComponent
- [ ] Tweens support easing functions
- [ ] Each TweenComponent manages its own tweens (no shared state)
- [ ] Tweens auto-update each frame via EntitySystem component updates
- [ ] Completed one-shot tweens are auto-removed from the component
- [ ] Looping tweens reset on completion; reverse looping tweens toggle direction
- [ ] Developers get back a tween object and call `.GetValue()` to apply the eased value
- [ ] Project builds cleanly — **0 errors, 0 warnings**
- [ ] All existing tests pass + new tween tests added

---

## Deliverables

| File | Type | Visibility | Notes |
|------|------|------------|-------|
| `Tweening/Tween.cs` | New | ⭐ PUBLIC | TweenVector2 and TweenFloat interpolation classes |
| `Tweening/EasingFunctions.cs` | New | ⭐ PUBLIC | 30+ standard easing curves (In/Out/InOut variants) |
| `Tweening/TweenComponent.cs` | New | ⭐ PUBLIC | Entity component managing active tweens |
| `EntityTweenTests.cs` | New | 🔒 Internal | Unit tests for tween interpolation and component |
| `EasingFunctionsTests.cs` | New | 🔒 Internal | Unit tests for easing function boundaries |
| `docs/EntityTweening.md` | New | ⭐ PUBLIC | User guide for entity tweening |

---

## Design Decisions

**Why simple value interpolation instead of wrapping MonoGame.Extended Tweener?**

The MGE `Tweener` uses `Expression<Func<T, Property>>` to read/write arbitrary object properties via reflection. This is overkill for our use case and creates several problems:
- Requires the target object reference every time
- Couples us to MGE's internal API
- Can't easily store/reuse tween values later
- The entity already knows its own properties — it doesn't need a tween to read them

Our approach:
- **Tween is just easing + time** — start value, end value, duration, easing function. That's it.
- **Developer owns the application** — call `.GetValue()` and apply it where you want
- **No expressions, no reflection, no target objects** — just values
- **Multiple tweens per component** — animate position, rotation, and scale simultaneously

**API Example:**
```csharp
// Create tweens
var posTween = tweenComponent.TweenToVector2(entity.Position, new Vector2(100, 200), 1f, EasingFunctions.InQuad);
var rotTween = tweenComponent.TweenToFloat(entity.Rotation, MathHelper.Pi, 0.5f, EasingFunctions.OutCubic);

// In entity update:
entity.Position = posTween.GetValue();
entity.Rotation = rotTween.GetValue();
```

**Looping and Reversing:**
```csharp
var bounceTween = tweenComponent.TweenToFloat(0f, -50f, 1f, t => (float)Math.Sin(t * Math.PI));

// Repeat: snap back to start and repeat
bounceTween.Loop = true;

// Ping-pong: smoothly reverse direction each cycle (works with monotonic easings)
bounceTween.Loop = true;
bounceTween.Reverse = true;
```

**Demo — relative offset animation:**
```csharp
// CharacterEntity bounces in place by tweening a Y offset
private float _originalY;
private TweenFloat? _yOffsetTween;

public override void OnStart()
{
    _yOffsetTween = tweenComponent.TweenToFloat(0f, -50f, 1f, t => (float)Math.Sin(t * Math.PI));
    _yOffsetTween.Loop = true;
}

public override void Update(GameTime gameTime)
{
    // Note: XML position is applied AFTER OnStart, so capture on first frame
    if (!_initialized) { _originalY = Position.Y; _initialized = true; }
    Position = new Vector2(Position.X, _originalY + _yOffsetTween.GetValue());
}
```

**Why per-component instead of a GameSystem?**
- Each entity manages its own animations independently
- No shared state between components
- Cleaner lifecycle: component updates tweens, removes completed ones
- No need for a static `TweenManager` or scene-level system

---

## Notes & Risks

- **Low risk** — simple interpolation math (Lerp + easing function)
- Easing functions from MonoGame.Extended (`EasingFunctions.InQuad`, `OutCubic`, etc.) are used but not tightly coupled — any `Func<float, float>` works
- Default easing is linear if none provided
- Completed tweens are auto-removed to prevent memory leaks
- **Reverse mode** works best with monotonic easings (linear, in-out quad). Half-sine waves (`sin(t * π)`) already do a round trip within one pass, so Loop alone gives smooth bounce without needing Reverse
- **XML-loaded entities**: position is set by `EntitySerializer.ApplyEntityProperties()` AFTER `OnStart()`. Capture the spawn position on the first Update frame, not in OnStart.

*Created: 2026-08-07 | Updated: 2026-08-13 | Part of Entity System Enhancements Project*
