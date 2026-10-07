---
name: multi-stack-solution-location
description: Where the stack-agnostic base of a multi-stack solution and its `-in-{stack}` extensions live, and what the base's Implementation/ holds
problem: skill-stack-split requires a skill whose implementation differs across stacks to be split into a stack-agnostic base plus `-in-{stack}` extensions and leaves their location to the category, but no folder exists for stack-agnostic architecture solutions, and a solution skill must always ship an Implementation/ folder even though most of its implementation is stack-specific.
decision: A multi-stack solution's base `solution-{name}` lives in skills/common-workflow/architecture/solutions/ and each extension `solution-{name}-in-{stack}` lives in skills/{stack}/architecture/solutions/. The base's Implementation/ holds only stack-independent elements, such as a contract or a Makefile target set, and every stack-specific element lives in the extension.
tags:
  - stack
  - concern/documentation
  - concern/documentation/adr
---

# Problem

[[skills/design/skill-stack-split.skill/adr/stack-agnostic-base-and-extensions.md|stack-agnostic-base-and-extensions]] leaves the location of a stack-agnostic base and its extensions to the category. For solution skills this leaves two open questions:

- which folder holds architecture solution bases, given that only `common-workflow/test/` holds a base today (`solution-conformance-testing`);
- what the base's mandatory `Implementation/` holds when most of the implementation is stack-specific.

# Selected variant

**Selected variant:** [[#`common-workflow/architecture/solutions/`, stack-independent Implementation only]]

# Searched variants

## `common-workflow/architecture/solutions/`, stack-independent Implementation only

**Selected.**

### Description
Bases go in `skills/common-workflow/architecture/solutions/`, which mirrors `skills/{stack}/architecture/solutions/`. Extensions stay in the stack's own `architecture/solutions/`. The base's `Implementation/` holds only elements every stack shares, such as a contract or a Makefile target set. Stack-specific elements live in the extension.

### Benefits
- The same relative path on both sides, so the base of `skills/go/architecture/solutions/solution-x-in-go` is found at `skills/common-workflow/architecture/solutions/solution-x`.
- Solutions stay separate from the design-process skills in `common-workflow/architecture/design/`.
- Matches the precedent of `solution-conformance-testing`, whose base carries the stack-independent Makefile and report contract.

### Costs
- The folder is new and starts empty.
- Test-area bases such as `solution-conformance-testing` stay in `common-workflow/test/`, so solution bases live in two places.

## Bases under `common-workflow/architecture/design/`

### Description
Put solution bases next to `solution-create` and the other design skills.

### Benefits
- No new folder.

### Costs
- Mixes skills that design solutions with the solutions themselves.
- Does not mirror the stack folders' `architecture/solutions/` layout.
- Rejected.

## Base with no Implementation/

### Description
The base holds only rules, and all Implementation files live in the extensions.

### Benefits
- There is no question about what counts as stack-independent.

### Costs
- Violates solution-create's rule that no solution ships without its Implementation.
- Shared contracts, such as Makefile targets, get copied into every extension.
- Rejected.
