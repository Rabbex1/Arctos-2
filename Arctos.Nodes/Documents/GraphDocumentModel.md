# Node Map Model

## Purpose

This document defines the current direction for the top-level persisted and in-memory model used by `Arctos.Nodes`.

Inside the wider Shell ecosystem, `document` is treated as a host concern. Inside `Arctos.Nodes`, the primary concept is now a `NodeMap`.

## Terminology

The current terminology is:

- `NodeMap`
  - the in-memory resolved model used by the Nodes system
- `NodeMapFile`
  - the persisted `.nmap` file shape
- `NodeLibraryFile`
  - the persisted `.nlib` file shape
- `NodeLibrary`
  - a library of node definitions
- `NodeLibraryReference`
  - a lightweight reference to a shared library, typically by `Name` and `Version`
- `NodeGraph`
  - the placed graph content

The important distinction is:

- Shell uses `document`
- Nodes uses `map`

## In-Memory and Persisted Use

`Arctos.Nodes` should support both:

- applications that build maps in memory
- applications that persist native Nodes artifacts directly

The in-memory model should not imply persistence. That is one of the main reasons for moving from `NodeDocument` to `NodeMap`.

## Resolved Runtime Model

The current direction is:

- `NodeMap`
  - `Settings`
  - `Graph`
  - `Libraries[]`

`NodeMap` is the resolved runtime model used by the editor, renderer, and host integrations.

Each entry in `Libraries[]` should be a reference to an in-memory `NodeLibrary` instance, not a duplicated flattened copy.

This allows:

- standard application libraries to be loaded once
- many maps to share the same in-memory libraries
- later resolution by `Name` and `Version`

## Persisted File Shapes

The persisted file shapes are intentionally separate from the resolved runtime model.

### `.nmap`

The `.nmap` file is the persisted map artifact.

The current direction is:

- `NodeMapFile`
  - `Settings`
  - `Graph`
  - `LocalLibrary?`
  - `LibraryReferences`

Rules:

- a `.nmap` must have `Settings`
- a `.nmap` must have a `Graph`
- a `.nmap` must have either:
  - a `LocalLibrary`
  - or at least one `LibraryReference`

This keeps a map usable while allowing either:

- self-contained portability
- shared-library reuse

### `.nlib`

The `.nlib` file is the persisted shared library artifact.

The current direction is:

- `NodeLibraryFile`
  - `Name`
  - `Version`
  - `Library`

Rules:

- an `.nlib` exists to provide reusable node definitions
- an `.nlib` does not contain a graph
- an `.nlib` should be loadable independently of any specific map

This gives applications a clean way to define:

- standard libraries
- shared visual languages
- reusable domain-specific node sets

## Resolved vs Persisted

This is the most important structural distinction:

- `NodeMapFile`
  - persisted source artifact
  - local library plus external references
- `NodeLibraryFile`
  - persisted shared library artifact
- `NodeMap`
  - resolved in-memory model
  - graph plus references to loaded libraries

So the runtime map should not contain copied library data as its primary working model. It should contain resolved references to actual in-memory libraries.

## Settings

Map settings remain the place for:

- format version
- startup mode
- canvas color
- map name
- map type
- edit permissions

The current settings-level concepts include:

- `FormatVersion`
- `StartupMode`
- `CanvasColor`
- `Name`
- `DocumentType`
- `RuntimeLibraryEdit`
- `RuntimeGraphEdit`

## Library Strategy

The old “every map must embed its own full library” model is no longer the preferred direction.

Instead, the system should support both:

- embedded local library content
- external shared libraries

This gives us the benefits of both approaches:

- embedded library
  - portable
  - easy to share as a single artifact
- external library
  - avoids duplication
  - supports shared standards
  - supports versioned reuse

The effective model should therefore support:

- standard libraries loaded by the host application
- local map-specific library additions or overrides where needed

## File Resolution Direction

The current direction for library resolution is:

- a map file names the libraries it needs
- the host application or manager resolves those references
- the resolved `NodeMap` holds references to the loaded in-memory libraries

The first useful library reference shape is:

- `Name`
- `Version`

Possible future additions may include:

- source hint
- package identifier
- URI or registry hint

## Manager Direction

The top-level manager is moving toward responsibility for both:

- map artifacts
- library artifacts

At a conceptual level, the manager should eventually be able to:

- load and save `.nmap`
- load and save `.nlib`
- hold loaded maps
- hold loaded shared libraries
- resolve library references

This remains a core concern, not a UI concern.

## Operation Modes

The current editor operation modes are:

- `LibraryEdit`
- `GraphEdit`
- `Run`

### LibraryEdit

In `LibraryEdit`:

- node definitions can be created and changed
- styles can be adjusted
- components can be added or removed
- library content can be curated

### GraphEdit

In `GraphEdit`:

- graph structure can be changed
- nodes can be placed, moved, and deleted
- later connections can be created or removed

### Run

In `Run`:

- the application may drive temporary visual changes
- the graph may provide runtime feedback
- interaction can be repurposed by the application

## Relationship to the Shell

Inside `Arctos.Shell`, a map may still be hosted inside a shell document wrapper.

That host-level wrapper is where:

- tabs
- docking
- file-open integration
- save commands
- active-document tracking

belong.

That is specifically why the Nodes core moved away from the `Document` name.

## Current Direction Summary

The current architectural direction is:

- the core model is `NodeMap`
- the primary persisted map artifact is `.nmap`
- the primary persisted shared library artifact is `.nlib`
- a resolved `NodeMap` contains references to in-memory libraries
- shared libraries are a first-class concept
- portability is supported, but duplication is no longer mandatory

This gives `Arctos.Nodes` a better foundation for:

- self-contained maps
- shared library ecosystems
- application-standard libraries
- future cross-map and cross-library workflows
