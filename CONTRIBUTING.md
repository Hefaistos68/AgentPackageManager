# Contributing to Agent Package Manager

Thank you for your interest in contributing! This document provides guidelines to help you get started.

## Development Setup

1. Clone the repository
2. Open `AgentPackageManager.sln` in Visual Studio 2022 (17.8+)
3. Restore NuGet packages
4. Build the solution

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Visual Studio 2022](https://visualstudio.microsoft.com/) with the **Visual Studio extension development** workload (for the VSIX project)

## Building

```bash
dotnet build
```

> **Note:** The `ApmVSExtension` project targets .NET Framework 4.8 and requires MSBuild (Visual Studio). The `ApmPackager` project and both test projects can be built with `dotnet build`.

## Running Tests

```bash
dotnet test
```

The solution includes two test projects:

| Project | Covers |
|---|---|
| `ApmPackager.Tests` | CLI tool: package creation, materialization, path handling, configuration |
| `ApmVSExtension.Tests` | VS extension: package GUIDs, command IDs, VSIX metadata, config serialization |

## Code Style

- Follow the existing code conventions in the repository
- Use XML documentation comments on all public and internal members
- Use `sealed` on classes not designed for inheritance
- Prefer `record` types for immutable data

## Pull Requests

1. Fork the repository and create a feature branch from `main`
2. Make your changes with clear, focused commits
3. Ensure all tests pass (`dotnet test`)
4. Ensure the solution builds without warnings
5. Open a pull request with a clear description of the change

## Reporting Issues

Use [GitHub Issues](../../issues) to report bugs or suggest features. Please include:

- A clear description of the problem or suggestion
- Steps to reproduce (for bugs)
- Expected vs actual behavior
- Environment details (OS, .NET version, Visual Studio version)
