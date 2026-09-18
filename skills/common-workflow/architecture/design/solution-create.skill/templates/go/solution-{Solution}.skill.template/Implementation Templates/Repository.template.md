---
description: Short description what must be made while creation or change at the repository root
element_kind: repository
change_kind: # create | extend
# - create if solution creates repository-root content (go.mod, Makefile, cmd/, top-level layout).
# - extend if solution extends existing repository-root content.
tags:
  - solution/{solution-name}
  - element/{element-name}
  # solution/{solution-name}: the owning solution name without the `solution-` prefix, kebab-case.
  # element/{element-name}: a fixed, stable id for the repository root — use `repo-root`.
---

# How Apply this template
- Replace all `hint`, `example` and `code example` blocks with real content. Do not keep them in the final skill file.
- If a section does not introduce any changes at the repository root, remove the section or add a note that no changes are introduced.

# Structure

## Repository Structure
```hint
Define how solution EXTENDS repository-root structure (go.mod, Makefile, cmd/, top-level directories).
```
```example
go.mod
Makefile
cmd/
  {service}/
    main.go
```

## Directory and package skills
```hint
Define how solution EXTENDS repository directory/package layout.
```
```example
| Directory | file | Description |
| --------- | ---- | ----------- |
| cmd/{service} | main.go | Composition root: manual constructor wiring, no DI container |
```

| Directory | file | Description |
| --------- | ---- | ----------- |
|           |      |             |

# Rules
```hint
Define how solution EXTENDS repository-root rules. Follow the Rule-section baseline in [[skills/design/skill-design.skill/skill-design.skill.md|skill-design]]:
- Use only ## MUST, ## SHOULD, ## MAY subblocks — never ## MUST NOT/## SHOULD NOT headings.
- Express a prohibition as a negatively-phrased bullet ("Never ...", "Do not ...") inside ## MUST or ## SHOULD, at whichever strength it actually carries.
- Never add a separate # Anti-patterns section: convert each would-be anti-pattern into a negative bullet with nested `Risk:` (the consequence) and `Fix:` (the correct alternative).
- Every ## MUST bullet carries a nested `Risk:` and `Fix:` (`Violation:` is optional); ## SHOULD bullets carry the elaboration only when the rule is non-obvious; ## MAY bullets never carry it.
- Only add a subblock for categories where this solution introduces new rules.
- If a category has no new rules, skip it — do not write an empty subblock.

MUST:
- show all added Rules
```

## MUST
```example
- `main.go` must be the only place that wires concrete adapters into the domain service.
  - Risk: a second wiring site drifts from the first and the two stop agreeing on which adapters are live.
  - Fix: keep every `New*` constructor call inside `run()`/`main()`; nothing else calls them.
- Never place business logic in `main.go`.
  - Risk: the composition root starts enforcing business rules instead of just wiring.
  - Fix: keep business invariants in the domain layer.
```

## SHOULD
```example
- ...
```

## MAY
```example
- ...
```

# Check list
```hint
Define how solution EXTENDS repository-root check list.
RECOMMENDATION:
- Prefer checkbox list
```
```example
- [ ] `go.mod` lists the new module with a pinned version
- [ ] `main.go` constructs the new adapter and passes it to the existing domain service
```
