# Agent Package Manager VS Code Extension

This extension adds the package creation workflow from `ApmVSExtension` to Visual Studio Code.

## Commands

- **Agent Package Manager: Create Agent NuGet Package (Workspace)**
  - Packages the `.github` folder from the selected workspace folder.
- **Agent Package Manager: Create Agent NuGet Package (Folder)**
  - Packages the `.github` folder from the folder you right-click in Explorer.

## Behavior

The extension mirrors the Visual Studio extension workflow:

1. Finds the `.github` folder in the selected base directory.
2. Reads or creates `.github/agent-package.json`.
3. Computes a content hash and auto-increments the patch version when packaged content changes.
4. Prompts for package metadata.
5. Creates the package in `bin/packages`.

## Packager dependency

By default, the extension uses the sibling `ApmPackager` project in this repository.

You can override that location with the `apmVscodeExtension.packagerPath` setting and point it to one of these:

- `ApmPackager.csproj`
- a built `ApmPackager.dll`
- a native executable

## Running locally

1. Open the `ApmVSCodeExtension` folder in Visual Studio Code.
2. Press `F5` to launch an Extension Development Host.
3. Run one of the contributed commands from the Command Palette or Explorer context menu.
