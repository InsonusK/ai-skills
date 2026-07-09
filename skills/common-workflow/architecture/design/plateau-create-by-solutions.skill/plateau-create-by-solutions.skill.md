---
name: plateau-create-by-solutions
description: Define how to build a new plateau skill from a list of solution skills, independent of target language or stack
whenToUse:
  - creating a new plateau from a set of solution skills
  - bootstrapping a stack-agnostic architecture plateau
---

# Input parameters

- `{plateau-name}` — name of the plateau being created.
- `{solutions}` — list of solution skills that must be implemented in the plateau.
- `{output}` — folder where the plateau should be created. Default `skills/{stack}/architecture/plateau` when the target stack is known; otherwise ask the user.
- `{target-stack}` (optional) — target language/stack (`dotnet`, `python`, `angular`, etc.). If unknown, infer from the solution skills or ask the user.

# Prerequisites

Read [[skills/common-workflow/architecture/design/solution-create.skill/solution-create.skill|solution-create]] first. It defines how a solution skill is structured and what files it produces. A plateau is built by aggregating those produced files across all selected solutions.

# Solution-skill structure

Every solution skill has an `Implementation/` folder with concrete mutations. Recognize these file patterns:

| File pattern | What it describes | Becomes |
| ------------ | ----------------- | ------- |
| `Implementation/Repository.{change_kind}.md` | Repository-level changes: overall layout, artifact list, layer responsibilities | Content for `sln-{plateau-name}.skill.md` |
| `Implementation/{Artifact}.{change_kind}.md` | An artifact created or extended by this solution (project, package, module, etc.) | One `artifact-{normalized}.skill.md` |
| `Implementation/{Artifact}.{change_kind}/{File}.{change_kind}.md` | A file created or extended inside an artifact | One `file-{normalized}.skill.md` |

> `{Artifact}` can be a concrete name (`Shared`, `App.Host`, `cli`) or a placeholder (`{Module}.Api`, `{Package}`, `{App}`).
> `{change_kind}` is one of `create`, `extend`, or another explicit kind defined by the solution skill.

The exact file extensions depend on the target stack. Examples:

| Stack | Artifact file | File inside artifact |
| ----- | ------------- | -------------------- |
| .NET  | `{Project}.csproj.{change_kind}.md` | `{Class}.cs.{change_kind}.md` |
| Python| `{Package}/__init__.py.{change_kind}.md` or `{Module}.py.{change_kind}.md` | `{Class}.py.{change_kind}.md` |
| Angular| `{Project}/...` or `libs/{lib}` | `{Component}.ts.{change_kind}.md` |

# How to build a plateau

1. Verify that `{output}` does not already contain a folder named `{plateau-name}`.
   - If the folder exists, ask the user whether to replace the existing plateau.
