# GUI Migration Guide: Old API → New API

This guide helps you migrate existing code that directly uses Myra types to the new CoreEssentials.GUI abstraction layer. After migration, your code will have **zero dependencies on Myra types**, making it engine-agnostic and future-proof.

---

## Quick Checklist

Before migrating, review this checklist:

- [ ] Replace all `using Myra.*` imports
- [ ] Replace direct Myra type instantiation (`new Panel()`, `new Label()`) with factory calls
- [ ] Update widget event handlers to use interface signatures (e.g., `IButton.Clicked`)
- [ ] Replace canvas construction with `CanvasFactory.CreateScreenSpace()` / `CreateWorldSpace()`
- [ ] Verify no Myra-specific features (XML layouts, `MyraEnvironment.Game`) remain in your code

---

## Before/After Examples

### Creating a Button

```csharp
// OLD (leaked Myra):
using Myra.Graphics2D.UI;
var button = new TextButton { Text = "Click" };
button.Click += (s, a) => { /* ... */ };

// NEW (clean abstraction):
using CoreEssentials.GUI.Factory;
IButton button = WidgetFactory.CreateTextButton("Click");
button.Clicked += (btn) => { /* ... */ };
```

### Creating a Panel with Background

```csharp
// OLD (leaked Myra):
using Myra.Graphics2D.UI;
using Myra.Graphics2D.Brushes;
var panel = new Panel();
panel.Background = new SolidBrush(Color.Black);
panel.Width = 200f;
panel.Height = 150f;

// NEW (clean abstraction):
using CoreEssentials.GUI.Factory;
using CoreEssentials.GUI.Types;
IPanel panel = WidgetFactory.CreatePanel();
panel.Background = new SolidColorBrush(Color.Black);
panel.Width = 200f;
panel.Height = 150f;
```

### Creating a Label

```csharp
// OLD (leaked Myra):
using Myra.Graphics2D.UI;
var label = new Label { Text = "Score: 0" };

// NEW (clean abstraction):
using CoreEssentials.GUI.Factory;
ILabel label = WidgetFactory.CreateLabel("Score: 0");
label.TextColor = Color.White; // Extra: color is now exposed on the interface
```

### Adding Widgets to a Container

```csharp
// OLD (leaked Myra):
panel.Widgets.Add(label);
panel.Widgets.Add(button);

// NEW (clean abstraction):
panel.AddChild(label);
panel.AddChild(button);
```

### Removing Widgets from a Container

```csharp
// OLD (leaked Myra):
panel.Widgets.Remove(label);

// NEW (clean abstraction):
panel.RemoveChild(label);
```

### Using a Canvas

```csharp
// OLD:
using Myra.Graphics2D.UI;
var canvas = new Canvas(true);  // screen space
canvas.AddWidget(new Label { Text = "Score" });

// NEW:
using CoreEssentials.GUI;
using CoreEssentials.GUI.Factory;
ICanvas canvas = CanvasFactory.CreateScreenSpace();
canvas.AddChild(WidgetFactory.CreateLabel("Score"));
```

### World-Space Canvas (Floating Labels)

```csharp
// OLD:
var worldCanvas = new Canvas(false); // world space

// NEW:
ICanvas worldCanvas = CanvasFactory.CreateWorldSpace();
worldCanvas.SetPosition(new Vector2(500f, 300f));
worldCanvas.AddChild(WidgetFactory.CreateLabel("Boss HP"));
```

### Initializing the GUI System

```csharp
// OLD (leaked Myra):
using Myra;
MyraEnvironment.Game = gameInstance;

// NEW (clean abstraction):
using CoreEssentials.GUI;
GUIManager.Init(gameInstance, 1280, 720); // width, height
```

### Drawing the GUI

```csharp
// OLD:
guiManager.Draw(spriteBatch);

// NEW (static method):
GUIManager.Draw(gameTime);
```

### Creating a Grid Layout

