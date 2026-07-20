# Installation and setup

This guide walks you through installing the local environment used to synchronize the AI skills library.

## Prerequisites

- **Python** 3.8 or later (3.11 recommended).
- **Git** 2.25 or later.
- **PowerShell** 5.1 or later, or PowerShell Core, if you use the Windows scripts.
- A network connection to clone `ai-skills-manager` and download Python packages.

## Windows

1. Open PowerShell in the repository root.
2. Run the initialization script:

   ```powershell
   scripts\ai-skills\init.ps1
   ```

   This creates a `.venv` virtual environment and installs the packages listed in `requirements.txt`.

3. Verify the installation:

   ```powershell
   .venv\Scripts\Activate.ps1
   aism --help
   ```

   You should see the `ai-skills-manager` CLI help output.

## Linux and macOS

The repository does not ship a dedicated Linux/macOS init script, but the equivalent steps are:

1. Create a virtual environment:

   ```bash
   python3 -m venv .venv
   ```

2. Install dependencies:

   ```bash
   .venv/bin/pip install -r requirements.txt
   ```

3. Verify the installation:

   ```bash
   .venv/bin/aism --help
   ```

You can also use the `Makefile` target:

```bash
make init
```

## What the init script does

`scripts/ai-skills/init.ps1` runs these commands automatically:

```powershell
python -m venv .venv
.\.venv\Scripts\Activate.ps1
pip install -r requirements.txt
```

It installs `ai-skills-manager` from the upstream repository, plus any additional dependencies.

## Troubleshooting

### `python` is not recognized

Install Python and make sure it is on your `PATH`. On Windows, use the Python installer from python.org and check the **Add Python to PATH** option.

### PowerShell execution policy blocks the script

If you see an error about execution policy, run the script with an explicit policy for the current session:

```powershell
powershell -ExecutionPolicy Bypass -File scripts\ai-skills\init.ps1
```

### `aism` is not found after init

Make sure the virtual environment is activated:

```powershell
.venv\Scripts\Activate.ps1
```

If it is still missing, reinstall dependencies:

```powershell
pip install -r requirements.txt
```

### Git access errors

`ai-skills-manager` clones repositories listed in `ai-skills.yaml`. If you see authentication errors, ensure your Git credentials or SSH keys are configured for the host in the error message.
