# Sprint 3 — GUI Buttons & Camera 🎛️

**Points:** 5 | **Status:** ✅ Complete (2026-09-05) | **Goal:** Migrate `SoundButtonEntity`, `VolumeButtonEntity`, and `CameraEntity` onto components, delete all three classes. The two button entities become new Myra-canvas components; the camera entity is largely re-expressed with the *existing* `CameraInputComponent` + `CameraFollowToggleComponent`.

## Why These Three Together

They are the "interactive HUD" group and share a pattern (a declarative component wired from scene data). Two are near-duplicates of each other (both own a Myra `Canvas` + a text button with a click handler), and the camera one is mostly already covered by existing components — so this sprint is mostly *declaring* behavior that exists, plus two small new canvas components.

- `SoundButtonEntity` = Myra `Canvas` + `WidgetFactory.CreateTextButton(text)` whose `Clicked` plays `AudioManager.PlayOneShotSound(asset)`.
- `VolumeButtonEntity` = same shape, but the click sets `AudioManager.SetMasterVolume(level)`.
- `CameraEntity` = owns a built-in `CameraComponent`; WASD pan / Q-E zoom / R reset (already in `CameraInputComponent`) + follow-target toggle (already in `CameraFollowToggleComponent`).

## Target Outcome

- New `SoundButtonComponent` and `VolumeButtonComponent` (each owns a Myra `Canvas`, builds a text button, wires the click handler).
- Camera entities declared as `GameObjectEntity` + `CameraComponent` + `CameraInputComponent` (+ `CameraFollowToggleComponent` where follow is used), replacing `CameraEntity`.
- `Entities/SoundButtonEntity.cs`, `VolumeButtonEntity.cs`, `CameraEntity.cs` deleted.

## Tasks

- [x] T1 ⭐ Create `SoundButtonComponent : EntityComponent` — **implemented as a thin subscriber** (see Follow-up notes): subscribes to the pre-declared `ButtonComponent.Clicked` in `OnAttach`, plays `AudioManager.PlayOneShotSound(SoundAsset)`. Exposes `SoundAsset`; button text lives on the built-in `ButtonComponent`.
- [x] T2 ⭐ Create `VolumeButtonComponent : EntityComponent` — same shape; click → `AudioManager.SetMasterVolume(VolumeLevel)`. Exposes `VolumeLevel`.
- [x] T3 🔁 Update `Content/Templates/SoundButtonTemplate.xml` and `VolumeButtonTemplate.xml` → `GameObjectEntity` root + `CanvasComponent` + `ButtonComponent` + the respective behavior component.
- [x] T4 🔁 Repoint every scene that declares a sound/volume button to the new component form, preserving each button's per-instance overrides (now component-targeted `<Overrides>`).
- [x] T5 ⭐ Migrate `CameraEntity` → `GameObjectEntity` + `CameraComponent` + `CameraInputComponent` (+ new `CameraFollowComponent` for the follow lerp; `CameraFollowToggleComponent` rewired to it). `Move`/`Zoom` are covered by input panning/zoom keys; `ResetCamera` by the R key; `SetFollowTarget`/`ToggleFollow` live on `CameraFollowComponent`.
- [x] T6 🔁 Update all scenes that declare a `CameraEntity` (`CameraScene.xml`, `GuiAnchorDemo.xml`, `LabelAlignmentDemoScene.xml`) to the component form (`CameraSpeed` entity override → `MoveSpeed` on `CameraInputComponent`).
- [x] T7 🔁 Delete `Entities/SoundButtonEntity.cs`, `VolumeButtonEntity.cs`, `CameraEntity.cs`; also delete dead C# scene classes `GuiAnchorDemoScene.cs` + `LabelAlignmentDemoScene.cs` (confirmed unreachable on the data-driven path — the manifest lists only XML files).
- [x] T8 🔒 Add unit tests: `SoundButtonComponent` (click plays the configured sound), `VolumeButtonComponent` (click sets master volume), `CameraFollowComponent` (lerp/toggle/destroyed-target), input follow-gating, rewired toggle — in `Sprint3GuiAndCameraComponentTests.cs`; plus camera/button declaration coverage in the Sprint 5b/5d scene tests.
- [x] T9 🔒 Build clean + full suite green + smoke-run all 7 scenes PASS.

