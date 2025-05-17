# GUI System

CoreEssentials-MonoGame includes a GUI system built on top of Myra, a UI library for MonoGame. This system allows you to create interactive user interfaces with buttons, panels, labels, and more.

## Key Components

### GUIManager

The `GUIManager` class manages the UI rendering and interaction:

```csharp
// Get the GUIManager from a scene
GUIManager guiManager = GetGameSystem<GUIManager>();

// Create and show a desktop
Desktop desktop = new Desktop();
guiManager.SetDesktop(desktop);
```

### Creating UI Elements

You can create UI elements using Myra's component system:

```csharp
// Create a panel
var panel = new Panel
{
    Width = 200,
    Height = 150,
    Background = new SolidBrush(Color.DarkBlue)
};

// Add a label
var label = new Label
{
    Text = "Score: 0",
    HorizontalAlignment = HorizontalAlignment.Center,
    VerticalAlignment = VerticalAlignment.Top,
    Margin = new Thickness(0, 10, 0, 0)
};
panel.Widgets.Add(label);

// Add a button
var button = new TextButton
{
    Text = "Click Me",
    Width = 100,
    HorizontalAlignment = HorizontalAlignment.Center,
    VerticalAlignment = VerticalAlignment.Bottom,
    Margin = new Thickness(0, 0, 0, 10)
};
button.Click += (s, a) => 
{
    label.Text = "Button clicked!";
};
panel.Widgets.Add(button);

// Add the panel to a desktop and show it
Desktop desktop = new Desktop();
desktop.Root = panel;
guiManager.SetDesktop(desktop);
```

## Creating UI with XML

Myra supports defining UI layouts in XML, which can be loaded at runtime:

```csharp
// Load UI from XML
var uiXml = @"
<Panel Width=""200"" Height=""150"" Background=""#FF00008B"">
    <Label Id=""scoreLabel"" Text=""Score: 0"" HorizontalAlignment=""Center"" VerticalAlignment=""Top"" Margin=""0, 10, 0, 0""/>
    <TextButton Id=""clickButton"" Text=""Click Me"" Width=""100"" HorizontalAlignment=""Center"" VerticalAlignment=""Bottom"" Margin=""0, 0, 0, 10""/>
</Panel>";

var project = Project.LoadFromXml(uiXml);
var panel = project.Root as Panel;
var scoreLabel = panel.FindWidgetById("scoreLabel") as Label;
var clickButton = panel.FindWidgetById("clickButton") as TextButton;

clickButton.Click += (s, a) =>
{
    scoreLabel.Text = "Button clicked!";
};

Desktop desktop = new Desktop();
desktop.Root = panel;
guiManager.SetDesktop(desktop);
```

## UI Components

Myra provides a wide range of UI components:

- **Containers**: Panel, HorizontalStackPanel, VerticalStackPanel, Grid
- **Basic Widgets**: Label, Image, TextButton, ImageButton
- **Input Widgets**: TextField, SpinButton, ComboBox, CheckBox
- **Advanced Widgets**: ScrollPane, Window, TabControl, ProgressBar
- **Dialogs**: FileDialog, MessageBox

## Using the Canvas System

The `Canvas` class provides a convenient way to manage a group of UI components that can be positioned together. It acts as a container for Myra UI widgets, allowing them to be treated as a cohesive unit.

### Creating a Canvas

You can create a Canvas instance and add widgets to it:

```csharp
// Create a new canvas
Canvas canvas = new Canvas();

// Set the canvas position (in screen coordinates)
// This immediately updates the position of all contained widgets
canvas.SetPosition(new Vector2(100, 50));

// Add widgets to the canvas
var label = new Label { Text = "Hello World" };
canvas.AddWidget(label);

var button = new Button();
button.Content = "Click Me";
canvas.AddWidget(button);
```

### Managing Canvas Content

The Canvas provides methods to add, remove, and manage widgets:

```csharp
// Add a widget to the canvas
canvas.AddWidget(new Label { Text = "New Label" });

// Remove a specific widget
canvas.RemoveWidget(label);

// Update the canvas (typically called in your game's Update method)
canvas.Update(gameTime);

// Clean up resources when done
canvas.CleanUp();
```

### Canvas Features

- **Positioning**: Set the position of the entire canvas and all its contained widgets immediately
- **Widget Management**: Add, remove, and manage multiple widgets as a group
- **Automatic Integration**: Seamlessly works with the GUIManager system
- **Clean Resource Management**: Properly disposes of resources when no longer needed

### Example Usage

```csharp
// In your scene's LoadContent method
private Canvas _hudCanvas;

public override void LoadContent()
{
    base.LoadContent();
    
    // Create HUD canvas
    _hudCanvas = new Canvas();
    _hudCanvas.SetPosition(new Vector2(10, 10));
    
    // Add score display
    var scoreLabel = new Label { Text = "Score: 0" };
    _hudCanvas.AddWidget(scoreLabel);
    
    // Add health bar
    var healthBar = new ProgressBar { 
        Width = 200, 
        Height = 20,
        Value = 100,
        Minimum = 0,
        Maximum = 100
    };
    healthBar.Top = 30;
    _hudCanvas.AddWidget(healthBar);
}

// In your scene's Update method
public override void Update(GameTime gameTime)
{
    base.Update(gameTime);
    
    // Update canvas position if needed
    _hudCanvas.Update(gameTime);
}

// When cleaning up
public override void UnloadContent()
{    _hudCanvas.CleanUp();
    base.UnloadContent();
}
```

## Testing GUI Components

Testing GUI components can be challenging due to their dependencies on the graphics system and Myra environment. The CoreEssentials test suite provides examples of how to test GUI components effectively.

### Testing Canvas with xUnit

