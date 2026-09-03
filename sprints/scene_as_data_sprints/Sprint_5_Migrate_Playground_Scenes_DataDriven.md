# Sprint 5 — Migrate Playground Scenes to Data-Driven 🎬

**Points:** 13 (split) | **Status:** Done ✅ (all sub-sprints landed) | **Goal:** Every playground demo scene runs from an XML file + its prefab assets — no per-scene C# subclass.

> **SPLIT:** This sprint was too large and high-risk to land in one go. It is tracked as four ordered sub-sprints, all complete:
> - [x] **5a** Foundation: data-driven booting (`loading.xml` + placeholder first scene, `Program.cs` from files) → `Sprint_5a_Foundation_DataDrivenBooting.md`
> - [x] **5b** Easy data scenes (GuiAnchor + SendMessage) → `Sprint_5b_EasyDataScenes.md`
> - [x] **5c** Physics scene → `Sprint_5c_PhysicsScene.md`
> - [x] **5d** Hard scenes (Character / Camera / LabelAlignment) — entity-property gap closed via components + additive knobs → `Sprint_5d_HardScenes_EntityPropertyGap.md`
>
> The tasks below are retained for reference only; work is tracked in the sub-sprints.

## Why This Sprint

With the prefab format locked (Sprint 3) and behavior as components (Sprint 4), each demo scene becomes a data file: `<Scene>` → `<GameSystems>` → `<System Type="EntitySystem">` owning prefab registrations + entity definitions, with behavior attached as components. `Program.cs` then starts purely from files.

## Tasks

All tasks landed across the four sub-sprints (see their docs for detail):

- [x] T1 ⭐ Rewrite `CharacterScene.xml`, `SendMessageDemoScene.xml`, `GuiAnchorDemo.xml` in the strict new format (add `<GameSystems><System>` wrapper + prefab registrations; convert old `<Template Source=>` to `EntityDefinition Source=`) — 5b/5d
- [x] T2 ⭐ New data files for scenes that were C#-only: `LabelAlignmentDemoScene` content, `CameraScene`, and a physics scene file (balls/VIP/border via `PhysicsSpawnComponent`) — 5c/5d
- [x] T3 ⭐ Attach Sprint 4 behavior components in the XML (keys, audio, debug, camera input, ping controls, physics spawn, save/load, physics debug overlay) — 5b/5c/5d
- [x] T4 ⭐ New data-driven loading screen asset (`loading.xml`) using `TransitionProgressComponent` — 5a
- [x] T5 ⭐ `Program.cs`: `SetLoadingScene("loading.xml")` + `LoadScene("<first scene>.xml")` string overloads — no `new LoadingScene(...)`, no scene subclass instances — 5a
- [x] T6 🔒 Delete the now-dead C# scene subclasses (`CharacterScene`, `SendMessageDemoScene`, `GuiAnchorDemoScene`, `LabelAlignmentDemoScene`, `CameraScene`, `PhysicsEntityScene`) **and** any code that referenced them (navigation targets become XML asset names) — 5b/5c/5d
- [x] T7 ⭐ Update `Content.mgcb` to include the new/renamed scene + prefab assets — 5a/5b/5c/5d
- [x] T8 🔁 Integration tests: each migrated scene parses and loads as a `DataDrivenScene`; transition through the data loading screen completes. Manual game crash-check (run the playground) — 5a/5b/5c/5d

## ✅ Completion Notes (all sub-sprints landed)

Every playground demo now runs from an XML file + prefab assets with no per-scene C# subclass:
- **5a** (`loading.xml`, `Program.cs` from files) → data-driven boot proven.
- **5b** (`GuiAnchorDemo.xml`, `SendMessageDemoScene.xml`) → easy scenes migrated, subclasses deleted.
- **5c** (`PhysicsEntityScene.xml`) → physics scene via Sprint 4 components, subclass deleted.
- **5d** (`CharacterScene.xml`, `CameraScene.xml`, `LabelAlignmentDemoScene.xml`) → hard scenes migrated; per-frame loops/draw ported to new components (`OrbitPanelComponent`, `CameraFollowToggleComponent`, `LabelAlignmentDebugOverlayComponent`, `HudLabelRefreshComponent`); entity-property gap closed with additive knobs (no breaking API change); all remaining subclasses deleted.

The LabelAlignmentDemo decision from the risks note: **ported to components** (not kept as a thin subclass). Full per-scene detail lives in each sub-sprint doc.

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
