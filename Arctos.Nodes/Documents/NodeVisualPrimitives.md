# Node Visual Primitives

## Purpose

This document captures the current design direction for primitive node visuals in `Arctos.Nodes`.

It is intended as a reference document for architecture and milestone discussions.

## Design Intent

The visual model should be built from a small set of strongly typed primitives rather than from arbitrary string-defined shapes or ad hoc per-application rendering rules.

The current visual breakdown is:

- node body shape
- ports as separate visual primitives
- connection paths and connection terminators
- junctions as connection-branching primitives
- external boundary nodes for graph entry and exit
- composite nodes as hierarchical graph containers
- optional header strip chrome
- optional header action buttons

This keeps the core flexible enough for current use cases while still preserving a closed and type-safe capability catalog.

## Node Definition Icon

Node definitions should include an icon as part of their persisted visual description.

The current direction is:

- the icon belongs to the node definition rather than to the node shape
- the first stored format should be an SVG string
- the icon is rendered into a defined icon slot rather than replacing the node body renderer

This keeps the visual identity of a node portable across:

- toolbox rendering
- graph summary rendering
- graph detail rendering

It also keeps icon assets:

- cross-platform
- scalable
- document or library driven rather than hard-coded in the application

The important boundary is:

- `NodeShape` defines the structural primitive
- the icon is a visual asset placed inside that structure

## Node Body Shape

Node body shape describes the main outer form of a node.

Node body shape should be independent from:

- port shape
- header strip visibility
- header actions

This allows the same body shape to be reused with different node behaviors and different UI chrome.

### Proposed `NodeShapeKind`

Initial candidate shape set:

- `RoundedRectangle`
- `Rectangle`
- `Diamond`
- `Circle`
- `Ellipse`
- `Capsule`
- `Hexagon`
- `Parallelogram`

Not all of these need to be implemented in `V1.0`, but they represent a reasonable closed catalog for the current use cases.

### Likely `V1.0` Minimum

The settled `V1.0` shape set is:

- `RoundedRectangle`
- `Diamond`
- `Circle`
- `Rectangle`
- `Capsule`

### Special Visual Containers

The following should be treated as related visual primitives:

- composite node
- comment / annotation

These are important for:

- JanusCMT grouped authoring regions
- Eric stage or composite boundaries
- NetMap site, rack, subnet, or logical grouping

## Ports

Ports should be modeled as a separate visual type that can be attached to a node.

Ports must not be implied by the node body shape itself.

This separation allows applications to use the same node shape with different port semantics.

### Proposed `PortShapeKind`

Initial port shape set:

- `Circle`
- `Square`
- `Diamond`

### Port Visual Properties

A port should support at least:

- fill color
- border color
- border thickness
- size
- placement/alignment on the node
- optional label or tooltip metadata

### Port Design Notes

Ports are especially important for:

- Eric execution and package graphs
- Janus routine and operation flows
- Scenix render pipeline graphs

NetMap may use ports less heavily, so the model should support nodes that have:

- many ports
- a few ports
- no ports

### Port Visibility

Ports should remain the endpoint concept for connections.

A port may be:

- visibly rendered with chrome
- visually minimal
- visually hidden

This allows the model to stay simple while still supporting clean-looking nodes that do not always show visible connection markers.

The current direction is:

- connections terminate on ports
- ports may or may not render visible chrome

## Connections

Connections should be modeled separately from node body shape and separately from ports.

The connection model should distinguish between:

- path shape
- terminator shape
- endpoint attachments

### Proposed `ConnectionPathKind`

Initial path set:

- `StraightLine`
- `SquareLine`
- `BezierLine`

### Path Notes

- `StraightLine`
  - useful for simple topology and relationship graphs
- `SquareLine`
  - orthogonal routing using horizontal and vertical segments only
- `BezierLine`
  - useful for classic node-editor flow graphs

For `V1.0`, `SquareLine` should be treated as orthogonal routing, not as a full obstacle-avoiding routing system.

### Proposed `ConnectionTerminatorKind`

Initial terminator set:

- `None`
- `Arrow`

Possible future additions:

- `Triangle`
- `Diamond`
- `Circle`

### Connection Visual Properties

A connection should support at least:

- path kind
- stroke color
- stroke thickness
- start terminator
- end terminator

Possible future styling support:

- dash style
- corner radius for orthogonal paths
- hover/selection overlays

## Connection Endpoints

The current direction is that connections terminate on ports only.

This keeps the model simpler for:

- validation
- serialization
- hit testing
- routing
- graph semantics

The endpoint rule is:

- node connections terminate on ports
- ports may have visible or hidden chrome

## External Boundary Nodes

Connections should be allowed to terminate in an external boundary node that represents connection to or from the surrounding graph tree.

In programming-style graphs, boundary nodes are the explicit input/output surface of the graph itself.

This is useful when:

- a graph needs explicit external inputs
- a graph needs explicit external outputs
- a composite or root graph needs visible entry and exit points
- a visual programming graph needs to show how inner flow crosses graph boundaries

An external boundary node should be treated as a lightweight graph primitive rather than as a normal domain node.

### Proposed `ExternalBoundaryKind`

Initial boundary set:

- `Input`
- `Output`

### Visual Direction

The preferred visual direction is:

- thin rectangle body
- pointed end indicating direction

That means:

- input boundary points inward toward the graph
- output boundary points outward from the graph

### Endpoint Semantics

An external boundary node should expose ports so that normal connection rules remain consistent.

The current direction is:

- connections still terminate on ports
- external boundary nodes provide those ports at the graph boundary
- boundary nodes make graph entry and exit explicit without requiring special-case connection semantics

This leads to a useful distinction:

