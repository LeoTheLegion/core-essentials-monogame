# Sprint 8 — XML Background Brush Support 🎨

**Points:** 2  
**Status:** ✅ Completed  
**Sprint Goal:** Enable `Background` attribute parsing in `GuiSerializer` so that solid color brushes can be set declaratively from XML, eliminating the need for imperative background setup in consumers like StickyLog.

---

## Background & Problem Statement

In Sprint 7, `StickyLog.LoadGUI()` was migrated to use `GuiSerializer.LoadGridFromXmlEmbedded()`, but one block of code remains imperative:

```csharp
// Current — still imperative
_grid = GuiSerializer.LoadGridFromXmlEmbedded("CoreEssentials.Content.StickyLogLayout.xml");

Color c = Color.Black;
c.A = 100;
_grid.Background = c.AsBrush(); // ← needs to move to XML
```

The goal of Sprint 7 was to reduce imperative boilerplate. This remaining 4-line block **should** be declarative in the XML layout file. However, `IBrush` is a composite interface (contains `Color`, `IsSolid`, `Opacity`) — it can't be expressed as a single XML attribute without help.

---

## Design Decisions

### Approach: Parse color strings → create IBrush internally

**Why not serialize full IBrush to XML?** The `IBrush` interface is an abstraction over engine-specific brush types (`Myra.SolidColorBrush`, etc.). Serializing the full object would require engine-specific XML tags, breaking our abstraction layer. Instead, we parse **simple color representations** in XML and create `IBrush` instances internally via `ColorAdapter`.

### Supported color formats:
- **Named colors:** `"Black"`, `"Red"`, `"White"`, etc. (case-insensitive)
- **Hex ARGB:** `"#AARRGGBB"` — e.g., `"#64000000"` for black with ~39% opacity
- **Hex RGB:** `"#RRGGBB"` — e.g., `"#000000"` defaults to fully opaque

### Attribute design:
```xml
<Grid Width="300" Height="100" Background="#64000000">
    <!-- Background parsed as IBrush internally -->
</Grid>
```

Simple one attribute. No need for separate color + opacity attributes since hex ARGB covers both. If a user wants named colors with custom alpha, they can use `Opacity` as a secondary override:

```xml
<Grid Width="300" Height="100" Background="Black" Opacity="0.4">
    <!-- Background = SolidColorBrush(Black) with Opacity = 0.4 -->
</Grid>
```

### Key constraint: `.Background` is NOT on `IWidget` or `IContainer`
- Only `IGrid` and `IPanel` expose `.Background { get; set; }` (added in Sprint 3 for StickyLog)
- `ApplyBaseProperties()` operates on base `IWidget` — it **cannot** set Background generically
- **Solution:** Handle Background per-type, inside each `LoadXFromXml()` method:
  - In `LoadGridFromXml()`: call `ParseBackgroundAttribute(element)` then cast to `IGrid`
  - In `LoadPanelFromXml()`: same pattern with `IPanel`
  - Labels/buttons don't need Background (they use TextColor instead)

---

## Tasks

