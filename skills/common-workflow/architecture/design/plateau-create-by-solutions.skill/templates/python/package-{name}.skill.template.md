---
name: package-name
description: Describe which plateau package/app does skill describe
domain: skill
type: template
plateau:
version:
tags:
  - skill/template/package
created_by:
---
# How Apply this template
- Find in all solutions from `created_by` files made by the project-level implementation file (`{App}.create.md`, `element_kind: project`)
- Replace all ```hint``` and ```example``` blocks with real content. Do not keep them in the final skill file.
- add to header properties `tags` tag `plateau/{plateau-name}`

# Goal
```hint
Define List of Goals that are pursued by the creation of this skill. Summarize all Goals from all finded project-level implementation files.

At the end of block writes list to all used templates to build block.

MUST:
- If Goals conflicted to each other as user to solve the problem
- Each bullet must be `<solution skill link> - <implementation file link>` (see plateau-build SKILL.md "Applied solutions list format")
- Keep only package-level content here. Do not include repository-level or module-level details.

RECOMENDATION:
- Prefer bullet list
- Prefer pure copy with out changing
```
```example
- Provide a home for the CLI application with a layered structure

__Applied solutions:__
- [[Solution link]] - [[implementation file link]]
```

# Core Principles
```hint
Define List of Core Principles that are pursued by the creation of this skill. Summarize all Core Principles from all finded project-level implementation files.

At the end of block writes list to all used templates to build block.

MUST:
- If Core Principles conflicted to each other as user to solve the problem
- Each bullet must be `<solution skill link> - <implementation file link>` (see plateau-build SKILL.md "Applied solutions list format")
- Keep only package-level content here. Do not include repository-level or module-level details.

RECOMENDATION:
- Prefer bullet list
- Prefer pure copy with out changing
```
```example
- Separate CLI, Command, Functions, and Service into their own directories

__Applied solutions:__
- [[Solution link]] - [[implementation file link]]
```

# Structure
## Repository place
`Where defined place in repository`
```
Where does it store in repository
/src
	/{App}
```
## Package Structure
```hint
Define package structure. Summarize all package structure from applied `{App}.create.md` or `{App}.extend.md` files. If find package structure in other files also applyed here.

At the end of block writes list to all used files to build block.

MUST:
- Keep only package-level content here. Do not include repository-level or module-level details.
- If solution conflicted to each other as user to solve the problem
- For package structure block:
  - use link to files which define module
- For Applied solutions block:
	- Each bullet must be `<solution skill link> - <implementation file link>` (see plateau-build SKILL.md "Applied solutions list format")
```
```example
- /{App}
	- /cli
		- [backup.py](./modules/module-cli-backup.skill.md)
	cli.py
```
```example
__Applied solutions:__
- [[Solution link]] - [[implementation file link]]
```

## Directory and module skills
```hint
Define package Table with directory and files which implement in package. Summarize all "Table with directory and files which implement in package" from all finded project-level implementation files.

At the end of block writes list to all used templates to build block.

MUST:
- If Directory and module skills conflicted to each other as user to solve the problem
- Each bullet must be `<solution skill link> - <implementation file link>` (see plateau-build SKILL.md "Applied solutions list format")
- Keep only package-level content here. Do not include repository-level or module-level details.

RECOMENDATION:
- Prefer bullet list
- Prefer pure copy with out changing
```
```example
| `Directory\|file`  | Description                                    | Pattern skill          |
| ------------------- | ---------------------------------------------- | ---------------------- |
| /cli                | Argparse wiring, one module per subcommand      | [[link to folder pattern]] |
| cli.py              | Entry point, builds parser, dispatches commands | [[link to file pattern]]    |

__Applied solutions:__
- [[Solution link]] - [[implementation file link]]
```

| `Directory|file` | Description | Pattern skill |
| ---------------- | ----------- | ------------- |
|                  |             |               |

