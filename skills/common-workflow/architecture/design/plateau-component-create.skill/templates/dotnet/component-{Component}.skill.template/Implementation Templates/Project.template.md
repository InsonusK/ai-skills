---
description: Short description what must be made while creation or change in project
name: # Project name
element_kind: # project | class
change_kind: # create | extend
# - create if the component creates a new project (normally the component's own project). Name of the project must be added into the `creates` property in the header of the component.
# - extend if the component extends an existing project. This MUST be the composition-root project (e.g. App.Host) — never a {Module}.* project. Link to the project must be added into the `extends` property in the header of the component.
tags:
  - component/{component-name}
  - element/{element-name}
  # component/{component-name}: the owning component name without the `component-` prefix, kebab-case.
  # element/{element-name}: the project name in kebab-case, no braces or dots
  # (e.g. Logging.csproj -> element/logging-csproj, App.Host.csproj -> element/app-host-csproj).
---

# How Apply this template
- Replace all `hint`, `example` and `code example` blocks with real content. Do not keep them in the final skill file.
- If a section does not introduce any changes for this project, remove the section or add a note that no changes are introduced.
- If `change_kind: extend` and the project being extended is not the composition-root project, stop — a component's `Implementation/` may only create its own project or extend the composition-root project. Reaching into a `{Module}.*` project means this unit is a Solution, not a Component.

# Goals
```hint
Define what this project is for, or how the component extends the composition-root project's goal.
MUST:
- show all goals this project change introduces
RECOMMENDATION:
- Prefer bullet list
```
```example
- Single home for the logging pipeline behavior and its configuration
```

# Core Principles
```hint
Define the core principles this project follows.
MUST:
- show all added Core Principles
RECOMMENDATION:
- Prefer bullet list
```
```example
- Contains no reference to any `{Module}.*` project or type
```

# Structure

## Project Structure
```hint
Define this project's internal structure.
```
```example
/Logging
  LoggingBehavior.cs
  LoggingOptions.cs
```

## Directory and class skills
```hint
Define this project's directories and files.
```
```example
| Directory | file | Description |
| ----------------- | ----------- |
| LoggingBehavior.cs | MediatR pipeline behavior logging every request/response pair |
```

| Directory | file | Description |
| ----------------- | ----------- |
|                   |             |

# NuGet Packages
```hint
Define this project's NuGet dependencies.
```
```example
| Package  | Version constraint | Purpose |
| -------- | ------------------ | ------- |
| Serilog  | >= 3.0              | Structured logging sink |
```

| Package | Version constraint | Purpose |
| ------- | ------------------ | ------- |
|         |                    |         |

# What Does NOT Belong Here
```hint
Define what does not belong to this project.
RECOMMENDATION:
- Prefer bullet list
```
```example
- Anything referencing a `{Module}.*` type — belongs to a Solution, not this component
```

# Allowed Dependencies
```hint
Define allowed dependencies this project may have.
RECOMMENDATION:
- Prefer bullet list
ATTENTION:
- A component's own project may depend on Shared/BuildingBlocks-style cross-cutting primitives if the target built_on_plateau already provides them, but never on a specific module.
```
```example
- [[Shared]]
```

# Rules
```hint
Define rules for this project. Follow the Rule-section baseline in [[skills/common-workflow/skill-design.skill/skill-design.skill.md|skill-design]]:
- Use only ## MUST, ## SHOULD, ## MAY subblocks — never ## MUST NOT/## SHOULD NOT headings.
- Express a prohibition as a negatively-phrased bullet ("Never ...", "Do not ...") inside ## MUST or ## SHOULD, at whichever strength it actually carries.
- Every ## MUST bullet carries a nested `Risk:` and `Fix:` (`Violation:` is optional); ## SHOULD bullets carry the elaboration only when the rule is non-obvious; ## MAY bullets never carry it.
- Only add a subblock for categories where this project introduces new rules.
```

## MUST
```example
- Never reference a type from any `{Module}.*` project.
  - Risk: the component stops being attachable to a service composed from a plateau/Solution set that doesn't include that module, defeating the point of building it as a Component.
  - Fix: depend only on Shared/BuildingBlocks-style primitives already guaranteed by `built_on_plateau`.
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
What must be true before this project is considered correctly created/extended?
RECOMMENDATION:
- Prefer checkbox list
```
```example
- [ ] `Logging.csproj` referenced by `App.Host.csproj` only
- [ ] No `{Module}.*` project references `Logging.csproj`
```
