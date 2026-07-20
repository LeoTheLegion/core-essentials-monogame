# GUI System Refactoring — Scrum Sprints 🚀

This folder contains sprint plans for the GUI system refactoring project using an agile/Scrum approach. Each file represents one sprint with tasks estimated in story points (1, 2, or 5 points).

## Why This Refactoring? ⚠️

Currently, users **must import Myra types directly** to use the CoreEssentials GUI system:

```csharp
// CURRENT — User must reference Myra directly ❌
using Myra.Graphics2D.UI;       // ← Must import this!
using Myra.Graphics2D.Brushes;  // ← And this!

var panel = new Panel();         // ← Myra type exposed
panel.Background = new SolidBrush(Color.Black); // ← Myra brush
```

**This defeats the purpose of a wrapper library.** Users should never see `Myra` in their `using` statements. The refactoring introduces an interface-based abstraction layer (same pattern as Physics) so users interact only with CoreEssentials types, and your custom GUI engine can be plugged in later by implementing interfaces.

---

## Project Structure — Before & After

### Current Structure ❌
```
CoreEssentials/src/gui/
├── GUIManager.cs      ← Uses Myra directly (Desktop, Panel, Widget...)
├── Canvas.cs          ← Uses Myra directly (Panel)
└── (StickyLog in debugging/) ← Uses Myra directly (Grid, Label, SolidBrush)
```

### Target Structure ✅ (Engine-Swap Pattern)
```
CoreEssentials/src/gui/
├── types/                         ← Pure interfaces (NO engine refs) ⭐
│   ├── IGuiManager.cs            ← Desktop/window management, widget lifecycle
│   ├── ICanvas.cs                ← Canvas positioning & widget management
│   ├── IWidget                   ← Base abstraction for all UI elements
│   ├── IContainer.cs             ← Widget container (has child widgets)
│   ├── IButton.cs                ← Clickable button with event
│   ├── ILabel.cs                 ← Text display
│   ├── IGrid.cs                  ← Grid layout container
│   ├── IPanel.cs                 ← Panel/container widget
│   ├── IBrush.cs                 ← Background/styling abstraction
│   └── IInputReceiver.cs         ← Focus/click/touch input handling
│
├── engines/                       ← Swapable GUI engine implementations
│   └── myra/                      ← Myra engine (current, default) ⭐
│       ├── GuiManagerImpl.cs     ← Implements IGuiManager, wraps Myra Desktop
│       ├── CanvasImpl.cs         ← Implements ICanvas, wraps Myra Panel
│       ├── Widgets/              ← Myra-backed widget implementations
│       │   ├── WidgetBase.cs
│       │   ├── ContainerWidget.cs← Wraps Myra Panel → implements IContainer + IPanel
│       │   ├── ButtonWidget.cs   ← Wraps Myra Button → implements IButton
│       │   ├── LabelWidget.cs    ← Wraps Myra Label → implements ILabel
│       │   └── GridWidget.cs     ← Wraps Myra Grid → implements IGrid
│       └── Brushes/              ← Myra-backed brush implementations
│           ├── BrushBase.cs
│           └── SolidColorBrush.cs← Wraps Myra SolidBrush
│
├── factory/                       ← Engine-agnostic factories ⭐
│   ├── WidgetFactory.cs          ← Creates widgets via interfaces (returns IWidget)
│   └── CanvasFactory.cs          ← Creates canvases returning ICanvas
│
└── Internal/                      ← Hidden from public API 🔒
    ├── EngineResolver.cs         ← Selects default engine (Myra) — swappable at runtime
    ├── ColorAdapter.cs           ← MonoGame Color → Myra brush conversion
    └── InputMapper.cs            ← Normalizes input events across engines
```

**Key Design Decision:** Users interact ONLY through interfaces (`IGuiManager`, `ICanvas`, `IWidget` derivatives). Zero engine types in user code. When you're ready to build your own GUI renderer, it's literally just a new folder under `engines/custom/`.

---

## Sprint Structure

Each sprint is designed to be approximately **5 total points** worth of work, following standard Scrum principles:
- **1 point** = Small task (30 min - 2 hours)
- **2 points** = Medium task (2-4 hours)  
- **5 points** = Large task (1 full day or more)

---

## Sprint Roadmap

