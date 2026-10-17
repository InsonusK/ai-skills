---
name: architecture catalog in one directory
description: Where the reworked architecture catalog (base plateau plus attachable variation points) is built while the old plateau catalogs stay in use
problem: The rework of the plateau approach is large and earlier attempts broke the existing catalogs midway; where are the new skills built so that the old catalogs keep working until the new one replaces them?
decision: Build the new catalog in its own directory, library/architecture/, with stack-agnostic skills in spec/ and stack extensions in {stack}/, and leave the old catalogs untouched until it replaces them.
tags:
  - stack
  - concern/documentation
  - concern/documentation/adr
---

# Problem
The plateau catalogs (`library/{stack}/architecture/`, `library/common-workflow/architecture/design/plateau-map/`) describe one plateau per combination of features. The rework replaces them with one base plateau plus variation points. Projects still use the old catalogs, and earlier rework attempts left them broken halfway.

# Selected variant
[[#Own directory, spec and stack folders]]
- Owner decision, 2026-10-14: build the new approach separately and remove the old one in one pass once the new one works.

# Searched variants

## Own directory, spec and stack folders

**Selected.**

### Description
`library/architecture/spec/` for stack-agnostic skills, `library/architecture/{stack}/` for `-in-{stack}` extensions. The location is final: when the new catalog replaces the old ones, only the old directories are deleted. `spec` rather than `core` because a spec skill alone cannot be applied — it says what a unit guarantees, its stack skill says how to build it.

### Benefits
- The old catalogs keep working, unchanged, for the whole rework.
- A project takes one stack with one `subpath`; the spec skills follow through links.
- No move later: links keep their paths when the old catalogs are deleted.

### Costs
- Two catalogs exist side by side for the duration; a project must take one of them, not both.
- `spec/` differs from the `core/` name used in `library/testing/` and `library/devops/`.

## Rework the old catalogs in place

### Description
Change `library/{stack}/architecture/` and the plateau-map skills step by step.

### Benefits
- One catalog at any time.

### Costs
- Every intermediate state is what projects sync; a half-done step breaks them.

## Temporary directory, moved at the end

### Description
Build in a scratch directory and move into `library/{stack}/architecture/` when done.

### Benefits
- The final layout matches the old one.

### Costs
- Every link is rewritten on the move.
