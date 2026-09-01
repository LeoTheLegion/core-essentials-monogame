# Sprint 5 — Migrate Playground Scenes to Data-Driven 🎬

**Points:** 5 | **Status:** Not Started | **Goal:** Every playground demo scene runs from an XML file + its prefab assets — no per-scene C# subclass.

## Why This Sprint

With the prefab format locked (Sprint 3) and behavior as components (Sprint 4), each demo scene becomes a data file: `<Scene>` → `<GameSystems>` → `<System Type="EntitySystem">` owning prefab registrations + entity definitions, with behavior attached as components. `Program.cs` then starts purely from files.

## Tasks

- [ ] T1 ⭐ Rewrite `CharacterScene.xml`, `SendMessageDemoScene.xml`, `GuiAnchorDemo.xml` in the strict new format (add `<GameSystems><System>` wrapper + prefab registrations; convert old `<Template Source=>` to `EntityDefinition Source=`)
- [ ] T2 ⭐ New data files for scenes that were C#-only: `LabelAlignmentDemoScene` content, `CameraScene`, and a physics scene file (balls/VIP/border via `PhysicsSpawnComponent`)
- [ ] T3 ⭐ Attach Sprint 4 behavior components in the XML (keys, audio, debug, camera input, ping controls, physics spawn, save/load, physics debug overlay)
- [ ] T4 ⭐ New data-driven loading screen asset (`loading.xml`) using `TransitionProgressComponent`
- [ ] T5 ⭐ `Program.cs`: `SetLoadingScene("loading.xml")` + `LoadScene("<first scene>.xml")` string overloads — no `new LoadingScene(...)`, no scene subclass instances
- [ ] T6 🔒 Delete the now-dead C# scene subclasses (`CharacterScene`, `SendMessageDemoScene`, `GuiAnchorDemoScene`, `LabelAlignmentDemoScene`, `CameraScene`, `PhysicsEntityScene`) **and** any code that referenced them (navigation targets become XML asset names)
- [ ] T7 ⭐ Update `Content.mgcb` to include the new/renamed scene + prefab assets
- [ ] T8 🔁 Integration tests: each migrated scene parses and loads as a `DataDrivenScene`; transition through the data loading screen completes. Manual game crash-check (run the playground)

## Acceptance Criteria

- No scene requires a C# subclass in the playground reference examples
- The game boots from files only (`Program.cs` has no scene class references)
- Build clean, all tests passing; running the game shows each demo working with its controls intact

## Notes & Risks

- **Navigation targets:** `NavigateOnKeyComponent` needs a way to name a target scene. Since scenes are now files, navigation should be key → *scene asset name* (string), resolved via `SceneManager.LoadScene(string)`. Confirm the component's property is an asset-name string, not a `Type`, so no C# references remain.
- **LabelAlignmentDemo per-frame loop:** its orbit + HUD refresh + debug overlay were in a scene coroutine. Port to a component (`OrbitPanelComponent` / debug overlay) or accept it as the one remaining thin subclass — decide during implementation and record the choice here.
- **Screen size:** set once in `Program.cs` (per developer decision); drop per-scene backbuffer resizes.
- **This is the risky sprint** — it's where "does the game actually still work" gets proven. Keep each scene migration small and run the playground after each.

---
*Created: 2026-09-01 | Part of Scene-as-Data Project*
