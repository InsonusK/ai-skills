---
name: repo-name
description: Describe which plateau Nx workspace does skill describe
whenToUse: One concrete sentence — which task must make the agent open this skill
  # MUST name a concrete situation: adding/removing a top-level app/lib, deciding where a new app/lib belongs, or reviewing the workspace-level layout of the `{plateau-name}` plateau. MUST NOT be vague ("when relevant").
  # Example: "when adding, removing, or relocating an app/lib in the workspace, or deciding which existing lib a new feature belongs to"
domain: skill
type: template
plateau:
version:
tags:
  - skill/template/repo
created_by:
---
# How Apply this template
- Fill `whenToUse` with the concrete workspace-level situations that require this skill (adding/removing an app/lib, deciding where new code belongs, reviewing the top-level layout). See [skill-design](skills/common-workflow/skill-design.skill/skill-design.skill.md) for the baseline rules.
- Find in all solutions from `created_by` files made by `Repository.create.md` / `Repository.extend.md`
- Replace all ```hint``` and ```example``` blocks with real content. Do not keep them in the final skill file.
- add to header properties `tags` tag `plateau/{plateau-name}`
- "Repository" here means the Nx workspace root — everything above the level of an individual app/lib project.

# Structure

## Workspace Structure
```hint
Define Nx workspace structure (top-level apps/libs layout). Summarize all workspace structure from applied `Repository.create.md` or `Repository.extend.md` files. If find workspace structure in other files also applyed here.

At the end of block writes list to all used templates to build block.

MUST:
- Keep only repository-level content here. Show only project (app/lib) folders.
- If solution conflicted to each other as user to solve the problem
- For workspace structure block:
  - use link to files which define the project
- For Applied solutions block:
	- Each bullet must be `<solution skill link> - <implementation file link>` (see plateau-build SKILL.md "Applied solutions list format")
```
```example
/apps
  /[platform-shell](./platform-shell/project-platform-shell.skill.md)
/libs
  /shared
    /[ui](./shared-ui/project-shared-ui.skill.md)

__Applied solutions:__
- [[Solution link]] - [[implementation file link]]
```

## Directory and project skills
```hint
Define repository Directory and project table. Summarize all Directory and project skills from all finded Repository.create.md/Repository.extend.md files.

At the end of block writes list to all used templates to build block.

MUST:
- If solution conflicted to each other as user to solve the problem
- Each bullet must be `<solution skill link> - <implementation file link>` (see plateau-build SKILL.md "Applied solutions list format")
- Keep only repository-level content here. Show only project directory, project template file and link to it. Do not list class/artifact skill files here.
```
```example
| Directory | template link | Description |
| ---------- | ------------- | ----------- |
| /apps/platform-shell | [[project-platform-shell.skill.md\|project-platform-shell.skill]] | Host application, composition root, routing shell |
| /libs/shared/ui | [[project-shared-ui.skill.md\|project-shared-ui.skill]] | Cross-feature reusable UI wrappers |
```

| Directory | template link | Description |
| ---------- | ------------- | ----------- |
|           |               |             |

__Applied solutions:__
- <Solution link> - <implementation file link>

# Rules
```hint
Define all repository MUST, SHOULD, MAY, SHOULD NOT, MUST NOT rules. Summarize all Rules from all finded Repository.create.md/Repository.extend.md files.

At the end of block writes list to all used templates to build block.

MUST:
- If solution conflicted to each other as user to solve the problem
- Each bullet must be `<solution skill link> - <implementation file link>` (see plateau-build SKILL.md "Applied solutions list format")
- Keep only repository-level content here. Do not include Rules that belong to a specific project or class/artifact.
- Only add a subblock for categories where at least one solution introduces a rule. If a category has no rules, skip it — do not write an empty subblock.
```

## MUST
```example
- Every Nx project must declare tags matching the `type:*`/`scope:*` taxonomy

__Applied solutions:__
- [[Solution link]] - [[implementation file link]]
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
Describe concrete wrong ways to apply the plateau at repository level and their consequences. Summarize all Anti-patterns from all finded Repository.create.md/Repository.extend.md files.

At the end of block writes list to all used templates to build block.

MUST:
- If solution conflicted to each other as user to solve the problem
- Each bullet must be `<solution skill link> - <implementation file link>` (see plateau-build SKILL.md "Applied solutions list format")
- Keep only repository-level content here.

Format:
- **{What NOT to do}**
  - Consequence: {negative consequence}
  - Instead: {correct alternative}
```
```example
- **Place a new feature directly under /apps instead of a routed lib under /libs**
  - Consequence: feature can no longer be reused or lazy-loaded independently
  - Instead: scaffold the feature as a lib under /libs and route to it lazily

__Applied solutions:__
- [[Solution link]] - [[implementation file link]]
```

# Unittest TestCases
```hint
Define repository-level integration/e2e checks (lint boundaries, affected-based CI, workspace-wide invariants). Summarize all "Unittest TestCases" from all finded Repository.create.md/Repository.extend.md files.

At the end of block writes list to all used templates to build block.

MUST:
- If solution conflicted to each other as user to solve the problem
- Each bullet must be `<solution skill link> - <implementation file link>` (see plateau-build SKILL.md "Applied solutions list format")
- Keep only repository-level content here.

RECOMMENDATION:
- Prefer checkbox list
```
```example
- [ ] WHEN `nx run-many -t lint` is executed THEN
  - [ ] `@nx/enforce-module-boundaries` reports no violations

__Applied solutions:__
- [[Solution link]] - [[implementation file link]]
```
