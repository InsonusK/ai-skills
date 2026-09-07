---
name: plateau-{plateau-name}--module-{name}
description: Describe which module skill define
whenToUse: One concrete sentence — which task must make the agent open this skill
  # MUST name a concrete situation: creating or editing this exact class/function module, or creating a new one that plays the same role. MUST NOT be vague ("when relevant").
  # Example: "when creating or editing {module-name}.py, or creating another module that plays the same role for a different command"
domain: skill
type: template
plateau:
version:
tags:
  - skill/template/module
created_by:
---
# How Apply this template
- Fill `whenToUse` with the concrete class/module-level situations that require this skill (creating/editing this module, or creating another module with the same role). See [skill-design](skills/common-workflow/skill-design.skill/skill-design.skill.md) for the baseline rules.
- Find in all solutions from `created_by` files made by `Class.template.md`, `functions.template.md` or `{Package}/__init__.py.template.md` (any file with `element_kind: class`, `functions` or `init`)
- Replace all ```hint``` and ```example``` blocks with real content. Do not keep them in the final skill file.
- add to header properties `tags` tag `plateau/{plateau-name}`

# Goal
```hint
Define List of Goals that are pursued by the creation of this skill. Summarize all Goals from all finded class/functions/init implementation files.

At the end of block writes list to all used templates to build block.

MUST:
- If Goals conflicted to each other as user to solve the problem
- Each bullet must be `<solution skill link> - <implementation file link>` (see plateau-create-by-solutions.skill.md "Applied solutions list format")
- Keep only class/module-level content here. Do not include repository-level or package-level details.

RECOMENDATION:
- Prefer bullet list
- Prefer pure copy with out changing

After summarized list add Applied solutions list with links to all solutions which effect to this Goals.
```
```example
- Build the argument parser and register subcommands
- Configure logging before any command runs

__Applied solutions:__
- [[Solution link]] - [[implementation file link]]
```

# Core Principles
```hint
Define List of Core Principles that are pursued by the creation of this skill. Summarize all Core Principles from all finded class/functions/init implementation files.

At the end of block writes list to all used templates to build block.

MUST:
- If Core Principles conflicted to each other as user to solve the problem
- Each bullet must be `<solution skill link> - <implementation file link>` (see plateau-create-by-solutions.skill.md "Applied solutions list format")
- Keep only class/module-level content here. Do not include repository-level or package-level details.
- Add Core principle `Apply ONE plateau template per class/module`

RECOMENDATION:
- Prefer bullet list
- Prefer pure copy with out changing
```
```example
- Apply ONE plateau template per class/module
- `cli.py` is the only file aware of all available subcommands
- Functions are stateless and reusable

__Applied solutions:__
- [[Solution link]] - [[implementation file link]]
```

# Naming convention
```hint
Define Naming convention. Summarize all "Naming convention" from all finded class/functions/init implementation files.

At the end of block writes list to all used templates to build block.

class/function/module naming convention. Fill table
- use case - when apply naming convention
- element name pattern - mask of the class/function name. Example: is_{rule}
- element name - example of the class/function name. Example: is_even
- file name pattern - file name pattern. Example: {module}.py
- file name - example of file name. Example: is_even.py
```

| use case | element name pattern | element name | file name pattern | file name |
| -------- | --------------------- | ------------- | ----------------- | --------- |
|          |                       |               |                   |           |

# Implementation
```hint
Define Implementaion of class/function/module. Summarize all "Implementation changes" from all finded class/functions/init implementation files.

At the end of block writes list to all used templates to build block.

MUST:
- Write a comment at the top of created module with information from applied skill properties
  - name
  - plateau
  - version

- If Implementation changes conflicted to each other as user to solve the problem
- Each bullet must be `<solution skill link> - <implementation file link>` (see plateau-create-by-solutions.skill.md "Applied solutions list format")
- Keep only class/module-level content here. Do not include repository-level or package-level details.

RECOMENDATION:
- Prefer bullet list
- Prefer pure copy with out changing
```
```example
# Skill: module-cli
# Plateau: default
# Version: 20260628

import argparse


def main(argv: list[str] | None = None) -> int:
    parser = argparse.ArgumentParser(prog="{App}")
    args = parser.parse_args(argv)
    return 0

__Applied solutions:__
- [[Solution link]] - [[implementation file link]]
```

# Rules
```hint
Define MUST, SHOULD, MAY rules of the class/function/module only — never `## MUST NOT`/`## SHOULD NOT` headings and never a separate `# Anti-patterns` section (see [skill-design](skills/common-workflow/skill-design.skill/skill-design.skill.md)). Summarize all "Rule changes"/"Anti-patterns" from all finded class/functions/init implementation files, phrasing every prohibition as a negatively-worded bullet ("Never...") inside `MUST`/`SHOULD` at whichever strength it carries, and fold any anti-pattern's worked "wrong way" example into the same bullet instead of keeping a separate section. Always include a bullet against applying several plateau templates per class/module.

At the end of block writes list to all used templates to build block.

MUST:
- If Rules conflicted to each other as user to solve the problem
- Each bullet must be `<solution skill link> - <implementation file link>` (see plateau-create-by-solutions.skill.md "Applied solutions list format")
- Keep only class/module-level content here. Do not include repository-level or package-level details.

RECOMENDATION:
- Prefer bullet list
- Prefer pure copy with out changing
```
```example
MUST:
	- ...
	- Never apply several plateau templates per class/module
	- Never use mutable default arguments in function signatures
	- Never put business logic in `__init__.py`
SHOULD:
	- ...
	- Never ... (a softer prohibition, phrased positively inside SHOULD)
	  
__Applied solutions:__
- [[Solution link]] - [[implementation file link]]
```

# Check list
```hint
Define what must be true before this template is considered correctly applied?. Summarize all "Check list" from all finded class/functions/init implementation files.

At the end of block writes list to all used templates to build block. 

MUST:
- If "Check list" conflicted to each other as user to solve the problem
- Each bullet must be `<solution skill link> - <implementation file link>` (see plateau-create-by-solutions.skill.md "Applied solutions list format")
- Keep only class/module-level content here. Do not include repository-level or package-level details.

RECOMENDATION:
- Prefer bullet list
- Prefer pure copy with out changing
```
```example
- [ ] `--debug` flag is defined
- [ ] Logging is configured before `args.run(args)`

__Applied solutions:__
- [[Solution link]] - [[implementation file link]]
```

# Unittest TestCases
```hint
Define list of unittests which must be created to test class/function/module. Summarize all "Unittest TestCases" from all finded class/functions/init implementation files.

At the end of block writes list to all used templates to build block.

MUST:
- If Check list conflicted to each other as user to solve the problem
- Each bullet must be `<solution skill link> - <implementation file link>` (see plateau-create-by-solutions.skill.md "Applied solutions list format")
- Keep only class/module-level content here. Do not include repository-level or package-level details.

RECOMENDATION:
- Prefer bullet list
- Prefer pure copy with out changing
```
```example
- [ ] WHEN call helper with valid input THEN returns expected result

__Applied solutions:__
- [[Solution link]] - [[implementation file link]]
```
