---
name: plateau-create-by-solutions
description: Define how to build plateau skills from a set of solution skills, for any target language/stack
whenToUse: when you write skills for building a plateau
---

# Input parameters
- {plateau-name} - name of created plateau
- {solutions} - list of solutions which must be implemented in created plateau
- {stack} - target language/stack of the plateau (`dotnet`, `python`, ...). Detect it from the {solutions} (their `domain`/`tags` header properties) or ask the user if it is unclear
- {output} - folder where you should put created plateau skills. Default `skills/{stack}/architecture/plateau`

# Prerequisites
Read [[skills/common-workflow/architecture/design/solution-create.skill/solution-create.skill|solution-create]] first. It defines how a solution-skill is structured per stack and what files it produces. A plateau is built by aggregating those produced files across all selected solutions.

# Solution-skill structure
Every solution-skill has an `Implementation/` folder with concrete mutations. The file patterns inside `Implementation/` depend on {stack}. Recognize these file patterns:

## .NET (`stack: dotnet`)
| File pattern | What it describes | Becomes |
| ------------ | ----------------- | ------- |
| `Implementation/Repository.create.md` | Repository-level changes: solution layout, project list, layer responsibilities | Content for `sln-{plateau-name}.skill.md` |
| `Implementation/{Project}.csproj.create.md` | A project created by this solution | One `csproj-{normalized}.skill.md` |
| `Implementation/{Project}.csproj.extend.md` | A project extended by this solution | Merged into the same `csproj-{normalized}.skill.md` |
| `Implementation/{Project}.csproj.create/{Class}.cs.create.md` | A class created inside a project | One `class-{normalized}.skill.md` |
| `Implementation/{Project}.csproj.extend/{Class}.cs.create.md` | A class created inside an extended project | One `class-{normalized}.skill.md` |
| `Implementation/{Project}.csproj.extend/{Class}.cs.extend.md` | A class extended inside a project | Merged into the same `class-{normalized}.skill.md` |

> `{Project}` can be a concrete name (`Shared`, `BuildingBlocks`, `App.Host`) or a placeholder (`{Module}.Api`, `{Module}.Application`, `{Module}.Domain`, `{Module}.Interfaces`).

## Python (`stack: python`)
| File pattern | What it describes | Becomes |
| ------------ | ----------------- | ------- |
| `Implementation/Repository.create.md` | Repository-level changes that are not specific to a single package/app: how several packages/apps in the same repo relate to each other | Content for `repo-{plateau-name}.skill.md` |
| `Implementation/{App}.create.md` (`element_kind: project`) | The root package/app created by this solution | One `package-{normalized}.skill.md` |
| `Implementation/{App}.extend.md` (`element_kind: project`) | The root package/app extended by this solution | Merged into the same `package-{normalized}.skill.md` |
| `Implementation/{App}.{dotted.path}.py.create.md` (`element_kind: class` or `functions`) | A class or a functions module created inside the package | One `module-{normalized}.skill.md` |
| `Implementation/{App}.{dotted.path}.py.extend.md` | A class or a functions module extended inside the package | Merged into the same `module-{normalized}.skill.md` |
| `Implementation/{App}.{dotted.path}.__init__.py.create.md` (`element_kind: init`) | A package `__init__.py` created inside the package | One `module-{normalized}.skill.md` |
| `Implementation/{App}.{dotted.path}.__init__.py.extend.md` | A package `__init__.py` extended inside the package | Merged into the same `module-{normalized}.skill.md` |

> `{App}` can be a concrete name (`myapp`, `billing_service`) or a placeholder (`{App}`, `{Package}`, `{Service}`). `{dotted.path}` mirrors the folder path with `.` instead of `/` (e.g. `cli.backup` means `cli/backup.py`).
>
> `Repository.create.md` / `Repository.extend.md` is a stack-agnostic pattern: use it whenever a solution's change is not specific to one project/package but affects how multiple projects/packages relate to each other in the repository. Most single-package Python repositories will never populate this tier — a plateau built from them will only have the package and module tiers.

# How to build a plateau
1. Detect {stack} from the {solutions} or ask the user
2. Define does {output} folder contain folder with name {plateau-name}
   - If folder exist ask user: Does he want to replace exist plateau.
