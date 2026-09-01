# Sprint 6 — Remove Old Scene-Load Path 🧹

**Points:** 2 | **Status:** Not Started | **Goal:** Delete the legacy flat-`<Scene>` loading path now that everything runs through the strict parser + `DataDrivenScene`.

## Why This Sprint

Sprint 5 made every playground scene data-driven. The old load path — `EntitySerializer.LoadSceneFromXml` / `Scene.LoadEntitiesFromXml` and its flat `<Scene>`/`<Template Source=>` element parsing — is now dead weight that duplicates the strict parser's job. Per the developer's "remove it" decision (breaking release), it goes away so there is exactly one way to load scene content from XML.

## Tasks

- [ ] T1 ⭐ Remove `Scene.LoadEntitiesFromXml(string)` / `LoadEntitiesFromXml(XMLAsset)` overloads
- [ ] T2 ⭐ Remove the flat-scene parsing path in `EntitySerializer` (`LoadSceneFromXml` + its `<Template Source=>` / flat-entity element handling) — keep only what save/load and prefab loading still need
- [ ] T3 🔒 Update or remove tests that exercised the old path (`EntitySerializerTests.LoadSceneFromXml_*`, `EntitySerializerPreOrderAttachTests`, `CommandBindingTests.LoadSceneFromXml_*`) — re-point them at the strict parser / `DataDrivenScene` where they still assert useful behavior, delete the rest
- [ ] T4 🔒 Grep for any remaining callers of the removed APIs and fix them
- [ ] T5 🔁 Build + full suite green

## Acceptance Criteria

- Exactly one XML→scene path remains (strict `SceneParser` + `DataDrivenScene`)
- No references to the removed overloads compile
- Build clean, all tests passing

## Notes & Risks

- **Careful with save/load:** `EntitySystem.SaveState` / `LoadState` and entity (de)serialization are a *separate* concern from scene loading. Do not remove component/entity serialization — only the flat *scene* element parsing. Verify what `LoadSceneFromXml` shares with save/load before deleting.
- **Prefab loading is untouched:** `EntityPrefabLoader.LoadFromXml` (the `<Prefab>` root) stays — it's the prefab path, not the scene path.
- **Order matters:** only safe to delete after Sprint 5 has removed every caller.

---
*Created: 2026-09-01 | Part of Scene-as-Data Project*
