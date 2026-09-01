# Sprint 4 — Playground Behavior Components 🎛️

**Points:** 5 | **Status:** In Progress (Batches A+B done) | **Goal:** Move every piece of per-scene runtime behavior into components so scenes can be pure data.

## Batches (work pace: batch by scene, commit between groups)

- [x] **Batch A — Character set:** `NavigateOnKeyComponent`, `SoundKeyComponent`, `VolumeKeyComponent`, `DebugToggleComponent`, `MusicComponent` + tests
- [x] **Batch B — Camera/Ping set:** `CameraInputComponent`, `PingControlComponent` + tests
- [ ] **Batch C — Physics set:** `PhysicsSpawnComponent`, `SaveLoadButtonsComponent`, `PhysicsDebugOverlayComponent` + tests

## Why This Sprint

The strict scene format expresses *structure* (entities, components, properties, binds, references) but not *behavior*. Today each demo scene subclass wires keyboard shortcuts, audio, debug toggles, camera input, and dynamic physics spawns in C#. To make the playground scenes fully data-driven, that behavior becomes components. Per the developer's direction, these live in the **playground** (they are demo behaviors, not library built-ins) — CoreEssentials keeps only genuinely general components.

## Behavior Inventory → Components

| Current home | Behavior | New component (playground) |
|---|---|---|
| `CharacterScene` | Q/W/E play sound; Z/X set master volume | `SoundKeyComponent`, `VolumeKeyComponent` |
| `CharacterScene` | M/Add navigate to other scenes | `NavigateOnKeyComponent` (key → scene asset-name string) |
| `CharacterScene` | F3 toggle entity debug mode + config | `DebugToggleComponent` |
| `CharacterScene` | background music start + pause-on-focus | `MusicComponent` (asset, auto-pause on app focus loss) |
| `GuiAnchorDemo` / `CameraScene` / `LabelAlignmentDemo` | WASD pan, Q/E zoom, R reset camera | `CameraInputComponent` (wraps the built-in `CameraComponent`) |
| `SendMessageDemo` | Space broadcast; P prefab spawn; B typed spawn; D destroy-last | `PingControlComponent` |
| `PhysicsEntityScene` | N random balls w/ collision filter + impulse; VIP balls; world border | `PhysicsSpawnComponent` (count, bounds, prefab, category mask) |
| `PhysicsEntityScene` | Save/Load GUI buttons → entity-system save/load | `SaveLoadButtonsComponent` |
| `PhysicsEntityScene` | F1 physics debug overlay (custom Draw) | `PhysicsDebugOverlayComponent` (F1 toggle + draw) |

Each component subscribes to `Input.Keyboard.KeyReleased` in `OnAttach` and unsubscribes in `OnDetach`, so attach/detach lifecycle is clean. Components that need the scene or game reach it via `EntitySystem?.Scene` / `Game` (added in Sprint 2).

## Tasks

- [x] T1 ⭐ `NavigateOnKeyComponent` — key → scene asset-name string; `SceneManager.LoadScene(string)`
- [x] T2 ⭐ `SoundKeyComponent` / `VolumeKeyComponent` — key → play one-shot asset / set master volume
- [x] T3 ⭐ `DebugToggleComponent` — key → toggle `EntitySystem.DebugMode` (+ optional config flags)
- [x] T4 ⭐ `MusicComponent` — start an audio asset on attach; pause/resume on application focus change
- [x] T5 ⭐ `CameraInputComponent` — WASD/QE/R driving a built-in `CameraComponent` (speed + zoom sensitivity props)
- [x] T6 ⭐ `PingControlComponent` — Space/P/B/D demo commands (broadcast, prefab spawn, typed spawn, destroy-last)
- [ ] T7 ⭐ `PhysicsSpawnComponent` — declarative random/VIP ball spawning with collision filters + impulses; optional world border
- [ ] T8 ⭐ `SaveLoadButtonsComponent` — save/load GUI buttons wired to the entity system's save/load
- [ ] T9 ⭐ `PhysicsDebugOverlayComponent` — F1 toggle + physics debug draw overlay
- [ ] T10 🔁 Unit tests for each component (attach/detach, key dispatch, side effects via fakes)

## Acceptance Criteria

- Every behavior in the inventory is reachable from a component with declarative properties
- Components subscribe/unsubscribe input cleanly on attach/detach (no leaked handlers)
- Build clean, all tests passing

## Notes & Risks

- **MusicComponent pause plumbing (Batch A decision):** `Entity.OnApplicationPause(bool)` was an empty virtual that did *not* forward to components, so a data-driven music component had no way to hear focus changes. Added a general `OnApplicationPause` virtual to `EntityComponent` and made `Entity.OnApplicationPause` forward it to every attached component. This is lifecycle plumbing (not a new built-in component), so it fits the architecture and is required for any focus-reactive component. Music starts on attach, stops on detach (scene unload → `Entity.OnDestroy` detaches components), and pauses/resumes via the forwarded call.
- **Testability seams:** each key-driven component exposes a public `HandleKey(Keys)` (invoked by its `KeyReleased` subscription) and routes its external side effect through a small `protected virtual` seam (`LoadScene`, `PlaySound`, `SetVolume`, `ApplyDebugConfig`, `PlayMusic/Pause/Resume/Stop`). Tests use recording subclasses to capture the requested value without real audio/scene transitions, and assert subscribe/unsubscribe by reading the private `_onKeyReleased` field (walking the type hierarchy — reflection does not inherit fields).
- **Test project now references the Playground:** `CoreEssentials.Tests.csproj` gained a `ProjectReference` to `CoreEssentials.Playground` so playground components are unit-testable. CoreEssentials marks its DesktopGL MonoGame package `PrivateAssets="All"`, so no transitive assembly conflict with the Playground's WindowsDX flavor — builds clean.
- **CameraInputComponent (Batch B):** input-only layer over the built-in `CameraComponent` on the same entity — WASD pans the owner (the camera follows via the component's late-update sync), Q/E zoom, R resets. All keys + speeds are declarative props. `IsKeyHeld(Keys)` is a virtual seam so tests simulate input; pan/reset logic runs for real in tests (no camera → position-only assertions).
- **PingControlComponent (Batch B):** single-entity driver for the SendMessage demo commands (broadcast / prefab spawn / typed spawn / destroy-last), tracking the last-spawned entity. Each command is a virtual seam (`Broadcast`, `SpawnPrefab`, `SpawnTyped`, `DestroyLast`) so tests observe without loading demo assets. Key dispatch compares against configurable keys (not hardcoded case labels).
- **Playground, not Core:** these are demo behaviors. If one proves general later it can be promoted to `Components/BuiltIn` in a separate change.
- **Key enum as XML property:** components take keys as strings (e.g. `"Q"`) and parse via `Enum.Parse<Keys>` so they're settable from flat attributes / `<Properties>`.
- **PhysicsSpawn is the biggest piece** — it must reproduce random positions, per-ball collision filters resolved from `PhysicsConfig`, and impulses declaratively.
- **No scene subclasses touched in this sprint** — components are added + tested; wiring them into scenes is Sprint 5.

---
*Created: 2026-09-01 | Part of Scene-as-Data Project*
