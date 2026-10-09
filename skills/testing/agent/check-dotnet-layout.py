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
        if 'example' in p.parts or 'adr' in p.parts or 'agent' in p.parts:
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
    if not p.is_file() or p.suffix != '.md' or 'agent' in p.parts or 'adr' in p.parts or 'example' in p.parts:
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

# The existing resolver excludes fenced examples; every real link in the affected files must resolve.
link_files = sorted({str(p) for p in markdown} | {n for n in changed if Path(n).is_file() and n.endswith('.md')})
rows = subprocess.check_output(['perl', 'skills/testing/agent/links.pl', *link_files], text=True)
for row in rows.splitlines():
    cols = row.split('\t')
    if cols[-1] == 'broken':
        failures.append(f'{cols[0]}:{cols[1]}: broken link {cols[2]}')

if args.wave >= 2:
    plateau = Path('skills/dotnet/architecture/plateau')
    for label, projects in [('core', 4), ('domain-service', 5), ('offline-sync-service', 6)]:
        root = plateau / f'plateau-{label}'
        test_projects = list((root / 'structure').glob('*.Tests/*csproj*.skill.md'))
        check(len(test_projects) == projects, f'{root}: wrong test-project count')
        for p in test_projects:
            check('cucumber-testing-in-dotnet.skill/cucumber-testing-in-dotnet.skill.md' in p.read_text(), f'{p}: no layout owner link')
        for p in (root / 'structure').glob('*.Tests/classes/*rule-steps.skill.md'):
            check('```csharp' not in p.read_text(), f'{p}: duplicate binding template')
    main = (solution / 'solution-dotnet-conformance-testing.skill.md').read_text()
    check('VP1' in main and 'VP4' in main and '`{Module}.Api` has no dedicated test project' in main, 'project selection changed')

if args.wave >= 3:
    examples = list(Path('skills/dotnet/architecture/plateau').glob('*/plateau-*.skill/example'))
    examples.append(Path('skills/testing/dotnet/solution-conformance-testing-in-dotnet.skill/example'))
    tracked = git('ls-files', '-co', '--exclude-standard').splitlines()
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
