# Character Components

The playground's three hand-written character entities — `CharacterEntity`, `AnimatedCharacterEntity`,
and `PlayerEntity` (which inherited from `CharacterEntity`) — have been replaced by a small set of
reusable, declarative components. Each character is now a plain `GameObjectEntity` carrying the right
set of `<Component>` declarations, so its behavior comes entirely from data with **no entity subclass**
and **no inheritance**.

This page documents those components: what they do, their configurable properties, and how to declare
them in scene/template XML.

## The Building Blocks

| Component | Purpose | Replaced |
|-----------|---------|----------|
| Built-in `SpriteComponent` (`SpriteAsset` property) | Loads a sprite asset via the `AssetManager` and renders it. | The inline `AssetManager.LoadAsset<Sprite>` + `AddComponent(new SpriteComponent(...))` in `CharacterEntity` / `AnimatedCharacterEntity`. |
| `BounceTweenComponent` | Makes the entity bob up and down with a looping, eased Y-offset. | The `_yOffsetTween` logic in `CharacterEntity.Update`. |
| `MoveByKeysComponent` | Moves the entity with the arrow keys. | The input handling in `PlayerEntity.Update`. |
| Built-in `AnimationComponent` (`SpriteAsset` + `AnimationName` properties) | Loads an animated sprite, registers and plays a named walk cycle. | The inline wiring in `AnimatedCharacterEntity.OnStart`. |
| `PauseScaleComponent` | Scales the entity up while the app is paused (window unfocused) and restores it on resume. | The `OnApplicationPause` overrides shared by both character entities. |

> **Declarative visuals.** A component must never *create* a sibling from `OnAttach` or `Update` —
> the entity iterates its live component collection during both passes, so adding one would throw
> "Collection was modified". The framework's answer is the built-in `SpriteAsset` properties on
> `SpriteComponent` and `AnimationComponent`: they turn the string→asset bridge into a plain XML
> property (resolved through the `AssetManager` on attach), so no per-game loader components are
> needed at all.

## Built-in SpriteComponent — declarative visuals

The built-in `SpriteComponent` accepts a `SpriteAsset` string property. On attach it loads the asset
through the `AssetManager` and assigns it to its `Sprite` property — but only when no explicit
sprite was set already, so a sprite assigned in code always wins. A missing asset is logged and
swallowed (the component attaches with a null sprite instead of breaking entity creation).

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `SpriteAsset` | `string` | `""` | Asset name of the sprite to load (e.g. `Sprites/character_sprite.xml`). |

```xml
<Component Type="SpriteComponent">
    <Properties>
        <Property Name="Origin" Value="0.5,0.5" />
        <Property Name="SpriteAsset" Value="Sprites/character_sprite.xml" />
    </Properties>
</Component>
```

## BounceTweenComponent

Makes the owning entity bob up and down. The baseline Y is captured on the **first** `Update` (the XML
position is applied *after* `OnStart`, so it cannot be read at construction), and every frame the
entity's Y is set to `baseline + tweenedOffset` while X is left untouched — so it composes cleanly with
horizontal movement from `MoveByKeysComponent`.

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `Amplitude` | `float` | `50` | How far (in pixels, upward) the entity bobs. The offset eases from `0` to `-Amplitude`. |
| `Duration` | `float` | `1.5` | Duration of one `0 → -Amplitude` leg in seconds. |
| `Loop` | `bool` | `true` | Whether the bounce repeats forever. |
| `Reverse` | `bool` | `true` | When looping, ping-pong (reverse each leg) instead of snapping back. |
| `Easing` | `Func<float,float>` | `EasingFunctions.InOutSine` | Easing applied to each leg. |

```xml
<Component Type="CoreEssentials.Playground.Components.BounceTweenComponent">
    <Properties>
        <Property Name="Amplitude" Value="50" />
        <Property Name="Duration" Value="1.5" />
        <Property Name="Loop" Value="true" />
        <Property Name="Reverse" Value="true" />
    </Properties>
</Component>
```

## MoveByKeysComponent

