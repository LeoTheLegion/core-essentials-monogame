# Sprint 1 — Built-In Audio Components 🎵

**Points:** 7 (at the per-sprint cap) | **Status:** ✅ Complete (2026-09-06) | **Goal:** Upgrade `AudioManager` with per-instance parameters and channels, add built-in `AudioSourceComponent` and `AudioListenerComponent` (Unity-`AudioSource`/`AudioListener` parity adapted for MonoGame), and migrate the playground off its five audio wrapper components. **This is the final sprint of this branch and feature set** — on completion the branch's acceptance bar (full suite green + all 7 scenes smoke-run PASS) must hold.

## Target Outcome

- `CoreEssentials/src/Audio/` gains per-instance volume/pitch/pan control, a minimal channel model (`Music` / `SFX` / `Master`), and loses its stray debug `Console.WriteLine`s from the playback path.
- Two new **built-in** components under `CoreEssentials/src/GameSystems/EntitySystems/EntityOOPSystem/Components/BuiltIn/`:
  - **`AudioSourceComponent`** — Unity-`AudioSource` parity: declarative `SoundAsset`, per-instance `Volume`/`Pitch`/`Pan`/`Loop`, `PlayOnAttach`, stops itself on detach (fixing the id-leak that forced `MusicComponent` to exist). Optional opt-in 2D spatialization (pan + distance attenuation relative to the listener) — our own math, since MonoGame has no native spatial audio; **default off**.
  - **`AudioListenerComponent`** — Unity-`AudioListener` parity: single per-scene audience on the camera entity. Owns master volume (absorbing `VolumeKeyComponent`/`VolumeButtonComponent` behavior) and is the position reference for spatial sources.
- The five playground wrappers (`MusicComponent`, `SoundKeyComponent`, `SoundButtonComponent`, `VolumeKeyComponent`, `VolumeButtonComponent`) are **deleted**; their scenes are rewired to built-ins + XML `<Bind>` wiring.

## Tasks

