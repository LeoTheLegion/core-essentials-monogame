# Sprint 5c — Physics Scene 🏀

**Points:** 3 | **Status:** Not Started | **Goal:** Migrate `PhysicsEntityScene` to a strict-format data file using the Sprint 4 physics components, and delete its C# subclass.

## Why This Sprint

The physics scene is self-contained: it spawns balls/VIP/border via `PhysicsSpawnComponent`, wires save/load GUI buttons via `SaveLoadButtonsComponent`, and toggles the debug overlay via `PhysicsDebugOverlayComponent`. All three components already exist from Sprint 4 Batch C, so this sprint is mostly authoring the data file + prefab registrations and deleting the subclass.

## Tasks

- [ ] T1 ⭐ Create `PhysicsEntityScene.xml` in the strict format: `<System Type="EntitySystem">` + `<System Type="PhysicsEngine">` (with its config), register `BallPrefab` from `BallTemplate.xml`, and declare entities carrying `PhysicsSpawnComponent` (5 regular + 3 VIP balls, world border), `SaveLoadButtonsComponent`, and `PhysicsDebugOverlayComponent`.
- [ ] T2 ⭐ Confirm the physics system's config resolves from `PhysicsConfig.xml` in the strict format; verify category resolution (`Player`/`Vip`) works through `ResolveCategory`.
- [ ] T3 🔒 Delete the now-dead subclass `PhysicsEntityScene.cs` (and any code referencing it). Repoint its navigation target (+/OemPlus → Camera scene) to a **scene asset-name string**.
- [ ] T4 ⭐ Update `Content.mgcb` to stage `PhysicsEntityScene.xml` with `/copy:`.
- [ ] 🔁 Integration tests: the file parses in the strict format and loads as a `DataDrivenScene`; balls/VIP/border spawn with correct categories, scale, colors; save/load buttons wire up; F1 toggles the debug overlay. Manual crash-check by running the playground.

## Acceptance Criteria

- The physics demo runs entirely from XML + prefab/config assets; its C# subclass is deleted.
- Balls, VIP balls, world border, save/load GUI, and F1 debug toggle all work when running the playground.
- Build clean, all tests passing.

## Notes & Risks

- **Screen size:** the old scene set 1280×720 per-scene; that now lives once in `Program.cs` (5a) — drop the per-scene backbuffer resize.
- If any physics component property is missing for a needed knob, add it as a small additive change and record it here.

---
*Created: 2026-09-01 | Part of Scene-as-Data Project (Sprint 5 split)*
