# Sprint 4 — Playground Behavior Components 🎛️

**Points:** 5 | **Status:** Not Started | **Goal:** Move every piece of per-scene runtime behavior into components so scenes can be pure data.

## Why This Sprint

The strict scene format expresses *structure* (entities, components, properties, binds, references) but not *behavior*. Today each demo scene subclass wires keyboard shortcuts, audio, debug toggles, camera input, and dynamic physics spawns in C#. To make the playground scenes fully data-driven, that behavior becomes components. Per the developer's direction, these live in the **playground** (they are demo behaviors, not library built-ins) — CoreEssentials keeps only genuinely general components.

## Behavior Inventory → Components

| Current home | Behavior | New component (playground) |
|---|---|---|
| `CharacterScene` | Q/W/E play sound; Z/X set master volume | `SoundKeyComponent`, `VolumeKeyComponent` |
| `CharacterScene` | M/Add navigate to other scenes | `NavigateOnKeyComponent` (key → scene type) |
| `CharacterScene` | F3 toggle entity debug mode + config | `DebugToggleComponent` |
| `CharacterScene` | background music start + pause-on-focus | `MusicComponent` (asset, auto-pause on app focus loss) |
| `GuiAnchorDemo` / `CameraScene` / `LabelAlignmentDemo` | WASD pan, Q/E zoom, R reset camera | `CameraInputComponent` (wraps the built-in `CameraComponent`) |
| `SendMessageDemo` | Space broadcast; P prefab spawn; B typed spawn; D destroy-last | `PingControlComponent` |
| `PhysicsEntityScene` | N random balls w/ collision filter + impulse; VIP balls; world border | `PhysicsSpawnComponent` (count, bounds, prefab, category mask) |
| `PhysicsEntityScene` | Save/Load GUI buttons → entity-system save/load | `SaveLoadButtonsComponent` |
| `PhysicsEntityScene` | F1 physics debug overlay (custom Draw) | `PhysicsDebugOverlayComponent` (F1 toggle + draw) |

Each component subscribes to `Input.Keyboard.KeyReleased` in `OnAttach` and unsubscribes in `OnDetach`, so attach/detach lifecycle is clean. Components that need the scene or game reach it via `EntitySystem?.Scene` / `Game` (added in Sprint 2).

## Tasks

- [ ] T1 ⭐ `NavigateOnKeyComponent` — key → `Type targetScene`; instantiates + `SceneManager.LoadScene`
- [ ] T2 ⭐ `SoundKeyComponent` / `VolumeKeyComponent` — key → play one-shot asset / set master volume
- [ ] T3 ⭐ `DebugToggleComponent` — key → toggle `EntitySystem.DebugMode` (+ optional config flags)
- [ ] T4 ⭐ `MusicComponent` — start an audio asset on attach; pause/resume on application focus change
- [ ] T5 ⭐ `CameraInputComponent` — WASD/QE/R driving a built-in `CameraComponent` (speed + zoom sensitivity props)
- [ ] T6 ⭐ `PingControlComponent` — Space/P/B/D demo commands (broadcast, prefab spawn, typed spawn, destroy-last)
- [ ] T7 ⭐ `PhysicsSpawnComponent` — declarative random/VIP ball spawning with collision filters + impulses; optional world border
- [ ] T8 ⭐ `SaveLoadButtonsComponent` — save/load GUI buttons wired to the entity system's save/load
- [ ] T9 ⭐ `PhysicsDebugOverlayComponent` — F1 toggle + physics debug draw overlay
- [ ] T10 🔁 Unit tests for each component (attach/detach, key dispatch, side effects via fakes)

## Acceptance Criteria

- Every behavior in the inventory is reachable from a component with declarative properties
- Components subscribe/unsubscribe input cleanly on attach/detach (no leaked handlers)
- Build clean, all tests passing

## Notes & Risks

- **Playground, not Core:** these are demo behaviors. If one proves general later it can be promoted to `Components/BuiltIn` in a separate change.
- **Key enum as XML property:** components take keys as strings (e.g. `"Q"`) and parse via `Enum.Parse<Keys>` so they're settable from flat attributes / `<Properties>`.
- **PhysicsSpawn is the biggest piece** — it must reproduce random positions, per-ball collision filters resolved from `PhysicsConfig`, and impulses declaratively.
- **No scene subclasses touched in this sprint** — components are added + tested; wiring them into scenes is Sprint 5.

---
*Created: 2026-09-01 | Part of Scene-as-Data Project*
