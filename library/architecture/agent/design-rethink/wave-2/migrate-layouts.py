#!/usr/bin/env python3
"""One-time wave-2 path migration; retained for review, not a consumer tool."""
from pathlib import Path
import os,re,subprocess,shutil
ROOT=Path(__file__).resolve().parents[5]
os.chdir(ROOT)
AREAS=('library/architecture/go','library/architecture/python','library/architecture/dotnet')
paths=[Path(x) for x in subprocess.check_output(['git','ls-files',*AREAS],text=True).splitlines()]

def relocate(path):
    s=str(path).replace('/internal/infrastructure/client/', '/internal/client/').replace('/src/todo/infrastructure/client/','/src/todo/client/').replace('/src/{package}/infrastructure/client/','/src/{package}/client/').replace('/src/Infrastructure/Client/','/src/Client/')
    s=s.replace('/src/Modules/Todo/Todo.Application.Tests/Services/','/src/Service/Todo/Todo.Service.Tests/')
    for file in ('RemotePlannerService.cs','RemotePlannerErrors.cs'):
        s=s.replace('/src/Modules/Todo/Todo.Application/Services/'+file,'/src/Service/Todo/Todo.Service/'+file)
    return Path(s)

def rewrite(s):
    for before,after in [('internal/infrastructure/client','internal/client'),('infrastructure/client','client'),('Infrastructure/Client','Client'),('Infrastructure\\Client','Client'),('Infrastructure.Client','Client'),('infrastructure.client','client')]:s=s.replace(before,after)
    s=s.replace('src/Modules/Todo/Todo.Application.Tests/Services','src/Service/Todo/Todo.Service.Tests')
    return s

# Capture project-relative edges before moving either end.
for path in paths:
    if not path.exists():continue
    new=relocate(path)
    try:s=path.read_text()
    except UnicodeError:continue
    if path.suffix=='.csproj':
        def edge(m):
            value=m.group(1)
            if '/' not in value and '\\' not in value:return m.group(0)
            if '$(' in value:return m.group(0)
            old_target=(path.parent/value.replace('\\','/')).resolve()
            new_target=relocate(old_target)
            relative=os.path.relpath(new_target, new.parent.resolve())
            if '\\' in value:relative=relative.replace('/','\\')
            return m.group(0).replace(value,relative)
        s=re.sub(r'Include="([^"]+)"',edge,s)
    s=rewrite(s)
    if 'todoapi' in str(path) and path.name=='todoapi.go':
        s=re.sub(r'// Item is[^\n]*\ntype Item struct \{\n\tTitle string\n\tDone  bool\n\}\n\n','',s)
        s=re.sub(r'\bItem\b','ItemDTO',s)
        dto=new.parent/'dto.go'; dto.parent.mkdir(parents=True,exist_ok=True)
        dto.write_text('// Package todoapi owns the logical API contract shared by transport implementations.\npackage todoapi\n\n// ItemDTO preserves the API fields; Domain Model mapping belongs to service.\ntype ItemDTO struct {\n\tTitle string\n\tDone bool\n}\n')
    if path.suffix=='.go':s=s.replace('todoapi.Item','todoapi.ItemDTO')
    if path.name=='todo_api.py':
        s=re.sub(r'@dataclass\(frozen=True\)\nclass Item:\n    """An item as the API answers it, the adapter\'s own type\."""\n\n    title: str\n    done: bool\n\n\n','',s)
        s=re.sub(r'\bItem\b','ItemDTO',s)
        s=s.replace('from todo.client.core.pipeline import Middleware','from todo.client.core.pipeline import Middleware\nfrom todo.client.todo_api.dto import ItemDTO')
        if '@dataclass' not in s:s=s.replace('from dataclasses import dataclass\n','')
        dto=new.parent/'dto.py'; dto.parent.mkdir(parents=True,exist_ok=True)
        dto.write_text('"""The logical API DTO contract shared by HTTP and gRPC implementations."""\n\nfrom dataclasses import dataclass\n\n\n@dataclass(frozen=True)\nclass ItemDTO:\n    """Provider fields only; Domain Model adaptation belongs to service."""\n\n    title: str\n    done: bool\n')
    if path.suffix=='.py':
        s=s.replace('from todo.client.todo_api import Item as','from todo.client.todo_api import ItemDTO as')
        s=s.replace('from todo.client.todo_api.todo_api import OPERATIONS, PREFIX, Item, TodoApi, new','from todo.client.todo_api.dto import ItemDTO\nfrom todo.client.todo_api.todo_api import OPERATIONS, PREFIX, TodoApi, new')
        s=s.replace('from todo.client.todo_api.todo_api import METHODS, PREFIX, Item, TodoApi, new, open_api','from todo.client.todo_api.dto import ItemDTO\nfrom todo.client.todo_api.todo_api import METHODS, PREFIX, TodoApi, new, open_api')
        if new.parent.name=='todo_api' and new.name=='__init__.py':s=s.replace('"Item"','"ItemDTO"')
    if path.suffix=='.cs':
        s=re.sub(r'\bTodoApiItem\b','TodoApiItemDto',s)
        if path.name in ('RemotePlannerService.cs','RemotePlannerErrors.cs'):
            s=s.replace('namespace TodoExampleService.Modules.Todo.Application.Services;', 'namespace TodoExampleService.Service.Todo;')
            if path.name=='RemotePlannerService.cs':s='using TodoExampleService.Modules.Todo.Application.Services;\n'+s
        if '/Todo.Application.Tests/Services/' in str(path):
            s=s.replace('namespace TodoExampleService.Modules.Todo.Application.Tests.Services.Steps;', 'namespace TodoExampleService.Service.Todo.Tests.Steps;').replace('using TodoExampleService.Modules.Todo.Application.Services;','using TodoExampleService.Service.Todo;')
        if path.name=='Program.cs' and 'RemotePlannerService' in s:s='using TodoExampleService.Service.Todo;\n'+s
        if path.name=='ITodoApi.cs':
            s=s.replace('/// <summary>An item as the todo API answers it, the adapter\'s own type.</summary>\npublic sealed record TodoApiItemDto(string Title, bool Done);\n\n','')
            (new.parent/'TodoApiItemDto.cs').parent.mkdir(parents=True,exist_ok=True)
            (new.parent/'TodoApiItemDto.cs').write_text('namespace TodoExampleService.Client.TodoApi.Abstractions;\n\n/// <summary>Logical API fields shared by transport implementations; no Domain Model behavior.</summary>\npublic sealed record TodoApiItemDto(string Title, bool Done);\n')
    if path.suffix=='.md' and path.name.endswith('.skill.md'):
        s=re.sub(r'^updated: \d{8}$','updated: 20261128',s,flags=re.M) if s!=path.read_text() or new!=path else s
    if new!=path:
        new.parent.mkdir(parents=True,exist_ok=True);new.write_text(s);path.unlink()
    elif s!=path.read_text():path.write_text(s)

