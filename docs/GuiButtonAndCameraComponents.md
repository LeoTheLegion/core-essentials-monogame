# GUI Button & Camera Components

The playground's hand-written `SoundButtonEntity`, `VolumeButtonEntity`, and `CameraEntity` have been
replaced by a small set of reusable, declarative components. Each is now a plain `GameObjectEntity`
carrying the right set of `<Component>` declarations, so its behavior comes entirely from data with
**no entity subclass**.

This page documents those components: what they do, their configurable properties, and how to declare
them in scene/template XML.

## The Building Blocks

| Component | Purpose | Replaced |
|-----------|---------|----------|
| `SoundButtonComponent` | Plays a one-shot sound when the entity's `ButtonComponent` is clicked. | `SoundButtonEntity` (canvas + widget + click wiring). |
| `VolumeButtonComponent` | Sets the master audio volume when the entity's `ButtonComponent` is clicked. | `VolumeButtonEntity`. |
| `CameraFollowComponent` | Lerps the owning entity toward a target each frame; suspends manual panning while following. | The follow field + lerp in `CameraEntity.Update`, plus `SetFollowTarget` / `ToggleFollow`. |
| `CameraInputComponent` (updated) | WASD pan, Q/E zoom, R reset — now ignores panning while a sibling `CameraFollowComponent` is following. | The input layer in `CameraEntity.Update` / key handler. |
| `CameraFollowToggleComponent` (updated) | Toggles follow on the camera's `CameraFollowComponent` when a key (default F) is released, and refreshes an info label. | The toggle that used to cast its camera reference to `CameraEntity`. |

> **Thin subscribers.** A component must never *create* a sibling from `OnAttach` or `Update` — the
> entity iterates its live component collection during both passes, so adding one would throw
> "Collection was modified". The button components therefore only *subscribe*: the canvas and the
> widget are owned by the built-in `CanvasComponent` / `ButtonComponent` siblings, which must be
> declared **before** the behavior component in the XML (attach order follows declaration order).

## SoundButtonComponent

Subscribes to the `Clicked` event of the `ButtonComponent` on the same entity and plays a configured
one-shot sound through the `AudioManager`. If no `ButtonComponent` is present, it logs a warning and
does nothing.

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `SoundAsset` | `string` | `""` | Asset name of the one-shot sound to play (e.g. `Audio/footstep1_sound.xml`). |

```xml
<Component Type="CoreEssentials.Playground.Components.SoundButtonComponent">
    <Properties>
        <Property Name="SoundAsset" Value="Audio/footstep1_sound.xml" />
    </Properties>
</Component>
```

## VolumeButtonComponent

Subscribes to the `Clicked` event of the `ButtonComponent` on the same entity and sets the master
audio volume. If no `ButtonComponent` is present, it logs a warning and does nothing.

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `VolumeLevel` | `float` | `0` | Master volume (0.0–1.0) applied on click. |

```xml
<Component Type="CoreEssentials.Playground.Components.VolumeButtonComponent">
    <Properties>
        <Property Name="VolumeLevel" Value="0.5" />
    </Properties>
</Component>
```

## Button Templates

The playground ships two prefabs that wire the whole button stack — canvas, widget, behavior:

- `Templates/SoundButtonTemplate.xml` — `GameObjectEntity` + `CanvasComponent` + `ButtonComponent` + `SoundButtonComponent`, tags `UI` + `Sound`.
- `Templates/VolumeButtonTemplate.xml` — same shape with `VolumeButtonComponent`, tags `UI` + `Volume`.

Per-instance values (button text, sound asset, volume level) are component-targeted `<Overrides>`:

