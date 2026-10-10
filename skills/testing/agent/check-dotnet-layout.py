#!/usr/bin/env python3
"""Wave gate for DOTNET-LAYOUT-INVARIANTS.md; run from the repository root."""
import argparse
import re
import subprocess
from pathlib import Path

parser = argparse.ArgumentParser()
parser.add_argument('--wave', type=int, choices=(1, 2, 3), default=3)
args = parser.parse_args()
base = 'b3570c54'
failures = []

def check(condition, message):
    if not condition:
        failures.append(message)

def git(*argv):
    return subprocess.check_output(['git', *argv], text=True, stderr=subprocess.DEVNULL)

cucumber = Path('skills/testing/dotnet/cucumber-testing-in-dotnet.skill/cucumber-testing-in-dotnet.skill.md')
check(cucumber.is_file(), 'folder Cucumber skill missing')
check(not Path('skills/testing/dotnet/cucumber-testing-in-dotnet.skill.md').exists(), 'old single-file skill still exists')
solution = Path('skills/dotnet/architecture/solutions/solution-dotnet-conformance-testing.skill')
vp4 = Path('skills/dotnet/architecture/solutions/solution-domain-shared-rules.skill')
roots = [solution, vp4, Path('skills/testing/dotnet')]
if args.wave >= 2:
    roots.append(Path('skills/dotnet/architecture/plateau'))

markdown = []
for root in roots:
    for p in root.rglob('*.md'):
        if 'examples' in p.parts or 'adr' in p.parts or 'agent' in p.parts:
            continue
        if root.name == 'plateau' and not any(part in ('plateau-core', 'plateau-domain-service', 'plateau-offline-sync-service') for part in p.parts):
            continue
        markdown.append(p)
        s = p.read_text()
        check('StepDefinitions' not in s, f'{p}: old binding layout')
        check(not re.search(r'(?<![.\w])Rules[/\\]|/Rules(?=[`\n /\\])', s), f'{p}: old feature layout')

for p in (solution / 'Implementation').rglob('*.md'):
    s = p.read_text()
    check('cucumber-testing-in-dotnet.skill/cucumber-testing-in-dotnet.skill.md' in s, f'{p}: no owner link')
    check('```csharp' not in s and '# NuGet Packages' not in s, f'{p}: generic template/packages duplicated')
    check('unchanged' not in s, f'{p}: unchanged template claim retained')

# Compare changed skills to the task base. Preserve every contributor; require a higher version.
changed = set(git('diff', '--name-only', base).splitlines())
changed.update(git('ls-files', '--others', '--exclude-standard').splitlines())
for name in sorted(changed):
    p = Path(name)
    if not p.is_file() or p.suffix != '.md' or 'agent' in p.parts or 'adr' in p.parts or 'examples' in p.parts:
        continue
    s = p.read_text()
    now = re.search(r'^version:\s*(\d+)', s, re.M)
    check(now is not None, f'{p}: changed skill lacks version')
    try:
        before = git('show', f'{base}:{name}')
    except subprocess.CalledProcessError:
        continue  # moved/new skill
    previous = re.search(r'^version:\s*(\d+)', before, re.M)
    check(not previous or now and int(now[1]) > int(previous[1]), f'{p}: version not raised')
    def contributors(text):
        m = re.search(r'^created_by:\n((?:[ \t]+.*\n)*)', text, re.M)
        return set(re.findall(r'\[\[([^]|]+)', m[1])) if m else set()
    check(contributors(before) <= contributors(s), f'{p}: original contributor removed')
    # Ensure metadata names keep their established identity through propagation.
    for field in ('name', 'plateau', 'element_kind', 'change_kind'):
        old_field = re.search(rf'^{field}:.*$', before, re.M)
        new_field = re.search(rf'^{field}:.*$', s, re.M)
        check(not old_field or new_field and old_field[0] == new_field[0], f'{p}: {field} changed')

