#!/bin/bash
# Fails when a file under library/ links a file under skills/.
# library/ is the published catalog; skills/ holds this repository's own skills,
# which other repositories never receive.
set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$ROOT"

out=$(grep -rInE '\[\[skills/|\]\(skills/' library || true)
if [ -n "$out" ]; then
    echo "library/ must not link skills/:" >&2
    echo "$out" >&2
    exit 1
fi
echo "link direction ok: no link from library/ into skills/"
