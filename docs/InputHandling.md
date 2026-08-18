# Input Handling

The Input Handling system in CoreEssentials-MonoGame provides a simplified interface for detecting and responding to user input from keyboard, mouse, and gamepads. It abstracts MonoGame's input mechanisms, offering both event-driven and polling methods for easier integration with your game logic.

## Key Components

### Input Manager

The `Input` class is the central static access point for all input handling:

```csharp
// Access input devices
var keyboard = Input.Keyboard; // CoreEssentials.Inputs.Keyboard
var mouse = Input.Mouse;     // MonoGame.Extended.Input.InputListeners.MouseListener
var touch = Input.Touch;     // MonoGame.Extended.Input.InputListeners.TouchListener
// (No gamepad handler — use Microsoft.Xna.Framework.Input.GamePad directly.)
```

### Keyboard Input (`CoreEssentials.Inputs.Keyboard`)

The `CoreEssentials.Inputs.Keyboard` class wraps `MonoGame.Extended.Input.InputListeners.KeyboardListener` to provide enhanced polling capabilities and a testable interface.

#### Polling Key States

For continuous actions like movement, polling methods are preferred. These should typically be called within an `Update` loop.

```csharp
// In your entity's Update method
public override void Update(GameTime gameTime)
{
    // Check if a key is currently held down
    if (Input.Keyboard.IsKeyDown(Keys.Space))
    {
        // Player jumps or action continues
    }

    // Check if a key was pressed once in this frame
    if (Input.Keyboard.IsKeyPressedOnce(Keys.P))
    {
        // Pause the game (action on initial press)
    }

    // Check if a key is currently up (not pressed)
    if (Input.Keyboard.IsKeyUp(Keys.LeftShift))
    {
        // Player is not sprinting
    }

    // Check if a key was released in this frame
    if (Input.Keyboard.IsKeyReleasedOnce(Keys.Enter))
    { 
        // Confirm selection (action on release)
    }
}
```

**Important:** For the polling methods (`IsKeyDown`, `IsKeyUp`, `IsKeyPressedOnce`, `IsKeyReleasedOnce`) to work correctly, `Input.Update(gameTime)` must be called once per frame, typically in your main game loop. This, in turn, calls `Input.Keyboard.Update(gameTime)`, which updates the internal previous and current keyboard states.

#### Event-Based Key Input

For discrete actions that should happen once per press or release, you can subscribe to events. These are forwarded from the underlying `MonoGame.Extended.Input.InputListeners.KeyboardListener`.

```csharp
// Subscribe to key events (e.g., in an OnStart or Initialize method)
Input.Keyboard.KeyPressed += OnKeyPressed;
Input.Keyboard.KeyReleased += OnKeyReleased;

// Event handler examples
private void OnKeyPressed(object sender, KeyboardEventArgs args)
{
    // This event fires repeatedly while a key is held down.
    // For single-press logic, consider IsKeyPressedOnce in Update or use KeyReleased.
    if (args.Key == Keys.F)
    {
        // Respond to F key press (e.g., fire weapon, interact)
    }
}

private void OnKeyReleased(object sender, KeyboardEventArgs args)
{
    if (args.Key == Keys.Escape)
    {
        // Respond to escape key release (e.g., open menu)
    }
}

// Remember to unsubscribe from events when the object is destroyed or scene unloads
// e.g., in OnDestroy or Unload method
// Input.Keyboard.KeyPressed -= OnKeyPressed;
// Input.Keyboard.KeyReleased -= OnKeyReleased;
```

### Mouse Input (`MonoGame.Extended.Input.InputListeners.MouseListener`)

Handle mouse events and check mouse states:

