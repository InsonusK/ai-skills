---
name: plateau-update-by-solutions
description: When a solution is added to a plateau or a solution used by a plateau is updated, propagate changes to the plateau root skill and to every skill inside the plateau's structure folder.
whenToUse:
  - adding a new solution to a plateau
  - updating an existing solution that is referenced by a plateau
  - removing a solution from a plateau
tags:
  - skill/architecture/plateau/design
  - stack
  - concern/architecture

---

# Goal

Keep the plateau root skill (`plateau-{plateau-name}.skill.md`) and all of its structural template skills (`{plateau-name}/structure/**`) consistent with every solution that the plateau applies, regardless of the plateau's target language/stack.

# Prerequisites

Read [[skills/common-workflow/architecture/design/plateau-create-by-solutions.skill/plateau-create-by-solutions.skill|plateau-create-by-solutions]] first. The same mapping and normalization rules apply during an update:

- `Implementation/` file patterns per stack:
  - .NET: `Repository.create.md`, `{Project}.csproj.create.md`, `{Project}.csproj.extend.md`, class files
  - Python: `Repository.create.md`, `{App}.create.md`, `{App}.extend.md`, class/functions/init files
- project/package and class/module name normalization (`{Module}.Api` → `csproj-module-api`, `ICommand.cs` → `class-i-command`; `{App}` → `package-app`, `{App}.cli.py` → `module-cli`)
- `.create.md` vs `.extend.md` semantics
- `{Module}`/`{App}` placeholders become generic templates

Also read [[skills/common-workflow/architecture/design/adr-create.skill/adr-create.skill|adr-create]]. The same plateau-level ADR rules from plateau-create-by-solutions' [[skills/common-workflow/architecture/design/plateau-create-by-solutions.skill/plateau-create-by-solutions.skill#recording-plateau-level-decisions|Recording plateau-level decisions]] apply during an update: any conflict resolved, or solution removed, gets recorded as an ADR in the plateau's own `adr/` folder.

# Input parameters

- {plateau-name} - name of the plateau to update
- {solution} - solution skill that is being added, updated, or removed
- {stack} - target language/stack of the plateau (`dotnet`, `python`, ...). Detect it from {solution} or from the existing plateau if it already exists
- {output} - folder containing the plateau. Default `skills/{stack}/architecture/plateau/{plateau-name}`

# How to identify affected structural skills

1. Open the plateau root skill: `{output}/plateau-{plateau-name}.skill.md`
2. Scan `{output}/structure/` for existing skills that already reference {solution} in `created_by` or `__Applied solutions__`
3. Scan `Implementation/` folder inside {solution} to discover all files:
   - `Repository.create.md` / `Repository.extend.md`
   - .NET: `{Project}.csproj.create.md` / `.extend.md` and nested `{Class}.cs.create.md` / `.extend.md`
   - Python: `{App}.create.md` / `.extend.md` (`element_kind: project`) and nested class/functions/init files
4. Map each discovered implementation file to a structural skill using the normalization rules from plateau-create-by-solutions.skill:
   - `Repository.create.md`/`.extend.md` → `sln-{plateau-name}.skill.md` (.NET) or `repo-{plateau-name}.skill.md` (Python)
   - `{Project}.csproj.create/.extend.md` → `csproj-{normalized}.skill.md`; `{App}.create/.extend.md` → `package-{normalized}.skill.md`
   - `{Class}.cs.create/.extend.md` → `class-{normalized}.skill.md`; python class/functions/init files → `module-{normalized}.skill.md`
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
- If merging the new solution required resolving a conflict (see [.create vs .extend during update](#create-vs-extend-during-update)), record the resolution as a plateau-level ADR in the plateau's own `adr/` folder, following [[skills/common-workflow/architecture/design/adr-create.skill/adr-create.skill|adr-create]]

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
- Record the removal, and any decision to delete or keep an emptied structural skill, as a plateau-level ADR in the plateau's own `adr/` folder, following [[skills/common-workflow/architecture/design/adr-create.skill/adr-create.skill|adr-create]]

# Workflow

1. Identify the plateau root skill: `{output}/plateau-{plateau-name}.skill.md`
2. Identify the solution being added, updated, or removed, and its {stack}
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


# Examples

- [[skills/common-workflow/architecture/design/plateau-update-by-solutions.skill/examples/example-add-solution|Adding a new solution to a .NET plateau]] — based on commit `8d4766e539b2ff9bcc2ec030f767497a20b39307` (`solution-entity-edit-timestamp` added to `plateau-default`)
- [[skills/common-workflow/architecture/design/plateau-update-by-solutions.skill/examples/example-update-solution|Updating an existing solution in a .NET plateau]] — based on commit `3b76d75bf299ce547c23a29821d6612545cbf265` (`solution-command-integration` refactored)

> Both examples below use the .NET file patterns and skill names (`sln-*`, `csproj-*`, `class-*`). For a Python plateau, apply the exact same workflow and rules, substituting the Python file patterns and skill names from [[skills/common-workflow/architecture/design/plateau-create-by-solutions.skill/plateau-create-by-solutions.skill|plateau-create-by-solutions]] (`repo-*`, `package-*`, `module-*`).

# Check list

- [ ] `plateau-create-by-solutions.skill` mapping rules were applied for the plateau's {stack}
- [ ] Plateau root skill references the new/updated solution in `created_by`
- [ ] Plateau root skill describes the solution in `Core Principles` or `Capabilities`
- [ ] Plateau root skill includes the solution in the correct `__Applied solutions__` list
- [ ] Every structural skill affected by the solution has been identified using `created_by`, `__Applied solutions__`, and the solution's `Implementation/` folder
- [ ] New skills were created for `.create.md` files that had no matching skill
- [ ] Existing skills were updated for `.extend.md` files
- [ ] `created_by` of every affected skill is up to date and has no duplicates
- [ ] `version` timestamps are updated in the plateau root skill and all changed structural skills
- [ ] No `hint` or `example` blocks remain in rewritten skills
- [ ] Any conflict resolution, solution exclusion, or solution-removal decision made during this update is recorded as a plateau-level ADR in the plateau's own `adr/` folder, following adr-create, and listed in the plateau root skill's `adr:` property
