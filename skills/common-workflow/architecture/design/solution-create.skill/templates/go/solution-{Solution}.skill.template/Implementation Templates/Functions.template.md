---
description: Short description what must be made while creation or change in a file that holds standalone functions, interfaces, or package-level values (no dominant struct)
project_name: # The package in which the file is located
name: # File name, e.g. config or quiz (without .go)
element_kind: functions
change_kind: # create | extend
# - create if solution creates a new file. Name of the file must be added into the `creates` property in the header of the solution.
# - extend if solution extends an existing file. Link to the file must be added into the `extends` property in the header of the solution.
tags:
  - solution/{solution-name}
  - element/{element-name}
  # solution/{solution-name}: the owning solution name without the `solution-` prefix, kebab-case.
  # element/{element-name}: the file path in kebab-case, no braces or dots
  # (e.g. internal/domain/interfaces/quiz.go -> element/internal-domain-interfaces-quiz-go).
---

# How Apply this template
- Replace all `hint`, `example` and `code example` blocks with real content. Do not keep them in the final skill file.
- If a section does not introduce any changes for this file, remove the section or add a note that no changes are introduced.

# Goals
```hint
Define how solution EXTENDS this file's goal.
MUST:
- show all added Goals
RECOMMENDATION:
- Prefer bullet list
```
```example
- Declare the domain's outbound port(s) for this capability, so the domain can depend on behavior without depending on which infrastructure provides it
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
- Every declaration here is business-named, not technology-named (e.g. `ChatStore`, never `RedisStore`, as the interface's own name)
```

# Naming convention
```hint
File and identifier naming convention. Fill table:
- use case - when apply naming convention
- identifier name pattern - mask of the function/interface/var name. Example: {Entity}Store
- identifier name - example. Example: ChatStore
- file name pattern - file name pattern. Example: {concept}.go
- file name - example of file name. Example: quiz.go
```

| use case | identifier name pattern | identifier name | file name pattern | file name |
| -------- | ------------------------ | ---------------- | ------------------- | --------- |
|          |                          |                  |                     |           |

# Implementation changes
```hint
Define how solution EXTENDS this file's implementation.
```
```example
[[File skill]] must declare the `{Port}` interface this solution's adapter implements.
```
```code example
// Notifier is the outbound port for pushing content to whatever channel a
// chat is reachable on.
type Notifier interface {
	SendMessage(ctx context.Context, chatID int64, text string) error
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
- Every port interface declared here must be named for what the domain needs, never for the technology that will implement it.
  - Risk: a technology-named port (e.g. `RedisStore`) leaks an infrastructure decision into the domain layer, so swapping the technology means renaming a domain-level type.
  - Fix: name the interface after its business purpose; let the adapter's own package name carry the technology.
- Never return a technology-specific error type from a port method's signature.
  - Risk: every caller of the port would need to import and understand the underlying technology's error package, defeating the port's purpose.
  - Fix: declare sentinel errors in this file and have every adapter translate its own errors into them.
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
- [ ] The new interface has no method beyond what a real caller in `internal/domain/services` needs today
- [ ] Every sentinel error this file declares is used by at least one adapter's translation logic
```

# Unittest TestCases
```hint
Define how solution EXTENDS this file's unit tests.
RECOMMENDATION:
- Prefer checkbox list
```
```example
- [ ] WHEN a consumer of the interface is given a fake implementation THEN it compiles without referencing any concrete adapter
```
