# GUI System

CoreEssentials-MonoGame provides an abstraction layer over the Myra UI framework, giving you a clean, engine-agnostic API for building user interfaces. All widgets are created through factories and interact via interfaces — concrete implementation types are never exposed to your code.

## Key Components

### WidgetFactory ⭐

All UI elements are created through `WidgetFactory`, which returns interface types (`IButton`, `ILabel`, `IPanel`, etc.). This ensures complete decoupling from the underlying rendering engine.

```csharp
using CoreEssentials.GUI;
using CoreEssentials.GUI.Factory;
using Microsoft.Xna.Framework;

// Create a panel
IPanel panel = WidgetFactory.CreatePanel();
panel.Width = 200f;
panel.Height = 150f;
panel.Background = new SolidColorBrush(Color.DarkBlue);

// Create a label and add it to the panel
ILabel label = WidgetFactory.CreateLabel("Score: 0");
panel.AddChild(label);

// Create a button with an event handler
IButton button = WidgetFactory.CreateTextButton("Click Me");
button.Clicked += (btn) => 
{
    label.Text = "Button clicked!";
};
panel.AddChild(button);

// Add the panel to the GUI root hierarchy
GUIManager.AddWidget(panel);
```

### Canvas 🖼️

The `Canvas` class provides a convenient way to manage groups of UI components that can be positioned together. Canvases support both **screen space** (HUDs, menus) and **world space** (floating labels above entities).

```csharp
// Screen-space canvas (default) — position in absolute screen coordinates
ICanvas hudCanvas = new Canvas();
hudCanvas.SetPosition(new Vector2(100f, 50f));

// Or explicitly specify screen space via factory
ICanvas explicitScreenCanvas = CanvasFactory.CreateScreenSpace();

// World-space canvas — position in game world coordinates (auto-converted via camera)
ICanvas worldCanvas = new Canvas(false); // or: CanvasFactory.CreateWorldSpace()
worldCanvas.SetPosition(new Vector2(500f, 300f)); // World position

// Add widgets to the canvas
ILabel scoreLabel = WidgetFactory.CreateLabel("Score: 10");
hudCanvas.AddWidget(scoreLabel);

IButton menuButton = WidgetFactory.CreateTextButton("Menu");
worldCanvas.AddWidget(menuButton);
```

#### Canvas Lifecycle

```csharp
// Add children (widgets) to the canvas
canvas.AddChild(WidgetFactory.CreateLabel("Hello World"));

// Remove a specific widget from the canvas
canvas.RemoveChild(label);

// Update canvas state each frame (required for world-space camera transforms)
canvas.Update(gameTime);

// Clean up and release resources when done
canvas.CleanUp();
```

### GUIManager — Lifecycle Management

`GUIManager` is a static class that manages initialization, rendering, and the root widget hierarchy. It delegates all operations to the active engine backend (default: Myra-based `GuiManagerImpl`).

```csharp
// Initialize the GUI system once in your game's LoadContent or Initialize
GUIManager.Init(this, 1280, 720); // width=1280, height=720

// Add widgets to the root hierarchy (visible globally)
GUIManager.AddWidget(panel);
GUIManager.RemoveWidget(panel);

// Check focus state
bool anyFocused = GUIManager.IsAnyWidgetFocused();
bool focused = GUIManager.IsWidgetFocused(button);

// Draw all GUI elements each frame in your game's Draw method
GUIManager.Draw(gameTime);
```

## API Reference