3. Create in {output} folder new folder with name {plateau-name}
4. Create subfolder `{output}/structure`
5. Discover all projects/packages and classes/modules contributed by {solutions}
   - Scan `Implementation/` folder in every solution-skill
   - .NET: collect all `{Project}.csproj.create.md` and `{Project}.csproj.extend.md`, and all class files nested under them
   - Python: collect all `{App}.create.md`/`{App}.extend.md` (`element_kind: project`), and all class/functions/init files nested under them
   - Normalize placeholder names (`{Module}`, `{App}`) to generic templates (see [Mapping rules](#mapping-rules))
6. Create the repository-level skill using the template that matches {stack}
   - .NET: `sln-{plateau-name}.skill.md` using [templates/dotnet/sln-{name}.skill.template.md](skills/common-workflow/architecture/design/plateau-create-by-solutions.skill/templates/dotnet/sln-{name}.skill.template.md)
   - Python: `repo-{plateau-name}.skill.md` using [templates/python/repo-{name}.skill.template.md](skills/common-workflow/architecture/design/plateau-create-by-solutions.skill/templates/python/repo-{name}.skill.template.md)
   - Aggregate all `Repository.create.md`/`Repository.extend.md` files from {solutions}
   - Keep repository-level content only
7. For each discovered project/package create its skill using the template that matches {stack}
   - .NET: `csproj-{normalized-name}.skill.md` using [templates/dotnet/csproj-{name}.skill.template.md](skills/common-workflow/architecture/design/plateau-create-by-solutions.skill/templates/dotnet/csproj-{name}.skill.template.md)
   - Python: `package-{normalized-name}.skill.md` using [templates/python/package-{name}.skill.template.md](skills/common-workflow/architecture/design/plateau-create-by-solutions.skill/templates/python/package-{name}.skill.template.md)
   - Merge `.create.md` and all `.extend.md` files for the same project/package
   - Keep project/package-level content only
8. For each discovered class/module create its skill using the template that matches {stack}
   - .NET: `class-{normalized-name}.skill.md` using [templates/dotnet/class-{name}.skill.template.md](skills/common-workflow/architecture/design/plateau-create-by-solutions.skill/templates/dotnet/class-{name}.skill.template.md)
   - Python: `module-{normalized-name}.skill.md` using [templates/python/module-{name}.skill.template.md](skills/common-workflow/architecture/design/plateau-create-by-solutions.skill/templates/python/module-{name}.skill.template.md)
   - Merge `.create.md` and `.extend.md` files for the same class/module
   - Keep class/module-level content only
9. Create `plateau-{plateau-name}.skill.md` using the plateau template that matches {stack}
   - .NET: [templates/dotnet/plateau-{name}.skill.template.md](skills/common-workflow/architecture/design/plateau-create-by-solutions.skill/templates/dotnet/plateau-{name}.skill.template.md)
   - Python: [templates/python/plateau-{name}.skill.template.md](skills/common-workflow/architecture/design/plateau-create-by-solutions.skill/templates/python/plateau-{name}.skill.template.md)
   - This is the plateau summary: goals, core principles, capabilities, use-cases
   - It is not a code-generation template; it explains what the plateau as a whole provides
10. Fill every skill template with real content
    - Follow `# How Apply this template` instructions inside each template
    - Remove all `hint` and `example` blocks from the final skill files
11. Fill header properties
    - `plateau` by name {plateau-name}
    - `version` by current UTC timestamp with format `YYYYMMDDHHMMSS`
    - `created_by` add links to all solutions which made effect on this skill

# Mapping rules

## .NET project name normalization
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

## .NET class name normalization
Map the class file name to the skill file name using kebab-case. Preserve interface prefix `I-` as `i-`:

| Class file | Skill file |
| ---------- | ---------- |
| `ICommand.cs` | `class-i-command.skill.md` |
| `IQuery.cs` | `class-i-query.skill.md` |
| `ValidationBehavior.cs` | `class-validation-behavior.skill.md` |
| `ConcurrencyBehavior.cs` | `class-concurrency-behavior.skill.md` |
| `ModuleRegistration.cs` | `class-module-registration.skill.md` |
| `EntityVersionResolverFactory.cs` | `class-entity-version-resolver-factory.skill.md` |

## Python package/app root normalization
Map the package/app root name to the skill file name using kebab-case:

| Package/app root | Skill file | Notes |
| ----------------- | ---------- | ----- |
| `{App}` | `package-app.skill.md` | Generic app/package template |
| `myapp` | `package-myapp.skill.md` | Concrete top-level package |
| `{Service}` | `package-service.skill.md` | Generic service package template |
| `billing_service` | `package-billing-service.skill.md` | Concrete service package |

## Python module normalization
Drop the `{App}.` prefix and the trailing `.py`/`.__init__.py`, replace remaining `.` separators with `-`, convert `snake_case` to kebab-case, and replace `__init__` with `init`:

| Implementation file | Skill file |
| -------------------- | ---------- |
| `{App}.cli.py.create.md` | `module-cli.skill.md` |
| `{App}.cli.__init__.py.create.md` | `module-cli-init.skill.md` |
| `{App}.cli.{Command}.py.create.md` | `module-cli-command.skill.md` |
| `{App}.command.backup.py.create.md` | `module-command-backup.skill.md` |
| `{App}.functions.helpers.py.create.md` | `module-functions-helpers.skill.md` |
| `{App}.service.backup_service.py.create.md` | `module-service-backup-service.skill.md` |

## .create vs .extend
Both file types contribute to the same target skill:
- `.create.md` introduces the project/package or class/module and its base responsibilities
- `.extend.md` adds responsibilities brought by other solutions
- Merge content from all files into one skill, grouping by section (Goal, Core Principles, Structure, Rules, Anti-patterns, Check list)
- List all contributing files in `__Applied solutions:__`

## Solution selection
- Include every solution that contributes at least one project/package, class/module, or repository-level change
- Classification, taxonomy, or policy solutions may affect only the repository skill and the plateau root skill (for example, by defining an entity type matrix). Include them in `created_by` and `__Applied solutions:__` even if they have no direct code files
- If a solution from {solutions} has no `Implementation/` content and does not affect plateau structure, document the decision to exclude it in the plateau root skill or ask the user for clarification

# Example: building a .NET plateau from three solutions

## Input
- plateau-name: `default`
- stack: `dotnet`
- solutions:
  - [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-sln-structure.skill/solution-sln-structure.skill|solution-sln-structure]]
  - [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-command-integration.skill/solution-command-integration.skill|solution-command-integration]]
  - [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-entity-concurrency-change.skill/solution-entity-concurrency-change.skill|solution-entity-concurrency-change]]

