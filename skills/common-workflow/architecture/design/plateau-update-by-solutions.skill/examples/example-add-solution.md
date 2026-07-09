# Example: adding a new solution to a plateau

## Input

- plateau-name: `default`
- target-stack: `dotnet`
- change-type: `add`
- solution: `solution-entity-edit-timestamp`
- output: `skills/dotnet/architecture/plateau/default`

## Source files discovered in Implementation/

`solution-entity-edit-timestamp`:
- `Shared.csproj.extend.md`
- `Shared.csproj.extend/ICommandWithTimestamp.cs.create.md`
- `{Module}.Domain.csproj.extend.md`
- `{Module}.Domain.csproj.extend/{EntityName}.cs.extend.md`
- `App.Infrastructure.csproj.extend.md`
- `App.Infrastructure.csproj.extend/AppDbContext.cs.extend.md`

## Affected skills and actions

| Implementation file | Structural skill | Action |
| ------------------- | ---------------- | ------ |
| `Shared.csproj.extend.md` | `structure/Shared/artifact-shared.skill.md` | Update |
| `Shared.csproj.extend/ICommandWithTimestamp.cs.create.md` | `structure/Shared/files/file-i-command-with-timestamp.skill.md` | Create |
| `{Module}.Domain.csproj.extend.md` | `structure/{Module}.Domain/artifact-module-domain.skill.md` | Update |
| `{Module}.Domain.csproj.extend/{EntityName}.cs.extend.md` | `structure/{Module}.Domain/files/file-entity.skill.md` | Update |
| `App.Infrastructure.csproj.extend.md` | `structure/App.Infrastructure/artifact-app-infrastructure.skill.md` | Update |
| `App.Infrastructure.csproj.extend/AppDbContext.cs.extend.md` | `structure/App.Infrastructure/files/file-appdbcontext.skill.md` | Update |

## Updates to plateau root skill

- Add `solution-entity-edit-timestamp` to `created_by`.
- Update `description` to mention the new capability.
- Add a new `Core Principles` bullet if the solution introduces a new principle.
- Add a new `Capabilities` section if needed.
- Add `solution-entity-edit-timestamp` to every relevant `__Applied solutions__` list.
- Bump `version`.