| Interface / Class | Namespace | Purpose |
|-------------------|-----------|---------|
| `IGuiManager` | `CoreEssentials.GUI.Types` | Lifecycle, widget management, rendering (engine-level) |
| `ICanvas` | `CoreEssentials.GUI.Types` | Positioned container for widgets in screen/world space |
| `IWidget` | `CoreEssentials.GUI.Types` | Base abstraction for all UI elements |
| `IContainer` | `CoreEssentials.GUI.Types` | Widget containers with child management (`AddChild`, `RemoveChild`) |
| `IPanel` | `CoreEssentials.GUI.Types` | Container with styling (`Background`, `BorderThickness`) |
| `ILabel` | `CoreEssentials.GUI.Types` | Text display (`Text`, `Font`, `TextColor`) |
| `IButton` | `CoreEssentials.GUI.Types` | Clickable element (`Text`, `Clicked` event) |
| `IGrid` | `CoreEssentials.GUI.Types` | Tabular layout with rows/columns, spacing, proportions |
| `IBrush` | `CoreEssentials.GUI.Types` | Background/styling abstraction (`Color`, `Opacity`) |
| `WidgetFactory` | `CoreEssentials.GUI.Factory` | Static factory methods returning interface instances |
| `CanvasFactory` | `CoreEssentials.GUI.Factory` | Creates screen-space or world-space canvases |
| `GUIManager` | `CoreEssentials.GUI` | Static facade for GUI lifecycle and root widget management |

## Complete Example — HUD Layout

```csharp
using CoreEssentials;
using CoreEssentials.GUI;
using CoreEssentials.GUI.Factory;
using Microsoft.Xna.Framework;
using SceneManagement;

public class GameHudScene : Scene
{
    private ICanvas _hudCanvas;
    private ILabel _scoreLabel;

    public override void LoadContent()
    {
        base.LoadContent();

        // Initialize GUI system if not already done
        GUIManager.Init(this.Game, 1280, 720);

        // Create HUD canvas in screen space
        _hudCanvas = CanvasFactory.CreateScreenSpace();
        _hudCanvas.SetPosition(new Vector2(20f, 20f));
        _hudCanvas.Background = new SolidColorBrush(new Color(50, 50, 50, 150));

        // Create score label
        _scoreLabel = WidgetFactory.CreateLabel("Score: 0");
        _scoreLabel.TextColor = Color.White;
        _hudCanvas.AddChild(_scoreLabel);

        // Create a grid for settings row
        IGrid settingsGrid = WidgetFactory.CreateGrid();
        settingsGrid.RowSpacing = 5f;
        settingsGrid.ColumnSpacing = 10f;

        ILabel healthLabel = WidgetFactory.CreateLabel("Health:");
        ILabel ammoLabel = WidgetFactory.CreateLabel("Ammo:");
        
        settingsGrid.AddChild(healthLabel);
        settingsGrid.SetColumn(healthLabel, 0);
        settingsGrid.AddChild(ammoLabel);
        settingsGrid.SetColumn(ammoLabel, 1);

        _hudCanvas.AddChild(settingsGrid);

        // Add canvas to the GUI root hierarchy
        GUIManager.AddWidget(_hudCanvas);
    }

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);

        // Update canvas for any camera transforms (screen-space is a no-op)
        _hudCanvas?.Update(gameTime);
    }

    // Example: update score from game logic
    public void OnScoreChanged(int newScore)
    {
        if (_scoreLabel != null)
            _scoreLabel.Text = $"Score: {newScore}";
    }

    public override void UnloadContent()
    {
        base.UnloadContent();
        
        // Clean up canvas resources
        _hudCanvas?.CleanUp();
    }
}
```

## Advanced: Swapping GUI Engines 🔮

The GUI system is designed to be engine-swappable. By default, `EngineResolver` returns the Myra-based `GuiManagerImpl`, but you can register a custom implementation at runtime:

```csharp
using CoreEssentials.GUI.Internal;

// Register a custom GUI engine (e.g., your own rendering backend)
EngineResolver.SetEngine(new CustomGuiEngine());

// Or restore the default Myra engine
EngineResolver.SetEngine(new GuiManagerImpl());
```

## XML Layout Support 📄

CoreEssentials provides a `GuiSerializer` class for parsing UI layouts from XML strings or asset files. This enables data-driven UI design while maintaining our abstraction layer — all widgets are returned as interface types, with no Myra types exposed.

