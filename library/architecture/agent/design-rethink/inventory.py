#!/usr/bin/env python3
"""Derive the wave-2 review inventory from tracked skill references and old paths."""
from pathlib import Path
import json
import subprocess

ROOT = Path(__file__).resolve().parents[4]
HERE = Path(__file__).resolve().parent
SHARED = ('api-client', 'app-layers', 'app-api-layer', 'app-command-layer', 'app-domain-layer')
OLD_PATHS = ('infrastructure/client', 'Infrastructure/Client')

def derive():
    paths = [Path(p) for p in subprocess.check_output(
        ['git', 'ls-files', 'library'], cwd=ROOT, text=True).splitlines()]
    rows = []
    for rel in paths:
        if not rel.name.endswith('.skill.md') or any(x in rel.parts for x in ('examples', 'templates')):
            continue
        content = (ROOT / rel).read_text()
        reasons = [n for n in SHARED + OLD_PATHS if n in content]
        if not reasons:
            if str(rel).startswith('library/architecture/'):
                reasons = ['current-catalog-conformance-review']
            else:
                continue
        if str(rel).startswith('library/develop/core/') and (rel.parent.name in tuple(name + '.skill' for name in SHARED) or rel.name == 'app-domain-layer.skill.md'):
            state = 'wave-1-shared-contract'
        elif str(rel).startswith(('library/architecture/', 'library/develop/')):
            state = 'wave-2-review'
        else:
            state = 'legacy-review-only-no-migration'
        prefix = rel.parent.as_posix() + '/'
        owned = [p for p in paths if p != rel and p.as_posix().startswith(prefix)] if rel.parent.name.endswith('.skill') else []
        rows.append(dict(skill=str(rel), state=state, evidence=reasons,
                         owned_file_count=len(owned),
                         example_roots=sorted({str(rel.parent / name) for name in ('examples', 'templates') if (ROOT / rel.parent / name).is_dir()})))
    return dict(integration_surfaces=['library/architecture/README.md', 'library/architecture/docs/catalog.md', 'library/architecture/backlog/client-code-organization.md', 'test/check-vp-attach.sh', 'test/check-conformance.sh', 'test/check-templates.sh', 'test/check-vp-diagrams.sh'],
                scope='Current develop/architecture consumers migrate in wave 2; legacy catalogs are inventoried for review only.',
                ownership='A directory skill owns its ADRs, diagrams, templates, examples, manifests and project files; child assets are covered by their owning root, not individually reviewed here.',
                entries=rows)

if __name__ == '__main__':
    output = json.dumps(derive(), indent=2) + '\n'
    import sys
    target = HERE / 'wave-2-inventory.json'
    if '--check' in sys.argv:
        if not target.exists() or target.read_text() != output:
            raise SystemExit('Inventory stale: run python3 library/architecture/agent/design-rethink/inventory.py')
        print(f'Wave-2 inventory OK: {len(derive()["entries"])} skill consumers classified.')
    else:
        target.write_text(output)
        print(f'Wrote {target.relative_to(ROOT)}')
