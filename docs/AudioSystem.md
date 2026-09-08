# Audio System

The Audio System in CoreEssentials-MonoGame provides a flexible framework for playing and managing sound effects and music in your game. It offers easy-to-use methods for playing one-shot sounds, looping background music, and controlling volume levels.

## Key Components

### AudioManager

The `AudioManager` class is the central component for audio playback and control, implemented as a singleton:

```csharp
// Access the AudioManager instance
AudioManager audioManager = AudioManager.Instance;
```

### Playing Sounds

The system supports both one-shot sound effects and looping background sounds:

```csharp
// Play a one-shot sound effect (fire and forget)
string effectId = AudioManager.Instance.PlayOneShotSound("footstep1_sound.xml");

// Play looping or controlled sounds (returns an ID for later control)
string musicId = AudioManager.Instance.PlaySound("background_music.xml");

// Stop a playing sound by ID
AudioManager.Instance.StopSound(musicId);

// Pause a playing sound
AudioManager.Instance.PauseSound(musicId);

// Resume a paused sound
AudioManager.Instance.ResumeSound(musicId);
```

### Volume Control

Control the global volume applied to all active sounds:

```csharp
// Set the master volume (0.0f to 1.0f) — applies to all currently playing sounds
AudioManager.Instance.SetMasterVolume(0.8f);
```

### Per-Instance Volume, Pitch and Pan

Each playing instance can be tuned independently of its clip, channel and the master volume:

```csharp
string id = AudioManager.Instance.PlaySound("footstep1_sound.xml");

// Per-instance volume multiplier (0.0–1.0), clamped into the final output.
AudioManager.Instance.SetInstanceVolume(id, 0.25f);

// Per-instance pitch as a Unity-style ratio (1.0 = normal, 2.0 = one octave up).
// Converted internally to MonoGame's semitone unit; non-positive ratios are treated as normal.
AudioManager.Instance.SetInstancePitch(id, 2.0f);   // one octave up

// Per-instance stereo pan (-1 = left, 0 = center, +1 = right).
AudioManager.Instance.SetInstancePan(id, 0.75f);    // mostly right
```

### Audio Channels

Every instance plays on an `AudioChannel` (`Master`, `Music` or `Sfx`). Each channel has its own
volume knob, so you can duck the music under dialogue without touching SFX:

```csharp
// Play a clip on a specific channel (defaults to AudioChannel.Master).
string musicId = AudioManager.Instance.PlaySound("background_music.xml", AudioChannel.Music);

AudioManager.Instance.SetChannelVolume(AudioChannel.Music, 0.5f); // duck the music
float m = AudioManager.Instance.GetChannelVolume(AudioChannel.Music);
```

Setting a channel volume re-applies it immediately to every active instance on that channel.

> **Effective volume formula.** A single sample's output is clamped to `0.0–1.0` and equals:
> `clip.Volume × instanceVolume × channelVolume × masterVolume`. Pitch and pan are applied
> independently of the volume chain.

### Audio Assets

The Audio System uses XML files to define sound resources:

```xml
<!-- Example sound effect XML (footstep1_sound.xml) -->
<SoundEffect>
  <File>footstep00.ogg</File>
  <Volume>1.0</Volume>
  <Pitch>0.0</Pitch>
  <Pan>0.0</Pan>
</SoundEffect>

<!-- Example music XML (background_music.xml) -->
<Music>
  <File>Goblins_Den_(Regular).wav</File>
  <Volume>0.7</Volume>
  <Loop>true</Loop>
</Music>
```

## Example from Playground

The CharacterScene demonstrates audio system usage:

```csharp
// Play background music
string songID = AudioManager.Instance.PlaySound("song1_sound.xml");

// Play sound effects on key press
private EventHandler<KeyboardEventArgs> PlaySound()
{
    return (sender, args) =>
    {
        if (args.Key == Keys.Q)
        {
            // Play a sound effect
            var id = AudioManager.Instance.PlayOneShotSound("footstep1_sound.xml");
            Console.WriteLine($"Sound played with ID: {id}");
        }
        
        if (args.Key == Keys.Z)
        {
            // Lower volume
            AudioManager.Instance.SetMasterVolume(0.1f);
            Console.WriteLine("Volume set to 10%");
        }
        
        if (args.Key == Keys.X)
        {
            // Reset volume
            AudioManager.Instance.SetMasterVolume(1.0f);
            Console.WriteLine("Volume set to 100%");
        }
    };
}

// Stop music when transitioning scenes
if (args.Key == Keys.Right)
{
    AudioManager.Instance.StopSound(songID);
    SceneManager.LoadScene(new PhysicsEntityScene());
}
```

## Audio Components (Per-Entity Sources & Listener)

For per-entity audio — a looping music shell on a plain entity, or a sound button declared purely
from XML data — use the built-in `AudioSourceComponent` and `AudioListenerComponent`. These attach
to entities, own their playback state (stopping themselves on detach so a scene unload leaks no
instances), and opt into 2D spatialization. See [Audio Components](./AudioComponents.md).

## Best Practices

- Use descriptive sound IDs for easy management
- Properly clean up and stop sounds during scene transitions
- Use XML files to define sound properties for better organization
- Adjust volume levels for a balanced audio experience
- Use one-shot sounds for brief effects and PlaySound for longer or looping audio
- Use `AudioChannel` (`Music`, `Sfx`) for group volume control — e.g. duck the music without touching SFX
- For per-entity audio (music shells, sound buttons), prefer the built-in components over raw manager calls — see [Audio Components](./AudioComponents.md)
- Enable `SpatialEnabled` on an `AudioSourceComponent` for opt-in 2D positional audio (pan + attenuation against the active listener)