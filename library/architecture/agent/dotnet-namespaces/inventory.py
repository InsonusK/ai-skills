#!/usr/bin/env python3
"""Inventory tracked .NET naming evidence without evaluating MSBuild or renaming."""
from collections import defaultdict
from pathlib import Path
import json
import re
import subprocess
import xml.etree.ElementTree as ET

ROOT = Path(__file__).resolve().parents[4]
CURRENT = 'library/architecture/dotnet/'
LEGACY = 'library/dotnet/architecture/'
paths = subprocess.check_output(
    ['git', 'ls-files', '-z', CURRENT, LEGACY], cwd=ROOT,
).decode().strip('\0').split('\0')
sources = {p: (ROOT / p).read_text() for p in paths if p.endswith(
    ('.cs', '.csproj', '.props', '.targets', '.proto', '.md'))}
projects = {p: ET.fromstring(text) for p, text in sources.items() if p.endswith('.csproj')}


def properties(element):
    return [{**node.attrib, 'property': node.tag, 'value': node.text}
            for node in element.iter() if node.tag in ('AssemblyName', 'RootNamespace')]


references = {}
incoming = defaultdict(list)
for name, project in projects.items():
    references[name] = [node.attrib['Include'] for node in project.iter('ProjectReference')]
    for target in references[name]:
        if '$(' not in target:
            resolved = str((ROOT / name).parent.joinpath(target.replace('\\', '/')).resolve()
                           .relative_to(ROOT))
            incoming[resolved].append(name)


def infrastructure(name):
    return '/Infrastructure/' in name or '/App.Infrastructure/' in name


def project_record(name):
    prefix = str(Path(name).parent) + '/'
    namespaces = sorted({ns for p, text in sources.items()
                         if p.startswith(prefix) and p.endswith('.cs')
                         for ns in re.findall(r'^\s*namespace\s+([\w.]+)', text, re.M)})
    return {
        'path': name, 'project_name': Path(name).stem,
        'identity_overrides_in_project': properties(projects[name]),
        'declared_namespaces': namespaces,
        'project_references': references[name],
        'referenced_by': sorted(incoming[name]),
    }


infra = {scope: [project_record(p) for p in sorted(projects)
                 if p.startswith(prefix) and infrastructure(p)]
         for scope, prefix in [('current', CURRENT), ('legacy', LEGACY)]}
namespace_prefixes = {record['project_name'] for records in infra.values()
                      for record in records if record['declared_namespaces']}
imports = defaultdict(list)
templates = []
for name, text in sorted(sources.items()):
    if name.endswith('.cs'):
        for target in sorted(set(re.findall(
                r'^\s*(?:global\s+)?using\s+(?:static\s+)?(?:\w+\s*=\s*)?([\w.]+)\s*;',
                text, re.M))):
            if any(target == prefix or target.startswith(prefix + '.')
                   for prefix in namespace_prefixes):
                imports[target].append(name)
    if name.startswith(CURRENT) and '/templates/' in name and infrastructure(name):
        templates.append({
            'path': name,
            'namespaces': re.findall(r'^\s*namespace\s+([\w.{}]+)', text, re.M),
            'path_identifier_tokens': sorted(set(re.findall(r'\{\w+\}', name))),
        })

output = {
    'scope': 'Tracked files only. Raw XML declarations, not evaluated MSBuild properties; '
             'explicit C# namespaces, not generated sources. Imports matched by project-name prefix.',
    'project_counts': {scope: sum(p.startswith(prefix) for p in projects)
                       for scope, prefix in [('current', CURRENT), ('legacy', LEGACY)]},
    'infrastructure_projects': infra,
    'identity_declarations': [
        {'path': p, 'declarations': properties(ET.fromstring(text))}
        for p, text in sorted(sources.items())
        if p.endswith(('.csproj', '.props', '.targets')) and properties(ET.fromstring(text))
    ],
    'infrastructure_import_consumers': dict(sorted(imports.items())),
    'current_infrastructure_template_identifiers': templates,
    'generated_namespace_inputs': [
        {'path': p, 'csharp_namespace': re.findall(r'csharp_namespace\s*=\s*"([^"]+)"', text)}
        for p, text in sorted(sources.items()) if p.endswith('.proto')
        and 'csharp_namespace' in text
    ],
    'legacy_guidance_consumers': [
        p for p, text in sorted(sources.items()) if p.startswith(LEGACY)
        and p.endswith('.md') and 'App.Infrastructure' in text
    ],
    'shared_persistence_boundary': [project_record(p) for p in sorted(projects)
                                    if p.startswith(CURRENT) and Path(p).stem == 'Shared.Persistence'],
}
print(json.dumps(output, indent=2, ensure_ascii=False))
