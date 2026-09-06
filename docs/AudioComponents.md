# Audio Components (Per-Entity Sources & Listener)

CoreEssentials ships two built-in audio components that translate Unity's `AudioSource` /
`AudioListener` split onto the Entity-Component Framework. They let you attach a sound to a
specific entity and have its playback state live on that entity — it plays (optionally looping)
on attach, stops itself on detach (so unloading a scene leaks no instances), and exposes
per-instance volume, pitch, pan and channel.

Both components live in:

```csharp
CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Components.BuiltIn
```

Because they are built-in, they can be attached from code **or** declared directly in XML scenes /
templates — no registration required.

- [`AudioSourceComponent`](#audiosourcecomponent) — a sound that plays from an entity.
- [`AudioListenerComponent`](#audiolistenercomponent) — the single "audience" for a scene (master volume + spatial reference).

> **MonoGame note:** MonoGame has no native spatial audio or listener concept. Spatialization here
> is *opt-in software math* (see [2D Spatialization](#2d-spatialization-opt-in)) and the listener is
> a convention, not a platform hook.

---

## AudioSourceComponent

A per-entity sound source. Attach it to any entity and the sound's playback state lives on that
entity.

### Parameters

| Property | Type | Default | Description |
| --- | --- | --- | --- |
| `SoundAsset` | `string` | `""` | Asset name of the clip this source plays (e.g. `"Audio/background_music.xml"`). Empty = no playback. |
| `Volume` | `float` | `1f` | This source's volume multiplier (0.0–1.0), independent of clip/channel/master. |
| `Pitch` | `float` | `1f` | Pitch as a Unity-style ratio (1.0 = normal, 2.0 = one octave up). Converted to semitones internally. |
| `Pan` | `float` | `0f` | Stereo pan (-1 = left, 0 = center, +1 = right). Overridden per-frame when spatial is on. |
| `Loop` | `bool` | `false` | Whether the main source loops. Applied to the resolved clip before playback. |
| `Channel` | `AudioChannel` | `Master` | Channel this source plays on (which per-channel volume applies). |
| `PlayOnAttach` | `bool` | `true` | Start playing automatically on attach. Set `false` for one-shot-only sources (e.g. sound buttons). |
| `SpatialEnabled` | `bool` | `false` | Opt-in 2D spatialization (pan + attenuation against the active listener). Off by default. |
| `MinDistance` | `float` | `50f` | Distance within which a spatial source is heard at full volume. |
| `MaxDistance` | `float` | `500f` | Distance beyond which a spatial source is silent (also the pan normalization range). |

### Methods

| Method | Description |
| --- | --- |
| `Play()` | Starts (or resumes) the main source from `SoundAsset`, applying this component's volume/pitch/pan. No-op when no asset is set. |
| `Stop()` | Stops the main source. |
| `Pause()` / `Resume()` | Pauses / resumes the main source. |
| `PlayOneShot(string? asset = null)` | Fires a transient one-shot (fire-and-forget; pruned by the manager when done). Defaults to `SoundAsset`. Expects a non-looping SFX asset. |
| `PlayOneShotNow()` | Parameterless one-shot command target for declarative `<Bind>` wiring — plays this source's own `SoundAsset`. This is what a button's `Clicked` event binds to. |
| `IsPlaying` (property) | Whether the main source is currently playing (or paused). |
| `InstanceId` (property) | The id of the main playing instance, or null when not playing. |

Lifecycle: on **attach** it resolves `SoundAsset` and (if `PlayOnAttach`) plays; on **detach** it
stops every instance it owns (main + one-shots). When the application is backgrounded it pauses the
main source and resumes it on return — but only a source it paused itself, so an intentionally
paused source is left alone.

### Code usage

```csharp
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Components.BuiltIn;

// A looping music shell on a plain entity:
var music = new Entity();
music.AddComponent(new AudioSourceComponent
{
    SoundAsset = "Audio/song1_sound.xml",
    Loop       = true,
    Channel    = AudioChannel.Music,
    Volume     = 0.7f
});
// Plays on attach; stops automatically when the entity is removed / scene unloaded.

// A one-shot source (e.g. a footstep trigger):
var source = owner.AddComponent(new AudioSourceComponent { PlayOnAttach = false });
source.PlayOneShot("footstep1_sound.xml");   // or just set SoundAsset and call PlayOneShot()
```

### XML usage

**Looping music shell** on a plain entity:

```xml
<Entity Name="Music" Type="Entity">
  <Component Type="AudioSourceComponent">
    <Properties>
      <Property Name="SoundAsset" Value="Audio/song1_sound.xml" />
      <Property Name="Loop" Value="true" />
      <Property Name="Channel" Value="Music" />
      <Property Name="Volume" Value="0.7" />
    </Properties>
  </Component>
</Entity>
```

**Sound button declared purely from data.** A template wires a built-in `AudioSourceComponent` to
the button's `Clicked` event with a `<Bind>` — no per-button C# needed:

```xml
<Component Type="AudioSourceComponent">
  <Properties>
    <Property Name="PlayOnAttach" Value="false" />
    <Property Name="Channel" Value="Sfx" />
  </Properties>
</Component>
<!-- The button's Clicked event fires PlayOneShotNow, which plays the source's SoundAsset. -->
<Bind Event="Clicked" Command="PlayOneShotNow" />
```

A per-button override then only sets `SoundAsset`:

```xml
<Component Type="AudioSourceComponent">
  <Properties>
    <Property Name="SoundAsset" Value="Audio/footstep1_sound.xml" />
  </Properties>
</Component>
```

> **Why `PlayOneShotNow`?** `<Bind>` resolves a handler by name and requires a **0-parameter**
> method for `Action` events such as `Clicked`. `PlayOneShot(string?)` takes one parameter and so
> cannot be bound directly; the parameterless `PlayOneShotNow()` exists specifically to be the
> declarative target.

### 2D Spatialization (opt-in)

Set `SpatialEnabled = true` and each frame the source derives its **pan** from the horizontal offset
relative to the active listener and its **attenuation** from distance:

- Pan maps the clamped `offset / MaxDistance` into `-1…+1`.
- Attenuation is `1.0` inside `MinDistance`, `0.0` at/over `MaxDistance`, and linear in between.
- The effective volume becomes `Volume × attenuation`; pan replaces the static `Pan`.

The math lives in the pure, unit-tested helper `CoreEssentials.Audio.SpatialAudioMath`
(`ComputePan`, `ComputeAttenuation`, `Distance`). If no listener is active, spatialization does
nothing for that frame.

```csharp
var source = owner.AddComponent(new AudioSourceComponent
{
    SoundAsset = "Audio/footstep1_sound.xml",
    SpatialEnabled = true,
    MinDistance   = 50f,
    MaxDistance   = 500f
});
```

---

## AudioListenerComponent

The audience for a scene — the spirit of Unity's `AudioListener`. Attach it to **exactly one**
entity per scene (conventionally the camera host). It owns the global master volume and provides
the world position that opt-in spatial sources pan and attenuate against.

### Parameters / Members

| Member | Type | Description |
| --- | --- | --- |
| `MasterVolume` | `float` | The global master volume (0.0–1.0), applied through the `AudioManager`. Setting it updates every active instance immediately. Defaults to `1f`. |
| `ListenerPosition` | `Vector2` | The listener's current world position — tracks the owner's position so moving the camera moves the audience with it. Read-only. |
| `ActiveListener` | `static AudioListenerComponent?` | The currently active listener, or null when none is attached. Spatial sources read this each frame. |

### Single-active-listener rule

On attach the component registers itself in the static `ActiveListener` slot and clears it on
detach. If a **second** listener attaches while one is already active, it logs a warning and does
**not** steal the slot — keeping the "exactly one audience" rule explicit rather than accidental.

### Code usage

```csharp
var camera = new CameraEntity();
camera.AddComponent(new AudioListenerComponent { MasterVolume = 0.9f });

// Later, from anywhere:
AudioListenerComponent.ActiveListener.MasterVolume = 0.5f;   // duck everything
Vector2 whereAmIListening = AudioListenerComponent.ActiveListener.ListenerPosition;
```

### XML usage

```xml
<Entity Name="Camera" Type="CameraEntity">
  <Component Type="AudioListenerComponent">
    <Properties>
      <Property Name="MasterVolume" Value="0.9" />
    </Properties>
  </Component>
</Entity>
```

---

## Volume Control Summary

A single sample's output is clamped to `0.0–1.0` and equals:

```
clip.Volume × instanceVolume × channelVolume × masterVolume
```

- `masterVolume` — set via `AudioManager.SetMasterVolume` or the active `AudioListenerComponent.MasterVolume`.
- `channelVolume` — per `AudioChannel`, via `AudioManager.Get/SetChannelVolume`.
- `instanceVolume` — per playing instance, via `AudioManager.SetInstanceVolume` (or an `AudioSourceComponent.Volume`).

Pitch and pan are applied independently of the volume chain. See [Audio System](./AudioSystem.md)
for the full `AudioManager` API.
