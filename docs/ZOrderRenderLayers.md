# Z-Order Render Layers

Z-order render layers let you control the draw order of entities across **different textures**. They solve a limitation of texture-based render batching: when entities with different textures need to interleave in depth, pure texture batching breaks the render order.

## The Problem

Texture batching groups all entities that share a texture and draws each group in a single `SpriteBatch` call. This is fast, but it forces all entities of one texture to be drawn before the next texture:

```
Entity A1 (Texture A, sort=10) → should be behind B
Entity B1 (Texture B, sort=20) → should be in the middle
Entity A2 (Texture A, sort=30) → should be in front of B
```

With pure texture batching, all A's are drawn together, then all B's — so B ends up in front of **everything**, which is wrong.

## The Solution: Z-Layers

A **z-layer** is an integer on each entity that groups entities into depth bands. Rendering proceeds **back-to-front** (low z-layer to high z-layer), and within each layer entities are batched by texture. This preserves batching while allowing textures to interleave correctly across layers.

```
Before:  {Texture A: [A1, A2], Texture B: [B1]}
          → Renders all A's, then all B's (wrong order)

After:   {Z0: {Texture A: [A1]}, Z1: {Texture B: [B1]}, Z2: {Texture A: [A2]}}
          → Renders Z0(A1), then Z1(B1), then Z2(A2) ✅
```

## Quick Start

```csharp
// Assign entities to z-layers. Lower layers render first (further back).
groundEntity.ZLayer = 0;   // background
playerEntity.ZLayer = 1;   // mid-ground
uiOverlay.ZLayer = 2;      // foreground
```

Entities that never set a z-layer default to **layer 0**, which preserves the previous texture-only batching behavior.

## API Reference

| Member | Type | Description |
|--------|------|-------------|
| `Entity.ZLayer` | `int` (get/set) | The z-order layer. Lower values render first (further back). Default `0`. |
| `Entity.SetZLayer(int)` | `Entity` | Sets the z-layer and returns the entity for chaining. |
| `Entity.GetZLayer()` | `int` | Returns the current z-layer. |

## How Rendering Works

1. **Group by z-layer** — Active entities are bucketed by their `ZLayer`, in ascending order.
2. **Within each layer, group by texture** — Entities sharing a texture in the same layer are batched together.
3. **Render back-to-front** — Layers are drawn from lowest to highest. Each (layer, texture) group uses a single `SpriteBatch.Begin/End` pair.
4. **Sort order** — Within a single layer and texture, entities keep their `sort` order (higher `sort` first).

### Trade-offs

- **More `SpriteBatch.Begin/End` calls** than pure texture batching (one per texture per layer, instead of one per texture).
- **Still fewer calls** than rendering every entity individually.
- **Correct render order** is maintained across textures.

## Layering Examples

### Background / Mid-ground / Foreground

```csharp
skyEntity.ZLayer = 0;        // always behind
buildingEntity.ZLayer = 1;   // mid-ground
characterEntity.ZLayer = 2;  // in front of buildings
particleEntity.ZLayer = 3;   // top-most effects
```

### Interleaving the Same Texture

You can place the same texture in different layers to force it behind or in front of other textures:

```csharp
// Both use Texture A, but A2 must render in front of B1.
a1.ZLayer = 0;   // Texture A, behind
b1.ZLayer = 1;   // Texture B, middle
a2.ZLayer = 2;   // Texture A, in front
```

### Keeping a UI Overlay on Top

```csharp
hud.ZLayer = 100; // reserve a high layer so it always renders last
```

## Relationship to `sort`

- `ZLayer` controls **which band** an entity is drawn in (coarse, cross-texture).
- `sort` controls the **order within a band + texture** (fine-grained).

Use `ZLayer` when you need to interleave different textures in depth, and `sort` for fine ordering among entities that share a layer and texture.

> **Note:** Z-layers are a **rendering-only** concern. They do not affect collision. Collision filtering is handled separately by the `CollisionCategory` flags (see [Collision Groups & Filtering](./CollisionGroups.md)).

## See Also

- [Entity System](./EntitySystem.md)
- [Entity Position & Rotation](./EntityPositionRotation.md)
- [Collision Groups & Filtering](./CollisionGroups.md)
