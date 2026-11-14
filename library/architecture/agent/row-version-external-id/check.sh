#!/usr/bin/env bash
# The completed prefix of the row-version migration; --full checks all stacks.
set -euo pipefail
cd "$(git rev-parse --show-toplevel)"
python3 - "${1:---base}" <<'PY'
from pathlib import Path
import sys
root=Path('library/architecture')
for stack,name in [('go','version.go'),('python','version.py')]:
    postgres=root/stack/f'vp-postgres-in-{stack}.skill'
    rel=Path('internal/infrastructure/postgres') if stack=='go' else Path('src/todo/infrastructure/postgres')
    source=(postgres/'examples'/rel/name).read_bytes()
    for vp in ['vp-unit-of-work']+(['vp-row-version'] if sys.argv[1]=='--full' else []):
        target=root/stack/f'{vp}-in-{stack}.skill/examples'/rel/name
        assert target.read_bytes()==source, f'version mechanism drift: {target}'
    if sys.argv[1]=='--full':
        skill=root/stack/f'vp-row-version-in-{stack}.skill'
        manifest=(skill/'conformance.copy').read_text()
        assert 'vp-row-version.skill/conformance ' in manifest, f'missing row-version scenarios: {skill}'
        assert 'vp-postgres.skill/conformance/postgres ' in manifest, f'missing inherited database scenarios: {skill}'
if sys.argv[1]=='--full':
    assert (root/'dotnet/vp-row-version-in-dotnet.skill/examples/src/App/App.Host/Api/Cli/Endpoint/ListsHandler.cs').is_file()
print('row-version mechanism copies ok')
PY
for check in check-link-direction check-vp-attach check-vp-diagrams check-templates check-conformance; do
    log=$(mktemp)
    if bash "test/$check.sh" > "$log" 2>&1; then
        tail -1 "$log"
    else
        cat "$log"
        rm -f "$log"
        exit 1
    fi
    rm -f "$log"
done

python3 test/check-architecture-tags.py
