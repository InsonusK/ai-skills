#!/usr/bin/env bash
# Check the delivered prefix of the CLI chain; defaults to the whole chain.
set -euo pipefail
cd "$(git rev-parse --show-toplevel)"
python3 - "${1:-row-version}" <<'PY'
from pathlib import Path
import fnmatch, re, sys
catalog = Path('library/architecture/dotnet')
chain = ['cli-api', 'postgres', 'unit-of-work', 'row-version']
through = sys.argv[1]
assert through in chain, f'unknown wave: {through}'
errors = []
def check(ok, message):
    if not ok: errors.append(message)
def files(folder):
    ignored = {'bin', 'obj', 'tmp', 'TestResults', '__pycache__', '.venv', '.pytest_cache', '.mypy_cache'}
    return {str(p.relative_to(folder)): p for p in folder.rglob('*')
            if p.is_file() and not (set(p.relative_to(folder).parts) & ignored)
            and not p.name.endswith('.feature.cs')}
for name in chain[:chain.index(through)+1]:
    skill = catalog / f'vp-{name}-in-dotnet.skill'
    example = skill / 'examples'
    text = (skill / f'vp-{name}-in-dotnet.skill.md').read_text()
    for heading in ['Goal', 'Core Principle', 'Rule', 'Check list']:
        check(text.count(f'\n# {heading}\n') == 1, f'{name}: heading {heading}')
    check('  - stack/dotnet\n' in text and '  - concern/architecture\n' in text, f'{name}: tags')
    # Every locally linked artifact exists. Root-relative library links and
    # file-relative links both follow the skill-design convention.
    for target in re.findall(r'\]\(([^)]+)\)', text):
        if '://' in target or target.startswith('#'): continue
        target = target.split('#')[0]
        check((Path(target) if target.startswith('library/') else skill/target).exists(), f'{name}: missing link {target}')
    rules = [line.split(maxsplit=1) for line in (skill/'attach.allow').read_text().splitlines()
             if line and not line.startswith('#')]
    base = Path(next(value for kind,value in rules if kind=='base'))
    expected_base = 'base-plateau' if name=='cli-api' else 'cli-api' if name=='postgres' else 'postgres'
    check(base == catalog/f'{"base-plateau" if expected_base=="base-plateau" else "vp-"+expected_base}-in-dotnet.skill/examples', f'{name}: wrong base')
    actual, original = files(example), files(base)
    for path, source in original.items():
        verb = 'delete' if path not in actual else 'change'
        if path in actual and actual[path].read_bytes() == source.read_bytes(): continue
        check(any(kind==verb and fnmatch.fnmatchcase(path,pattern) for kind,pattern in rules), f'{name}: unlisted {verb} {path}')
    for line in (skill/'conformance.copy').read_text().splitlines():
        if not line or line.startswith('#'): continue
        source, destination = line.split()
        source = Path('library/architecture/spec')/source
        check(source.is_dir(), f'{name}: missing conformance source {source}')
        for feature in source.glob('*.feature'):
            copy = skill/destination/feature.name
            check(copy.is_file() and copy.read_bytes()==feature.read_bytes(), f'{name}: stale copy {copy}')
    for resource in (example/'src/App/App.Host/Api/Cli/Endpoint').glob('*Handler.cs'):
        resource_name = resource.stem.removesuffix('Handler').lower()
        features = example/'src/App/App.Host.Tests/features/Cli'
        check(any(features.glob(f'{resource_name}*.feature')), f'{name}: no feature for CLI resource {resource_name}')
    for source in (skill/'templates').rglob('*.cs'):
        target = example/source.relative_to(skill/'templates')
        # templates use no placeholders in fixed code
        if '{Module}' not in str(source):
            check(target.is_file() and source.read_bytes()==target.read_bytes(), f'{name}: fixed template drift {source}')
    for source in (catalog/'vp-cli-api-in-dotnet.skill/templates').rglob('*.cs'):
        target = example/source.relative_to(catalog/'vp-cli-api-in-dotnet.skill/templates')
        check(target.is_file() and source.read_bytes()==target.read_bytes(), f'{name}: CLI template drift {target}')
    if name!='cli-api':
        check(not (example/'docs').exists() and not (example/'third_party').exists(), f'{name}: old API artifacts')
        for path in ['src/App/App.Host/Api/Grpc','src/App/App.Host/Api/Http','src/App/App.Host.Tests/Steps/Http','src/App/App.Host.Tests/Steps/Grpc']:
            check(not (example/path).exists(), f'{name}: old entry point {path}')
        check(not any(line.startswith(('vp-http-api.', 'vp-grpc-api.')) for line in (skill/'conformance.copy').read_text().splitlines()), f'{name}: old API conformance')
    for path in re.findall(r'participant .*?<br/>([^\n]+)', (skill/'diagrams/example-flow.mmd').read_text()):
        check((example/path.strip()).is_file(), f'{name}: missing diagram participant {path}')
if errors:
    print('\n'.join('FAIL: '+error for error in errors)); sys.exit(1)
print(f'dotnet entities ok: CLI chain through {through}; attach, links, templates, scenarios, diagrams, entry-point scope')
PY
