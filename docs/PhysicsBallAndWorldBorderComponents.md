# Physics Ball & World Border Components

The physics demo scene (`Scenes/PhysicsEntityScene.xml`) used to depend on two hand-written
entity subclasses — `Ball` and `WorldBorder`. Both are now declared **purely from data** using
components, and both classes have been deleted. This page documents the three pieces that replace
them:

- [`WorldBorderComponent`](#worldbordercomponent) — builds the four static arena borders.
- [`BallMovementComponent`](#ballmovementcomponent) — the random "kick + spin" ball movement.
- [`GameEntity`](#gameentity--the-saveload-shim) — the thin save/load shim that lets a plain
  component-composed entity round-trip through the generic serializer.

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
used to live in the hand-written ball: a coroutine that repeatedly applies a random-direction
impulse plus a random angular (spin) impulse, waiting a random number of seconds between kicks.
On detach it stops all of its coroutines. When no `RigidbodyComponent` is attached the component is
inert.

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

## GameEntity — the save/load shim

The framework's `GameStateSerializer` only saves entities that implement `ISaveableEntity`, by
calling *their* `SaveState()`/`LoadState()`. The framework's own `GameObjectEntity` does not
implement that interface, and this project does not modify framework code. So the physics ball is
declared as a thin playground-local subclass:

```csharp
public class GameEntity : GameObjectEntity, ISaveableEntity
```

It uses **generic** serialization — no per-entity save/load code:

- `SaveState()` writes the entity transform (position/rotation/scale/sort/active), its tags, and
  the serialized state of **every attached `ISerializableComponent`** (the built-in
  `SpriteComponent`, `RigidbodyComponent`, and `ColliderComponent` all implement it).
- `LoadState()` restores them. Before deserializing a `RigidbodyComponent` whose body does not yet
  exist, it calls `CreateBody()` first so the saved linear/angular velocity is applied to a real
  body at the restored position.

Any component that implements `ISerializableComponent` round-trips for free — adding one to a
`GameEntity` requires no new save/load code.

`GameEntity.OnStart` also owns the ball's runtime hydration (mirroring the old hand-written ball):
it rolls a random display scale for fresh balls, loads the ball sprite synchronously, and creates
any missing `RigidbodyComponent`/`ColliderComponent`. Each addition is guarded by "only if not
already present", so a deserialized ball is never double-hydrated.

### Ball declaration (from `Content/Templates/BallTemplate.xml`)

```xml
<Prefab Type="CoreEssentials.Playground.Entities.GameEntity" Rotation="0" Sort="0" Active="true">
    <Tags>
        <Tag Name="Ball" />
        <Tag Name="Physical" />
    </Tags>
    <Components>
        <Component Type="SpriteComponent">
            <Properties>
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
        <Component Type="ColliderComponent">
            <Properties>
                <Property Name="Restitution" Value="1.0" />
            </Properties>
        </Component>
        <Component Type="BallMovementComponent" />
    </Components>
</Prefab>
```

### Save format

A saved ball now looks like this (the generic per-component shape):

```xml
<Entity Id="ball_1234" Type="CoreEssentials.Playground.Entities.GameEntity" Rotation="70.07" Sort="0" Active="true">
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
  - A full `GameEntity` save/load round-trip: transform, tags, sprite color, and physics
    linear/angular velocity all survive through the generic serializer.
- `CoreEssentials.Tests/SceneManagement/Sprint5cPhysicsDataSceneTests.cs` — asserts the data-driven
  scene spawns 8 `GameEntity` balls (5 regular + 3 VIP) and a border carrying a
  `WorldBorderComponent`.