## Source files discovered in Implementation/

`solution-sln-structure`:
- `Repository.create.md`
- `Shared.csproj.create.md`
- `BuildingBlocks.csproj.create.md`
- `App.Host.csproj.create.md`
- `App.Infrastructure.csproj.create.md`
- `{Module}.Api.csproj.create.md`
- `{Module}.Application.csproj.create.md`
- `{Module}.Domain.csproj.create.md`
- `{Module}.Domain.csproj.create/{Entity}.cs.create.md`
- `{Module}.Interfaces.csproj.create.md`

`solution-command-integration`:
- `Shared.csproj.extend.md`
- `Shared.csproj.extend/ICommand.cs.create.md`
- `{Module}.Interfaces.csproj.extend.md`
- `{Module}.Interfaces.csproj.extend/{Command}.cs.create.md`
- `{Module}.Application.csproj.extend.md`
- `{Module}.Application.csproj.extend/{FeatureName}.Handler.cs.create.md`
- `{Module}.Application.csproj.extend/{FeatureName}.Validator.cs.create.md`
- `{Module}.Application.csproj.extend/{Module}ApplicationRegistration.cs.create.md`
- `App.Host.csproj.extend.md`

`solution-entity-concurrency-change`:
- `Shared.csproj.extend.md`
- `Shared.csproj.extend/IVersioned.cs.create.md`
- `Shared.csproj.extend/IHasVersions.cs.create.md`
- `BuildingBlocks.csproj.extend.md`
- `BuildingBlocks.csproj.extend/ConcurrencyBehavior.cs.create.md`
- `BuildingBlocks.csproj.extend/ETagEncoder.cs.create.md`
- `{Module}.Domain.csproj.extend.md`
- `{Module}.Domain.csproj.extend/{EntityName}.cs.extend.md`
- `{Module}.Application.csproj.extend.md`
- `{Module}.Application.csproj.extend/{Entity}VersionResolver.cs.create.md`
- `{Module}.Interfaces.csproj.extend.md`
- `{Module}.Interfaces.csproj.extend/{Command}.cs.extend.md`
- `{Module}.Api.csproj.extend.md`
- `{Module}.Api.csproj.extend/Single{Entity}Controller.cs.extend.md`
- `App.Host.csproj.extend.md`
- `App.Host.csproj.extend/EntityVersionResolverRegistration.cs.create.md`
- `App.Infrastructure.csproj.extend.md`
- `App.Infrastructure.csproj.extend/EntityVersionResolverFactory.cs.create.md`

