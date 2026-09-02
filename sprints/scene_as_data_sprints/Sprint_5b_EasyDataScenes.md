# Sprint 5b — Easy Data Scenes (GuiAnchor + SendMessage) 🎯

**Points:** 3 | **Status:** Done | **Goal:** Migrate the two simplest demo scenes to strict-format data files with their behavior as components, and delete their now-dead C# subclasses.

## Why This Sprint

With booting proven in 5a, this sprint migrates the two least-complex scenes — `GuiAnchorDemo` (HUD is already almost fully data-driven) and `SendMessageDemo` (ping/prefab controls). They exercise navigation-to-file, prefab registration from a template asset, and key-driven behavior components without any per-frame coroutines or custom draw code.

## Tasks

- [x] T1 ⭐ Rewrite `GuiAnchorDemo.xml` in the strict format: wrap existing content in `<Scene><GameSystems><System Type="EntitySystem">`, register any prefabs it uses, convert old `<Template Source=>` to `EntityDefinition Source=`, and attach Sprint 4 behavior components (navigation keys, score command wiring stays declarative).
- [x] T2 ⭐ Rewrite `SendMessageDemoScene.xml` in the strict format: wrap content, register `PingPrefab` from `PingPrefabTemplate.xml`, convert entity definitions, and attach key-driven components for Space (broadcast "OnPing"), P (staggered prefab spawn), B (typed spawn), D (destroy last), Esc (navigate to Character scene file).
- [x] T3 ⭐ Verify navigation targets are **scene asset-name strings** resolved via `SceneManager.LoadScene(string)` — confirm the navigate component's property is a string, not a `Type`, so no C# references remain.
- [x] T4 🔒 Delete the now-dead subclasses `GuiAnchorDemoScene.cs` and `SendMessageDemoScene.cs` (and any code referencing them).
- [x] T5 ⭐ Update `Content.mgcb` if asset names changed; ensure both new scene files are staged with `/copy:`.
- [x] 🔁 Integration tests: each migrated scene parses in the strict format and loads as a `DataDrivenScene`; navigation between them resolves to file names. Manual crash-check by running the playground.

## Acceptance Criteria

- `GuiAnchorDemo` and `SendMessageDemo` run entirely from XML + prefab assets; their C# subclasses are deleted.
- Navigation between these scenes (and out to Character) works via scene asset-name strings.
- Build clean, all tests passing; the two demos work with controls intact when running the playground.

## Notes & Risks

- **SendMessageDemo ping stagger:** the staggering was already a per-key counter in the old scene, not a coroutine. It is carried by `PingControlComponent` (Sprint 4 key-driven component): each spawn increments an internal counter and offsets the position by `(count % 5) * 80` from the declarative `SpawnPosition`. No new component was needed.
- **Camera:** `GuiAnchorDemo.xml` now declares a `CoreEssentials.Playground.CameraEntity` with `<EntityOverrides><Property Name="CameraSpeed" Value="300"/></EntityOverrides>`; the entity's built-in input layer handles WASD/Q/E/R. This exercises the entity-property override feature from the prior commit.
- **Navigation:** both scenes navigate out via `NavigateOnKeyComponent` with string asset targets (`PhysicsEntityScene.xml`, `SendMessageDemoScene.xml`, `CharacterScene.xml`). `CharacterScene.cs` and `LabelAlignmentDemoScene.cs` (still C# until Sprint 5d) were switched to `SceneManager.LoadScene("SendMessageDemoScene.xml")` so the code compiles after deletion.
- Keep each migration isolated and run the playground after both land.

---
*Created: 2026-09-01 | Part of Scene-as-Data Project (Sprint 5 split)*
