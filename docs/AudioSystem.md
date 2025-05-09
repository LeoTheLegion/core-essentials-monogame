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

Control volume levels globally or for individual sounds:

```csharp
// Set the master volume (0.0f to 1.0f)
AudioManager.Instance.SetMasterVolume(0.8f);

// Set the volume for a specific sound
AudioManager.Instance.SetSoundVolume(soundId, 0.5f);
```

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

## Audio Clip Instance

For more advanced audio control, you can work directly with `AudioClipInstance` objects:

```csharp
// Get an AudioClipInstance
AudioClipInstance instance = AudioManager.Instance.GetAudioInstance(soundId);

if (instance != null)
{
    // Check if the instance is still playing
    bool isPlaying = instance.IsPlaying;
    
    // Get or set the current playback position
    float position = instance.Position;
    instance.Position = 5.0f; // Jump to 5 seconds
    
    // Get the total duration
    float duration = instance.Duration;
}
```

## Best Practices

- Use descriptive sound IDs for easy management
- Properly clean up and stop sounds during scene transitions
- Use XML files to define sound properties for better organization
- Adjust volume levels for a balanced audio experience
- Use one-shot sounds for brief effects and PlaySound for longer or looping audio
- Consider using spatial audio for positional sound in 2D games
- Implement sound categories (music, sfx, ui, etc.) for group volume control