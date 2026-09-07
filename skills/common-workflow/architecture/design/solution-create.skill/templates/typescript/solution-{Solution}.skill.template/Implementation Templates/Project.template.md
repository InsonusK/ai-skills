---
description: Short description what must be made while creation or change in a package
name: # Package name (as declared in package.json "name")
element_kind: # repository | package | class | module | index
change_kind: # create | extend
# - create if solution creates a new package template. Name of the package must be added into the `creates` property in the header of the solution.
# - extend if solution extends an existing package template. Link to the package must be added into the `extends` property in the header of the solution.
tags:
  - solution/{solution-name}
  - element/{element-name}
  # solution/{solution-name}: the owning solution name without the `solution-` prefix, kebab-case.
  # element/{element-name}: the package name in kebab-case, no braces or dots
  # (e.g. {Package}/package.json -> element/package-package-json).
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
Define how solution EXTENDS package rules. Follow the Rule-section baseline in [[skills/common-workflow/skill-design.skill/skill-design.skill.md|skill-design]]:
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
- Never import a sibling package's internal module path instead of its `index.ts`.
  - Risk: couples this package to internal structure that is free to change.
  - Fix: import only through the dependency's published `index.ts`.
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
Define how solution EXTENDS package check list.
RECOMMENDATION:
- Prefer checkbox list
```
```example
- [ ] `package.json` declares `test`, `coverage`, `mutation` scripts
```