## Acceptance Criteria

- No code references `SoundButtonEntity`, `VolumeButtonEntity`, or `CameraEntity`.
- Sound/volume buttons declared from XML play/set audio identically; cameras declared from XML pan/zoom/reset/follow as before.
- Full suite green; all 7 scenes smoke-run PASS.

## Deliverables

| File | Action | Purpose |
|------|--------|---------|
| `Components/SoundButtonComponent.cs` | Create | Myra text button → one-shot sound |
| `Components/VolumeButtonComponent.cs` | Create | Myra text button → master volume |
| `Content/Templates/{SoundButtonTemplate,VolumeButtonTemplate}.xml` | Modify | Repoint + declare components |
| `Content/Scenes/CameraScene.xml`, `GuiAnchorDemo.xml`, `LabelAlignmentDemoScene.xml` (+ any others) | Modify | Cameras + buttons → component form |
| `Entities/{SoundButtonEntity,VolumeButtonEntity,CameraEntity}.cs` | Delete | Behavior now in components |
| `CoreEssentials.Tests/**` | Add/Modify | New component tests; adjust existing references |

## Notes & Risks

- **Myra `Canvas` lifecycle headlessly:** the button components own a Myra `Canvas`. Confirm it constructs/cleans up safely under the test host (the current entities already do this, so behavior should carry over — but verify in the new component's tests).
- **Data-driven configure path:** both entities have an `_configured` guard so constructor-created vs. XML-created instances behave the same. The components must wire their button from properties in `OnAttach` regardless of how the entity was created.
- **Camera public API used elsewhere:** grep for `CameraEntity` method calls (`Move`, `Zoom`, `ResetCamera`, `ToggleFollow`, `.Camera`) across scenes/components before deleting — e.g. `CameraScene`'s follow toggle may call into it. Ensure the equivalent is reachable via the components (or a scene-level helper) so no call site breaks.
- **`CameraComponent` ownership:** the camera instance is owned by the built-in `CameraComponent`; the entity only added input + follow. Confirm `CameraInputComponent`/`CameraFollowToggleComponent` already cover every key/action `CameraEntity.Update` performs (WASD/Q/E/R/follow) before removing the class.

## Follow-up (2026-09-05): design decisions taken during implementation

The original plan had the button components *own* their Myra `Canvas`. During implementation, three
design calls were confirmed with the developer instead:

1. **Thin subscribers, not canvas owners.** The templates declare `GameObjectEntity` +
   `CanvasComponent` + `ButtonComponent` (built-in), and `SoundButtonComponent` /
   `VolumeButtonComponent` only subscribe to `ButtonComponent.Clicked` in `OnAttach`. This reuses
   framework code (canvas lifecycle, widget creation, alignment) and matches the pre-declare-siblings
   rule — a component must never create siblings from `OnAttach`/`Update`. Button text moved from the
   old `ButtonText` entity property to `ButtonComponent.Text` via component-targeted `<Overrides>`.
2. **New `CameraFollowComponent`.** The follow lerp + `SetFollowTarget`/`ToggleFollow`/`FollowingTarget`
   surface left `CameraEntity` live here; `CameraInputComponent` gained an `IsFollowing()` gate that
   suspends WASD panning while it is active (matching the old entity), and `CameraFollowToggleComponent`
   now finds the component on its referenced camera instead of casting to `CameraEntity`.
3. **Dead C# scene classes deleted.** `GuiAnchorDemoScene.cs` and `LabelAlignmentDemoScene.cs` were
   unreachable on the data-driven path (`SceneManager.LoadScene(string)` always wraps in a
   `DataDrivenScene`; the manifest lists only XML files) and were removed with this sprint.

Docs: new `docs/GuiButtonAndCameraComponents.md`; `docs/SceneAsData.md` examples repointed off
`CameraEntity`; `docs/CameraSystem.md` cross-links the follow component.

---
*Created: 2026-09-05 | Part of Playground Entity → Components Project*
