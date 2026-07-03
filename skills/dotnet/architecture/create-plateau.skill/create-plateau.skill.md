---
name: plateau-build
description: Define how build plateau skills by patterns
whenToUse: when you write skills for building plateau
---

# Input parameters
- {plateau-name} - name of created plateau
- {solutions} - list of solutions which must be implemented in created plateau
- {output} - folder where you should put created plateau skills. Default `skills\dotnet\architecture\plateau`

# Prerequisites
Read [create-solution.skill](../create-solution.skill/create-solution.skill.md) first. It defines how a solution-skill is structured and what files it produces. A plateau is built by aggregating those produced files across all selected solutions.

# Solution-skill structure
Every solution-skill has an `Implementation/` folder with concrete mutations. Recognize these file patterns:

| File pattern | What it describes | Becomes |
| ------------ | ----------------- | ------- |
| `Implementation/Repository.create.md` | Repository-level changes: solution layout, project list, layer responsibilities | Content for `sln-{plateau-name}.skill.md` |
| `Implementation/{Project}.csproj.create.md` | A project created by this solution | One `csproj-{normalized}.skill.md` |
| `Implementation/{Project}.csproj.extend.md` | A project extended by this solution | Merged into the same `csproj-{normalized}.skill.md` |
| `Implementation/{Project}.csproj.create/{Class}.cs.create.md` | A class created inside a project | One `class-{normalized}.skill.md` |
| `Implementation/{Project}.csproj.extend/{Class}.cs.create.md` | A class created inside an extended project | One `class-{normalized}.skill.md` |
| `Implementation/{Project}.csproj.extend/{Class}.cs.extend.md` | A class extended inside a project | Merged into the same `class-{normalized}.skill.md` |

> `{Project}` can be a concrete name (`Shared`, `BuildingBlocks`, `App.Host`) or a placeholder (`{Module}.Api`, `{Module}.Application`, `{Module}.Domain`, `{Module}.Interfaces`).

# How to build a plateau
1. Define does {output} folder contain folder with name {plateau-name}
   - If folder exist ask user: Does he want to replace exist plateau.
