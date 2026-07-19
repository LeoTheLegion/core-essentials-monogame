# CoreEssentials-MonoGame Framework Documentation

## Overview

CoreEssentials-MonoGame is a comprehensive framework built on top of MonoGame that provides essential building blocks for game development. Designed to accelerate game development by providing ready-to-use systems for common game development challenges.

**Version:** 0.8.2  
**Author:** Michael Mena  
**Repository:** [https://github.com/LeoTheLegion/core-essentials-monogame](https://github.com/LeoTheLegion/core-essentials-monogame)

## Key Features

- **Scene Management**: Easily manage and transition between game scenes
- **Entity System**: Object-oriented entity management for game objects
- **Physics Integration**: Built-in physics using Aether.Physics2D
- **Coroutine System**: Manage sequence-based operations and time-dependent tasks
- **Input Handling**: Simplified input detection and event handling
- **Audio Management**: Play, pause, and manage sound effects and music
- **Debugging Tools**: In-game console, logging, and physics visualization
- **GUI System**: User interface components built with Myra
- **XML Documentation**: IntelliSense support for easier API usage

## Getting Started

### Installation

CoreEssentials is available as a NuGet package:

```bash
dotnet add package CoreEssentials-MonoGame
```

### Basic Usage

A minimal MonoGame project using CoreEssentials:

```csharp
using CoreEssentials;
using Microsoft.Xna.Framework;

namespace YourGame
{
    public class Game1 : MainGame
    {
        public Game1() : base() 
        { 
            // Configuration can be done here
        }

        protected override void Initialize()
        {
            base.Initialize();
            // Load your initial scene
            SceneManager.LoadScene(new YourGameScene());
        }
    }
}
```

## Core Concepts

- [Scene Management](./SceneManagement.md)
- [Entity System](./EntitySystem.md)
- [Physics System](./PhysicsSystem.md)
  - [Migration Guide: Legacy → New Physics API](./Migration_Guide_Physics.md)
- [Coroutines](./Coroutines.md)
- [Input Handling](./InputHandling.md)
- [Audio System](./AudioSystem.md)
- [Debugging Tools](./DebuggingTools.md)
- [XML Documentation](./XMLDocumentation.md)

## Playground Examples

The framework includes a Playground project with practical demonstrations:

- **CharacterScene**: Demonstrates sprite animation, character movement, and audio management
- **PhysicsEntityScene**: Shows physics interactions with multiple entities and collision detection

To run these examples, build and run the CoreEssentials.Playground project.

## License

This project is licensed under the terms specified in the repository.