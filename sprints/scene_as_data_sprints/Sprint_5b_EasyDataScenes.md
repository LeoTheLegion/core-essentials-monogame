# Sprint 5b — Easy Data Scenes (GuiAnchor + SendMessage) 🎯

**Points:** 3 | **Status:** Not Started | **Goal:** Migrate the two simplest demo scenes to strict-format data files with their behavior as components, and delete their now-dead C# subclasses.

## Why This Sprint

With booting proven in 5a, this sprint migrates the two least-complex scenes — `GuiAnchorDemo` (HUD is already almost fully data-driven) and `SendMessageDemo` (ping/prefab controls). They exercise navigation-to-file, prefab registration from a template asset, and key-driven behavior components without any per-frame coroutines or custom draw code.

## Tasks

- [ ] T1 ⭐ Rewrite `GuiAnchorDemo.xml` in the strict format: wrap existing content in `<Scene><GameSystems><System Type="EntitySystem">`, register any prefabs it uses, convert old `<Template Source=>` to `EntityDefinition Source=`, and attach Sprint 4 behavior components (navigation keys, score command wiring stays declarative).
- [ ] T2 ⭐ Rewrite `SendMessageDemoScene.xml` in the strict format: wrap content, register `PingPrefab` from `PingPrefabTemplate.xml`, convert entity definitions, and attach key-driven components for Space (broadcast "OnPing"), P (staggered prefab spawn), B (typed spawn), D (destroy last), Esc (navigate to Character scene file).
- [ ] T3 ⭐ Verify navigation targets are **scene asset-name strings** resolved via `SceneManager.LoadScene(string)` — confirm the navigate component's property is a string, not a `Type`, so no C# references remain.
- [ ] T4 🔒 Delete the now-dead subclasses `GuiAnchorDemoScene.cs` and `SendMessageDemoScene.cs` (and any code referencing them).
- [ ] T5 ⭐ Update `Content.mgcb` if asset names changed; ensure both new scene files are staged with `/copy:`.
- [ ] 🔁 Integration tests: each migrated scene parses in the strict format and loads as a `DataDrivenScene`; navigation between them resolves to file names. Manual crash-check by running the playground.

## Acceptance Criteria

- `GuiAnchorDemo` and `SendMessageDemo` run entirely from XML + prefab assets; their C# subclasses are deleted.
- Navigation between these scenes (and out to Character) works via scene asset-name strings.
- Build clean, all tests passing; the two demos work with controls intact when running the playground.

## Notes & Risks

- **SendMessageDemo ping stagger:** if staggering was a coroutine in the old scene, port it to a key-driven component (Sprint 4 pattern) or a small dedicated component; record the choice here.
- Keep each migration isolated and run the playground after both land.

---
*Created: 2026-09-01 | Part of Scene-as-Data Project (Sprint 5 split)*
