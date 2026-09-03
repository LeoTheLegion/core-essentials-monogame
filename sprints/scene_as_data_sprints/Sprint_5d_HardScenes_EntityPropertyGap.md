# Sprint 5d — Hard Scenes (Character / Camera / LabelAlignment) 🧩

**Points:** 5 | **Status:** ✅ Done (commit `eb2afb3`) | **Goal:** Migrate the three most complex demo scenes to strict-format data files, porting their per-frame loops and custom draw code to components, and delete their C# subclasses.

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
- [x] T1 ⭐ Port LabelAlignmentDemo's per-frame orbit loop to a component (`OrbitPanelComponent`); port its custom debug `Draw` overlay to an `IDrawableComponent`; port the throttled HUD label refresh to a component.
- [x] T2 ⭐ Author `CharacterScene.xml`, `CameraScene.xml`, and `LabelAlignmentDemoScene.xml` in the strict format, attaching Sprint 4 + new components (audio keys, volume, debug toggle/config, camera follow, navigation).
- [x] T3 🔒 Delete the now-dead subclasses `CharacterScene.cs`, `CameraScene.cs`, `LabelAlignmentDemoScene.cs` (and any code referencing them); repoint all navigation targets to scene asset-name strings.
- [x] T4 ⭐ Update `Content.mgcb` to stage the three new scene files with `/copy:`.
- [x] 🔁 Integration tests: each file parses in the strict format and loads as a `DataDrivenScene`; orbit loop advances the panel; camera follow toggles; audio/volume keys fire (via recording seams). Manual crash-check by running the playground.

## ✅ Completion Notes (2026-09-02, commit `eb2afb3`)

**New components** (`CoreEssentials.Playground/`):
- `OrbitPanelComponent` — moves its owner around a world-space ellipse every frame: `(CenterX + cos(t·Speed)·RadiusX, CenterY + sin(t·Speed)·RadiusY)`. Replaces the old scene's `RunFrameLoop` orbit. Virtual `ComputePosition` seam for tests.
- `CameraFollowToggleComponent` — on a configured key (default F) toggles `CameraEntity.ToggleFollow(target)` and rewrites an info `TextEntity` from `InfoTemplate` (`{state}` → ON/OFF). Camera/target/label are Entity-typed properties wired via `<Reference>`. Virtual `DoToggle`/`UpdateInfo` seams.
- `HudLabelRefreshComponent` — every `IntervalSeconds` (0.5) rewrites its host `LabelComponent.Text` from `TextTemplate`, substituting the label's measured `{w}`/`{h}`. Replaces the old throttled HUD refresh.
- `LabelAlignmentDebugOverlayComponent` (`IDrawableComponent`) — auto-discovers every `CanvasComponent`/`LabelComponent` in the system and draws canvas bounds (screen→DarkSlateGray, world→DarkOrange via camera projection), each label's rendered bounds in its own text color, and a white cross at the alignment reference point. Uses its **own** `SpriteBatch` (entity rendering runs inside an already-begun batch with the camera view — same pattern as Aether's DebugView). Static `ComputeLabelBounds(label, canvas, toScreen)` is pure geometry for tests.

**Additive entity/component knobs:**
- `PlayerEntity()` parameterless ctor; `SoundButtonEntity`/`VolumeButtonEntity` gained public settable `SoundAsset`/`ButtonText` and `VolumeLevel`/`ButtonText` plus an `OnStart` that wires the widget only when values arrived via `<EntityOverrides>` (a `_configured` flag keeps constructor-created instances from double-configuring).
- `DebugToggleComponent` gained `StartEnabled` (turns debug on at attach) and `DebugFontAsset` (loads + assigns the system debug font), with a virtual `LoadDebugFont` seam.

**Scene XML authored (strict format):**
- `CharacterScene.xml` — rewritten in place from the old loose template file to `<GameSystems>`/`<System Type="EntitySystem">`. Three templates registered as prefabs; characters typed; text + buttons are prefab-based with `<EntityOverrides>`; inert shells carry `MusicComponent`, `DebugToggleComponent` (StartEnabled + all flags + font), three `SoundKeyComponent` (Q/W/E), two `VolumeKeyComponent` (Z/X), and two `NavigateOnKeyComponent` (+ → Physics, M → SendMessage).
- `CameraScene.xml` — camera + player typed entities; a typed `TextEntity` info label with multi-line text (`&#10;` character references preserve real newlines); a follow-toggle shell wired via three `<Reference>` links; nav to `CharacterScene.xml`.
- `LabelAlignmentDemoScene.xml` — camera (speed 300); screen-space HUD root with four label children (three alignment labels + info, each refreshed by `HudLabelRefreshComponent`); world-space panel (pinned 280×150 canvas) orbiting via `OrbitPanelComponent` with title/caption label children; the overlay shell; nav to `SendMessageDemoScene.xml`.

**Decisions & gotchas:**
- **Music-stripped load test.** `MusicComponent.OnAttach → AudioManager.PlaySound` throws headlessly (the mock content returns a null `SoundEffect`, and there is no per-entity try/catch in `DataDrivenScene`). The CharacterScene *parse* test asserts the real file carries the music shell; its *load* test stages a `StripEntity(..., "music")` variant so the rest of the scene still loads.
- **Discovery is flat, not recursive.** `EntitySystem.GetEntities()` already returns every entity (nested children are registered as first-class entities *and* linked via `Children`). The overlay's initial recursive discovery double-counted nested components — fixed to filter the flat list directly (caught by a failing unit test).
- **Parented-entity position.** A parented entity's `Position` setter is ignored (the getter derives world position from `LocalPosition`), so tests must set `LocalPosition` for children.
- **Parse with prefabs needs AssetManager up.** `SceneParser.Parse` eagerly loads `<Prefab Asset=...>` through `AssetManager`, so the CharacterScene parse test inits a `MockContentManager` and stages the three templates (the 5c parse test only passed via leaked static state).
- **mgcb:** added `/copy:` entries for `CameraScene.xml` and `LabelAlignmentDemoScene.xml`; `CharacterScene.xml` already had one.

**Tests:** +17 — `Sprint5dComponentTests.cs` (11 unit: orbit trajectory, follow toggle/info substitution, HUD format/throttle, debug StartEnabled/font, overlay discovery + label-bounds geometry) and `Sprint5dDataSceneTests.cs` (6 integration: parse + full-load per scene; camera reference resolution; orbit advancing the panel). Full suite green **1114 passed / 0 failed / 3 skipped**.

## Acceptance Criteria

- Character, Camera, and LabelAlignment demos run entirely from XML + assets; their C# subclasses are deleted.
- Per-frame orbit, debug overlay, HUD refresh, camera follow, and all audio/keyboard controls work when running the playground.
- Build clean, all tests passing.

## Notes & Risks

- **This is the riskiest slice** — run the playground after each scene lands.
- If a per-frame loop proves cleaner as a thin subclass than a component, that decision (and its justification) gets recorded here rather than silently shipped.

---
*Created: 2026-09-01 | Part of Scene-as-Data Project (Sprint 5 split)*