## Python Dependencies
```hint
Define package Table with PyPI / standard library dependencies. Summarize all "Table with dependencies" from all finded project-level implementation files.

At the end of block writes list to all used templates to build block.

MUST:
- If Directory and module skills conflicted to each other as user to solve the problem
- Each bullet must be `<solution skill link> - <implementation file link>` (see plateau-build SKILL.md "Applied solutions list format")
- Keep only package-level content here. Do not include repository-level or module-level details.

RECOMENDATION:
- Prefer pure copy with out changing
```
```example
| Package   | Version constraint | Purpose                |   
| --------- | ------------------ | ---------------------- | 
| pydantic  | >= 2.0              | Request/response DTO validation |

__Applied solutions:__
- [[Solution link]] - [[implementation file link]]
```

| Package                                   | Version constraint | Purpose                               |
| ----------------------------------------- | ------------------ | ------------------------------------- |
|                                           |                    |                                       |

## What Does NOT Belong Here
```hint
Define components which doesnot belong to this package. Summarize all "components which doesnot belong to this package" from all finded project-level implementation files.

At the end of block writes list to all used templates to build block.

MUST:
- If "What Does NOT Belong Here" conflicted to each other as user to solve the problem
- Each bullet must be `<solution skill link> - <implementation file link>` (see plateau-build SKILL.md "Applied solutions list format")
- Keep only package-level content here. Do not include repository-level or module-level details.

RECOMENDATION:
- Prefer bullet list
- Prefer pure copy with out changing
```
```example
- Business logic - belongs to [[Other package skill]]

__Applied solutions:__
- [[Solution link]] - [[implementation file link]]
```

## Allowed Dependencies
```hint
Define Allowed dependencies that package may have. Summarize all "Allowed dependencies that package may have" from all finded project-level implementation files.

At the end of block writes list to all used templates to build block.

MUST:
- If "Allowed dependencies" conflicted to each other as user to solve the problem
- Each bullet must be `<solution skill link> - <implementation file link>` (see plateau-build SKILL.md "Applied solutions list format")
- Keep only package-level content here. Do not include repository-level or module-level details.

RECOMENDATION:
- Prefer bullet list
- Prefer pure copy with out changing
```
```example
- Standard library only (`argparse`, `logging`, `sys`)

__Applied solutions:__
- [[Solution link]] - [[implementation file link]]
```

# Rules
```hint
Define MUST, SHOULD, SHOULD NOT, MUST NOT rules. Summarize all "Rules" from all finded project-level implementation files.

At the end of block writes list to all used templates to build block. 

MUST:
- If Rules conflicted to each other as user to solve the problem
- Each bullet must be `<solution skill link> - <implementation file link>` (see plateau-build SKILL.md "Applied solutions list format")
- Keep only package-level content here. Do not include repository-level or module-level details.

RECOMENDATION:
- Prefer bullet list
- Prefer pure copy with out changing
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
MUST NOT:
- extended_by solution modify Allowed Dependencies without explicit user confirmation

# Anti-patterns
```hint
Define What mean that skill applyed wrong. Summarize all "Anti-patterns" from all finded project-level implementation files.

At the end of block writes list to all used templates to build block. 

MUST:
- If "Anti-patterns" conflicted to each other as user to solve the problem
- Each bullet must be `<solution skill link> - <implementation file link>` (see plateau-build SKILL.md "Applied solutions list format")
- Keep only package-level content here. Do not include repository-level or module-level details.

RECOMENDATION:
- Prefer bullet list
- Prefer pure copy with out changing
```
```example
- Place all code in a single script

__Applied solutions:__
- [[Solution link]] - [[implementation file link]]
```

# Check list
```hint
Define what must be true before this template is considered correctly applied?. Summarize all "Check list" from all finded project-level implementation files.

At the end of block writes list to all used templates to build block. 

MUST:
- If "Check list" conflicted to each other as user to solve the problem
- Each bullet must be `<solution skill link> - <implementation file link>` (see plateau-build SKILL.md "Applied solutions list format")
- Keep only package-level content here. Do not include repository-level or module-level details.

RECOMENDATION:
- Prefer bullet list
- Prefer pure copy with out changing
```
```example
- [ ] `/cli`, `/command`, `/functions`, `/service` directories exist
- [ ] `cli.py` is at the package root

__Applied solutions:__
- [[Solution link]] - [[implementation file link]]
```