### String-Based Loading (Inline XML)

Define your layout directly in code using an XML string:

```csharp
using CoreEssentials.GUI;
using CoreEssentials.GUI.Types;
using Microsoft.Xna.Framework;

// Define UI layout as a string
string inlineXml = @"
<Panel Width=""400"" Height=""300"" X=""100"" Y=""100"">
    <Label Text=""Score: 0"" Width=""300"" X=""50"" Y=""20"" TextColor=""Yellow"" />
    <Button Text=""Reset Score"" Width=""150"" Height=""40"" X=""125"" Y=""200"" />
</Panel>";

// Parse the XML and get an IPanel (interface type)
IPanel panel = GuiSerializer.LoadPanelFromXml(inlineXml, contentManager);

// Find widgets by iterating children
IButton resetButton = panel.Children.OfType<IButton>().FirstOrDefault();
if (resetButton != null)
{
    resetButton.Clicked += (_) => 
    {
        var label = panel.Children.OfType<ILabel>().FirstOrDefault();
        if (label != null) label.Text = "Score: 0";
    };
}

// Add to the GUI root hierarchy
GUIManager.AddWidget(panel);
```

### Asset-Based Loading (XML File)

Store your layout in a `.xml` file under the `Content/` directory and load it at runtime:

1. Create an XML file, e.g., `Content/layout/main.xml`:
   ```xml
   <Panel Width="300" Height="200">
       <Label Text="Asset Loaded Panel" X="20" Y="20" />
       <Button Text="Load Asset" X="50" Y="80" />
   </Panel>
   ```

2. Load the XML asset and parse it:
   ```csharp
   using CoreEssentials.Assets;
   
   // Create an XMLAsset pointing to your file path
   var xmlAsset = new XMLAsset("layout/main.xml");
   xmlAsset.Load(contentManager);  // Reads directly from disk
   
   // Parse the asset content into interface types
   IPanel panel = GuiSerializer.LoadPanelFromXml(xmlAsset, contentManager);
   
   GUIManager.AddWidget(panel);
   ```

### Supported Widget Types

| XML Element | Interface Type | Key Properties |
|-------------|----------------|----------------|
| `<Label>` | `ILabel` | Text, Width, Height, X, Y, TextColor |
| `<Button>` | `IButton` | Text, Width, Height, X, Y |
| `<Panel>` | `IPanel` | Width, Height, X, Y, BorderThickness |
| `<Grid>` | `IGrid` | RowSpacing, ColumnSpacing, children |

### XML Attribute Conventions

- **Property mapping**: XML attribute names map 1:1 to interface property names (case-insensitive). Position attributes use `X` and `Y`.
- **Child recursion**: Container elements (`<Panel>`, `<Grid>`) automatically parse nested child widgets.
- **Optional Id**: Use the `Id` attribute for later identification, though ID-based lookup is not yet implemented in `GuiSerializer`.

### Event Handling Pattern

XML cannot directly wire up event handlers. The recommended approach is to parse the layout first, then attach handlers via code:

```csharp
var panel = GuiSerializer.LoadPanelFromXml(xmlString, contentManager);

// Find button by type/position and attach handler
IButton myButton = panel.Children.OfType<IButton>().FirstOrDefault();
if (myButton != null)
{
    myButton.Clicked += (_) => 
    {
        // Handle click
    };
}

GUIManager.AddWidget(panel);
```

### Content Manager Integration

`GuiSerializer` accepts an optional `IContentManager` for resolving asset paths (e.g., fonts, brushes). If not provided, these properties will be null — acceptable for simple layouts but should be documented if your design relies on them.