# Separate .NET integration-service assemblies from consumer handler assemblies.
for skill in Path('library/architecture/dotnet').glob('vp-*-client-in-dotnet.skill'):
    pass
for name in ('vp-http-client','vp-grpc-client','vp-client-retry'):
    skill=Path(f'library/architecture/dotnet/{name}-in-dotnet.skill'); example=skill/'examples'
    service=example/'src/Service/Todo/Todo.Service'; tests=example/'src/Service/Todo/Todo.Service.Tests'
    service.mkdir(parents=True,exist_ok=True);tests.mkdir(parents=True,exist_ok=True)
    (service/'Todo.Service.csproj').write_text('''<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <RootNamespace>$(ServiceNamespace).Service.Todo</RootNamespace>
  </PropertyGroup>
  <ItemGroup>
    <!-- Consumer-owned service interface; no command dispatch from this project. -->
    <ProjectReference Include="..\\..\\..\\Modules\\Todo\\Todo.Application\\Todo.Application.csproj" />
    <ProjectReference Include="..\\..\\..\\Modules\\Todo\\Todo.Domain\\Todo.Domain.csproj" />
    <ProjectReference Include="..\\..\\..\\Client\\TodoApi\\TodoApi.Abstractions\\TodoApi.Abstractions.csproj" />
  </ItemGroup>
</Project>
''')
    application=example/'src/Modules/Todo/Todo.Application/Todo.Application.csproj'
    text=application.read_text();text=re.sub(r'    <!-- The interface of the todo API[^\n]*-->\n','',text)
    text=re.sub(r'^.*<ProjectReference[^\n]*TodoApi.Abstractions[^\n]*\n','',text,flags=re.M);application.write_text(text)
    old_test=example/'src/Modules/Todo/Todo.Application.Tests/Todo.Application.Tests.csproj'
    text=old_test.read_text().replace('$(ServiceNamespace).Modules.Todo.Application.Tests','$(ServiceNamespace).Service.Todo.Tests').replace('..\\Todo.Application\\Todo.Application.csproj','..\\Todo.Service\\Todo.Service.csproj')
    (tests/'Todo.Service.Tests.csproj').write_text(text)
    host=example/'src/App/App.Host/App.Host.csproj';text=host.read_text().replace('  </ItemGroup>', '    <ProjectReference Include="..\\..\\Service\\Todo\\Todo.Service\\Todo.Service.csproj" />\n  </ItemGroup>',1);host.write_text(text)
    sln=example/'Todo.slnx';text=sln.read_text().replace('</Solution>', '''  <Folder Name="/src/Service/Todo/">
    <Project Path="src/Service/Todo/Todo.Service/Todo.Service.csproj" />
    <Project Path="src/Service/Todo/Todo.Service.Tests/Todo.Service.Tests.csproj" />
  </Folder>
</Solution>''');sln.write_text(text)
    # The service tests left the Application test project: the policy remains test-per-subject.
    src=example/'src/Modules/Todo/Todo.Application.Tests/Steps/AssemblyInfo.cs'
    if src.exists():(tests/'Steps').mkdir(exist_ok=True);shutil.copyfile(src,tests/'Steps/AssemblyInfo.cs')

# The namespace checker owns the expected package-to-project mapping.
p=Path('library/architecture/agent/dotnet-namespaces/migrate.py');s=p.read_text().replace("'Infrastructure.Client", "'Client").replace("'Todo.Domain': 'Modules.Todo.Domain',", "'Todo.Domain': 'Modules.Todo.Domain',\n    'Todo.Service': 'Service.Todo',")
p.write_text(s)
print('Client packages and logical DTO files moved in current stack examples/templates; .NET service assemblies split.')
