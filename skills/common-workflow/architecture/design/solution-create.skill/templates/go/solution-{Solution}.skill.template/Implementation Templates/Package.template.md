---
description: Short description what must be made while creation or change of a Go package (a directory under cmd/ or internal/)
name: # Package path, e.g. internal/domain/services or internal/infrastructure/redisstore
element_kind: package
change_kind: # create | extend
# - create if solution creates a new package. The package path must be added into the `creates` property in the header of the solution.
# - extend if solution extends an existing package. Link to the package must be added into the `extends` property in the header of the solution.
tags:
  - solution/{solution-name}
  - element/{element-name}
  # solution/{solution-name}: the owning solution name without the `solution-` prefix, kebab-case.
  # element/{element-name}: the package path in kebab-case, no braces or dots
  # (e.g. internal/infrastructure/redisstore -> element/internal-infrastructure-redisstore).
---

# How Apply this template
- Replace all `hint`, `example` and `code example` blocks with real content. Do not keep them in the final skill file.
- If a section does not introduce any changes for this package, remove the section or add a note that no changes are introduced.

# Goals
```hint
Define how solution EXTENDS package goal.
MUST:
- show all added goals
RECOMMENDATION:
- Prefer bullet list
```
```example
- Give the domain a durable, business-named place to store and reload the entities this module owns
```

# Core Principles
```hint
Define how solution EXTENDS package core principles.
MUST:
- show all added Core Principles
RECOMMENDATION:
- Prefer bullet list
```
```example
- The package implements a port declared in `internal/domain/interfaces`; nothing outside this package and the port's own declaration knows which storage technology backs it
```

# Structure

## Repository place
`Where this package lives in the repository`
```example
internal/
  infrastructure/
    {adapter}/
```

## Package Structure
```hint
Define how solution EXTENDS package structure (the files inside this package directory).
```
```example
internal/infrastructure/{adapter}/
  client.go
```

## Directory and file skills
```hint
Define how solution EXTENDS the table of files inside this package.
```
```example
| Directory\|file | Description | Pattern skill |
| --------------- | ----------- | -------------- |
| client.go       | `Client` struct implementing the domain's outbound port | [[link to struct pattern]] |
```

| Directory\|file | Description | Pattern skill |
| --------------- | ----------- | -------------- |
|                 |             |                |

# Go Dependencies
```hint
Define how solution EXTENDS this package's module/standard-library dependencies.
```
```example
| Module | Version constraint | Purpose |
| ------ | ------------------- | ------- |
| google.golang.org/grpc | >= 1.83 | client connection to the external service |
```

| Module | Version constraint | Purpose |
| ------ | ------------------- | ------- |
|        |                     |         |

# What Does NOT Belong Here
```hint
Define how solution EXTENDS the list of components which do not belong in this package.
RECOMMENDATION:
- Prefer bullet list
```
```example
- Business logic — belongs to [[internal/domain/services package skill]]
```

# Allowed Dependencies
```hint
Define how solution EXTENDS the allowed-imports list for this package.
RECOMMENDATION:
- Prefer bullet list
ATTENTION:
- Solution should not change allowed dependencies of an existing package. Confirm the extension with the user before adding.
```
```example
- `internal/domain/interfaces` (the port this package implements)
```

# Rules
```hint
Define MUST, SHOULD, MAY rules only — never `## MUST NOT`/`## SHOULD NOT` headings and never a separate `# Anti-patterns` section (see [[skills/design/skill-design.skill/skill-design.skill.md|skill-design]]). Only add a subblock for categories where this solution introduces new rules.

MUST:
- show all added Rules
```

## MUST
```example
- The package must implement its port's method set exactly — no exported method beyond what the port declares.
  - Risk: an adapter-specific method leaking out invites a caller to depend on the concrete adapter instead of the port, defeating the substitution this pattern exists for.
  - Fix: keep every exported method on the adapter type limited to the port's interface; put adapter-only helpers behind unexported functions.
- Never import a sibling `internal/infrastructure/*` package from here.
  - Risk: two adapters depending on each other couples storage/transport technologies that should be independently replaceable.
  - Fix: if two adapters must coordinate, do it through the domain layer that already depends on both ports.
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
Define how solution EXTENDS this package's check list.
RECOMMENDATION:
- Prefer checkbox list
```
```example
- [ ] The package's exported type satisfies its domain port (compile-time assertion or direct usage as that interface)
- [ ] No import of another `internal/infrastructure/*` package
```
