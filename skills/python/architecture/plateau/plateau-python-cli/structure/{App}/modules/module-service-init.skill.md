---
name: module-service-init
description: Make the service directory a Python package
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
- Make `service` a Python package so submodules can be imported.

__Applied solutions:__
- [[skills/python/architecture/solutions/solution-default-cli.skill/solution-default-cli.skill.md|solution-default-cli]] - [[skills/python/architecture/solutions/solution-default-cli.skill/Implementation/{App}.service.__init__.py.create.md|{App}.service.__init__.py.create]]

# Core Principles
- Apply ONE plateau template per class/module.
- An empty `__init__.py` is sufficient to mark the directory as a package.

__Applied solutions:__
- [[skills/python/architecture/solutions/solution-default-cli.skill/solution-default-cli.skill.md|solution-default-cli]] - [[skills/python/architecture/solutions/solution-default-cli.skill/Implementation/{App}.service.__init__.py.create.md|{App}.service.__init__.py.create]]

# Naming convention

| use case | element name pattern | element name | file name pattern | file name |
| -------- | -------------------- | ------------ | ----------------- | --------- |
| Package init | - | - | service/__init__.py | service/__init__.py |

# Implementation

```python
# Skill: module-service-init
# Plateau: plateau-python-cli
# Version: 20260710003814

# {App}/service/__init__.py
```

__Applied solutions:__
- [[skills/python/architecture/solutions/solution-default-cli.skill/solution-default-cli.skill.md|solution-default-cli]] - [[skills/python/architecture/solutions/solution-default-cli.skill/Implementation/{App}.service.__init__.py.create.md|{App}.service.__init__.py.create]]

# Rules

## MUST
- `{App}/service/__init__.py` must exist.

__Applied solutions:__
- [[skills/python/architecture/solutions/solution-default-cli.skill/solution-default-cli.skill.md|solution-default-cli]] - [[skills/python/architecture/solutions/solution-default-cli.skill/Implementation/{App}.service.__init__.py.create.md|{App}.service.__init__.py.create]]

# Anti-patterns

- **Apply SEVERAL plateau template per class/module**
  - Consequence: conflicting responsibilities and inconsistent generated code.
  - Instead: apply exactly one plateau module template per file.

__Applied solutions:__
- [[skills/python/architecture/solutions/solution-default-cli.skill/solution-default-cli.skill.md|solution-default-cli]] - [[skills/python/architecture/solutions/solution-default-cli.skill/Implementation/{App}.service.__init__.py.create.md|{App}.service.__init__.py.create]]

# Check list

- [ ] `{App}/service/__init__.py` exists (can be empty).

__Applied solutions:__
- [[skills/python/architecture/solutions/solution-default-cli.skill/solution-default-cli.skill.md|solution-default-cli]] - [[skills/python/architecture/solutions/solution-default-cli.skill/Implementation/{App}.service.__init__.py.create.md|{App}.service.__init__.py.create]]

# Unittest TestCases

- [ ] WHEN the `service` package is imported THEN no exception is raised.

__Applied solutions:__
- [[skills/python/architecture/solutions/solution-default-cli.skill/solution-default-cli.skill.md|solution-default-cli]] - [[skills/python/architecture/solutions/solution-default-cli.skill/Implementation/{App}.service.__init__.py.create.md|{App}.service.__init__.py.create]]
