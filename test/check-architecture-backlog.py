#!/usr/bin/env python3
"""Check task/index coverage, statuses, required context and local doc links."""
from pathlib import Path
import re
import sys
from urllib.parse import unquote, urlsplit

ROOT = Path(__file__).resolve().parents[1]
CATALOG = ROOT / 'library/architecture'
BACKLOG = CATALOG / 'backlog'
REQUIRED = (
    'Context', 'Sources and scope', 'Dependencies and start condition',
    'Expected result', 'Validation', 'Open decisions',
)
STATUSES = {'Ready', 'Needs decision', 'Deferred'}
errors = []

def check_links(path):
    for target in re.findall(r'\[[^\]\n]*\]\(([^\s)]+)\)', path.read_text(encoding='utf-8')):
        url = urlsplit(target)
        if url.scheme or url.netloc or not url.path:
            continue
        candidate = path.parent / unquote(url.path)
        if not candidate.exists():
            errors.append(f'{path.relative_to(ROOT)}: missing link {target}')

index = (CATALOG / 'README.md').read_text(encoding='utf-8').split('## Backlog\n', 1)[1]
rows = re.findall(r'^\| \[[^\]]+\]\(backlog/([^/)]+\.md)\) \| ([^|]+) \| ([^|]+) \|$', index, re.M)
linked = [name for name, _, _ in rows]
cards = {p.name for p in BACKLOG.glob('*.md') if p.name != 'README.md'}
if not rows:
    errors.append('Backlog index has no task rows')
if set(linked) != cards:
    errors.append(f'Index/card mismatch: missing cards {set(linked)-cards}; unindexed {cards-set(linked)}')
if len(linked) != len(set(linked)):
    errors.append('Duplicate task card in index')
for name, status, trigger in rows:
    path = BACKLOG / name
    if not path.is_file():
        continue
    text = path.read_text(encoding='utf-8')
    status = status.strip()
    if status not in STATUSES or f'Status: {status}\n' not in text:
        errors.append(f'{name}: missing, invalid or inconsistent status')
    if not trigger.strip():
        errors.append(f'{name}: empty start condition')
    for section in REQUIRED:
        match = re.search(rf'^## {re.escape(section)}\n(.*?)(?=^## |\Z)', text, re.M | re.S)
        if not match or not match.group(1).strip():
            errors.append(f'{name}: missing/empty {section}')
    if '[Shared execution guidance](README.md)' not in text or '(../README.md#backlog)' not in text:
        errors.append(f'{name}: missing shared guidance or index link')
for path in [CATALOG / 'README.md', *BACKLOG.glob('*.md'), CATALOG / 'agent/backlog-cards/PLAN.md']:
    check_links(path)
if errors:
    print('\n'.join(errors), file=sys.stderr)
    sys.exit(1)
print(f'architecture backlog ok: {len(cards)} indexed cards, statuses, context sections and local links')