- node ports are node-level input/output
- boundary nodes are graph-level input/output

### Relationship to Composite Nodes

External boundary nodes are especially useful for:

- projected inputs and outputs of composite nodes
- graph-root level entry and exit points
- keeping internal graph wiring explicit when crossing graph boundaries

They should therefore be considered part of the early graph-tree and composite-node design space.

## Junctions

Junctions should exist as an early graph primitive rather than being simulated through invisible fake nodes or arbitrary mid-line branching.

Junctions are not normal nodes.

They are dedicated graph elements used to branch connections.

### Proposed `JunctionKind`

Initial junction set:

- `Split`

### `V1.0` Junction Semantics

For `V1.0`, a junction should support:

- one incoming connection
- multiple outgoing connections

This covers the valid and common fan-out case without introducing ambiguous merge semantics too early.

### Junction Visual Direction

Initial visual direction:

- small dot
- or small square

Junction visuals should remain lightweight and should not be treated as full nodes with normal node chrome.

## Composite Nodes

`Group` and `Composite` should be treated as the same core primitive.

The current direction is to use a single `CompositeNode` concept with configurable visuals and configurable expansion behavior.

A composite node:

- owns an internal graph tree
- may behave like a simple visual grouping container
- may behave like a collapsible higher-level node
- may expose internal structure through projected ports later where needed

This keeps the hierarchy model simple:

- root document composite
- nested composite nodes
- leaf nodes

### Composite Data Direction

The current direction is:

- a composite node owns its internal graph directly
- the internal graph is not a separate document by default
- extraction into a separate reusable document can be a later operation

This means the document itself can be treated as the root composite node.

### Composite Display Modes

Composite behavior should be controlled by configuration rather than by separate primitive types.

### Proposed `CompositeExpandedDisplayKind`

Initial display set:

- `Inline`
- `Overlay`

Possible future additions:

- `SeparateView`

### Composite Configuration Ideas

Likely composite configuration areas include:

- `CanCollapse`
- `CollapsedTitle`
- `ExpandedDisplay`
- `ProjectInternalPorts`
- border styling
- background styling

This allows the same primitive to support both simple groups and drill-down composites.

### Example: Visual Grouping Container

A simple colored group-like composite could behave like:

- `CanCollapse = false`
- `ExpandedDisplay = Inline`
- `Border = None`
- `Background = LightBlue`
- `ProjectInternalPorts = false`

### Example: Collapsible Composite Node

A drill-down composite could behave like:

- `CanCollapse = true`
- `CollapsedTitle = "Complex Node"`
- `ExpandedDisplay = Overlay`
- `Border = Black`
- `Background = #0707FF`
- `ProjectInternalPorts = false`

### Composite Interaction Direction

The preferred early direction is:

- composite drill-down should use an overlay workspace
- the overlay may be implemented as a separate Skia canvas layered above the parent canvas
- expansion and collapse should be animatable
- the parent graph remains visible as dimmed or inactive context

This avoids the need for early auto-routing and sibling-node displacement that would come from in-place expansion of internal graphs.

### Composite Summary vs Detail

A composite node should support distinct presentations for:

- collapsed or summary view in the parent graph
- expanded internal graph view in overlay or inline mode

The expanded internal graph does not need to preserve the same geometric shape as the collapsed summary node.

The important continuity is identity and navigation, not literal shape morphing.

## Header Strips

Header strips should be treated as optional node chrome rather than as an inherent part of every node shape.

This is especially important for shapes such as:

- diamond
- circle
- compact utility nodes

where a permanently visible header may not fit naturally.

### Proposed `HeaderStripVisibilityMode`

Initial visibility modes:

- `AlwaysVisible`
- `Hidden`
- `VisibleOnHover`

### Header Strip Content

A header strip may support:

- title text
- icon
- status badge
- action buttons

The strip itself should remain optional.

## Header Actions

Header strips should support optional icon actions.

These actions allow lightweight node interaction without requiring inline editing controls inside the canvas.

### Proposed `HeaderActionKind`

Initial action set:

- `Edit`
- `Delete`

Possible future additions:

- `ExpandCollapse`
- `Duplicate`
- `Open`
- `Run`

### `V1.0` Guidance

For `V1.0`, header actions should be treated as simple click actions only.

They are appropriate for:

- opening popup settings
- opening detail panels
- deleting nodes
- invoking quick actions

They should not be treated as a replacement for full in-canvas widget editing.

## Relationship to `V1.0` Editing Scope

Current `V1.0` direction:

- nodes render static data inside the canvas
- editing is primarily done through property panels, popups, or dialogs
- inline textboxes, dropdowns, and complex embedded editors are deferred

This means header actions are allowed in `V1.0`, but heavy embedded widget behavior is not.

## Relationship to Current Use Cases

This primitive model is intended to support:

- `JanusCMT`
  - flow and orchestration nodes with optional detail access
- `Eric`
  - typed, port-heavy execution and package graphs
- `Scenix`
  - render pipeline graphs similar in spirit to Blender-style node layouts
- `NetMap`
  - topology and operations-oriented nodes with metadata, actions, and optional relationships

One important design rule follows from these use cases:

- `Arctos.Nodes` must support both flow/program-style graphs and topology/operations-style graphs

That means the primitive model should not become overly biased toward only one style of graph.

## Current Architectural Direction

The current direction is:

- small closed shape catalog
- strong styling and content flexibility
- ports as separate primitives
- connections and junctions as first-class graph primitives
- external boundary nodes as explicit graph entry and exit primitives
- composite nodes as the shared hierarchy primitive for grouping and drill-down
- optional node chrome
- type-safe contracts only

This should give the editor enough expressive power without drifting into arbitrary or unsupported rendering contracts.
