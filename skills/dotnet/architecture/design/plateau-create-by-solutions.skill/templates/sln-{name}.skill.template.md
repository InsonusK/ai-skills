---
name: sln-name
description: Describe which plateau repository does skill describe
domain: skill
type: template
plateau:
version:
tags:
  - skill/template/sln
created_by:
---
# How Apply this template
- Find in all solutions from created_by files made by Repository.template.md
- Replace all ```hint``` and ```example``` blocks with real content. Do not keep them in the final skill file.
- add to header properties `tags` tag `plateau/{plateau-name}`

# Structure

## Repository Structure
```hint
Define solution structure. Summarize all project structure from applied `Repository.created` or `Repository.extended` files. If find project structure in other files also applyed here.

At the end of block writes list to all used templates to build block.

MUST:
- Keep only repository-level content here. Show only project folders.
- If solution conflicted to each other as user to solve the problem
- For project structure block:
  - use link to files which define project
- For Applied solutions block:
	- Each bullet must be `<solution skill link> - <Repository.template.md link>` (see plateau-build SKILL.md "Applied solutions list format")
```
```example
- /src
	- /App
		- /[App.Host](./App.Host/csproj-app-host.skill.md)

__Applied solutions:__
- [[Solution link]] - [[Repository.template.md link]]
```

## Directory and class skills
```hint
Define repository Directory and class. Summarize all Directory and class skills from all finded Repository.template.md

At the end of block writes list to all used templates to build block.

MUST:
- If solution conflicted to each other as user to solve the problem
- Each bullet must be `<solution skill link> - <Repository.template.md link>` (see plateau-build SKILL.md "Applied solutions list format")
- Keep only repository-level content here. Show only project directory, project template file and link to it
```
```example
| Directory \| file | template link     | Description        |
| ----------------- | ----------------- | ------------------ |
| /src/App          | [[template link]] | project desciption |
```

| Directory \| file | template link | Description |
| ----------------- | ------------- | ----------- |
|                   |               |             |

__Applied solutions:__
- <Solution link> - <Repository.template.md link>

# Rules
```hint
Define all repository RULES. Summarize all RULES from all finded Repository.template.md

At the end of block writes list to all used templates to build block.

MUST:
- If solution conflicted to each other as user to solve the problem
- Each bullet must be `<solution skill link> - <Repository.template.md link>` (see plateau-build SKILL.md "Applied solutions list format")
- Keep only repository-level content here. Do not include Rules that belong to a specific project or class.
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
- [[Solution link]] - [[Repository.template.md link]]
```

