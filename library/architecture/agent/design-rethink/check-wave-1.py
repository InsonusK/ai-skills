#!/usr/bin/env python3
"""Check authored contracts, ADR registration, links and migration inventory."""
from pathlib import Path
import re
import subprocess
import sys

ROOT = Path(__file__).resolve().parents[4]
HERE = Path(__file__).resolve().parent
skills = [ROOT / f'library/develop/core/{name}.skill/{name}.skill.md' for name in ('api-client', 'app-layers')]
files = list((ROOT / 'library/develop/core/api-client.skill').rglob('*.md'))
files += list((ROOT / 'library/develop/core/app-layers.skill').rglob('*.md'))
errors = []
def slug(s):
    return re.sub(r'[^\w\- ]', '', s.lower()).replace(' ', '-')
for file in files:
    text = file.read_text()
    for target in re.findall(r'\[[^\]]*\]\(([^)]+)\)', text):
        if '://' in target: continue
        path, _, anchor = target.partition('#')
        resolved = ROOT / path if path.startswith('library/') else file.parent / path if path else file
        if not resolved.exists():
            errors.append(f'{file.relative_to(ROOT)}: missing {target}')
        elif anchor and resolved.is_file():
            headings = re.findall(r'^#+ (.*)$', resolved.read_text(), re.M)
            if anchor not in {slug(h) for h in headings}:
                errors.append(f'{file.relative_to(ROOT)}: missing anchor {target}')
    for path in re.findall(r'@import "([^"]+)"', text):
        if not (file.parent / path).exists():
            errors.append(f'{file.relative_to(ROOT)}: missing diagram {path}')
for skill in skills:
    text = skill.read_text()
    top = re.findall(r'^# (.+)$', text, re.M)
    for title in ('Goal','Core Principle','Rule','Check list'):
        if top.count(title) != 1: errors.append(f'{skill.name}: section {title}')
    if set(top) - {'Goal','Core Principle','Rule','Check list','Scope','Workflow','Example'}:
        errors.append(f'{skill.name}: unsupported sections')
    if not re.search(r'^updated: \d{8}$', text, re.M): errors.append(f'{skill.name}: date')
    if '  - stack\n' not in text or '  - concern/architecture\n' not in text:
        errors.append(f'{skill.name}: facets')
    rule = text.split('# Rule\n',1)[1].split('# Check list',1)[0].split('# Example',1)[0]
    for heading, body in re.findall(r'^### ([^\n]+)\n(.*?)(?=^### |^## |\Z)', rule, re.S | re.M):
        # All current rules before SHOULD/MAY are mandatory.
        must = rule.find('### '+heading) < rule.find('## SHOULD') if '## SHOULD' in rule else True
        if must and ('- Risk:' not in body or '- Fix:' not in body):
            errors.append(f'{skill.name}: missing rule rationale: {heading}')
    for entry in re.findall(r'^  - (adr/[^\n]+)',text,re.M):
        if not (skill.parent / entry).exists() or f'](./{entry})' not in text:
            errors.append(f'{skill.name}: ADR missing or not linked: {entry}')
for folder, name in [('api-client','api-dtos-returned-to-service'),('api-client','entity-clients-and-system-services'),('app-layers','sibling-client-and-system-service-boundaries')]:
    file = ROOT / f'library/develop/core/{folder}.skill/adr/{name}.md'
    text = file.read_text()
    if 'concern/documentation/adr' not in text: errors.append(f'{name}: ADR tag')
    variants = text.split('# Searched variants\n',1)[1]
    if variants.count('**Selected.**') != 1: errors.append(f'{name}: selection')
    for variant in re.split(r'^## ', variants, flags=re.M)[1:]:
        for section in ('Description','Benefits','Costs'):
            if f'### {section}' not in variant: errors.append(f'{name}: {section}')
if errors: raise SystemExit('\n'.join(errors))
subprocess.run([sys.executable, str(HERE / 'inventory.py'), '--check'], cwd=ROOT, check=True)
print('Wave 1 OK: shared skill structure, ADR registration, links/anchors, diagrams and downstream inventory.')