# The existing resolver excludes fenced examples; every real link in the affected files must resolve.
link_files = sorted({str(p) for p in markdown} | {n for n in changed if Path(n).is_file() and n.endswith('.md')})
rows = subprocess.check_output(['perl', 'skills/testing/agent/links.pl', *link_files], text=True)
for row in rows.splitlines():
    cols = row.split('\t')
    if cols[-1] == 'broken':
        failures.append(f'{cols[0]}:{cols[1]}: broken link {cols[2]}')
    # Verify concrete Markdown fragments on task-owned rule links, not just file existence.
    if cols[-1] == 'ok' and '#' in cols[2] and cols[3].endswith('.md'):
        anchor = cols[2].split('#', 1)[1]
        headings = re.findall(r'^#{1,6}\s+(.+?)\s*$', Path(cols[3]).read_text(), re.M)
        def slug(heading):
            return re.sub(r'[^\w\s-]', '', heading.lower()).replace(' ', '-')
        if anchor in ('keep-tests-in-separate-test-projects', 'exercise-production-code-from-bindings',
                      'one-binding-class-per-domain-concept', 'assert-the-concrete-ardalisresult-state',
                      'allowed-dependencies', 'testing-conventions'):
            check(anchor in {slug(heading) for heading in headings}, f'{cols[0]}:{cols[1]}: missing rule anchor {anchor}')

if args.wave >= 2:
    plateau = Path('skills/dotnet/architecture/plateau')
    for label, projects in [('core', 4), ('domain-service', 5), ('offline-sync-service', 6)]:
        root = plateau / f'plateau-{label}'
        test_projects = list((root / 'structure').glob('*.Tests/*csproj*.skill.md'))
        check(len(test_projects) == projects, f'{root}: wrong test-project count')
        for p in test_projects:
            project = p.parent.name.removesuffix('.Tests')
            production = list((root / 'structure' / project).glob('*csproj*.skill.md'))
            check(len(production) == 1, f'{p}: production counterpart missing/ambiguous')
            if len(production) == 1:
                check(str(production[0]) + '#allowed-dependencies' in p.read_text(), f'{p}: production dependency boundary not linked')
            check('cucumber-testing-in-dotnet.skill/cucumber-testing-in-dotnet.skill.md' in p.read_text(), f'{p}: no layout owner link')
        for p in (root / 'structure').glob('*.Tests/classes/*rule-steps.skill.md'):
            check('```csharp' not in p.read_text(), f'{p}: duplicate binding template')
    main = (solution / 'solution-dotnet-conformance-testing.skill.md').read_text()
    check('VP1' in main and 'VP4' in main and '`{Module}.Api` has no dedicated test project' in main, 'project selection changed')

if args.wave >= 3:
    examples = list(Path('skills/dotnet/architecture/plateau').glob('*/plateau-*.skill/examples'))
    examples.append(Path('skills/testing/dotnet/solution-conformance-testing-in-dotnet.skill/examples'))
    tracked = git('ls-files', '-co', '--exclude-standard').splitlines()
    # A folder migration must preserve both feature text and executable behavior.
    for old in git('ls-tree', '-r', '--name-only', base).splitlines():
        if not any(old.startswith(str(example) + '/') for example in examples):
            continue
        source = Path(old)
        if source.suffix not in ('.feature', '.cs'):
            continue
        target = Path(*('features' if part == 'Rules' else 'Steps' if part == 'StepDefinitions' else part for part in source.parts))
        check(target.is_file(), f'{target}: original feature/production/step file lost')
        if target.is_file():
            before = git('show', f'{base}:{old}')
            after = target.read_text()
            if source.suffix == '.cs':
                before = before.replace('.StepDefinitions;', '.Steps;')
            check(before == after, f'{target}: behavior or feature text changed during rename')
    for example in examples:
        paths = [Path(n) for n in tracked if n.startswith(str(example) + '/') and Path(n).is_file()]
        check(any(p.suffix == '.feature' for p in paths), f'{example}: no feature files')
        for p in paths:
            check('StepDefinitions' not in p.parts and 'Rules' not in p.parts, f'{p}: old folder')
            if p.suffix in ('.cs', '.csproj', '.json', '.sh', '.md'):
                check('StepDefinitions' not in p.read_text(), f'{p}: stale binding reference')
                check(not re.search(r'(?<![.\w])Rules[/\\]', p.read_text()), f'{p}: stale feature reference')

if failures:
    print('\n'.join('FAIL: ' + message for message in failures))
    raise SystemExit(1)
print(f'.NET layout: wave {args.wave} checks passed ({len(markdown)} skills, links, versions and provenance)')
