---
name: devops-skills-in-one-directory
description: Where DevOps skills live — every stack's CI, release, and deploy skills under `skills/devops/`, the stack-agnostic ones in `core/`, `workflows/`, and `deploy/`, each stack's in `{stack}/`
problem: DevOps skills were split between `skills/devops/workflows/` and `skills/{stack}/devops/`, so one CI process was read in two or more directories, and a workflow named actions that lived with another stack's files. Where should DevOps skills live?
decision: All DevOps skills live under `skills/devops/` — stack-agnostic skills in `core/`, `workflows/`, `deploy/`, `{skill-name}-in-{stack}` in `{stack}/`; a skill there links outside the directory only to `skills/testing/` and `skills/design/`.
tags:
  - stack
  - concern/documentation
  - concern/documentation/adr
---

# Problem
A workflow skill in `skills/devops/workflows/` relied on composite actions described in `skills/{stack}/devops/`, one folder per stack. Reading one CI process meant reading several directories, the same as testing before [[./testing-skills-in-one-directory.md|testing-skills-in-one-directory]]. Where should DevOps skills live?

# Selected variant
[[#One directory, core plus one folder per stack]]
- Decided by the owner on 2026-10-10, to match `skills/testing/`.

# Searched variants

## One directory, core plus one folder per stack

**Selected.**

### Description
`skills/devops/core/`, `workflows/`, and `deploy/` hold stack-agnostic skills; `skills/devops/{stack}/` holds every `{skill-name}-in-{stack}`. A DevOps skill links outside `skills/devops/` only to `skills/testing/`, whose contract its workflows call, and to `skills/design/`.

### Benefits
- One directory to read for a CI process.
- A project lists `skills/devops/core`, `skills/devops/workflows`, and `skills/devops/{stack}` and gets exactly its DevOps set.
- The same placement as testing, so the two concerns that meet in a workflow are found the same way.

### Costs
- A second concern not placed by stack.
- A base and its extensions are not adjacent; the shared name ties them.

## Keep DevOps skills by stack

### Description
The status quo: workflows in `skills/devops/workflows/`, stack actions in `skills/{stack}/devops/`.

### Benefits
- One placement rule for stack-specific skills.

### Costs
- A CI process is read across directories.
- Nothing marks the DevOps skills as one set.
