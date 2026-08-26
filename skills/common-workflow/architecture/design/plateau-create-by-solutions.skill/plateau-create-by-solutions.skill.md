---
name: plateau-create-by-solutions
description: Define how to build plateau skills from a set of solution skills, for any target language/stack
whenToUse: when you write skills for building a plateau
tags:
  - skill/architecture/plateau/design
  - stack
  - concern/architecture

---

# Input parameters
- {plateau-name} - name of created plateau
- {solutions} - list of solutions which must be implemented in created plateau
- {parent_plateaus} - optional list of existing plateaus this plateau composes, in addition to {solutions}. Empty when the plateau is built from scratch. See [[skills/common-workflow/architecture/design/solution-plateau-hierarchy.skill.md|solution-plateau-hierarchy]] for merge semantics.
- {standalone} - whether the built plateau is meant to be usable/deployable on its own (`true`) or exists only to be composed into a larger plateau (`false`). Ask the user if unclear.
- {stack} - target language/stack of the plateau (`dotnet`, `python`, ...). Detect it from the {solutions} (their `domain`/`tags` header properties) or ask the user if it is unclear
- {output} - folder where you should put created plateau skills. Default `skills/{stack}/architecture/plateau`

# Plateau element skill naming
Every skill that describes a plateau element (repository/solution, project/package, class/module, etc.) must include the plateau name in its identity:

1. **Skill file name** — must start with the prefix `plateau-{plateau-name}--`.
   - Example for a class skill: `plateau-{plateau-name}--class-{name}.skill.md`
   - Example for a project skill: `plateau-{plateau-name}--csproj-{name}.skill.md`
   - Example for a package skill: `plateau-{plateau-name}--package-{name}.skill.md`
   - Example for a module skill: `plateau-{plateau-name}--module-{name}.skill.md`
   - Example for a repository/solution skill: `plateau-{plateau-name}--sln-{plateau-name}.skill.md` (or `plateau-{plateau-name}--repo-{plateau-name}.skill.md` for Python)

2. **Skill description** — the `description` header of an element skill must explicitly state which plateau the element belongs to.
   - Example: `Class {name} in the {plateau-name} plateau`
   - Example: `Project {name} of the {plateau-name} plateau`

3. **Skill `name` header property** — must carry the same `plateau-{plateau-name}--` prefix as the skill file name (point 1), not just the bare element name.
   - Example for a class skill: `name: plateau-{plateau-name}--class-{name}`
   - Example for a project skill: `name: plateau-{plateau-name}--csproj-{name}`
   - Risk: different plateaus routinely implement overlapping elements — a shared parent plateau's `structure/` is copied into every plateau that composes it (per [[skills/common-workflow/architecture/design/solution-plateau-hierarchy.skill.md|solution-plateau-hierarchy]]'s union-merge), and independent plateaus at the same depth often need the same class/project (e.g. every plateau with a `{Module}.Domain.Tests` project needs its own step-definitions class). If `name` is only the bare element name (`class-entity`, `csproj-shared`), every plateau's copy collides on that same `name` value, which breaks any tool that indexes skills by `name`.
   - Fix: give every element skill's `name` the full `plateau-{plateau-name}--{element-name}` value — identical to its file name minus the `.skill.md` extension. This is unambiguous even when two plateaus define an element with the exact same role and content, and needs no cross-plateau coordination to stay unique.

# Prerequisites
Read [[skills/common-workflow/architecture/design/solution-create.skill/solution-create.skill|solution-create]] first. It defines how a solution-skill is structured per stack and what files it produces. A plateau is built by aggregating those produced files across all selected solutions.

