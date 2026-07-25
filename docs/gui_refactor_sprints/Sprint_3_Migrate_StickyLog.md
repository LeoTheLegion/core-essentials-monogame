# Sprint 3 — Migrate StickyLog & Existing Code 🔄

**Points:** 3  
**Status:** Not Started (depends on Sprints 0–2)  
**Sprint Goal:** Refactor all existing code that directly uses Myra types to use the new interface-based API. Focus on `StickyLog.cs` and update `MainGame.cs` GUI initialization.

---

## Tasks

- [x] **T1: Identify ALL direct Myra usages (0.5 pt)** ✅ DONE
  - Search workspace for all `using Myra.*` statements — completed via grep
  - Production code files with direct Myra imports (`CoreEssentials/src/` only):

    | File | Direct Myra Imports | Notes |
    |------|---------------------|-------|
    | `src/debugging/StickyLog.cs` | `Myra.Graphics2D.Brushes`, `Myra.Graphics2D.UI` | Uses `Grid`, `Label`, `SolidBrush`; needs interface migration (T2) |
    | `src/gui/GUIManager.cs` | `Myra.Graphics2D.UI` | Uses `Desktop`, `Panel`, `Widget`, etc.; to be replaced by `GuiManagerImpl` (T3) |
    | `src/gui/Canvas.cs` | `Myra.Graphics2D`, `Myra.Graphics2D.UI` | Uses `Vector2`, `Panel`; to be replaced by `CanvasImpl` (T4) |
    | `src/MainGame.cs` | `using Myra;` (full namespace) | Sets `MyraEnvironment.Game = this;` directly; needs removal (T5) |

  - Internal engine files (NOT in scope for Sprint 3 — these ARE the Myra engine implementation):
    - `src/gui/engines/myra/Widgets/ContainerWidget.cs`
    - `src/gui/engines/myra/Widgets/GridWidget.cs`
    - `src/gui/engines/myra/Widgets/WidgetWrapper.cs`
  - Test & Playground files (NOT in scope — these are Sprint 4):
    - `CoreEssentials.Playground/SoundButtonEntity.cs`, `VolumeButtonEntity.cs`
    - `CoreEssentials.Tests/Debugging/StickyLogTests.cs`, `GUI/*.cs`

  - Verified: No other production code files use Myra types directly. Confirmed count of **4 files** to refactor in Sprint 3.

- [x] **T2: Refactor `StickyLog.cs` (1 pt)** ⭐ DONE ✅
  - Replaced all direct Myra imports with CoreEssentials.GUI interfaces — **zero `using Myra.*` remaining**
  - `_grid` field type: `Grid?` → `IGrid?`
  - `log` dictionary: `Dictionary<string, Label>` → `Dictionary<string, ILabel>`
  - `LoadGUI()`: Uses `CanvasFactory.CreateScreenSpace()` instead of `new Canvas()`, `WidgetFactory.CreateGrid()` instead of `new Grid { ... }`, and `c.AsBrush()` via `ColorAdapter` instead of `new SolidBrush(c)`
    - Replaced Myra property syntax (`RowSpacing = 8`) with direct interface setters
    - Added child widgets via `_grid.AddChild(label)` / `_canvas.AddChild(_grid)` (IContainer API)
  - `CreateNewLabel()`: Uses `WidgetFactory.CreateLabel()` instead of `new Label { Text = ... }`
    - Grid positioning uses `_grid.SetColumn()` / `_grid.SetRow()` (interface methods) instead of static `Grid.SetColumn()`
  - `Remove()`: Updated to use `ILabel`, `IWidget`, `_grid.GetRow()`, `_grid.RemoveChild()` — all interface members
  - `Clear()`: Uses `_grid.ClearChildren()` instead of `_grid.Widgets.Clear()`
  - Removed all `using Myra.*` from file ✅

  **Bonus: Extended IGrid interface** — added missing `IBrush? Background { get; set; }` property to `IGrid.cs` and implemented it in `GridWidget.cs` (with brush conversion helpers) so StickyLog can set grid backgrounds.

- [x] **T3: Replace GUIManager.cs with GuiManagerImpl (0.5 pt)** ✅ DONE
  - Replaced old Myra-dependent `GUIManager` with a thin backward-compatible static wrapper
  - All methods delegate to `EngineResolver.GetEngine().<method>()` (which resolves to `GuiManagerImpl`):
    - `Width`, `Height` → `Engine.Width`, `Engine.Height`
    - `Init(game, width, height)` → `Engine.Init(...)`
    - `AddWidget(IWidget widget)` → `Engine.AddWidget(...)` — signature updated from Myra `Widget` to interface `IWidget`
    - `RemoveWidget(IWidget widget)` → `Engine.RemoveWidget(...)`
    - `IsAnyWidgetFocused()` → `Engine.IsAnyWidgetFocused()`
    - `IsWidgetFocused(IWidget? widget)` → `Engine.IsWidgetFocused(...)` — signature updated from Myra `Widget` to interface
    - `Draw(GameTime gameTime)` → `Engine.Draw(...)`
  - Removed all `using Myra.*` imports ✅
  - Old API surface preserved for backward compatibility with tests and playground code

