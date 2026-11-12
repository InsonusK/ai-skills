---
name: conformance scenarios copied into each stack
description: How every stack's logging variation point runs one set of scenarios — each stack writing its own, a shared folder referenced by path, or copies of one source checked by a script
problem: Masking is written once per stack (Go, dotnet, Python), and a form of payload one stack forgets leaks every secret of that form; how is every stack made to prove the same guarantees, so that a gap found in one closes in all?
decision: The scenarios live once in spec/vp-logging.skill/conformance/ (the .feature files and the masking.proto they give as input); each stack skill names in conformance.copy where its example takes a copy; make conformance-sync writes the copies and test/check-conformance.sh, run on every pull request to master, fails while one is missing or differs.
tags:
  - stack
  - concern/architecture
  - concern/testing
  - concern/documentation
  - concern/documentation/adr
---

# Problem
The Go variation point had its own scenarios. Reading them against the code for dotnet found three leaks Go's scenarios never asked about (a form body, a JSON text, a proto `Any`). Each stack writing its own scenarios repeats that: the stack whose author missed a form is the one that leaks it. [solution-shared-conformance-testing](library/testing/core/solution-shared-conformance-testing.skill/solution-shared-conformance-testing.skill.md) moves such scenarios into one project every component references.

# Selected variant
[[#One source, copies checked by a script]]
- Owner decision, 2026-10-22: one set of feature files for the critical behaviour, run by every stack; a make command copies them and the pull-request check fails on a skill whose copies are missing.

# Searched variants

## One source, copies checked by a script

**Selected.**

### Description
`spec/vp-logging.skill/conformance/` holds the `.feature` files and `masking.proto`, the messages the scenarios give as a payload. A stack skill's `conformance.copy` names the folder its example takes a copy into; `make conformance-sync` writes every copy; `test/check-conformance.sh` (in `test/test.sh`, run by the pull-request workflow) fails while a copy is missing or differs. A stack adds files of its own beside the copies for the ways only it has.

### Benefits
- An example stays whole: copied into a project, it runs its scenarios with no path back into this repository.
- A scenario added to the source fails the check of every stack until each has taken it — and so until each passes it.

### Costs
- The copies are files in each example; a reviewer edits the source, never a copy.
- Unlike [solution-shared-conformance-testing](library/testing/core/solution-shared-conformance-testing.skill/solution-shared-conformance-testing.skill.md), the folder holds one non-`.feature` file: `masking.proto` is the scenarios' input, not a step definition, and a hand-built copy of it per stack would drift.

## A shared folder referenced by path

### Description
Each example's runner reads `../../../../spec/vp-logging.skill/conformance` directly.

### Benefits
- No copies.

### Costs
- An example copied into a project breaks: the path points out of the project.

## Scenarios written by each stack

### Description
Each stack keeps its own scenarios, as before.

### Benefits
- Each stack words its steps in its own terms.

### Costs
- The guarantees drift; the leaks found in Go would have been missed again in dotnet and Python.
