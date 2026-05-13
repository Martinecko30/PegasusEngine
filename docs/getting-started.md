# Getting Started

This guide explains how to set up, build, and run PegasusEngine.

## Requirements

Before working with the project, make sure you have:

- .NET SDK installed.
- A C# IDE such as JetBrains Rider or Visual Studio.
- Git installed, if cloning from a repository.
- DocFX installed, if you want to build the documentation.

## Clone the Repository

bash git clone <repository-url> cd PegasusEngine```

## Restore Dependencies

From the solution root, run:

```bash
bash dotnet restore PegasusEngine.sln
``` 

## Build the Solution

``` bash
bash dotnet build PegasusEngine.sln```
```

## Run the Project

Open the solution in your IDE and run the appropriate startup project.

Usually, the editor project should be used as the main application entry point.

## Build Documentation

If DocFX is installed, documentation can be generated from the solution root with:

```
bash docfx docs/docfx.json
```

To build and serve the documentation locally:

```bash
bash docfx docs/docfx.json --serve
```

Then open the local URL shown in the terminal, usually:

```bash
text http://localhost:8080
```

## Documentation Workflow

When adding new public classes, methods, or properties, use XML documentation comments:
```csharp
/// Describes what this method does.
public void Example() { }
```

DocFX will include those comments in the generated API reference.