- [x] T1 ⭐ AudioManager upgrades: add per-instance setters `SetInstanceVolume(id, float)`, `SetInstancePitch(id, float)`, `SetInstancePan(id, float)` (threading through `AudioClipInstance` → the underlying `ISoundEffectInstance`); introduce `AudioChannel` enum (`Master`, `Music`, `Sfx`) with per-channel volume (`GetChannelVolume`/`SetChannelVolume`), effective instance volume = clip volume × channel volume × master volume; extend play entry points to accept a channel (default `Master` for one-shots, keep existing signatures source-compatible). Remove the `Console.WriteLine`s from `PlaySound` and `AudioClipInstance.UpdateVolume`.
- [x] T2 ⭐ Create built-in `AudioSourceComponent : EntityComponent`: properties `SoundAsset`, `Volume = 1f`, `Pitch = 1f`, `Pan = 0f`, `Loop = false`, `Channel = AudioChannel.Master`, `PlayOnAttach = true`; methods `Play()`, `PlayOneShot(string? asset = null)`, `Stop()`, `Pause()`, `Resume()`. Owns its instance id from `AudioManager` and stops it on detach (so a scene unload leaks nothing). All AudioManager calls behind `protected virtual` seams for test observability.
- [x] T3 ⭐ Create built-in `AudioListenerComponent : EntityComponent`: owns master volume (`MasterVolume`, applied via `AudioManager.SetMasterVolume`), exposes the listener position (owner's position) as the spatial reference, and is discoverable by sources (e.g. a static active-listener slot set on attach / cleared on detach — exactly one active listener per scene; warn if a second attaches).
- [x] T4 ⭐ *(stretch — defer first if scope pressure appears)* Opt-in 2D spatialization on `AudioSourceComponent`: `SpatialEnabled = false` by default, plus `MinDistance`/`MaxDistance`. When enabled and an active listener exists, each update computes pan from X-offset (clamped −1..1) and linear distance attenuation between min/max, applied through T1's per-instance setters. Pan/attenuation math lives in small pure static helpers so it is unit-testable without a live listener or engine.
- [x] T5 🔒 Tests: new test files for `AudioManager` channel/per-instance behavior, `AudioSourceComponent` lifecycle (play-on-attach, stop-on-detach, one-shot, pause/resume), `AudioListenerComponent` master-volume + single-active-listener rules, and the spatial math helpers. Update/delete the tests that covered the five deleted playground wrappers.
- [x] T6 🔁 Playground migration: rewire every scene/prefab referencing the five wrapper components — music scenes → `<AudioSourceComponent SoundAsset="..." Loop="true" Channel="Music" />`; sound-key scenes → `AudioSourceComponent` + key binding (or a bound method); button sounds → XML `<Bind>` of `Clicked` → `PlayOneShot`; volume controls → `AudioListenerComponent.MasterVolume` via key/button. Delete the five wrapper files and their now-dead usings in tests.
- [x] T7 🔒 Docs: update `docs/AudioSystem.md` (per-instance params, channels, listener notes) and add a new doc page for the two built-in components (usage, parameters, XML examples, spatialization opt-in). Update `docs/README.md` index if it lists pages.
- [x] T8 🔁 Build clean + full suite green + smoke-run all 7 scenes PASS (`pwsh ./scripts/run-all-scenes.ps1`).

## Acceptance Criteria

- Per-instance volume/pitch/pan and channel volumes are settable from code and verified by tests; existing `PlaySound`/`PlayOneShotSound` call sites still compile unchanged.
- An entity with `<AudioSourceComponent Loop="true" />` in XML plays on attach and stops on detach — no id leaks across scene transitions (verified by a test that detaches the owner and asserts the instance was stopped).
- Exactly one active `AudioListenerComponent` per scene; a second attach warns and does not silently steal the slot.
- Spatialization is off by default; when on, pan/attenuation match the pure-helper math in tests.
- No code references the five deleted playground components; all 7 scenes smoke-run PASS; full suite green.

## Deliverables

| File | Action | Purpose |
|------|--------|---------|
| `CoreEssentials/src/Audio/AudioManager.cs` | Modify | Per-instance volume/pitch/pan, channels, debug-log removal |
| `CoreEssentials/src/Audio/AudioClipInstance.cs` | Modify | Thread per-instance params to the underlying instance; drop debug log |
| `CoreEssentials/src/Audio/AudioChannel.cs` | Create | Channel enum (Master/Music/Sfx) |
| `CoreEssentials/.../Components/BuiltIn/AudioSourceComponent.cs` | Create | Built-in per-entity playback component |
| `CoreEssentials/.../Components/BuiltIn/AudioListenerComponent.cs` | Create | Built-in listener/master-volume/spatial-reference component |
| `CoreEssentials.Tests/**` | Create/Modify | New audio tests; wrapper-component tests updated/deleted |
| `CoreEssentials.Playground/Components/Music*.cs, Sound*.cs, Volume*.cs` | Delete | Replaced by built-ins + XML wiring |
| `CoreEssentials.Playground/Content/**/*.xml` | Modify | Rewire scenes to the built-in components |
| `docs/AudioSystem.md` + new component doc page | Modify/Create | Document the new API surface |

## Notes & Risks

- **MonoGame has no native spatial audio.** The listener is not a platform hook — pan/attenuation are our own per-frame math, which is why it is opt-in and default-off. Keep the math in pure static helpers so it stays testable and cheap to replace later.
- **Channel model is deliberately minimal** (three fixed channels, volume only). No mixer graph, no ducking curves — that's a future feature, not this sprint.
- **`AudioClipInstance.UpdateVolume` currently multiplies clip volume × master.** T1 must fold channel + per-instance factors into that single effective-volume computation in one place so the three knobs don't drift apart.
- **Prefab loader requires a true parameterless constructor** for both new components (lesson from the `RigidbodyComponent` optional-arg ctor bug) — do not give them optional-argument constructors.
- **Branch finality:** this is the last feature set on `feature/scene-as-data`. T8's smoke run of all 7 scenes is the branch's exit gate; do not leave any scene referencing a deleted component.

## Completion Record

**Completed:** 2026-09-06. All T1–T8 done; branch's exit gate holds (full suite **1227 passed / 0 failed / 3 skipped**; all 7 scenes smoke-run PASS).

Notes from execution:
- **Option A migration** was chosen for the playground: `MusicComponent` + `SoundButtonComponent` deleted outright; `SoundKeyComponent`, `VolumeKeyComponent` and `VolumeButtonComponent` were rebased onto the built-ins (keeping their protected-virtual seams) rather than deleted, so key/button input still routes through them.
- **`<Bind>` signature constraint:** `Action` events (e.g. `Clicked`) resolve a handler by name with ≤1 parameter, so the declarative one-shot target is the parameterless `PlayOneShotNow()` — `PlayOneShot(string?)` cannot be bound directly.
- **`MusicComponent` app-focus behavior preserved** via `AudioSourceComponent.OnApplicationPause(bool)`, which pauses/resumes only the main source it paused itself.
- **Pre-existing flake:** `EntitySystemTests.SpawnAfter_CreatesEntityAtPositionAfterDelay` is a coroutine-timing flake (passes in isolation and on re-run); unrelated to audio.

---
*Created: 2026-09-06 | Completed: 2026-09-06 | Part of Built-In Audio Components Project*