```xml
<Prefabs>
    <Prefab Name="SoundButtonPrefab" Asset="Templates/SoundButtonTemplate.xml" />
</Prefabs>
...
<EntityDefinition Source="SoundButtonPrefab" Id="footstep1Button">
    <Position X="100" Y="100" />
    <Overrides>
        <Component Type="CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Components.BuiltIn.ButtonComponent">
            <Property Name="Text" Value="Footstep 1" />
        </Component>
        <Component Type="CoreEssentials.Playground.Components.SoundButtonComponent">
            <Property Name="SoundAsset" Value="Audio/footstep1_sound.xml" />
        </Component>
    </Overrides>
</EntityDefinition>
```

## CameraFollowComponent

Makes the owning entity (a camera host) smoothly follow another entity. Each `Update` the owner's
position is lerped toward the target by `LerpFactor`; while following, a sibling
`CameraInputComponent` suspends manual panning so the two never fight over the position. If the
target is destroyed mid-follow, following stops instead of lerping to a stale position.

The target is set via `<Reference Name="FollowTarget" TargetId="..."/>` (resolved onto this
component's `FollowTarget` property after attach) or programmatically:

| Member | Kind | Description |
|--------|------|-------------|
| `FollowTarget` | `Entity?` (private setter) | The entity to follow, or null when not following. Settable via `<Reference/>`. |
| `FollowingTarget` | `bool` (get) | Whether a target is currently set. |
| `LerpFactor` | `float`, default `0.1` | Per-frame lerp factor toward the target. |
| `SetFollowTarget(target, startFollowingImmediately = true)` | method | Starts or stops following. |
| `ToggleFollow(targetToToggle)` | method | Toggles: stops if already following that exact target, otherwise starts. |

```xml
<EntityDefinition Type="CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.GameObjectEntity" Id="camera">
    <Position X="0" Y="0" />
    <Components>
        <Component Type="CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Components.BuiltIn.CameraComponent" />
        <Component Type="CoreEssentials.Playground.Components.CameraInputComponent">
            <Properties>
                <Property Name="MoveSpeed" Value="300" />
            </Properties>
        </Component>
        <Component Type="CoreEssentials.Playground.Components.CameraFollowComponent" />
    </Components>
</EntityDefinition>
```

## Camera Input (updated)

`CameraInputComponent` (see [Camera System](./CameraSystem.md)) gained one behavior: while a sibling
`CameraFollowComponent` is following, the pan keys are ignored (zoom and reset still work). This
mirrors the old `CameraEntity`, which gated WASD panning behind its follow state.

## Camera Follow Toggle (updated)

`CameraFollowToggleComponent` no longer casts its camera reference to a concrete entity type. It now
looks up a `CameraFollowComponent` on the referenced camera entity and calls `ToggleFollow` /
reads `FollowingTarget` through it — so any camera host carrying that component works, including the
plain `GameObjectEntity` declarations above. The `<Reference>` wiring is unchanged:

```xml
<EntityDefinition Type="CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.GameObjectEntity" Id="followToggle">
    <Position X="0" Y="0" />
    <Components>
        <Component Type="CoreEssentials.Playground.Components.CameraFollowToggleComponent">
            <Properties>
                <Property Name="ToggleKey" Value="F" />
            </Properties>
        </Component>
    </Components>
    <References>
        <Reference Name="Camera" TargetId="camera" />
        <Reference Name="FollowTarget" TargetId="player" />
        <Reference Name="InfoLabel" TargetId="cameraInfoText" />
    </References>
</EntityDefinition>
```

## Migration Notes

- `SoundButtonEntity`, `VolumeButtonEntity`, and `CameraEntity` are **deleted**. Any C# code that
  created them must build the equivalent component stack on a `GameObjectEntity` instead.
- Entity-level `<EntityOverrides>` targeting `CameraSpeed` are replaced by a component-targeted
  `MoveSpeed` property on `CameraInputComponent`.
- The dead C# scene subclasses `GuiAnchorDemoScene` and `LabelAlignmentDemoScene` were removed in the
  same change — their XML scene files (`Scenes/GuiAnchorDemo.xml`, `Scenes/LabelAlignmentDemoScene.xml`)
  are the only runtime path.
