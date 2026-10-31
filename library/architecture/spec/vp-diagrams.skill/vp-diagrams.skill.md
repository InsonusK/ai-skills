---
name: vp-diagrams
description: How a variation point of the architecture catalog is drawn — mermaid only for what flows or connects, the concept in its spec skill without classes or a language, one call of the example and its classes in each stack extension with their real names, a text tree only for a file layout, and every diagram parsed before it is committed
whenToUse: when you create or revise a variation point's spec or stack skill, or draw or review a diagram of one
updated: 20261031
tags:
  - stack
  - concern/architecture
  - concern/documentation
---

# Goal
- **A variation point read at a glance** - A reader sees from its diagrams how a variation point works before reading its rules: the concept in the spec skill, one real call and the classes that make it in each stack extension.

# Core Principle
- **Two levels, one tool** - The spec skill shows the idea with no language in it; the stack extension shows the same idea as the example really runs it. Both are mermaid, so they read alike, diff as text, and are drawn the way [mermaid-diagram](library/common-workflow/mermaid-diagram.skill.md) says.

# Rule

## MUST

### Draw the concept in the spec skill
Draw in `spec/{vp}.skill/diagrams/concept.mmd` the parts of the variation point and how a call passes them, naming roles (service, adapter, fixed code) and no class, package, or language, and import it in Core Principle: a `flowchart` for how the parts connect, a `sequenceDiagram` when the order of the steps is the idea.
- Violation: a class name in a spec diagram; an ASCII scheme of a flow.
- Risk: a concept tied to one stack reads as that stack's rule; a scheme in ASCII cannot be rendered and drifts in width with every edit.
- Fix: `vp-client-retry`'s `concept.mmd`: a `sequenceDiagram` of the call, the loop, and the outcome.

### Draw one call of the example in each stack extension
Draw in `{stack}/{vp}-in-{stack}.skill/diagrams/example-flow.mmd` a `sequenceDiagram` of one call of the example, numbered, with activations, each participant the real class and the file that holds it, relative to the example's root (`participant Retry as retry.Middleware<br/>internal/infrastructure/client/middleware/retry/retry.go`), its error paths as notes at the part that decides them, and import it in the Example section.
- Violation: names that are not in the example; every branch drawn as its own arrows through every layer.
- Risk: a diagram without its files leaves the reader searching for the code it describes; a diagram whose names are not the code's sends them after classes that do not exist; a diagram of every branch is read by nobody.
- Fix: one path through the parts; the outcomes are notes.

### Draw the classes of the example in each stack extension
Draw in `{stack}/{vp}-in-{stack}.skill/diagrams/example-classes.mmd` a `classDiagram` of the types the variation point adds or uses, grouped by package with `namespace`, each with the members a reader calls, with who uses, implements, and calls what, and import it after `example-flow.mmd`; give a type whose name repeats in two packages a diagram id of its own and its real name as the label (`class HttpClient["Client"]`).
- Violation: a type that is not in the example; every private field and helper drawn; a dot in a `namespace` name, which mermaid does not parse (`Client_Http`).
- Risk: the sequence shows the order of the calls, not where the code lives, so a reader cannot go from the picture to the class to change.
- Fix: search the example for every type and member of the diagram before committing it.

### Keep a text tree for a file layout
Write the layout of a variation point's files as a text tree in a code block, not as a diagram.
- Risk: a file tree in mermaid is a picture of what a tree already says, and it takes more lines to change.
- Fix: everything that flows or connects is mermaid; only `path  what it holds` lines stay text.

### Parse every diagram before committing it
Parse each new or changed `.mmd` with mermaid's own parser.
- Risk: a diagram that does not parse shows an error where the reader expects the picture, and no scenario catches it.
- Fix: `mermaid.parse` of each file, or the preview of Markdown Preview Enhanced.

# Check list
- [ ] The spec skill imports `diagrams/concept.mmd`, with roles and no class names.
- [ ] Each stack extension imports `diagrams/example-flow.mmd` in Example; each participant names its real class and its file.
- [ ] Each stack extension imports `diagrams/example-classes.mmd` after it; every type and member on it is in the example.
- [ ] No flow is drawn in ASCII; only file layouts stay text trees.
- [ ] Every diagram parses.