| Sprint | Name | Points | Status | Description |
|--------|------|--------|--------|-------------|
| 📋 [0](Sprint_0_Core_Types.md) | Core Interface Definitions | 5 | Not Started | Define all pure interfaces: `IGuiManager`, `ICanvas`, `IWidget`, `IButton`, `ILabel`, etc. — zero Myra references |
| 🔧 [1](Sprint_1_Myra_Wrappers.md) | Myra Engine Wrappers | 5 | Not Started | Implement widget wrappers in `engines/myra/` that wrap each Myra type and implement the corresponding interface |
| ⚙️ [2](Sprint_2_Engine_And_Factory.md) | GuiManagerImpl, CanvasImpl & Factory Layer | 5 | Not Started | Implement `GuiManagerImpl`, `CanvasImpl`, `EngineResolver`, and factory classes (`WidgetFactory`, `CanvasFactory`) |
| 🔄 [3](Sprint_3_Migrate_StickyLog.md) | Migrate StickyLog & Existing Code | 3 | Not Started | Refactor `StickyLog.cs` to use interfaces; update `MainGame.cs` GUI initialization; remove direct Myra usage from all existing code |
| ✅ [4](Sprint_4_Update_Tests.md) | Update Tests & Playground | 5 | Not Started | Rewrite GUI tests to use factories/interfaces; update Playground examples (`SoundButtonEntity`, `VolumeButtonEntity`); remove all `using Myra.*` from user files |
| 📚 [5](Sprint_5_Documentation.md) | Documentation Updates | 2 | Not Started | Rewrite `docs/GUISystem.md`; document engine-swapping; add migration guide (old API → new API); update README and CONTRIBUTING |

---

## Sprint Point Summary

- **Total Points:** 25 points across 6 sprints
- **Average Per Sprint:** ~4.2 points
- **Timeline Estimate:** 6 weeks (one sprint per week) or compressed to 3–4 weeks with parallel work on lower-risk sprints

---

## Key Workflow Phases

**Foundation (Sprint 0):** Define all pure interfaces — no Myra references allowed in `types/` folder. This is the contract that enables engine swapping.

**Core Implementation (Sprint 1–2):** Implement Myra wrappers, factory layer, and engine resolver. Users can start using the new API by end of Sprint 2.

**Migration & Validation (Sprint 3–4):** Refactor all existing code to use interfaces; update tests and playground; remove direct Myra imports from user-facing files.

**Documentation (Sprint 5):** Rewrite docs, add migration guide, document custom engine path for future development.

---

## Comparison: Physics vs GUI Patterns

Both follow the exact same architecture:

```
┌─────────────────────────────────┐     ┌─────────────────────────────────┐
│  types/ (Pure Interfaces)       │     │  types/ (Pure Interfaces)       │
│  IPhysicsBody, IFixture...      │     │  IWidget, IButton, ILabel...    │
│  NO engine references           │     │  NO engine references            │
└──────────────┬──────────────────┘     └──────────────┬──────────────────┘
               │                                      │
       ┌───────▼────────┐                      ┌─────▼──────────────┐
       │ engines/aether/│                      │engines/myra/ (or custom/) │
       │ PhysicsEngine  │                      │ GuiManagerImpl,      │
       │ PhysicsBody... │                      │ ButtonWidget, Label..│
       └───────┬────────┘                      └─────┬────────────────┘
               │                                      │
       ┌───────▼────────┐                      ┌─────▼──────────────┐
       │ factory/       │                      │ factory/           │
       │ PhysicsFactory │                      │ WidgetFactory      │
       └────────────────┘                      └────────────────────┘
```

The pattern is identical: **interfaces first, engine implementations second, factories third**. When you build your custom GUI engine, it slots in exactly where `engines/custom/` lives — no changes to types or factories needed.

---

## Adding a Custom GUI Engine in the Future 🔮

When ready to build your own GUI renderer:

```
CoreEssentials/src/gui/engines/custom/
├── GuiManagerImpl.cs     ← Implement IGuiManager with custom render loop
├── CanvasImpl.cs         ← Implement ICanvas with custom positioning logic
├── Widgets/              ← Custom widget implementations
│   ├── ContainerWidget.cs← Your panel → implements IContainer
│   ├── ButtonWidget.cs   ← Your button → implements IButton
│   └── LabelWidget.cs    ← Your label → implements ILabel
└── Brushes/              ← Brush implementations

// Register it:
EngineResolver.SetEngine(new CustomGuiManagerImpl());  // That's it!
```

Everything else — `WidgetFactory.CreateTextButton("Play")`, `ICanvas`, interfaces — works identically. User code never changes. This is exactly how Physics allows swapping Aether for another engine — just implement the interfaces.

---

*Created: 2026-07-20 | Part of GUI System Refactoring Project*
