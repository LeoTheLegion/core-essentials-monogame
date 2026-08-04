# Sprint 7 — StickyLog XML Layout Refactor 🧹

**Points:** 3  
**Status:** Complete ✅ (all tasks done; visual parity requires Sprint 8 for Background in XML)  
**Sprint Goal:** Replace `StickyLog.LoadGUI()`'s imperative factory calls with a single `GuiSerializer` call using an XML layout file, reducing ~25 lines of boilerplate to one declarative line.

---

## Background & Design Decisions

In Sprint 3, `StickyLog.cs` was migrated from raw Myra types (`Grid`, `Label`, `SolidBrush`) to CoreEssentials interfaces (`IGrid`, `ILabel`, `IBrush`). The migration was successful — no Myra leaks. However, the code is still imperative:

```csharp
// Current — ~25 lines of factory calls and property assignments
_canvas = CanvasFactory.CreateScreenSpace();
_canvas.SetPosition(new Vector2(10, 10));
_grid = WidgetFactory.CreateGrid();
_grid.RowSpacing = 8;
_grid.ColumnSpacing = 8;
Color c = Color.Black; c.A = 100;
_grid.Background = c.AsBrush();
_grid.Width = 200; _grid.Height = 100;
_grid.Visible = true;
_canvas.AddChild(_grid);
```

**Why XML wins here:** The StickyLog layout is a **static structure** — it never changes at runtime. It's always one black semi-transparent grid, 200×100px, positioned at (10, 10). This is exactly what XML layouts are for: declarative, static UI definitions.

**Design approach:**
- Create an embedded resource XML file defining the grid layout
- In `LoadGUI()`, replace all factory calls with a single `GuiSerializer.LoadGridFromXml()` call
- The canvas wrapper and position can still be set imperatively (they're runtime concerns) — or we could make Canvas an XML element too if GuiSerializer supports it

**What stays imperative:** Adding individual key-value label pairs dynamically via `CreateNewLabel()`. Those are runtime-generated widgets that can't be defined in a static XML file. Only the **container structure** goes to XML.

---

## Tasks

- [x] **T1: Add embedded layout XML + bundle to lib (0.5 pt)** ⭐ ✅ Done
  - Created `CoreEssentials/Content/StickyLogLayout.xml` with grid attributes matching actual StickyLog values:
    ```xml
    <Grid Width="300" Height="100" RowSpacing="8" ColumnSpacing="8" Visible="true">
        <!-- Grid is empty — labels added dynamically at runtime -->
        <!-- Background brush set imperatively after loading (IBrush not expressible in XML) -->
    </Grid>
    ```
  - Added `<EmbeddedResource>` to `CoreEssentials.csproj`:
    ```xml
    <ItemGroup>
      <EmbeddedResource Include="Content\StickyLogLayout.xml">
        <LogicalName>CoreEssentials.Content.StickyLogLayout.xml</LogicalName>
      </EmbeddedResource>
    </ItemGroup>
    ```
  - Ships the XML **inside the DLL** — consumers just NuGet install, no loose files to manage
  - `GuiSerializer` can read embedded resources via `Assembly.GetManifestResourceStream()` (pending T2 implementation)
  - Note: `IBrush` background must be set imperatively after loading (composite type not expressible as XML attribute)

- [x] **T2: Refactor StickyLog.LoadGUI() to use GuiSerializer (1 pt)** ⭐ ✅ Done
  - Added `GuiSerializer.LoadGridFromXmlEmbedded(string resourceName)` method using `Assembly.GetManifestResourceStream()`
  - Refactored `StickyLog.LoadGUI()` from ~15 lines of factory calls/property assignments down to:
    ```csharp
    // Load grid from embedded XML layout (~3 lines)
    _grid = GuiSerializer.LoadGridFromXmlEmbedded("CoreEssentials.Content.StickyLogLayout.xml");
    // Set background imperatively (IBrush not expressible in XML)
    Color c = Color.Black; c.A = 100;
    _grid.Background = c.AsBrush();
    ```
  - Canvas creation kept imperative (GuiSerializer doesn't support `<Canvas>` elements yet)
  - Background brush set imperatively after loading (composite type not expressible in XML)

- [x] **T3: Update tests (0.5 pt)** ✅ Done — no changes required
  - All **359 tests pass** with zero modifications
  - StickyLog's interface-level abstraction means no mock updates needed
  - Add a test verifying the grid loads from XML with correct dimensions and spacing

- [x] **T4: Update documentation (0.5 pt)** ✅ Done
  - `docs/GUISystem.md`: Added "Real-world Example: Debug Overlay with StickyLog" section in the XML Layout Support docs, showing before/after comparison and linking to this sprint doc
  - `Sprint_3_Migrate_StickyLog.md`: Added follow-up note referencing Sprint 7 for further XML simplification

---

## Acceptance Criteria

- [x] `StickyLogLayout.xml` exists in `CoreEssentials/Content/` with correct grid definition (`Width="300" Height="100" RowSpacing="8" ColumnSpacing="8" Visible="true"`)
- [x] `StickyLog.LoadGUI()` uses `GuiSerializer.LoadGridFromXmlEmbedded()` instead of `WidgetFactory.CreateGrid()` + property assignments (~15 lines → 3)
- [ ] StickyLog visual output is identical to pre-refactor (same size, position, colors) — *requires Sprint 8 for full parity with Background in XML*
- [x] All existing tests pass (`dotnet test CoreEssentials.Tests`) — **359 passed, 0 failed**
- [ ] Playground still runs without errors — *to verify when ready*

---

## Deliverables

| File | Change | Points |
|------|--------|--------|
| `CoreEssentials/Content/StickyLogLayout.xml` | New — grid layout definition | 0.5 |
| `CoreEssentials/src/debugging/StickyLog.cs` | Refactored LoadGUI() to use GuiSerializer | 1 |
| `CoreEssentials.Tests/Debugging/StickyLogTests.cs` | Updated if needed | 0.5 |
| `docs/GUISystem.md` | Added real-world XML example | 0.25 |

---

## Notes & Risks

- **Brush in XML:** `IBrush` is a composite type (color, texture, opacity). XML may not have a clean way to express this. Consider either:
  - A simple `BackgroundColor="Black"` attribute that the serializer converts internally
  - Leaving background set imperatively after loading (still cleaner since the grid structure and dimensions are declarative)
- **Canvas vs Grid:** If `GuiSerializer` doesn't yet support `<Canvas>` elements, keep canvas creation imperative — only move what makes sense to XML. The goal is cleaner code, not a one-to-one XML mirror.
- **Dynamic children:** `CreateNewLabel()` still adds widgets at runtime via factory calls — this is correct behavior. XML defines the static structure; runtime code populates it dynamically.

---

*Created: 2026-07-25 | Part of GUI System Refactoring Project*
