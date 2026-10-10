---
name: testing-skills-in-one-directory
description: Where testing skills live — every stack's testing skills under `skills/testing/`, the stack-agnostic ones in `core/` and each stack's in `{stack}/`, instead of under `skills/common-workflow/` and `skills/{stack}/`
problem: Testing skills were split by stack like every other skill, so tracing a failing test meant reading the stack-agnostic skill, the stack's skill, and other stacks' skills in three directories, with more testing rules restated inside architecture catalogs — where should testing skills live so one place answers how a program is tested?
decision: All testing skills live under `skills/testing/` — stack-agnostic `{skill-name}` in `core/`, `{skill-name}-in-{stack}` in `{stack}/`; every stack-specific testing skill carries `-in-{stack}`, base or not; a skill there links only inside `skills/testing/`.
tags:
  - stack
  - concern/documentation
  - concern/documentation/adr
---

# Problem
A skill's stack decided its directory: the base in `skills/common-workflow/test/`, each extension in `skills/{stack}/test/`. For testing this worked against how the skills are used (owner, 2026-10-07): when a test fails, the reader goes through the stack skill, the agnostic skill, and other stacks' skills for comparison — three directories — and part of the testing rules sat in architecture catalogs as well. The aim is that an agent applies the testing skills to any program in a stack and gets a stable result, which requires testing to be one self-contained set. Where should testing skills live?

# Selected variant
[[#One directory, core plus one folder per stack]]

# Searched variants

## One directory, core plus one folder per stack

**Selected.**

### Description
`skills/testing/core/` holds every stack-agnostic testing skill, `skills/testing/{stack}/` every skill of that stack. A stack-specific testing skill is named `{skill-name}-in-{stack}` whether or not a base exists, so a base and its extensions are matched by name across the folders. A skill under `skills/testing/` links only to files under `skills/testing/`; the direction rule between a base and its extensions is unchanged.

### Benefits
- One directory to read when a test fails.
- `ai-skill-manager` selects skills by path: a project lists `skills/testing/core` and `skills/testing/{stack}` and gets exactly its testing set.
- No link leaves the directory, so loading a testing skill never pulls in an architecture catalog, and the set can be applied to a program not built from one.

### Costs
- Testing is the one concern not placed by stack; the placement rule in skill-design carries an exception.
- A base and its extensions are not adjacent; the shared name is what ties them.
- A testing skill cannot link a skill outside the directory even when it relies on it; such a dependency is named as plain text or moved in.

## One directory, grouped by topic

### Description
`skills/testing/{skill-name}/` holding the base and every `-in-{stack}` extension side by side.

### Benefits
- A topic's base and all its stack variants are adjacent, and a missing variant is visible as a gap.

### Costs
- A project cannot select its testing set by path: every topic folder mixes all stacks, so each skill would be listed one by one.

## Keep testing skills by stack

### Description
The status quo: base in `skills/common-workflow/test/`, extensions in `skills/{stack}/test/`.

### Benefits
- One placement rule for every skill.

### Costs
- The three-directory trace described in the problem.
- Nothing marks the testing skills as one set, so links into architecture catalogs accumulate unnoticed.
