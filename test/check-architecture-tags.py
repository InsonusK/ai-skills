#!/usr/bin/env python3
"""Validate architecture feature metadata against the shared testing vocabulary."""
import json
import os
import re
import sys
import tomllib
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
CATALOG = ROOT / 'library/architecture'
vocabulary = json.loads((ROOT / 'library/testing/core/solution-conformance-testing.skill/assets/tools/testing/tags.json').read_text(encoding="utf-8"))
types = set()
for item in vocabulary['type']:
    types.update('@type/' + item['tag'] + '/' + value for value in item.get('values', []))
    if 'values' not in item:
        types.add('@type/' + item['tag'])
categories = {'@category/' + item['tag'] for item in vocabulary['category']}
allowed = types | categories | {'@status/todo', '@status/broken'}
errors = []
feature_tags = {}


def fail(path, line, message):
    errors.append(f'{path.relative_to(ROOT)}:{line}: {message}')


def feature_files(folder):
    for directory, children, files in os.walk(folder):
        children[:] = [name for name in children if name not in {'.venv', 'bin', 'obj', '__pycache__', '.pytest_cache', '.mypy_cache', 'TestResults'}]
        for name in files:
            if name.endswith('.feature'):
                yield Path(directory) / name


for path in feature_files(CATALOG):
    tags_seen = set()
    pending = []
    feature_category = []
    rule_category = []
    scenario = None
    found_feature = False

    def check_category(line, extra=()):
        effective = set(feature_category + rule_category + scenario['category'] + list(extra))
        if len(effective) != 1:
            fail(path, line, f'expected one effective category, got {sorted(effective)}')

    def finish_scenario():
        if scenario and not scenario['examples']:
            check_category(scenario['line'])

    for number, raw in enumerate(path.read_text(encoding="utf-8").splitlines(), 1):
        line = raw.strip()
        if not line or line.startswith('#'):
            continue
        if line.startswith('@'):
            tags = line.split('#', 1)[0].split()
            pending.extend(tags)
            tags_seen.update(tags)
            for tag in tags:
                if tag not in allowed:
                    fail(path, number, f'unknown tag {tag}')
            continue
        own_category = [tag for tag in pending if tag.startswith('@category/')]
        if line.startswith('Feature:'):
            found_feature = True
            declared = [tag for tag in pending if tag.startswith('@type/')]
            if len(declared) != 1:
                fail(path, number, f'expected one feature type, got {declared}')
            feature_category = own_category
            pending = []
        elif line.startswith('Rule:'):
            finish_scenario()
            scenario = None
            if any(tag.startswith('@type/') for tag in pending):
                fail(path, number, 'type tags belong on Feature')
            rule_category = own_category
            pending = []
        elif re.match(r'Scenario(?: Outline)?:', line):
            finish_scenario()
            if any(tag.startswith('@type/') for tag in pending):
                fail(path, number, 'type tags belong on Feature')
            scenario = {'line': number, 'category': own_category, 'examples': 0}
            pending = []
        elif line.startswith('Examples:') and scenario:
            if any(tag.startswith('@type/') for tag in pending):
                fail(path, number, 'type tags belong on Feature')
            scenario['examples'] += 1
            check_category(number, own_category)
            pending = []
    finish_scenario()
    if not found_feature:
        fail(path, 1, 'no Feature declaration')
    feature_tags[path] = tags_seen

# pytest-bdd turns feature tags into pytest marks; strict marker runs must
# recognise the same metadata as the repository gate.
for project in (CATALOG / 'python').glob('*.skill/examples*/pyproject.toml'):
    config = tomllib.loads(project.read_text(encoding="utf-8"))
    marks = config.get('tool', {}).get('pytest', {}).get('ini_options', {}).get('markers', [])
    registered = {mark.split(':', 1)[0].strip() for mark in marks}
    used = set().union(*(tags for path, tags in feature_tags.items() if path.is_relative_to(project.parent)))
    for missing in sorted({tag[1:] for tag in used} - registered):
        fail(project, 1, f'pytest mark is not registered: {missing}')

# Every common proto HTTP contract consumer also keeps its own static pin.
# Equality alone would miss an operation accidentally removed from both sides.
http_pins = 0
for manifest in CATALOG.glob('*/' + '*.skill/conformance.copy'):
    for entry in manifest.read_text(encoding="utf-8").splitlines():
        if not entry.startswith('vp-http-api.skill/conformance/proto-http-contract '):
            continue
        destination = entry.split()[1]
        stack = manifest.parent.parent.name
        pin = manifest.parent / destination / f'operation-list-in-{stack}.feature'
        if not pin.is_file():
            fail(manifest, 1, f'missing HTTP operation pin: {pin.relative_to(ROOT)}')
        elif 'the HTTP API and its OpenAPI contract both have exactly the operations:' not in pin.read_text(encoding="utf-8"):
            fail(pin, 1, 'missing static HTTP operation assertion')
        else:
            http_pins += 1

if errors:
    print('\n'.join(errors), file=sys.stderr)
    raise SystemExit(1)
print(f'architecture tags ok: {len(feature_tags)} features, controlled types/categories, pytest registrations, {http_pins} HTTP pins')
