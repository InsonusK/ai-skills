---
description: Short description what must be made while creation or change in project
name: # Project name
element_kind: # repository | project | class
change_kind: # create | extend
# - create if solution creates a new project template. Name of the project must be added into the `creates` property in the header of the solution.
# - extend if solution extends an existing project template. Link to the project must be added into the `extends` property in the header of the solution.
tags:
  - solution/{solution-name}
  - element/{element-name}
  # solution/{solution-name}: the owning solution name without the `solution-` prefix, kebab-case.
  # element/{element-name}: the project name in kebab-case, no braces or dots
  # (e.g. Shared.csproj -> element/shared-csproj, {Module}.Domain.csproj -> element/module-domain-csproj).
---

# How Apply this template
- Replace all `hint`, `example` and `code example` blocks with real content. Do not keep them in the final skill file.
- If a section does not introduce any changes for this project, remove the section or add a note that no changes are introduced.

# Goals
```hint
Define how solution EXTENDS project goal.
MUST:
- show all added goals
RECOMMENDATION:
- Prefer bullet list
```
```example
- Encapsulate domain logic
```

# Core Principles
```hint
Define how solution EXTENDS project core principles.
MUST:
- show all added Core Principles
RECOMMENDATION:
- Prefer bullet list
```
```example
- Entities define consistency.
```

# Structure

## Project Structure
```hint
Define how solution EXTENDS project structure.
```
```example
/ProjectName
  /DirectoryName
    ClassesInDirectory.cs
```

## Directory and class skills
```hint
Define how solution EXTENDS project directory and files.
```
```example
| Directory | file   | Description           |
| ------------------- | --------------------- |
| /DirectoryName      | Directory description |
| ClassInDirectory.cs | Class description     |
```

| Directory | file | Description |
| ----------------- | ----------- |
|                   |             |

# NuGet Packages
```hint
Define how solution EXTENDS project NuGet dependencies.
```
```example
| Package   | Version constraint | Purpose                |
| --------- | ------------------ | ---------------------- |
| Ardalis   | >= 8.0             | SpecificationEvaluator |
```

| Package | Version constraint | Purpose |
| ------- | ------------------ | ------- |
|         |                    |         |

# What Does NOT Belong Here
```hint
Define how solution EXTENDS project components which do not belong to it.
RECOMMENDATION:
- Prefer bullet list
```
```example
- Commands - belong to [[Other csproj skill]]
```

# Allowed Dependencies
```hint
Define how solution EXTENDS allowed dependencies that project may have.
RECOMMENDATION:
- Prefer bullet list
ATTENTION:
- Solution should not change allowed dependencies. Confirm extension from user before adding.
```
```example
- [[Shared]]
```

# Rules
```hint
Define how solution EXTENDS project rules. Follow the Rule-section baseline in [[skills/common-workflow/skill-design.skill/skill-design.skill.md|skill-design]]:
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
- Never reference DbContext directly from Application.
  - Risk: couples orchestration to persistence and breaks testability.
  - Fix: use `IRepository<T>` from Shared.
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
Define how solution EXTENDS project check list.
RECOMMENDATION:
- Prefer checkbox list
```
```example
- [ ] `int Id` with `internal set` present
```
