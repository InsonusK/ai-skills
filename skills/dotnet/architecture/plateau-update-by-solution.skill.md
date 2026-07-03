---
name: plateau-update-by-solution
description: When a solution is added to a plateau or a solution used by a plateau is updated, propagate changes to the plateau root skill and to every skill inside the plateau's structure folder.
whenToUse:
  - adding a new solution to a plateau
  - updating an existing solution that is referenced by a plateau
  - removing a solution from a plateau
---

# Goal

Keep the plateau root skill (`plateau-{plateau-name}.skill.md`) and all of its structural template skills (`{plateau-name}/structure/**`) consistent with every solution that the plateau applies.

# Prerequisites

Read [[skills/dotnet/architecture/create-plateau.skill/create-plateau.skill.md|create-plateau.skill]] first. The same mapping and normalization rules apply during an update:

- `Implementation/` file patterns (`Repository.create.md`, `{Project}.csproj.create.md`, `{Project}.csproj.extend.md`, class files)
- project and class name normalization (`{Module}.Api` → `csproj-module-api`, `ICommand.cs` → `class-i-command`)
- `.create.md` vs `.extend.md` semantics
- `{Module}` projects become generic module templates

# Input parameters

- {plateau-name} - name of the plateau to update
- {solution} - solution skill that is being added, updated, or removed
- {output} - folder containing the plateau. Default `skills/dotnet/architecture/plateau/{plateau-name}`

# How to identify affected structural skills

1. Open the plateau root skill: `{output}/plateau-{plateau-name}.skill.md`
2. Scan `{output}/structure/` for existing skills that already reference {solution} in `created_by` or `__Applied solutions__`
3. Scan `Implementation/` folder inside {solution} to discover all files:
   - `Repository.create.md`
   - `{Project}.csproj.create.md` and `{Project}.csproj.extend.md`
   - nested `{Class}.cs.create.md` and `{Class}.cs.extend.md`
4. Map each discovered implementation file to a structural skill using the normalization rules from create-plateau.skill:
   - `Repository.create.md` → `sln-{plateau-name}.skill.md`
   - `{Project}.csproj.create/.extend.md` → `csproj-{normalized}.skill.md`
   - `{Class}.cs.create/.extend.md` → `class-{normalized}.skill.md`
5. The union of (2) and (4) is the set of skills that must be created or updated

# Rules

## Adding a new solution

MUST:
- Update the plateau root skill: `description`, `created_by`, `Core Principles`, `Capabilities`, `Use cases`, `__Applied solutions__`
- Create any structural skill that does not yet exist but is required by the new solution's `.create.md` files
- Update any existing structural skill that is targeted by the new solution's `.extend.md` files
- Add the solution to `created_by` of every affected skill
- Add/update `__Applied solutions__` links in every affected skill
- Bump `version` of every changed structural skill
- Bump `version` of the plateau root skill

## Updating an existing solution

MUST:
- Find every structural skill that references the solution in `created_by` or `__Applied solutions__`
- Re-scan the solution's `Implementation/` folder for changes
- Update affected structural skills to reflect the new implementation state
- Update `__Applied solutions__` links if implementation file names changed
- Bump `version` of every changed structural skill
- Bump `version` of the plateau root skill

## .create vs .extend during update

| Situation | Action |
| --------- | ------ |
| `.create.md` file and target skill does not exist | Create new skill from template |
| `.create.md` file and target skill already exists | Conflict: ask user whether to overwrite or merge |
| `.extend.md` file and target skill does not exist | Error: the base skill must exist before it can be extended |
| `.extend.md` file and target skill exists | Merge the extension into the existing skill |

## created_by maintenance

MUST:
- Add the solution link to `created_by` if it is not already present
- Preserve the existing order or append consistently with other plateau skills
- Avoid duplicate entries

MUST NOT:
- Remove other solutions from `created_by` unless explicitly instructed

## Removing a solution from a plateau

If a solution is removed from the plateau:

- Remove the solution from `created_by` of the plateau root skill
- Remove the solution's `__Applied solutions__` bullets from the plateau root skill
- For each affected structural skill:
  - Remove content that came only from that solution
  - Remove the solution from `created_by`
  - If the skill becomes empty of content and `created_by`, consider deleting it or ask the user
- Bump `version` of every changed skill

# Workflow

1. Identify the plateau root skill: `{output}/plateau-{plateau-name}.skill.md`
2. Identify the solution being added, updated, or removed
3. Discover affected structural skills (see [How to identify affected structural skills](#how-to-identify-affected-structural-skills))
4. For each affected structural skill:
   - Apply the correct `.create` / `.extend` action
   - Add/update goals, core principles, rules, anti-patterns, check lists, and applied solutions
   - Update `created_by` and `__Applied solutions__`
   - Bump `version`
5. Update the plateau root skill:
   - `description`
   - `created_by`
   - `Core Principles`
   - `Capabilities`
   - `Use cases`
   - `__Applied solutions__`
6. Bump the plateau root skill `version`
7. Verify that no `hint`, `example`, or `# How Apply this template` blocks remain in updated skills

# Example: adding solution-entity-edit-timestamp to plateau-default

This example is based on the real change in commit `8d4766e539b2ff9bcc2ec030f767497a20b39307`.

## Input

- plateau-name: `default`
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
- `{Module}.Interfaces.csproj.extend.md`
- `{Module}.Interfaces.csproj.extend/{Command}.cs.extend.md`
- `{Module}.Application.csproj.extend.md`
- `App.Infrastructure.csproj.extend.md`
- `App.Infrastructure.csproj.extend/AppDbContext.cs.extend.md`

## Affected skills and actions

| Implementation file | Structural skill | Action |
| ------------------- | ---------------- | ------ |
| `Shared.csproj.extend.md` | `structure/Shared/csproj-shared.skill.md` | Update |
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
| `App.Infrastructure.csproj.extend/{EntityName}Config.cs.extend.md` | `structure/App.Infrastructure/classes/class-module-to-module-config.skill.md` | Update |

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

# Check list

- [ ] `create-plateau.skill` mapping rules were applied
- [ ] Plateau root skill references the new/updated solution in `created_by`
- [ ] Plateau root skill describes the solution in `Core Principles` or `Capabilities`
- [ ] Plateau root skill includes the solution in the correct `__Applied solutions__` list
- [ ] Every structural skill affected by the solution has been identified using `created_by`, `__Applied solutions__`, and the solution's `Implementation/` folder
- [ ] New skills were created for `.create.md` files that had no matching skill
- [ ] Existing skills were updated for `.extend.md` files
- [ ] `created_by` of every affected skill is up to date and has no duplicates
- [ ] `version` timestamps are updated in the plateau root skill and all changed structural skills
- [ ] No `hint` or `example` blocks remain in rewritten skills
