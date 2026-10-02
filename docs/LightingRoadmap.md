# Lighting Roadmap (Design Note)

> **Status: Planned / Design-only.** This document records the decisions and open work for a future lighting feature. No code is implemented yet. It is written so the design is captured *before* any lighting code exists, because the depth model we chose dictates which lighting techniques are even possible.

Lighting in CoreEssentials will be **2D sprite lighting** — the same kind found in Hollow Knight, Celeste, Ori, and Dead Cells — not 3D per-pixel shading. This note explains what that means for our existing render pipeline, what "beautiful 2D" games actually do, and the concrete work needed to get there.

---

## The Core Decision: We Keep `ZLayer` / `sort` As-Is

The central question this discussion settled is whether lighting requires us to change or "upgrade" our depth model. **It does not.**

### `ZLayer` is *not* a fake hardware Z-buffer — it's the correct 2D abstraction

A common worry is that `ZLayer`/`sort` are "poor man's depth" that will need replacing with real (hardware) depth once lighting lands. That's the wrong frame:

- **MonoGame's `SpriteBatch` does not write to a Z-buffer.** It renders textured quads in draw order with alpha blending — no depth writes, no depth tests. The 0–1 `layerDepth` float is only a *sort key* for Deferred sort mode; it is never written as per-pixel depth to the GPU.
- To get "real" hardware depth you'd have to **abandon SpriteBatch** and write a custom quad renderer that enables depth writes. That's a large pipeline rewrite.
- More importantly, **it would make 2D look worse.** Hardware depth gives *per-pixel* occlusion; `ZLayer`/`sort` give *per-sprite* occlusion. In flat 2D art you want "the character is in front of the wall," not "the left half of my sprite is in front and the right half is behind." Discrete per-sprite layers match how 2D artists author scenes; a Z-buffer is a 3D concept forced onto flat art.

| | Painter's algorithm (`ZLayer`/`sort`) | Hardware Z-buffer |
|---|---|---|
| Granularity | Per-sprite (whole sprite in front or behind) | Per-pixel (sprite can be *partially* behind another) |
| How artists think about it | "Character on layer 2, wall on layer 1" | "This pixel is closer to the camera than that pixel" |
| Transparent pixels | Don't affect ordering (alpha blending) | Write depth anyway → hard edges / artifacts |
| Matches 2D art direction? | **Yes** — layers are how 2D scenes are authored | No — a 3D concept on flat art |

**Conclusion:** `ZLayer`/`sort` are the *correct* model for 2D sprite rendering. Every beautiful 2D game uses the painter's algorithm, not a Z-buffer. The data-driven `ZLayer` work (see [Z-Order Render Layers](./ZOrderRenderLayers.md)) is architecturally sound and lighting-compatible as-is. **No changes to `ZLayer`/`sort` semantics, polarity, or representation are planned.**

### Two independent axes — the invariant to protect

The one nuance worth locking in *before* lighting code exists is that people conflate two different orderings:

- **Paint order** — what's visually on top. This is `ZLayer` + `sort`. Painter's algorithm. Unchanged by lighting.
- **Occlusion relationship** — which light affects which surface, and which occluder blocks which light. This is a *separate* axis driven by the **occlusion mask** + a **light/mask layer tag**, *not* paint order.

> **Rule: `ZLayer`/`sort` decide what you *see*; masks + tags decide what *blocks light*. Keep them orthogonal.**

The failure mode to guard against is making occlusion piggyback on `ZLayer`/`sort` ("the occluder just needs a higher ZLayer than the light so it blocks it"). That works until art order and light order disagree — e.g. a wall that's visually *behind* the character but should still cast its shadow — and then lighting breaks in ways that are painful to debug because the cause is draw-order, not lighting.

The reason beautiful 2D games get correct lighting with **no Z-buffer** is precisely that they keep these two axes separate: a wall blocks a character's light because of its *mask + light-layer tag*, not because it was drawn after the character. "In front in the frame" ≠ "blocks light."

---

## What "Beautiful 2D" Games Actually Do

Live technical sources (Unity URP 2D docs, Gamasutra features) were unavailable at writing time, so this is synthesized from well-established, stable technique knowledge. The reference implementation to clone conceptually is **Unity URP 2D Lighting**, which is exactly the "light = entity + mask + layer" pattern.

