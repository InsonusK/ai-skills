#!/usr/bin/env python3
"""Apply or check the service-qualified namespace migration in current .NET code."""
import argparse
from pathlib import Path
import re
import subprocess
import xml.etree.ElementTree as ET

ROOT = Path(__file__).resolve().parents[4]
SAMPLE = 'TodoExampleService'
TOKEN = '{service-namespace}'
AREAS = ('library/architecture/dotnet', 'library/develop/dotnet')
MAPPING = {
    'App.Host': 'App.Host', 'App.Migrate': 'App.Migrate',
    'BuildingBlocks': 'BuildingBlocks', 'Shared': 'Shared',
    'Middleware': 'Middleware', 'MaskedLogging': 'MaskedLogging',
    'Todo.Commands': 'Modules.Todo.Commands',
    'Todo.Application': 'Modules.Todo.Application',
    'Todo.Domain': 'Modules.Todo.Domain',
    'Todo.Service': 'Service.Todo',
    'Postgres': 'Infrastructure.Database.Postgres',
    'Client.Abstractions': 'Client.Core.Abstractions',
    'Client.Pipeline': 'Client.Core.Pipeline',
    'Client.Http': 'Client.Core.Http',
    'Client.Grpc': 'Client.Core.Grpc',
    'TodoApi': 'Client.TodoApi',
    'Retry': 'Client.Middleware.Retry',
    'Contracts.Todo.V1': 'Contracts.Todo.V1',
}
NAMES = '|'.join(re.escape(n) for n in sorted(MAPPING, key=len, reverse=True))
# Preserve comments and string literals (log categories, routes and paths).
# EF model type-name literals and documentation cref names are handled explicitly.
CODE = re.compile(r'//[^\n]*|/\*[\s\S]*?\*/|@?"(?:\\.|""|[^"\\])*"'
                  r'|(?<![\w.])(?P<name>' + NAMES + r')(?=\.|\b)')


def qualify(name, prefix):
    for old in sorted(MAPPING, key=len, reverse=True):
        if name == old or name.startswith(old + '.'):
            return prefix + '.' + MAPPING[old] + name[len(old):]
    raise ValueError(f'Unknown namespace/project identity: {name}')


def source(text, prefix):
    def replace(match):
        name = match.group('name')
        return prefix + '.' + MAPPING[name] if name else match.group()
    text = CODE.sub(replace, text)
    text = text.replace('"Todo.Domain.Entities.Item"',
                        '"' + prefix + '.Modules.Todo.Domain.Entities.Item"')
    return re.sub(r'(?<=cref=")([\w.]+)',
                  lambda m: qualify(m.group(), prefix)
                  if any(m.group().startswith(n + '.') for n in MAPPING) else m.group(), text)


def tracked():
    return [ROOT / p for p in subprocess.check_output(
        ['git', 'ls-files', '-z', *AREAS], cwd=ROOT,
    ).decode().strip('\0').split('\0') if p]


def apply(files):
    for path in files:
        template = '/templates/' in str(path)
        prefix = TOKEN if template else SAMPLE
        text = path.read_text() if path.suffix in ('.cs', '.csproj', '.proto', '.props') else None
        if text is None:
            continue
        old = text
        if path.suffix == '.cs':
            text = source(text, prefix)
        elif path.suffix == '.csproj':
            if '<RootNamespace>' not in text:
                root = qualify(path.stem, '$(ServiceNamespace)')
                text = text.replace('>\n', '>\n  <PropertyGroup>\n'
                                    f'    <RootNamespace>{root}</RootNamespace>\n'
                                    '  </PropertyGroup>\n', 1)
            text = text.replace('<RootNamespace>' + TOKEN + '.',
                                '<RootNamespace>$(ServiceNamespace).')
        elif path.suffix == '.proto':
            text = re.sub(r'(?<=csharp_namespace = ")([\w.]+)',
                          lambda m: m.group() if m.group().startswith(SAMPLE + '.')
                          else qualify(m.group(), prefix), text)
        elif path.name == 'Directory.Build.props' and '<ServiceNamespace>' not in text:
            text = text.replace('<PropertyGroup>', '<PropertyGroup>\n'
                                f'    <ServiceNamespace>{SAMPLE}</ServiceNamespace>', 1)
        if text != old:
            path.write_text(text)


