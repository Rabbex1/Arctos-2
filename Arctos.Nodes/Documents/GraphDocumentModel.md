# Graph Document Model

## Purpose

This document defines the current direction for how a node graph should be represented at the document level in `Arctos.Nodes`.

It is written in a clear reference style so it can support both:

- internal architecture discussions
- future user or developer documentation

## Overview

A graph document should not begin directly at the root node graph.

Instead, the graph should live inside a document model that provides:

- graph settings
- a node library
- editing rules for definition-driven visuals
- optional root graph content

This allows the editor to support different application needs while staying generic.

## In-Memory and Persisted Use

`Arctos.Nodes` should support two equally valid usage patterns:

- applications that build or project graph documents in memory
- applications that persist real Arctos node documents directly

In many application scenarios:

- the application creates or projects graph data in memory
- the application passes that graph document into `Arctos.Nodes`
- the application receives the updated graph back after editing
- or the application observes changes live while editing is happening

In other scenarios, especially document-driven tools such as `NetMap`:

- the application may save and load the actual Arctos node document
- the node document may be the primary persisted artifact
- runtime callbacks and live services still remain application-owned

This means `Arctos.Nodes` should be understood as both:

- an in-memory graph editor
- a native persisted document model for apps that want to use it that way

## Core Idea

The graph document sits above the root node graph.

That means the document is responsible for describing:

- what kind of graph this is
- what node definitions are available
- how those node definitions should look by default
- which visual properties are fixed by the application
- which visual properties users may override in edit mode

The node graph itself then contains the actual graph content when the document includes one.

## High-Level Structure

The current direction is:

- `GraphDocument`
  - settings
  - node library
  - optional node graph

This keeps document-level concerns separate from graph-content concerns.

## Graph Document

A `GraphDocument` is the top-level container for a graph editing session.

It should contain:

- document settings
- a node library
- optional node graph content

The document is the place where applications define the rules and defaults for a graph.

## Application-Owned Persistence

Applications remain responsible for their real business persistence formats.

That includes:

- product-specific file formats
- SQL storage
- remote APIs
- domain object serialization

`Arctos.Nodes` should not own those application-specific persistence concerns.

Instead, the application should:

- load or construct domain data
- project that data into a `GraphDocument`
- pass the document to `Arctos.Nodes`
- map the edited document back into domain objects when needed

## Document Settings

Document settings describe how the document behaves in the editor.

Examples may include:

- name
- document type
- file format version
- content flags such as `ContainsGraph`
- startup operation mode
- editor presentation settings
- display preferences
- node definition override rules
- graph-specific behavioral options

These settings should focus on the document as a graph editing artifact, not on application runtime behavior.

## Native Document Persistence

`Arctos.Nodes` should support a native persistence format for its own document model.

This is useful for:

- document-driven applications such as `NetMap`
- node library files
- shared style and component libraries
- sample documents
- bug reproduction
- renderer validation
- regression testing
- verifying that WPF and Blazor produce the same visual results
- sharing test graphs across UI hosts

The important rule is that this remains:

- native Arctos document persistence
- not application business persistence

Applications may choose to use it as much or as little as they want.

## Neutral Graph Serialization

If `Arctos.Nodes` provides built-in persistence, it should serialize the generic graph document model rather than any application-specific business format.

That means it may reasonably include:

- document settings
- node library
- node graph
- node instances
- connections
- junctions
- external boundary nodes
- composite hierarchy
- layout and visual information

It should not include:

- runtime delegates
- app service bindings
- click or double-click callbacks
- transient run-mode integration hooks

This allows the editor to round-trip its own graph state while still staying generic.

## Document Sections

Every saved Arctos document should contain:

- `Settings`
- `NodeLibrary`

Some documents should also contain:

- `NodeGraph`

The current direction is:

- `Settings` is always required
- `NodeLibrary` is always required
- `NodeGraph` is optional

This allows a single file type to support both:

- library-only documents
- full graph documents

## Versioning and Content Flags

Because the document format is expected to evolve, version information should live in `Settings`.

Important settings-level fields should include:

- `FormatVersion`
- `ContainsGraph`

This allows the loader to:

- detect the persisted document version
- know whether a graph section should be present
- support migration later without redesigning the file model

## Node Library

Each graph document should carry a node library.

The node library defines the set of node definitions that can be used in that document.

Each node definition should have a stable type name or identifier and a default visual and structural definition.

This is important because:

- the toolbox needs to know what kinds of nodes can be created
- each node instance needs a consistent base node definition
- applications often need all instances of a given type to look the same by default