| Concept | What it is | CoreEssentials equivalent |
|---|---|---|
| **Light source** (Point / Spot / Directional) | An object that emits light into a 2D lightmap or in realtime | An **entity** with a `LightComponent` — inherits `ZLayer`/`sort` for free |
| **Light type** | Point = radial falloff; Spot = cone; Directional = parallel across screen | Shader parameters on the light's effect |
| **Occlusion mask** | A separate *alpha* texture authored per sprite; opaque pixels block light, transparent don't. This is the fake "per-pixel depth" | A second material/texture channel per entity, or a dedicated occluder sprite. **The key new primitive.** |
| **Light layers / mask layers** | Lights illuminate only sprites tagged with matching layers; occluders block only lights on matching layers | A tag/mask field on entities (we already have `EntityTags`) |
| **Baked vs realtime** | Baked = lightmap texture generated at edit-time (cheap, can't move). Realtime = computed per-frame (lights/occluders can move, costs more) | Bake-to-texture = a content/build step; realtime = the post-pass / per-sprite shader path |
| **Lighting blend** | `Soft` (multiply scene by lightmap) vs `Hard` (sharp shadows), plus an ambient/fog color for the "unlit" base | A sampling `PostPass`: multiply scene target by a lightmap, add ambient tint — already our seam |

The critical insight: **the occlusion mask is what substitutes for a Z-buffer.** Light doesn't ask "what's in front of me?" — it asks "is this pixel marked as opaque to my light layer?" That's a per-pixel boolean from an authored alpha channel, not geometry depth. This is why 2D lighting looks correct without any depth buffer.

### How the specific games do it

- **Hollow Knight** — the gold standard. Per-sprite occlusion masks + *realtime* point/spot lights (the lantern casts shadows in real time). Dark areas are a screen-space darkness multiply; lit surfaces are the sprite multiplied by the lightmap at its pixel.
- **Celeste** — heavily **baked**. Most lighting is pre-baked into the tileset/background art for performance; dynamic lights are sparse and cheap. Mood comes from baked gradients + a few realtime accents.
- **Ori (Blind Forest / Will of the Wisps)** — mostly **baked lightmaps + additive glow sprites**, with very little true occlusion. The "beautiful" look is ~90% art direction (glow sprites, bloom post-pass), not per-pixel lighting math. This validates glow-sprites as the *aesthetic* workhorse.
- **Dead Cells** — mostly baked/flat; the light feel comes from color grading + vignette (an additive `PostPass` — literally an existing feature).

The pattern: **baked for base mood + a few realtime lights for interaction + glow sprites + a grade pass.** Nobody does full deferred shading in 2D.

---

## The Three Architectural Paths

Ranked by how much depth information the light must read:

- **Path A — Light/glow sprites (no real depth).** A light *is* an entity: a big soft additive sprite, plus a full-screen darkness multiply. Occlusion is faked with alpha "occlusion masks." Lights interleave with the scene via `ZLayer`/`sort`. Fits the current codebase almost for free — the additive/sampling `PostPass` modes are the seam; light sprites ride the existing batcher.
- **Path C — Forward per-sprite lighting (per-sprite, still no real depth).** Each sprite's shader receives light parameters (positions, intensities) as uniforms and shades itself in the fragment stage. Occlusion is *still* draw-order + masks, not per-pixel. Fits `ShaderComponent` with light uniforms, batched by the existing `(effect, signature)` runs.
- **Path B — Deferred / G-buffer (real depth).** Render into a target that also writes depth-stencil, then a lighting pass reads color + depth (+ normals) and shades per-pixel. Real cast shadows. This is the expensive one: the current `RenderTarget2D` is color-only; you'd need a depth-stencil view, and MonoGame's `SpriteBatch` isn't designed to write meaningful per-sprite depth — likely a move to a custom quad renderer. **Skip unless a real game demands it.**

**Recommendation: A → C, keep the door to B open cheaply.**

---

## What We Need To Get Done

Ordered by "how much of the beautiful-2D look you get per unit of architecture." Items 1–4 compose on top of existing seams; item 5 is deferred.

### 1. Glow / light sprites as entities (Path A) — *do first*
Additive soft sprites on their own z-layer, plus a full-screen darkness multiply via the existing sampling `PostPass`. Gets ~80% of Ori's look with **zero new pipeline primitives**. The immediate win; almost entirely already in the codebase.
- [ ] Define how a "light volume" entity is declared (likely a sprite + additive blend on its own `ZLayer`).
- [ ] Provide or document the darkness-multiply sampling post-pass as the base lighting pass.
- [ ] Document the layer convention: light volume on layer N, occluders on N+1.

### 2. Per-sprite occlusion mask (Path C) — *the real unlock*
Add an optional "occlusion material" / alpha channel to a sprite. A light then shades each pixel by `lightmap × (mask ? 1 : ambient)`. This is the **one new primitive** needed beyond today's seams, and it's what turns "pretty glow" into "Hollow Knight shadows."
- [ ] Decide: second texture slot on `SpriteComponent`, or a dedicated occluder sprite? (This is the single design decision that sets our ceiling.)
- [ ] Wire the mask through the per-sprite `ShaderComponent` so it's batched by the existing `(effect, signature)` runs.
- [ ] Define the ambient/fallback color for unlit pixels.

> **The one primitive to decide:** *Do we add an optional per-sprite occlusion/alpha channel to the sprite renderer?* Yes → realtime masks (Hollow Knight), baked lightmaps, layer-restricted lights — full beautiful-2D ceiling. No → capped at glow sprites + full-screen grade (Ori-lite / Dead Cells look).

### 3. Light layers via tags (cheap once #2 exists)
Map light ↔ occluder ↔ lit-sprite relationships to `EntityTags` so a lantern only lights the "world" layer and not the "UI/HUD" layer. Authoring-friendly and data-driven — fits the XML scene model (`<Light>` element with a layer attribute).
- [ ] Define the tag/mask vocabulary (light layer, mask layer, lit layer).
- [ ] Confirm `EntityTags` is sufficient or add a dedicated light-layer field.

### 4. Baked lightmaps as content (Path A/C hybrid)
Let artists ship a pre-baked lightmap texture per scene region; the post-pass just multiplies it in. Zero runtime cost, maximum control — how Celeste/Ori actually ship their base lighting.
- [ ] Define the content format / asset type for a baked lightmap.
- [ ] Provide the multiply-in sampling post-pass (overlaps with #1's darkness pass).

### 5. Realtime shadow-casting (Path B) — *deferred*
A depth-stencil target + custom quad renderer for true cast shadows is a large pipeline rewrite for a 2D library where masks already give the same visual result. **Do not build until a real game demands it.**
- [ ] Forward-compat note: when the render-to-target path is built, let the target *carry* a depth-stencil view (even unused) so "real shadows" later is additive rather than a rewrite.

---

## Data Model (When It Lands)

Lighting extends the **same** data-driven XML system we already have — it does not change `ZLayer`:

- A `<Light>` element: position, type (point/spot/directional), intensity, color → a light entity. Its **paint** layer = `ZLayer` (already exists).
- An occluder role: a flag/tag on an entity meaning "I have an occlusion mask" → drives the *occlusion* axis, not paint order.
- A light-layer / mask-layer tag so a lantern lights the "world" layer but not the "HUD" layer.

Net effect on the existing primitives: **`ZLayer` and `sort` are untouched.** They gain new *users* (lights/glow/occluders) and one *orthogonal companion axis* (masks + tags) that lighting introduces alongside them — but their semantics, polarity, and data-driven representation don't move a single line.

---

## Related Documentation

- [Z-Order Render Layers](./ZOrderRenderLayers.md) — `ZLayer`/`sort` semantics and the data-driven (XML) representation this lighting design builds on.
- [Render Pipeline](./RenderPipeline.md) — pre/process/post passes; the sampling `PostPass` is the seam for darkness/lightmap multiply.
- [Shader Uniforms](./ShaderUniforms.md) — per-sprite effect uniforms, the mechanism Path C drives lights through.
- [Entity Tags](./EntityTags.md) — the existing tagging system that light/mask layers will build on.
