# Sprint 0 — Core Interface Definitions 📋

**Points:** 5  
**Status:** Not Started (no dependencies)  
**Sprint Goal:** Define all pure interface types in `types/` folder with zero references to Myra or any external GUI library.

---

## Tasks

- [ ] **T1: Create `IGuiManager.cs` (2 pts)** ⭐ User-facing
  - Lifecycle: `Init(Game game, int width, int height)`, `Shutdown()`
  - Widget management: `AddWidget(IWidget widget)`, `RemoveWidget(IWidget widget)`
  - Rendering: `Draw(GameTime gameTime)`
  - Root access: `Width { get; }`, `Height { get; }` — proxy to root panel dimensions
  - Focus queries: `IsAnyWidgetFocused()`, `IsWidgetFocused(IWidget widget)`
  - Desktop management (optional): `SetDesktop(IDesktop desktop)`, `GetRootPanel()`

- [ ] **T2: Create `ICanvas.cs` (1 pt)** ⭐ User-facing
  - Positioning: `Position { get; set; }`, `SetPosition(Vector2 position)`
  - Space type: `IsScreenSpace { get; }` — screen space vs world space toggle
  - Widget management: `AddWidget(IWidget widget)`, `RemoveWidget(IWidget widget)`
  - Lifecycle: `Update(GameTime gameTime)`, `CleanUp()`
  - Canvas should be a class (not interface) since it needs mutable state and constructor behavior

- [ ] **T3: Create base widget interfaces (2 pts)** ⭐ User-facing
  - `IWidget` — Base abstraction for all UI elements:
    - Properties: `Width`, `Height`, `Visible`, `Enabled`, `IsMouseInside`, `IsKeyboardFocused`
    - Positioning: `Left`, `Top`, `X`, `Y`, `Margin` (Thickness), `HorizontalAlignment`, `VerticalAlignment`
  - `IContainer : IWidget` — Has child widgets:
    - Methods: `AddChild(IWidget widget)`, `RemoveChild(IWidget widget)`, `ClearChildren()`
    - Properties: `Children { get; }`, `Widgets { get; }` (collection access)
  - `IPanel : IContainer` — Panel-specific styling:
    - Properties: `Background { get; set; }` (IBrush), `BorderThickness`

- [ ] **T4: Create control interfaces (1 pt)** 🔒 Mostly internal, some user-facing
  - `IButton : IWidget` — Clickable button:
    - Events/Delegates: `Click` event or callback registration (`OnClick(Action)`)
    - Properties: `Text { get; set; }`, `Enabled { get; set; }` (override base)
  - `ILabel : IWidget` — Text display:
    - Properties: `Text { get; set; }`, `Font`, `TextColor { get; set; }`
  - `IGrid : IContainer` — Grid layout container:
    - Properties: `RowsProportions`, `ColumnsProportions`, `RowSpacing`, `ColumnSpacing`
    - Static helpers: `SetRow(IWidget, int)`, `SetColumn(IWidget, int)`, `GetRow(IWidget)`, `GetColumn(IWidget)`

- [ ] **T5: Create styling interfaces (0.5 pt)** 🔒 Internal use only
  - `IBrush` — Background/styling abstraction:
    - Properties: `Color { get; }` (MonoGame Color), `IsSolid`, `Opacity`
  - `IColor` — Color representation for text, borders, etc.

---

## Acceptance Criteria

- [ ] All interface files exist in `CoreEssentials/src/gui/types/` folder
- [ ] **ZERO references to Myra types** — no `using Myra.*`, no Myra type names anywhere in `types/`
- [ ] Project builds cleanly (`dotnet build CoreEssentials`) — 0 errors, only existing warnings
- [ ] All interfaces use `Microsoft.Xna.Framework.Vector2`, NOT any external Vector2

---

## Deliverables

| File | Interface / Type | Visibility | Notes |
|------|------------------|------------|-------|
| `types/IGuiManager.cs` | `IGuiManager` | ⭐ PUBLIC — user-facing API | Lifecycle + widget management + rendering |
| `types/ICanvas.cs` | `ICanvas` (class) | ⭐ PUBLIC — user-facing API | Canvas positioning & container logic |
| `types/IWidget.cs` | `IWidget` | ⭐ PUBLIC — base for all widgets | Properties: size, position, visibility, focus |
| `types/IContainer.cs` | `IContainer : IWidget` | ⭐ PUBLIC — containers have children | AddChild/RemoveChild/ClearChildren |
| `types/IPanel.cs` | `IPanel : IContainer` | public (user-facing) | Background brush styling |
| `types/IButton.cs` | `IButton : IWidget` | ⭐ PUBLIC — user creates buttons | Click event, Text property |
| `types/ILabel.cs` | `ILabel : IWidget` | ⭐ PUBLIC — user creates labels | Text display |
| `types/IGrid.cs` | `IGrid : IContainer` | public (used by StickyLog) | Grid layout with proportions/spacings |
| `types/IBrush.cs` | `IBrush` | 🔒 Internal only | Background/styling abstraction |

---

## Notes & Risks

- **Critical:** These interfaces MUST NOT reference Myra. They are the *contract* that allows engine swapping later (including your future custom GUI engine).
- `IWidget`, `IButton`, and `ILabel` are the primary user-facing types — design them with clean, intuitive APIs.
- Consider whether `IGuiManager` should be a static class or an instance-based GameSystem. Physics uses `PhysicsEngine : GameSystem + IFixedUpdateGameSystem`. GUI likely needs similar pattern.
- `ICanvas` is used as a **class** (not interface) because it has mutable state, constructors with parameters (`isScreenSpace`), and needs to be instantiated by users. The interface approach was only needed for the widget hierarchy where engine swapping matters most.

---

*Created: 2026-07-20 | Part of GUI System Refactoring Project*
