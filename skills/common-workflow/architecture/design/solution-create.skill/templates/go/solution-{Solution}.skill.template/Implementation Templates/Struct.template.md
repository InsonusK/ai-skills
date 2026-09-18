---
description: Short description what must be made while creation or change in a file organized around one struct and its methods
project_name: # The package in which the file is located
name: # Struct name, e.g. Server, Client, Store
element_kind: struct
change_kind: # create | extend
# - create if solution creates a new struct file. Name of the file must be added into the `creates` property in the header of the solution.
# - extend if solution extends an existing struct file. Link to the file must be added into the `extends` property in the header of the solution.
tags:
  - solution/{solution-name}
  - element/{element-name}
  # solution/{solution-name}: the owning solution name without the `solution-` prefix, kebab-case.
  # element/{element-name}: the file path in kebab-case, no braces or dots
  # (e.g. internal/api/grpc/server.go -> element/internal-api-grpc-server-go).
---

# How Apply this template
- Replace all `hint`, `example` and `code example` blocks with real content. Do not keep them in the final skill file.
- If a section does not introduce any changes for this struct/file, remove the section or add a note that no changes are introduced.

# Goals
```hint
Define how solution EXTENDS this file's goal.
MUST:
- show all added Goals
RECOMMENDATION:
- Prefer bullet list
```
```example
- Translate gRPC calls into calls on the shared domain service
```

# Core Principles
```hint
Define how solution EXTENDS this file's core principles.
MUST:
- show all added Core Principles
RECOMMENDATION:
- Prefer bullet list
```
```example
- The struct holds only the dependencies it needs (typically an interface or a concrete domain-service pointer), constructed once in `main.go` and never re-constructed per call
```

# Naming convention
```hint
Struct and file naming convention. Fill table:
- use case - when apply naming convention
- struct name pattern - mask of the struct name. Example: {Entity}Store
- struct name - example of the struct name. Example: QuizStore
- file name pattern - file name pattern. Example: {entity}_store.go
- file name - example of file name. Example: quiz_store.go
```

| use case | struct name pattern | struct name | file name pattern | file name |
| -------- | -------------------- | ------------ | ------------------ | --------- |
|          |                      |              |                    |           |

# Implementation changes
```hint
Define how solution EXTENDS this file's implementation.

When this solution is built on a plateau (`built_on_plateau` is set), structure the change as a delta from the plateau:
- AS IS — copy or summarize the relevant implementation from the plateau's file skill.
- TO BE — show the implementation after the solution's changes.

When `built_on_plateau` is empty, describe the change directly without the AS IS/TO BE split.
```
```example
### AS IS
[[File skill]] currently has no `{Method}`.

### TO BE
[[File skill]] gains `{Method}`, calling the domain service and translating its result.
```
```code example
### TO BE
type Server struct {
	notifications *services.NotificationService
}

func New(notifications *services.NotificationService) *Server {
	return &Server{notifications: notifications}
}
```

# Rule changes
```hint
Define how solution EXTENDS this file's rules. Follow the Rule-section baseline in [[skills/design/skill-design.skill/skill-design.skill.md|skill-design]]:
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
- Every exported method's first parameter must be `ctx context.Context`, even when unused today.
  - Risk: adding cancellation/tracing later forces every call site's signature to change at once.
  - Fix: accept `ctx context.Context` from the first method onward.
- Never let the struct hold a second, independently-constructed copy of a dependency another adapter already holds.
  - Risk: two copies of the same domain service can drift (e.g. one wired with a mock, the other with the real dependency), so behavior differs by which entry point was called.
  - Fix: construct the dependency once in `main.go` and pass the same pointer to every adapter that needs it.
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
Define how solution EXTENDS this file's check list.
RECOMMENDATION:
- Prefer checkbox list
```
```example
- [ ] `Server` embeds the generated `Unimplemented*Server` type for forward compatibility
- [ ] Every method logs entry, success, and failure via the shared `log/slog` pattern
```

# Unittest TestCases
```hint
Define how solution EXTENDS this file's unit tests.
RECOMMENDATION:
- Prefer checkbox list
```
```example
- [ ] WHEN the domain service returns `ErrNotFound` THEN the method returns a `codes.NotFound` status
```
