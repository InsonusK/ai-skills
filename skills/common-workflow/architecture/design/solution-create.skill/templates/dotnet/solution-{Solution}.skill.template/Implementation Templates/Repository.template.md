---
description: Short description what must be made while creation or change in repository
element_kind: # repository | project | class
change_kind: # create | extend
# - create if solution creates a new repository-level template.
# - extend if solution extends an existing repository-level template.
tags:
  - solution/{solution-name}
  - element/{element-name}
  # solution/{solution-name}: the owning solution name without the `solution-` prefix, kebab-case.
  # element/{element-name}: the repository name in kebab-case, no braces or dots
  # (e.g. repo my-app -> element/my-app-repo).
---

# How Apply this template
- Replace all `hint`, `example` and `code example` blocks with real content. Do not keep them in the final skill file.
- If a section does not introduce any changes for the repository, remove the section or add a note that no changes are introduced.

# Structure

## Project Structure
```hint
Define how solution EXTENDS repository structure.
```
```example
/src
  /App
    /App.Host
```

## Directory and class skills
```hint
Define how solution EXTENDS repository directory and files.
```
```example
| Directory | file | Description        |
| ----------------- | ------------------ |
| /src/App          | project description |
```

| Directory | file | Description |
| ----------------- | ----------- |
|                   |             |

# Rules
```hint
Define how solution EXTENDS repository rules. Follow the Rule-section baseline in [[skills/common-workflow/skill-design.skill/skill-design.skill.md|skill-design]]:
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
- Command must realize ICommand<Result<DTO>>
  - Risk: the mediator pipeline cannot infer the result type, so validation and result mapping do not apply.
  - Fix: implement ICommand<Result<DTO>> on the command record.
- Never place business logic in repository-level scripts.
  - Risk: repositories start enforcing business rules instead of just orchestrating persistence.
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

# Unittest TestCases
```hint
Define how solution EXTENDS repository integration unit tests.

RECOMMENDATION:
- Prefer checkbox list
```
```example
- [ ] WHEN call Post /record THEN
  - [ ] return expected result
  - [ ] new record exist in Database
- [ ] WHEN call Get /record/{id} and record exist THEN
  - [ ] Return expected result
- [ ] WHEN call Get /record/{id} and record NOT exist THEN
  - [ ] Return 404 error
  - [ ] Response has expected body
```
