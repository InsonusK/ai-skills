---
name: plateau-{plateau-name}--repo-{name}
description: Describe which plateau repository does skill describe
whenToUse: One concrete sentence — which task must make the agent open this skill
  # MUST name a concrete situation: adding/removing a top-level package/app, deciding where a new package belongs, or reviewing the repository-level layout of the `{plateau-name}` plateau. MUST NOT be vague ("when relevant").
  # Example: "when adding, removing, or relocating a package in the repository, or deciding which existing package a new module belongs to"
domain: skill
type: template
plateau:
version:
tags:
  - skill/template/repo
created_by:
---
# How Apply this template
- Fill `whenToUse` with the concrete repository-level situations that require this skill (adding/removing a package, deciding where new code belongs, reviewing the top-level layout). See [skill-design](skills/common-workflow/skill-design.skill/skill-design.skill.md) for the baseline rules.
- Find in all solutions from `created_by` files made by `Repository.create.md` / `Repository.extend.md`
- Replace all ```hint``` and ```example``` blocks with real content. Do not keep them in the final skill file.
- add to header properties `tags` tag `plateau/{plateau-name}`

> Most single-package Python repositories never populate `Repository.create.md`. Only build this skill when a solution actually contributes repository-level content (relating several packages/apps to each other). Otherwise the plateau has only the package and module tiers.

# Structure

## Repository Structure
```hint
Define repository structure. Summarize all repository structure from applied `Repository.create.md` or `Repository.extend.md` files. If find repository structure in other files also applyed here.

At the end of block writes list to all used templates to build block.

MUST:
- Keep only repository-level content here. Show only package/app folders.
- If solution conflicted to each other as user to solve the problem
- For repository structure block:
  - use link to files which define package
- For Applied solutions block:
	- Each bullet must be `<solution skill link> - <implementation file link>` (see plateau-create-by-solutions.skill.md "Applied solutions list format")
```
```example
- /src
	- /{App}
		- [{App}](./{App}/package-app.skill.md)

__Applied solutions:__
- [[Solution link]] - [[implementation file link]]
```

## Directory and package skills
```hint
Define repository Directory and package. Summarize all Directory and package skills from all finded Repository.create.md/Repository.extend.md files

At the end of block writes list to all used templates to build block.

MUST:
- If solution conflicted to each other as user to solve the problem
- Each bullet must be `<solution skill link> - <implementation file link>` (see plateau-create-by-solutions.skill.md "Applied solutions list format")
- Keep only repository-level content here. Show only package directory, package template file and link to it
```
```example
| Directory \| file | template link     | Description        |
| ----------------- | ----------------- | ------------------ |
| /src/{App}         | [[template link]] | app desciption |
```

| Directory \| file | template link | Description |
| ----------------- | ------------- | ----------- |
|                   |               |             |

__Applied solutions:__
- <Solution link> - <implementation file link>

# Rules
```hint
Define all repository RULES. Summarize all RULES from all finded Repository.create.md/Repository.extend.md files

At the end of block writes list to all used templates to build block.

MUST:
- If solution conflicted to each other as user to solve the problem
- Each bullet must be `<solution skill link> - <implementation file link>` (see plateau-create-by-solutions.skill.md "Applied solutions list format")
- Keep only repository-level content here. Do not include Rules that belong to a specific package or class/module.
```
```example
MUST:
	- ...
SHOULD:
	- ...
SHOULD NOT:
	- ...
MUST NOT:
	- ...

__Applied solutions:__
- [[Solution link]] - [[implementation file link]]
```
