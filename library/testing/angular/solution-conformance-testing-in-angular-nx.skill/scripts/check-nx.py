#!/usr/bin/env python3
"""Exercise affected selection and real runner failures in an isolated two-commit copy."""
import json
import os
from pathlib import Path
import shutil
import subprocess
import tempfile

source = Path(__file__).resolve().parents[1] / 'example'
template = (source.parent / 'templates/playwright.config.mts').read_text()
assert template.replace('{E2eProject}', 'portal-e2e').replace('{HostProject}', 'portal') == (source / 'apps/portal-e2e/playwright.config.mts').read_text(), 'e2e config differs from its template'
for project in source.glob('*/*/project.json'):
    assert 'metadata' not in json.loads(project.read_text()), f'{project}: testing declaration in a generated project'
with tempfile.TemporaryDirectory(prefix='nx-shapes-proof-') as tmp:
    root = Path(tmp) / 'workspace'
    shutil.copytree(source, root, ignore=shutil.ignore_patterns(
        'node_modules', 'tmp', 'out', 'dist', '.angular', '.nx'))
    shutil.copytree(source / 'node_modules', root / 'node_modules', symlinks=True)
    shutil.copytree(source / 'tools/livingdoc/node_modules', root / 'tools/livingdoc/node_modules', symlinks=True)
    def command(args, expected=0, env=None):
        result = subprocess.run(args, cwd=root, env={**os.environ, **(env or {})},
                                stdout=subprocess.PIPE, stderr=subprocess.STDOUT, text=True)
        if (result.returncode == 0) != (expected == 0):
            raise AssertionError(f'{args}: exit {result.returncode}\n{result.stdout[-6000:]}')
        return result.stdout
    command(['git', 'init', '-b', 'main'])
    command(['git', 'config', 'user.email', 'nx-proof@example.invalid'])
    command(['git', 'config', 'user.name', 'Nx proof'])
    command(['git', 'add', '.'])
    command(['git', 'commit', '-m', 'baseline'])
    path = root / 'libs/formatter/src/index.ts'
    path.write_text(path.read_text() + '\n// Changed formatter scope for the delta proof.\n')
    command(['git', 'add', '.'])
    command(['git', 'commit', '-m', 'change formatter'])
    command(['make', 'test-and-report', 'TEST_RUN_PURPOSE=check', 'DELTA_BASE=HEAD~1'])
    work = root / 'tmp/testing/kinds'
    for kind, expected in [('unit', {'formatter', 'portal'}), ('components', {'portal'}), ('ui', {'portal-e2e'})]:
        data = json.loads((work / kind / 'result/projects.json').read_text())
        actual = {project['name'] for project in data['projects'] if project['selected']}
        assert actual == expected, (kind, actual)
        executed = {path.name for path in (work / kind / 'projects').iterdir()}
        assert executed == expected, (kind, executed)
        assert data['errors'] == [], data
        assert 'affected' in (work / kind / 'mode').read_text()
    inventory = json.loads((work / 'unit/result/scenarios.json').read_text())['scenarios']
    assert any('libs/linkcheck/' in scenario['uri'] and scenario['status'] == 'not-run' for scenario in inventory)
    mutation = json.loads((work / 'mutation/report/mutation/reports/mutation-report.json').read_text())
    assert set(mutation['files']) == {'libs/formatter/src/index.ts'}, mutation['files'].keys()
    print('Two-commit check: unit formatter+portal, components portal, UI portal-e2e, mutation formatter file; linkcheck did not run')
    # Repeat the same target, then change only a spec to prove cache cannot mask failure.
    command(['make', 'test-kind-components'])
    spec = root / 'apps/portal/src/app/spec/portal.component.spec.ts'
    saved = spec.read_bytes()
    spec.write_text(spec.read_text().replace("Review 1 link", "deliberately wrong"))
    command(['make', 'test-kind-components'], expected=1)
    badge = json.loads((work / 'components/badges/components.json').read_text())
    assert badge['color'] == 'red'
    summary = json.loads((work / 'components/result/components-test.json').read_text())
    assert any('[portal]' in test['name'] and test['status'] == 'failed' for test in summary['tests'])
    spec.write_bytes(saved)
    print('Changed component expectation: second run executes and fails with portal identity')
    # Catalog browser suffixes must execute as part of UI, alongside *.ui.spec.ts.
    catalog_spec = root / 'apps/portal-e2e/src/portal.a11y.spec.ts'
    catalog_spec.write_text("import { test, expect } from '@playwright/test';\ntest('catalog suite discovery', async ({ page }) => { await page.goto('/'); await expect(page.getByRole('button', { name: 'Check URL' })).toBeVisible(); });\n")
    command(['make', 'test-kind-ui'])
    assert any('catalog suite discovery' in test['name'] for test in json.loads((work / 'ui/result/ui-test.json').read_text())['tests'])
    catalog_spec.unlink()
    print('Catalog *.a11y.spec.ts suite executes in the UI kind')
    # A project with the standard test target and no spec cannot turn the kind green.
    spec_dir = root / 'libs/linkcheck/src/lib/spec'
    hidden = root / 'libs/linkcheck/src/lib/spec.hidden'
    spec_dir.rename(hidden)
    command(['make', 'test-kind-components'], expected=1)
    assert json.loads((work / 'components/badges/components.json').read_text())['color'] == 'red'
    assert any('linkcheck' in error for error in json.loads((work / 'components/result/projects.json').read_text())['errors'])
    hidden.rename(spec_dir)
    print('Project with a test target and no spec: failed component kind, linkcheck named')
    # A feature file whose scenarios are all excluded leaves its project with nothing run.
    feature = root / 'libs/formatter/src/features/heading.feature'
    saved_feature = feature.read_bytes()
    feature.write_text(feature.read_text().replace('Feature:', '@status/todo\nFeature:', 1))
    command(['make', 'test-kind-unit'], expected=1)
    assert json.loads((work / 'unit/badges/tests.json').read_text())['color'] == 'red'
    assert any('formatter' in error for error in json.loads((work / 'unit/result/unit-test.json').read_text())['errors'])
    feature.write_bytes(saved_feature)
    print('Project whose scenarios all are excluded: failed unit kind, formatter named')
    command(['make', 'test-kind-components', 'TEST_RUN_PURPOSE=check', 'DELTA_BASE=HEAD'])
    assert (work / 'components/skipped').is_file()
    print('No affected applicable projects: explicit skip, no green empty suite')
    command(['make', 'test-kind-unit', 'TEST_RUN_PURPOSE=check', 'DELTA_BASE=HEAD'])
    assert (work / 'unit/skipped').is_file()
    zero_inventory = json.loads((work / 'unit/result/scenarios.json').read_text())['scenarios']
    assert len(zero_inventory) == len(inventory)
    assert any(scenario['status'] == 'not-run' for scenario in zero_inventory)
    assert not list((work / 'unit/badges').iterdir())
    assert not list((work / 'unit/report').iterdir())
    print('Zero-affected unit: complete retained inventory, explicit skip, no badge/report')
