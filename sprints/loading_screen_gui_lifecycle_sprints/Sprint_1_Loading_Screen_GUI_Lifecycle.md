# Sprint 1 — Loading-Screen GUI Lifecycle 🎬

- **Points:** 5
- **Status:** ✅ Done (2026-09-03)
- **Sprint Goal:** Stop the loading screen's label from persisting after a transition, and stop the target scene's GUI from showing through while it loads — by making canvas registration follow the scene lifecycle.

## Tasks

| ID | Task | Points | Visibility |
|----|------|--------|------------|
| T1 | Failing tests: canvas not registered until first pump; unregistered on `CleanUp` (3 cases) | 2 | 🔒 |
| T2 | Failing test: loading screen is unloaded after the swap in a transition | 1 | 🔒 |
| T3 | Defer canvas registration to first `Update`; unregister on `CleanUp` (`CanvasImpl`) | 2 | ⭐ |
| T4 | Unload the loading screen after the scene swap (`SceneManager`) | 1 | 🔒 |
| T5 | Update two transition tests that read loading-screen systems post-swap (sample live instead) | 1 | 🔒 |
| T6b | Validation: build clean + full suite green | — | 🔁 |

## Acceptance Criteria

- [x] A canvas is **not** in the global GUI before its first `Update`, and **is** after.
- [x] `CleanUp` removes a pumped canvas from the global GUI; cleaning up an unpumped canvas does not throw.
- [x] After a loading-screen transition completes, `SceneManager.LoadingScene.IsLoaded == false` (so its canvas detaches).
- [x] Build is clean and the full test suite passes (1125 passed / 0 failed / 3 skipped).

## Deliverables

| File | Change | Purpose |
|------|--------|---------|
| `CoreEssentials/src/GUI/engines/myra/CanvasImpl.cs` | Modified | Adds `_isRegistered`; registers on first `Update` via `EnsureRegistered()`; unregisters in `CleanUp()`. No longer registers in the constructor. |
| `CoreEssentials/src/Scene/SceneManager.cs` | Modified | `TransitionWithLoadingScreenCoroutine` now unloads the loading screen before swapping to the target, so its canvas detaches from the global GUI. |
| `CoreEssentials.Tests/GUI/CanvasRegistrationLifecycleTests.cs` | New | 3 tests for the deferred registration contract. |
| `CoreEssentials.Tests/SceneManagement/SceneManagerTests.cs` | Modified | New test: loading screen unloaded after swap. |
| `CoreEssentials.Tests/SceneManagement/DataDrivenSceneTests.cs` | Modified | Sample loading-screen progress live during the transition (systems are gone post-swap). |
| `CoreEssentials.Tests/SceneManagement/BootFromFilesTests.cs` | Modified | Same live-sampling fix for the real-file boot test. |
| `docs/SceneManagement.md` | Updated | Documents the transition lifecycle and canvas registration behavior. |
| `docs/GUISystem.md` | Updated | Documents deferred global registration of canvases. |

## Root Cause

`MainGame.Draw` renders the scene layer, then the **global** `GUIManager.Draw`, which draws every canvas registered into the global root panel — regardless of current scene or load state. A canvas registered itself in its constructor (`CanvasImpl` → `_manager.AddWidget(this)`), so:

- The target scene's canvases registered while it was still loading (not yet current) → rendered through during load.
- The loading screen's canvas stayed registered forever after the swap (the coroutine kept it loaded "for reuse") → its label kept rendering on top of the new scene.

## Notes & Risks

- **Why unload, not hide:** once a canvas is registered it stays until `CleanUp`, and the loading scene stops being pumped after the swap — so nothing would call `CleanUp` if we merely left it loaded. Unloading makes each transition a clean reload (the coroutine already calls `_loadingScene.Load()` at the start of every transition), which also avoids a latent double-registration throw in `Scene.Load`.
- **Direct GUI users:** code that creates a canvas outside a scene (e.g. the Debug StickyLog) must pump it at least once for it to register — this is already what `StickyLog.Update` does each frame.
- No issue/PR numbers are referenced in code or docs.

---
*Created: 2026-09-03 | Part of Loading-Screen GUI Lifecycle Project*
