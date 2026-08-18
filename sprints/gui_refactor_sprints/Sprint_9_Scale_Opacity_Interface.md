# Sprint 9 — Add Scale and Opacity to IWidget Interface 🎛️

**Points:** 2  
**Status:** ✅ Completed  
**Sprint Goal:** Add `Scale` (Vector2) and `Opacity` (float) properties to the `IWidget` interface, enabling visual transformation effects like fade-out animations and scale pulsing across all GUI widgets.

---

## Background & Problem Statement

Issue #29 reported that migrating `ShootingGallery` from CoreEssentials `v0.10.2` → `v0.13.1` lost several visual effects because `IWidget` doesn't expose `Scale` or `Opacity`:

- **Floating text fade-out** — setting `_label.Opacity = _transparency` no longer compiles
- **Text scale pulsing** — `_label.Scale = new Vector2(...)` for radiation effect animations
- **Runtime text scaling** — `SetScale()` on `TextEntity` is now a no-op

### Root Cause

The `IWidget` interface (the base abstraction for all GUI widgets) only exposes:
- `Width`, `Height`, `Position`, `Visible`, `Enabled`
- `Margin`, `HorizontalAlignment`, `VerticalAlignment`

But Myra's underlying `Myra.Graphics2D.UI.Widget` class **already has** both `Scale` (Vector2) and `Opacity` (float). The CoreEssentials wrapper layer just wasn't exposing them.

---

## Design Decisions

### Why add to IWidget, not just ILabel?

The issue originally suggested adding these properties at least to `ILabel`, but since **all** GUI widgets inherit from `IWidget` (via the hierarchy: `ILabel : IWidget`, `IButton : IWidget`, `IPanel : IContainer : IWidget`, etc.), adding to `IWidget` gives every widget type access to these transformations. This is more consistent and useful — any widget could benefit from fade-out or scaling animations.

### Implementation: Delegate through WidgetWrapper

Myra's `Widget.Scale` returns a global::Myra.Vector2 (not Microsoft.Xna.Framework.Vector2), so we need conversion in the getter/setter. Myra's `Opacity` is already float, so it's a direct pass-through.

```csharp
// In WidgetWrapper.cs
public Vector2 Scale
{
    get => new(MyraWidget.Scale.X, MyraWidget.Scale.Y);
    set => MyraWidget.Scale = new global::Myra.Vector2(value.X, value.Y);
}

public float Opacity
{
    get => MyraWidget.Opacity;
    set => MyraWidget.Opacity = value;
}
```

### Non-breaking change

Adding new properties to an interface is technically a breaking change for existing implementations (they must now implement the members), but since:
1. The concrete wrappers (`WidgetWrapper` and its subclasses) are in our control
2. No external consumers implement `IWidget` directly
3. This fills a gap that was previously inaccessible

This is effectively non-breaking for API users — their code won't break, they just gain new capabilities.

---

## Tasks

- [x] **T1: Add Scale and Opacity to IWidget interface (0.5 pt)** ✅
  - File: `CoreEssentials/src/GUI/types/IWidget.cs`
  - Added `Vector2 Scale { get; set; }` with XML documentation
  - Added `float Opacity { get; set; }` with XML documentation

- [x] **T2: Implement delegation in WidgetWrapper (0.5 pt)** ✅
  - File: `CoreEssentials/src/GUI/engines/myra/Widgets/WidgetWrapper.cs`
  - Implemented `Scale` property with Vector2 conversion (Myra.Vector2 ↔ Microsoft.Xna.Framework.Vector2)
  - Implemented `Opacity` property as direct pass-through to Myra

- [x] **T3: Update GUISystem.md documentation (0.5 pt)** ✅
  - File: `docs/GUISystem.md`
  - Added Scale and Opacity to the widget properties table
  - Added example code showing fade-out animation and scale pulsing patterns

- [x] **T4: Create unit tests for Scale/Opacity (0.5 pt)** ✅
  - File: `CoreEssentials.Tests/GUI/ScaleOpacityTests.cs`
  - Tests verifying default values, setting/getting Scale and Opacity on different widget types

---

## Acceptance Criteria

- [x] `IWidget.Scale` returns and sets a `Vector2` correctly through the Myra wrapper
- [x] `IWidget.Opacity` returns and sets a `float` (0.0–1.0 range) correctly
- [x] Properties work on all widget types (labels, buttons, panels, containers)
- [x] Documentation includes usage examples for animations/transitions
- [ ] Visual verification in Playground — *create test scene to confirm fade-out and pulse effects*

---

## Deliverables

| File | Change | Points |
|------|--------|--------|
| ✅ `CoreEssentials/src/GUI/types/IWidget.cs` | Added `Scale { get; set; }` (Vector2) and `Opacity { get; set; }` (float) properties with XML docs | 0.5 |
| ✅ `CoreEssentials/src/GUI/engines/myra/Widgets/WidgetWrapper.cs` | Implemented Scale/Opacity delegation with Vector2 conversion | 0.5 |
| ✅ `docs/GUISystem.md` | Updated widget table + added animation examples | 0.5 |
| ✅ `CoreEssentials.Tests/GUI/ScaleOpacityTests.cs` | Created test file with default value and set/get tests | 0.5 |

**Total: ~2 pts** (rounded to **2**)

---

## Implementation Notes

### Vector2 Conversion

Myra uses its own `global::Myra.Vector2` type, not Microsoft.Xna.Framework.Vector2. The conversion is straightforward but must be explicit:

```csharp
// Getter: Myra.Vector2 → Xna.Vector2
get => new(MyraWidget.Scale.X, MyraWidget.Scale.Y);

// Setter: Xna.Vector2 → Myra.Vector2  
set => MyraWidget.Scale = new global::Myra.Vector2(value.X, value.Y);
```

### Why not add to XML serializer?

The `GuiSerializer` currently parses layout attributes like `Width`, `Height`, `Position`, etc. Adding `Scale` and `Opacity` as XML attributes could be a future enhancement (Sprint 10+), but for now we focus on the interface exposure. Runtime animation of these properties is the primary use case from Issue #29.

### Future Considerations

- **XML support:** Could add `scale="1,1"` and `opacity="0.8"` attributes to container elements
- **Default values:** Myra defaults Scale to 1,1 and Opacity to 1.0 — these are sensible defaults we don't need to override
- **Animation helpers:** Future sprint could add helper methods like `FadeTo(float opacity, float duration)` or `PulseScale(Vector2 target, float duration)`
