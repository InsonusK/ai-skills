<<<<<<< HEAD
# Example: adding solution-entity-edit-timestamp to plateau-default

This example is based on the real change in commit `8d4766e539b2ff9bcc2ec030f767497a20b39307`.
=======
# Example: adding a solution to an existing plateau

This example walks through adding `solution-entity-edit-timestamp` to the .NET `default` plateau.

The target plateau structure is described in [[../../plateau-create-by-solutions.skill/examples/example-dotnet-default-plateau|Example: creating the .NET `default` plateau]].
>>>>>>> 11a7c72188d1a29a8b798607e9dc6bf95097f294

## Input

- plateau-name: `default`
<<<<<<< HEAD
- solution: `solution-entity-edit-timestamp`
- output: `skills/dotnet/architecture/plateau/default`

## Source files discovered in Implementation/

`solution-entity-edit-timestamp`:

- `Shared.csproj.extend.md`
- `Shared.csproj.extend/ICreationInfoModel.cs.create.md`
- `Shared.csproj.extend/ICreationInfoModelReadOnly.cs.create.md`
- `Shared.csproj.extend/IUpdateInfoModel.cs.create.md`
- `Shared.csproj.extend/IUpdateInfoModelReadOnly.cs.create.md`
- `Shared.csproj.extend/ICommandWithTimestamp.cs.create.md`
- `{Module}.Domain.csproj.extend.md`
- `{Module}.Domain.csproj.extend/{EntityName}.cs.extend.md`
- `{Module}.Domain.csproj.extend/{EntityName}Config.cs.extend.md`
- `{Module}.Interfaces.csproj.extend.md`
- `{Module}.Interfaces.csproj.extend/{Command}.cs.extend.md`
- `{Module}.Application.csproj.extend.md`
- `{Module}.Application.csproj.extend/{FeatureName}.Handler.cs.extend.md`
- `{Module}.Application.csproj.extend/{FeatureName}.Validator.cs.extend.md`
- `App.Infrastructure.csproj.extend.md`
- `App.Infrastructure.csproj.extend/AppDbContext.cs.extend.md`

## Affected skills and actions
=======
- target-stack: `dotnet`
- change-type: `add`
- solution: `solution-entity-edit-timestamp`
- output: `skills/dotnet/architecture/artifacts/plateau/default`

## Identify affected skills

1. Open `plateau-default.skill.md` and scan `created_by` — `solution-entity-edit-timestamp` is not present yet.
2. Scan `solution-entity-edit-timestamp/Implementation/` and map files using `plateau-create-by-solutions.skill` rules:
>>>>>>> 11a7c72188d1a29a8b798607e9dc6bf95097f294

| Implementation file | Structural skill | Action |
| ------------------- | ---------------- | ------ |
| `Shared.csproj.extend.md` | `structure/Shared/csproj-shared.skill.md` | Update |
<<<<<<< HEAD
| `Shared.csproj.extend/ICreationInfoModel.cs.create.md` | `structure/Shared/classes/class-i-creation-info-model.skill.md` | Create |
| `Shared.csproj.extend/ICreationInfoModelReadOnly.cs.create.md` | `structure/Shared/classes/class-i-creation-info-model-read-only.skill.md` | Create |
| `Shared.csproj.extend/IUpdateInfoModel.cs.create.md` | `structure/Shared/classes/class-i-update-info-model.skill.md` | Create |
| `Shared.csproj.extend/IUpdateInfoModelReadOnly.cs.create.md` | `structure/Shared/classes/class-i-update-info-model-read-only.skill.md` | Create |
| `Shared.csproj.extend/ICommandWithTimestamp.cs.create.md` | `structure/Shared/classes/class-i-command-with-timestamp.skill.md` | Create |
| `{Module}.Domain.csproj.extend.md` | `structure/{Module}.Domain/csproj-module-domain.skill.md` | Update |
| `{Module}.Domain.csproj.extend/{EntityName}.cs.extend.md` | `structure/{Module}.Domain/classes/class-entity.skill.md` | Update |
| `{Module}.Domain.csproj.extend/{EntityName}Config.cs.extend.md` | `structure/{Module}.Domain/classes/class-entity-config.skill.md` | Update |
| `{Module}.Interfaces.csproj.extend.md` | `structure/{Module}.Interfaces/csproj-module-interfaces.skill.md` | Update |
| `{Module}.Interfaces.csproj.extend/{Command}.cs.extend.md` | `structure/{Module}.Interfaces/classes/class-command.skill.md` | Update |
| `{Module}.Application.csproj.extend.md` | `structure/{Module}.Application/csproj-module-application.skill.md` | Update |
| `{Module}.Application.csproj.extend/{FeatureName}.Handler.cs.extend.md` | `structure/{Module}.Application/classes/class-feature-handler.skill.md` | Update |
| `{Module}.Application.csproj.extend/{FeatureName}.Validator.cs.extend.md` | `structure/{Module}.Application/classes/class-feature-validator.skill.md` | Update |
| `App.Infrastructure.csproj.extend.md` | `structure/App.Infrastructure/csproj-app-infrastructure.skill.md` | Update |
| `App.Infrastructure.csproj.extend/AppDbContext.cs.extend.md` | `structure/App.Infrastructure/classes/class-appdbcontext.skill.md` | Update |

## Updates to plateau root skill

- Add `solution-entity-edit-timestamp` to `created_by`
- Update `description` to mention "entity edit timestamps"
- Add a new `Core Principles` bullet about `ActionTimeStamp`, user timestamps, and server timestamps assigned by `AppDbContext`
- Add a new `Capabilities` section **Entity edit timestamps**
- Add `solution-entity-edit-timestamp` to every relevant `__Applied solutions__` list
- Bump `version` from `20260628` to `20260629223200`

## Updates to repository skill (`sln-default.skill.md`)

- Add `solution-entity-edit-timestamp` to `created_by`
- Add timestamp contracts to `Goal` and `Core Principles`
- Add `entity-edit-timestamp` column to the `Entity Type Matrix`
- Add a `solution-entity-edit-timestamp.skill` block under `Requirements` describing which entity types get which timestamp contracts
- Update `Rules`, `Anti-patterns`, `Check list`, and `Unittest TestCases` with timestamp-related items
- Add `solution-entity-edit-timestamp` to all `__Applied solutions__` lists
- Bump `version`

## Updates to a project skill (`csproj-shared.skill.md`)

- Add `solution-entity-edit-timestamp` to `created_by`
- Add timestamp contract goals to `Goal`
- Add `Timestamp contracts live in Shared.Timestamps` to `Core Principles`
- Add `/Timestamps` folder and interface links to `Project Structure`
- Add `/Timestamps` entries to `Directory and class skills`
- Add `ICommandWithTimestamp` rules to `Rules`
- Add `__Applied solutions__` bullet linking to `Shared.csproj.extend.md`
- Bump `version`

## New class skills created

- `structure/Shared/classes/class-i-creation-info-model.skill.md`
- `structure/Shared/classes/class-i-creation-info-model-read-only.skill.md`
- `structure/Shared/classes/class-i-update-info-model.skill.md`
- `structure/Shared/classes/class-i-update-info-model-read-only.skill.md`
- `structure/Shared/classes/class-i-command-with-timestamp.skill.md`
- `structure/App.Infrastructure/classes/class-appdbcontext.skill.md`
=======
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
>>>>>>> 11a7c72188d1a29a8b798607e9dc6bf95097f294
