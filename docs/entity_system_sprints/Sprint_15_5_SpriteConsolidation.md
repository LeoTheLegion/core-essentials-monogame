# Sprint 15.5 — Sprite Consolidation & Animation Component 🎞️

**Points:** 5.5
**Status:** ✅ Completed
**Completed:** 2026-08-14
**Sprint Goal:** Unify `Sprite` and `AnimatedSprite` into a single `Sprite` type and add an `AnimationComponent` so animated entities use the component system (and the base `Entity.GetSize()`).

**Dependencies:** Sprint 6 (Components), Sprint 10 (XML Definitions)
**Breaking:** ⚠️ Yes — public API rename + content XML schema change. Requires a migration guide.

---

## Background

Today there are two overlapping sprite types:

- `Sprite` — a drawable that can be a plain `texture2d` (single frame) **or** a `spritesheet` (picks one static frame via `_defaultFrame`). It has a vestigial `AnimationFrame` property that `Draw()` ignores.
- `AnimatedSprite` — a `spritesheet` + a frame sequence (`int[]`) + frame rate. The "real" animated sprite.

Both are "a drawable backed by a sheet with N frames". A static sprite is just a 1-frame sprite. This split forces `SpriteComponent` (holds `Sprite`) and animation code (uses `AnimatedSprite`) to diverge, and blocks a clean `AnimationComponent`.

**Target model:**

- `SpriteSheet` — texture + grid (unchanged).
- `Sprite` (renamed from `AnimatedSprite`, absorbs old `Sprite`) — N frames + optional frame rate. `texture2d` source = 1 frame; `spritesheet` source = N frames. Exposes `Draw()`, `DrawFrame()`, `GetSize()`, `Texture` (for instanced batching).
- `AnimationState` — per-entity playback (unchanged).
- `AnimationComponent` — drives one or more named animations on an entity, pushing the current frame into the `SpriteComponent`.

---

## Tasks

- [x] **T1: Rename `AnimatedSprite` → `Sprite`, fold in old `Sprite` (2 pts)** ⭐ User-facing / 🔒 Internal
  - New `Sprite` accepts both source types:
    - `texture2d` → single-frame sprite (keep per-sprite `Origin`)
    - `spritesheet` → N-frame sprite (frame sequence + frame rate)
  - Merge API: `Draw()`, `DrawFrame()`, `GetSize()`, `Texture`, `FrameCount`, `FrameRate`, `SpriteSize`
  - Remove the old `Sprite` class; update all references (`SpriteComponent`, `Entity.RegisterForInstancedRendering`, playground, tests)
  - **Remove the unconditional `Debug.Primitives.DrawRectangle(..., Color.Red)` in both draw paths** (leftover debug code drawing a red outline on every sprite)

- [x] **T2: Content XML schema + migration (0.5 pt)** ⭐ User-facing
  - `Sprite` XML supports both `texture2d` and `spritesheet` source types (unified `SpriteData` root)
  - Migrate `ball_sprite.xml` (texture2d), `character_sprite.xml`, `character_anim_walk.xml` (spritesheet) to the unified schema
  - Document the schema in the migration guide

- [x] **T3: Add `AnimationComponent` (1.5 pts)** ⭐ User-facing
  - Multi-animation: `Dictionary<string, AnimationState>` keyed by name
  - `AddAnimation(name, Sprite)`, `Play(name)`, `Stop(name?)`, `SetSpeed(name, speed)`
  - `CurrentAnimation`, `Animations` (names)
  - `Update()` advances playing states and pushes the current frame into the entity's `SpriteComponent`
  - `Draw()` fallback: render the current frame directly if no `SpriteComponent` present
  - `GetSize()` → current frame size × `Owner.Scale`
  - `ISerializableComponent`: persist animation names + asset names, current name, speed/loop state

- [x] **T4: Wire `Entity.GetSize()` fallback chain (0.5 pt)** 🔒 Internal
  - `SpriteComponent` → `AnimationComponent` → `Vector2.Zero`