2. Create in {output} folder new folder with name {plateau-name}
3. Create subfolder `{output}/structure`
4. Discover all projects and classes contributed by {solutions}
   - Scan `Implementation/` folder in every solution-skill
   - Collect all `{Project}.csproj.create.md` and `{Project}.csproj.extend.md`
   - Collect all class files nested under those project files
   - Normalize `{Module}` projects to generic module templates (see [Project name normalization](#project-name-normalization))
5. Create `sln-{plateau-name}.skill.md` using [sln-{name}.skill.template.md](./templates/sln-{name}.skill.template.md)
   - Aggregate all `Repository.create.md` files from {solutions}
   - Keep repository-level content only
6. For each discovered project create `csproj-{normalized-name}.skill.md` using [csproj-{name}.skill.template.md](./templates/csproj-{name}.skill.template.md)
   - Merge `.create.md` and all `.extend.md` files for the same project
   - Keep project-level content only
7. For each discovered class create `class-{normalized-name}.skill.md` using [class-{name}.skill.template.md](./templates/class-{name}.skill.template.md)
   - Merge `.cs.create.md` and `.cs.extend.md` files for the same class
   - Keep class-level content only
8. Create `plateau-{plateau-name}.skill.md` using [plateau-{name}.skill.template.md](./templates/plateau-{name}.skill.template.md)
   - This is the plateau summary: goals, core principles, capabilities, use-cases
   - It is not a code-generation template; it explains what the plateau as a whole provides
9. Fill every skill template with real content
   - Follow `# How Apply this template` instructions inside each template
   - Remove all `hint` and `example` blocks from the final skill files
10. Fill header properties
    - `plateau` by name {plateau-name}
    - `version` by current UTC timestamp with format `YYYYMMDDHHMMSS`
    - `created_by` add links to all solutions which made effect on this skill

# Mapping rules

## Project name normalization
Map the project file name to the skill file name using kebab-case:

| Project file | Skill file | Notes |
| ------------ | ---------- | ----- |
| `Shared.csproj` | `csproj-shared.skill.md` | Concrete cross-cutting project |
| `BuildingBlocks.csproj` | `csproj-building-blocks.skill.md` | Concrete technical-patterns project |
| `App.Host.csproj` | `csproj-app-host.skill.md` | Concrete composition-root project |
| `App.Infrastructure.csproj` | `csproj-app-infrastructure.skill.md` | Concrete persistence project |
| `App.Infrastructure.Migrations.csproj` | `csproj-app-infrastructure-migrations.skill.md` | Concrete migrations project |
| `App.Queries.csproj` | `csproj-app-queries.skill.md` | Concrete cross-module read-model project |
| `{Module}.Api.csproj` | `csproj-module-api.skill.md` | Generic module template |
| `{Module}.Application.csproj` | `csproj-module-application.skill.md` | Generic module template |
| `{Module}.Domain.csproj` | `csproj-module-domain.skill.md` | Generic module template |
| `{Module}.Interfaces.csproj` | `csproj-module-interfaces.skill.md` | Generic module template |

## Class name normalization
Map the class file name to the skill file name using kebab-case. Preserve interface prefix `I-` as `i-`:

| Class file | Skill file |
| ---------- | ---------- |
| `ICommand.cs` | `class-i-command.skill.md` |
| `IQuery.cs` | `class-i-query.skill.md` |
| `ValidationBehavior.cs` | `class-validation-behavior.skill.md` |
| `ConcurrencyBehavior.cs` | `class-concurrency-behavior.skill.md` |
| `ModuleRegistration.cs` | `class-module-registration.skill.md` |
| `EntityVersionResolverFactory.cs` | `class-entity-version-resolver-factory.skill.md` |

## .create vs .extend
Both file types contribute to the same target skill:
- `.create.md` introduces the project/class and its base responsibilities
- `.extend.md` adds responsibilities brought by other solutions
- Merge content from all files into one skill, grouping by section (Goal, Core Principles, Structure, Rules, Anti-patterns, Check list)
- List all contributing files in `__Applied solutions:__`

## Solution selection
- Include every solution that contributes at least one project, class, or repository-level change
- Classification, taxonomy, or policy solutions may affect only the repository skill and the plateau root skill (for example, by defining an entity type matrix). Include them in `created_by` and `__Applied solutions:__` even if they have no direct code files
- If a solution from {solutions} has no `Implementation/` content and does not affect plateau structure, document the decision to exclude it in the plateau root skill or ask the user for clarification

# Applied solutions list format
Every content section that summarizes one or more source solutions must end with an `__Applied solutions:__` list.

Each bullet must contain **exactly two wikilinks separated by ` - `**:
1. The parent solution skill file (`solution-*.skill.md`).
2. The specific implementation/template file inside that solution that contributed the content.

```markdown
__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-command-integration.skill/solution-command-integration.skill.md|solution-command-integration]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-command-integration.skill/Implementation/Shared.csproj.extend.md|Shared.csproj extend]]
```

When the content comes directly from the solution skill file and there is no separate implementation/template file, list the solution skill file once.

```markdown
__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-command-integration.skill/solution-command-integration.skill.md|solution-command-integration]]
```

# Repository skill structure
The repository skill describes the whole solution. Its sections must stay at the repository level.

`## Project Structure`:
- Show **only project folders**.
- Do not list class files or sub-folders inside projects.

`## Directory and class skills`:
- Show **only project directories**, the matching project template skill file, and a short description.
- Do not list individual class skill files in this table.

```markdown
| `Directory\|file` | template link | Description |
| ---------------- | ------------- | ----------- |
| /Shared | [[csproj-Shared.skill.md\|csproj-Shared.skill]] | Cross-cutting primitives |
| /{Module}.Domain | [[csproj-module-domain.skill.md\|csproj-module-domain.skill]] | Business logic |
```

# Rules
MUST:
- Remove all `hint` and `example` blocks from final skill file. Do not keep them in the final skill file.
- Follow "# How Apply this template" rules defined in template.
- Write every `__Applied solutions:__` bullet as `[[solution skill link]] - [[implementation/template link]]` when an implementation/template file exists.
- Keep repository skill `## Project Structure` limited to project folders only.
- Keep repository skill `## Directory and class skills` limited to project directories and project template links.
- Normalize `{Module}` projects to generic module templates, not concrete module names.
- Merge `.create.md` and `.extend.md` files for the same project/class into a single skill file.
- Include every solution that contributes project, class, or repository-level content in `created_by`.
MUST NOT:
- Change other skills except the one you are building without explicit instruction in the template.
- Omit the parent solution skill link from `__Applied solutions:__` bullets.
- List class skill files in the repository skill `## Directory and class skills` table.
- Create separate skill files for `.create.md` and `.extend.md` of the same project/class.
