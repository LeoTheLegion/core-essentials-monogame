# Scene-Persistent Entities

By default, every scene is self-contained: when `SceneManager.LoadScene(...)` transitions away from a scene, the scene's game systems are disposed and all of its entities are destroyed. This is the correct behavior for gameplay content, but it makes it awkward to keep *stateful* things alive across scenes — the classic example being background music that should continue playing (or at least not restart) when the player moves from one level to the next.

Scene-persistent entities solve this: any top-level entity tagged `persist` is carried across a scene transition and re-adopted into the incoming scene, so it survives as the **same live instance**. Everything else in the outgoing scene is torn down normally.

## How It Works

When a transition begins, the framework:

1. **Carries** — before the outgoing scene is unloaded, every top-level entity that carries the `persist` tag is detached from its `EntitySystem` and staged. The whole subtree of such an entity (its children) travels with it.
2. **Adopts** — after the incoming scene has loaded its own systems, the carried entities are re-adopted into the incoming scene's first `EntitySystem`.

Because the entities are moved rather than destroyed and recreated, their state is preserved across the transition:

- The entity instance is unchanged (reference identity is kept).
- One-time lifecycle hooks (`OnAwake` / `OnStart`) are **not** re-fired.
- Component state survives — for example a looping audio source keeps playing, because the underlying playback lives in the global `AudioManager`, which itself is not touched by scene unload.

If the incoming scene has no `EntitySystem` to host the carried entities, they are released (destroyed) so nothing is leaked or left driving behind the scenes.

## Marking an Entity Persistent

Add a `persist` tag to the entity in your scene XML:

```xml
<Scene>
  <GameSystems>
    <System Type="EntitySystem">
      <Entities>
        <EntityDefinition Type="MusicEntity" Id="backgroundMusic">
          <Tags>
            <Tag Name="persist" />
          </Tags>
          <Components>
            <Component Type="AudioSourceComponent">
              <!-- flat overrides, e.g. the clip to loop -->
            </Component>
          </Components>
        </EntityDefinition>
      </Entities>
    </System>
  </GameSystems>
</Scene>
```

You can also apply the tag at runtime with `entity.SetTag("persist")` on any top-level entity. Only **top-level** entities are carried; children of a persistent root travel with that root automatically, and a child is never carried on its own.

## Example: Persistent Background Music

The motivating use case is music that should not stop or restart across scene changes. Model the music as an entity (with an `AudioSourceComponent`) in each scene, tagged `persist`, and let each scene optionally retune it:

```csharp
// A looping music source declared entirely from data — no per-scene C# required:
//   <EntityDefinition Type="MusicEntity" Id="backgroundMusic">
//     <Tags><Tag Name="persist" /></Tags>
//     <Components>
//       <Component Type="AudioSourceComponent">
//         <Properties>
//           <Property Name="SoundAsset" Value="Audio/background_music.xml" />
//           <Property Name="Loop" Value="true" />
//           <Property Name="Channel" Value="Music" />
//         </Properties>
//       </Component>
//     </Components>
//   </EntityDefinition>

public class MusicEntity : Entity
{
    // No OnStart/OnAwake work needed: with PlayOnAttach (the default) and Loop=true the
    // source starts playing on attach. Because this entity is tagged persist, that running
    // loop survives LoadScene transitions — it is not stopped when its scene unloads and is
    // not restarted when it is re-adopted into the next scene.
}
```

Because the entity is carried rather than recreated, the loop has no gap or restart between scenes, and no app-level static shim (a manually-managed "MusicManager") is required.

**Per-scene retuning.** If a later scene wants a *different* track, the cleanest approach is to declare that scene's own `persist`-tagged music entity with its own `SoundAsset`. On transition, the incoming scene's fresh entity and the carried one would both exist — so in practice you keep a single persistent source and change its clip by stopping and restarting it:

```csharp
var audio = musicEntity.GetComponent<AudioSourceComponent>();
audio.Stop();
audio.SoundAsset = "Audio/boss_theme.xml"; // resolved on the next Play()
audio.Play();                              // starts the new clip (this does restart)
```

The seamless part — no gap, no restart, preserved volume/pitch/pan/loop state — applies to keeping the *same* running instance across scenes. Switching to a different clip necessarily restarts that clip.

## Behavior Summary

| Situation | Result |
| --- | --- |
| Top-level entity tagged `persist` | Carried across the transition; same instance in the new scene; keeps updating |
| Child of a persistent root | Travels with the root; same instance in the new scene |
| Entity **not** tagged `persist` | Destroyed when its scene is unloaded (normal behavior) |
| Incoming scene has no `EntitySystem` | Carried entities are released so nothing leaks |
| Adoption | Does not re-fire `OnAwake` / `OnStart`; component state is preserved |

## API Reference

The carry/adopt behavior is driven entirely by the `persist` tag and requires no new public API on your side. The supporting primitives added to support it are:

### EntitySystem.DetachEntity(Entity root)

Removes an entity (and its whole subtree) from this system's managed list and all lookup indexes, and clears each entity's back-reference to the system. Does **not** fire lifecycle hooks, so state is preserved. Used internally when carrying entities out of a scene being left.

### EntitySystem.AdoptEntity(Entity root)

Re-registers an entity (and its whole subtree) with this system: sets the back-reference, ensures unique IDs, assigns sort sequences, and updates all lookup indexes. Does **not** fire lifecycle hooks. Used internally when re-adopting carried entities into a scene being entered.

### Entity.ClearGameSystem()

Clears the entity's back-reference to its owning `EntitySystem` (used as part of detach).

## Caveats

- `persist` is a **reserved framework tag name**. The scene-transition logic reads this tag to decide what to carry, so avoid reusing it for your own gameplay queries — an entity you tag `persist` for unrelated reasons will be carried across scenes.
- Only **top-level** entities are carried. To persist a group, tag the root and let its children travel with it.
- Persistence is scoped to scene transitions driven by `SceneManager`. Entities created after a transition completes belong to that scene normally and will not persist unless tagged.
- Because adoption reuses the incoming scene's first `EntitySystem`, if a scene hosts multiple entity systems, carried entities land in the first one (by registration order).
