# Sprint 7 — Docs, Version Bump & Release 📦

**Points:** 2 | **Status:** ✅ Done | **Goal:** Document the new format + prefab terminology, bump to 0.20.0, and open the PR to `development`.

## Why This Sprint Last

Docs reference the final API surface, file names, and schema — all of which only stabilize after Sprints 3–6 land. The version bump and PR close out the effort (issues #92 scene-as-data, #84 prefab auto-registration, #81 per-instance overrides).

## Tasks

- [x] T1 ⭐ New `docs/Prefabs.md` (replaces `EntityTemplates.md`): prefab terminology, `<Prefab>` file format, `RegisterPrefab`/`HasPrefab`/lazy `InstantiateFromAsset`, per-instantiation overrides. Fix the stale `EntityTemplateLoader` references in the old doc (lines ~36/44/305/318) as part of the replacement
- [x] T2 ⭐ New `docs/SceneAsData.md`: the strict `<Scene>` schema, `Type=` vs `Source=`, flat + precise overrides, binds/references, `DataDrivenScene`, string `LoadScene`/`SetLoadingScene` overloads, the data-driven loading screen + `TransitionProgressComponent`, and a **breaking-change migration note** (old flat `<Scene>` removed, `<EntityTemplate>` root renamed)
- [x] T3 ⭐ Update `docs/SceneManagement.md` (data-driven scenes, string overloads) and the `docs/README.md` index (swap Entity Templates → Prefabs; add Scene-as-Data)
- [x] T4 ⭐ Fix remaining cross-doc references to removed APIs / old schema (`docs/EntitySystem.md`, `docs/XMLEntityDefinitions.md`, `docs/SendMessage.md`, `docs/GameStateSerialization.md`)
- [x] T5 ⭐ Version bump 0.19.x → **0.20.0** (breaking changes) in the package/project metadata
- [x] T6 🔁 Build + full suite green; run the game crash-check one final time
- [x] T7 ⭐ Push `feature/scene-as-data` and open a PR to `development` — "Closes #92", "Closes #84", "Closes #81" go in the **PR body only** (never in code/comments/docs)

## Acceptance Criteria

- All docs reflect prefab terminology and the new strict schema; no dangling links to removed files/APIs
- Package version is 0.20.0
- Build clean, all tests passing
- PR open against `development` with the three issue references in the body

## Notes & Risks

- **No issue numbers in code or docs** — repo convention; they belong only in the commit/PR text.
- **Sprint folder is throwaway:** `sprints/scene_as_data_sprints/` is discarded after merge (per its README). Do not link to it from `docs/`.
- **Migration note matters most:** users upgrading 0.19 → 0.20 need a clear "what broke and how to fix it" section (old flat scene XML, `<EntityTemplate>` root, removed `LoadEntitiesFromXml`).

## Completion Notes

- **T1** — New `docs/Prefabs.md`: prefab model (`Prefab`/`ComponentDefinition`), full API reference (`RegisterPrefab` ×2, `HasPrefab`/`TryGetPrefab`, lazy `InstantiateFromAsset`, `Instantiate` + per-instantiation overrides, `InstantiatePrefab` convenience methods), `<Prefab>` XML schema with attribute tables and examples, `EntityPrefabLoader.LoadFromXml`, and a "Migration from Entity Templates" table. Replaces the old doc — `docs/EntityTemplates.md` deleted (no remaining in-docs links; only throwaway sprint docs mentioned it).
- **T2** — New `docs/SceneAsData.md`: mental model, strict `<Scene>`/`<GameSystems>`/`<System>` anatomy, prefab registration, `Type=` vs `Source=`, hierarchy with pre-order attach note, the three override forms (flat / precise `<Overrides>` / `<EntityOverrides>`) with value-parsing rules, binds + references, `DataDrivenScene` + string overloads, the data-driven loading screen (`loading.xml` + `TransitionProgressComponent` + `Program.cs` boot), a complete example, and the **Breaking Changes (0.19 → 0.20)** section: (1) flat loader removed with an old→new XML rewrite example, (2) `<EntityTemplate>`→`<Prefab>` root rename, (3) API rename table, plus "What did not change" (single-entity serialization, SaveState/LoadState, component discovery).
- **T3** — `docs/SceneManagement.md`: data-driven section now documents the string `SetLoadingScene`/`LoadScene` overloads and links to `SceneAsData.md`. `docs/README.md`: version 0.18.0 → 0.20.0; index swaps Entity Templates → Prefabs and adds Scene-as-Data under Scene Management; Playground Examples list corrected (default launch is now the data-driven `HomeScene.xml` after a data-driven `loading.xml`).
- **T4** — Cross-doc fixes: `docs/XMLEntityDefinitions.md` (Quick Start, "Loading Scenes", strict scene schema section + `Source=` attribute row, menu example load call, custom-components examples re-pointed to single-entity loading, Complete Example rewritten to the strict format with `SceneManager.LoadScene` + `FindById`); `docs/EntitySystem.md` and `docs/GameStateSerialization.md` (template links → Prefabs); `docs/SendMessage.md` (`InstantiateTemplate` → `InstantiatePrefab`, link → Prefabs). Final grep across `docs/` for removed APIs is clean — the only remaining mentions are the intentional old→new tables inside `Prefabs.md` and `SceneAsData.md`.
- **T5** — `CoreEssentials.csproj` `<Version>` 0.19.1 → **0.20.0** (minor bump: breaking changes, new features).
- **T6** — Build green (`Build succeeded`, 0 errors); full suite **1098 passed / 0 failed / 3 skipped (Total 1101)** — unchanged from Sprint 6, as expected for docs-only changes.
- **T7** — Branch `feature/scene-as-data` pushed; PR opened to `development` with the three issue references in the PR body only (none in code, comments, or docs). Docs + version commit: `dff66de`.

---
*Created: 2026-09-01 | Part of Scene-as-Data Project*