Here's how the Canvas class is tested:

```csharp
public class CanvasTests : IDisposable
{
    private readonly Game _mockGame;

    public CanvasTests()
    {
        // Create a real Game instance for testing
        _mockGame = new Game1();
        
        // Set up Myra environment
        MyraEnvironment.Game = _mockGame;
    }
    
    void IDisposable.Dispose()
    {
        // Clean up resources
        _mockGame?.Dispose();
    }
    
    // Helper method to initialize GUIManager before tests
    private void InitializeGUIManager()
    {
        // Initialize GUIManager with real Game instance
        GUIManager.Init(_mockGame, 800, 600);
    }
    
    [Fact]
    public void AddWidget_AddsWidgetToRootPanel()
    {
        // Arrange
        InitializeGUIManager();
        var canvas = new Canvas();
        var widget = new Label();

        // Act
        canvas.AddWidget(widget);

        // Assert
        // Use reflection to access _rootPanel
        var rootPanelField = typeof(Canvas).GetField("_rootPanel", 
            BindingFlags.NonPublic | BindingFlags.Instance);
        var rootPanel = (Panel)rootPanelField.GetValue(canvas);
        
        Assert.Contains(widget, rootPanel.Widgets);
    }
    
    // Additional test methods for other Canvas functionality
}
```

### Key Testing Patterns

When testing GUI components, consider these approaches:

1. **Environment Setup**: Properly initialize the Myra environment with a Game instance
2. **Reflection for Private Members**: Use reflection to access and verify private state
3. **Clean Resource Management**: Implement IDisposable to clean up after tests
4. **Integration Testing**: Test interactions between components (e.g., Canvas and GUIManager)
5. **Component State Verification**: Check that component state changes correctly after operations

For more detailed examples, see the `CanvasTests.cs` and `GUIManagerTests.cs` files in the test project.

### Testing GUIManager

The static `GUIManager` class can be tested using similar approaches:

```csharp
public class GUIManagerTests : IDisposable
{
    private readonly Game _mockGame;

    public GUIManagerTests()
    {
        // Create a real Game instance for testing
        _mockGame = new Game1();
        
        // Set Myra environment before tests
        MyraEnvironment.Game = _mockGame;
    }
    
    void IDisposable.Dispose()
    {
        // Clean up resources
        _mockGame?.Dispose();
    }
    
    [Fact]
    public void AddWidget_AddsWidgetToRootPanel()
    {
        // Arrange
        GUIManager.Init(_mockGame, 800, 600);
        var widget = new Label { Text = "Test Label" };
        
        // Act
        GUIManager.AddWidget(widget);
        
        // Assert
        var rootPanelGetter = typeof(GUIManager).GetProperty("Root", 
            BindingFlags.NonPublic | BindingFlags.Static);
        var rootPanel = (Panel)rootPanelGetter.GetValue(null);
        
        Assert.Contains(widget, rootPanel.Widgets);
    }
    
    // Additional tests for other GUIManager functionality
}
```

```csharp
// Create a form-like interface
var form = new VerticalStackPanel
{
    Width = 300,
    Spacing = 10
};

// Add a title
form.Widgets.Add(new Label
{
    Text = "Player Settings",
    Font = FontSystem.Default.GetFont(20),
    HorizontalAlignment = HorizontalAlignment.Center
});

// Add input fields
var nameField = new TextField { Hint = "Enter your name" };
form.Widgets.Add(nameField);

var difficultyPanel = new HorizontalStackPanel { Spacing = 5 };
difficultyPanel.Widgets.Add(new Label { Text = "Difficulty:" });
var difficultyCombo = new ComboBox();
difficultyCombo.Items.Add(new ListItem { Text = "Easy" });
difficultyCombo.Items.Add(new ListItem { Text = "Normal" });
difficultyCombo.Items.Add(new ListItem { Text = "Hard" });
difficultyCombo.SelectedIndex = 1;
difficultyPanel.Widgets.Add(difficultyCombo);
form.Widgets.Add(difficultyPanel);

// Add a checkbox
var soundCheckBox = new CheckBox { Text = "Enable sound" };
form.Widgets.Add(soundCheckBox);

// Add buttons
var buttonsPanel = new HorizontalStackPanel
{
    Spacing = 10,
    HorizontalAlignment = HorizontalAlignment.Center
};
var saveButton = new TextButton { Text = "Save" };
var cancelButton = new TextButton { Text = "Cancel" };
buttonsPanel.Widgets.Add(saveButton);
buttonsPanel.Widgets.Add(cancelButton);
form.Widgets.Add(buttonsPanel);
```

## Handling UI Updates

Update UI elements in response to game events:

```csharp
// Update a score label
public void UpdateScore(int score)
{
    if (_scoreLabel != null)
    {
        _scoreLabel.Text = $"Score: {score}";
    }
}

// Show a message dialog
public void ShowGameOverDialog(int finalScore)
{
    var messageBox = Dialog.CreateMessageBox("Game Over", $"Your final score: {finalScore}");
    messageBox.ButtonOk.Click += (s, a) => 
    {
        // Return to main menu
        SceneManager.LoadScene(new MainMenuScene());
    };
    
    messageBox.Show(guiManager.Desktop);
}
```

## Best Practices

- Organize UI code into separate methods or classes for maintainability
- Use XML when possible for complex layouts
- Keep UI logic separate from game logic
- Use proper layout containers (StackPanel, Grid) rather than absolute positioning
- Implement responsive layouts that adapt to different screen sizes
- Cache references to frequently updated UI elements
- Set appropriate tab order for keyboard navigation
- Provide visual feedback for interactive elements
- Consider accessibility features (text size, color contrast)
- Implement UI animations for better user experience