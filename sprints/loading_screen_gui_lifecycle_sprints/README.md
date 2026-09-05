# Loading-Screen GUI Lifecycle — Scrum Sprints 🎬

Fix two scene-transition bugs that share one root cause: a canvas registers itself into the **global** GUI at construction, so canvases keep rendering for scenes that are not (or no longer) active. The loading screen's label stays on screen after a transition, and the target scene's own GUI shows through while it is still loading.

> **Stacks on `feature/scene-as-data`** — depends on data-driven scenes booting from XML (`Program.cs`) and the smoke-run harness used to verify each scene visually.

## Sprint Roadmap

| Sprint | Name | Points | Status | Description |
|--------|------|--------|--------|-------------|
| 1 | [Loading-Screen GUI Lifecycle](Sprint_1_Loading_Screen_GUI_Lifecycle.md) | 5 | Not Started | Attach a canvas to the global GUI on first pump, detach on unload; unload the loading screen after the swap so it stops rendering and the target scene's GUI doesn't show through while loading |

## Why This?

`MainGame.Draw` renders the scene layer first, then the **global** `GUIManager.Draw`, which draws *every* canvas registered into the global root panel — regardless of which scene is current or even loaded. A canvas registers itself in its constructor (`CanvasImpl` → `_manager.AddWidget(this)`), so:

- The **target scene's** canvases register while it is still loading (it isn't current yet) → they render through during the load.
- The **loading screen's** canvas stays registered forever after the swap (the coroutine deliberately keeps it loaded for reuse) → its label keeps rendering on top of the new scene.

The fix makes a canvas attach to the main GUI only once its scene is actually being pumped, and detach when the scene unloads. That single lifecycle rule fixes both problems at once — no overlay needed, because the loading screen's own label already covers the screen during load.

## Point Summary

- Sizing: 1 = small, 2 = medium, 5 = large

## Workflow Phases

Core fix (Sprint 1): deferred canvas attach + loading-screen unload → verify each scene visually with the smoke-run harness.
