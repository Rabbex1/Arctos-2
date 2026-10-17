# Arctos

Arctos is a .NET framework for building desktop tooling and UI-heavy applications. It brings together reusable infrastructure for commands, documents, tool windows, plugins, shared WPF controls, charting, inputs, and supporting data models.

This repository contains the current public solution for the second-generation Arctos codebase.

## Overview

Arctos is organized as a set of focused libraries rather than a single monolithic application. The current public solution centers on core framework abstractions, shell/runtime services, and WPF-oriented UI infrastructure.

## Solution Layout

The main projects in this repository are:

- `Arctos`
  - core framework types such as plugins, documents, messaging, configuration, utilities, and view-model contracts
- `Arctos.Shell`
  - shell-oriented services for commands, menus, dialogs, tool windows, logging, documents, and options
- `Arctos.Wpf`
  - shared WPF controls, styles, dialogs, resource loading, drag-drop, and AvalonDock support
- `Arctos.Wpf.Charts`
  - charting primitives, axes, ranges, presenters, and graphics
- `Arctos.Wpf.Inputs`
  - WPF input/view-model and templating infrastructure
- `Arctos.Wpf.OpenTk`
  - OpenTK-backed rendering and interop support
- `Arctos.TimeData`
  - time/data abstractions and sequence models
- `Arctos.Core`
  - additional core-layer library work

## Building

The solution builds with the standard .NET SDK workflow:

```powershell
dotnet build "Arctos 2.sln" -v:minimal
```

For more build notes, see [`BUILDING.md`](BUILDING.md).

The reusable Forms brush editor and its WPF and MudBlazor harnesses are documented in [`Forms/BRUSH_INPUT.md`](Forms/BRUSH_INPUT.md).

## Status

This is an early public release of a long-evolved codebase. The repository is usable today, but it is still being cleaned up and shaped for a broader audience. Some projects may move to separate branches over time as the public surface area becomes more deliberate.

## Goals

Arctos is intended to provide reusable building blocks for:

- command-driven desktop tooling
- dockable WPF application shells
- shared controls and input components
- charting and rendering infrastructure
- plugin-oriented application composition

## License

This repository is released under the MIT license. See [`LICENSE`](LICENSE).
