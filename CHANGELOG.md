# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/), and this project adheres to [Semantic Versioning](https://semver.org/).

## [1.0.0] - 2025-07-17

### Added

- **ApmPackager CLI tool** for creating and materializing agent packages
  - `pack` command to create `.nupkg` packages from `.github` folders
  - `materialize` command to extract package content into project directories
  - Auto-generated `agent-package.json` configuration
  - Content hash tracking for automatic version bumping
- **ApmVSExtension** Visual Studio extension
  - Right-click solution menu command to create agent packages
  - Package configuration dialog for editing metadata
  - Automatic patch version increment on content changes
- **Smart materialization strategy** via embedded MSBuild inline task
  - Copies new files from packages on build
  - Preserves user-modified files using SHA-256 manifest tracking
  - Updates unmodified files when package version changes
  - Solution-level materialization with project-level fallback
- **Unit test suites** for both CLI tool and VS extension
