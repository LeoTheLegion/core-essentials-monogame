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