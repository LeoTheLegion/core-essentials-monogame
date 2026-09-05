# Sprint 3 — GUI Buttons & Camera 🎛️

**Points:** 5 | **Status:** ⬜ Not Started | **Goal:** Migrate `SoundButtonEntity`, `VolumeButtonEntity`, and `CameraEntity` onto components, delete all three classes. The two button entities become new Myra-canvas components; the camera entity is largely re-expressed with the *existing* `CameraInputComponent` + `CameraFollowToggleComponent`.

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

- [ ] T1 ⭐ Create `SoundButtonComponent : EntityComponent` — owns a `Canvas`, builds the text button in `OnAttach`, wires `Clicked` → `AudioManager.PlayOneShotSound(SoundAsset)`. Expose `SoundAsset` + `ButtonText` as settable properties (bindable from `<EntityOverrides>`). Clean up the canvas in `OnDetach`.
- [ ] T2 ⭐ Create `VolumeButtonComponent : EntityComponent` — same shape; click → `AudioManager.SetMasterVolume(VolumeLevel)`. Expose `VolumeLevel` + `ButtonText`.
- [ ] T3 🔁 Update `Content/Templates/SoundButtonTemplate.xml` and `VolumeButtonTemplate.xml` → `GameObjectEntity` root + the respective `<Component>` with asset/text (or volume/text) properties.
- [ ] T4 🔁 Repoint every scene that declares a sound/volume button to the new component form, preserving each button's per-instance overrides.
- [ ] T5 ⭐ Migrate `CameraEntity` → `GameObjectEntity` + `CameraComponent` + `CameraInputComponent` (+ `CameraFollowToggleComponent` where follow is used). Map `CameraEntity`'s public surface (`Move`, `Zoom`, `ResetCamera`, `SetFollowTarget`, `ToggleFollow`) onto the existing components; confirm each scene's camera usage (pan/zoom/reset/follow) is covered.
- [ ] T6 🔁 Update all scenes that declare a `CameraEntity` (`CameraScene.xml`, `GuiAnchorDemo.xml`, `LabelAlignmentDemoScene.xml`) to the component form.
- [ ] T7 🔁 Delete `Entities/SoundButtonEntity.cs`, `VolumeButtonEntity.cs`, `CameraEntity.cs`; remove references/imports.
- [ ] T8 🔒 Add unit tests: `SoundButtonComponent` (click plays the configured sound), `VolumeButtonComponent` (click sets master volume), and camera declaration coverage (a camera declared purely from XML pans/zooms/resets). Reuse existing camera-component test patterns where present.
- [ ] T9 🔒 Build clean + full suite green (expect 1174/0/3) + smoke-run all 7 scenes PASS.

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

---
*Created: 2026-09-05 | Part of Playground Entity → Components Project*
