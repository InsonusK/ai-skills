#!/usr/bin/env python3
"""Exercise real native failures and the shared adapter's missing/empty-result policy."""
from pathlib import Path
import json
import os
import subprocess
import sys
import tempfile

example = Path(sys.argv[1]).resolve()

def run_failure(kind, extra_env=None):
    with tempfile.TemporaryDirectory(prefix='angular-red-') as tmp:
        env = {**os.environ, **(extra_env or {})}
        with open(Path(tmp) / 'runner.log', 'w') as log:
            result = subprocess.run(['make', f'test-kind-{kind}', f'TEST_WORK_DIR={tmp}/work'],
                                    cwd=example, env=env, stdout=log, stderr=subprocess.STDOUT)
        assert result.returncode != 0, f'{kind}: broken runner returned zero'
        kind_dir = Path(tmp) / 'work/kinds' / kind
        assert json.loads((kind_dir / f'badges/{kind}.json').read_text())['color'] == 'red'
        assert (kind_dir / f'report/{kind}/index.html').is_file()
        print(f'{kind}: non-zero, red badge, inspectable report')

for kind, pattern, injected in [
    ('components', '*.component.spec.ts', "\nit('deliberate failure', () => expect(true).toBe(false));\n"),
    ('ui', '*.ui.spec.ts', "\ntest('deliberate failure', async () => expect(true).toBe(false));\n"),
]:
    spec = sorted(path for path in example.rglob(pattern) if path.is_file() and 'node_modules' not in path.parts)[0]
    saved = spec.read_bytes()
    try:
        spec.write_bytes(saved + injected.encode())
        run_failure(kind)
    finally:
        spec.write_bytes(saved)

with tempfile.TemporaryDirectory(prefix='missing-chromium-') as tmp:
    run_failure('ui', {'PLAYWRIGHT_BROWSERS_PATH': tmp})

# No native data, and syntactically valid but empty data, both fail even with runner exit 0.
for kind in ('components', 'ui'):
    for fixture in (None, {'success': True, 'testResults': []} if kind == 'components' else {'suites': []}):
        with tempfile.TemporaryDirectory(prefix='angular-adapter-') as tmp:
            directory = Path(tmp)
            for folder in ('result', 'report', 'badges'):
                (directory / folder).mkdir()
            if fixture is not None:
                (directory / f'result/{kind}.native.json').write_text(json.dumps(fixture))
            result = subprocess.run(['node', 'tools/testing/angular-results.mjs', kind, '0'], cwd=example,
                                    env={**os.environ, 'TEST_KIND_DIR': tmp})
            assert result.returncode != 0
            assert json.loads((directory / f'badges/{kind}.json').read_text())['color'] == 'red'
print('Missing browser, missing native results and empty native suites fail visibly')
