# Example: adding a solution to an existing plateau

This example walks through adding `solution-entity-edit-timestamp` to the .NET `default` plateau.

The target plateau structure is described in [[../../plateau-create-by-solutions.skill/examples/example-dotnet-default-plateau|Example: creating the .NET `default` plateau]].

## Input

- plateau-name: `default`
- target-stack: `dotnet`
- change-type: `add`
- solution: `solution-entity-edit-timestamp`
- output: `skills/dotnet/architecture/artifacts/plateau/default`

## Identify affected skills

1. Open `plateau-default.skill.md` and scan `created_by` — `solution-entity-edit-timestamp` is not present yet.
2. Scan `solution-entity-edit-timestamp/Implementation/` and map files using `plateau-create-by-solutions.skill` rules:

| Implementation file | Structural skill | Action |
| ------------------- | ---------------- | ------ |
| `Shared.csproj.extend.md` | `structure/Shared/csproj-shared.skill.md` | Update |
| `Shared.csproj.extend/ICommandWithTimestamp.cs.create.md` | `structure/Shared/classes/class-i-command-with-timestamp.skill.md` | Create |
| `{Module}.Domain.csproj.extend.md` | `structure/{Module}.Domain/csproj-module-domain.skill.md` | Update |
| `{Module}.Domain.csproj.extend/{EntityName}.cs.extend.md` | `structure/{Module}.Domain/classes/class-entity.skill.md` | Update |
| `App.Infrastructure.csproj.extend.md` | `structure/App.Infrastructure/csproj-app-infrastructure.skill.md` | Update |
| `App.Infrastructure.csproj.extend/AppDbContext.cs.extend.md` | `structure/App.Infrastructure/classes/class-appdbcontext.skill.md` | Update |

## Update steps

1. **Plateau root skill** (`plateau-default.skill.md`):
   - Add `solution-entity-edit-timestamp` to `created_by`.
   - Update `description` to mention "entity edit timestamps".
   - Add a new `Core Principles` bullet about client `ActionTimeStamp` and authoritative server timestamps.
   - Add a new `Capabilities` section **Entity edit timestamps**.
   - Add `solution-entity-edit-timestamp` to relevant `__Applied solutions__` lists.
   - Bump `version`.

2. **Repository skill** (`sln-default.skill.md`):
   - Add `solution-entity-edit-timestamp` to `created_by`.
   - Add timestamp contracts to `Goal` and `Core Principles`.
   - Update `Rules`, `Anti-patterns`, `Check list` with timestamp-related items.
   - Add `__Applied solutions__` bullets.
   - Bump `version`.

3. **Project skills** (`csproj-shared.skill.md`, `csproj-module-domain.skill.md`, `csproj-app-infrastructure.skill.md`):
   - Add `solution-entity-edit-timestamp` to `created_by`.
   - Merge new goals, principles, rules from the corresponding `.extend.md` files.
   - Add `__Applied solutions__` bullets.
   - Bump `version`.

4. **New class skill** (`class-i-command-with-timestamp.skill.md`):
   - Create from template.
   - Fill content from `ICommandWithTimestamp.cs.create.md`.
   - Set `created_by` to `solution-entity-edit-timestamp`.
   - Add `__Applied solutions__` bullet.
   - Set `version`.

5. **Updated class skills** (`class-entity.skill.md`, `class-appdbcontext.skill.md`):
   - Merge new rules from `{EntityName}.cs.extend.md` and `AppDbContext.cs.extend.md`.
   - Add `solution-entity-edit-timestamp` to `created_by` and `__Applied solutions__`.
   - Bump `version`.
