# Sprint 8 — XML Background Brush Support 🎨

**Points:** 2  
**Status:** Not Started (depends on Sprints 0–7)  
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

- [ ] **T1: Add color-to-brush parsing utility (0.5 pt)** ⭐
  - File: `CoreEssentials/src/gui/GuiSerializer.cs` (or new internal helper)
  - Create a private static method:
    ```csharp
    private static IBrush? ParseBackgroundAttribute(XElement element, IContentManager? contentManager = null)
    {
        var bgAttr = element.Attribute("Background")?.Value;
        if (string.IsNullOrEmpty(bgAttr)) return null;

        Color color = ParseColorString(bgAttr); // see below
        float opacity = 1.0f;

        // Optional Opacity override attribute
        if (element.Attribute("Opacity") != null &&
            float.TryParse(element.Attribute("Opacity")!.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out float op))
        {
            opacity = op;
        }

        var brush = color.AsBrush();
        brush.Opacity = opacity;
        return brush;
    }

    private static Color ParseColorString(string value)
    {
        // 1. Try hex ARGB: #AARRGGBB or #RRGGBB
        if (value.StartsWith("#", StringComparison.OrdinalIgnoreCase))
        {
            var hex = value.Substring(1);
            if (hex.Length == 6) return ParseHexRGB(hex);   // RGB → opaque
            if (hex.Length == 8) return ParseHexARGB(hex);  // ARGB with alpha
        }

        // 2. Try named colors
        var named = value.Trim();
        return named.ToUpperInvariant() switch
        {
            "BLACK"   => Color.Black,
            "WHITE"   => Color.White,
            "RED"     => new Color(255, 0, 0),
            "GREEN"   => new Color(0, 128, 0),
            "BLUE"    => new Color(0, 0, 255),
            "YELLOW"  => new Color(255, 255, 0),
            "GRAY"    => new Color(128, 128, 128),
            _         => throw new FormatException($"Unknown color: '{value}'"),
        };
    }

    private static Color ParseHexRGB(string hex)
    {
        byte r = Convert.ToByte(hex.Substring(0, 2), 16);
        byte g = Convert.ToByte(hex.Substring(2, 2), 16);
        byte b = Convert.ToByte(hex.Substring(4, 2), 16);
        return new Color(r, g, b, 255); // fully opaque
    }

    private static Color ParseHexARGB(string hex)
    {
        byte a = Convert.ToByte(hex.Substring(0, 2), 16);
        byte r = Convert.ToByte(hex.Substring(2, 2), 16);
        byte g = Convert.ToByte(hex.Substring(4, 2), 16);
        byte b = Convert.ToByte(hex.Substring(6, 2), 16);
        return new Color(r, g, b, a);
    }
    ```

- [ ] **T2: Wire ParseBackgroundAttribute into LoadGridFromXml + LoadPanelFromXml (0.5 pt)** ⭐
  - File: `CoreEssentials/src/gui/GuiSerializer.cs`
  - In `LoadGridFromXml()`, after `ApplyBaseProperties(grid, element)`:
    ```csharp
    var background = ParseBackgroundAttribute(element);
    if (background != null) grid.Background = background;
    ```
  - In `LoadPanelFromXml()`, same pattern:
    ```csharp
    var background = ParseBackgroundAttribute(element);
    if (background != null) panel.Background = background;
    ```
  - Labels/buttons remain unchanged (they use TextColor, not Background)

- [ ] **T3: Update StickyLogLayout.xml (0.25 pt)**
  - File: `CoreEssentials/Content/StickyLogLayout.xml`
  - Replace with declarative background:
    ```xml
    <Grid Width="300" Height="100" RowSpacing="8" ColumnSpacing="8" 
          Visible="true" Background="#64000000">
        <!-- No child widgets — labels added dynamically at runtime -->
    </Grid>
    ```
  - `#64` = 100/255 ≈ 39% opacity (matches original `Color.Black; c.A = 100`)

- [ ] **T4: Refactor StickyLog.LoadGUI() to remove imperative background (0.25 pt)**
  - File: `CoreEssentials/src/debugging/StickyLog.cs`
  - Remove the 4-line color setup block — everything is now in XML:
    ```csharp
    // After refactor — just load and add:
    _grid = GuiSerializer.LoadGridFromXmlEmbedded("CoreEssentials.Content.StickyLogLayout.xml");
    _canvas.AddChild(_grid);
    ```

- [ ] **T5: Update tests (0.25 pt)**
  - File: `CoreEssentials.Tests/GUI/GuiSerializerTests.cs`
  - Add test verifying Background attribute parsing for various formats:
    - Named color: `"Black"`
    - Hex RGB: `"#FF000000"`
    - Hex ARGB with alpha override

---

## Acceptance Criteria

- [ ] `Background="#64000000"` on any container element produces an `IBrush` with correct color and opacity
- [ ] `StickyLog.LoadGUI()` has **zero** imperative background code — everything is in XML
- [ ] StickyLog visual output is identical to pre-Sprint 8 (same semi-transparent black grid)
- [ ] All existing tests pass (`dotnet test CoreEssentials.Tests`)

---

## Deliverables

| File | Change | Points |
|------|--------|--------|
| `CoreEssentials/src/gui/GuiSerializer.cs` | Added `ParseBackgroundAttribute()` + `ParseColorString()`, wired into `ApplyBaseProperties()` | 0.5 |
| `CoreEssentials/Content/StickyLogLayout.xml` | Added `Background="#64000000"` attribute | 0.25 |
| `CoreEssentials/src/debugging/StickyLog.cs` | Removed imperative color setup (now in XML) | 0.25 |
| `CoreEssentials.Tests/GUI/GuiSerializerTests.cs` | Added background brush parsing tests | 0.25 |

**Total: ~1.75 pts** (rounded to **2**)

---

## Notes & Risks

- **IWidget.Background availability:** Need to verify that the base `IWidget` interface exposes `.Background`. If only `IGrid` and `IPanel` have it, we may need to handle this per-type rather than in a shared `ApplyBaseProperties()` method.
- **Color format consistency:** MonoGame doesn't have a built-in hex parser. We'll implement manually — keep it simple (RGB/ARGB hex + named colors). No need for CSS-style color names beyond the basics StickyLog actually uses.
- **Opacity interaction with Background:** If both `Background="#AARRGGBB"` and `Opacity="0.5"` are present, the explicit `Opacity` attribute should override/compose with the alpha in the hex value. Priority: `Opacity` attribute > embedded alpha in hex > default 1.0.

---

*Draft created: 2026-07-26 | Follow-up to Sprint 7 StickyLog XML Refactor*