```csharp
// OLD (leaked Myra):
using Myra.Graphics2D.UI;
var grid = new Grid();
grid.RowProportions.Add(Proportion.Relative(1));
grid.ColumnProportions.Add(Proportion.Relative(2));
grid.Children.Add(label);
Grid.SetRow(label, 0);
Grid.SetColumn(label, 1);

// NEW (clean abstraction):
using CoreEssentials.GUI.Factory;
IGrid grid = WidgetFactory.CreateGrid();
grid.RowProportions.Add(1f);
grid.ColumnProportions.Add(2f);
grid.AddChild(label);
grid.SetRow(label, 0);
grid.SetColumn(label, 1);
```

### Creating a Vertical Stack Panel (Layout Container)

> **Note:** Stack panels are not yet exposed through `WidgetFactory`. If you need them, use the canvas's container hierarchy or create a panel and manually position children. This is planned for a future enhancement.

---

## Common Patterns — Migration Reference Table

| Action | Old Code | New Code |
|--------|----------|----------|
| Create button | `new TextButton()` | `WidgetFactory.CreateTextButton(text)` |
| Create label | `new Label()` | `WidgetFactory.CreateLabel(text)` |
| Create panel | `new Panel()` | `WidgetFactory.CreatePanel()` |
| Create grid | `new Grid()` | `WidgetFactory.CreateGrid()` |
| Add child | `panel.Widgets.Add(w)` | `panel.AddChild(w)` |
| Remove child | `panel.Widgets.Remove(w)` | `panel.RemoveChild(w)` |
| Clear children | N/A | `container.ClearChildren()` |
| Button click event | `button.Click += (s, a) =>` | `button.Clicked += (b) =>` |
| Screen canvas | `new Canvas(true)` | `CanvasFactory.CreateScreenSpace()` |
| World canvas | `new Canvas(false)` | `CanvasFactory.CreateWorldSpace()` |
| Initialize GUI | `MyraEnvironment.Game = game` | `GUIManager.Init(game, w, h)` |
| Set background | `panel.Background = new SolidBrush(color)` | `panel.Background = new SolidColorBrush(color)` |

---

## Breaking Changes Summary

### Event Signature Change — Button Clicks

The most common breaking change is the button click event signature:

```csharp
// OLD:
button.Click += (sender, args) => { ... };

// NEW:
button.Clicked += (button) => { ... };
```

The new pattern uses a strongly-typed `Action<IButton>` delegate instead of the traditional `(object sender, EventArgs e)` pattern. This provides better type safety and eliminates casting.

### Widget Hierarchy Methods

Container widget methods have been renamed for clarity:

| Old Method | New Method | Notes |
|------------|-----------|-------|
| `panel.Widgets.Add(widget)` | `panel.AddChild(widget)` | More explicit intent |
| `panel.Widgets.Remove(widget)` | `panel.RemoveChild(widget)` | Symmetric with AddChild |
| *(none)* | `container.ClearChildren()` | New: remove all children at once |

### Color → Brush Conversion

Solid colors now use the abstraction-friendly `SolidColorBrush` instead of Myra's internal `SolidBrush`:

```csharp
// OLD:
panel.Background = new SolidBrush(Color.Red);

// NEW:
panel.Background = new SolidColorBrush(Color.Red);
```

---

## What's NOT Migrated

The following Myra features are **intentionally excluded** from the abstraction layer because they are engine-specific:

- **XML UI Layouts** — `Project.LoadFromXml()` and XML-based widget definitions
- **MyraEnvironment** — Direct access to Myra's global runtime state
- **Raw Myra types** — `Desktop`, `Window`, `TabControl`, `FileDialog`, etc. (these remain accessible if you reference Myra directly, but are not part of the public API)

If you need any excluded features, consider opening an issue or accessing the raw backend via a future `IEngineBackend` interface.

---

## Need Help?

If your code uses patterns not covered in this guide, check:
- [`docs/GUISystem.md`](GUISystem.md) — Full API reference with new-style examples
- `CoreEssentials.Playground/` — Working examples using the new abstraction layer
- `CoreEssentials.Tests/GUI/` — Test files demonstrating interface usage
