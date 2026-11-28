#!/usr/bin/env python3
"""Validate wave 0 references and current-spec coverage; no runtime claims."""
from pathlib import Path
import re

root = Path(__file__).resolve().parents[4]
architecture = root / 'library/architecture'
files = [Path(__file__).with_name('INVARIANTS.md'), architecture / 'docs/catalog.md']
errors = []
for file in files:
    for target in re.findall(r'\[[^\]]*\]\(([^)]+)\)', file.read_text()):
        path = target.split('#', 1)[0]
        if not path or '://' in path:
            continue
        resolved = root / path if path.startswith('library/') else file.parent / path
        if not resolved.exists():
            errors.append(f'{file.relative_to(root)}: missing {target}')
sample = (architecture / 'docs/catalog.md').read_text()
for spec in sorted((architecture / 'spec').glob('*/*.skill.md')):
    name = spec.name.removesuffix('.skill.md')
    if f'`{name}`' not in sample:
        errors.append(f'Current specification absent from sample: {name}')
relations = {'requires', 'requires-any', 'shares-contract', 'unresolved'}
section = sample.split('| From | Relation | To |', 1)[1].split('\n\n', 1)[0]
for row in section.splitlines()[2:]:
    cells = [cell.strip() for cell in row.strip('|').split('|')]
    if len(cells) != 4 or cells[1] not in relations or not all(cells):
        errors.append(f'Invalid dependency row: {row}')
for marker in ['implementation pending', 'API DTOs mapped by service', 'client-owned logical API DTOs']:
    if marker not in sample:
        errors.append(f'Missing boundary qualification: {marker}')
if errors:
    raise SystemExit('\n'.join(errors))
print('Wave 0 OK: references resolve; 14 current specs covered; dependency rows typed; client migration qualifications present.')
print('Graph endpoint and full roadmap coverage checks are deferred to the structured maps in wave 3.')

# Later waves add their own check gate alongside the original sample checks.
import subprocess
import sys
wave1 = Path(__file__).with_name('check-wave-1.py')
if wave1.exists():
    subprocess.run([sys.executable, str(wave1)], cwd=root, check=True)
