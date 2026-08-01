# Arctos.Nodes Architecture Notes

These are working notes captured during the top-down redesign. They are intentionally informal and may later be collated into a proper AI-readable integration and architecture document.

## Current Direction

### Recursive Spatial Model

- `Map` is the only recursive spatial container.
- A `Map` can contain child `Map` items and `NodePlacement` items.
- `NodeDefinition` is the only owner of `Column` and `Component`.
- `Column` and `Component` never exist directly at map level.

### Node Composition

- `NodeDefinition` owns:
  - `NodeColumn`
  - `NodeRow`
  - `NodeComponent`
- Existing row and column persistence is still considered valid.
- The older `CompositeNode` idea is no longer the preferred direction because recursion now belongs to `Map`.

### Overview Responsibility

- Both `Map` and `Node` can have an overview, but they are not the same concept.
- `Map` overview and `Node` overview must be treated as distinct presentation modes with different interaction meaning.
- A parent map shows child maps using a simplified overview representation.
- Drill-down flow is:
  - `Map -> Map -> ... -> Node`
- Breadcrumbs track the navigation path through nested maps.

### Overview Distinction

- `Map` overview is a navigation projection.
- `Node` overview is a presentation projection.
- They may both look visually simplified, but they serve different purposes.

#### Map Overview

- Represents a child map inside a parent map.
- Exists to support structural navigation.
- Double-clicking a map overview drills down into the child map.

#### Node Overview

- Represents a compact form of a node.
- Exists to support summary, status, dashboard, and quick interaction scenarios.
- Double-clicking a node overview does not drill down.
- Double-clicking a node overview should open detail, popup content, or expanded interaction for that node.

## Persistence Direction

### `.nlib`

- `.nlib` contains reusable `NodeDefinition` items only.
- `.nlib` is the import/source artifact for reusable node types.

### `.nmap`

- `.nmap` should be self-contained.
- A saved map should embed the node definitions it uses.
- A `.nmap` should not require external library references in order to load.
- Provenance metadata may be stored later so a node can record where it originally came from, but that metadata must not be required for loading.

### Runtime vs Persisted

- `NodeDefinition` is persisted.
- `NodePlacement` is persisted.
- `NodeInstance` is runtime-only.
- `NodeInstance` may be built from:
  - a persisted placement
  - an embedded definition
  - or a definition originally sourced from a library

## Library Registration

### Host Responsibilities

- Host applications should not need to parse `.nlib` themselves.
- Hosts should tell the core what libraries are available.
- The core should load, validate, and index the library content.

### Registration API Direction

- Base registration method should accept an in-memory `NodeLibrary`.
- Helper overloads can load from:
  - file path
  - `Uri`
- Preferred API shape:
  - `TryRegisterLibrary(NodeLibrary library, out reason)`
  - `TryRegisterLibrary(string path, out reason)`
  - `TryRegisterLibrary(Uri uri, out reason)`

### Registration Notes

- The path and `Uri` overloads are convenience helpers only.
- They should load the library and then call the base in-memory registration method.
- This keeps the core model portable across WPF, Blazor, and future platforms.

## Host / Profile Direction

### Platform Split

- `Arctos.Nodes` holds the core model/runtime concepts.
- `Arctos.Nodes.Host` will hold host-facing contracts such as sessions, profiles, and library registration.
- `Arctos.Nodes.Wpf` and `Arctos.Nodes.Blazor` are platform implementations.

### First-Party Host Apps

- Instead of a disposable test bed, the first consumers should be real apps:
  - desktop `Arctos Node Design`
  - web `Arctos Node Design`
- These apps should act as:
  - real editors
  - reference host implementations
  - showcase/demo applications

### Profile Direction

- Profiles remain the preferred way to define operational context.
- The first useful profiles are:
  - `NodeDesign`
  - `NodeTest`
  - `MapDesign`
  - `MapInteraction`
- Profiles should be capability-driven rather than hard-coded by name checks.

## Demo Direction

- A strong web demo is AI-generated structural maps.
- Example use case:
  - user enters a prompt
  - AI generates a `.nmap`
  - the design app opens it
  - user drills through nested maps down to leaf nodes
- This is useful even before interactive runtime nodes are fully implemented.

## Example Use Cases

### Cross-Project Host Use Cases

These are concrete projects intended to consume `Arctos.Nodes`. They are useful for identifying the common host/runtime model.

#### 1. Scenix