```csharp
// Get current mouse position
Vector2 mousePosition = Input.Mouse.Position;

// Check if mouse buttons are pressed
bool isLeftButtonDown = Input.Mouse.IsButtonDown(MouseButton.Left);
bool isRightButtonDown = Input.Mouse.IsButtonDown(MouseButton.Right);

// Subscribe to mouse events
Input.Mouse.ButtonPressed += OnMouseButtonPressed;
Input.Mouse.ButtonReleased += OnMouseButtonReleased;
Input.Mouse.Moved += OnMouseMoved;
Input.Mouse.Scrolled += OnMouseScrolled;

// Event handler examples
private void OnMouseButtonPressed(object sender, MouseButtonEventArgs args)
{
    if (args.Button == MouseButton.Left)
    {
        // Respond to left click at args.Position
    }
}

private void OnMouseMoved(object sender, MouseEventArgs args)
{
    // Mouse moved to args.Position
    // args.Delta contains movement amount since last frame
}
```

### Gamepad Input

> **Note:** The `Input` system currently exposes **keyboard**, **mouse**, and **touch**
> handlers only — there is no `Input.Gamepad` API yet. For controller support, use
> MonoGame's built-in `Microsoft.Xna.Framework.Input.GamePad` directly, e.g.:
>
> ```csharp
> // Check if a gamepad is connected
> bool isConnected = GamePad.GetState(PlayerIndex.One).IsConnected;
>
> // Get thumbstick values (-1.0 to 1.0 in each axis)
> Vector2 leftStick = GamePad.GetState(PlayerIndex.One).ThumbSticks.Left;
>
> // Check button states
> bool isAButtonDown = GamePad.GetState(PlayerIndex.One).Buttons.A == ButtonState.Pressed;
> ```

## Event-Based Input Handling

The system provides an event-driven model for responding to input changes:

```csharp
// Example of adding event handlers in a Scene class
protected override IEnumerator OnStartCoroutine()
{
    // Register input handlers
    Input.Keyboard.KeyReleased += HandleKeyRelease();
    Input.Mouse.ButtonPressed += HandleMouseClick();
    
    yield return null;
}

// Remember to unregister event handlers to prevent memory leaks
public override void Unload()
{
    base.Unload();
    Input.Keyboard.KeyReleased -= HandleKeyRelease();
    Input.Mouse.ButtonPressed -= HandleMouseClick();
}

// Using anonymous methods as event handlers
private EventHandler<KeyboardEventArgs> HandleKeyRelease()
{
    return (sender, args) =>
    {
        if (args.Key == Keys.Space)
        {
            // Handle space key release
        }
    };
}
```

## Example from Playground

The CharacterScene demonstrates keyboard input handling:

```csharp
protected override IEnumerator OnStartCoroutine()
{
    // Register input handler for scene transitions
    Input.Keyboard.KeyReleased += Reset();
    Input.Keyboard.KeyReleased += PlaySound();
    
    yield return null;
}

private EventHandler<KeyboardEventArgs> Reset()
{
    return (sender, args) =>
    {
        if (args.Key == Keys.Right)
        {
            // Switch to a different scene when right arrow is pressed
            AudioManager.Instance.StopSound(songID);
            SceneManager.LoadScene(new PhysicsEntityScene());
        }
    };
}

private EventHandler<KeyboardEventArgs> PlaySound()
{
    return (sender, args) =>
    {
        if (args.Key == Keys.Q)
        {
            // Play a sound effect when Q is pressed
            var id = AudioManager.Instance.PlayOneShotSound("footstep1_sound.xml");
            Console.WriteLine($"Sound played with ID: {id}");
        }
        
        // Additional key handling...
    };
}
```

## Best Practices

- Always unregister input event handlers when scenes are unloaded
- Consider using InputAction abstractions for more complex input mappings
- Separate input handling from game logic for cleaner code organization
- Handle multiple input methods (keyboard, mouse, gamepad) for better accessibility
- Use polling (IsKeyDown) for continuous input like movement
- Use events for discrete input like shooting or jumping
- Consider debouncing rapidly fired input events when appropriate
- Implement input buffering for action games requiring precise timing