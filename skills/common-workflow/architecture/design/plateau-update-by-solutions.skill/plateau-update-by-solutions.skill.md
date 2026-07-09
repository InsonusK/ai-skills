---
name: plateau-update-by-solutions
description: When a solution is added to a plateau, updated, or removed, propagate the changes to the plateau root skill and to every structural skill inside the plateau's structure folder
whenToUse:
  - adding a new solution to a plateau
  - updating an existing solution that is referenced by a plateau
  - removing a solution from a plateau
  - rebuilding a plateau after multiple solutions changed
---

# Goal

Keep the plateau root skill (`plateau-{plateau-name}.skill.md`) and all of its structural template skills (`{plateau-name}/structure/**`) consistent with every solution that the plateau applies.

# Prerequisites

Read [[skills/common-workflow/architecture/design/plateau-create-by-solutions.skill/plateau-create-by-solutions.skill|plateau-create-by-solutions]] first. The same mapping and normalization rules apply during an update:

- `Implementation/` file patterns (`Repository.{change_kind}.md`, `{Artifact}.{change_kind}.md`, nested file-level items)
- artifact and file name normalization (`{Module}.Api` → `artifact-module-api`, `ICommand.cs` → `file-i-command`)
- `.create.md` vs `.extend.md` semantics
- placeholder artifacts become generic templates

# Input parameters

- `{plateau-name}` — name of the plateau to update.
- `{solutions}` — list of solution skills that are being added, updated, or removed. For a single-solution change this list contains one item.
- `{change-type}` — one of:
  - `add` — a new solution is being introduced to the plateau.
  - `update` — an existing solution in the plateau has changed.
  - `remove` — a solution is being removed from the plateau.
  - `rebuild` — multiple solutions changed; re-create the plateau from the full list.
- `{output}` — folder containing the plateau. Default `skills/{stack}/architecture/plateau/{plateau-name}`.
- `{target-stack}` (optional) — target language/stack (`dotnet`, `python`, `angular`, etc.). If unknown, infer from the plateau or solution skills.

# How to identify affected structural skills

1. Open the plateau root skill: `{output}/plateau-{plateau-name}.skill.md`.
2. Scan `{output}/structure/` for existing skills that already reference any of `{solutions}` in `created_by` or `__Applied solutions__`.
3. Scan the `Implementation/` folder inside each solution in `{solutions}` to discover all files:
   - `Repository.{change_kind}.md`
   - `{Artifact}.{change_kind}.md`
   - nested `{File}.{change_kind}.md`
4. Map each discovered implementation file to a structural skill using the normalization rules from `plateau-create-by-solutions.skill`:
   - `Repository.{change_kind}.md` → `sln-{plateau-name}.skill.md`
   - `{Artifact}.{change_kind}.md` → `artifact-{normalized}.skill.md`
   - `{File}.{change_kind}.md` → `file-{normalized}.skill.md`
5. The union of (2) and (4) is the set of skills that must be created or updated.

# Rules

## Adding a new solution

### MUST
- Update the plateau root skill: `description`, `created_by`, `Core Principles`, `Capabilities`, `Use cases`, `__Applied solutions__`.
- Create any structural skill that does not yet exist but is required by the new solution's `.create.md` files.
- Update any existing structural skill that is targeted by the new solution's `.extend.md` files.
- Add the solution to `created_by` of every affected skill.
- Add/update `__Applied solutions__` links in every affected skill.
- Bump `version` of every changed structural skill.
- Bump `version` of the plateau root skill.

## Updating an existing solution

### MUST
- Find every structural skill that references the solution in `created_by` or `__Applied solutions__`.
- Re-scan the solution's `Implementation/` folder for changes.
- Update affected structural skills to reflect the new implementation state.
- Update `__Applied solutions__` links if implementation file names changed.
- Bump `version` of every changed structural skill.
- Bump `version` of the plateau root skill.

## .create vs .extend during update

| Situation | Action |
| --------- | ------ |
| `.create.md` file and target skill does not exist | Create new skill from template |
| `.create.md` file and target skill already exists | Conflict: ask user whether to overwrite or merge |
| `.extend.md` file and target skill does not exist | Error: the base skill must exist before it can be extended |
| `.extend.md` file and target skill exists | Merge the extension into the existing skill |

## created_by maintenance

### MUST
- Add the solution link to `created_by` if it is not already present.
- Preserve the existing order or append consistently with other plateau skills.
- Avoid duplicate entries.

### MUST NOT
- Remove other solutions from `created_by` unless explicitly instructed.

## Removing a solution from a plateau

If a solution is removed from the plateau:

- Remove the solution from `created_by` of the plateau root skill.
- Remove the solution's `__Applied solutions__` bullets from the plateau root skill.
- For each affected structural skill:
  - Remove content that came only from that solution.
  - Remove the solution from `created_by`.
  - If the skill becomes empty of content and `created_by`, consider deleting it or ask the user.
- Bump `version` of every changed skill.

## Rebuilding a plateau from a full solution list

If multiple solutions changed at once or the plateau is out of sync:

1. Treat the current plateau as a source of truth for existing skills.
2. Re-run the discovery step from `plateau-create-by-solutions.skill` for the full `{solutions}` list.
3. For every structural skill that no longer has a contributing solution, consider deleting it or ask the user.
4. For every new or changed implementation file, create or update the corresponding structural skill.
5. Rewrite the plateau root skill summary from the full solution list.
6. Bump `version` of every changed skill.

# Workflow

1. Identify the plateau root skill: `{output}/plateau-{plateau-name}.skill.md`.
2. Identify the solutions being added, updated, or removed.
3. Discover affected structural skills (see [How to identify affected structural skills](#how-to-identify-affected-structural-skills)).
4. For each affected structural skill:
   - Apply the correct `.create` / `.extend` action.
   - Add/update goals, core principles, rules, anti-patterns, check lists, and applied solutions.
   - Update `created_by` and `__Applied solutions__`.
   - Bump `version`.
5. Update the plateau root skill:
   - `description`
   - `created_by`
   - `Core Principles`
   - `Capabilities`
   - `Use cases`
   - `__Applied solutions__`
6. Bump the plateau root skill `version`.
7. Verify that no `hint`, `example`, or `code example` blocks remain in updated skills.

# Examples

## Plateau structure examples

Before updating a plateau, understand how the plateau is built from solution skills:

- [[../../plateau-create-by-solutions.skill/examples/example-dotnet-default-plateau|Example: creating the .NET `default` plateau]]
- [[../../plateau-create-by-solutions.skill/examples/example-python-default-cli-plateau|Example: creating the Python `default-cli` plateau]]

## Update workflow examples

- [[./examples/example-add-solution|Example: adding a new solution to a plateau]]
- [[./examples/example-update-solution|Example: updating an existing solution in a plateau]]

# Check list

- [ ] `plateau-create-by-solutions.skill` mapping rules were applied.
- [ ] Plateau root skill references the new/updated solution in `created_by`.
- [ ] Plateau root skill describes the solution in `Core Principles` or `Capabilities`.
- [ ] Plateau root skill includes the solution in the correct `__Applied solutions__` list.
- [ ] Every structural skill affected by the solution has been identified using `created_by`, `__Applied solutions__`, and the solution's `Implementation/` folder.
- [ ] New skills were created for `.create.md` files that had no matching skill.
- [ ] Existing skills were updated for `.extend.md` files.
- [ ] `created_by` of every affected skill is up to date and has no duplicates.
- [ ] `version` timestamps are updated in the plateau root skill and all changed structural skills.
- [ ] No `hint`, `example`, or `code example` blocks remain in rewritten skills.
