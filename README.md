# AI skills

A library of reusable AI agent skills for software engineering tasks.

## Why

AI agents work best when they follow clear, consistent instructions. This repository collects those instructions as *skills*: markdown files that an agent can read and apply when solving problems in a specific domain.

Use this library when you want:

- A shared source of truth for how AI agents should behave in your projects.
- Consistent patterns across multiple tech stacks (Angular, .NET, Python, DevOps, and more).
- Version-controlled skills that can be synchronized into an IDE extension or agent workspace.

## Installation

Requires Python 3.8 or later and Git.

On Windows:

```powershell
scripts\ai-skills\init.ps1
```

On Linux or macOS, create the environment and install dependencies manually:

```bash
python3 -m venv .venv
.venv/bin/pip install -r requirements.txt
```

See [docs/installation.md](docs/installation.md) for platform-specific notes and troubleshooting.

## Quick start

1. Initialize the workspace (creates `.venv` and installs `ai-skills-manager`):

   ```powershell
   scripts\ai-skills\init.ps1
   ```

2. Synchronize the skills into the local agent directories:

   ```powershell
   .venv\Scripts\Activate.ps1
   scripts\ai-skills\sync.ps1
   ```

After the sync finishes, the configured skills appear under `.agents/skills/` and `.claude/skills/` (depending on `ai-skills.yaml`).

## Documentation

- [Installation and setup](docs/installation.md) — detailed setup for Windows, Linux, and macOS.
- [API reference](docs/api/reference.md) — commands, configuration, and testing.

## Project structure

```
.
├── ai-skills.yaml          # Sync configuration for ai-skills-manager
├── skills/                 # Source skill files organized by stack/domain
├── scripts/ai-skills/        # Helper scripts for local setup and sync
├── templates/                # Skill templates
├── test/                     # Validation scripts for the sync pipeline
└── docs/                     # Human-readable documentation
```

- `skills/` — the actual skill library. Each skill is a markdown file with frontmatter describing when and how an agent should use it.
- `scripts/ai-skills/` — convenience wrappers for `ai-skills-manager`.
- `ai-skills.yaml` — declares where skills come from, where they should be copied, and how conflicts are handled.
- `test/` — PowerShell and Bash scripts that run the sync in a clean environment to verify the configuration.

## Contributing

Skills are synchronized from upstream repositories defined in `ai-skills.yaml`. To change the library, update the source skill and run `scripts/ai-skills/sync.ps1` (or `aism sync`) so the local copy reflects the latest version.

For the skill authoring conventions used in this library, see `templates/skill.md`.
