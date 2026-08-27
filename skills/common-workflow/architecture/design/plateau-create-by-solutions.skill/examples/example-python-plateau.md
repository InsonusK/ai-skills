# Example: building a Python plateau from one solution

## Input
- plateau-name: `default`
- stack: `python`
- solutions:
  - [[skills/python/architecture/solutions/solution-default-cli.skill/solution-default-cli.skill|solution-default-cli]]

## Source files discovered in Implementation/

`solution-default-cli`:
- `{App}.create.md`
- `{App}.cli.py.create.md`
- `{App}.cli.__init__.py.create.md`
- `{App}.cli.{Command}.py.create.md`
- `{App}.command.__init__.py.create.md`
- `{App}.command.{Command}.py.create.md`
- `{App}.functions.__init__.py.create.md`
- `{App}.functions.{Function}.py.create.md`
- `{App}.service.__init__.py.create.md`
- `{App}.service.{Service}.py.create.md`

## Resulting plateau structure

```
plateau/default/
├── plateau-default.skill.md
└── structure/
    └── {App}/
        ├── package-app.skill.md
        └── modules/
            ├── module-cli.skill.md
            ├── module-cli-init.skill.md
            ├── module-cli-command.skill.md
            ├── module-command-init.skill.md
            ├── module-command-command.skill.md
            ├── module-functions-init.skill.md
            ├── module-functions-function.skill.md
            ├── module-service-init.skill.md
            └── module-service-service.skill.md
```

## Resulting `plateau-default.skill.md` frontmatter

```yaml
---
name: default
description: Default Python CLI plateau
domain: skill
type: template
version: 20250101120000
tags:
  - skill/template/plateau
parent_plateaus:
standalone: true
created_by:
  - [[skills/python/architecture/solutions/solution-default-cli.skill/solution-default-cli.skill.md|solution-default-cli]]
---
```

## Key observations
- `{App}.create.md` (`element_kind: project`) becomes the foundation of `package-app.skill.md`.
- `{App}.cli.py.create.md` → `module-cli.skill.md`; `{App}.cli.__init__.py.create.md` → `module-cli-init.skill.md`.
- `{Command}`, `{Function}` and `{Service}` stay as placeholders because `solution-default-cli` itself defines them as generic templates, not concrete names.
- This solution never populates `Repository.create.md`, so no `repo-default.skill.md` is produced — the plateau has only the package and module tiers.
