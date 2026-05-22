# Arctos.Nodes Milestones

## M1 - Architecture and Contract Definition

Milestone 1 is the design milestone. Its purpose is to lock down the core concepts before implementation begins.

### M1.1 - Define Core Terminology

Confirm and document the project vocabulary, including:

- `App`
- `Arctos.Nodes`
- `Node Definition`
- `Node Instance`
- `Available Nodes`
- `Core Capability Catalog`

### M1.2 - Define Architectural Boundaries

Document what belongs to:

- the application
- Arctos.Nodes
- future thin platform wrappers

Clarify that applications define nodes and consume the editor API, while Arctos.Nodes defines the supported structural and visual system.

### M1.3 - Define Definition vs Instance

Decide and document the separation between:

- node type-level description
- node instance placement and runtime state

This includes decisions around:

- position
- size
- value state
- selection state
- transient editor state

### M1.4 - Define the Core Capability Catalog

Establish the first closed set of supported core primitives, including:

- shape kinds
- fill/background kinds
- border kinds
- text/style primitives
- content element kinds
- port layouts and port placement concepts
- visual modes

### M1.5 - Define the Type-Safe Contract Strategy

Document the contract philosophy:

- enums for closed choices
- typed descriptors for parameterized values
- strings only for user-facing content

This milestone item should explicitly reject string-defined structural or visual semantics.

### M1.6 - Define the Node Visual Model

Decide how nodes describe their appearance in the editor, including:

- toolbox representation
- graph summary representation
- graph detail representation

Clarify whether these are separate descriptors, layered descriptors, or a shared descriptor with level-of-detail rules.

### M1.7 - Define the Node Content Model

Decide how node content is represented in a type-safe way, including areas such as:

- icon/content slots
- labels
- descriptions
- text input controls
- ports
- future extensible content blocks if needed

### M1.8 - Define Zoom and Level-of-Detail Rules

Establish how the editor decides when to display:

- summary content
- detail content
- toolbox-oriented representations

Clarify whether thresholds and switching rules are core-defined, configurable, or both.

### M1.9 - Define Node Creation Flow

Define the shared creation model behind:

- toolbox-based creation
- right-click add creation

Clarify the expected creation pipeline and the contracts involved in turning an available node definition into a node instance.

### M1.10 - Define Initial Interaction Ownership

Decide which interaction behaviors are part of the core editor model, such as:

- selection
- pan/zoom
- node movement
- connection creation
- inline editing
- resize
- expand/collapse

## M2 - Core Model Foundations

Establish the first implementation-ready shared model for:

- graph contracts
- node contracts
- ports and connections
- rendering-facing descriptions
- editor state contracts

## M3 - First Editor Slice

Deliver the first usable editor workflow with:

- available node definitions
- node creation
- graph placement
- node rendering
- port rendering
- connection rendering
- basic interaction

## M4 - Platform Integration and Validation

Deliver thin platform integration layers and test-bed validation to prove that the same Arctos.Nodes programming model can be hosted consistently across targets.

## M5 - Expansion and Hardening

Expand the capability catalog and refine the editor through:

- richer node content
- improved interaction behavior
- performance tuning
- rendering refinement
- usability polish
- cross-application validation
