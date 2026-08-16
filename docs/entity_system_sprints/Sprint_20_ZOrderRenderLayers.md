# Sprint 20 — Z-Order Render Layers 📐

**Points:** 4.5  
**Status:** ✅ Completed  
**Completed:** 2026-08-16  
**Sprint Goal:** Combine texture batching with z-order layers to maintain correct render order across different textures.

---

## Background

Sprint 5 introduced texture-based render batching to minimize `SpriteBatch.Begin/End` calls. However, this created a limitation:

**Problem:** When entities with different textures need to interleave in z-order, texture batching breaks the render order.

**Example:**
```
Entity A1 (Texture A, sort=10) → Behind B
Entity B1 (Texture B, sort=20) → Middle
Entity A2 (Texture A, sort=30) → In front of B
```

Current batching renders all A's together, then all B's — putting B in front of everything.

**Solution:** Group entities by both texture AND z-order layer to maintain correct interleaving.

---

## Tasks

- [x] **T1: Add z-order layer to Entity (1 pt)** 🔒 Internal
  - `ZLayer` property on Entity (int, default 0)
  - `ZLayer` takes precedence over `sort` for grouping
  - Entities in same z-layer can be batched by texture

- [x] **T2: Implement z-aware render batching (2 pts)** ⭐ User-facing
  - Sort entities by z-layer first, then by texture within each layer
  - Render layers back-to-front (low to high z-layer)
  - Within each layer, batch by texture
  - Single `SpriteBatch.Begin()` per texture per layer

- [x] **T3: Write unit tests (1 pt)** 🔁 Validation
  - Test entities render in correct z-order
  - Test batching still works within same z-layer
  - Test interleaved textures render correctly

- [x] **T4: Create user documentation (0.5 pt)** 📚 User-facing
  - Create `docs/ZOrderRenderLayers.md` user guide
  - Document z-layer concept
  - Document render batching with z-layers
  - Provide layering examples

---

## Acceptance Criteria

- [x] Entities can be assigned to z-layers
- [x] Z-layer determines render order across textures
- [x] Texture batching still works within each z-layer
- [x] Interleaved textures render in correct order
- [x] Project builds cleanly — **0 errors, 0 warnings**
- [x] All existing tests pass + new z-order tests added

---

## Deliverables

| File | Type | Visibility | Notes |
|------|------|------------|-------|
| `Entity.cs` | Modified | ⭐ PUBLIC | Add ZLayer property |
| `EntitySystem.cs` | Modified | ⭐ PUBLIC | Z-aware render batching |
| `EntityZOrderTests.cs` | New | 🔒 Internal | Unit tests for z-ordering |
| `docs/ZOrderRenderLayers.md` | New | ⭐ PUBLIC | User guide for z-order layers |

---

## Implementation Approach

```
Current:  {Texture A: [A1, A2, A3], Texture B: [B1]}
          → Renders all A's, then all B's (wrong order)

Proposed: {Z0: {Texture A: [A1]}, Z1: {Texture B: [B1]}, Z2: {Texture A: [A2, A3]}}
          → Renders Z0(A1), Z1(B1), Z2(A2, A3) in order ✅
```

**Trade-offs:**
- More `SpriteBatch.Begin/End` calls than pure texture batching
- Still fewer calls than no batching
- Correct render order maintained

---

## Notes & Risks

- **High priority** — solves the z-order limitation from Sprint 5
- Consider making z-layer optional (default 0 maintains backward compatibility)
- Could combine with spatial partitioning (Sprint 7) for even better performance
- Alternative: use `SpriteSortMode.BackToFront` with custom depth buffer
- **Out of scope: collision** — z-layers are a rendering concern only. Collision filtering is already handled by the `CollisionCategory` flags from Sprint 19; z-layers do not affect physics.

---

*Created: 2026-08-08 | Follow-up to Sprint 5 Render Batching*
