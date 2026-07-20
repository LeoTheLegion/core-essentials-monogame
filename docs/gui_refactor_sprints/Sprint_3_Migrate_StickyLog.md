# Sprint 3 — Migrate StickyLog & Existing Code 🔄

**Points:** 3  
**Status:** Not Started (depends on Sprints 0–2)  
**Sprint Goal:** Refactor all existing code that directly uses Myra types to use the new interface-based API. Focus on `StickyLog.cs` and update `MainGame.cs` GUI initialization.

---

## Tasks

- [ ] **T1: Identify ALL direct Myra usages (0.5 pt)**
  - Search workspace for all `using Myra.*` statements
  - Current known locations:
    - `CoreEssentials/src/debugging/StickyLog.cs` — uses `Grid`, `Label`, `SolidBrush`, `Proportion` from Myra
    - `CoreEssentials/src/gui/GUIManager.cs` — uses `Desktop`, `Panel`, `Widget`, `ContentControl`, `ComboView` from Myra (to be replaced by `GuiManagerImpl`)
    - `CoreEssentials/src/gui/Canvas.cs` — uses `Panel` from Myra (to be replaced by `CanvasImpl`)
    - `CoreEssentials/src/MainGame.cs` — sets `MyraEnvironment.Game = this;` directly
  - Verify no other files use Myra types in production code

- [ ] **T2: Refactor `StickyLog.cs` (1 pt)** ⭐
  - Replace all direct Myra imports with CoreEssentials.GUI interfaces
  - Changes needed:
    - `_grid` field type: `Myra.Graphics2D.UI.Grid?` → `IGrid?`
    - `log` dictionary: `Dictionary<string, Label>` → `Dictionary<string, ILabel>`
    - `LoadGUI()`: Replace Myra Grid creation with `WidgetFactory.CreatePanel()` or direct factory calls
      - `_grid = new Grid { RowSpacing = 8, ColumnSpacing = 8 }` → use factory to create grid widget
      - `_grid.Background = new SolidBrush(c)` → `_grid.Background = new SolidColorBrush(c)` (via ColorAdapter)
    - `CreateNewLabel()`: Replace Myra Label creation with `ILabel keyLabel = WidgetFactory.CreateLabel(key);`
      - Use static helpers: `GridWidget.SetRow(keyLabel, logCount)`, `GridWidget.SetColumn(keyLabel, 0)`
    - `Log()`, `Remove()`, `Clear()` — update to use interface members (no Myra-specific API changes expected)
  - Remove all `using Myra.*` from file

- [ ] **T3: Replace GUIManager.cs with GuiManagerImpl (0.5 pt)**
  - Delete or deprecate old `GUIManager.cs` in favor of new `GuiManagerImpl` from Sprint 2
  - If keeping backward compatibility, add a thin static wrapper:
    ```csharp
    // Old API still works but delegates to new implementation
    public static class GUIManager {
        private static IGuiManager Impl => EngineResolver.GetEngine();
        public static void Init(Game game, int width, int height) => Impl.Init(game, width, height);
        // ... proxy remaining methods
    }
    ```
  - Or break the API — decide based on whether external users depend on `GUIManager`

- [ ] **T4: Replace Canvas.cs with CanvasImpl (0.5 pt)**
  - Delete or deprecate old `Canvas.cs` in favor of new `CanvasImpl` from Sprint 2
  - If keeping backward compatibility, add a thin static wrapper similar to T3 above
  - Ensure world-space camera conversion still works via `Camera.MainCamera.WorldToScreen()`

- [ ] **T5: Update MainGame.cs (0.5 pt)**
  - Remove direct `MyraEnvironment.Game = this;` call
  - GUI initialization should now go through the engine resolver or static wrapper:
    ```csharp
    // Before: MyraEnvironment.Game = this;
    //         GUIManager.Init(this, width, height);

    // After (option A): EngineResolver.GetEngine().Init(this, width, height);
    // After (option B - if wrapper kept): GUIManager.Init(this, width, height); // internally calls engine
    ```
  - Remove `using Myra;` from MainGame.cs if no longer needed

---

## Acceptance Criteria

- [ ] **Zero `using Myra.*` statements in production code** (excluding test/playground files — those are Sprint 4)
- [ ] `StickyLog` works end-to-end: creates grid, adds labels, updates log entries, toggles visibility with R key
- [ ] `StickyLog` uses only `CoreEssentials.GUI` interfaces and factory methods
- [ ] Old `GUIManager.cs` and `Canvas.cs` are either replaced or wrapped to delegate to new implementations
- [ ] `MainGame.cs` no longer directly accesses Myra types — initialization goes through engine resolver or wrapper
- [ ] Project builds cleanly (`dotnet build CoreEssentials`) — 0 errors

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

- **Backward compatibility decision:** If any external users already depend on `GUIManager.Init()` or `new Canvas()`, keep thin static wrappers that delegate to the new implementation. This minimizes breaking changes while still achieving the goal of hiding Myra types internally.
- **StickyLog focus check:** The existing StickyLog uses `Grid` properties like `_grid.Visible`. Ensure `IGrid` exposes a `Visible` property (it should inherit from `IWidget` which has it).
- **MainGame.cs other Myra usage:** Verify that MainGame doesn't use any other Myra types beyond `MyraEnvironment.Game`. If it does, those need to be handled too.

---

*Created: 2026-07-20 | Part of GUI System Refactoring Project*