- Graphics pipeline system based on OpenTK.
- Nodes represent graphics/data/shader/animation flow.
- The hosting app builds maps dynamically in real time.
- Standard node libraries are likely, with possible future support for creating new node types.

#### 2. Eric

- Deterministic production pipeline.
- Strong fit for graph-based flow and patching.
- Likely uses predefined node libraries.
- Main focus is wiring predefined nodes together.

#### 3. Astraea Lab

- Experimental AI / neural network surface.
- Good fit for patching experimental systems together.
- Also a good fit for dashboard-style map overviews showing resource usage and state.
- Host app likely provides the map/runtime structure, but custom node design is still valuable.

#### 4. NetMap

- Work-oriented static operational map for IP-connected equipment.
- Nodes are not primarily defined by the host app at runtime; maps are designed and persisted.
- Main use case is a mostly static day-to-day map edited by an administrator.
- Users drill down, select equipment, and interact through node content such as:
  - links
  - status
  - external tool launch actions
  - telemetry feedback

#### 5. JanusCMT

- Communication and protocol orchestration system.
- Nodes are used to define and edit structures that would otherwise be stored in text formats.
- Also extends into a domain-specific visual programming surface for routines.
- This is one of the strongest cases for both node design and map interaction.

#### 6. Nodes

- `Arctos.Nodes` documenting itself using maps and nodes.
- Strong showcase for:
  - recursive maps
  - node overviews
  - drill-down
  - documentation as a live navigable surface

### Organisational Structures

- The recursive `Map -> Map -> Node` model works well for organisational hierarchies.
- Example:
  - top-level map shows departments
  - drilling into a department shows sub-departments or teams
  - drilling further eventually reaches leaf nodes representing people
- This supports both static structural viewing and later interactive runtime use.

### Interactive People / Entity Nodes

- Leaf nodes can act as interactive entity cards.
- Example person node content could include:
  - photo
  - name
  - title
  - department
  - email link
  - phone link
  - status indicator
- An image component has not been implemented yet, but there is no structural reason it cannot be added later.
- This means the system can support interactive company structures, government structures, and similar drill-down information systems.

### System Documentation by Map

- `Arctos.Nodes` itself is a strong first-party example of the recursive map model.
- The system can document its own structure as a navigable visual map.
- A top-level overview can show major subsystems, with drill-down into:
  - libraries
  - host contracts
  - platform layers
  - editors
  - runtime concepts
- This supports both:
  - overview understanding of the whole system
  - detailed drill-down to specific areas, functions, or concepts
- This use case should be represented in the web app, and may also be useful as a bundled desktop example.
- It is a good showcase because it demonstrates the system as:
  - a design tool
  - a documentation surface
  - a navigable architecture map

### "Nodes Explaining Nodes" Demo

- A signature first-party demo should be `Arctos.Nodes` explaining how `Arctos.Nodes` works.
- The demo is not only static documentation; it is also a live demonstration of the system being used as a documentation surface.
- Higher levels show subsystem overviews.
- Lower levels drill into concrete concepts such as:
  - libraries
  - host contracts
  - runtime objects
  - classes
  - methods
- Leaf nodes can include links directly to GitHub source files, documentation pages, or examples.
- This creates a self-documenting system where:
  - the structure explains the architecture
  - the drill-down demonstrates navigation
  - the leaf content connects the user directly to the real implementation
- This should be treated as a flagship web demo and a strong candidate for a bundled desktop example.

## Documentation Direction

- We should start capturing design decisions as we go instead of waiting until the end.
- The future project should likely include:
  - `README.md` for human entry
  - `AGENTS.md` for repo workflow and coding behavior
  - a deeper AI-readable integration/architecture document for consumers of the library

## Open Questions

- Final persisted shape for `Map` and `NodePlacement`
- How embedded definitions are stored inside `.nmap`
- Exact capability contract for profiles
- Exact host/session boundary in `Arctos.Nodes.Host`
- Whether provenance metadata is stored on definitions, placements, or both

## Emerging Common Ground

- Some hosts are app-driven and build maps dynamically from domain data.
- Some hosts are document-driven and rely on persisted user-authored maps.
- Some hosts mainly use predefined libraries.
- Some hosts need authoring of new node definitions.
- Some hosts need structural navigation via nested maps.
- Some hosts need dashboard-style node overviews and interaction without structural editing.
- This suggests the system must cleanly separate:
  - library registration
  - node definition authoring
  - map authoring
  - map interaction/runtime use
