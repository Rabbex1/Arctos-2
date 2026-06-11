# Arctos.Nodes Roadmap

## Overview

Arctos.Nodes is a reusable node editor core intended to support multiple applications through a shared, host-neutral architecture.

Initial target applications include:

- JanusCMT Studio
- Scenix
- Eric
- NetMap
- future desktop or web tools

The project goal is not to build a product-specific editor. The goal is to build a reusable node editor family with a strong shared core that can be consumed by different applications.

For roadmap and architecture discussion, the current mental model is:

- `App -> Arctos.Nodes`

Platform wrappers such as WPF or Blazor are intentionally out of scope for this first planning phase unless a decision specifically depends on them.

## Use Case Drivers

The roadmap should be informed by real application use cases rather than by abstract editor features alone.

Current design-driver applications are:

- `JanusCMT`
  - visual authoring for device profiles, routine packages, and higher-level orchestration
- `Eric`
  - package graph authoring, execution graph authoring, and reusable composite/library graph authoring
- `Scenix`
  - replacing the current tree-based OpenTK render pipeline editing experience with a node-based graph editor
- `NetMap`
  - visual authoring, navigation, and operational launching from network and subsystem maps

These use cases reinforce several shared design needs:

- the editor must remain generic while still supporting rich app-defined node sets
- applications must map domain models into Arctos.Nodes rather than exposing file formats directly to the editor
- typed ports, validation, and graph structure visibility are important first-class concerns
- property panels, diagnostics, and surrounding tooling are part of the overall authoring experience
- composite/subgraph support is likely to become important after the first core editor slice
- the editor must support both flow/program-style graphs and topology/operations-style graphs

## Direction

Arctos.Nodes should become the authoritative programming model for:

- graph structure
- node structure
- node visuals
- interaction rules
- layout rules
- view state rules
- rendering-facing descriptions

The application should define its own domain-specific nodes, but only by composing capabilities exposed by Arctos.Nodes.

This means:

- the app defines what nodes exist
- Arctos.Nodes defines what a node can be made from
- the app cannot request unsupported visual or structural primitives

Different applications may use those capabilities differently:

- some applications will be strongly port and connection driven
- some applications will use grouping, metadata, and actions more heavily than dense connection graphs

Across the current use cases, Arctos.Nodes should be treated as an authoring surface rather than as a runtime engine.

Applications remain responsible for:

- business persistence when they choose not to use native Arctos documents
- compilation or validation passes
- runtime execution semantics
- mapping between domain objects and editor state

## Current Artifact Direction

The persisted artifact split is now becoming clearer:

- `.nmap`
  - map file
  - settings
  - graph
  - local library and/or external library references
- `.nlib`
  - shared library file
  - reusable node definitions only

The in-memory resolved model is:

- `NodeMap`
  - settings
  - graph
  - references to loaded in-memory libraries

This direction reduces duplication while still allowing fully self-contained maps where needed.

## Architectural Principles

### 1. Generic Editor, App-Specific Nodes

Arctos.Nodes must remain application-neutral.

It should understand graphs, nodes, ports, connections, layout, and rendering descriptions, but it should not know about:

- product file formats
- product-specific persistence
- product-specific business workflows

Applications are responsible for mapping their own domain concepts into Arctos.Nodes definitions and instances.

This projection pattern is common across the current use cases:

- app domain model
- projection into Arctos.Nodes editor structures
- graph editing
- projection back into app domain objects

### 2. Fully Encapsulated Nodes

Nodes should be treated as fully described editor objects.

The core should support complete node descriptions including areas such as:

- icon
- background and fill
- border styling
- typography
- content elements
- ports
- layout
- size
- position/state through node instances
- multiple visual levels of detail

### 3. Strongly Typed Capability Model

The core capability surface must be type-safe.

Strings should not be used to define editor semantics such as:

- shape kind
- fill kind
- border kind
- content element kind
- port placement
- visual mode

Strings are acceptable only for user-facing content such as labels, descriptions, and input values.

The expected direction is:

- enums for closed choices
- typed descriptors for parameterized visual or structural data
- application-defined node identity outside of string-driven core semantics

### 4. Core Capability Catalog

Arctos.Nodes should own the catalog of supported node capabilities.

This catalog represents what the editor can render and interact with. Applications define nodes by composing from this supported set.

Examples of capability areas include:

- supported shape kinds
- supported fills and gradients
- supported borders
- supported content elements
- supported port layouts
- supported visual modes

### 5. Multiple Node Views

A node should support multiple visual representations of the same underlying definition.

Initial expected views are:

- toolbox view
- graph summary view
- graph detail view

These are not separate node types. They are different visual states or levels of detail for the same node definition and node instance.

### 6. Dual Node Creation UX

The editor should support both common node creation patterns:

- side toolbox/panel
- right click add

These should be treated as different UI entry points into the same creation model rather than separate systems.

## Terminology

The following terms should be used consistently:

- `Node Map`
  - the in-memory resolved model used by the Nodes system
- `Node Map File`
  - the persisted `.nmap` artifact
- `Node Library File`
  - the persisted `.nlib` artifact
- `Node Definition`
  - the app-defined description of a node built from a `NodeShape` and core-supported capabilities
- `Node Instance`
  - a placed occurrence of a node inside a graph
- `Node Library`
  - the set of node definitions a map or shared library exposes to the editor
- `Node Graph`
  - the actual placed graph content when the map contains one
- `Core Capability Catalog`
  - the set of structural and visual primitives Arctos.Nodes supports

## Definition vs Instance

This separation is an important early design boundary.

Expected direction:

- `Node Definition`
  - structural identity
  - supported content model
  - port model
  - default visual description
  - view-specific visual descriptions where required
- `Node Instance`
  - graph position
  - current size if resizable
  - current values/state
  - selection/hover/expanded state
  - other graph-local transient state

Final boundaries will be formalized during Milestone 1.

## Initial Non-Goals

The first phase should not attempt to build:

- product-specific persistence inside the editor
- product-specific node logic inside the core
- app runtime execution inside the editor
- a giant visual scripting engine
- arbitrary string-driven visual definitions
- unsupported custom one-off rendering primitives

## Roadmap Shape

The current roadmap direction is:

- `M1`
  - define the architecture and contracts
  - define the capability catalog
  - define node definition versus node instance boundaries
  - define node creation flow
  - define initial content and visual model
- `M2`
  - establish the shared graph and node contracts
  - establish the first reusable rendering-facing model
  - establish the initial interaction model
- `M3`
  - deliver the first usable editor slice including node creation, layout, ports, and connections
- `M4`
  - integrate thin platform wrappers and test beds
  - verify that the same node definitions can be rendered consistently across targets

This roadmap is expected to evolve as Milestone 1 decisions are finalized.
