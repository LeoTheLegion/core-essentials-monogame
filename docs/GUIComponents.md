# GUI Entity Components

CoreEssentials provides built-in entity components for rendering GUI widgets as part of the entity hierarchy — a Unity-like model where one **Canvas** component is the single source of truth for UI, and widget components bind into it.

- `CanvasComponent` — owns a `Canvas` (screen-space or world-space)
- `LabelComponent` — renders a text label at the entity's position
- `ButtonComponent` — renders a clickable text button at the entity's position

All live in the namespace `CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Components.BuiltIn`.

## Design: One Canvas, One Source of Truth

Following Unity's Canvas model, **exactly one canvas per UI subtree** is expected. You attach a single `CanvasComponent` to a root entity (e.g., a HUD root or an in-world panel entity), and any widget component added to that entity **or any of its descendants** resolves the nearest ancestor canvas and adds its widget into it.

```
HudRoot ── CanvasComponent            ← single source of truth
 ├── ScoreEntity ── LabelComponent    → label widget lives in HudRoot's canvas
 └── MenuEntity  ── ButtonComponent   → button widget lives in HudRoot's canvas
```

Widget components **never own a canvas themselves**. If no `CanvasComponent` exists anywhere up the parent chain, attaching a widget component throws an `InvalidOperationException`:

> No CanvasComponent found for the given entity or any of its ancestors. Add a CanvasComponent to this entity or one of its parents before attaching GUI widget components.

### Position Sync

Each frame, a widget's position is synced so it sits at its owning entity's position **relative to the canvas entity**:

```
widget.Position = Owner.Position - CanvasEntity.Position
```

Move the entity (including children moving via `LocalPosition`) and its widget follows automatically. For screen-space canvases this means "screen coordinates relative to the canvas anchor"; for world-space canvases it means "world offset from the canvas entity".

## CanvasComponent

Owns a single `Canvas` and drives its lifecycle end to end:

- **Update** — syncs the canvas position to `Owner.Position` and pumps the canvas (required for world-space camera transforms).
- **OnDetach** — calls `Canvas.CleanUp()`, releasing all child widgets.

Because `Entity` destroys its children recursively, a single root `CanvasComponent` covers an entire HUD subtree.

### Constructor

```csharp
public CanvasComponent(bool isScreenSpace = true)
```

| Parameter | Description |
|---|---|
| `isScreenSpace` | `true` (default): canvas renders in screen space, positioned in absolute screen coordinates. `false`: world space — the canvas follows the owning entity's position relative to the main camera. |

### Properties

| Property | Type | Description |
|---|---|---|
| `Canvas` | `Canvas` | The canvas owned by this component (the single source of truth for the subtree). |
| `IsScreenSpace` | `bool` | Whether the canvas renders in screen space. |

### Methods

```csharp
public void AddWidget(IWidget widget)   // add any widget directly to this canvas
public void RemoveWidget(IWidget widget) // remove a widget from this canvas
```

These convenience methods let games add arbitrary widgets (panels, grids, custom controls) without needing a dedicated component.

### Static helpers

```csharp
public static CanvasComponent? FindCanvas(Entity? entity);  // nearest canvas, or null
public static CanvasComponent RequireCanvas(Entity? entity); // nearest canvas, or throws
```

Both walk the entity's parent chain (the entity itself first, then each ancestor). `RequireCanvas` is what widget components use internally.

### Example

```csharp
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Components.BuiltIn;

// HUD root — screen-space canvas anchored at a screen position
var hudRoot = new HudRootEntity();
hudRoot.AddComponent<CanvasComponent>(); // isScreenSpace: true
hudRoot.LocalPosition = new Vector2(16f, 16f);
scene.Add(hudRoot);

// Score label as a child entity
var score = new ScoreEntity();
score.AddComponent<LabelComponent>("Score: 0");
hudRoot.AddChild(score);
```

## LabelComponent

Renders a text label (`ILabel`) into the nearest ancestor canvas.

### Constructors

```csharp
public LabelComponent()          // template-friendly; set Text before attaching
public LabelComponent(string text)
```

### Properties (all can be set before attaching; applied on attach)

| Property | Type | Default | Description |
|---|---|---|---|
| `Text` | `string` | `""` | Display text. |
| `TextColor` | `Color` | `White` | Color of the label's text. |
| `Scale` | `Vector2` | `(1, 1)` | Scale of the label widget. |
| `Visible` | `bool` | `true` | Whether the label is visible. |
| `Opacity` | `float` | `1.0f` | Opacity (0 = fully transparent, 1 = opaque). |

### Example

```csharp
var healthLabel = new HealthLabelEntity();
healthLabel.AddComponent<LabelComponent>("HP: 100");
// or configure after construction:
var comp = healthLabel.GetComponent<LabelComponent>();
comp.TextColor = Color.LimeGreen;
comp.Opacity = 0.8f;
```

## ButtonComponent

Renders a clickable text button (`IButton`) into the nearest ancestor canvas and exposes a plain `Clicked` event (no arguments).

### Constructors

```csharp
public ButtonComponent()          // template-friendly; set Text before attaching
public ButtonComponent(string text)
```

### Properties (all can be set before attaching; applied on attach)

| Property | Type | Default | Description |
|---|---|---|---|
| `Text` | `string` | `""` | Display text on the button. |
| `Scale` | `Vector2` | `(1, 1)` | Scale of the button widget. |
| `Visible` | `bool` | `true` | Whether the button is visible. |
| `Enabled` | `bool` | `true` | Whether the button receives input. |

### Events

```csharp
public event Action? Clicked;
```

Bridged from the widget's `Clicked(IButton)` event — subscribers simply get notified, no arguments. The component unsubscribes from the widget before removing it on detach, so detaching a button never leaves dangling handlers.

### Example

```csharp
var menuButton = new MenuButtonEntity();
menuButton.LocalPosition = new Vector2(0f, 64f); // relative to HUD root

var comp = menuButton.AddComponent<ButtonComponent>("Menu");
comp.Clicked += () => SceneManager.LoadScene(new MainMenuScene());

hudRoot.AddChild(menuButton);
```

## Lifecycle Summary

| Phase | What happens |
|---|---|
| `OnAttach` (widget components) | Resolve nearest canvas (`RequireCanvas`, throws if missing), create the widget via `WidgetFactory`, apply all properties, add the widget to the canvas. |
| `Update` | Sync widget position to `Owner.Position - CanvasEntity.Position`. The `CanvasComponent` also syncs and pumps its canvas. |
| `OnDetach` (widget components) | Unsubscribe events (button), remove the widget from the canvas, clear references. |
| `OnDetach` (`CanvasComponent`) | `Canvas.CleanUp()` — releases all child widgets. |

## Notes

- **Widget creation goes through `WidgetFactory`**, so the components stay decoupled from the Myra backend (see [GUI System](./GUISystem.md)).
- **World-space canvases** are useful for in-world UI (e.g., a floating panel attached to an NPC); screen-space is the default for HUDs and menus.
- **Font handling** is intentionally out of scope for v1 — widgets use the GUI engine's default font. Custom fonts can be applied later via the underlying `ILabel.Font` property if needed.
