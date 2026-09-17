---
name: registry-repo-root
description: Conflict Detection result for the `repo-root` element
tags:
  - concern/architecture
  - stack/go
  - element/repo-root
---

# Element
`repo-root`

# Involved solutions
- [[skills/go/architecture/solutions/solution-go-repository-structure.skill/solution-go-repository-structure.skill.md|solution-go-repository-structure]] (`.create`)
- [[skills/go/architecture/solutions/solution-go-conformance-testing.skill/solution-go-conformance-testing.skill.md|solution-go-conformance-testing]] (`.extend`)

# Classification
`FMN` — **F**: no Constraint. **M**: `solution-go-conformance-testing` adds new `Makefile` targets (`unit-test`/`mutation-test`/`test-report`/`test-and-report`) and a new file (`report-template/index.html`) on top of `solution-go-repository-structure`'s `go.mod`/`Makefile` (`build`/`run`/`lint`)/`.gitignore`. **N**: independent — disjoint `Makefile` target names, disjoint files.

# Ordering
`source: ordering-only` — `solution-go-repository-structure` must exist first (there is no `Makefile` to extend otherwise); trivially satisfied since it is this catalog's foundational solution.

# Resolution
Canonical — no resolver needed. New targets appended to an existing `Makefile`, and a new standalone file, do not collide with anything `solution-go-repository-structure` created.