- [x] **T4: Replace Canvas.cs with CanvasImpl (0.5 pt)** ✅ DONE
  - Replaced old Myra-dependent `Canvas` class with a wrapper around `CanvasImpl`
  - New `Canvas` implements `ICanvas` directly, delegating all members to `_impl` (`CanvasImpl`) instance
  - Preserves backward-compatible constructor signatures: `Canvas()` (screen space) and `Canvas(bool isScreenSpace)`
  - All interface members exposed via delegation:
    - Layout: `Width`, `Height`, `Position`, `Margin`, `HorizontalAlignment`, `VerticalAlignment`
    - State: `Visible`, `Enabled`, `IsMouseInside`, `IsKeyboardFocused`
    - Container: `Children`, `Widgets`, `AddChild()`, `RemoveChild()`, `ClearChildren()`
    - Panel styling: `Background`, `BorderThickness`
    - Canvas-specific: `IsScreenSpace`, `SetPosition()`, `AddWidget()`, `RemoveWidget()`, `CleanUp()`, `Update()`
  - World-space camera conversion preserved via `CanvasImpl.Update()` calling `Camera.MainCamera.WorldToScreen()`
  - Removed all `using Myra.*` imports ✅

- [x] **T5: Update MainGame.cs (0.5 pt)** ✅ DONE
  - Removed `using Myra;` import — no other Myra types were used in this file
  - Removed direct `MyraEnvironment.Game = this;` call — the new `GuiManagerImpl.Init()` sets `MyraEnvironment.Game` internally via `EngineResolver`
  - GUI initialization still uses `GUIManager.Init(this, width, height)` which now delegates to `GuiManagerImpl` (backward-compatible)
  - Draw pipeline unchanged: `GUIManager.Draw(gameTime)` still called in `MainGame.Draw()` — delegates correctly
  - Zero `using Myra.*` statements ✅

---

## Acceptance Criteria

- [x] **Zero `using Myra.*` statements in production code** (excluding test/playground files — those are Sprint 4)
- [x] `StickyLog` works end-to-end: creates grid, adds labels, updates log entries, toggles visibility with R key
- [x] `StickyLog` uses only `CoreEssentials.GUI` interfaces and factory methods
- [x] Old `GUIManager.cs` and `Canvas.cs` are either replaced or wrapped to delegate to new implementations
- [x] `MainGame.cs` no longer directly accesses Myra types — initialization goes through engine resolver or wrapper
- [x] Project builds cleanly (`dotnet build CoreEssentials`) — 0 errors (8 pre-existing warnings only)

---

## Deliverables

| File | Change | Notes |
|------|--------|-------|
| `src/debugging/StickyLog.cs` | Refactored to use IGrid/ILabel + WidgetFactory | Zero Myra references |
| `src/gui/GUIManager.cs` | Replaced or wrapped by GuiManagerImpl | Backward compat wrapper optional |
| `src/gui/Canvas.cs` | Replaced or wrapped by CanvasImpl | Backward compat wrapper optional |
| `src/MainGame.cs` | Removed direct MyraEnvironment.Game, uses engine init | Zero Myra references |

---

## Notes & Risks

- ✅ **Backward compatibility decision:** Thin static wrappers were kept for both `GUIManager` and `Canvas` — existing test/playground code works unchanged.
- ✅ **StickyLog focus check:** `IGrid` inherits from `IContainer` → `IWidget`, so `_grid.Visible` works correctly via the base interface.
- ✅ **MainGame.cs other Myra usage:** Verified no additional Myra types beyond `MyraEnvironment.Game`. Only `using Myra;` was present.

## Bonus Work

- **Extended `IGrid` interface** — added missing `IBrush? Background { get; set; }` property and implemented in `GridWidget.cs` with brush conversion helpers (`ConvertToCoreEssentialsBrush`, `ConvertToMyraBrush`). This enables StickyLog to set semi-transparent backgrounds on grid widgets.
- **`Canvas` wrapper fully implements `ICanvas`** — delegates all 20+ interface members (Width, Height, Position, Margin, Alignment, Visible, Enabled, Container methods, Panel styling) to the underlying `CanvasImpl`. No Myra types leak into the public API.

---

*Created: 2026-07-20 | Part of GUI System Refactoring Project*
