# Scene-as-Data — Scrum Sprints 🚀

One effort covering scene-as-data, prefab auto-registration, and per-instance property overrides. Temporary folder — discarded after merge.

## Why This?

Every scene needs a C# subclass just to declare its game systems, register prefabs, and point at an entity XML file. This project makes the scene file fully self-describing:

- `<Scene>` → `<GameSystems>` → each `<System>` owns its own data (prefab registrations + entities).
- `EntityDefinition` is the only entity element: `Type=` = plain class, `Source=` = prefab instance, with optional property overrides.
- Terminology adopts Unity's **prefab** (was "template") across API, XML, and docs.

Breaking changes are accepted — early library. No backwards-compatibility shims beyond `[Obsolete]` API aliases for one release.

## Sprint Roadmap

| Sprint | Name | Points | Status | Description |
|--------|------|--------|--------|-------------|
| 0 | [Prefab Registration & Overrides](Sprint_0_Prefab_Registration_And_Overrides.md) | 5 | ✅ Done | `RegisterPrefab`/`HasPrefab`/lazy `InstantiateFromAsset`, override-merge core + C# overrides overload |
| 1 | [Scene Format Parser](Sprint_1_Scene_Format_Parser.md) | 5 | ✅ Done | Strict `<Scene>` parser: systems, prefabs, entities, flat + precise overrides |
| 2 | [DataDrivenScene & Loading Screen](Sprint_2_DataDrivenScene_And_Loading_Screen.md) | 5 | ✅ Done (2026-08-31) | `DataDrivenScene`, `LoadScene(string)`/`SetLoadingScene(string)`, loading screen as data |
| 3 | [Prefab Format Rename & Content Migration](Sprint_3_Prefab_Format_Rename_And_Content_Migration.md) | 2 | ✅ Done (2026-09-01) | `<EntityTemplate>`→`<Prefab>` root; migrate content + fixtures |
| 4 | [Playground Behavior Components](Sprint_4_Playground_Behavior_Components.md) | 5 | ✅ Done | Per-scene runtime behavior (keys/audio/debug/camera/physics) moved into components |
| 5 | [Migrate Playground Scenes Data-Driven](Sprint_5_Migrate_Playground_Scenes_DataDriven.md) | 13 | ✅ Done (2026-09-02) | All six demo scenes run from XML; `Program.cs` boots from files; scene subclasses deleted |
| 6 | [Remove Old Scene-Load Path](Sprint_6_Remove_Old_Scene_Load_Path.md) | 2 | ✅ Done | Legacy flat-`<Scene>` load path removed; strict `SceneParser` + `DataDrivenScene` is the only XML→scene path |
| 7 | [Docs, Version & Release](Sprint_7_Docs_Version_Release.md) | 2 | 🔄 In Progress (T1–T6 done) | `Prefabs.md` + `SceneAsData.md`, version 0.20.0; PR to `development` pending |

> Sprints 3–7 supersede the original single "Migration, Docs & Release" sprint (see [Sprint_3_Migration_Docs_Release.md](Sprint_3_Migration_Docs_Release.md), kept for reference only).

## Point Summary

- Total: 28 points (8 sprints)
- Sizing: 1 = small, 2 = medium, 5 = large

## Workflow Phases

Foundation (Sprint 0) → Core Implementation (Sprints 1–2) → Format Lock (Sprint 3) → Behavior as Components (Sprint 4) → Migration (Sprint 5) → Cleanup & Quality Gate (Sprints 6–7)
