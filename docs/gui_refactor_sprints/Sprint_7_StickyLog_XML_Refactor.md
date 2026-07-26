# Sprint 7 — StickyLog XML Layout Refactor 🧹

**Points:** 3  
**Status:** In Progress (T1 complete, T2–T4 pending)  
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

- [ ] **T2: Refactor StickyLog.LoadGUI() to use GuiSerializer (1 pt)** ⭐
  - File: `CoreEssentials/src/debugging/StickyLog.cs`
  - Replace imperative factory code with XML-based loading:
    ```csharp
    // Before — ~25 lines of factory calls and property assignments
    // After — ~3-4 lines:
    _grid = GuiSerializer.LoadGridFromXmlEmbedded("CoreEssentials.Content.StickyLogLayout");

    // Set position imperatively (runtime concern)
    _canvas.SetPosition(new Vector2(10, 10));
    ```
  - **Canvas handling:** If `GuiSerializer` supports `<Canvas>` elements in T3 of Sprint 6, use it. Otherwise, keep `_canvas = CanvasFactory.CreateScreenSpace()` and only move the grid to XML.
  - **Brush handling:** If color-to-brush mapping isn't supported in XML yet, set background imperatively after loading:
    ```csharp
    _grid.Background = Color.Black.WithAlpha(100).AsBrush();
    ```

- [ ] **T3: Update tests (0.5 pt)**
  - File: `CoreEssentials.Tests/Debugging/StickyLogTests.cs` (verify it exists)
  - Ensure tests pass with XML-based layout loading
  - If tests mock the UI setup, update mocks to work with the new pattern
  - Add a test verifying the grid loads from XML with correct dimensions and spacing

- [ ] **T4: Update documentation (0.5 pt)**
  - `docs/GUISystem.md`: Add "Real-world example" section showing StickyLog as an XML layout use case
  - Note in Sprint 3 migration guide that imperative code can be further simplified with XML

---

## Acceptance Criteria

- [ ] `StickyLogLayout.xml` exists in `CoreEssentials/Content/` with correct grid definition
- [ ] `StickyLog.LoadGUI()` uses `GuiSerializer` instead of `WidgetFactory.CreateGrid()` + property assignments
- [ ] StickyLog visual output is identical to pre-refactor (same size, position, colors)
- [ ] All existing tests pass (`dotnet test CoreEssentials.Tests`)
- [ ] Playground still runs without errors

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
