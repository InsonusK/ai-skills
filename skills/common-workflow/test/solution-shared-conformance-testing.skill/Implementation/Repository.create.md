---
description: Shared conformance-spec project holding only .feature files, reused across components
element_kind: repository
change_kind: create
tags:
  - solution/component-conformance-testing
  - element/repository
---

# Structure

## Project Structure
```
/{Component}.ConformanceSpec
  /Rules
    {Rule}.feature
```

Each consuming component's own test project references this directory/project and supplies its own step definitions there, binding these shared scenarios to that component's real implementation. The shared project itself never contains step definitions or production code — see the parent solution's Core Principles.

## Directory and class skills
| Directory | file | Description |
| --- | --- | --- |
| /Rules | {Rule}.feature | A scenario group proven identically by every consuming component |

# Rule

## MUST
- Contain only `.feature` files in this project — no step definitions, no production code, no stack-specific implementation beyond what packaging the directory as a referenceable project requires.
  - Risk: any implementation code here ties the shared spec to one component's stack/tooling, defeating reuse by components on a different stack.
  - Fix: keep this project to `.feature` files only; every consuming component supplies its own step definitions in its own project.

# Check list
- [ ] Only `.feature` files exist in this project.
- [ ] No step definitions or production code live here.
