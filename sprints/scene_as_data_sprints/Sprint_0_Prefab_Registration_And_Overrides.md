# Sprint 0 — Prefab Registration & Overrides 🧱

**Points:** 5 | **Status:** In Progress | **Goal:** Prefab registration semantics + property-override core, no XML yet.

## Tasks

- [ ] T1 ⭐ `EntitySystem.RegisterPrefab(name, assetName)` + `RegisterPrefab(name, prefab)` — idempotent (re-register replaces + logs), optional strict mode
- [ ] T2 ⭐ `bool HasPrefab(string name)`
- [ ] T3 ⭐ Lazy `Instantiate(string assetName, Vector2 position)` — auto-registers from Content on first use, caches
- [ ] T4 🔒 Override-merge machinery in `EntityTemplateLoader` (per-instantiation copy of component definitions; cached prefab never mutated)
- [ ] T5 ⭐ C# `Instantiate(name, position, overrides)` overload — applies before `OnAttach`, components see final values
- [ ] T6 ⭐ Rename API to prefab vocabulary: `RegisterTemplate`→`RegisterPrefab`, `Entity.InstantiateTemplate`→`InstantiatePrefab`; keep `[Obsolete]` shims one release
- [ ] T7 🔁 Build + full test suite green

## Acceptance Criteria

- Consumer can instantiate a Content prefab with zero registration calls
- Duplicate registration is idempotent / detectable via `HasPrefab`
- Overrides visible to components in `OnAttach` (no deferred-init workaround)
- 1007+ tests passing, build clean

---
*Created: 2026-08-31 | Part of Scene-as-Data Project*
