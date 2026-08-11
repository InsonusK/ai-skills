---
description: Short description what must be made while creation or change in a package
name: # Package name (as declared in package.json "name")
element_kind: # repository | package | class | module | index
change_kind: # create | extend
# - create if solution creates a new package template. Name of the package must be added into the `creates` property in the header of the solution.
# - extend if solution extends an existing package template. Link to the package must be added into the `extends` property in the header of the solution.
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
- Give the package a Cucumber/coverage/mutation-gated test suite
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
- Step definitions call the package's exported functions/classes directly, never a re-implementation of them
```

# Structure

## Project Structure
```hint
Define how solution EXTENDS package structure.
```
```example
/{package-name}
  /src
    index.ts
    {ClassName}.ts
  /features
    {rule}.feature
  /step-definitions
    {rule}.steps.ts
```

## Directory and class skills
```hint
Define how solution EXTENDS package directory and files.
```
```example
| Directory | file   | Description           |
| ------------------- | --------------------- |
| /src      | index.ts | Public API surface |
| /features | {rule}.feature | Cucumber scenarios for {rule} |
```

| Directory | file | Description |
| ----------------- | ----------- |
|                   |             |

# npm Packages
```hint
Define how solution EXTENDS package npm dependencies.
```
```example
| Package   | Version constraint | Purpose                |
| --------- | ------------------ | ---------------------- |
| @cucumber/cucumber | ^10 | Run Gherkin scenarios against step definitions |
```

| Package | Version constraint | Purpose |
| ------- | ------------------ | ------- |
|         |                    |         |

# What Does NOT Belong Here
```hint
Define how solution EXTENDS package components which do not belong to it.
RECOMMENDATION:
- Prefer bullet list
```
```example
- Shared Gherkin `.feature` files across multiple implementations - belong to the conformance-spec package/repository, not here
```

# Allowed Dependencies
```hint
Define how solution EXTENDS allowed dependencies that package may have.
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
Define how solution EXTENDS package MUST, SHOULD, MAY, SHOULD NOT, MUST NOT rules.
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
Describe concrete wrong ways to apply the solution to this package and their consequences.
Each item must tell the agent what NOT to do, why it is harmful, and what to do instead.

Format:
- **{What NOT to do}**
  - Consequence: {negative consequence}
  - Instead: {correct alternative}
```
```example
- **Import a sibling package's internal module path instead of its `index.ts`**
  - Consequence: couples this package to internal structure that is free to change
  - Instead: import only through the dependency's published `index.ts`
```

# Check list
```hint
Define how solution EXTENDS package check list.
RECOMMENDATION:
- Prefer checkbox list
```
```example
- [ ] `package.json` declares `test`, `coverage`, `mutation` scripts
```
