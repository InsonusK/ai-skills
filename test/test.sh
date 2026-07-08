#!/bin/bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
cd "$SCRIPT_DIR"

python -m venv .venv

if [ -f ".venv/Scripts/python.exe" ]; then
    VENV_PYTHON=".venv/Scripts/python.exe"
elif [ -f ".venv/bin/python" ]; then
    VENV_PYTHON=".venv/bin/python"
else
    echo "Python executable not found in virtual environment" >&2
    exit 1
fi

"$VENV_PYTHON" -m pip install -r "${SCRIPT_DIR}/../requirements.txt"

if [ -f ".venv/Scripts/aism.exe" ]; then
    AISM=".venv/Scripts/aism.exe"
elif [ -f ".venv/bin/aism" ]; then
    AISM=".venv/bin/aism"
else
    echo "aism executable not found in virtual environment" >&2
    exit 1
fi

"$AISM" sync