> **Note: XML files use `/copy:` in Content.mgcb**
> 
> Unlike textures or font files that are processed by MonoGame's content pipeline (using `/build:`), raw data files like `.xml`, `.json`, and `.csv` should be configured with `/copy:` in `Content.mgcb`. This tells the pipeline to simply copy the file as-is to the output directory without any transformation.
> 
> Example entry in your project's `Content/Content.mgcb`:
> ```
> #begin layout/main.xml
> /copy:layout/main.xml
> ```

---

### Real-world Example: Debug Overlay with StickyLog ⭐

The `StickyLog` class in the `CoreEssentials.Debugging` namespace is a real-world example of XML layouts powering runtime UI. It displays an on-screen debug overlay that can show key-value pairs (FPS, entity counts, game state) during development.

**Before Sprint 7** — imperative setup (~15 lines):
```csharp
_grid = WidgetFactory.CreateGrid();
_grid.RowSpacing = 8; _grid.ColumnSpacing = 8;
_grid.Background = Color.Black.WithAlpha(100).AsBrush();
_grid.Width = 300; _grid.Height = 100;
_grid.Visible = true;
```

**After Sprint 7** — declarative XML layout (3 lines):
```csharp
// Grid structure loaded from embedded resource: CoreEssentials.Content.StickyLogLayout.xml
_grid = GuiSerializer.LoadGridFromXmlEmbedded("CoreEssentials.Content.StickyLogLayout.xml");
_grid.Background = Color.Black.WithAlpha(100).AsBrush(); // opacity still set imperatively (Sprint 8 pending)
```

The XML layout file (`StickyLogLayout.xml`) defines the static container structure — dimensions, spacing, visibility. Runtime-dynamic elements (individual log entries added via `CreateNewLabel()`) remain imperative, which is correct: **XML for static structure, code for dynamic content**.

See `docs/gui_refactor_sprints/Sprint_7_StickyLog_XML_Refactor.md` for the full migration details.

---

## UI Components

The abstraction layer supports a wide range of UI components via the factory:

- **Containers**: Panel, StackPanel, Grid
- **Basic Widgets**: Label, Button
- **Input Widgets**: TextField (via `WidgetFactory`)
- **Advanced Widgets**: ProgressBar, ComboBox (via `WidgetFactory`)

## Using the Canvas System

The `Canvas` class provides a convenient way to manage a group of UI components that can be positioned together. It acts as a container for widgets, allowing them to be treated as a cohesive unit. Canvas can operate in either screen space (default) or world space.

### Creating a Canvas

```csharp
using CoreEssentials.GUI;
using CoreEssentials.GUI.Factory;
using CoreEssentials.GUI.Types;

// Create a new canvas in screen space (default)
ICanvas screenCanvas = CanvasFactory.CreateScreenSpace();
screenCanvas.SetPosition(new Vector2(100, 50));

// Or create a world-space canvas for floating labels above entities
ICanvas worldCanvas = CanvasFactory.CreateWorldSpace();
worldCanvas.SetPosition(new Vector2(500, 300)); // World position

// Add widgets to the canvas
ILabel label = WidgetFactory.CreateLabel("Hello World");
screenCanvas.AddChild(label);

IButton button = WidgetFactory.CreateTextButton("Click Me");
worldCanvas.AddChild(button);
```

### Managing Canvas Content

The canvas provides methods to add, remove, and manage widgets:

```csharp
// Add a widget to the canvas
canvas.AddChild(WidgetFactory.CreateLabel("New Label"));

// Remove a specific widget
canvas.RemoveChild(label);

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
private ICanvas _hudCanvas;

public override void LoadContent()
{
    base.LoadContent();
    
    // Create HUD canvas
    _hudCanvas = CanvasFactory.CreateScreenSpace();
    _hudCanvas.SetPosition(new Vector2(10, 10));
    
    // Add score display
    ILabel scoreLabel = WidgetFactory.CreateLabel("Score: 0");
    _hudCanvas.AddChild(scoreLabel);
    
    // Add health bar
    IProgressBar healthBar = WidgetFactory.CreateProgressBar();
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