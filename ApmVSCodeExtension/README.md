# Agent Package Manager VS Code Extension

Create NuGet packages from .github agent configuration folders directly inside Visual Studio Code.

## What it does

The extension packages GitHub Copilot assets (agents, skills, instructions, prompts, and related files) from a selected folder into a .nupkg using ApmPackager.

It supports two command flows:

- Workspace-based packaging from the Command Palette
- Folder-based packaging from the Explorer context menu

## Commands

### Agent Package Manager: Create Agent NuGet Package (Workspace)

- Command ID: apmVscodeExtension.createWorkspaceAgentPackage
- Trigger: Command Palette
- Behavior:
  - If one workspace folder is open, that folder is used.
  - If multiple folders are open, you are prompted to choose one.
  - The selected folder must contain a .github directory.

### Agent Package Manager: Create Agent NuGet Package (Folder)

- Command ID: apmVscodeExtension.createFolderAgentPackage
- Trigger: Explorer folder context menu
- Behavior:
  - Uses the folder you right-click in Explorer.
  - If a file is selected, uses its parent directory.
  - The resolved base folder must contain a .github directory.

## Packaging workflow

For both commands, the extension:

1. Locates .github in the selected base directory.
2. Reads or creates .github/agent-package.json.
3. Computes a content hash of package source files (excluding agent-package.json).
4. Auto-bumps patch version when content changed since last package.
5. Prompts for metadata: package ID, version, description, authors.
6. Runs ApmPackager with:
   - pack --source <baseDirectory> --output <baseDirectory>/bin/packages --force
7. Writes updated contentHash to agent-package.json.
8. Shows success and offers Reveal in Explorer for the generated .nupkg.

## Configuration

### apmVscodeExtension.packagerPath

Optional string setting to override how ApmPackager is resolved.

Supported values:

- Path to ApmPackager.csproj
- Path to ApmPackager.dll
- Path to native executable

Resolution behavior:

- If this setting is configured, it is used (relative paths resolve from workspace root).
- If empty, the extension tries repository-relative defaults:
  - ../ApmPackager/ApmPackager.csproj
  - ./ApmPackager/ApmPackager.csproj

If no valid packager is found, packaging fails with an actionable error.

## Prerequisites

- Visual Studio Code 1.90 or newer
- .NET 8 SDK (required when packager path points to .csproj or .dll)
- ApmPackager available either:
  - as sibling project in this repository, or
  - via apmVscodeExtension.packagerPath

For development and tests in this folder:

- Node.js 18+
- npm

## Install and run

### Development Host (recommended while editing)

1. Open this folder in VS Code: ApmVSCodeExtension
2. Install dependencies:
   - npm install
3. Press F5 (Run Extension)
4. In the Extension Development Host, execute one of the extension commands

### Package and install VSIX locally

1. Install VSCE:
   - npm install -g @vscode/vsce
2. Build VSIX from this folder:
   - vsce package
3. Install the generated VSIX:
   - VS Code command: Extensions: Install from VSIX...
   - or CLI: code --install-extension apm-vscode-extension-<version>.vsix

## Usage example

1. Ensure your target folder contains .github
2. Run one of the commands
3. Confirm or edit metadata prompts
4. Retrieve output in:
   - <baseDirectory>/bin/packages

## Files created/updated by the extension

- .github/agent-package.json
  - Created if missing
  - Updated when metadata changes and after successful package creation (contentHash)
- <baseDirectory>/bin/packages/*.nupkg

## Testing

Run unit tests for extension logic from this folder:

- npm test

Current tests cover version bumping, path handling, config normalization and persistence, packager command selection, source file enumeration, and package content hash behavior.

## Troubleshooting

- Error: Open a workspace folder first.
  - Open a folder/workspace in VS Code before running workspace command.

- Error: No .github folder found in '<path>'.
  - Ensure .github exists in the selected base folder.

- Error: Configured packager path '<path>' does not exist.
  - Fix apmVscodeExtension.packagerPath.

- Error: Unable to locate ApmPackager.
  - Configure apmVscodeExtension.packagerPath or use repository sibling layout.