- [x] **T5: Register `AnimationComponent` in `EntitySerializer` (0.25 pt)** 🔒 Internal
  - `Register<AnimationComponent>("AnimationComponent")`

- [x] **T6: Refactor playground entities to components (0.5 pt)** ⭐ User-facing
  - `CharacterEntity` → uses `SpriteComponent` (drop `Render`/`GetSize` overrides)
  - `AnimatedCharacterEntity` → uses `AnimationComponent` (drop `Render`/`GetSize`/`Update` overrides)

- [x] **T7: Write unit tests (0.5 pt)** 🔁 Validation
  - `Sprite` unified: texture2d single-frame + spritesheet N-frame `GetSize`/`Draw`
  - `AnimationComponent`: add/play/stop, frame advance, `GetSize` × scale
  - Base `Entity.GetSize()` via both components
  - `AnimationComponent` serialization round-trip
  - No stray debug rectangle on draw

- [x] **T8: Documentation + migration guide (0.75 pt)** 📚 User-facing
  - `docs/SpriteSystem.md` — unified `Sprite` usage, parameters, example
  - `docs/AnimationComponent.md` — multi-animation usage, example
  - `docs/Migration_Guide_SpriteConsolidation.md` — breaking changes, API mapping, XML schema migration

---

## Acceptance Criteria

- [x] Single `Sprite` type handles both static (texture2d) and animated (spritesheet) sources
- [x] `AnimatedSprite` class removed; all references updated
- [x] No red debug rectangle drawn on sprites by default
- [x] `AnimationComponent` drives named animations into a `SpriteComponent`
- [x] Base `Entity.GetSize()` works for sprite and animated entities without overrides
- [x] `CharacterEntity` and `AnimatedCharacterEntity` use components (no sprite overrides)
- [x] Project builds cleanly — **0 errors, 0 warnings**
- [x] All existing tests pass + new sprite/animation tests added
- [x] Migration guide documents the breaking rename + XML schema change

---

## Deliverables

| File | Type | Visibility | Notes |
|------|------|------------|-------|
| `Assets/Sprite.cs` | Modified (absorbs `AnimatedSprite`) | ⭐ PUBLIC | Unified sprite type |
| `Assets/AnimatedSprite.cs` | Removed | ⭐ PUBLIC | Renamed to `Sprite` |
| `Components/BuiltIn/AnimationComponent.cs` | New | ⭐ PUBLIC | Multi-animation component |
| `Components/BuiltIn/SpriteComponent.cs` | Modified | ⭐ PUBLIC | Holds unified `Sprite` |
| `Entity.cs` | Modified | ⭐ PUBLIC | `GetSize()` fallback chain |
| `Serialization/EntitySerializer.cs` | Modified | 🔒 Internal | Register `AnimationComponent` |
| `Content/*.xml` | Modified | ⭐ PUBLIC | Unified sprite XML schema |
| `UnifiedSpriteTests.cs` / `AnimationComponentTests.cs` | New | 🔒 Internal | Unit tests |
| `docs/SpriteSystem.md` | New | ⭐ PUBLIC | Sprite usage guide |
| `docs/AnimationComponent.md` | New | ⭐ PUBLIC | Animation component guide |
| `docs/Migration_Guide_SpriteConsolidation.md` | New | ⭐ PUBLIC | Breaking-change migration |

---

## Notes & Risks

- **Breaking change** — `AnimatedSprite` → `Sprite` rename affects public API and content XML. Mitigated by the migration guide and a dedicated sprint/branch.
- **Content migration** — existing `*.xml` sprite files must be re-validated against the unified schema. Playground content is the primary test surface.
- **Debug rectangle removal** — the stray red outline is currently visible in the playground; removing it changes the visual baseline (expected, desirable).
- **Batching** — `Sprite.Texture` must remain available for `RegisterForInstancedRendering`; verify instanced rendering still batches correctly after the merge.

---

*Created: 2026-08-14 | Part of Entity System Enhancements Project*
