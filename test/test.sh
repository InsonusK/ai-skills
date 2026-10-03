#!/bin/bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
cd "$SCRIPT_DIR"

if ! command -v aism >/dev/null 2>&1; then
    echo "aism executable not found in PATH. Install it:" >&2
    echo "  curl -fsSL https://raw.githubusercontent.com/InsonusK/go-ai-skill-manage/master/scripts/install.sh | sh" >&2
    exit 1
fi

aism --version
aism sync
