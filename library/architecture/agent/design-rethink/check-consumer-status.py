#!/usr/bin/env python3
"""Reject catalog-work status in consumer skill prose; ignore legitimate runtime states."""
from pathlib import Path
import re
import subprocess

ROOT = Path(__file__).resolve().parents[4]
paths = [ROOT / p for p in subprocess.check_output(['git', 'ls-files', 'library', 'skills'], cwd=ROOT, text=True).splitlines() if p.endswith('.skill.md')]
patterns = re.compile(r'migration status:|await(?:s|ing)?\s+(?:the\s+)?(?:planned\s+)?(?:wave\s*\d|propagation wave)|await wave\s*\d|draft contract[^\n]*no (?:client|consumer|producer|realization) yet|no v3 prior art|full authoring[^\n]*deferred|README backlog', re.I)
errors = []
for p in paths:
    fenced = False
    for n, line in enumerate(p.read_text().splitlines(), 1):
        if re.match(r'^\s*(```|~~~)', line):
            fenced = not fenced
            continue
        if not fenced and patterns.search(line):
            errors.append(f'{p.relative_to(ROOT)}:{n}: {line.strip()}')
if errors:
    raise SystemExit('\n'.join(errors))
print(f'Consumer-status audit OK: {len(paths)} skill/template files; no catalog-progress markers matched.')
