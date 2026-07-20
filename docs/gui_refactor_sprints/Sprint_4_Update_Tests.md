# Sprint 4 — Update Tests & Playground ✅

**Points:** 5  
**Status:** Not Started (depends on Sprints 0–3)  
**Sprint Goal:** Rewrite all GUI tests to use the new interface-based API, update Playground examples to stop using Myra types directly, and remove all `using Myra.*` from user-facing files.

---

## Tasks

- [ ] **T1: Update `GUIManagerTests.cs` (1 pt)**
  - Current test creates Myra types (`Panel`, `Label`) directly — replace with factory-created widgets
  - Replace `MyraEnvironment.Game = _mockGame;` setup with engine initialization
  - Test should use `WidgetFactory.CreatePanel()` and `IGuiManager` via `EngineResolver.GetEngine()`
  - Verify all assertions still pass (focus checks, widget add/remove)

- [ ] **T2: Update `CanvasTests.cs` (1 pt)**
  - Current test creates Myra types (`Label`) directly — replace with factory-created widgets
  - Replace `MyraEnvironment.Game = _mockGame;` setup
  - Test should use `ICanvas canvas = CanvasFactory.CreateScreenSpace();` and `WidgetFactory.CreateLabel()`
  - Verify position updates, widget add/remove, cleanup all still work

- [ ] **T3: Update `CanvasWorldSpaceTests.cs` (1 pt)**
  - Same pattern as T2 but for world-space canvas
  - Ensure camera conversion logic is tested with the new implementation
  - Mock or use real `Camera.MainCamera` for world-to-screen tests

- [ ] **T4: Update Playground — `SoundButtonEntity.cs` (0.5 pt)** ⭐
  - Current code uses `using Myra.Graphics2D.UI;` and `Button.CreateTextButton()` directly
  - Replace with: `IButton button = WidgetFactory.CreateTextButton(_buttonText);`
  - Remove all `using Myra.*` statements
  - Click handler pattern stays the same (C# event delegation)

- [ ] **T5: Update Playground — `VolumeButtonEntity.cs` (0.5 pt)** ⭐
  - Same pattern as T4
  - Replace `Button.CreateTextButton()` with `WidgetFactory.CreateTextButton()`
  - Remove all `using Myra.*` statements

- [ ] **T6: Search for remaining Myra usages in tests & playground (1 pt)**
  - Run grep across entire workspace for `using Myra` to find any missed files
  - Check test helper methods, mock setups, and any other entities that might use Myra types
  - Ensure all playground examples compile without Myra imports

---

## Acceptance Criteria

- [ ] All GUI tests pass (`dotnet test CoreEssentials.Tests`) — same coverage as before refactor
- [ ] `GUIManagerTests.cs` uses `IGuiManager` and factories, no direct Myra type instantiation
- [ ] `CanvasTests.cs` uses `ICanvas` and factories, no direct Myra type instantiation
- [ ] `CanvasWorldSpaceTests.cs` works with world-space canvas implementation
- [ ] `SoundButtonEntity.cs` — zero Myra references, uses `WidgetFactory.CreateTextButton()` ✅
- [ ] `VolumeButtonEntity.cs` — zero Myra references, uses `WidgetFactory.CreateTextButton()` ✅
- [ ] **Zero `using Myra.*` statements in any test or playground file** (production code already done in Sprint 3)

---

## Deliverables

| File | Change | Notes |
|------|--------|-------|
| `CoreEssentials.Tests/GUI/GUIManagerTests.cs` | Updated to use IGuiManager + factories | No Myra types created directly |
| `CoreEssentials.Tests/GUI/CanvasTests.cs` | Updated to use ICanvas + factories | No Myra types created directly |
| `CoreEssentials.Tests/GUI/CanvasWorldSpaceTests.cs` | Updated for world-space canvas | No Myra types created directly |
| `CoreEssentials.Playground/SoundButtonEntity.cs` | Uses WidgetFactory.CreateTextButton() | Zero Myra references |
| `CoreEssentials.Playground/VolumeButtonEntity.cs` | Uses WidgetFactory.CreateTextButton() | Zero Myra references |

---

## Notes & Risks

- **Test isolation:** The existing tests set up a real Game instance and Myra environment. With the new structure, tests still need a valid game context for rendering but should initialize through `EngineResolver.GetEngine().Init()` instead of manually setting `MyraEnvironment.Game`.
- **Mocking interfaces:** If you want to test GUI logic without a full Myra runtime, consider adding mock implementations of `IGuiManager` and `ICanvas` in tests. This would allow unit testing GUI logic in isolation (headless).
- **Playground entity lifecycle:** Ensure the click handlers (`button.Click += ...`) still work correctly with the new event pattern on `IButton`. The test should verify button clicks trigger the expected sound/volume actions.

---

*Created: 2026-07-20 | Part of GUI System Refactoring Project*
