# Sprint 1 — Code Organization 📦

**Points:** 5 | **Status:** ✅ Done | **Goal:** Move the playground's 27 flat C# files into responsibility-based folders with matching namespaces, update every reference (usings, XML `Type=` strings, test assertions), and remove confirmed-dead code. Zero behavior change.

> **Branch:** landed directly on `feature/scene-as-data`. Lands before Sprint 2 so content moves start from a clean, organized codebase.

## Why This Sprint

A flat folder of 27 files mixes entities, components, and entry-point plumbing with no way to tell what is a scene building block vs. a debug overlay. Folder + namespace organization makes the playground as navigable as the core library it demonstrates — and keeps "where do new things go" obvious for future sprints.

## Target Folders & Namespaces

| Folder | Namespace | Files |
|--------|-----------|-------|
| `Entities/` | `CoreEssentials.Playground.Entities` | Ball, PlayerEntity, CharacterEntity, AnimatedCharacterEntity, CameraEntity, TextEntity, SoundButtonEntity, VolumeButtonEntity, WorldBorder |
| `Components/` | `CoreEssentials.Playground.Components` | CameraInputComponent, CameraFollowToggleComponent, PhysicsSpawnComponent, PhysicsDebugOverlayComponent, HudLabelRefreshComponent, SaveLoadButtonsComponent, OrbitPanelComponent, LabelAlignmentDebugOverlayComponent, MusicComponent, SoundKeyComponent, VolumeKeyComponent, DebugToggleComponent, NavigateOnKeyComponent, PingControlComponent, PingReceiverComponent, ScoreKeeperComponent |
| ~~`Scenes/`~~ | — | `XmlLayoutScene` was confirmed dead and deleted (see T3) |

Stays at project root: `Program.cs` (top-level, no namespace), `SceneLaunchOptionsParser.cs`.

## Tasks

- [x] T1 🔁 Move entity files into `Entities/` and set their namespace to `CoreEssentials.Playground.Entities`; add the new using wherever they are referenced.
- [x] T2 🔁 Move component files into `Components/` and set their namespace to `CoreEssentials.Playground.Components`; update referencing usings.
- [x] T3 🔁 Decide `XmlLayoutScene`: it is referenced by no scene XML and no C# code — confirm dead (grep + tests) and delete, or move to `Scenes/` if retained.
- [x] T4 🔁 Update all fully-qualified XML `Type=` references to the new namespaces: prefab templates (`BallTemplate.xml`, `CharacterTemplate.xml`, `SoundButtonTemplate.xml`, `TextTemplate.xml`, `VolumeButtonTemplate.xml`) and scene files (`CameraScene.xml`, `CharacterScene.xml`, `GuiAnchorDemo.xml`, `LabelAlignmentDemoScene.xml`, …).
- [x] T5 🔁 Update test references that assert fully-qualified type strings (e.g. `Sprint5dDataSceneTests` compares `"CoreEssentials.Playground.CharacterEntity"` / `"CoreEssentials.Playground.CameraEntity"`) and any test XML fixtures copied from playground content.
- [x] T6 🔒 Build clean + full suite green; smoke-run all 7 scenes via the harness — same PASS list as before the move.

## Acceptance Criteria

- No C# file remains at the project root except `Program.cs` and `SceneLaunchOptionsParser.cs`.
- Every moved type's namespace matches its folder; no file references another playground type without an explicit using.
- All XML `Type=` strings resolve (scene boot proves it — harness PASS for every scene).
- Full suite green; no behavior change beyond file/namespace locations.

## Notes & Risks

- **Fully-qualified XML references are the main blast radius** — the scene parser resolves `Type=` via reflection across loaded assemblies, so a stale string fails at scene load, not compile time. The harness smoke-run is the safety net (T6).
- **Test type-string assertions** compare exact FQNs; they must move in lockstep with the namespaces (T5) or they fail on string mismatch, which is easy to miss if only the build is checked.
- Keep each move + its reference updates in a single commit so a broken state never lands.

## ✅ Completion Notes

- **Folders/namespaces:** 9 entities → `Entities/` (`CoreEssentials.Playground.Entities`), 16 components → `Components/` (`CoreEssentials.Playground.Components`). `Program.cs` and `SceneLaunchOptionsParser.cs` remain at the project root. Two cross-folder usings were needed: `CameraFollowToggleComponent` and `PhysicsSpawnComponent` now import `CoreEssentials.Playground.Entities`.
- **T3 — `XmlLayoutScene` deleted:** grep confirmed its only reference was itself (no scene XML, no C#, no tests) — removed rather than moved, so the `Scenes/` folder was not created.
- **T4 — FQN remap rationale:** the prefab loader resolves `Type=` by trying the full name first and then falling back to short-name matching. A stale FQN would therefore *not* be caught at compile time — it would fail scene boot at runtime (or, worse for short names, resolve to a different type). All 11 affected content XMLs were remapped and verified to contain zero old-style FQNs.
- **T5 — test files touched:** added `Entities`/`Components` usings in `PlaygroundBehaviorComponentTests`, `Sprint5dComponentTests`, `Sprint5bDataSceneTests`, `Sprint5cPhysicsDataSceneTests`, `Sprint5dDataSceneTests`; fixed 4 FQN string assertions in `Sprint5dDataSceneTests` (CharacterEntity, CameraEntity, PlayerEntity, TextEntity).
- **T6 — verification:** build clean; full suite **1174 passed / 0 failed / 3 skipped** (identical to pre-move baseline); harness smoke-run: **all 7 scenes PASS**.
- **Deferred to Sprint 2:** `PhysicsScene_Save.xml` (repo root + playground copy) still contains 8 old-style FQN references — it is a runtime save artifact, not part of the content pipeline; relocated/handled with the content moves.