## Resulting plateau structure

```
plateau/default/
├── plateau-default.skill.md
└── structure/
    ├── sln-default.skill.md
    ├── Shared/
    │   ├── csproj-shared.skill.md
    │   └── classes/
    │       ├── class-i-command.skill.md
    │       ├── class-i-has-versions.skill.md
    │       ├── class-i-entity-version-resolver.skill.md
    │       └── class-i-versioned.skill.md
    ├── BuildingBlocks/
    │   ├── csproj-building-blocks.skill.md
    │   └── classes/
    │       ├── class-concurrency-behavior.skill.md
    │       └── class-etag-encoder.skill.md
    ├── App.Host/
    │   ├── csproj-app-host.skill.md
    │   └── classes/
    │       ├── class-entity-version-resolver-registration.skill.md
    │       └── class-module-registration.skill.md
    ├── App.Infrastructure/
    │   ├── csproj-app-infrastructure.skill.md
    │   └── classes/
    │       └── class-entity-version-resolver-factory.skill.md
    ├── {Module}.Api/
    │   ├── csproj-module-api.skill.md
    │   └── classes/
    │       └── class-single-entity-controller.skill.md
    ├── {Module}.Application/
    │   ├── csproj-module-application.skill.md
    │   └── classes/
    │       ├── class-entity-version-resolver.skill.md
    │       ├── class-feature-handler.skill.md
    │       ├── class-feature-validator.skill.md
    │       └── class-module-application-registration.skill.md
    ├── {Module}.Domain/
    │   ├── csproj-module-domain.skill.md
    │   └── classes/
    │       ├── class-entity.skill.md
    │       └── class-entity-config.skill.md
    └── {Module}.Interfaces/
        ├── csproj-module-interfaces.skill.md
        └── classes/
            ├── class-command.skill.md
            └── class-query.skill.md
```

## Key observations
- `Shared.csproj.create.md` + `Shared.csproj.extend.md` (from two solutions) → one `csproj-shared.skill.md`.
- `ICommand.cs.create.md` → `class-i-command.skill.md`; `IVersioned.cs.create.md` → `class-i-versioned.skill.md`.
- `{Module}.Api.csproj.create.md` + `{Module}.Api.csproj.extend.md` → one `csproj-module-api.skill.md`.
- `{Entity}.cs.create.md` + `{EntityName}.cs.extend.md` → one `class-entity.skill.md`.
- `Repository.create.md` from `solution-sln-structure` becomes the foundation of `sln-default.skill.md`.

# Example: building a Python plateau from one solution

## Input
- plateau-name: `default`
- stack: `python`
- solutions:
  - [[skills/python/architecture/solutions/solution-default-cli.skill/solution-default-cli.skill|solution-default-cli]]

## Source files discovered in Implementation/

`solution-default-cli`:
- `{App}.create.md`
- `{App}.cli.py.create.md`
- `{App}.cli.__init__.py.create.md`
- `{App}.cli.{Command}.py.create.md`
- `{App}.command.__init__.py.create.md`
- `{App}.command.{Command}.py.create.md`
- `{App}.functions.__init__.py.create.md`
- `{App}.functions.{Function}.py.create.md`
- `{App}.service.__init__.py.create.md`
- `{App}.service.{Service}.py.create.md`

