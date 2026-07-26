# Sprint 6 — XML Layout Support 📄

**Points:** 6  
**Status:** Not Started (depends on Sprints 0–5)  
**Sprint Goal:** Add a `GuiSerializer` class that parses UI layouts from XML strings and returns interface types via the existing factory, keeping our abstraction layer intact.

---

## Background & Design Decisions

Users want data-driven UI layout — define widgets in XML instead of code. Our Sprint 5 abstraction already keeps Myra types hidden behind interfaces (`IWidget`, `ILabel`, `IPanel`, etc.) and a factory (`WidgetFactory`). We extend that same pattern: **XML → interface, not XML → Myra**.

**Single class approach:** One static `GuiSerializer` in `CoreEssentials/src/gui/GuiSerializer.cs`. Each widget type gets its own static method (e.g., `LoadLabelFromXml()`, `LoadPanelFromXml()`). Container types recursively parse their children. This keeps the API clean — one entry point, no raw Myra leaks.

**Factory integration:** Under the hood, each loader calls `WidgetFactory.Create*()` and maps XML attributes to interface properties (e.g., `<Label Text="Hi"/>` → `label.Text = "Hi"`). Asset-loaded properties (`Font`, `Brush`) use a provided `IContentManager`.

**Supported widget types:**
| Interface | XML Element | Key Properties |
|-----------|-------------|----------------|
| `IWidget` | *(base)* | Width, Height, Visible, Enabled, Position, Margin, Opacity |
| `ILabel` | `<Label>` | Text, Font, TextColor |
| `IButton` | `<Button>` | Text, Clicked (event registration) |
| `IPanel` | `<Panel>` | Background (Brush), BorderThickness |
| `IContainer` | `<StackPanel>`, `<Grid>`, etc. | Children elements parsed recursively |
| `ICanvas` | `<Canvas>` | IsScreenSpace, AddWidget/RemoveWidget |

**XML attribute conventions:**
- Property names map 1:1 to interface property names (case-insensitive)
- Child widgets are nested as XML elements (containers parse children automatically)
- Optional `Id` attribute for later lookup via factory or canvas
- Asset paths (`Font`, `Brush`) resolve through the provided `IContentManager`

**Example usage:**
```csharp
// --- String-based (inline XML) ---
var panel = GuiSerializer.LoadPanelFromXml(@"
    <Panel Width=""400"" Height=""300"">
        <Label Id=""scoreLabel"" Text=""Score: 0"" VerticalAlignment=""Top""/>
        <Button Id=""myButton"" Text=""Click Me!"" Clicked=""OnButtonClick""/>
    </Panel>", contentManager);

// --- Asset-based (loads XMLAsset first, then parses) ---
// Option A: Direct instantiation — reads from disk via File.ReadAllText()
var asset = new XMLAsset("layout/main.xml");
asset.Load(contentManager); // contentManager arg is only for null-checking; file is read directly
var panel2 = GuiSerializer.LoadPanelFromXml(asset, contentManager);

// Option B: Via AssetManager — preferred for reference counting & caching
var asset = (XMLAsset)AssetManager.LoadAsset<XMLAsset>("layout_main.xml"); // key = name + type suffix
var panel3 = GuiSerializer.LoadPanelFromXml(asset, contentManager);

// Register a click handler by ID lookup
var button = panel.Children.OfType<IButton>().FirstOrDefault(b => b.Id == "myButton");
button.Clicked += OnButtonClick;

canvas.AddWidget(panel);
```

---

## Tasks

- [x] **T1: Simple widget loaders (non-container types) + tests (2 pt)** ⭐
  - New file: `CoreEssentials/src/gui/GuiSerializer.cs`
  - Implemented string-based and asset-based overloads for leaf widgets.
  - Introduced `IWidgetFactory` and `DefaultWidgetFactory` to support mocking in tests.
  - Map XML attributes → interface properties.
  - Handle optional `Id` attribute.
  - **Tests included:** Unit tests in `CoreEssentials.Tests/GUI/GuiSerializerTests.cs` using `FakeWidgetFactory` to avoid `GraphicsDevice` requirements.
  - **Infrastructure:** Added `FakePanel` and `FakeGrid` to test suite to support wider GUI testing without graphics dependencies.

- [x] **T2: Container widget loaders + recursion (1 pt)** ⭐ — *completed*
  - Added `LoadPanelFromXml` and `LoadGridFromXml` to `GuiSerializer.cs`.
  - Implemented recursive logic via `LoadChildren` to handle nested UI hierarchies.
  - Mapped container-specific properties (`BorderThickness` for panels, `RowSpacing`/`ColumnSpacing` for grids).
  - **Tests included:** Unit tests verifying complex nested structures and container properties in `GuiSerializerTests.cs`.

