# CoreEssentials-MonoGame

[![NuGet](https://img.shields.io/nuget/v/CoreEssentials-MonoGame.svg)](https://www.nuget.org/packages/CoreEssentials-MonoGame)
[![Build Status](https://img.shields.io/badge/build-passing-brightgreen.svg)](https://github.com/LeoTheLegion/core-essentials-monogame)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)

A comprehensive framework built on top of MonoGame that provides essential building blocks for game development. CoreEssentials-MonoGame accelerates game development by offering ready-to-use systems for common game development challenges.

## Features

- **🎮 Scene Management**: Easily create, transition between, and manage game scenes
- **👾 Entity System**: Object-oriented approach to game entity management
- **🔄 Coroutine System**: Simplify asynchronous operations with Unity-like coroutines
- **🎯 Input Handling**: Event-driven input system for keyboard, mouse, and gamepad
- **🎵 Audio Management**: Flexible audio playback with XML-defined sound properties
- **🛠️ Debugging Tools**: In-game console, logging, and visual debugging aids
- **🧩 Game System Architecture**: Modular, extensible game systems
- **💪 Physics Integration**: Built-in physics using Aether.Physics2D
- **🖼️ GUI System**: User interface components powered by Myra
- **🎨 Asset Management**: Simplified asset loading, caching and management

## Getting Started

### Installation

CoreEssentials-MonoGame is available as a NuGet package:

```bash
# Via Package Manager Console
Install-Package CoreEssentials-MonoGame

# Via CLI
dotnet add package CoreEssentials-MonoGame
```

### Basic Usage

Here's a simple example to get you started:

```csharp
using CoreEssentials;
using Microsoft.Xna.Framework;

namespace YourGame
{
    public class Game1 : MainGame
    {
        public Game1() : base() 
        { 
            // Configure game properties
            Window.Title = "My Game";
        }

        protected override void Initialize()
        {
            base.Initialize();
            
            // Load your initial scene
            SceneManager.LoadScene(new MainMenuScene());
        }
    }
}
```

## Documentation

Comprehensive documentation is available in the [docs](docs) folder:

- [Getting Started Guide](docs/GettingStarted.md)
- [Scene Management](docs/SceneManagement.md)
- [Entity System](docs/EntitySystem.md)
- [Physics System](docs/PhysicsSystem.md)
- [Coroutines](docs/Coroutines.md)
- [Input Handling](docs/InputHandling.md)
- [Audio System](docs/AudioSystem.md)
- [Debugging Tools](docs/DebuggingTools.md)
- [Game Systems](docs/GameSystems.md)
- [Asset Management](docs/AssetManagement.md)
- [GUI System](docs/GUISystem.md)
- [Advanced Topics](docs/AdvancedTopics.md)

## Playground Examples

The repository includes a `CoreEssentials.Playground` project with example scenes that demonstrate various features:

- **Character Scene**: Demonstrates sprite animation, character entities, and audio management
- **Physics Entity Scene**: Shows physics simulation with multiple dynamic bodies
- **GUI Examples**: Includes Canvas usage for creating UI layouts and interactive elements

To run the examples:

```bash
# Clone the repository
git clone https://github.com/LeoTheLegion/core-essentials-monogame.git

# Navigate to the repository
cd core-essentials-monogame

# Build and run the playground
dotnet build
dotnet run --project CoreEssentials.Playground
```

## Code Examples

### Creating a UI with Canvas

```csharp
// Create a new canvas for UI elements
Canvas hudCanvas = new Canvas();
hudCanvas.SetPosition(new Vector2(20, 20));

// Add widgets to the canvas
var healthLabel = new Label { Text = "Health: 100" };
hudCanvas.AddWidget(healthLabel);

var inventoryButton = new Button();
inventoryButton.Content = "Inventory";
inventoryButton.Top = 30;
hudCanvas.AddWidget(inventoryButton);

// Update the canvas position in your game loop
hudCanvas.Update(gameTime);

// Clean up when done
hudCanvas.CleanUp();
```

## Building from Source

```bash
# Clone the repository
git clone https://github.com/LeoTheLegion/core-essentials-monogame.git

# Build the solution
dotnet build

# Run tests
bash ./scripts/test.sh

# Publish the NuGet package
bash ./scripts/publish.sh
```

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

1. Fork the repository
2. Create your feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add some amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## License

This project is licensed under the MIT License - see the LICENSE file for details.

## Acknowledgments

- [MonoGame](https://www.monogame.net/) - The base framework that makes this all possible
- [Aether.Physics2D](https://github.com/tainicom/Aether.Physics2D) - The physics engine integrated into CoreEssentials
- [Myra](https://github.com/rds1983/Myra) - The UI library used for the GUI system
- [MonoGame.Extended](https://github.com/craftworkgames/MonoGame.Extended) - For additional MonoGame extensions

## Contact

Michael Mena - [@LeoTheLegion](https://github.com/LeoTheLegion)

Project Link: [https://github.com/LeoTheLegion/core-essentials-monogame](https://github.com/LeoTheLegion/core-essentials-monogame)