# Entity Debug Visualization

The debug visualization system provides visual overlays for entities, making it easier to debug positioning, hierarchy, tags, and bounds during development.

## Overview

Debug visualization is controlled through the `EntitySystem.DebugMode` property and configured via `EntitySystem.DebugConfig`. When enabled, overlays are automatically rendered after entity drawing.

## Enabling Debug Mode

```csharp
// Enable debug mode
entitySystem.DebugMode = true;

// Optionally configure which overlays to show
entitySystem.DebugConfig.ShowEntityBounds = true;
entitySystem.DebugConfig.ShowEntityIds = true;
entitySystem.DebugConfig.ShowEntityTags = true;
entitySystem.DebugConfig.ShowEntityHierarchy = true;
entitySystem.DebugConfig.ShowEntityPosition = true;
```

## Debug Overlays

### Entity Bounds
Draws a bounding box around each entity based on its position and scale.

```csharp
entitySystem.DebugConfig.ShowEntityBounds = true;
entitySystem.DebugConfig.BoundsColor = Color.Lime; // Default
```

### Entity IDs
Displays the entity's unique ID above its position.

```csharp
entitySystem.DebugConfig.ShowEntityIds = true;
entitySystem.DebugConfig.IdColor = Color.Yellow; // Default
```

**Note:** Requires a debug font to be set:
```csharp
var fontAsset = new FontAsset("DebugFont");
fontAsset.Load(contentManager);
entitySystem.DebugFont = fontAsset;
```

### Entity Tags
Displays the entity's tags below its position.

```csharp
entitySystem.DebugConfig.ShowEntityTags = true;
entitySystem.DebugConfig.TagColor = Color.Cyan; // Default
```

### Parent-Child Hierarchy
Draws lines connecting parent entities to their children.

```csharp
entitySystem.DebugConfig.ShowEntityHierarchy = true;
entitySystem.DebugConfig.HierarchyColor = Color.Magenta; // Default
```

### Position Markers
Draws a small crosshair at each entity's position.

```csharp
entitySystem.DebugConfig.ShowEntityPosition = true;
entitySystem.DebugConfig.PositionColor = Color.Red; // Default
```

## Configuration Options

The `DebugConfig` class provides the following properties:

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `ShowEntityBounds` | `bool` | `false` | Draw bounding boxes |
| `ShowEntityIds` | `bool` | `false` | Display entity IDs |
| `ShowEntityTags` | `bool` | `false` | Display entity tags |
| `ShowEntityHierarchy` | `bool` | `false` | Draw parent-child lines |
| `ShowEntityPosition` | `bool` | `false` | Draw position markers |
| `BoundsColor` | `Color` | `Lime` | Color for bounds |
| `IdColor` | `Color` | `Yellow` | Color for IDs |
| `TagColor` | `Color` | `Cyan` | Color for tags |
| `HierarchyColor` | `Color` | `Magenta` | Color for hierarchy lines |
| `PositionColor` | `Color` | `Red` | Color for position markers |
| `LineThickness` | `float` | `1.0f` | Line thickness for overlays |

## Example Usage

```csharp
// Setup in your scene or game initialization
entitySystem.DebugMode = true;

// Configure debug overlays
entitySystem.DebugConfig.ShowEntityBounds = true;
entitySystem.DebugConfig.ShowEntityIds = true;
entitySystem.DebugConfig.ShowEntityHierarchy = true;
entitySystem.DebugConfig.LineThickness = 2f;

// Set a font for text overlays
var debugFont = new FontAsset("DebugFont");
debugFont.Load(Content);
entitySystem.DebugFont = debugFont;

// Toggle debug mode at runtime (e.g., with a key press)
if (Keyboard.GetState().IsKeyDown(Keys.F3))
{
    entitySystem.DebugMode = !entitySystem.DebugMode;
}
```

## Performance Considerations

- Debug visualization **only** renders when `DebugMode` is `true`
- Text overlays require a font asset and SpriteBatch draw calls
- Hierarchy lines are drawn once per frame for all entities with parents
- Consider disabling debug mode in release builds for optimal performance

## Best Practices

1. **Toggle at runtime**: Use a key binding to toggle debug mode without restarting the game
2. **Selective overlays**: Enable only the overlays you need for the issue you're debugging
3. **Release builds**: Disable or conditionally compile debug mode for production builds
4. **Font caching**: Reuse the same font asset across scenes to avoid reload overhead
