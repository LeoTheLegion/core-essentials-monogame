# Sprint 5 — Documentation Updates 📚

**Points:** 2  
**Status:** Not Started (depends on Sprints 0–4)  
**Sprint Goal:** Rewrite all GUI documentation to reflect the new interface-based API, create a migration guide for existing users, and update project-level docs.

---

## Tasks

- [ ] **T1: Rewrite `docs/GUISystem.md` (1 pt)**
  - Replace all Myra-specific examples with CoreEssentials.GUI interfaces
  - Show only the abstraction API — no `using Myra.*` in any code snippet
  - Structure:
    ```markdown
    # GUI System

    ## Key Components
    ### WidgetFactory ⭐
    Create UI elements using factory methods that return interfaces:
    (examples with IButton, ILabel, IPanel via factories)

    ### Canvas 🖼️
    Manage groups of UI components in screen or world space:
    (ICanvas creation examples — ScreenSpace vs WorldSpace)

    ## API Reference
    - `IGuiManager` — lifecycle, widget management, rendering
    - `ICanvas` — positioning, container for widgets
    - `IWidget`, `IButton`, `ILabel`, `IPanel`, `IGrid` — UI element interfaces
    - `WidgetFactory` — static factory methods returning interface instances

    ## Advanced: Swapping GUI Engines 🔮
    (document EngineResolver.SetEngine() for future custom engines)
    ```
  - Remove or heavily reduce the "XML Layout" section (Myra-specific feature, not exposed through abstraction)
  - Keep UI component list but describe via CoreEssentials interfaces

- [ ] **T2: Create Migration Guide (0.5 pt)**
  - New file: `docs/GUI_Migration_Guide.md`
  - Show before/after code examples for common patterns:
    ```markdown
    ## Migration Guide: Old API → New API

    ### Creating a Button
    // OLD (leaked Myra):
    using Myra.Graphics2D.UI;
    var button = Button.CreateTextButton("Click");
    
    // NEW (clean abstraction):
    using CoreEssentials.GUI;
    IButton button = WidgetFactory.CreateTextButton("Click");

    ### Creating a Panel with Background
    // OLD:
    var panel = new Panel();
    panel.Background = new SolidBrush(Color.Black);
    
    // NEW:
    var panel = WidgetFactory.CreatePanel();
    panel.Background = new SolidColorBrush(Color.Black);

    ### Using a Canvas
    // OLD:
    var canvas = new Canvas(true);  // screen space
    canvas.AddWidget(new Label { Text = "Score" });
    
    // NEW:
    ICanvas canvas = CanvasFactory.CreateScreenSpace();
    canvas.AddChild(WidgetFactory.CreateLabel("Score"));
    ```
  - Include a checklist of changes users need to make in their codebase

- [ ] **T3: Update project-level documentation (0.5 pt)**
  - `README.md`: Update GUI System description — no mention of Myra in feature list
  - `CONTRIBUTING.md`: Add note about GUI abstraction layer pattern if relevant
  - Update any inline XML documentation comments to reflect new interface types

---

## Acceptance Criteria

- [ ] `docs/GUISystem.md` rewritten with zero Myra-specific code examples — only CoreEssentials.GUI interfaces and factory methods shown
- [ ] `docs/GUI_Migration_Guide.md` created with clear before/after comparisons for all common patterns
- [ ] `README.md` updated to reflect new GUI architecture (optional: still credit Myra as the default engine in a "Powered by" section)
- [ ] All code examples in documentation compile (or at least are syntactically valid C#)

---

## Deliverables

| File | Change | Notes |
|------|--------|-------|
| `docs/GUISystem.md` | Rewritten for abstraction API | Zero Myra code snippets |
| `docs/GUI_Migration_Guide.md` | New — before/after examples | Helps existing users migrate |
| `README.md` | Updated GUI description | Credit Myra as default engine (optional) |

---

## Notes & Risks

- **Myra credit:** Consider whether to still mention Myra in public docs. Options:
  - **Hidden:** No mention — users think CoreEssentials has a built-in renderer. Good for abstraction purity.
  - **Transparent:** Mention "Powered by Myra (default engine)" with link to GitHub. Honest but less clean.
  - **Hybrid:** Document only the abstraction API; add a small "Under the Hood" section mentioning Myra as the default implementation.
- **XML layout feature:** Myra supports loading UI from XML (`Project.LoadFromXml()`). This is NOT exposed through our abstraction layer by design — it's too engine-specific. If users need this, they can access the raw Myra backend via an optional `IEngineBackend` interface (future enhancement).

---

*Created: 2026-07-20 | Part of GUI System Refactoring Project*
