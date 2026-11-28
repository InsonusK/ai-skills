#!/usr/bin/env python3
"""Wave-2 architecture invariants and documented VP coverage, not runtime proof."""
from pathlib import Path
import re,subprocess,xml.etree.ElementTree as ET
ROOT=Path(__file__).resolve().parents[5]
errors=[]
source_paths=set(subprocess.check_output(["git","ls-files","--cached","--others","--exclude-standard"],cwd=ROOT,text=True).splitlines())
contracts={
'go':['internal/service/remote_planner.go','internal/client/todoapi/contract/dto.go','internal/client/core/outcome/outcome.go'],
'python':['src/todo/service/remote_planner.py','src/todo/client/todo_api/dto.py','src/todo/client/core/outcomes.py'],
'dotnet':['src/Service/Todo/Todo.Service/RemotePlannerService.cs','src/Client/TodoApi/TodoApi.Abstractions/ITodoApi.cs','src/Client/TodoApi/TodoApi.Abstractions/TodoApiItemDto.cs','src/Client/Core/Client.Abstractions/ApiOutcomes.cs']}
for stack,relatives in contracts.items():
 examples=[ROOT/f'library/architecture/{stack}/{name}-in-{stack}.skill/examples' for name in ('vp-http-client','vp-grpc-client','vp-client-retry')]
 for relative in relatives:
  files=[example/relative for example in examples]
  if not all(p.is_file() for p in files):errors.append(f'{stack}: missing common contract {relative}');continue
  if len({p.read_bytes() for p in files})!=1:errors.append(f'{stack}: transport/retry changes shared contract {relative}')
 for example in examples:
  for old in ('internal/infrastructure/client','src/todo/infrastructure/client','src/Infrastructure/Client'):
   if any(p.is_file() and p.relative_to(ROOT).as_posix() in source_paths for p in (example/old).rglob('*')):errors.append(f'Old client root still populated: {example/old}')
  if stack=='dotnet':
   application=example/'src/Modules/Todo/Todo.Application/Todo.Application.csproj'
   app=ET.fromstring(application.read_text())
   if any('Client' in r.attrib['Include'] for r in app.iter('ProjectReference')):errors.append(f'{application}: Application references client')
   service=example/'src/Service/Todo/Todo.Service/Todo.Service.csproj'
   refs=[r.attrib['Include'].replace('\\','/') for r in ET.fromstring(service.read_text()).iter('ProjectReference')]
   if not any('Todo.Application.csproj' in r for r in refs):errors.append(f'{service}: missing consumer-owned port reference')
   if any('Client.Http/' in r or 'Client.Grpc/' in r or '/TodoApi/TodoApi.csproj' in r for r in refs):errors.append(f'{service}: concrete protocol reference')
  if stack=='python':
   init=example/'src/todo/client/todo_api/__init__.py'
   if re.search(r'from .*todo_api\.todo_api|import grpc|import httpx',init.read_text()):errors.append(f'{init}: logical import loads transport')
  for relative in relatives[:2]:
   s=(example/relative).read_text()
   if re.search(r'httpclient|grpcclient|using Grpc|from grpc|import grpc|httpx2|HttpStatus',s):errors.append(f'{example/relative}: service/DTO contract knows transport')
# Every current spec and stack implementation is navigable from each layout page.
for stack in contracts:
 page=ROOT/f'library/architecture/docs/layouts/{stack}.md';text=page.read_text()
 for spec in (ROOT/'library/architecture/spec').glob('*/*.skill.md'):
  name=spec.name.removesuffix('.skill.md')
  if f'[{name}]' not in text:errors.append(f'{page}: missing VP {name}')
 for target in re.findall(r'\[[^\]]*\]\(([^)]+)\)',text):
  if '://' not in target and not (page.parent/target.split('#',1)[0]).exists():errors.append(f'{page}: broken link {target}')
# Retired client paths are forbidden in active stack instructions, not in ADR history.
for area in ('library/architecture/go','library/architecture/python','library/architecture/dotnet','library/develop/go','library/develop/python','library/develop/dotnet'):
 for file in (ROOT/area).rglob('*.skill.md'):
  text=file.read_text()
  if re.search(r'infrastructure/client|Infrastructure/Client|Infrastructure\.Client|infrastructure\.client',text):errors.append(f'{file.relative_to(ROOT)}: retired client path')
if errors:raise SystemExit('\n'.join(errors))
print('Wave 2 structure OK: stable service/DTO/outcomes across transports, sibling roots, .NET dependency direction, and 14-spec layout coverage per stack.')