I do not currently see a useful document shape where the node library would not exist.

## Node Definitions

A node definition describes what a node of that type should be by default.

Typical type-level defaults may include:

- node shape
- icon
- colors
- border styling
- font size
- font color
- header behavior
- port definitions
- connection behavior where relevant

This means the node library is the natural place for visual defaults such as:

- SVG icon assets
- background color
- border color
- font settings
- header strip settings
- standard icons

### Icons

Node definitions should include an icon.

The current direction is:

- the icon belongs to the node definition
- the icon is separate from the node shape
- the first persisted format should be an SVG string

This is useful because:

- the same icon can be shown in the toolbox, summary view, and detail view
- SVG remains cross-platform and scales cleanly
- the visual identity of a node can live in the document or node library rather than being hard-coded in the application

The intended boundary is:

- `NodeShape` defines the structural primitive
- the icon is a visual asset rendered inside defined icon regions or slots

The icon should not be treated as a replacement for the node's structural rendering contract.

## Node Instances

A node instance is a placed occurrence of a node definition within the node graph.

The node instance should normally contain:

- a reference to its node definition
- position
- size where applicable
- instance values and state
- transient editor state where appropriate

The node instance should not need to restate all default visual information if that is already provided by its node definition.

## Type Defaults and Instance Overrides

The current direction is:

- node definitions provide defaults
- node instances inherit those defaults
- document settings decide whether selected fields may be overridden

This is especially useful for applications that want a strong visual language.

For example:

- all nodes of type `Router`
- all nodes of type `Invoke`
- all nodes of type `Wait`

may need to share the same visual design automatically.

## Edit Control and Style Control

The document model should support applications that want to control visual consistency.

For example, an application may want:

- node color fixed by type
- border fixed by type
- font style fixed by type
- header appearance fixed by type

while still allowing users to:

- move nodes
- connect nodes
- delete nodes
- add nodes

This means the document model should focus on controlling:

- node definition defaults
- which visual properties are overridable

rather than controlling basic graph editing actions.

## Operation Modes

The document should be understood as operating in three broad modes:

- `LibraryEdit`
- `TreeEdit`
- `Run`

### Library Edit Mode

In `LibraryEdit` mode:

- node definitions can be created and edited
- styles can be adjusted
- components can be added or removed
- the node library can be curated

### Tree Edit Mode

In `TreeEdit` mode:

- graph structure can be changed
- nodes can be added, moved, and deleted
- connections can be created or removed
- visual editing may still be limited by document or node definition policy

### Run Mode

In `Run` mode:

- the application may drive temporary visual changes
- the graph may provide runtime feedback
- click or double-click behavior may be repurposed by the application

Examples include:

- highlighting active flow
- showing status, progress, or actions
- allowing operator interaction

These runtime visual changes should be treated as application-driven presentation state rather than as document defaults.

## Node Graph Content

When `Settings.ContainsGraph` is `true`, the document contains a node graph.

The node graph content inside a document should still follow the current hierarchy direction:

- the root graph can be a composite node
- nested composite nodes can contain internal graphs
- leaf nodes contain no internal graph

This means the document owns the graph as a whole, while the graph itself may still be hierarchical.

## Relationship to the Toolbox

The toolbox should be driven from the node library.

That means:

- the document defines which node definitions are available
- each node definition has a type name or identifier
- each new node instance starts from the defaults defined by that definition

This supports consistent creation behavior across:

- side panel creation
- right-click add
- future import or template workflows

## Editing Feedback Patterns

Applications may integrate with the editor in more than one way.

Common patterns include:

- retrieve the edited graph document after the editing session
- observe live changes while editing is in progress

Both patterns fit naturally with the in-memory-first model.

## Relationship to Current Use Cases

This model supports the current use cases well:

- `JanusCMT`
  - consistent definition-driven visuals for routines, operations, and orchestration nodes
- `Eric`
  - well-defined node libraries and strong package-level graph rules
- `Scenix`
  - consistent visual language across render pipeline node definitions
- `NetMap`
  - can persist the actual Arctos node document directly, including styles, components, and graph content

It also supports applications that want strong visual consistency without preventing graph editing.

## Current Architectural Direction

The current direction is:

- the graph lives inside a `GraphDocument`
- the document owns settings, a node library, and optional node graph content
- node definitions provide default visuals and structure
- node instances reference node definitions and carry placement and instance state
- `LibraryEdit`, `TreeEdit`, and `Run` are editor concerns, not node-definition concerns

This gives `Arctos.Nodes` a stable foundation for graph loading, editing, saving, and cross-platform rendering.