Read [[skills/common-workflow/architecture/design/adr-create.skill/adr-create.skill|adr-create]] too. Assembling a plateau out of several solutions forces choices — resolving a conflict between solutions, excluding a solution — that belong to the plateau itself, not to any single solution. Record them following [Recording plateau-level decisions](#recording-plateau-level-decisions).

Read [[skills/common-workflow/architecture/design/solution-plateau-hierarchy.skill.md|solution-plateau-hierarchy]] too, when {solutions} is not the only input — i.e. when this plateau also composes one or more existing plateaus via `parent_plateaus`. It defines the union-by-default merge semantics and the `standalone` field this skill's templates now carry.

Be aware of [[skills/common-workflow/architecture/design/plateau-component-create.skill/plateau-component-create.skill.md|plateau-component-create]] too. A Plateau Component (an optional, cross-cutting capability like logging or tracing) is never part of {solutions} and never belongs in `created_by` or `structure/` — it attaches separately, to an already-composed service, not to the plateau's own definition. If a candidate in {solutions} looks like it only wires itself in at the composition root and never touches a module, verify it against that skill's test before assembling it into this plateau.

# Solution-skill structure
Every solution-skill has an `Implementation/` folder with concrete mutations. The file patterns inside `Implementation/` depend on {stack}. Recognize these file patterns:

## .NET (`stack: dotnet`)
| File pattern | What it describes | Becomes |
| ------------ | ----------------- | ------- |
| `Implementation/Repository.create.md` | Repository-level changes: solution layout, project list, layer responsibilities | Content for `plateau-{plateau-name}--sln-{plateau-name}.skill.md` |
| `Implementation/{Project}.csproj.create.md` | A project created by this solution | One `plateau-{plateau-name}--csproj-{normalized}.skill.md` |
| `Implementation/{Project}.csproj.extend.md` | A project extended by this solution | Merged into the same `plateau-{plateau-name}--csproj-{normalized}.skill.md` |
| `Implementation/{Project}.csproj.create/{Class}.cs.create.md` | A class created inside a project | One `plateau-{plateau-name}--class-{normalized}.skill.md` |
| `Implementation/{Project}.csproj.extend/{Class}.cs.create.md` | A class created inside an extended project | One `plateau-{plateau-name}--class-{normalized}.skill.md` |
| `Implementation/{Project}.csproj.extend/{Class}.cs.extend.md` | A class extended inside a project | Merged into the same `plateau-{plateau-name}--class-{normalized}.skill.md` |

> `{Project}` can be a concrete name (`Shared`, `BuildingBlocks`, `App.Host`) or a placeholder (`{Module}.Api`, `{Module}.Application`, `{Module}.Domain`, `{Module}.Interfaces`).

## Python (`stack: python`)
| File pattern | What it describes | Becomes |
| ------------ | ----------------- | ------- |
| `Implementation/Repository.create.md` | Repository-level changes that are not specific to a single package/app: how several packages/apps in the same repo relate to each other | Content for `plateau-{plateau-name}--repo-{plateau-name}.skill.md` |
| `Implementation/{App}.create.md` (`element_kind: project`) | The root package/app created by this solution | One `plateau-{plateau-name}--package-{normalized}.skill.md` |
| `Implementation/{App}.extend.md` (`element_kind: project`) | The root package/app extended by this solution | Merged into the same `plateau-{plateau-name}--package-{normalized}.skill.md` |
| `Implementation/{App}.{dotted.path}.py.create.md` (`element_kind: class` or `functions`) | A class or a functions module created inside the package | One `plateau-{plateau-name}--module-{normalized}.skill.md` |
| `Implementation/{App}.{dotted.path}.py.extend.md` | A class or a functions module extended inside the package | Merged into the same `plateau-{plateau-name}--module-{normalized}.skill.md` |
| `Implementation/{App}.{dotted.path}.__init__.py.create.md` (`element_kind: init`) | A package `__init__.py` created inside the package | One `plateau-{plateau-name}--module-{normalized}.skill.md` |
| `Implementation/{App}.{dotted.path}.__init__.py.extend.md` | A package `__init__.py` extended inside the package | Merged into the same `plateau-{plateau-name}--module-{normalized}.skill.md` |

> `{App}` can be a concrete name (`myapp`, `billing_service`) or a placeholder (`{App}`, `{Package}`, `{Service}`). `{dotted.path}` mirrors the folder path with `.` instead of `/` (e.g. `cli.backup` means `cli/backup.py`).
>
> `Repository.create.md` / `Repository.extend.md` is a stack-agnostic pattern: use it whenever a solution's change is not specific to one project/package but affects how multiple projects/packages relate to each other in the repository. Most single-package Python repositories will never populate this tier — a plateau built from them will only have the package and module tiers.

# How to build a plateau
1. Detect {stack} from the {solutions} or ask the user
2. Define does {output} folder contain folder with name {plateau-name}
   - If folder exist ask user: Does he want to replace exist plateau.
3. Create in {output} folder new folder with name {plateau-name}
4. Create subfolder `{output}/{plateau-name}/plateau-{plateau-name}.skill/`
   - This folder holds the plateau root skill and its example application
5. Create subfolder `{output}/{plateau-name}/plateau-{plateau-name}.skill/example/`
   - Put a real, complete, minimal example application built according to the plateau here
   - The example must demonstrate the plateau's patterns in executable/runnable form and must be referenced from the plateau root skill
   - If {parent_plateaus} is non-empty, seed this `example/` folder from the first parent plateau's `example/` folder first, then extend it with the patterns introduced by this plateau's {solutions}. The child example must evolve the parent example, not recreate it from scratch. When multiple parents exist, pick the parent whose example is closest in shape or ask the user if it is unclear.
6. Create subfolder `{output}/{plateau-name}/structure`
7. If {parent_plateaus} is non-empty, seed `{output}/{plateau-name}/structure` from every parent plateau's own `structure/` folder first, merged by project/class per [[skills/common-workflow/architecture/design/solution-plateau-hierarchy.skill.md|solution-plateau-hierarchy]]'s union-by-default rule
   - Copy every project/class skill file from each parent's `structure/` folder
   - When two parents (or a parent and {solutions}) define the same project/class, merge their content the same way step 9/10 merge `.create.md`/`.extend.md` files — by section, keeping `__Applied solutions:__` trailers from every contributor
   - On any conflict (disagreeing content for the same project/class/rule) between parents, or between a parent and {solutions}, stop and ask the user, then record a plateau-level ADR (see [Recording plateau-level decisions](#recording-plateau-level-decisions))
8. Discover all projects/packages and classes/modules contributed by {solutions}
   - Scan `Implementation/` folder in every solution-skill
   - .NET: collect all `{Project}.csproj.create.md` and `{Project}.csproj.extend.md`, and all class files nested under them
   - Python: collect all `{App}.create.md`/`{App}.extend.md` (`element_kind: project`), and all class/functions/init files nested under them
   - Normalize placeholder names (`{Module}`, `{App}`) to generic templates (see [Mapping rules](#mapping-rules))
9. Create the repository-level skill using the template that matches {stack}
   - .NET: `plateau-{plateau-name}--sln-{plateau-name}.skill.md` using [templates/dotnet/sln-{name}.skill.template.md](skills/common-workflow/architecture/design/plateau-create-by-solutions.skill/templates/dotnet/sln-{name}.skill.template.md)
   - Python: `plateau-{plateau-name}--repo-{plateau-name}.skill.md` using [templates/python/repo-{name}.skill.template.md](skills/common-workflow/architecture/design/plateau-create-by-solutions.skill/templates/python/repo-{name}.skill.template.md)
   - Aggregate all `Repository.create.md`/`Repository.extend.md` files from {solutions}, plus every parent's own repository-level skill content when {parent_plateaus} is non-empty
   - Keep repository-level content only
10. For each discovered project/package create its skill using the template that matches {stack}
    - .NET: `plateau-{plateau-name}--csproj-{normalized-name}.skill.md` using [templates/dotnet/csproj-{name}.skill.template.md](skills/common-workflow/architecture/design/plateau-create-by-solutions.skill/templates/dotnet/csproj-{name}.skill.template.md)
    - Python: `plateau-{plateau-name}--package-{normalized-name}.skill.md` using [templates/python/package-{name}.skill.template.md](skills/common-workflow/architecture/design/plateau-create-by-solutions.skill/templates/python/package-{name}.skill.template.md)
    - Merge `.create.md` and all `.extend.md` files for the same project/package
    - Keep project/package-level content only
11. For each discovered class/module create its skill using the template that matches {stack}
    - .NET: `plateau-{plateau-name}--class-{normalized-name}.skill.md` using [templates/dotnet/class-{name}.skill.template.md](skills/common-workflow/architecture/design/plateau-create-by-solutions.skill/templates/dotnet/class-{name}.skill.template.md)
    - Python: `plateau-{plateau-name}--module-{normalized-name}.skill.md` using [templates/python/module-{name}.skill.template.md](skills/common-workflow/architecture/design/plateau-create-by-solutions.skill/templates/python/module-{name}.skill.template.md)
    - Merge `.create.md` and `.extend.md` files for the same class/module
    - Keep class/module-level content only
12. Create `plateau-{plateau-name}.skill/plateau-{plateau-name}.skill.md` using the plateau template that matches {stack}
    - .NET: [templates/dotnet/plateau-{name}.skill.template.md](skills/common-workflow/architecture/design/plateau-create-by-solutions.skill/templates/dotnet/plateau-{name}.skill.template.md)
    - Python: [templates/python/plateau-{name}.skill.template.md](skills/common-workflow/architecture/design/plateau-create-by-solutions.skill/templates/python/plateau-{name}.skill.template.md)
    - This is the plateau summary: goal, core principles, capabilities, use-cases
    - It is not a code-generation template
    - If {parent_plateaus} is non-empty, describe the union of every parent's summary plus the delta that {solutions} add or change on top — the reader should not need to open every parent to get the full picture.
    - If {parent_plateaus} is empty, describe the complete plateau built from all solutions in `created_by`
13. Fill every skill template with real content
    - Follow `# How Apply this template` instructions inside each template
    - Remove all `hint` and `example` blocks from the final skill files
14. Fill header properties
    - `name` by {plateau-name}
    - `version` by current UTC timestamp with format `YYYYMMDDHHMMSS`
    - `parent_plateaus` — list of wikilinks to every plateau this one composes. Leave empty when the plateau is built from scratch without a parent. Use a wikilink, for example `[[skills/dotnet/architecture/draft/plateau/plateau-stateless-non-interactive-service/plateau-stateless-non-interactive-service.skill/plateau-stateless-non-interactive-service.skill.md|plateau-stateless-non-interactive-service]]`. A single-element list expresses what the old singular `parent_plateau` used to mean; several elements express composition of independent plateaus — see [[skills/common-workflow/architecture/design/solution-plateau-hierarchy.skill.md|solution-plateau-hierarchy]] for merge semantics.
    - `created_by` — list of wikilinks to every solution skill applied directly by this plateau, on top of whatever `parent_plateaus` already contribute. If `parent_plateaus` is empty, `created_by` lists every solution that defines the plateau.
    - `standalone` — `true` if this plateau is meant to be usable/deployable on its own, `false` if it exists only to be composed into a larger plateau. Use {standalone} if given, otherwise ask the user.

# Mapping rules

## .NET project name normalization
Map the project file name to the skill file name using kebab-case:

| Project file | Skill file | Notes |
| ------------ | ---------- | ----- |
| `Shared.csproj` | `plateau-{plateau-name}--csproj-shared.skill.md` | Concrete cross-cutting project |
| `BuildingBlocks.csproj` | `plateau-{plateau-name}--csproj-building-blocks.skill.md` | Concrete technical-patterns project |
| `App.Host.csproj` | `plateau-{plateau-name}--csproj-app-host.skill.md` | Concrete composition-root project |
| `App.Infrastructure.csproj` | `plateau-{plateau-name}--csproj-app-infrastructure.skill.md` | Concrete persistence project |
| `App.Infrastructure.Migrations.csproj` | `plateau-{plateau-name}--csproj-app-infrastructure-migrations.skill.md` | Concrete migrations project |
| `App.Queries.csproj` | `plateau-{plateau-name}--csproj-app-queries.skill.md` | Concrete cross-module read-model project |
| `{Module}.Api.csproj` | `plateau-{plateau-name}--csproj-module-api.skill.md` | Generic module template |
| `{Module}.Application.csproj` | `plateau-{plateau-name}--csproj-module-application.skill.md` | Generic module template |
| `{Module}.Domain.csproj` | `plateau-{plateau-name}--csproj-module-domain.skill.md` | Generic module template |
| `{Module}.Interfaces.csproj` | `plateau-{plateau-name}--csproj-module-interfaces.skill.md` | Generic module template |

## .NET class name normalization
Map the class file name to the skill file name using kebab-case. Preserve interface prefix `I-` as `i-`:

| Class file | Skill file |
| ---------- | ---------- |
| `ICommand.cs` | `plateau-{plateau-name}--class-i-command.skill.md` |
| `IQuery.cs` | `plateau-{plateau-name}--class-i-query.skill.md` |
| `ValidationBehavior.cs` | `plateau-{plateau-name}--class-validation-behavior.skill.md` |
| `ConcurrencyBehavior.cs` | `plateau-{plateau-name}--class-concurrency-behavior.skill.md` |
| `ModuleRegistration.cs` | `plateau-{plateau-name}--class-module-registration.skill.md` |
| `EntityVersionResolverFactory.cs` | `plateau-{plateau-name}--class-entity-version-resolver-factory.skill.md` |

## Python package/app root normalization
Map the package/app root name to the skill file name using kebab-case:

| Package/app root | Skill file | Notes |
| ----------------- | ---------- | ----- |
| `{App}` | `plateau-{plateau-name}--package-app.skill.md` | Generic app/package template |
| `myapp` | `plateau-{plateau-name}--package-myapp.skill.md` | Concrete top-level package |
| `{Service}` | `plateau-{plateau-name}--package-service.skill.md` | Generic service package template |
| `billing_service` | `plateau-{plateau-name}--package-billing-service.skill.md` | Concrete service package |

## Python module normalization
Drop the `{App}.` prefix and the trailing `.py`/`.__init__.py`, replace remaining `.` separators with `-`, convert `snake_case` to kebab-case, and replace `__init__` with `init`:

| Implementation file | Skill file |
| -------------------- | ---------- |
| `{App}.cli.py.create.md` | `plateau-{plateau-name}--module-cli.skill.md` |
| `{App}.cli.__init__.py.create.md` | `plateau-{plateau-name}--module-cli-init.skill.md` |
| `{App}.cli.{Command}.py.create.md` | `plateau-{plateau-name}--module-cli-command.skill.md` |
| `{App}.command.backup.py.create.md` | `plateau-{plateau-name}--module-command-backup.skill.md` |
| `{App}.functions.helpers.py.create.md` | `plateau-{plateau-name}--module-functions-helpers.skill.md` |
| `{App}.service.backup_service.py.create.md` | `plateau-{plateau-name}--module-service-backup-service.skill.md` |

## .create vs .extend
Both file types contribute to the same target skill:
- `.create.md` introduces the project/package or class/module and its base responsibilities
- `.extend.md` adds responsibilities brought by other solutions
- Merge content from all files into one skill, grouping by section (Goal, Core Principles, Structure, Rules, Anti-patterns, Check list)
- List all contributing files in `__Applied solutions:__`

## Solution selection
- Never include a Plateau Component in {solutions} — see [Prerequisites](#prerequisites) and [[skills/common-workflow/architecture/design/plateau-component-create.skill/plateau-component-create.skill.md|plateau-component-create]].
- Include every solution that contributes at least one project/package, class/module, or repository-level change
- Classification, taxonomy, or policy solutions may affect only the repository skill and the plateau root skill (for example, by defining an entity type matrix). Include them in `created_by` and `__Applied solutions:__` even if they have no direct code files
- If a solution from {solutions} has no `Implementation/` content and does not affect plateau structure, record the decision to exclude it as a plateau-level ADR (see [Recording plateau-level decisions](#recording-plateau-level-decisions)) or ask the user for clarification

# Recording plateau-level decisions
Building a plateau forces choices that are easy to forget once the plateau is assembled — for example resolving conflicting rules between two solutions, resolving conflicting content between two parent plateaus (or a parent and {solutions}) when composing via `parent_plateaus`, or excluding a solution that has no `Implementation/` content. The plateau owns these decisions, not any single structural skill, because they are about how the solutions and plateaus combine.

- Store plateau-level ADRs in `{output}/{plateau-name}/adr/`, a folder sibling to `structure/`.
- Follow [[skills/common-workflow/architecture/design/adr-create.skill/adr-create.skill|adr-create]] to write each ADR.
- List every created ADR in the plateau root skill's `adr:` YAML property and link it from the relevant section of the plateau root skill (for example `Core Principals` or `Capabilities`).

# Examples
- [[skills/common-workflow/architecture/design/plateau-create-by-solutions.skill/examples/example-dotnet-plateau.md|.NET plateau example]]
- [[skills/common-workflow/architecture/design/plateau-create-by-solutions.skill/examples/example-python-plateau.md|Python plateau example]]

# Applied solutions list format
Every content section that summarizes one or more source solutions must end with an `__Applied solutions:__` list.

Each bullet must contain **exactly two wikilinks separated by ` - `**:
1. The parent solution skill file (`solution-*.skill.md`).
2. The specific implementation/template file inside that solution that contributed the content.

```example
__Applied solutions:__
- [[skills/dotnet/architecture/draft/solutions/solution-command-integration.skill/solution-command-integration.skill.md|solution-command-integration]] - [[skills/dotnet/architecture/draft/solutions/solution-command-integration.skill/Implementation/Shared.csproj.extend.md|Shared.csproj extend]]
```

When the content comes directly from the solution skill file and there is no separate implementation/template file, list the solution skill file once.

```example
__Applied solutions:__
- [[skills/dotnet/architecture/draft/solutions/solution-command-integration.skill/solution-command-integration.skill.md|solution-command-integration]]
```

# Repository/root skill structure
The repository/root skill (`plateau-*--sln-*.skill.md` for .NET, `plateau-*--repo-*.skill.md` for Python) describes the whole plateau at the highest level. Its sections must stay at that level.

`## Project Structure`:
- Show **only project/package folders**.
- Do not list class/module files or sub-folders inside projects/packages.

`## Directory and class skills`:
- Show **only project/package directories**, the matching project/package template skill file, and a short description.
- Do not list individual class/module skill files in this table.

```example
| `Directory\|file` | template link | Description |
| ---------------- | ------------- | ----------- |
| /Shared | [[plateau-{plateau-name}--csproj-Shared.skill.md\|plateau-{plateau-name}--csproj-Shared.skill]] | Cross-cutting primitives |
| /{Module}.Domain | [[plateau-{plateau-name}--csproj-module-domain.skill.md\|plateau-{plateau-name}--csproj-module-domain.skill]] | Business logic |
```

```example
| `Directory\|file` | template link | Description |
| ---------------- | ------------- | ----------- |
| /{App} | [[plateau-{plateau-name}--package-app.skill.md\|plateau-{plateau-name}--package-app.skill]] | CLI application root |
```

# Rules
MUST:
- Give every plateau element skill file a name that starts with `plateau-{plateau-name}--`.
- Write the `description` header of every plateau element skill so that it clearly states the element belongs to the `{plateau-name}` plateau.
- Write `whenToUse` in the plateau root skill and in every plateau element skill (repository/solution, project/package, class/module) as one concrete sentence stating when the agent must open that specific skill — e.g. which file/folder is being created or edited, or which task requires checking that level of the plateau. Follow the `whenToUse` baseline in [skill-design](skills/common-workflow/skill-design.skill/skill-design.skill.md) — an agent must be able to decide to open the skill from that sentence alone. A generic sentence copied across every element (e.g. "when working in the {plateau-name} plateau") is not concrete enough.
- Detect {stack} before selecting a template folder, and select the template folder (`templates/dotnet/` or `templates/python/`) that matches it.
- Remove all `hint` and `example` blocks from final skill file. Do not keep them in the final skill file.
- Follow "# How Apply this template" rules defined in the selected template.
- Write every `__Applied solutions:__` bullet as `<solution skill link> - <implementation/template link>` when an implementation/template file exists.
- Keep the repository/root skill `## Project Structure` limited to project/package folders only.
- Keep the repository/root skill `## Directory and class skills` limited to project/package directories and their template links.
- Normalize placeholder projects/packages (`{Module}`, `{App}`, `{Service}`) to generic module/package templates, not concrete names.
- Merge `.create.md` and `.extend.md` files for the same project/package/class/module into a single skill file.
- Include every solution that contributes project/package, class/module, or repository-level content in `created_by`.
- If two solutions define conflicting rules for the same project/package/class/module, resolve the conflict or ask the user before merging, and record the resolution as a plateau-level ADR (see [Recording plateau-level decisions](#recording-plateau-level-decisions)).
- Give a plateau with a non-empty `parent_plateaus` the union of every parent's content by default (not just one parent's delta), plus the delta added or changed by the solutions in `created_by` on top of that union, per [[skills/common-workflow/architecture/design/solution-plateau-hierarchy.skill.md|solution-plateau-hierarchy]].
- When two parent plateaus disagree on the same project/package/class/module — or a parent disagrees with a solution in `created_by` — stop and ask the user before merging, and record the resolution as a plateau-level ADR (see [Recording plateau-level decisions](#recording-plateau-level-decisions)).
- Set `standalone: true`/`false` explicitly on every plateau, per [[skills/common-workflow/architecture/design/solution-plateau-hierarchy.skill.md|solution-plateau-hierarchy]].
- Record every plateau-level decision (conflict resolution, solution exclusion) as an ADR in the plateau's own `adr/` folder, following [Recording plateau-level decisions](#recording-plateau-level-decisions).
- Place the plateau root skill file inside `plateau-{plateau-name}.skill/plateau-{plateau-name}.skill.md`, not directly under `{output}/{plateau-name}/`.
- Create a real example application in `plateau-{plateau-name}.skill/example/` that follows the plateau's patterns and can be used as a runnable reference. Link to the example from the plateau root skill.
- If `parent_plateaus` is non-empty, the `example/` must be copied from the parent plateau's `example/` and then extended with this plateau's new patterns. Do not build a child plateau example from scratch when a parent example exists.
MUST NOT:
- Change other skills except the one you are building without explicit instruction in the template.
- Omit the parent solution skill link from `__Applied solutions:__` bullets.
- List class/module skill files in the repository/root skill `## Directory and class skills` table.
- Create separate skill files for `.create.md` and `.extend.md` of the same project/package/class/module.
- Silently drop or override one parent's content when `parent_plateaus` has more than one entry and two parents disagree — resolve every such conflict per [Recording plateau-level decisions](#recording-plateau-level-decisions) instead.
- Use the old singular `parent_plateau` field — every plateau uses the `parent_plateaus` list, even for a single parent.
- Compose a Plateau Component into `created_by` or `structure/` — a Component is attached separately, outside plateau assembly; see [[skills/common-workflow/architecture/design/plateau-component-create.skill/plateau-component-create.skill.md|plateau-component-create]].
