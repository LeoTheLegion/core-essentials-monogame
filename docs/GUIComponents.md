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

### Position Sync (container alignment)

Each frame, a widget is positioned **inside its canvas** by `HorizontalAlignment`/`VerticalAlignment`, with the entity's position relative to the canvas entity added as a margin:

```
widget.Position = (Owner.Position - CanvasEntity.Position) + AlignmentOffset(canvas)
```

`AlignmentOffset` places the widget's top-left corner for the configured alignment: Left/Top is `(0, 0)`; Center shifts by half the canvas minus half the rendered size; Right/Bottom shifts by the canvas size minus the rendered size (rendered = measured size × scale). A host entity with no position is therefore positioned by alignment alone — e.g. `Center`/`Center` centers the widget in the whole canvas.

Because this math assumes scaling about the widget's top-left corner, both components pin the widget's `TransformOrigin` to `(0, 0)` on attach (the GUI engine's default origin is the widget center). This keeps scaled widgets aligned exactly where the math places them instead of drifting off-center as scale grows.

Move the entity (including children moving via `LocalPosition`) and its widget follows automatically as a margin shift. For screen-space canvases this means "screen coordinates relative to the canvas anchor"; for world-space canvases it means "world offset from the canvas entity".

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
| `Width` | `float` | Canvas width. For **screen-space** canvases, auto-sizing (the default) reports the GUI viewport size — the canvas IS the screen. For **world-space** canvases it defines the anchored rectangle that `AnchorComponent` resolves against (in world units); set it explicitly to pin the panel size. |
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
| `HorizontalAlignment` | `HorizontalAlignment` | `Left` | How the label is positioned **inside its canvas**: `Left` (default) puts its left edge on the canvas's left edge, `Center` centers it in the canvas, `Right` puts its right edge on the canvas's right edge. The entity's position relative to the canvas entity acts as a margin from that reference point. Applied per frame during position sync; scale-aware. |
| `VerticalAlignment` | `VerticalAlignment` | `Top` | Same as above for the vertical axis (`Top`, `Center`, `Bottom`). |
| `FontAsset` | `string?` | `null` | TTF/OTF font file name (relative to the game's `Content` folder) used to render the label's text. When `null` or empty, the engine default font is used. See [Fonts](#fonts-label-and-button). |
| `FontSize` | `int` | `20` | Size in pixels used when loading `FontAsset`. Ignored while `FontAsset` is null/empty. |

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

### Positioning a label by alignment alone (container semantics)

Alignment positions the label **inside its canvas** — no anchor position needed. A host
entity with no position is placed purely by alignment:

```csharp
// Centered in the whole canvas, even when scaled:
var comp = entity.AddComponent<LabelComponent>("+10");
comp.HorizontalAlignment = HorizontalAlignment.Center;
comp.VerticalAlignment = VerticalAlignment.Center;

// Hugs the canvas's right edge with a 16px margin:
entity.LocalPosition = new Vector2(-16, 0);
```

The entity's position relative to the canvas entity is treated as a **margin** added on top
of the aligned reference point. The offset is computed per frame from the widget's current
(measured) size and scale, so it stays correct while the text changes or the scale animates.

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
| `HorizontalAlignment` | `HorizontalAlignment` | `Left` | How the button is positioned inside its canvas (container semantics, see `LabelComponent`). |
| `VerticalAlignment` | `VerticalAlignment` | `Top` | Same as above for the vertical axis. |
| `BackgroundTint` | `Color?` | `null` | Optional background box color. When `null` (default) the button renders with **no opaque background**, so a game-supplied back-plate shows through. When set, a solid box of that color is drawn behind the text in every state. Ignored while `BackgroundAsset` is set (the sprite wins). Live: setting it after attach updates the rendered widget immediately. See [Button background](#button-background). |
| `BackgroundAsset` | `string?` | `null` | Sprite asset name drawn as the button's background, stretched to fill the widget in every state. When set it takes **precedence** over `BackgroundTint` and is tinted by `BackgroundColor`. When null/empty (default) the button falls back to its `BackgroundTint` behavior. See [Button background](#button-background). |
| `BackgroundColor` | `Color` | `White` | Tint applied to `BackgroundAsset`. Only has an effect while a background sprite is assigned. |
| `FontAsset` | `string?` | `null` | TTF/OTF font file name (relative to the game's `Content` folder) used to render the button's text. When `null` or empty, the engine default font is used. See [Fonts](#fonts-label-and-button). |
| `FontSize` | `int` | `20` | Size in pixels used when loading `FontAsset`. Ignored while `FontAsset` is null/empty. |

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

## Fonts (Label and Button)

Both `LabelComponent` and `ButtonComponent` can render their text in a custom typeface by pointing at a **TrueType/OpenType font file**.

- Set `FontAsset` to the font file name, relative to the game's `Content` folder (e.g. `"Fonts/display.ttf"`).
- Set `FontSize` to the size in pixels (default `20`).
- Leave `FontAsset` null or empty to keep the engine default font.

```csharp
var title = new TitleEntity();
var label = title.AddComponent<LabelComponent>("My Game");
label.FontAsset = "Fonts/display.ttf";   // resolved relative to <game>/Content/
label.FontSize = 48;
```

### How it works

The GUI backend (Myra) renders text through **FontStashSharp**, not MonoGame's `SpriteFont`. CE therefore loads the raw `.ttf`/`.otf` bytes from `Content/{FontAsset}` and builds a Myra `SpriteFontBase` via `GuiFontLoader.Load(assetName, size)`, which caches results per (asset, size) pair. Because fonts are read as raw files (the same convention as other file-based assets), the font must be present under the game's `Content` folder at runtime — it is **not** run through the MonoGame content pipeline.

A missing or empty asset name surfaces as an exception from `GuiFontLoader.Load`; while `FontAsset` is null/empty no font is loaded and the default remains in effect.

## Button background

By default a button has **no opaque background box** — `BackgroundTint` is `null`, so only the text is drawn and whatever sits behind the button (a sprite, panel, or other back-plate) shows through. This matches the common case where art supplies the button's visual.

To draw a solid CE-managed background instead, set `BackgroundTint`:

```csharp
var comp = menuButton.AddComponent<ButtonComponent>("Menu");
comp.BackgroundTint = Color.CornflowerBlue;   // solid box behind the text in every state
```

The tint is applied to all of Myra's button states (normal, hover, pressed, disabled, focused) as a single solid brush. Setting `BackgroundTint` back to `null` restores the transparent default. The property is live — setting it after attach updates the rendered widget immediately.

### Themed background from a sprite asset

Instead of a flat solid box, a button can render a **sprite** as its background: point `BackgroundAsset` at a sprite asset name (the same `"Sprites/foo.xml"` form used by `SpriteComponent`). The texture is stretched to fill the widget's background area in every visual state, and tinted by `BackgroundColor` (default white).

```csharp
var comp = menuButton.AddComponent<ButtonComponent>("Menu");
comp.BackgroundAsset = "Sprites/button_plate.xml";  // a sprite asset under Content/
comp.BackgroundColor = Color.Tomato;                // tints the plate
```

**Precedence:** while `BackgroundAsset` is non-empty, the sprite wins and `BackgroundTint` is ignored. Clearing `BackgroundAsset` (back to null/empty) restores the solid/transparent `BackgroundTint` behavior. Both properties are live — changing either after attach re-applies the background immediately.

Under the hood the sprite is drawn through a small `TextureBrush` (a Myra `IBrush` over a `Texture2D`), because Myra ships no textured brush of its own; it is assigned to all five state brushes just like the solid tint case.

### Background and hit-testing

A button's ability to receive clicks is **independent** of whether it has a background. In Myra, input only advances to a widget when `!InputFallsThrough(localPos)`, and the base `Widget` implementation returns `false` unconditionally (buttons do not override it). The background brush is consulted only during rendering — never during hit-testing.

Concretely: a button with `BackgroundTint == null` **and** no `BackgroundAsset` still registers clicks over its full bounds, so the transparent default is safe to use for text-only buttons that sit on top of game-supplied art. No invisible/transparent brush needs to be added to make such buttons clickable.

## Widget Sizing: AutoWidth / AutoHeight

Every widget (`IWidget`) reports its size through `Width`/`Height`, but how that size is
determined depends on two flags, both defaulting to `true`:

- **`AutoWidth`** — when `true`, `Width` returns the size *measured from the widget's content*
  (e.g., the text width of a label). Setting `Width` while auto is on has **no effect**.
- **`AutoHeight`** — same for the vertical axis.

This means an auto-sized label always reports its real pixel size instead of `0`, so you can
use `Width`/`Height` for layout math (centering, hit areas, spacing) without any special-casing.

### Pinning an explicit size

To force a fixed size, turn the corresponding auto flag off first — toggling it off pins the
widget at its current measured size, so there is no visual jump:

```csharp
var button = WidgetFactory.CreateTextButton("Save");
button.AutoWidth = false;   // pins width to the measured text width
button.Width = 200f;        // now sets an explicit size
button.AutoHeight = false;
button.Height = 50f;
```

Turning auto back on restores content-measured sizing:

```csharp
button.AutoWidth = true;    // Width is measured from the text again
```

### XML

In XML layout files, a `Width`/`Height` attribute pins auto-sizing automatically — you do not
need (and cannot) express the flags in XML:

```xml
<Button Text="Save" Width="200" Height="50" />   <!-- fixed size -->
<Label Text="Score: 0" />                        <!-- auto-sized, reports measured Width/Height -->
```

### Canvas note

A **screen-space** canvas IS the screen, so while auto-sized (the default) its `Width`/`Height`
report the GUI viewport (`GUIManager.Width/Height`) instead of a content measurement — there is
no meaningful "content size" for the whole screen. A **world-space** canvas keeps the normal
widget semantics: auto means measured content size, so give it an explicit `Width`/`Height` to
define its anchored rectangle.

Setting `Canvas.Width`/`Canvas.Height` (or `CanvasComponent.Width`/`Height`) pins auto-sizing on
set, so an explicit size is always applied.

## Lifecycle Summary

| Phase | What happens |
|---|---|
| `OnAttach` (widget components) | Resolve nearest canvas (`RequireCanvas`, throws if missing), create the widget via `WidgetFactory`, apply all properties, add the widget to the canvas. |
| `Update` | Position the widget inside its canvas by alignment, with the entity's position relative to the canvas entity as a margin (see Position Sync). The `CanvasComponent` also syncs and pumps its canvas. |
| `OnDetach` (widget components) | Unsubscribe events (button), remove the widget from the canvas, clear references. |
| `OnDetach` (`CanvasComponent`) | `Canvas.CleanUp()` — releases all child widgets. |

## Notes

- **Widget creation goes through `WidgetFactory`**, so the components stay decoupled from the Myra backend (see [GUI System](./GUISystem.md)).
- **World-space canvases** are useful for in-world UI (e.g., a floating panel attached to an NPC); screen-space is the default for HUDs and menus. World-space canvases require a main camera (`Camera.MainCamera`) to project their world position.
- **`AnchorComponent` drives position every frame**, so don't also drive the same entity's position from game code — the anchor wins on the next update.
- **Fonts** are supplied as raw TTF/OTF files under the game's `Content` folder and loaded through FontStashSharp (see [Fonts](#fonts-label-and-button)); they are not MonoGame `SpriteFont`s and do not go through the content pipeline.
- **Button background** is transparent by default; set `BackgroundTint` to draw a solid box (see [Button background](#button-background)).
