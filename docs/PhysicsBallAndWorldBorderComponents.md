# Physics Ball & World Border Components

The physics demo scene (`Scenes/PhysicsEntityScene.xml`) used to depend on two hand-written
entity subclasses — `Ball` and `WorldBorder`. Both are now declared **purely from data** using
components, and both classes have been deleted. This page documents the three pieces that replace
them:

- [`WorldBorderComponent`](#worldbordercomponent) — builds the four static arena borders.
- [`BallMovementComponent`](#ballmovementcomponent) — the random "kick + spin" ball movement (+ random display scale).
- [`BallSaveComponent`](#ballsavecomponent) — the playground-owned save component that lets a plain
  `GameObjectEntity` round-trip through the framework serializer.

## WorldBorderComponent

Builds four static physics bodies (left, right, top, bottom) sized to `Size` at the owner's
position, so balls stay inside the arena. On attach it resolves the pipe-separated category names
in `BorderCategoryMask` from the engine's `PhysicsConfig`, so each border collider both belongs to
and collides with every named ball category — a regression here would silently let balls escape.

### Parameters

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `Size` | `Vector2` | `(0, 0)` | Arena size in pixels. Must be non-zero on both axes or no borders are created. |
| `BorderCategoryMask` | `string` | `"Player\|Vip"` | Pipe-separated named collision categories resolved from the engine's `PhysicsConfig`. |

### Example

```xml
<EntityDefinition Type="CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.GameObjectEntity" Id="worldBorder">
    <Position X="0" Y="0" />
    <Components>
        <Component Type="WorldBorderComponent">
            <Properties>
                <Property Name="Size" Value="1280,720" />
            </Properties>
        </Component>
    </Components>
</EntityDefinition>
```

In the physics demo the border is not declared in the scene — `PhysicsSpawnComponent` creates it
programmatically at startup (`CreateWorldBorderEntity`) so its size tracks the configured arena.

## BallMovementComponent

Gives the owning entity (a ball carrying a built-in `RigidbodyComponent`) the random movement that
used to live in the hand-written ball: on attach it rolls a random display scale for the ball (so
balls come in varied sizes) — but only when the owner's scale is still the default `(1, 1)`, so an
explicit/VIP scale or a scale restored from a save wins — and starts a coroutine that repeatedly
applies a random-direction impulse plus a random angular (spin) impulse, waiting a random number of
seconds between kicks. On detach it stops all of its coroutines. When no `RigidbodyComponent` is
attached the component is inert.

### Parameters

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `ImpulseStrength` | `float` | `500000` | Magnitude of each random-direction kick impulse. |
| `SpinImpulseHalfRange` | `float` | `5` | Half-range of the random angular (spin) impulse, uniform in `[-r, r]`. |
| `MinWaitSeconds` | `int` | `1` | Inclusive minimum seconds between kicks. |
| `MaxWaitSeconds` | `int` | `4` | Inclusive maximum seconds between kicks. |

### Example

```xml
<Component Type="BallMovementComponent" />
```

No properties are required — the defaults reproduce the original ball's behavior.

## BallSaveComponent

The framework's `GameStateSerializer` saves an entity iff it carries an `ISaveableComponent`, by
calling *that component's* `SaveState()`/`LoadState()`. The ball is a plain `GameObjectEntity`, so
its saveability comes entirely from attaching this playground-owned component — no entity subclass:

```csharp
public class BallSaveComponent : EntityComponent, ISaveableComponent
```

It uses **explicit** serialization — the ball saves exactly the state it needs to restore:

- `SaveState()` writes the owner's transform (position/rotation/scale/sort/active), its tags, and
  the specific state of the components a ball needs — sprite color/asset (`<SpriteState/>`),
  rigidbody mass + velocity (`<RigidbodyState/>`), and collider settings (`<ColliderState/>`).
  Each value is read by name from the sibling component's public properties; there is no
  per-component serialization interface. The framework serializer stamps the authoritative
  `Prefab="..."` attribute on the returned element.
- `LoadState()` restores them. Before applying a `RigidbodyComponent`'s saved velocity, it calls
  `CreateBody()` first (if the body does not yet exist) so the linear/angular velocity lands on a
  real body at the restored position.

The ball's runtime hydration is now fully data-driven: the prefab declares every component, and
`sprites`/collider/rigidbody are created by their own `OnAttach`. The random display scale lives in
`BallMovementComponent.OnAttach` (guarded so explicit/VIP/loaded scales win).

### Ball declaration (from `Content/Templates/BallTemplate.xml`)

```xml
<Prefab Type="CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.GameObjectEntity" Rotation="0" Sort="0" Active="true">
    <Tags>
        <Tag Name="Ball" />
        <Tag Name="Physical" />
    </Tags>
    <Components>
        <Component Type="SpriteComponent">
            <Properties>
                <Property Name="SpriteAsset" Value="Sprites/ball_sprite.xml" />
                <Property Name="Color" Value="White" />
                <Property Name="Origin" Value="0.5,0.5" />
            </Properties>
        </Component>
        <Component Type="RigidbodyComponent">
            <Properties>
                <Property Name="FixedRotation" Value="false" />
                <Property Name="Mass" Value="1.0" />
            </Properties>
        </Component>
        <!-- Fully declared in XML (possible after the parameterless ctor + settable ShapeType). -->
        <Component Type="ColliderComponent">
            <Properties>
                <Property Name="ShapeType" Value="Circle" />
                <Property Name="Restitution" Value="1.0" />
                <Property Name="Offset" Value="0,1" />
            </Properties>
        </Component>
        <Component Type="BallMovementComponent" />
        <!-- Saves/loads this ball through the framework serializer. -->
        <Component Type="BallSaveComponent" />
    </Components>
</Prefab>
```

### Save format

A saved ball now looks like this (the component-based shape, with the authoritative `Prefab` attribute):

```xml
<Entity Id="ball_1234" Prefab="BallPrefab" Type="CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.GameObjectEntity" Rotation="70.07" Sort="0" Active="true">
    <Position X="1088.8" Y="688.2" />
    <Scale X="0.79" Y="0.79" />
    <Tags>
        <Tag Name="Physical" />
        <Tag Name="Ball" />
    </Tags>
    <SpriteState ColorR="255" ColorG="255" ColorB="255" ColorA="255" OriginX="0.5" OriginY="0.5" Effects="" LayerDepth="0" SortOrderOverride="-1" AnimationFrame="0" SpriteAsset="Sprites/ball_sprite.xml" />
    <RigidbodyState Type="Dynamic" Mass="0.63" FixedRotation="False" SyncFromPhysics="True" LinearVelocityX="-50.6" LinearVelocityY="-8.7" AngularVelocity="13.0" />
    <ColliderState ShapeType="Circle" Friction="0.2" Restitution="1" Categories="Cat1" CollidesWith="All" OffsetX="0" OffsetY="1" Radius="0.4" />
</Entity>
```

This replaces the old bespoke `<Physics/>` + `<Sprite/>` shape. Any checked-in `*_Save.xml` from
the old format is obsolete and should be regenerated by saving in-game after this change.

## Tests

- `CoreEssentials.Tests/Playground/Sprint4PhysicsComponentTests.cs` — covers all three pieces:
  - `WorldBorderComponent` creates exactly four static bodies whose colliders carry the combined
    `Player|Vip` mask, and creates none for an invalid size.
  - `BallMovementComponent` applies an impulse on tick and stops cleanly on detach; it is inert
    without a rigidbody.
  - A full ball save/load round-trip through the component + prefab path: transform, tags, sprite
    color, and physics linear/angular velocity all survive. A hand-created ball with a save component
    but no registered prefab fails fast on save.
- `CoreEssentials.Tests/SceneManagement/Sprint5cPhysicsDataSceneTests.cs` — asserts the data-driven
  scene spawns 8 plain `GameObjectEntity` balls carrying a `BallSaveComponent` (5 regular + 3 VIP)
  and a border carrying a `WorldBorderComponent`.
