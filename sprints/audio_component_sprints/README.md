# Built-In Audio Components — Scrum Sprints 🎵

## Why This?

The library's audio system today is a single flat singleton: `AudioManager` tracks playback by opaque GUID strings, has **no per-instance volume/pitch/pan API**, and no concept of channels (music vs. SFX). Because nothing binds audio to an entity's lifecycle, the playground carries five thin wrapper components (`MusicComponent`, `SoundKeyComponent`, `SoundButtonComponent`, `VolumeKeyComponent`, `VolumeButtonComponent`) purely to remember ids and stop sounds on detach.

This project brings audio up to the same standard as the rest of the Entity-Component Framework: playback state lives on the entity that makes the sound (Unity-`AudioSource` style), audience/global state lives on a single listener component (Unity-`AudioListener` style, adapted honestly for MonoGame's lack of native spatial audio), and global mixing becomes a first-class concept (channels).

## Before / After

```
Before:                              After:
CoreEssentials/src/Audio/            CoreEssentials/src/Audio/
├── AudioManager.cs   (flat)         ├── AudioManager.cs   (per-instance volume/pitch/pan, channels)
├── AudioClipInstance.cs             ├── AudioChannel.cs   (new: Music / SFX / Master enum)
├── ISoundEffect*.cs                 └── AudioClipInstance.cs
                                     CoreEssentials/src/GameSystems/EntitySystems/EntityOOPSystem/Components/BuiltIn/
Playground/Components/               ├── AudioSourceComponent.cs   (new, built-in)
├── MusicComponent.cs      ✂ deleted ├── AudioListenerComponent.cs (new, built-in: master volume + opt-in 2D spatialization)
├── SoundKeyComponent.cs   ⇄ rebased
├── SoundButtonComponent.cs✂ deleted
├── VolumeKeyComponent.cs  ⇄ rebased
└── VolumeButtonComponent.cs⇄ rebased
```

## Sprint Roadmap

| Sprint # | Name | Points | Status | Description |
|----------|------|--------|--------|-------------|
| 1 | [Sprint_1_AudioComponents](./Sprint_1_AudioComponents.md) | 7 | ✅ Complete (2026-09-06) | AudioManager upgrades (per-instance volume/pitch/pan, channels), built-in `AudioSourceComponent`, built-in `AudioListenerComponent` with opt-in 2D spatialization, playground migration off the five wrapper components. **Final sprint of this branch/feature set.** |

## Point Summary

- Total: **7 points** — at the per-sprint cap. It bundles three previously separate efforts (manager upgrades → source component → listener) into one because they are tightly coupled and this is the branch's last feature set. T4 (opt-in 2D spatialization) is the designated stretch item: if scope pressure appears, it defers to a follow-up and the sprint lands at 6 points with the rest of the audio track intact.
- The acceptance bar for the branch's final state includes the full suite green and all 7 scenes smoke-running PASS.

## Workflow Phases

1. **Foundation** — `AudioManager` API upgrades (per-instance volume/pitch/pan, channels).
2. **Core Implementation** — `AudioSourceComponent` + `AudioListenerComponent` as built-ins.
3. **Migration** — playground scenes rewired to the built-ins; five wrapper components deleted.
4. **Quality Gate** — full suite + smoke run of all 7 scenes.

## Sprint Structure

- ⭐ = user-facing / public API
- 🔒 = internal use only
- 🔁 = validation task (build + tests)
