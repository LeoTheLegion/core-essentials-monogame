# GUI Entity Components

CoreEssentials provides built-in entity components for rendering GUI widgets as part of the entity hierarchy — a Unity-like model where one **Canvas** component is the single source of truth for UI, and widget components bind into it.

- `CanvasComponent` — owns a `Canvas` (screen-space or world-space)
- `LabelComponent` — renders a text label at the entity's position
- `ButtonComponent` — renders a clickable text button at the entity's position
- `AnchorComponent` — pins an entity to an anchor point of its canvas (Unity RectTransform style)

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
| `IsScreenSpace` | `bool` | Whether the canvas renders in screen space. **Settable at runtime** — flipping it switches the space on the next update, and XML scene files can declare world-space canvases via a `<Property Name="IsScreenSpace" Value="false" />` element. |
| `Width` | `float` | Canvas width. For world-space canvases this defines the anchored rectangle that `AnchorComponent` resolves against (in world units). |
| `Height` | `float` | Canvas height (same as `Width`). |

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

### World-Space Canvases

Setting `IsScreenSpace` to `false` (in code or XML) turns the canvas into an **in-world panel**: it is positioned in world coordinates through the main camera, so it moves with its owning entity and can be attached to NPCs, vehicles, or any other moving object. Give the canvas a `Width`/`Height` to define its anchored rectangle:

```csharp
// In-world panel that follows an NPC
var panel = new Entity();
panel.Position = new Vector2(640, 360);
var canvas = panel.AddComponent<CanvasComponent>(isScreenSpace: false);
canvas.Width = 320f;
canvas.Height = 200f;
```

Child entities of the panel entity position themselves **relative to the panel** (the same convention as screen-space canvases), so they stay pinned inside it while it moves around the world. Combine with `AnchorComponent` for resize-stable layouts within the panel.

## AnchorComponent

Pins the owning entity to an anchor point of its canvas, mirroring Unity's RectTransform anchor + offset model. Each frame the entity's position is resolved as:

```
position = (Anchor * canvasRect) + Offset
```

where `canvasRect` is:
- **Screen-space canvas** — the GUI viewport (`GUIManager.Width/Height`), so HUD layouts survive window resizes.
- **World-space canvas** — the canvas's own `Width`/`Height` (falling back to the GUI viewport when unset), in world units relative to the canvas entity.

Entities without a canvas anywhere up the parent chain are left untouched, so plain gameplay entities can carry this component harmlessly. For parented entities the resolved position is written to `LocalPosition` (canvas-relative); for root entities it goes to `Position`.

### Constructors

```csharp
public AnchorComponent()                        // defaults: MiddleCenter, no offset
public AnchorComponent(AnchorPreset preset, Vector2 offset)
```

### Properties (all live pass-throughs; all XML-serializable)

| Property | Type | Default | Description |
|---|---|---|---|
| `Preset` | `AnchorPreset` | `MiddleCenter` | One of the nine Unity-style anchor presets. Setting it updates `Anchor`. |
| `Anchor` | `Vector2` | `(0.5, 0.5)` | Normalized anchor point: (0, 0) = top-left, (1, 1) = bottom-right. Assigning directly does not change `Preset` — the last property written wins. |
| `Offset` | `Vector2` | `(0, 0)` | Offset from the anchor point (screen pixels for screen-space canvases, world units for world-space). |
| `Active` | `bool` | `true` | Set to false to freeze the current position while keeping the anchor configuration. |

### AnchorPreset Values

`TopLeft`, `TopCenter`, `TopRight`, `MiddleLeft`, `MiddleCenter`, `MiddleRight`, `BottomLeft`, `BottomCenter`, `BottomRight`.

### Example — HUD with anchored elements

```csharp
var hudRoot = new Entity();
hudRoot.AddComponent<CanvasComponent>(); // screen-space, at origin
scene.Add(hudRoot);

var score = new Entity();
score.AddComponent<LabelComponent>("Score: 0");
score.AddComponent(new AnchorComponent(AnchorPreset.TopLeft, new Vector2(16, 16)));
hudRoot.AddChild(score);

var pauseButton = new Entity();
pauseButton.AddComponent<ButtonComponent>("Pause");
pauseButton.AddComponent(new AnchorComponent(AnchorPreset.TopRight, new Vector2(-16, 16)));
hudRoot.AddChild(pauseButton);
```

### Example — XML scene with anchored widgets

```xml
<EntityDefinition Type="Entity" Id="hud">
    <Components>
        <Component Type="CanvasComponent" />
    </Components>
    <Children>
        <EntityDefinition Type="Entity" Id="score">
            <Components>
                <Component Type="LabelComponent">
                    <Properties>
                        <Property Name="Text" Value="Score: 0" />
                    </Properties>
                </Component>
                <Component Type="AnchorComponent">
                    <Properties>
                        <Property Name="Preset" Value="TopLeft" />
                        <Property Name="Offset" Value="16,16" />
                    </Properties>
                </Component>
            </Components>
        </EntityDefinition>
    </Children>
</EntityDefinition>
```

> **Loading order note:** scene loading attaches components **pre-order** (parents before children), so a child's `AnchorComponent`/widget component always finds its ancestor `CanvasComponent` already attached.

## LabelComponent

Renders a text label (`ILabel`) into the nearest ancestor canvas.

### Constructors

```csharp
public LabelComponent()          // template-friendly; set Text before attaching
public LabelComponent(string text)
```

### Properties (live pass-throughs)
All properties can be set **before** attaching (applied on attach) **or after** attaching — setting one after attach immediately updates the rendered widget, so labels are suitable for dynamic HUD values (score, timer, health, cooldowns).

| Property | Type | Default | Description |
|---|---|---|---|
| `Text` | `string` | `""` | Display text. |
| `TextColor` | `Color` | `White` | Color of the label's text. |
| `Scale` | `Vector2` | `(1, 1)` | Scale of the label widget. |
| `Visible` | `bool` | `true` | Whether the label is visible. |
| `Opacity` | `float` | `1.0f` | Opacity (0 = fully transparent, 1 = opaque). |

### Example

```csharp
var scoreEntity = new ScoreEntity();
scoreEntity.AddComponent<LabelComponent>("Score: 0");

// Later — e.g. every time the score changes:
var comp = scoreEntity.GetComponent<LabelComponent>();
comp.Text = "Score: 42";      // updates the rendered widget immediately
comp.TextColor = Color.Gold;
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
- **World-space canvases** are useful for in-world UI (e.g., a floating panel attached to an NPC); screen-space is the default for HUDs and menus. World-space canvases require a main camera (`Camera.MainCamera`) to project their world position.
- **`AnchorComponent` drives position every frame**, so don't also drive the same entity's position from game code — the anchor wins on the next update.
- **Font handling** is intentionally out of scope for v1 — widgets use the GUI engine's default font. Custom fonts can be applied later via the underlying `ILabel.Font` property if needed.
