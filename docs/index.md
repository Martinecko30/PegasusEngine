---
_layout: landing
---

# Pegasus Engine

![Pegasus Engine Logo](../PegasusEngine/res/PegasusLogo.svg)

Pegasus Engine is a modular 3D game engine built with **C# 13** and **.NET 9.0**, using **OpenGL** through **OpenTK**.

It is designed around a layer-based architecture, asset management tools, an event system, logging, profiling, and an ImGui-based editor interface.

Inspired by [Spartan Engine](https://github.com/PanosK92/SpartanEngine) and [Laura](https://github.com/jakubg05/Laura).

## Documentation

- [Introduction](introduction.md)
- [Getting Started](getting-started.md)
- [API Reference](api/)

## Quick Links

| Section | Description |
|---|---|
| Introduction | Overview of the engine and project structure. |
| Getting Started | Setup instructions for building and running the project. |
| API Reference | Generated documentation from XML comments in the source code. |

## About

## Features

- **Modular Architecture**  
  Uses a `LayerStack` system to manage engine components such as rendering, UI, and logic independently.

- **Modern Rendering**  
  OpenGL-based rendering with support for shaders, skyboxes, meshes, and renderer modules.

- **Asset Pipeline**
  - **Model Loading** using **AssimpNet**.
  - **Image Processing** using **StbImageSharp**.
  - **Metadata System** using YAML-based `.pgmeta` files for asset GUIDs and properties.

- **Integrated Profiler**  
  Real-time performance monitoring with high-precision timers.

- **Event System**  
  Custom event-driven input and engine state management.

- **Logging**  
  Diagnostic logging using **Serilog**.

- **Editor**  
  ImGui-based editor interface for engine tools and workflows.

## Tech Stack

| Area | Technology |
|---|---|
| Language | C# 13.0 |
| Framework | .NET 9.0 |
| Graphics API | OpenGL 4.x |
| Windowing/Input | OpenTK |
| Editor UI | ImGui.NET |
| Model Importing | AssimpNet |
| Metadata Serialization | YamlDotNet |
| Logging | Serilog |
| Image Loading | StbImageSharp |

## Project Structure

```aiignore
PegasusEngine/
├── PegasusEngine/         # Core engine source (Layers, Events, Renderer)
|   ├── res/               # Engine resources (Images, models)
|   └── src/               # Engine main code
|       ├── Core/          # Core components
|       ├── Project/       # Project managing
|       └── Renderer/      # 3D Rendering
├── PegasusEditor/         # Editor tools and UI
├── PegasusRuntime/        # Runtime components
└── old/                   # Legacy modules (Audio, Physics, Scripting) **IN REWORK**
```

## Supported Platforms

Currently, Pegasus Engine primarily supports **Windows**.

Linux support is planned for the future. macOS support may be considered later, but it is not currently planned.

## Roadmap

- [x] **Core Scripting**: C# assembly loading and building.
- [x] **Project Management**: `.pgproj` serialization and asset tracking.
- [ ] **Prefab System**: Entity serialization and instantiation.
- [ ] **Particle System**: GPU-driven particle system.
- [ ] **PBR Rendering**: Physically based rendering implementation.
- [ ] **Audio Engine**: Custom audio system.
- [ ] **Physics System**: Custom 3D physics.
- [ ] **Linux Support**: Planned Linux platform support.

> [!TIP]
> There are many more features that could benefit this project. Feel free to implement them or create a new issue requesting them.

## Projects Using Pegasus

| Project | Description | Author |
|---|---|---|
| | | |

There are currently no listed projects using Pegasus Engine.

**Using Pegasus Engine in any way, shape, or form? Reach out — I would love to showcase your project!**

## License

This project is licensed under the MIT License. See the `LICENSE` file for details.

![Pegasus Engine Screenshot](../PegasusEngineScreenshot.png)