2. Create `{output}/{plateau-name}`.
3. Create the subfolder `{output}/{plateau-name}/structure`.
4. Discover all artifacts and files contributed by `{solutions}`.
   - Scan the `Implementation/` folder in every solution skill.
   - Collect all `Repository.{change_kind}.md` files.
   - Collect all artifact-level files (`{Artifact}.{change_kind}.md`).
   - Collect all file-level items nested under those artifact files.
   - Normalize placeholder artifacts to generic templates (see [Artifact name normalization](#artifact-name-normalization)).
5. Create `sln-{plateau-name}.skill.md`.
   - Aggregate all `Repository.{change_kind}.md` files from `{solutions}`.
   - Keep repository-level content only.
6. For each discovered artifact create `artifact-{normalized-name}.skill.md`.
   - Merge all `.create.md` and `.extend.md` files for the same artifact.
   - Keep artifact-level content only.
7. For each discovered file create `file-{normalized-name}.skill.md`.
   - Merge all file-level `.create.md` and `.extend.md` files for the same file.
   - Keep file-level content only.
8. Create `plateau-{plateau-name}.skill.md`.
   - This is the plateau summary: goals, core principles, capabilities, use-cases.
   - It is not a code-generation template; it explains what the plateau as a whole provides.
9. Fill every generated skill with real content.
   - Summarize and merge content from the contributing implementation files.
   - Remove all `hint`, `example`, and `code example` blocks from the final skill files.
10. Fill header properties:
    - `plateau` — `{plateau-name}`.
    - `version` — current UTC timestamp with format `YYYYMMDDHHMMSS`.
    - `created_by` — add links to all solutions that contributed to the skill.

# Mapping rules

## Artifact name normalization

Map the artifact source file name to the plateau skill file name using kebab-case. Preserve common prefixes (for example `I-` becomes `i-`).

| Source artifact | Plateau skill file | Notes |
| --------------- | ------------------ | ----- |
| `Shared.csproj` | `artifact-shared.skill.md` | Concrete cross-cutting artifact |
| `App.Host.csproj` | `artifact-app-host.skill.md` | Concrete composition root |
| `{Module}.Api.csproj` | `artifact-module-api.skill.md` | Generic module template |
| `{Package}/__init__.py` | `artifact-package.skill.md` | Python package template |
| `{App}/cli.py` | `artifact-cli.skill.md` | CLI entry point |
| `libs/{Library}` | `artifact-library.skill.md` | Generic library template |

## File name normalization

Map the source file name to the plateau skill file name using kebab-case. Preserve common prefixes (for example `I-` becomes `i-`).

| Source file | Plateau skill file |
| ----------- | ------------------ |
| `ICommand.cs` | `file-i-command.skill.md` |
| `ValidationBehavior.cs` | `file-validation-behavior.skill.md` |
| `backup_service.py` | `file-backup-service.skill.md` |
| `app.component.ts` | `file-app-component.skill.md` |

## .create vs .extend

Both file kinds contribute to the same target skill:

- `.create.md` introduces the artifact/file and its base responsibilities.
- `.extend.md` adds responsibilities brought by other solutions.
- Merge content from all files into one skill, grouping by section (Goal, Core Principles, Structure, Rules, Anti-patterns, Check list).
- List all contributing files in `__Applied solutions:__`.

## Solution selection

- Include every solution that contributes at least one artifact, file, or repository-level change.
- Classification, taxonomy, or policy solutions may affect only the repository skill and the plateau root skill (for example, by defining an entity type matrix). Include them in `created_by` and `__Applied solutions:__` even if they have no direct code files.
- If a solution from `{solutions}` has no `Implementation/` content and does not affect plateau structure, document the decision to exclude it in the plateau root skill or ask the user for clarification.

# Applied solutions list format

Every content section that summarizes one or more source solutions must end with an `__Applied solutions:__` list.

Each bullet must contain **exactly two wikilinks separated by ` - `** when an implementation/template file exists:

1. The parent solution skill file (`solution-*.skill.md`).
2. The specific implementation/template file inside that solution that contributed the content.

```example
__Applied solutions:__
- [[skills/common-workflow/architecture/solutions/solution-command-integration.skill/solution-command-integration.skill.md|solution-command-integration]] - [[skills/common-workflow/architecture/solutions/solution-command-integration.skill/Implementation/Shared.csproj.extend.md|Shared.csproj extend]]
```

When the content comes directly from the solution skill file and there is no separate implementation/template file, list the solution skill file once.

```example
__Applied solutions:__
- [[skills/common-workflow/architecture/solutions/solution-command-integration.skill/solution-command-integration.skill.md|solution-command-integration]]
```

# Repository skill structure

The repository skill describes the whole solution. Its sections must stay at the repository level.

`## Repository Structure`:
- Show **only artifact folders**.
- Do not list file-level items or sub-folders inside artifacts.

`## Directory and artifact skills`:
- Show **only artifact directories**, the matching artifact template skill file, and a short description.
- Do not list individual file skill files in this table.

```example
| Directory\|file | template link | Description |
| ---------------- | ------------- | ----------- |
| /Shared | [[artifact-shared.skill.md\|artifact-shared.skill]] | Cross-cutting primitives |
| /{Module}.Domain | [[artifact-module-domain.skill.md\|artifact-module-domain.skill]] | Business logic |
```

# Rules

## MUST
- Remove all `hint`, `example`, and `code example` blocks from the final skill file.
- Follow any `# How Apply this template` rules defined in the source templates.
- Write every `__Applied solutions:__` bullet as `<solution skill link> - <implementation/template link>` when an implementation/template file exists.
- Keep repository skill `## Repository Structure` limited to artifact folders only.
- Keep repository skill `## Directory and artifact skills` limited to artifact directories and artifact template links.
- Normalize placeholder artifacts to generic templates, not concrete names.
- Merge `.create.md` and `.extend.md` files for the same artifact/file into a single skill file.
- Include every solution that contributes artifact, file, or repository-level content in `created_by`.
- If two solutions define conflicting rules for the same artifact/file, resolve the conflict or ask the user before merging.

## MUST NOT
- Change other skills except the one you are building without explicit instruction.
- Omit the parent solution skill link from `__Applied solutions:__` bullets.
- List file skill files in the repository skill `## Directory and artifact skills` table.
- Create separate skill files for `.create.md` and `.extend.md` of the same artifact/file.

# Examples

- [[./examples/example-dotnet-default-plateau|Example: creating the .NET `default` plateau]] — full mapping from solution skills to the real `skills/dotnet/architecture/artifacts/plateau/default` structure.
- [[./examples/example-python-default-cli-plateau|Example: creating the Python `default-cli` plateau]] — mapping from `solution-default-cli` to a Python CLI plateau structure.

# Check list

- [ ] Target stack is known or inferred from solution skills.
- [ ] Output folder does not already contain a plateau with the same name, or the user confirmed replacement.
- [ ] Every solution in `{solutions}` has been scanned for `Implementation/` files.
- [ ] Repository-level content is aggregated into `sln-{plateau-name}.skill.md`.
- [ ] Artifact-level content is merged into `artifact-{normalized}.skill.md` files.
- [ ] File-level content is merged into `file-{normalized}.skill.md` files.
- [ ] Plateau root skill summarizes goals, core principles, capabilities, and use-cases.
- [ ] `created_by` lists every contributing solution.
- [ ] `version` is set to current UTC timestamp `YYYYMMDDHHMMSS`.
- [ ] No `hint`, `example`, or `code example` blocks remain in final files.
