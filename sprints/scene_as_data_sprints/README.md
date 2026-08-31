# Scene-as-Data — Scrum Sprints 🚀

Closes #92 (scene-as-data), #84 (template auto-registration), and #81 (per-instance property overrides) in one effort. Temporary folder — discarded after merge.

## Why This?

Every scene needs a C# subclass just to declare its game systems, register prefabs, and point at an entity XML file. This project makes the scene file fully self-describing:

- `<Scene>` → `<GameSystems>` → each `<System>` owns its own data (prefab registrations + entities).
- `EntityDefinition` is the only entity element: `Type=` = plain class, `Source=` = prefab instance, with optional property overrides.
- Terminology adopts Unity's **prefab** (was "template") across API, XML, and docs.

Breaking changes are accepted — early library. No backwards-compatibility shims beyond `[Obsolete]` API aliases for one release.

## Sprint Roadmap

| Sprint | Name | Points | Status | Description |
|--------|------|--------|--------|-------------|
| 0 | [Prefab Registration & Overrides](Sprint_0_Prefab_Registration_And_Overrides.md) | 5 | In Progress | `RegisterPrefab`/`HasPrefab`/lazy `Instantiate`, override-merge core + C# overrides overload |
| 1 | [Scene Format Parser](Sprint_1_Scene_Format_Parser.md) | 5 | Not Started | Strict `<Scene>` parser: systems, prefabs, entities, flat + precise overrides |
| 2 | [DataDrivenScene & Loading Screen](Sprint_2_DataDrivenScene_And_Loading_Screen.md) | 5 | Not Started | `DataDrivenScene`, `LoadScene(string)`/`SetLoadingScene(string)`, loading screen as data |
| 3 | [Migration, Docs & Release](Sprint_3_Migration_Docs_Release.md) | 2 | Not Started | Migrate all playground XML to new format, docs, version bump |

## Point Summary

- Total: 17 points (4 sprints)
- Sizing: 1 = small, 2 = medium, 5 = large

## Workflow Phases

Foundation (Sprint 0) → Core Implementation (Sprints 1–2) → Migration & Quality Gate (Sprint 3)
