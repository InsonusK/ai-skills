---
description: Short description what must be made while creation or change in Angular project
name: # Project name
element_kind: # repository | project | class
change_kind: # create | extend
# - create if solution creates a new project template. Name of the project must be added into the `creates` property in the header of the solution.
# - extend if solution extends an existing project template. Link to the project must be added into the `extends` property in the header of the solution.
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
- Encapsulate feature routing
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
- Feature modules are lazy-loaded
```

# Structure

## Project Structure
```hint
Define how solution EXTENDS project structure.
```
```example
/{App}
  /src
    /app
      /features
        /{Feature}
          {Feature}.component.ts
          {Feature}.service.ts
```

## Directory and file skills
```hint
Define how solution EXTENDS project directory and files.
```
```example
| Directory | file   | Description           |
| ------------------- | --------------------- |
| /src/app/features   | feature modules       |
| {Feature}.component.ts | UI component       |
```

| Directory | file | Description |
| ----------------- | ----------- |
|                   |             |

# NPM Packages
```hint
Define how solution EXTENDS project NPM dependencies.
```
```example
| Package   | Version constraint | Purpose                |
| --------- | ------------------ | ---------------------- |
| @angular/core | >= 17.0        | Framework core         |
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
- Domain logic - belongs to backend services
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
Define how solution EXTENDS project MUST, SHOULD, MAY, SHOULD NOT, MUST NOT rules.
Only add a subblock for categories where this solution introduces new rules.
If a category has no new rules, skip it — do not write an empty subblock.

MUST:
- show all added Rules
```

## MUST
```example
- ...
```

## SHOULD
```example
- ...
```

## MAY
```example
- ...
```

## SHOULD NOT
```example
- ...
```

## MUST NOT
```example
- ...
```

# Anti-patterns
```hint
Describe concrete wrong ways to apply the solution to this project and their consequences.
Each item must tell the agent what NOT to do, why it is harmful, and what to do instead.

Format:
- **{What NOT to do}**
  - Consequence: {negative consequence}
  - Instead: {correct alternative}
```
```example
- **Import feature modules eagerly in AppModule**
  - Consequence: increases initial bundle size
  - Instead: lazy-load feature modules via router
```

# Check list
```hint
Define how solution EXTENDS project check list.
RECOMMENDATION:
- Prefer checkbox list
```
```example
- [ ] `angular.json` is updated if needed
```