- [ ] **T3: Convenience overloads + integration tests (1 pt)** ⭐
  - Add generic/flexible methods to `GuiSerializer.cs`:
    ```csharp
    public static IWidget LoadFromXml(string xmlData, IContentManager? contentManager = null);
    public static IWidget LoadFromXml(XMLAsset asset, IContentManager? contentManager = null);
    ```
    These detect the root element type and dispatch to the appropriate typed loader.
  - **Integration tests** in `CoreEssentials.Tests/GUI/GuiSerializerIntegrationTests.cs`:
    - Deeply nested layout (`<Canvas><Panel><Grid>...</Grid></Panel></Canvas>`)
    - Verify full hierarchy via `.Children` / `.Widgets`
    - ID-based lookup across the tree
    - Mocked `IContentManager` path for font/brush resolution
  - **Tests included with implementation**

- [ ] **T4: Update Playground with XML example scene (0.5 pt)** ⭐
  - New file in `CoreEssentials.Playground/`: `XmlLayoutScene.cs`
  - Demo both string-based and asset-based usage:
    ```csharp
    // --- String-based (inline XML) ---
    var panel = GuiSerializer.LoadPanelFromXml(@"
        <Panel Width=""400"" Height=""300"">
            <Label Text=""Hello from XML!"" VerticalAlignment=""Top""/>
            <Button Id=""myButton"" Text=""Click Me!"" VerticalAlignment=""Bottom""/>
        </Panel>");

    // --- Asset-based (loads from file) ---
    var asset = new XMLAsset("layout/main.xml");
    asset.Load(contentManager);
    var panel2 = GuiSerializer.LoadPanelFromXml(asset, contentManager);

    // Wire up click handler by ID lookup
    var button = panel.Children.OfType<IButton>().FirstOrDefault(b => b.Id == "myButton");
    button.Clicked += _ => { /* ... */ };

    canvas.AddWidget(panel);
    ```
  - Show nested container example (Grid with rows/columns containing buttons)

- [ ] **T5: Update documentation (0.5 pt)**
  - `docs/GUISystem.md`: Add new section "XML Layouts" 
    - Explain the `GuiSerializer` API clearly with code examples
    - Document XML conventions (element names, attribute mapping, child recursion)
    - Emphasize that **no Myra types are exposed** — everything returns interfaces
  - `docs/GUI_Migration_Guide.md`: Note XML layout as a new feature added post-abstraction

---

## Acceptance Criteria

- [ ] `GuiSerializer` class exists in `CoreEssentials/src/gui/GuiSerializer.cs` with all methods implemented
- [ ] All methods return **interface types** (`IWidget`, `ILabel`, etc.) — no raw Myra leaks
- [ ] Container recursion works: nested XML elements produce correct parent-child widget trees
- [ ] All unit tests pass (`dotnet test CoreEssentials.Tests/GUI/GuiSerializerTests.cs`)
- [ ] Integration tests verify full hierarchy loading and ID lookups
- [ ] Playground XML example compiles and runs without errors
- [ ] `docs/GUISystem.md` has XmlLayout section with clear examples

---

## Deliverables

| File | Change | Points |
|------|--------|--------|
| `CoreEssentials/src/gui/GuiSerializer.cs` | New — XML-to-interface serializer (split across T1–T3) | 4 |
| `CoreEssentials.Tests/GUI/GuiSerializerTests.cs` | New — unit tests (included with T1, T2, T3) | 0 |
| `CoreEssentials.Tests/GUI/GuiSerializerIntegrationTests.cs` | New — integration tests (T3) | 0 |
| `CoreEssentials.Playground/XmlLayoutScene.cs` | New — demo scene | 0.5 |
| `docs/GUISystem.md` | Added "XML Layouts" section | 0.25 |
| `docs/GUI_Migration_Guide.md` | Updated with XML feature note | 0.25 |

---

## Notes & Risks

- **Property mapping strategy:** Hand-written switch/mapping is preferred over reflection for clarity and performance. Each loader method parses attributes into the corresponding interface properties.
- **Asset-loaded types:** `Font` (on labels) and `Brush`/`Background` (on panels/grids) need resolution through `IContentManager`. If not provided, these properties will be null — acceptable for simple layouts but should be documented.
- **Event registration:** XML can't directly wire up events like `Clicked`. The recommended pattern is ID-based lookup after parsing: find the widget by iterating children, then attach handlers in code.
- **Extensibility:** Adding a new widget type later means adding one method to `GuiSerializer` and one test — straightforward and consistent with existing factory patterns.

## XMLAsset Behavior (important)

Unlike other asset types (`Texture2DAsset`, `FontAsset`) which load through MonoGame's content pipeline, `XMLAsset.Load()` reads **directly from disk** via `File.ReadAllText()`. The `IContentManager` parameter is only used for null-checking — it does NOT participate in loading.

- File path: `{exePath}/Content/{assetName}` (e.g., `bin/Debug/net8.0/Content/layout/main.xml`)
- Content is stored as a raw string in `XMLAsset.XMLContent` property
- **Two ways to use:**
  - **Direct instantiation** (`new XMLAsset("name")`): Simple but no caching — each call creates a new instance with its own copy of the file content
  - **Via AssetManager** (`AssetManager.LoadAsset<XMLAsset>()`): Preferred for reference counting & deduplication. Key format is `{assetName}_{TypeName}` (e.g., `"layout_main.xml_XMLAsset"`)

---

*Created: 2026-07-25 | Part of GUI System Refactoring Project*