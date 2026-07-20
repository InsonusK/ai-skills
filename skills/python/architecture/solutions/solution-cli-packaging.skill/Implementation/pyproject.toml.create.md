---
description: Create the project metadata, build system and console entry point for a CLI application
project_name: "{App}"
name: "pyproject.toml"
element_kind: config
change_kind: create
---

# Goals
- Describe the CLI application as an installable Python project (name, version, dependencies).
- Declare a PEP 517 build backend so `pip` can build a wheel from a local checkout or from a Git/GitHub source.
- Expose a console command backed by `{App}/cli.py`'s `main()` function.

# Core Principles
- `pyproject.toml` is the single descriptor for build system and project metadata — no `setup.py`/`setup.cfg`.
- `[project.scripts]` is the only supported way to expose a runnable terminal command; it must point at the `main()` entry point created by [[../../solution-default-cli.skill/Implementation/{App}.cli.py.create.md|cli.py]].
- Package discovery configuration must match the actual repository layout ({App} at repo root vs. a `src/` layout).
- See [glossary: pyproject.toml](../glossary/pyproject-toml.md) for what this file is and how `pip` uses it.

# Implementation changes
Create `pyproject.toml` at the repository root:

```toml
[build-system]
requires = ["setuptools>=61.0", "wheel"]
build-backend = "setuptools.build_meta"

[project]
name = "{app-name}"
version = "0.1.0"
description = "{Short description of the CLI application}"
readme = "README.md"
requires-python = ">=3.9"
dependencies = [
]

[project.urls]
Homepage = "https://github.com/{org}/{app-name}"
Repository = "https://github.com/{org}/{app-name}.git"
Issues = "https://github.com/{org}/{app-name}/issues"

[project.scripts]
{app-name} = "{App}.cli:main"

[project.optional-dependencies]
dev = [
    "pytest>=7.0",
    "build",
    "twine",
]

[tool.setuptools.packages.find]
where = ["."]
exclude = ["test", "test.*", "*test*"]
```

Notes:
- `{app-name}` is the distribution/command name (kebab-case, e.g. `my-cli`); `{App}` is the importable package directory (snake_case, e.g. `my_cli`). They may differ.
- When [[../../solution-test.skill/solution-test.skill.md|solution-test]] is also applied, merge its `[tool.setuptools.packages.find]` exclusion list with the one shown here instead of declaring the table twice.
- When the repository uses a `src/` layout, set `where = ["src"]` instead of `where = ["."]`.

# Rule changes

## MUST
- Declare `[build-system]` with `build-backend = "setuptools.build_meta"` (or another PEP 517 backend) before any other table.
- Declare `[project]` with at least `name`, `version`, `description`, and `requires-python`.
- Declare `[project.scripts]` mapping a console command name to `{App}.cli:main`, where `main` accepts no required positional arguments.
- Keep `[tool.setuptools.packages.find]` consistent with the actual repository layout.

## SHOULD
- Declare `[project.urls].Repository` so an installed package can be traced back to its source.
- Pin minimum versions for runtime dependencies (`package>=X.Y`).
- Separate development-only tools (`pytest`, `build`, `twine`) into `[project.optional-dependencies].dev`.

## MUST NOT
- Reference `{App}.cli:main` before `cli.py` exposes a callable, argument-optional `main()` function.
- Add a second, conflicting `[tool.setuptools.packages.find]` table when solution-test already defines one — merge exclusions instead.
- Hardcode local filesystem paths in `dependencies` — this breaks `pip install` from a fresh clone or from a Git/GitHub URL.

# Anti-patterns
- **Ship a CLI app without `[project.scripts]`**
  - Consequence: users must remember to run `python -m {App}.cli`, and no command is added to `PATH` after `pip install`.
  - Instead: declare `[project.scripts]` so `pip` generates a console-script wrapper on install.
- **Omit `requires-python`**
  - Consequence: `pip` may attempt to build/install on an incompatible interpreter and fail with a confusing backend error instead of a clear version message.
  - Instead: declare `requires-python = ">=X.Y"` matching the syntax actually used in the code.
- **Point `where` at a layout the repository doesn't use**
  - Consequence: `setuptools.packages.find` silently discovers zero packages (or the wrong ones), and the built wheel is empty or incomplete.
  - Instead: match `where` to the real top-level layout, and verify with `python -m build` before publishing.

# Check list
- [ ] `pyproject.toml` exists at the repository root with `[build-system]`, `[project]`, and `[project.scripts]`.
- [ ] `pip install -e .` succeeds locally and installs a runnable console command.
- [ ] `pip install git+https://github.com/{org}/{app-name}.git` succeeds from a clean virtual environment.
- [ ] Running the installed console command invokes `{App}/cli.py`'s `main()`.