## Resulting plateau structure

```
plateau/default/
├── plateau-default.skill.md
└── structure/
    └── {App}/
        ├── package-app.skill.md
        └── modules/
            ├── module-cli.skill.md
            ├── module-cli-init.skill.md
            ├── module-cli-command.skill.md
            ├── module-command-init.skill.md
            ├── module-command-command.skill.md
            ├── module-functions-init.skill.md
            ├── module-functions-function.skill.md
            ├── module-service-init.skill.md
            └── module-service-service.skill.md
```

## Key observations
- `{App}.create.md` (`element_kind: project`) becomes the foundation of `package-app.skill.md`.
- `{App}.cli.py.create.md` → `module-cli.skill.md`; `{App}.cli.__init__.py.create.md` → `module-cli-init.skill.md`.
- `{Command}`, `{Function}` and `{Service}` stay as placeholders because `solution-default-cli` itself defines them as generic templates, not concrete names.
- This solution never populates `Repository.create.md`, so no `repo-default.skill.md` is produced — the plateau has only the package and module tiers.

# Applied solutions list format
Every content section that summarizes one or more source solutions must end with an `__Applied solutions:__` list.

Each bullet must contain **exactly two wikilinks separated by ` - `**:
1. The parent solution skill file (`solution-*.skill.md`).
2. The specific implementation/template file inside that solution that contributed the content.

```example
__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-command-integration.skill/solution-command-integration.skill.md|solution-command-integration]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-command-integration.skill/Implementation/Shared.csproj.extend.md|Shared.csproj extend]]
```

When the content comes directly from the solution skill file and there is no separate implementation/template file, list the solution skill file once.

```example
__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-command-integration.skill/solution-command-integration.skill.md|solution-command-integration]]
```

# Repository/root skill structure
The repository/root skill (`sln-*.skill.md` for .NET, `repo-*.skill.md` for Python) describes the whole plateau at the highest level. Its sections must stay at that level.

`## Project Structure`:
- Show **only project/package folders**.
- Do not list class/module files or sub-folders inside projects/packages.

`## Directory and class skills`:
- Show **only project/package directories**, the matching project/package template skill file, and a short description.
- Do not list individual class/module skill files in this table.

```example
| `Directory\|file` | template link | Description |
| ---------------- | ------------- | ----------- |
| /Shared | [[csproj-Shared.skill.md\|csproj-Shared.skill]] | Cross-cutting primitives |
| /{Module}.Domain | [[csproj-module-domain.skill.md\|csproj-module-domain.skill]] | Business logic |
```

```example
| `Directory\|file` | template link | Description |
| ---------------- | ------------- | ----------- |
| /{App} | [[package-app.skill.md\|package-app.skill]] | CLI application root |
```

# Rules
MUST:
- Detect {stack} before selecting a template folder, and select the template folder (`templates/dotnet/` or `templates/python/`) that matches it.
- Remove all `hint` and `example` blocks from final skill file. Do not keep them in the final skill file.
- Follow "# How Apply this template" rules defined in the selected template.
- Write every `__Applied solutions:__` bullet as `<solution skill link> - <implementation/template link>` when an implementation/template file exists.
- Keep the repository/root skill `## Project Structure` limited to project/package folders only.
- Keep the repository/root skill `## Directory and class skills` limited to project/package directories and their template links.
- Normalize placeholder projects/packages (`{Module}`, `{App}`, `{Service}`) to generic module/package templates, not concrete names.
- Merge `.create.md` and `.extend.md` files for the same project/package/class/module into a single skill file.
- Include every solution that contributes project/package, class/module, or repository-level content in `created_by`.
- If two solutions define conflicting rules for the same project/package/class/module, resolve the conflict or ask the user before merging.
MUST NOT:
- Change other skills except the one you are building without explicit instruction in the template.
- Omit the parent solution skill link from `__Applied solutions:__` bullets.
- List class/module skill files in the repository/root skill `## Directory and class skills` table.
- Create separate skill files for `.create.md` and `.extend.md` of the same project/package/class/module.
