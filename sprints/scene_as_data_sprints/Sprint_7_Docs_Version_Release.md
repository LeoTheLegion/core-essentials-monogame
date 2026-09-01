# Sprint 7 — Docs, Version Bump & Release 📦

**Points:** 2 | **Status:** Not Started | **Goal:** Document the new format + prefab terminology, bump to 0.20.0, and open the PR to `development`.

## Why This Sprint Last

Docs reference the final API surface, file names, and schema — all of which only stabilize after Sprints 3–6 land. The version bump and PR close out the effort (issues #92 scene-as-data, #84 prefab auto-registration, #81 per-instance overrides).

## Tasks

- [ ] T1 ⭐ New `docs/Prefabs.md` (replaces `EntityTemplates.md`): prefab terminology, `<Prefab>` file format, `RegisterPrefab`/`HasPrefab`/lazy `InstantiateFromAsset`, per-instantiation overrides. Fix the stale `EntityTemplateLoader` references in the old doc (lines ~36/44/305/318) as part of the replacement
- [ ] T2 ⭐ New `docs/SceneAsData.md`: the strict `<Scene>` schema, `Type=` vs `Source=`, flat + precise overrides, binds/references, `DataDrivenScene`, string `LoadScene`/`SetLoadingScene` overloads, the data-driven loading screen + `TransitionProgressComponent`, and a **breaking-change migration note** (old flat `<Scene>` removed, `<EntityTemplate>` root renamed)
- [ ] T3 ⭐ Update `docs/SceneManagement.md` (data-driven scenes, string overloads) and the `docs/README.md` index (swap Entity Templates → Prefabs; add Scene-as-Data)
- [ ] T4 ⭐ Fix remaining cross-doc references to removed APIs / old schema (`docs/EntitySystem.md`, `docs/XMLEntityDefinitions.md`, `docs/SendMessage.md`, `docs/GameStateSerialization.md`)
- [ ] T5 ⭐ Version bump 0.19.x → **0.20.0** (breaking changes) in the package/project metadata
- [ ] T6 🔁 Build + full suite green; run the game crash-check one final time
- [ ] T7 ⭐ Push `feature/scene-as-data` and open a PR to `development` — "Closes #92", "Closes #84", "Closes #81" go in the **PR body only** (never in code/comments/docs)

## Acceptance Criteria

- All docs reflect prefab terminology and the new strict schema; no dangling links to removed files/APIs
- Package version is 0.20.0
- Build clean, all tests passing
- PR open against `development` with the three issue references in the body

## Notes & Risks

- **No issue numbers in code or docs** — repo convention; they belong only in the commit/PR text.
- **Sprint folder is throwaway:** `sprints/scene_as_data_sprints/` is discarded after merge (per its README). Do not link to it from `docs/`.
- **Migration note matters most:** users upgrading 0.19 → 0.20 need a clear "what broke and how to fix it" section (old flat scene XML, `<EntityTemplate>` root, removed `LoadEntitiesFromXml`).

---
*Created: 2026-09-01 | Part of Scene-as-Data Project*
