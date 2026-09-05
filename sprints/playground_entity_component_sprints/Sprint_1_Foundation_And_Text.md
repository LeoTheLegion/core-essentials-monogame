# Sprint 1 — Foundation & Text 🧱

**Points:** 5 | **Status:** ⬜ Not Started | **Goal:** Point every text declaration at the framework's existing `GameObjectEntity` with a new `TextComponent : IDrawableComponent`, then migrate and delete `TextEntity`. This proves the end-to-end "components only" pattern (new component → XML declaration → class deleted) that Sprints 2–4 will scale.

## Why This Sprint First

`Entity` is **abstract**, so every scene/prefab `<Type=...>` must resolve to a *concrete*, instantiable type. The framework already ships exactly the shared concrete entity we need — `CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.GameObjectEntity` ("a basic, behavior-free entity — the equivalent of Unity's plain GameObject"), which all 7 scenes already use for shell entities. Per the confirmed decision (no new `Entity` subclasses), this sprint reuses it rather than creating a playground-local duplicate. `TextEntity` is the ideal first migration: it is self-contained (no inheritance, no save/load), its only behavior is custom text rendering in `Render()`, and that maps cleanly onto a new `IDrawableComponent` — which the base `Entity.Render()` already auto-draws. Finishing this sprint deletes a real class end-to-end with zero behavior change, de-risking the rest of the project.

## Target Outcome

- Reuse the framework's existing `GameObjectEntity` (`CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.GameObjectEntity`) as the shared concrete entity — no new class.
- New `TextComponent : EntityComponent, IDrawableComponent` in `CoreEssentials.Playground.Components` owning font load + aligned text drawing.
- `TextTemplate.xml` + every scene `<EntityDefinition Type=...TextEntity>` repointed at `GameObjectEntity`, with a `<Component Type="TextComponent">` carrying the text/color/alignment properties.
- `Entities/TextEntity.cs` deleted.

## Tasks

- [x] T1 ✅ Reuse the framework's existing `GameObjectEntity` (already in `CoreEssentials/src/GameSystems/EntitySystems/EntityOOPSystem/GameObjectEntity.cs`) — no new entity class is created. No test needed beyond what Sprints' scene tests already cover.
- [ ] T2 ⭐ Create `TextComponent : EntityComponent, IDrawableComponent` — move font load (`AssetManager.LoadAsset<FontAsset>("Fonts/base")`) into `OnAttach`, and the alignment-aware `DrawString` from `TextEntity.Render` into `Draw(SpriteBatch)`. Expose `Text`, `Color`, `Alignment` (and the internal offset) as settable properties so XML `<Property>` values bind.
- [ ] T3 🔁 Update `Content/Templates/TextTemplate.xml`: root `<Prefab Type=...GameObjectEntity>`, add a `<Component Type="TextComponent">` block with the text/color/alignment properties currently baked into `TextEntity`.
- [ ] T4 🔁 Repoint every scene that declares a `TextEntity` (e.g. `CameraScene.xml` `cameraInfoText`) to `GameObjectEntity` + a `TextComponent` with matching per-instance overrides.
- [ ] T5 🔁 Delete `Entities/TextEntity.cs`. Remove any now-unused references/imports.
- [ ] T6 🔒 Add unit tests for `TextComponent` (font loads on attach; alignment math for Left/Center/Right matches the old `TextEntity.Render`).
- [ ] T7 🔒 Build clean + full suite green (expect 1174/0/3) + smoke-run all 7 scenes PASS.

## Acceptance Criteria

- No code references `TextEntity`; the type is fully removed.
- A text entity declared purely from XML (`GameObjectEntity` + `TextComponent`) renders identically to before (same font, color, alignment).
- Full suite green; all 7 scenes smoke-run PASS.

## Deliverables

| File | Action | Purpose |
|------|--------|---------|
| `Components/TextComponent.cs` | Create | Drawable text rendering component |
| `Content/Templates/TextTemplate.xml` | Modify | Point at `GameObjectEntity`, declare `TextComponent` |
| `Content/Scenes/CameraScene.xml` (and any others) | Modify | Repoint `TextEntity` refs to `GameObjectEntity` + `TextComponent` |
| `Entities/TextEntity.cs` | Delete | Behavior now in `TextComponent` |
| `Components/CameraFollowToggleComponent.cs` | Modify | Info-label cast: `TextEntity` → lookup of `TextComponent` |
| `CoreEssentials.Tests/**` | Add/Modify | `TextComponent` tests; adjust any `TextEntity` references |

## Notes & Risks

- **Property binding for `Alignment` (enum):** confirm the component property binder parses enum strings (e.g. `"Center"`) — check how other components bind enums before relying on it; if not supported, accept a string and map internally.
- **Font asset name:** keep `"Fonts/base"` exactly as-is (it moved in Organization Sprint 2). Do not regress to the old root-level key.
- **`TextEntity.TextAlignment` enum** is currently nested in the class — move it to `TextComponent` (or a shared location) so XML/properties can reference it.
- Keep the smoke run as the behavioral gate: text appears on CameraScene and CharacterScene HUDs.

---
*Created: 2026-09-05 | Part of Playground Entity → Components Project*
