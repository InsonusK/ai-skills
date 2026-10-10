#!/usr/bin/env python3
"""Exercise affected selection and real runner failures in an isolated two-commit copy."""
import json
import os
from pathlib import Path
import shutil
import subprocess
import tempfile

source = Path(__file__).resolve().parents[1] / 'example'
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
    for kind, expected in [('unit', {'formatter', 'portal'}), ('components', {'portal'}), ('ui', {'portal'})]:
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
    print('Two-commit check: unit formatter+portal, components/UI portal, mutation formatter file; linkcheck did not run')
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
    catalog_spec = root / 'apps/portal/src/app/spec/portal.a11y.spec.ts'
    catalog_spec.write_text("import { test, expect } from '@playwright/test';\ntest('catalog suite discovery', async ({ page }) => { await page.goto('/'); await expect(page.getByRole('button', { name: 'Check URL' })).toBeVisible(); });\n")
    command(['make', 'test-kind-ui'])
    assert any('catalog suite discovery' in test['name'] for test in json.loads((work / 'ui/result/ui-test.json').read_text())['tests'])
    catalog_spec.unlink()
    print('Catalog *.a11y.spec.ts suite executes in the UI kind')
    # A selected project cannot disappear because it has no tests.
    empty = root / 'libs/empty'
    empty.mkdir(parents=True)
    (empty / 'project.json').write_text(json.dumps({
        'name': 'empty', 'root': 'libs/empty', 'projectType': 'library',
        'metadata': {'testing': {'unit': True, 'components': False, 'ui': False}},
        'targets': {'conformance-unit': {'executor': 'nx:run-commands', 'cache': False,
          'options': {'command': 'node tools/testing/nx-project.mjs unit empty', 'forwardAllArgs': False}}}
    }))
    command(['make', 'test-kind-unit'], expected=1)
    assert json.loads((work / 'unit/badges/tests.json').read_text())['color'] == 'red'
    assert any('empty' in error for error in json.loads((work / 'unit/result/unit-test.json').read_text())['errors'])
    shutil.rmtree(empty)
    print('Declared empty project: failed unit kind and red repository badge')
    # A command that exits zero but publishes no data is not a successful project test.
    project = root / 'apps/portal/project.json'
    saved = project.read_bytes()
    data = json.loads(saved)
    data['targets']['conformance-components']['options']['command'] = 'node -e "process.exit(0)"'
    project.write_text(json.dumps(data))
    command(['make', 'test-kind-components'], expected=1)
    assert json.loads((work / 'components/badges/components.json').read_text())['color'] == 'red'
    assert any('portal' in error for error in json.loads((work / 'components/result/projects.json').read_text())['errors'])
    project.write_bytes(saved)
    print('Zero-exit target without fresh result: failed component kind and portal evidence')
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
    # Missing metadata/target is a declaration failure, rather than disappearing from run-many.
    data = json.loads(saved)
    del data['targets']['conformance-components']
    project.write_text(json.dumps(data))
    command(['make', 'test-kind-components'], expected=1)
    assert json.loads((work / 'components/badges/components.json').read_text())['color'] == 'red'
    print('Missing declared target: failed kind, red badge')
