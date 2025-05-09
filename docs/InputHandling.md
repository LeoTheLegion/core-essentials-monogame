# Input Handling

The Input Handling system in CoreEssentials-MonoGame provides a simplified interface for detecting and responding to user input from keyboard, mouse, and gamepads. It abstracts MonoGame's input mechanisms into an event-driven model for easier integration with your game logic.

## Key Components

### Input Manager

The `Input` class is the central access point for all input handling:

```csharp
// Access input devices
var keyboard = Input.Keyboard;
var mouse = Input.Mouse;
var gamepad = Input.Gamepad;
```

### Keyboard Input

Handle keyboard events and check key states:

```csharp
// Check if a key is currently pressed
bool isSpacePressed = Input.Keyboard.IsKeyDown(Keys.Space);

// Subscribe to key events
Input.Keyboard.KeyPressed += OnKeyPressed;
Input.Keyboard.KeyReleased += OnKeyReleased;

// Event handler examples
private void OnKeyPressed(object sender, KeyboardEventArgs args)
{
    if (args.Key == Keys.Space)
    {
        // Respond to space bar press
    }
}

private void OnKeyReleased(object sender, KeyboardEventArgs args)
{
    if (args.Key == Keys.Escape)
    {
        // Respond to escape key release
    }
}
```

### Mouse Input

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

Handle gamepad input for controller support:

```csharp
// Check if a gamepad is connected
bool isConnected = Input.Gamepad.IsConnected(PlayerIndex.One);

// Get thumbstick values (-1.0 to 1.0 in each axis)
Vector2 leftStick = Input.Gamepad.GetLeftStick(PlayerIndex.One);
Vector2 rightStick = Input.Gamepad.GetRightStick(PlayerIndex.One);

// Get trigger values (0.0 to 1.0)
float leftTrigger = Input.Gamepad.GetLeftTrigger(PlayerIndex.One);
float rightTrigger = Input.Gamepad.GetRightTrigger(PlayerIndex.One);

// Check button states
bool isAButtonDown = Input.Gamepad.IsButtonDown(PlayerIndex.One, Buttons.A);

// Subscribe to button events
Input.Gamepad.ButtonPressed += OnGamepadButtonPressed;
Input.Gamepad.ButtonReleased += OnGamepadButtonReleased;
```

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
            Debug.Console.WriteLine($"Sound played with ID: {id}");
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