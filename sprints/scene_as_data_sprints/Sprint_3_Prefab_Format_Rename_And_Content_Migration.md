# Sprint 3 — Prefab Format Rename & Content Migration 🏷️

**Points:** 2 | **Status:** ✅ Done (2026-09-01) | **Goal:** The prefab file format's root element becomes `<Prefab>`; all content and test fixtures follow.

## Why This Sprint First

Every later sprint (scene migration, data-driven startup) reads prefabs through `EntityPrefabLoader.LoadFromXml`, which still requires an `<EntityTemplate>` root. Locking the new root now means Sprints 5–6 write content in the final format from day one — no second migration pass.

## Tasks

- [x] T1 ⭐ `EntityPrefabLoader` accepts `<Prefab>` as the required root (and nested `<Prefab>` children); update all error messages to say "Prefab". This is a breaking release, so `<EntityTemplate>` is **not** kept as an accepted XML root (the `[Obsolete]` `EntityTemplate` C# alias from Sprint 0 stays — that's the API shim, not the file format)
- [x] T2 ⭐ Migrate every playground prefab content file's root to `<Prefab>`: `BallTemplate.xml`, `CharacterTemplate.xml`, `PingPrefabTemplate.xml`, `SoundButtonTemplate.xml`, `TextTemplate.xml`, `VolumeButtonTemplate.xml` (filenames intentionally kept — they're asset names referenced in code; a rename is out of scope)
- [x] T3 🔒 Update test fixtures that write an `<EntityTemplate>` root: `SendMessageTests`, `EntityTemplateTests`, `PrefabRegistrationTests`, `SceneParserTests`, `DataDrivenSceneTests`
- [x] T4 🔁 Build + full suite green

## Acceptance Criteria

- `LoadFromXml` rejects a non-`<Prefab>` root with a message naming `<Prefab>`
- All playground prefab content parses under the new root
- Build clean, all tests passing

## Notes & Risks

- **Scope guard:** this sprint touches only the *prefab file format* (root element). Scene files still use the old flat `<Scene>` at this point — that's Sprint 6. Do not start migrating scene files here.
- **Filenames stay:** `*Template.xml` names are asset references in code + `Content.mgcb`; renaming them is churn with no behavioral gain and is deferred (possibly never).
- **Nested children:** `ParseChildren` iterates `Elements("EntityTemplate")` — must become `Elements("Prefab")`.

## Notes

- **Loader:** `EntityPrefabLoader.LoadFromXml` now requires a `<Prefab>` root (`"Root element must be 'Prefab'."`), `ParseTemplateElement`'s nested error says "Nested Prefab", `ParseChildren` iterates `Elements("Prefab")`, and the bind wrapper element is renamed to `<Prefab>`. Class doc updated from "EntityTemplates" to "prefabs". The `[Obsolete]` `EntityTemplate : Prefab` C# alias is untouched (API shim, not file format).
- **Docs in code:** `EntitySystem.RegisterPrefab(string,string)` and the obsolete `RegisterTemplate(string,string)` param docs now reference `<Prefab>`.
- **Content:** all six playground prefab files (`Ball/Character/PingPrefab/SoundButton/Text/VolumeButton` `*Template.xml`) migrated to a `<Prefab>` root. Filenames intentionally unchanged (asset names referenced in code + `Content.mgcb`).
- **Tests:** fixtures updated in `EntityTemplateTests`, `SendMessageTests`, `PrefabRegistrationTests`, `SceneParserTests`, `DataDrivenSceneTests`. One slip during editing left a stray quote in `SendMessageTests` (`</Prefab>""));`) — caught by the build, fixed.
- **Result:** full suite **1035 passed / 0 failed / 3 skipped** (unchanged from Sprint 2 baseline — this sprint is a rename, no behavior change).

---
*Created: 2026-09-01 | Part of Scene-as-Data Project*
