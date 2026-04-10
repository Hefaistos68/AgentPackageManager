# Agent Package Manager

A toolset for packaging and distributing GitHub Copilot agent configurations (`.github` folder contents) as NuGet packages. Teams can author agent instructions, custom skills, and Copilot settings in one repository and share them across multiple projects via standard NuGet infrastructure.

## Overview

Agent Package Manager consists of two components:

| Component | Description |
|---|---|
| **ApmPackager** | .NET 8 command-line tool that creates NuGet packages from `.github` folders and materializes package content into projects |
| **ApmVSExtension** | Visual Studio extension (VSIX) that adds a right-click menu command to create agent packages directly from a solution |

### How It Works

1. **Author** your `.github` folder with Copilot instructions, agent definitions, skills, and other configuration files
2. **Package** the folder into a NuGet package (`.nupkg`) using the CLI tool or VS extension
3. **Install** the package into consuming projects via standard NuGet package management
4. **Build** the consuming project — the package's MSBuild targets automatically materialize the `.github` content at the solution level

> **Important:** The `.github` files are **not** placed into your project immediately when the NuGet package is installed. NuGet package restore only extracts the package — it does not execute MSBuild targets. The files are materialized on the **first build** after installation. This is a NuGet limitation: there is no mechanism to run custom logic at install or restore time with `PackageReference`.

The materialization uses a smart merge strategy:
- **New files** from the package are always copied
- **Unmodified files** are updated when the package version changes
- **User-modified files** are preserved and never overwritten
- A `.agent-manifest` file tracks file hashes to detect user modifications

## Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) or later
- [Visual Studio 2022](https://visualstudio.microsoft.com/) 17.8+ (for the VSIX extension)

### Building

```bash
dotnet build
```

### Running Tests

```bash
dotnet test
```

## CLI Usage

### Creating a Package

Package the `.github` folder from a source directory:

```bash
ApmPackager pack --source <directory> [--output <directory>] [--force]
```

| Option | Description |
|---|---|
| `--source` | **(Required)** Directory containing the `.github` folder to package |
| `--output` | Output directory for the generated `.nupkg` (defaults to current directory) |
| `--force` | Overwrite an existing package file |

Package metadata is read from `.github/agent-package.json`. If the file does not exist, it is created with default values.

### Materializing Package Content

Extract packaged `.github` content into a project directory:

```bash
ApmPackager materialize --package <path> --project <directory> [--force]
```

| Option | Description |
|---|---|
| `--package` | **(Required)** Path to a `.nupkg` file or extracted package directory |
| `--project` | **(Required)** Target project directory to receive the `.github` content |
| `--force` | Overwrite existing files in the target directory |

## Visual Studio Extension

The VSIX extension adds a **Create Agent NuGet Package** command to two context menus in Solution Explorer:

### Solution-Level Packaging

Right-click the **solution node** to package the `.github` folder located at the solution root. The generated package is written to `bin/packages/` under the solution directory.

[![Solution Context Menu](Extension%20Context%20Menu.jpg)](Extension%20Context%20Menu.jpg)

### Project-Level Packaging

Right-click a **project node** to package the `.github` folder located in the project directory. This allows individual projects to maintain their own agent configurations independently. The generated package is written to `bin/packages/` under the project directory.

### Workflow

Both commands follow the same workflow:

1. Locate the `.github` folder in the selected directory (solution root or project directory)
2. Read or create an `agent-package.json` configuration file
3. Open a dialog to review and edit package metadata
4. Create the NuGet package

Auto-version bumping: if the `.github` content has changed since the last package was created, the patch version is automatically incremented.

## Package Structure

Generated packages follow this layout:

```
<PackageId>.<Version>.nupkg
├── agentcontent/
│   └── github/          # Mirror of the source .github folder
│       ├── agents/
│       ├── skills/
│       ├── copilot-instructions.md
│       └── ...
└── build/
    └── <PackageId>.targets   # MSBuild targets for auto-materialization
```

## Configuration

### agent-package.json

The `agent-package.json` file is stored in the `.github` folder and controls package metadata:

```json
{
  "packageId": "MyProject.AgentConfig",
  "version": "1.0.0",
  "description": "GitHub Copilot assets package",
  "authors": "YourName",
  "contentHash": ""
}
```

| Field | Description |
|---|---|
| `packageId` | NuGet package identifier |
| `version` | Semantic version of the package |
| `description` | Package description |
| `authors` | Package authors |
| `contentHash` | Auto-managed hash of packaged content (used for version bumping) |

## Solution Structure

```
AgentPackageManager/
├── ApmPackager/              # CLI tool (.NET 8)
├── ApmPackager.Tests/        # Unit tests for the CLI tool (NUnit)
├── ApmVSExtension/           # Visual Studio extension (.NET Framework 4.8)
├── ApmVSExtension.Tests/     # Unit tests for the VS extension (NUnit)
└── README.md
```

## Contributing

See [CONTRIBUTING.md](CONTRIBUTING.md) for guidelines on how to contribute to this project.

## License

This project is licensed under the MIT License. See [LICENSE](LICENSE) for details.