- [x] **T1: Add color-to-brush parsing utility (0.5 pt)** ✅
  - File: `CoreEssentials/src/gui/GuiSerializer.cs`
  - Added private static methods:
    - `ParseBackgroundAttribute(XElement element)` → parses `Background` + optional `Opacity` attributes
    - `ParseColorString(string value)` → supports hex ARGB (`#AARRGGBB`), hex RGB (`#RRGGBB`), and named colors (Black, White, Red, Green, Blue, Yellow, Gray)
    - `ParseHexRGB(string hex)` / `ParseHexARGB(string hex)` → byte-level hex parsing
  - Added `using CoreEssentials.GUI.Internal;` for `ColorAdapter.AsBrush()`
  - ⚠️ Fixed: `new Color((byte)r, ...)` casts needed to avoid ambiguity between `Color(int,int,int,int)` and `Color(byte,byte,byte,byte)

- [x] **T2: Wire ParseBackgroundAttribute into LoadGridFromXml + LoadPanelFromXml (0.5 pt)** ✅
  - File: `CoreEssentials/src/gui/GuiSerializer.cs`
  - In `LoadGridFromXml()`: calls `ParseBackgroundAttribute(element)` → sets `grid.Background` if non-null
  - In `LoadPanelFromXml()`: same pattern with `panel.Background`
  - Replaced placeholder Background handling in `LoadPanelFromXml()` (old comment about "simplified for now")

- [x] **T3: Update StickyLogLayout.xml (0.25 pt)** ✅
  - File: `CoreEssentials/Content/StickyLogLayout.xml`
  - Added `Background="#64000000"` attribute to `<Grid>` element
  - Updated XML comment from "IBrush cannot be expressed as a simple XML attribute" → actual declarative approach

- [x] **T4: Refactor StickyLog.LoadGUI() to remove imperative background (0.25 pt)** ✅
  - File: `CoreEssentials/src/debugging/StickyLog.cs`
  - Removed the 4-line color setup block (`Color c = Color.Black; c.A = 100; _grid.Background = c.AsBrush();`)
  - Updated comment to reference declarative XML approach

- [x] **T5: Update tests (0.25 pt)** ✅
  - File: `CoreEssentials.Tests/GUI/GuiSerializerTests.cs`
  - Added 5 new test methods:
    - `LoadGridFromXml_BackgroundHexARGB_ParsesCorrectly` — validates `#64000000` parsing
    - `LoadGridFromXml_BackgroundNamedColor_ParsesCorrectly` — validates named color support
    - `LoadPanelFromXml_BackgroundHexRGB_ParsesCorrectly` — validates hex ARGB on panels
    - `LoadGridFromXml_BackgroundWithOpacityOverride_ParsesCorrectly` — validates `Opacity="0.4"` override
    - `LoadWidgetFromXml_NoBackground_ReturnsNullBackground` — validates null when no attribute

---

## Acceptance Criteria

- [x] `Background="#64000000"` on any container element produces an `IBrush` with correct color and opacity
- [x] `StickyLog.LoadGUI()` has **zero** imperative background code — everything is in XML
- [ ] StickyLog visual output is identical to pre-Sprint 8 (same semi-transparent black grid) — *visual verification needed in Playground*
- [x] All existing tests pass (`dotnet test CoreEssentials.Tests`) — 14/14 GuiSerializer tests passing

---

## Deliverables

| File | Change | Points |
|------|--------|--------|
| ✅ `CoreEssentials/src/gui/GuiSerializer.cs` | Added `ParseBackgroundAttribute()`, `ParseColorString()`, `ParseHexRGB()`, `ParseHexARGB()`; wired into `LoadGridFromXml()` and `LoadPanelFromXml()` | 0.5 |
| ✅ `CoreEssentials/Content/StickyLogLayout.xml` | Added `Background="#64000000"` attribute, updated comments | 0.25 |
| ✅ `CoreEssentials/src/debugging/StickyLog.cs` | Removed imperative color setup (now in XML) | 0.25 |
| ✅ `CoreEssentials.Tests/GUI/GuiSerializerTests.cs` | Added 5 background brush parsing tests | 0.25 |

**Total: ~1.75 pts** (rounded to **2**)

---

## Notes & Risks

- ✅ **IWidget.Background availability:** Verified — only `IGrid` and `IPanel` expose `.Background`, handled per-type inside each `LoadXFromXml()` method (not in shared `ApplyBaseProperties`).
- ✅ **Color format consistency:** Implemented manual hex parser supporting `#AARRGGBB`, `#RRGGBB`, and 7 named colors. Named colors use MonoGame's standard values (Green = `(0,128,0)` not CSS green `(0,128,0)`).
- ✅ **Opacity interaction with Background:** Implemented — `ParseBackgroundAttribute()` creates brush from color (including embedded alpha), then sets `.Opacity = opacity`. The `SolidColorBrush.ApplyOpacity()` method multiplies the stored base color's alpha by `_opacity`, so both compose multiplicatively. Priority: explicit `Opacity` attribute overrides to a fixed value, independent of hex alpha.

---

*Draft created: 2026-07-26 | Follow-up to Sprint 7 StickyLog XML Refactor*