def check(files, sample_only):
    errors = []
    count = 0
    if sample_only:
        files = [ROOT / 'library/architecture/dotnet/base-plateau-in-dotnet.skill/examples'
                 / 'src/App/App.Host/Version/AppVersion.cs']
    for path in files:
        template = '/templates/' in str(path)
        prefix = TOKEN if template else SAMPLE
        if path.suffix == '.cs':
            text = path.read_text()
            namespaces = re.findall(r'^namespace ([^;\s]+)', text, re.M)
            assembly_attributes = not namespaces and '[assembly:' in text and not re.search(
                r'\b(class|record|struct|interface|enum)\s+\w+', re.sub(r'//[^\n]*', '', text))
            if not assembly_attributes and (not namespaces or any(
                    not n.startswith(prefix + '.') for n in namespaces)):
                errors.append(f'{path.relative_to(ROOT)}: unqualified namespace {namespaces}')
            owner_source = path
            if template:
                skill, relative = str(path).split('/templates/', 1)
                examples = sorted(Path(skill).glob('examples*/' + relative)) or sorted(
                    (ROOT / 'library/architecture/dotnet').glob('*.skill/examples*/' + relative))
                if examples:
                    owner_source = examples[0]
            for parent in owner_source.parents:
                owners = list(parent.glob('*.csproj'))
                if owners:
                    root = ET.fromstring(owners[0].read_text()).findtext('.//RootNamespace')
                    expected_root = (root or '').replace('$(ServiceNamespace)', prefix)
                    if not expected_root or any(n != expected_root and not n.startswith(
                            expected_root + '.') for n in namespaces):
                        errors.append(f'{path.relative_to(ROOT)}: namespace outside owning project {expected_root}')
                    break
                if parent == ROOT:
                    if not sample_only:
                        errors.append(f'{path.relative_to(ROOT)}: no owning project/example')
                    break
            if source(text, prefix) != text:
                errors.append(f'{path.relative_to(ROOT)}: stale type reference')
            if not template and TOKEN in text:
                errors.append(f'{path.relative_to(ROOT)}: unfilled placeholder')
            count += 1
        elif path.suffix == '.csproj':
            project = ET.fromstring(path.read_text())
            expected = qualify(path.stem, '$(ServiceNamespace)')
            if project.findtext('.//RootNamespace') != expected:
                errors.append(f'{path.relative_to(ROOT)}: RootNamespace must be {expected}')
            for ref in project.iter('ProjectReference'):
                target = (path.parent / ref.attrib['Include'].replace('\\', '/')).resolve()
                candidates = [target]
                if template:
                    skill, relative = str(path).split('/templates/', 1)
                    candidates = [(example.parent / ref.attrib['Include'].replace('\\', '/')).resolve()
                                  for example in Path(skill).glob('examples*/' + relative)]
                if not any(p.exists() for p in candidates):
                    errors.append(f'{path.relative_to(ROOT)}: missing reference {target}')
        elif path.name == 'Directory.Build.props':
            if ET.fromstring(path.read_text()).findtext('.//ServiceNamespace') != SAMPLE:
                errors.append(f'{path.relative_to(ROOT)}: missing sample service namespace')
        elif path.suffix == '.proto':
            for ns in re.findall(r'csharp_namespace\s*=\s*"([^"]+)"', path.read_text()):
                if not ns.startswith(prefix + '.'):
                    errors.append(f'{path.relative_to(ROOT)}: unqualified generated namespace')
    if errors:
        raise SystemExit('\n'.join(errors))
    print(f'dotnet namespaces ok: {count} C# sources, identities, references and generated namespaces')


if __name__ == '__main__':
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--apply', action='store_true')
    parser.add_argument('--sample-only', action='store_true')
    args = parser.parse_args()
    files = tracked()
    if args.apply:
        apply(files)
    check(files, args.sample_only)
