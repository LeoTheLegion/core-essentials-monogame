# Sprint 3 — Migration, Docs & Release 📦

> ⚠️ **SUPERSEDED (2026-09-01):** this single sprint was too large once the scope grew to "port all scene behavior into components" and "remove the old load path." It is split into Sprints 3–7 (see [README](README.md)). This file is kept for reference only — do not work from it.

**Points:** 2 | **Status:** ❌ Superseded by Sprints 3–7 | **Goal:** Migrate all content to the new format + docs + version bump.

## Tasks

- [ ] T1 ⭐ Migrate all playground XML scene files to the new `<Scene>` format (required — no compat path)
- [ ] T2 ⭐ Rename prefab file format root `<EntityTemplate>` → `<Prefab>` across Content
- [ ] T3 ⭐ `Program.cs` on data-driven loading scene + first scene (`SetLoadingScene("loading.xml")`, `LoadScene("CharacterScene.xml")`)
- [ ] T4 ⭐ Docs: new `docs/Prefabs.md` (replaces `EntityTemplates.md`), new `docs/SceneAsData.md` (schema, examples, loading screen, breaking-change migration note); update `SceneManagement.md`, `docs/README.md` index
- [ ] T5 ⭐ Version bump 0.19.x → **0.20.0** (breaking changes)
- [ ] T6 🔁 Build + full test suite green; run game crash-check

## Acceptance Criteria

- No scene requires a C# subclass in the playground reference examples
- All docs reflect prefab terminology and the new schema
- Build clean, all tests passing, PR to `development`

---
*Created: 2026-08-31 | Part of Scene-as-Data Project*
