# Sprint 2 — GuiManagerImpl, CanvasImpl & Factory Layer ⚙️

**Points:** 5  
**Status:** Not Started (depends on Sprints 0–1)  
**Sprint Goal:** Implement the engine-level `GuiManagerImpl` and `CanvasImpl`, create the factory layer (`WidgetFactory`, `CanvasFactory`), and build the `EngineResolver` for future custom engine support.

---

## Tasks

- [ ] **T1: Create `EngineResolver.cs` (0.5 pt)** 🔒 Internal
  - Static class that holds a reference to the active `IGuiManager` implementation
  - Methods:
    - `SetEngine(IGuiManager engine)` — sets which engine backend to use (default: Myra)
    - `GetEngine()` → returns current engine instance
  - Default at startup: points to `GuiManagerImpl` (Myra version)
  - **Purpose:** Enables `EngineResolver.SetEngine(new CustomGuiEngine())` for future custom engines — zero user code changes needed

- [ ] **T2: Create `ColorAdapter.cs` (0.5 pt)** 🔒 Internal
  - Static helper class that converts between MonoGame `Color` and Myra brush types
  - Methods:
    - `ToMyraBrush(Color color)` → returns `Myra.Graphics2D.Brushes.SolidBrush`
    - `WithAlpha(this Color color, byte alpha)` → extension method for convenient alpha adjustment (used in StickyLog)
  - **Purpose:** Users pass `Color`, never Myra brush types — conversion happens internally

- [ ] **T3: Create `GuiManagerImpl.cs` (2 pts)** ⭐ User-facing via GameSystem
  - Implements `IGuiManager`
  - Wraps a single `Myra.Graphics2D.UI.Desktop` instance
  - Static properties for root panel access: `Width { get; }`, `Height { get; }`
  - Methods:
    - `Init(Game game, int width, int height)` — creates Desktop, sets MyraEnvironment.Game internally (replaces direct `MyraEnvironment.Game = this` from MainGame.cs), initializes root Panel
    - `AddWidget(IWidget widget)` — unwrap IWidget to get Myra instance, add to `_desktop.Root.Widgets`
    - `RemoveWidget(IWidget widget)` — same unwrapping for removal
    - `Draw(GameTime gameTime)` → calls `_desktop.Render()`
    - `IsAnyWidgetFocused()`, `IsWidgetFocused(IWidget widget)` — recursive focus check across widget tree (migrate existing logic from current GUIManager.cs)
  - **Key change:** Removes direct Myra dependency from user-facing code. MainGame.cs will call `EngineResolver.GetEngine().Init(...)` instead of setting `MyraEnvironment.Game` directly.

- [ ] **T4: Create `CanvasImpl.cs` (1 pt)** ⭐ User-facing
  - Implements `ICanvas`
  - Wraps a single `Myra.Graphics2D.UI.Panel` as the root container

  - Methods:
    - Constructor: `CanvasImpl(bool isScreenSpace = true)` — creates Panel, registers with GuiManagerImpl via `AddWidget()`
    - `SetPosition(Vector2 position)` — updates internal Position and delegates to underlying Myra Panel's screen coordinates
    - `AddWidget(IWidget widget)`, `RemoveWidget(IWidget widget)` — delegate to internal Panel's Widgets collection
    - `Update(GameTime gameTime)` — if world space, converts canvas Position via `Camera.MainCamera.WorldToScreen()` (migrate existing logic from current Canvas.cs); then updates Myra Panel position
    - `CleanUp()` — clears children, removes from GuiManagerImpl's widget list

- [ ] **T5: Create `WidgetFactory.cs` (1 pt)** ⭐ User-facing
  - Static factory class that creates widgets via interfaces using the active engine
  - Methods:
    - `CreatePanel()` → returns `IPanel` — delegates to CanvasImpl or ContainerWidget constructor
    - `CreateLabel(string text)` → returns `ILabel` — delegates to LabelWidget constructor
    - `CreateTextButton(string text)` → returns `IButton` — delegates to ButtonWidget.CreateTextButton()
  - **Purpose:** Users call `WidgetFactory.CreateTextButton("Play")` and get back an interface. They never see `new ButtonWidget()` or Myra types.

- [ ] **T6: Create `CanvasFactory.cs` (0.5 pt)** ⭐ User-facing
  - Static factory class for canvas creation
  - Methods:
    - `CreateScreenSpace()` → returns `ICanvas` — delegates to CanvasImpl(true)
    - `CreateWorldSpace()` → returns `ICanvas` — delegates to CanvasImpl(false)

---

## Acceptance Criteria

- [ ] `EngineResolver` correctly manages the active engine instance and defaults to Myra ✅
- [ ] `ColorAdapter` converts MonoGame Color to Myra brushes seamlessly ✅
- [ ] `GuiManagerImpl` implements all `IGuiManager` members and handles initialization internally ✅
  - `MyraEnvironment.Game` is set inside `Init()`, not in MainGame.cs anymore
  - Focus-checking logic migrated from existing GUIManager.cs (recursive tree walk)
- [ ] `CanvasImpl` correctly handles both screen space and world space positioning ✅
  - World space: uses `Camera.MainCamera.WorldToScreen()` to convert position
  - Screen space: sets Position via Vector2 on underlying Panel
- [ ] `WidgetFactory` methods return interfaces, not concrete types ✅
- [ ] `CanvasFactory` creates canvases in both space modes ✅
- [ ] Project builds cleanly (`dotnet build CoreEssentials`) — 0 errors

---

## Deliverables

| File | Purpose | Visibility |
|------|---------|------------|
| `Internal/EngineResolver.cs` | Swaps GUI engine backend at runtime | 🔒 Internal only |
| `Internal/ColorAdapter.cs` | Color ↔ Myra brush conversion | 🔒 Internal only |
| `engines/myra/GuiManagerImpl.cs` | Wraps Myra Desktop, implements IGuiManager | ⭐ User-facing via GameSystem |
| `engines/myra/CanvasImpl.cs` | Wraps Myra Panel as canvas, implements ICanvas | ⭐ User-facing |
| `factory/WidgetFactory.cs` | Creates widgets via interfaces (IPanel, IButton, etc.) | ⭐ User-facing API entry point |
| `factory/CanvasFactory.cs` | Creates canvases returning ICanvas | ⭐ User-facing API entry point |

---

## Notes & Risks

- **Focus-checking logic migration:** The existing `GUIManager.isWidgetFocused()` method does a recursive tree walk checking `IsMouseInside`, `IsTouchInside`, `IsKeyboardFocused`. This needs to be ported exactly — it handles nested containers, ContentControls (like buttons), and ComboViews.
- **MainGame.cs change:** Remove direct `MyraEnvironment.Game = this;` call. Instead, the engine's `Init()` method will handle Myra setup internally, or MainGame calls `EngineResolver.GetEngine().Init(this, width, height)`.
- **Position via Vector2:** All widget positioning uses `Vector2 Position` (not separate X/Y/Left/Top). CanvasImpl delegates position updates to the underlying Myra Panel's screen coordinates.

- **World-to-screen conversion:** `CanvasImpl.Update()` needs access to `Camera.MainCamera`. Ensure Camera system is initialized before GUI, or add a null-guard.

---

*Created: 2026-07-20 | Part of GUI System Refactoring Project*
