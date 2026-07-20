# API reference

This page documents the commands, configuration file, and test scripts used to work with the `ai-skills` library.

## `scripts/ai-skills/init.ps1`

Initialize the local workspace.

Use this command the first time you clone the repository, or after deleting the `.venv` directory, to install the Python environment and dependencies.

### Parameters

None.

### Example

```powershell
scripts\ai-skills\init.ps1
```

### What it does

- Creates a Python virtual environment in `.venv`.
- Activates the environment.
- Runs `pip install -r requirements.txt`.

### Common errors

- `python` is not on `PATH`: install Python and add it to your environment variables.
- PowerShell execution policy blocks the script: run with `-ExecutionPolicy Bypass` or change the policy for the current user.

## `scripts/ai-skills/sync.ps1`

Synchronize the skills from the configured sources into the local agent directories.

Use this command after updating `ai-skills.yaml` or when you want to refresh the local skills from upstream.

### Parameters

| Parameter | Type   | Required | Description                          |
| --------- | ------ | -------- | ------------------------------------ |
| `-v`      | switch | No       | Print verbose output during the sync. |
| `-help`   | switch | No       | Show usage information.               |

### Examples

Run a normal sync:

```powershell
scripts\ai-skills\sync.ps1
```

Run a sync with verbose output:

```powershell
scripts\ai-skills\sync.ps1 -v
```

Show help:

```powershell
scripts\ai-skills\sync.ps1 -help
```

### What it does

- Activates the `.venv` environment.
- Runs `aism sync`.
- Exits with the same status code as `aism` so failures are visible to CI or automation.

### Common errors

- Virtual environment is missing: run `scripts\ai-skills\init.ps1` first.
- `aism` exits with a non-zero status: check the error output for conflicts, missing sources, or network issues.

## `aism sync`

The underlying `ai-skills-manager` command that performs the synchronization.

Use this command directly if you prefer to manage the virtual environment yourself.

### Parameters

| Parameter | Type   | Required | Description                          |
| --------- | ------ | -------- | ------------------------------------ |
| `-v`      | flag   | No       | Print verbose output during the sync. |

### Example

```powershell
.venv\Scripts\Activate.ps1
aism sync
```

### Common errors

- `aism` not found: activate the virtual environment or reinstall dependencies.
- Sync error on conflict: check `ai-skills.yaml` for `on_conflict: error` and resolve any local changes before syncing.

## `ai-skills.yaml`

Declarative configuration for `ai-skills-manager`. It tells the tool where to read skills from and where to write them.

### Top-level fields

- `sources` (list, required): one or more sources to sync skills from. Each source has:
  - `path` (string, required): URL or local path of the source repository.
  - `type` (string, required): source type, for example `github` or `local`.
  - `tree` (string, optional): branch or tag to use when `type` is `github`.
  - `subpath` (list, optional): paths inside the source to copy.
- `settings` (map, required): global sync settings.
  - `target` (map, required): output configuration.
    - `default` (map, required): default target location.
      - `path` (string, required): destination directory, for example `.agents/skills`.
    - `claude` (map, optional): target for Claude-compatible adapters.
      - `path` (string, required): destination directory, for example `.claude/skills`.
      - `adapters` (list, required): adapters to apply, for example `claude-property-adapter`.
    - `for_each` (map, optional): adapters applied to every target.
      - `adapters` (list, optional): adapter names, for example `link-adapter`.
  - `remove_orphans` (boolean, required): when `true`, delete local skills that no longer exist in the source.
  - `on_conflict` (string, required): how to handle local changes. Common values: `error`, `overwrite`.
  - `dry_run` (boolean, required): when `true`, preview changes without writing files.

### Example

```yaml
sources:
  - path: https://github.com/InsonusK/ai-skills.git
    type: github
    tree: master
    subpath:
      - skills/common-workflow/work-in-git-tree.skill.md
      - skills/common-workflow/mermaid-diagram.skill.md

settings:
  target:
    for_each:
      adapters:
        - link-adapter
    default:
      path: .agents/skills
  remove_orphans: true
  on_conflict: error
  dry_run: false
```

### Common errors

- `on_conflict: error` fails when a local file differs from the source. Resolve the conflict or set `on_conflict: overwrite` temporarily.
- Missing `subpath` entries copy the whole repository, which is usually not intended. Be explicit about the skills you want.

## `test/test.ps1`

Validate the sync pipeline on Windows.

Use this script to confirm that `ai-skills.yaml` is valid and that the sync produces the expected files.

### Parameters

None.

### Example

```powershell
test\test.ps1
```

### What it does

- Creates a fresh `.venv` inside the `test/` directory.
- Installs `requirements.txt`.
- Runs `aism sync` with the test configuration (`test/ai-skills.yaml`).

### Common errors

- `aism` not found after installation: check that the test virtual environment was created successfully and that `pip` completed without errors.
- Sync fails with `on_conflict: error`: the test directory may contain stale files from a previous run. Delete `test/.venv` and `test/tmp/` and run the test again.

## `test/test.sh`

Validate the sync pipeline on Linux or macOS.

Use this script in CI or on a Unix-like workstation to verify the same behavior as `test/test.ps1`.

### Parameters

None.

### Example

```bash
test/test.sh
```

### What it does

- Creates a fresh `.venv` inside the `test/` directory.
- Installs `requirements.txt`.
- Runs `aism sync` with the test configuration.

### Common errors

- `python3` not found: install Python 3.
- Script fails with `set -e`: the error is printed at the end of the output. Fix the failing step and rerun.

## `Makefile` targets

The `Makefile` provides shortcuts for the most common tasks.

| Target          | Description                                              |
| --------------- | -------------------------------------------------------- |
| `init`          | Create `.venv` and install dependencies, then run sync. |
| `ai-skill-sync` | Run `aism sync` in the existing environment.               |
| `test-lib`      | Run `test/test.sh` to validate the sync pipeline.        |

### Example

```bash
make test-lib
```
