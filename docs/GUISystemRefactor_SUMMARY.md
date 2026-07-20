# GUI System Refactoring - Summary

## Problem Statement 🚫

Currently, users **must import Myra types directly** to use the CoreEssentials GUI system. The current API leaks Myra internals:

```csharp
// CURRENT — User must reference Myra directly ❌
using Myra.Graphics2D.UI;       // ← Must import this!
using Myra.Graphics2D.Brushes;  // ← And this!

var panel = new Panel();         // ← Myra type exposed
panel.Background = new SolidBrush(Color.Black); // ← Myra brush
var button = Button.CreateTextButton("Click");   // ← Myra factory method
```

**This defeats the purpose of a wrapper library.** Users should never see `Myra` in their `using` statements.

---

## Current Architecture 📋

### Files
| File | Purpose | Myra Types Leaked |
|------|---------|-------------------|
| `GUIManager.cs` | Static manager for desktop/root panel | `Desktop`, `Panel`, `Widget`, `ContentControl`, `ComboView` |
| `Canvas.cs` | Container for widget groups (screen/world space) | `Panel` |
| `StickyLog.cs` | Debug log overlay | `Grid`, `Label`, `SolidBrush`, `Proportion` |

### Usage in Codebase
- **Tests** (`CoreEssentials.Tests/GUI/`): Directly instantiate Myra types (`Panel`, `Label`) and set `MyraEnvironment.Game`.
- **Playground** (`SoundButtonEntity.cs`, `VolumeButtonEntity.cs`): Use `Button.CreateTextButton()` from Myra.
- **MainGame.cs**: Sets `MyraEnvironment.Game = this` — user must replicate this in their game.

---

## Target Architecture 🏗️ (Engine-Swap Pattern)

This follows the same pattern as Physics: a pure interface layer with swapable engine implementations. Today it's Myra; tomorrow you can add your own custom GUI engine as just another `engines/` folder.

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
├── engines/                       ← Swapable GUI engine implementations (like Physics)
│   ├── myra/                      ← Myra engine (current, default) ⭐
│   │   ├── GuiManagerImpl.cs     ← Implements IGuiManager, wraps Myra Desktop
│   │   ├── CanvasImpl.cs         ← Implements ICanvas, wraps Myra Panel
│   │   ├── Widgets/              ← Myra-backed widget implementations
│   │   │   ├── WidgetBase.cs     ← Base class implementing IWidget
│   │   │   ├── ContainerWidget.cs← Wraps Myra Panel → implements IContainer + IPanel
│   │   │   ├── ButtonWidget.cs   ← Wraps Myra Button → implements IButton
│   │   │   │                       + static factory: CreateTextButton()
│   │   │   ├── LabelWidget.cs    ← Wraps Myra Label → implements ILabel
│   │   │   ├── GridWidget.cs     ← Wraps Myra Grid → implements IGrid
│   │   │   └── ComboBoxWidget.cs ← Wraps Myra ComboBox → implements IComboBox
│   │   └── Brushes/              ← Myra-backed brush implementations
│   │       ├── BrushBase.cs      ← Base class implementing IBrush
│   │       ├── SolidColorBrush.cs← Wraps Myra SolidBrush
│   │       └── TextureBrush.cs   ← Wraps Myra TextureBrush (for images)
│   │
│   └── custom/                    ← Future: your own GUI engine 🔮
│       ├── GuiManagerImpl.cs     ← Implements IGuiManager with custom renderer
│       ├── CanvasImpl.cs         ← Implements ICanvas
│       ├── Widgets/              ← Custom widget implementations
│       └── Brushes/              ← Custom brush implementations
│
├── factory/                       ← Engine-agnostic factories ⭐
│   ├── WidgetFactory.cs          ← Creates widgets via interfaces (returns IWidget)
│   │                               ← Uses IGuiManager's engine internally — user doesn't know which one
│   └── CanvasFactory.cs          ← Creates canvases returning ICanvas
│
├── StickyLog.cs                   ← User-facing API: uses IGrid/ILabel, NOT Myra types directly
└── Internal/                      ← Hidden from public API 🔒
    ├── EngineResolver.cs         ← Selects default engine (Myra) — swappable at runtime
    ├── ColorAdapter.cs           ← MonoGame Color → engine-specific brush conversion
    └── InputMapper.cs            ← Normalizes input events across engines
