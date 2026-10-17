---
name: develop skills in one directory
description: Where the skills about writing application code live — the layer split, decomposition, logging, API and client rules
problem: The layer-split skill (app-layers) applies to every application in every stack, not only to the plateau catalog; where does it live, and where do the other development skills go?
decision: library/develop/, with stack-agnostic skills in core/ and -in-{stack} extensions in {stack}/, the same shape as library/testing/; new development skills are created there, and those in library/common-workflow/develop/ move there later.
tags:
  - stack
  - concern/documentation
  - concern/documentation/adr
---

# Problem
`app-layers` states how any application is structured; the architecture catalog's `base-plateau` and every variation point build on it, but so does code written outside any plateau. The existing development skills sit in `library/common-workflow/develop/` without a stack split.

# Selected variant
[[#library/develop with core and stack folders]]
- Owner decision, 2026-10-17: the layer skill is general and lives apart from the plateau work; the skills in `library/common-workflow/develop` move to `library/develop` later.

# Searched variants

## library/develop with core and stack folders

**Selected.**

### Description
`library/develop/core/{skill-name}` and `library/develop/{stack}/{skill-name}-in-{stack}`, created now in their final place. `core`, not `spec`: a development skill applies on its own; its stack extension only makes it concrete.

### Benefits
- One `subpath` gives a project every development rule; one stack folder gives one stack's details.
- The final location from the start: no links to rewrite when the old folder empties.
- Same shape as `library/testing/` and `library/devops/`.

### Costs
- Until the move, development skills live in two folders.

## Inside the architecture catalog (library/architecture/spec)

### Description
Next to `base-plateau`, moved out later.

### Benefits
- Built together with the plateau work it serves first.

### Costs
- A general rule sits in a catalog about plateaus, and moving it later rewrites every link to it.

## library/common-workflow/develop

### Description
Next to the existing development skills.

### Benefits
- One folder today.

### Costs
- The folder is to be emptied; adding to it adds to the later move, and it has no stack split.