Moves the owning entity with the arrow keys. Each held key nudges one axis by `MoveSpeed × DeltaTime`
(milliseconds), mapping left/right/up/down to `-X/+X/-Y/+Y`. Key bindings and speed are declarative,
mirroring `CameraInputComponent`.

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `LeftKey` | `Keys` | `Left` | Move left. |
| `RightKey` | `Keys` | `Right` | Move right. |
| `UpKey` | `Keys` | `Up` | Move up. |
| `DownKey` | `Keys` | `Down` | Move down. |
| `MoveSpeed` | `float` | `1` | Speed in world units per millisecond (matches the original `PlayerEntity`). |

```xml
<Component Type="CoreEssentials.Playground.Components.MoveByKeysComponent">
    <Properties>
        <Property Name="MoveSpeed" Value="1" />
    </Properties>
</Component>
```

## Built-in AnimationComponent — declarative walk cycles

The built-in `AnimationComponent` (a pure controller that advances frames into the entity's
`SpriteComponent`) accepts `SpriteAsset` + `AnimationName` string properties. On attach it loads the
asset through the `AssetManager`, registers it as an animation under `AnimationName`, and starts
playing it. If an animation is already registered under that name (e.g. by code in an entity's own
`OnStart`), the code-registered one wins and the asset is not loaded.

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `SpriteAsset` | `string` | `""` | Asset name of the animated sprite (e.g. `Sprites/character_anim_walk.xml`). |
| `AnimationName` | `string` | `walk` | Name used to register and immediately play the loaded animation. |

```xml
<Component Type="SpriteComponent">
    <Properties>
        <Property Name="Origin" Value="0.5,0.5" />
        <Property Name="SpriteAsset" Value="Sprites/character_anim_walk.xml" />
    </Properties>
</Component>
<Component Type="CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Components.BuiltIn.AnimationComponent">
    <Properties>
        <Property Name="SpriteAsset" Value="Sprites/character_anim_walk.xml" />
        <Property Name="AnimationName" Value="walk" />
    </Properties>
</Component>
```

## PauseScaleComponent

Scales the owning entity up while the application is paused (window unfocused) and restores it on
resume. Because `Entity.OnApplicationPause` forwards to every attached component, a single declarative
instance reproduces the effect both old character entities shared via their `OnApplicationPause`
overrides — with no entity subclass.

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `PausedScale` | `float` | `1.5` | Scale applied while paused (matches the original characters). |

```xml
<Component Type="CoreEssentials.Playground.Components.PauseScaleComponent">
    <Properties>
        <Property Name="PausedScale" Value="1.5" />
    </Properties>
</Component>
```

## Example: A Fully Data-Driven Character

A static, bouncing character (as used for `staticCharacter` in `Scenes/CharacterScene.xml`):

```xml
<EntityDefinition Type="CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.GameObjectEntity" Id="staticCharacter">
    <Position X="320" Y="360" />
    <Tags>
        <Tag Name="Character" />
        <Tag Name="Static" />
    </Tags>
    <Components>
        <Component Type="SpriteComponent">
            <Properties>
                <Property Name="Origin" Value="0.5,0.5" />
                <Property Name="SpriteAsset" Value="Sprites/character_sprite.xml" />
            </Properties>
        </Component>
        <Component Type="CoreEssentials.Playground.Components.BounceTweenComponent">
            <Properties>
                <Property Name="Amplitude" Value="50" />
                <Property Name="Duration" Value="1.5" />
                <Property Name="Loop" Value="true" />
                <Property Name="Reverse" Value="true" />
            </Properties>
        </Component>
        <Component Type="CoreEssentials.Playground.Components.PauseScaleComponent">
            <Properties>
                <Property Name="PausedScale" Value="1.5" />
            </Properties>
        </Component>
    </Components>
</EntityDefinition>
```

A movable player is the same shape with `MoveByKeysComponent` added (and no `BounceTweenComponent` if
you don't want it to bob). An animated character swaps `BounceTweenComponent` for the built-in
`AnimationComponent` (with its own `SpriteAsset` + `AnimationName`) and points both components' visual
at the walk-cycle sprite.

---
*Created: 2026-09-05 | Updated: 2026-09-05 (loader components retired in favor of built-in SpriteAsset) | Part of Playground Entity → Components Project (Sprint 2)*