```

### Key Design Decisions

1. **Users interact ONLY through interfaces** — `IGuiManager`, `ICanvas`, `IWidget` derivatives. Zero engine types in user code.

2. **Engine is transparent** — `WidgetFactory.CreateButton()` returns an `IButton`. The factory uses whatever engine `EngineResolver` points to. User never knows or cares.

3. **Swap engines at startup** — `EngineResolver.SetEngine(new CustomGuiEngine())` swaps the backend without touching user code.

4. **Static factory methods on widgets** — Replace Myra's `Button.CreateTextButton()` with `WidgetFactory.CreateTextButton("Click")`.

5. **Color abstraction** — Map MonoGame `Color` → engine-specific brush internally. Users pass `Color`, never a Myra type.

6. **StickyLog refactored** — Uses `IGrid` and `ILabel` interfaces instead of direct Myra types.

---

## API Comparison: Before vs After

### Current (Leaking Myra) ❌
```csharp
using Myra.Graphics2D.UI;
using Myra.Graphics2D.Brushes;

var panel = new Panel { Width = 200, Height = 150 };
panel.Background = new SolidBrush(Color.Black);
var label = new Label { Text = "Score: 0" };
panel.Widgets.Add(label);
GUIManager.Init(game, 800, 600);
```

### After — Engine Abstraction ✅ (Works with ANY engine)
```csharp
using CoreEssentials.GUI;  // ← Only one import needed!

// Init is handled automatically via GameSystem registration
IGuiManager gui = GetGameSystem<IGuiManager>();

var panel = WidgetFactory.CreatePanel(width: 200, height: 150);
panel.Background = Color.Black.WithAlpha(128); // Extension helper — no Myra types!
ILabel label = WidgetFactory.CreateLabel("Score: 0");
panel.AddChild(label);

// Click handler — engine-agnostic event pattern
button.Click += (sender, args) => { /* ... */ };
```

### Swapping to Your Custom Engine 🔮 (Zero user code changes!)
```csharp
// In MainGame.LoadContent(), before any GUI is created:
EngineResolver.SetEngine(new MyCustomGuiEngine());  // ← That's it!

