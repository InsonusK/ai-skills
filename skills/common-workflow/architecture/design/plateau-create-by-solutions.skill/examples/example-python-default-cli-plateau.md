# Example: creating the Python `default-cli` plateau

This example shows how a Python CLI plateau is produced from `solution-default-cli`.

## Input

- plateau-name: `default-cli`
- target-stack: `python`
- output: `skills/python/architecture/artifacts/plateau/default-cli`
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

## Mapping from solution Implementation files to plateau skills

| Solution | Implementation file | Plateau skill |
| -------- | ------------------- | ------------- |
| solution-default-cli | `{App}.create.md` | `sln-default-cli.skill.md` |
| solution-default-cli | `{App}.cli.py.create.md` | `structure/{App}/files/file-cli-py.skill.md` |
| solution-default-cli | `{App}.cli.__init__.py.create.md` | `structure/{App}/files/file-cli-init-py.skill.md` |
| solution-default-cli | `{App}.cli.{Command}.py.create.md` | `structure/{App}/files/file-cli-command-py.skill.md` |
| solution-default-cli | `{App}.command.__init__.py.create.md` | `structure/{App}/files/file-command-init-py.skill.md` |
| solution-default-cli | `{App}.command.{Command}.py.create.md` | `structure/{App}/files/file-command-command-py.skill.md` |
| solution-default-cli | `{App}.functions.__init__.py.create.md` | `structure/{App}/files/file-functions-init-py.skill.md` |
| solution-default-cli | `{App}.functions.{Function}.py.create.md` | `structure/{App}/files/file-functions-function-py.skill.md` |
| solution-default-cli | `{App}.service.__init__.py.create.md` | `structure/{App}/files/file-service-init-py.skill.md` |
| solution-default-cli | `{App}.service.{Service}.py.create.md` | `structure/{App}/files/file-service-service-py.skill.md` |

## Resulting plateau structure

```
plateau/default-cli/
├── plateau-default-cli.skill.md
└── structure/
    ├── sln-default-cli.skill.md
    └── {App}/
        ├── artifact-app.skill.md
        └── files/
            ├── file-cli-py.skill.md
            ├── file-cli-init-py.skill.md
            ├── file-cli-command-py.skill.md
            ├── file-command-init-py.skill.md
            ├── file-command-command-py.skill.md
            ├── file-functions-init-py.skill.md
            ├── file-functions-function-py.skill.md
            ├── file-service-init-py.skill.md
            └── file-service-service-py.skill.md
```

## Key observations

- `{App}.create.md` becomes the foundation of `sln-default-cli.skill.md`.
- The root `{App}` folder is treated as one artifact → `artifact-app.skill.md`.
- Individual `.py` files map to `file-{normalized}.skill.md` inside `structure/{App}/files/`.
- `__init__.py` files are not skipped; each becomes a separate file skill because it carries package-level rules.
- Placeholder names (`{App}`, `{Command}`, `{Function}`, `{Service}`) stay generic in the plateau output.
