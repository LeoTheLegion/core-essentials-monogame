# Sprint 5d — Hard Scenes (Character / Camera / LabelAlignment) 🧩

**Points:** 5 | **Status:** Not Started (entity-property feature landed — ready) | **Goal:** Migrate the three most complex demo scenes to strict-format data files, porting their per-frame loops and custom draw code to components, and delete their C# subclasses.

## Why This Sprint Last

These three scenes carry real logic that has no data-file equivalent yet:
- **CharacterScene** — audio (Q/W/E sounds, Z/X volume), F3 debug toggle+config, plays a song on start, pause/resume on focus, navigation to SendMessage + Physics.
- **CameraScene** — camera follow toggle (F) via `CameraEntity.ToggleFollow(player)`, info text.
- **LabelAlignmentDemo** — the most complex: a per-frame orbit loop coroutine (`RunFrameLoop` moving a panel in world space), a throttled HUD label refresh (measured width), and a custom `Draw` debug overlay (canvas/label bounds + crosshairs via `Primitives`).

## ⚠️ Open Decision (must resolve before starting)

The strict parser resolves overrides **only to component properties**, never entity-level ones. But these demos keep state on the *entity itself*: `TextEntity.Text/Color/Alignment`, `SoundButtonEntity.Configure(asset,text)`, `VolumeButtonEntity.Configure(vol,label)`, `CameraEntity.CameraSpeed`. There is currently no way to set these from XML.

Chosen: **(a) CoreEssentials feature** — add per-instance *entity* property overrides (small, reusable, benefits all future scenes). **Implemented.**

## ✅ Entity-Property Feature (landed)

Per the developer's confirmation (2026-09-02), option **(a)** was implemented as a small, reusable CoreEssentials feature — closing the "entity-level values" half of the per-instance override gap. Component overrides already existed; this adds the entity-level path so state living directly on an entity (e.g. `TextEntity.Text/Color/Alignment`, `CameraEntity.CameraSpeed`) can be set per-instantiation from both C# and scene XML.

**What changed:**
- `Prefab.EntityOverrides` (`Dictionary<string,string>`) — deep-copied in `Prefab.Clone()`.
- `PrefabOverrides.Apply(prefab, componentOverrides, entityOverrides)` — new 3-arg overload (the 2-arg form delegates with `null`); merges entity overrides into the clone.
- `EntityPrefabLoader.BuildSubtree` applies entity overrides via reflection **before** `OnStart`/`OnAttach` (same value-parsing as components; unknown/unwritable props warn instead of silently no-op'ing).
- `EntitySystem.Instantiate` / `InstantiateFromAsset` gained an `entityOverrides` parameter.
- Scene XML: new `<EntityOverrides>` element on `<EntityDefinition>` (flat `Property Name/Value` pairs), parsed by `SceneParser` and carried through `DataDrivenScene`. Works for nested `<Children>` too.

**Tests:** `CoreEssentials.Tests/.../Serialization/EntityOverrideTests.cs` — 11 tests covering the C# API (applied-before-OnStart, multi-type parsing, no prefab mutation, component+entity together), the parser (populate + strictness errors), and end-to-end data-driven scenes (root + nested child).

**Docs:** `docs/EntityTemplates.md` → "Per-Instantiation Overrides".

## Tasks

- [x] T0 🔒 Resolve the entity-property decision above; if (a), implement + test the CoreEssentials override feature first.
- [ ] T1 ⭐ Port LabelAlignmentDemo's per-frame orbit loop to a component (`OrbitPanelComponent`); port its custom debug `Draw` overlay to an `IDrawableComponent`; port the throttled HUD label refresh to a component.
- [ ] T2 ⭐ Author `CharacterScene.xml`, `CameraScene.xml`, and `LabelAlignmentDemoScene.xml` in the strict format, attaching Sprint 4 + new components (audio keys, volume, debug toggle/config, camera follow, navigation).
- [ ] T3 🔒 Delete the now-dead subclasses `CharacterScene.cs`, `CameraScene.cs`, `LabelAlignmentDemoScene.cs` (and any code referencing them); repoint all navigation targets to scene asset-name strings.
- [ ] T4 ⭐ Update `Content.mgcb` to stage the three new scene files with `/copy:`.
- [ ] 🔁 Integration tests: each file parses in the strict format and loads as a `DataDrivenScene`; orbit loop advances the panel; camera follow toggles; audio/volume keys fire (via recording seams). Manual crash-check by running the playground.

## Acceptance Criteria

- Character, Camera, and LabelAlignment demos run entirely from XML + assets; their C# subclasses are deleted.
- Per-frame orbit, debug overlay, HUD refresh, camera follow, and all audio/keyboard controls work when running the playground.
- Build clean, all tests passing.

## Notes & Risks

- **This is the riskiest slice** — run the playground after each scene lands.
- If a per-frame loop proves cleaner as a thin subclass than a component, that decision (and its justification) gets recorded here rather than silently shipped.

---
*Created: 2026-09-01 | Part of Scene-as-Data Project (Sprint 5 split)*