// Everything else — WidgetFactory, IButton, IPanel, etc. — works identically.
var button = WidgetFactory.CreateTextButton("Play");  // Now renders with your engine!
```

---

## Implementation Phases

### Phase 1: Define Interfaces (types/) 📝
- `IGuiManager` — Init, AddWidget, RemoveWidget, Draw, IsFocused queries, SetDesktop
- `ICanvas` — Position, SpaceType, AddWidget, RemoveWidget, Update, CleanUp
- `IWidget`, `IContainer`, `IPanel`, `IButton`, `ILabel`, `IGrid`, `IBrush`, `IInputReceiver`

**Rule**: Zero engine references in any interface file. No Myra, no custom — pure abstractions.

### Phase 2: Implement Myra Engine Wrappers (engines/myra/) 🛠️
- Move existing GUIManager/Canvas into `GuiManagerImpl.cs` and `CanvasImpl.cs` under `engines/myra/`
- Create widget wrappers in `engines/myra/Widgets/` — each wraps a Myra type, implements the interface
- Create brush wrappers in `engines/myra/Brushes/`
- Internal conversion: MonoGame `Color` → Myra `SolidColorBrush`
- `ButtonWidget` includes static factory method `CreateTextButton()`

### Phase 3: Factory Layer & Engine Resolver ⚙️
- `EngineResolver.SetEngine(IGuiManager impl)` — configures which engine to use (default: Myra)
- `WidgetFactory.CreatePanel()`, `WidgetFactory.CreateLabel()`, etc. — delegates to current engine
- `CanvasFactory.CreateScreenSpace()` and `CreateWorldSpace()` return `ICanvas`

### Phase 4: Refactor StickyLog & Existing Code 🔧
- `StickyLog` uses only `IGrid`/`ILabel` interfaces, never Myra types directly
- `MainGame.cs`: Remove direct `MyraEnvironment.Game = this`; instead engine resolver handles it
- All public APIs return interfaces, not concrete Myra types

### Phase 5: Update Tests & Playground 🔄
- **Tests**: Stop creating Myra types directly. Use `WidgetFactory` and interfaces.
- **Playground**: Replace `Button.CreateTextButton()` with `WidgetFactory.CreateTextButton()`.
- Remove all `using Myra.*` from test/playground files.

### Phase 6: Update Documentation 📚
- Rewrite `docs/GUISystem.md` to show only the abstraction API.
- Document how to swap engines (one-liner for custom engine).
- Add migration guide section for existing users.

---

## Adding a Custom GUI Engine in the Future 🔮

When you're ready to build your own GUI renderer, follow this pattern:

```
CoreEssentials/src/gui/engines/custom/
├── GuiManagerImpl.cs     ← Implement IGuiManager with your custom render loop
├── CanvasImpl.cs         ← Implement ICanvas with your positioning logic
├── Widgets/              ← Your widget implementations
│   ├── ContainerWidget.cs← Your panel implementation → implements IContainer
│   ├── ButtonWidget.cs   ← Your button → implements IButton, Draw() uses your sprite batch
│   └── LabelWidget.cs    ← Your label → implements ILabel, text rendered with your font system
└── Brushes/              ← Your brush implementations
```

**What you need to implement:**
1. `IGuiManager` — your render loop + widget tree management
2. `ICanvas` — your positioning/space logic
3. Each `IWidget` derivative your engine supports
4. Register it: `EngineResolver.SetEngine(new CustomGuiManagerImpl())`

**What users don't need to touch:**
- Their `using CoreEssentials.GUI;` stays the same
- `WidgetFactory.CreateTextButton("Play")` still works
- All interface contracts remain identical

This is exactly how Physics allows swapping Aether for another engine — just implement the interfaces.

---

## Benefits Summary ✅

1. **Zero Engine Dependencies in User Code** — No `using Myra.*` anywhere. Users write code once, engine can change forever after.
2. **Cleaner API** — Factory constructors, static helpers, and interface-based design reduce boilerplate.
3. **Easier Testing** — Interfaces can be mocked without needing a full Myra environment setup.
4. **Engine Independence** — Swap Myra for your custom engine by implementing interfaces + one `SetEngine()` call. User code never changes.
5. **Consistent with Physics Pattern** — Same `types/` → `engines/` abstraction layer as `CoreEssentials.Physics`. One mental model, two subsystems.
6. **Future-Proof Path to Custom Engine** — When you're ready to build your own GUI renderer, it's literally just a new folder under `engines/custom/`.

---

## Risks & Mitigations ⚠️

| Risk | Mitigation |
|------|-----------|
| Breaking existing users | Provide migration guide; major version bump (0.x → 1.0) |
| Loss of Myra-specific features (XML layout loading, complex widgets) | Expose optional `IEngineBackend` interface for advanced users who need raw engine access — opt-in only |
| Performance overhead from indirection | Minimal — wrapper classes are thin delegates; consider `sealed` + `partial` where performance-critical |
| Myra dependency still in CoreEssentials package | Acceptable short-term. When custom engine is ready, make Myra an optional plugin package |

---

## Comparison: Physics vs GUI Refactoring Patterns

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
