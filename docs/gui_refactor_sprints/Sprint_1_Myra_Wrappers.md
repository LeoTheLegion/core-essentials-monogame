# Sprint 1 — Myra Engine Wrappers 🔧

**Points:** 5  
**Status:** Not Started (depends on Sprint 0)  
**Sprint Goal:** Implement widget wrapper classes in `engines/myra/` that wrap each Myra type and implement the corresponding interface from Sprint 0. These are thin delegates — no logic, just pass-through to Myra.

---

## Tasks

- [ ] **T1: Create `WidgetBase.cs` (1 pt)** 🔒
  - Base class for all widget wrappers — holds the wrapped Myra widget reference
  - Implements `IWidget`: delegates all properties/methods to underlying Myra widget
    - `Width { get; set; }` → `_myraWidget.Width = value`
    - `Visible { get; set; }` → `_myraWidget.Visible = value`
    - `Left`, `Top`, `Margin`, `HorizontalAlignment`, etc. — all pass-through
  - Constructor: `protected WidgetBase(Myra.Graphics2D.UI.Widget myraWidget)`
  - Protected accessor: `MyraWidget { get; }` for derived classes

- [ ] **T2: Create `ContainerWidget.cs` (1 pt)** 🔒
  - Wraps `Myra.Graphics2D.UI.Panel` → implements `IContainer`, `IPanel`
  - Inherits from `WidgetBase` with `Panel` as underlying type
  - Implements `IContainer`:
    - `AddChild(IWidget widget)` — unwrap the IWidget to get its Myra instance, add to `_panel.Widgets.Add()`
    - `RemoveChild(IWidget widget)` — same unwrapping for removal
    - `ClearChildren()` → `_panel.Widgets.Clear()`
  - Implements `IPanel`:
    - `Background { get; set; }` — converts IBrush to Myra Brush, assigns to `_panel.Background`

- [ ] **T3: Create `ButtonWidget.cs` (1 pt)** ⭐ User-facing factory
  - Wraps `Myra.Graphics2D.UI.Button` → implements `IButton`
  - Inherits from `WidgetBase` with `Button` as underlying type
  - Implements `IButton`:
    - `Text { get; set; }` — delegates to button's Content or Text property
    - `Click` event — registers a C# event on ButtonWidget that fires when Myra button's `.Click += (s, e) => _onClick?.Invoke()` is triggered
  - **Static factory method:** `CreateTextButton(string text)` → creates ButtonWidget wrapping a new Myra TextButton with the given text. Replaces Myra's `Button.CreateTextButton()`.

- [ ] **T4: Create `LabelWidget.cs` (0.5 pt)** 🔒
  - Wraps `Myra.Graphics2D.UI.Label` → implements `ILabel`
  - Inherits from `WidgetBase` with `Label` as underlying type
  - Constructor: `LabelWidget(string text)` — convenient factory-style constructor
  - Implements `ILabel`:
    - `Text { get; set; }` → `_label.Text = value`

- [ ] **T5: Create `GridWidget.cs` (0.5 pt)** 🔒
  - Wraps `Myra.Graphics2D.UI.Grid` → implements `IGrid`
  - Inherits from `WidgetBase` with `Grid` as underlying type
  - Constructor: `GridWidget()` — default constructor, no parameters needed
  - Implements `IGrid`:
    - `RowsProportions`, `ColumnsProportions` — expose Myra Grid's proportion collections
    - `RowSpacing`, `ColumnSpacing` → pass-through properties
    - Static helper methods: `SetRow(IWidget widget, int row)`, `SetColumn(IWidget widget, int col)`, `GetRow(IWidget widget)`, `GetColumn(IWidget widget)` — internally unwrap the IWidget to its Myra instance and call `Grid.SetRow/SetColumn/GetRow/GetColumn()`

- [ ] **T6: Create brush wrappers (0.5 pt)** 🔒
  - `Brushes/BrushBase.cs` — base class wrapping a Myra Brush, implements `IBrush`
    - Properties: `Color { get; }`, `Opacity` — pass-through to Myra Brush
  - `Brushes/SolidColorBrush.cs` — wraps `Myra.Graphics2D.Brushes.SolidBrush`
    - Constructor: `SolidColorBrush(Color color)` — creates Myra SolidBrush internally

---

## Acceptance Criteria

- [ ] All wrapper classes exist in `engines/myra/Widgets/` and `engines/myra/Brushes/`
- [ ] Each wrapper correctly implements its corresponding interface from Sprint 0
- [ ] **All properties/methods delegate to the underlying Myra widget** — no custom logic, just pass-through
- [ ] `ButtonWidget.CreateTextButton()` works as a static factory method returning `IButton`
- [ ] `LabelWidget(string text)` and `GridWidget()` have convenient constructors
- [ ] Static grid helpers (`SetRow`, etc.) correctly unwrap IWidgets and call Myra Grid methods
- [ ] Project builds cleanly (`dotnet build CoreEssentials`) — 0 errors

---

## Deliverables

| File | Implements | Wraps | Visibility |
|------|-----------|-------|------------|
| `engines/myra/Widgets/WidgetBase.cs` | `IWidget` (abstract base) | `Myra.Graphics2D.UI.Widget` | 🔒 Internal |
| `engines/myra/Widgets/ContainerWidget.cs` | `IContainer`, `IPanel` | `Myra.Panel` | 🔒 Internal |
| `engines/myra/Widgets/ButtonWidget.cs` | `IButton` + static factory | `Myra.Button` / `TextButton` | ⭐ User-facing via factory |
| `engines/myra/Widgets/LabelWidget.cs` | `ILabel` | `Myra.Label` | ⭐ User-facing directly |
| `engines/myra/Widgets/GridWidget.cs` | `IGrid` + static helpers | `Myra.Grid` | 🔒 Internal (used by StickyLog) |
| `engines/myra/Brushes/BrushBase.cs` | `IBrush` (abstract base) | `Myra.Graphics2D.Brushes.Brush` | 🔒 Internal |
| `engines/myra/Brushes/SolidColorBrush.cs` | `IBrush` | `Myra.SolidBrush` | 🔒 Internal |

---

## Notes & Risks

- **Thin wrappers only** — these classes should have almost zero logic. They exist solely to bridge the interface-to-Myra gap. If you find yourself writing complex conversion logic, reconsider whether it belongs in an adapter layer instead.
- **Unwrapping IWidgets:** When `AddChild(IWidget widget)` receives a user-created widget, that widget is already one of our wrapper types (e.g., `ButtonWidget`). We need to access its underlying Myra instance — use a protected property or cast pattern.
- **Event handling on Button:** Myra uses `(sender, EventArgs) => {}` delegates for click events. Wrap this in a C# event (`Action<IButton, EventArgs> Click`) that users can subscribe to cleanly.
- Verify all types still use `Microsoft.Xna.Framework.Vector2`, not any external type.

---

*Created: 2026-07-20 | Part of GUI System Refactoring Project*
