# Sprint 0 — Prefab Registration & Overrides 🧱

**Points:** 5 | **Status:** ✅ Done (2026-08-31) | **Goal:** Prefab registration semantics + property-override core, no XML yet.

## Tasks

- [x] T1 ⭐ `EntitySystem.RegisterPrefab(name, assetName)` + `RegisterPrefab(name, prefab)` — idempotent (re-register replaces + logs), optional strict mode
- [x] T2 ⭐ `bool HasPrefab(string name)`
- [x] T3 ⭐ Lazy `InstantiateFromAsset(assetName, position)` — auto-registers from Content on first use under the asset's base name, caches; explicit registration always wins
- [x] T4 🔒 Override-merge machinery in new `PrefabOverrides.Apply` (deep `Prefab.Clone()` per instantiation; cached prefab never mutated)
- [x] T5 ⭐ C# `Instantiate(name, position, overrides)` overload — applies before `OnAttach`, components see final values
- [x] T6 ⭐ Rename API to prefab vocabulary: `EntityTemplate`→`Prefab` (alias kept), `RegisterTemplate`→`RegisterPrefab`, `Entity/Component.InstantiateTemplate`→`InstantiatePrefab`; `[Obsolete]` shims kept one release
- [x] T7 🔁 Build + full test suite green — **1020 passed / 0 failed / 3 skipped** (baseline 1007, +13 new tests)

## Notes

- Deferred-attach window: while a prefab pass is in flight, `Entity.AddComponent` stores components without firing `OnAttach`; the loader completes attachment (`Entity.AttachPendingComponents`) after all properties + overrides are final. Covers both prefab-created components and ones the entity's own `OnStart` adds (e.g. Ball).
- `EntityComponent.Attach()` guarantees `OnAttach` runs exactly once per attachment; detach paths reset the flag so re-adds behave as before.

## Acceptance Criteria

- Consumer can instantiate a Content prefab with zero registration calls
- Duplicate registration is idempotent / detectable via `HasPrefab`
- Overrides visible to components in `OnAttach` (no deferred-init workaround)
- 1007+ tests passing, build clean

---
*Created: 2026-08-31 | Part of Scene-as-Data Project*
