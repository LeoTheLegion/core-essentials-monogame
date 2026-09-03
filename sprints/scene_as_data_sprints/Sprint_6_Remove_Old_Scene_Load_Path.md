# Sprint 6 — Remove Old Scene-Load Path 🧹

**Points:** 2 | **Status:** ✅ Done | **Goal:** Delete the legacy flat-`<Scene>` loading path now that everything runs through the strict parser + `DataDrivenScene`.

## Why This Sprint

Sprint 5 made every playground scene data-driven. The old load path — `EntitySerializer.LoadSceneFromXml` / `Scene.LoadEntitiesFromXml` and its flat `<Scene>`/`<Template Source=>` element parsing — is now dead weight that duplicates the strict parser's job. Per the developer's "remove it" decision (breaking release), it goes away so there is exactly one way to load scene content from XML.

## Tasks

- [x] T1 ⭐ Remove `Scene.LoadEntitiesFromXml(string)` / `LoadEntitiesFromXml(XMLAsset)` overloads
- [x] T2 ⭐ Remove the flat-scene parsing path in `EntitySerializer` (`LoadSceneFromXml` + its `<Template Source=>` / flat-entity element handling) — keep only what save/load and prefab loading still need
- [x] T3 🔒 Update or remove tests that exercised the old path (`EntitySerializerTests.LoadSceneFromXml_*`, `EntitySerializerPreOrderAttachTests`, `CommandBindingTests.LoadSceneFromXml_*`) — re-point them at the strict parser / `DataDrivenScene` where they still assert useful behavior, delete the rest
- [x] T4 🔒 Grep for any remaining callers of the removed APIs and fix them
- [x] T5 🔁 Build + full suite green

## ✅ Completion Notes

The legacy flat-`<Scene>` load path is gone — exactly one XML→scene path remains (strict `SceneParser` + `DataDrivenScene`).

**Removed from `CoreEssentials/src/Scene/Scene.cs`:**
- Both `LoadEntitiesFromXml(string)` / `LoadEntitiesFromXml(XMLAsset)` overloads.

**Removed from `EntitySerializer.cs` (the flat-scene path):**
- Public: `LoadSceneFromFile`, `LoadSceneFromXml`.
- Scene-only private helpers that nothing else reaches: `LoadEntityFromTemplate` (`<Template Source=>` handling), both `LoadEntityFromDefinition` overloads, `AttachComponentsPreOrder`, `AttachComponents`, `GetChildDefinitions`, `ResolveReferences`, `SetReference`, and the now-dead `CreateEntityByTypeName`.
- The unused `EntityElement` const.

**Kept (still used by single-entity + save/load):** `LoadEntity<T>` / `LoadEntityFromFile<T>`, `SaveEntity` / `SaveEntityToString`, `ApplyEntityProperties`, `ParseRootElement`, `CreateEntityDocument`, `ParseVector2`, `LoadComponents`, `LoadComponent`, `ApplyProperties`, `HandleSpecialProperties`, `SetProperty`, and the whole `IComponentFactory` / `DefaultComponentFactory` machinery. Prefab loading (`EntityPrefabLoader.LoadFromXml`) was never touched.

**Tests (T3):**
- Deleted `EntitySerializerPreOrderAttachTests.cs` — all four tests called the removed `LoadSceneFromXml`; the pre-order attach behavior they guarded is now provided by `DataDrivenScene.InstantiateDefinition` and exercised by Sprint 5d's nested canvas/label integration tests.
- `EntitySerializerTests.cs`: deleted the 7 flat-scene-structure tests; **re-pointed** the two "existing-component override" tests at `LoadEntity<EntityWithPreCreatedComponent>` (same shared `LoadComponent` path — preserves the modify-not-duplicate + multi-property assertions).
- `CommandBindingTests.cs`: deleted the 5 tests that called removed `LoadSceneFromXml` (`BindWiring_EndToEnd`, `BindInsideComponentsElement_IsApplied`, `ReferenceOntoComponent_ResolvesTarget`, `CustomFactoryWithBuiltInComponents_AttachesAll`, `DiscoveredComponent_AttachesWithoutRegistration`) plus their now-dead fixtures (`ScoreKeeperComponent`, `LabelLikeEntity`, `ClickCounterEntity`, `KeeperEntity`) and the `CreateTestFactory` helper. All direct `CommandBindings.ApplyBindings` tests and the factory/discovery-only tests remain.

**Verification:** zero `.cs` references to the removed APIs (grep across CoreEssentials / Playground / Tests). Build clean; full suite **1098 passed / 0 failed / 3 skipped** (down from 1114 by exactly the 16 removed old-path tests, 2 re-pointed).

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
