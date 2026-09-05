# Sprint 5c — Physics Scene 🏀

**Points:** 3 | **Status:** Done | **Goal:** Migrate `PhysicsEntityScene` to a strict-format data file using the Sprint 4 physics components, and delete its C# subclass.

## Why This Sprint

The physics scene is self-contained: it spawns balls/VIP/border via `PhysicsSpawnComponent`, wires save/load GUI buttons via `SaveLoadButtonsComponent`, and toggles the debug overlay via `PhysicsDebugOverlayComponent`. All three components already exist from Sprint 4 Batch C, so this sprint is mostly authoring the data file + prefab registrations and deleting the subclass.

## Tasks

- [x] T1 ⭐ Create `PhysicsEntityScene.xml` in the strict format: `<System Type="EntitySystem">` + `<System Type="PhysicsEngine">` (with its config), register `BallPrefab` from `BallTemplate.xml`, and declare entities carrying `PhysicsSpawnComponent` (5 regular + 3 VIP balls, world border), `SaveLoadButtonsComponent`, and `PhysicsDebugOverlayComponent`.
- [x] T2 ⭐ Confirm the physics system's config resolves from `PhysicsConfig.xml` in the strict format; verify category resolution (`Player`/`Vip`) works through `ResolveCategory`.
- [x] T3 🔒 Delete the now-dead subclass `PhysicsEntityScene.cs` (and any code referencing it). Repoint its navigation target (+/OemPlus → Camera scene) to a **scene asset-name string**.
- [x] T4 ⭐ Update `Content.mgcb` to stage `PhysicsEntityScene.xml` with `/copy:`.
- [x] 🔁 Integration tests: the file parses in the strict format and loads as a `DataDrivenScene`; balls/VIP/border spawn with correct categories, scale, colors; save/load buttons wire up; F1 toggles the debug overlay. Manual crash-check by running the playground.

## Acceptance Criteria

- The physics demo runs entirely from XML + prefab/config assets; its C# subclass is deleted.
- Balls, VIP balls, world border, save/load GUI, and F1 debug toggle all work when running the playground.
- Build clean, all tests passing.

## Notes & Risks

- **Screen size:** the old scene set 1280×720 per-scene; that now lives once in `Program.cs` (5a) — drop the per-scene backbuffer resize.
- If any physics component property is missing for a needed knob, add it as a small additive change and record it here.

## Completion Notes

- **No new knobs needed.** Every Sprint 4 physics component default already matched the old scene exactly; the XML spells all properties out anyway so the data file stays self-documenting.
- **System order in the file:** `<System Type="PhysicsEngine" Config="PhysicsConfig.xml"/>` first, then the `EntitySystem`, then a parameterless `<System Type="PhysicsDebugRenderer"/>` last (it resolves its sibling engine lazily on first draw — Gap 3 fix).
- **Navigation repoint:** `CharacterScene.cs` (`+=`/OemPlus) now loads `"PhysicsEntityScene.xml"` as a string; the scene's own `navCamera` shell navigates to `"CameraScene.xml"` (a Sprint 5d target — that file does not exist yet).
- **Tests:** `Sprint5cPhysicsDataSceneTests` (3 tests) — parse assertions for system order + config asset + prefab registration + all component knobs, and a full `DataDrivenScene` load with the real Aether engine: asserts `engine.Config.Gravity == (0,1000)`, `Player`/`Vip` category resolution, 8 spawned `Ball` entities (5 regular + 3 VIP incl. `vip_ball_blue`), a `WorldBorder`, and all three shell components attached.
- **Full suite green after this sprint:** 1097 passed / 0 failed / 3 skipped.

---
*Created: 2026-09-01 | Part of Scene-as-Data Project (Sprint 5 split)*
