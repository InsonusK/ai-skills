#!/usr/bin/env bash
# Cheap source-contract check; full runtime verification uses make test-and-report.
set -euo pipefail
ROOT=$(git rev-parse --show-toplevel)
SKILL="$ROOT/skills/testing/angular/solution-conformance-testing-in-angular.skill"
CORE="$ROOT/skills/testing/core/solution-conformance-testing.skill/assets"
TS="$ROOT/skills/testing/typescript/solution-conformance-testing-in-typescript.skill"
EXAMPLE="$SKILL/example"
for file in tools/testing/testing.mk tools/testing/testing.sh tools/testing/test-report.sh \
  tools/testing/kind.sh tools/testing/normalize-scenarios.sh tools/testing/messages-results.jq \
  tools/livingdoc/render.mjs tools/livingdoc/package.json tools/livingdoc/package-lock.json; do
  cmp "$CORE/$file" "$EXAMPLE/$file"
done
for file in tools/testing/kinds/components.sh tools/testing/kinds/ui.sh \
  tools/testing/angular-results.mjs vitest.components.config.mts; do
  cmp "$SKILL/assets/$file" "$EXAMPLE/$file"
done
cmp "$TS/assets/tools/testing/kinds/unit.sh" "$EXAMPLE/tools/testing/kinds/unit.sh"
cmp "$TS/assets/cucumber.mjs" "$EXAMPLE/cucumber.mjs"
python3 - "$SKILL" "$TS" <<'PY'
from pathlib import Path
import sys, json
skill, ts = map(Path, sys.argv[1:]); example = skill/'example'
base = (ts/'assets/tools/testing/kinds/mutation.sh').read_text()
selector = '  ' + (skill/'templates/domain-delta-selection.sh').read_text().strip().replace('{DomainRoot}', 'src/linkcheck')
old = next(line for line in base.splitlines() if line.startswith('  FILES='))
assert (example/'tools/testing/kinds/mutation.sh').read_text() == base.replace(old, selector).replace('no src/**/*.ts file changed', 'no domain source file changed')
config = (skill/'templates/playwright.ui.config.ts').read_text().replace('{SourceRoot}', 'src').replace('{BaseUrl}', 'http://127.0.0.1:4387').replace('{ServeCommand}', 'npm run start -- --host 127.0.0.1 --port 4387')
assert (example/'playwright.ui.config.ts').read_text() == config
for source in (ts/'example/src').rglob('*'):
    if source.is_file(): assert source.read_bytes() == (example/'src'/source.relative_to(ts/'example/src')).read_bytes(), source
assert (example/'src/app/test/__screenshots__/validation.ui.spec.ts/linkcheck-form.png').is_file()
assert json.loads((example/'package-lock.json').read_text())['packages']['']['version'] == '0.1.0'
assert 'test-and-report:' not in (example/'Makefile').read_text()
assert 'playwright install --with-deps chromium' in (example/'Makefile').read_text()
PY
make -s -C "$EXAMPLE" test-kinds
make -s -C "$EXAMPLE" test-readme-check
bash "$ROOT/skills/testing/agent/check.sh"
echo 'Angular example: source contract and copied assets passed'
