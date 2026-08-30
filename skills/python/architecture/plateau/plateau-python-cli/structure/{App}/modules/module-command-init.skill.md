---
name: module-command-init
description: Make the command directory a Python package
domain: skill
type: template
plateau: plateau-python-cli
version: 20260710003814
tags:
  - skill/template/module
  - plateau/plateau-python-cli
  - stack/python
  - concern/architecture

created_by:
  - "[[skills/python/architecture/solutions/solution-default-cli.skill/solution-default-cli.skill.md|solution-default-cli]]"
---

# Goal
- Make `command` a Python package so submodules can be imported.

__Applied solutions:__
- [[skills/python/architecture/solutions/solution-default-cli.skill/solution-default-cli.skill.md|solution-default-cli]] - [[skills/python/architecture/solutions/solution-default-cli.skill/Implementation/{App}.command.__init__.py.create.md|{App}.command.__init__.py.create]]

# Core Principles
- Apply ONE plateau template per class/module.
- An empty `__init__.py` is sufficient to mark the directory as a package.

__Applied solutions:__
- [[skills/python/architecture/solutions/solution-default-cli.skill/solution-default-cli.skill.md|solution-default-cli]] - [[skills/python/architecture/solutions/solution-default-cli.skill/Implementation/{App}.command.__init__.py.create.md|{App}.command.__init__.py.create]]

# Naming convention

| use case | element name pattern | element name | file name pattern | file name |
| -------- | -------------------- | ------------ | ----------------- | --------- |
| Package init | - | - | command/__init__.py | command/__init__.py |

# Implementation

```python
# Skill: module-command-init
# Plateau: plateau-python-cli
# Version: 20260710003814

# {App}/command/__init__.py
```

__Applied solutions:__
- [[skills/python/architecture/solutions/solution-default-cli.skill/solution-default-cli.skill.md|solution-default-cli]] - [[skills/python/architecture/solutions/solution-default-cli.skill/Implementation/{App}.command.__init__.py.create.md|{App}.command.__init__.py.create]]

# Rules

## MUST
- `{App}/command/__init__.py` must exist.

__Applied solutions:__
- [[skills/python/architecture/solutions/solution-default-cli.skill/solution-default-cli.skill.md|solution-default-cli]] - [[skills/python/architecture/solutions/solution-default-cli.skill/Implementation/{App}.command.__init__.py.create.md|{App}.command.__init__.py.create]]

# Anti-patterns

- **Apply SEVERAL plateau template per class/module**
  - Consequence: conflicting responsibilities and inconsistent generated code.
  - Instead: apply exactly one plateau module template per file.

__Applied solutions:__
- [[skills/python/architecture/solutions/solution-default-cli.skill/solution-default-cli.skill.md|solution-default-cli]] - [[skills/python/architecture/solutions/solution-default-cli.skill/Implementation/{App}.command.__init__.py.create.md|{App}.command.__init__.py.create]]

# Check list

- [ ] `{App}/command/__init__.py` exists (can be empty).

__Applied solutions:__
- [[skills/python/architecture/solutions/solution-default-cli.skill/solution-default-cli.skill.md|solution-default-cli]] - [[skills/python/architecture/solutions/solution-default-cli.skill/Implementation/{App}.command.__init__.py.create.md|{App}.command.__init__.py.create]]

# Unittest TestCases

- [ ] WHEN the `command` package is imported THEN no exception is raised.

__Applied solutions:__
- [[skills/python/architecture/solutions/solution-default-cli.skill/solution-default-cli.skill.md|solution-default-cli]] - [[skills/python/architecture/solutions/solution-default-cli.skill/Implementation/{App}.command.__init__.py.create.md|{App}.command.__init__.py.create]]
