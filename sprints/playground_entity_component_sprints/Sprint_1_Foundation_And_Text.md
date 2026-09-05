# Sprint 1 — Foundation & Text 🧱

**Points:** 5 | **Status:** ⬜ Not Started | **Goal:** Introduce the single shared concrete `GameEntity` and a new `TextComponent : IDrawableComponent`, then migrate and delete `TextEntity`. This proves the end-to-end "components only" pattern (new component → XML declaration → class deleted) that Sprints 2–4 will scale.

## Why This Sprint First

`Entity` is **abstract**, so every scene/prefab `<Type=...>` must resolve to a *concrete*, instantiable type. Before deleting any of the 8 classes we need one shared concrete entity they can all point at. `TextEntity` is the ideal first migration: it is self-contained (no inheritance, no save/load), its only behavior is custom text rendering in `Render()`, and that maps cleanly onto a new `IDrawableComponent` — which the base `Entity.Render()` already auto-draws. Finishing this sprint deletes a real class end-to-end with zero behavior change, de-risking the rest of the project.

## Target Outcome

- New `GameEntity : Entity` (parameterless constructor) in `CoreEssentials.Playground.Entities`.
- New `TextComponent : EntityComponent, IDrawableComponent` in `CoreEssentials.Playground.Components` owning font load + aligned text drawing.
- `TextTemplate.xml` + every scene `<EntityDefinition Type=...TextEntity>` repointed at `GameEntity`, with a `<Component Type="TextComponent">` carrying the text/color/alignment properties.
- `Entities/TextEntity.cs` deleted.

## Tasks

- [ ] T1 ⭐ Create `GameEntity : Entity` (public parameterless constructor; no behavior). Add unit test asserting it instantiates and is a valid concrete `Entity`.
- [ ] T2 ⭐ Create `TextComponent : EntityComponent, IDrawableComponent` — move font load (`AssetManager.LoadAsset<FontAsset>("Fonts/base")`) into `OnAttach`, and the alignment-aware `DrawString` from `TextEntity.Render` into `Draw(SpriteBatch)`. Expose `Text`, `Color`, `Alignment` (and the internal offset) as settable properties so XML `<Property>` values bind.
- [ ] T3 🔁 Update `Content/Templates/TextTemplate.xml`: root `<Prefab Type=...GameEntity>`, add a `<Component Type="TextComponent">` block with the text/color/alignment properties currently baked into `TextEntity`.
- [ ] T4 🔁 Repoint every scene that declares a `TextEntity` (e.g. `CameraScene.xml` `cameraInfoText`) to `GameEntity` + a `TextComponent` with matching per-instance overrides.
- [ ] T5 🔁 Delete `Entities/TextEntity.cs`. Remove any now-unused references/imports.
- [ ] T6 🔒 Add unit tests for `TextComponent` (font loads on attach; alignment math for Left/Center/Right matches the old `TextEntity.Render`).
- [ ] T7 🔒 Build clean + full suite green (expect 1174/0/3) + smoke-run all 7 scenes PASS.

## Acceptance Criteria

- No code references `TextEntity`; the type is fully removed.
- A text entity declared purely from XML (`GameEntity` + `TextComponent`) renders identically to before (same font, color, alignment).
- Full suite green; all 7 scenes smoke-run PASS.

## Deliverables

| File | Action | Purpose |
|------|--------|---------|
| `Entities/GameEntity.cs` | Create | Shared concrete entity for all XML-defined entities |
| `Components/TextComponent.cs` | Create | Drawable text rendering component |
| `Content/Templates/TextTemplate.xml` | Modify | Point at `GameEntity`, declare `TextComponent` |
| `Content/Scenes/CameraScene.xml` (and any others) | Modify | Repoint `TextEntity` refs to `GameEntity` + `TextComponent` |
| `Entities/TextEntity.cs` | Delete | Behavior now in `TextComponent` |
| `CoreEssentials.Tests/**` | Add/Modify | `GameEntity` + `TextComponent` tests; adjust any `TextEntity` references |

## Notes & Risks

- **Property binding for `Alignment` (enum):** confirm the component property binder parses enum strings (e.g. `"Center"`) — check how other components bind enums before relying on it; if not supported, accept a string and map internally.
- **Font asset name:** keep `"Fonts/base"` exactly as-is (it moved in Organization Sprint 2). Do not regress to the old root-level key.
- **`TextEntity.TextAlignment` enum** is currently nested in the class — move it to `TextComponent` (or a shared location) so XML/properties can reference it.
- Keep the smoke run as the behavioral gate: text appears on CameraScene and CharacterScene HUDs.

---
*Created: 2026-09-05 | Part of Playground Entity → Components Project*
