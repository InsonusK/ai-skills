---
name: plateau-create-by-solutions
<<<<<<< HEAD
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
=======
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

>>>>>>> 11a7c72188d1a29a8b798607e9dc6bf95097f294
1. The parent solution skill file (`solution-*.skill.md`).
2. The specific implementation/template file inside that solution that contributed the content.

```example
__Applied solutions:__
<<<<<<< HEAD
- [[skills/dotnet/architecture/solutions/🧩validated/solution-command-integration.skill/solution-command-integration.skill.md|solution-command-integration]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-command-integration.skill/Implementation/Shared.csproj.extend.md|Shared.csproj extend]]
=======
- [[skills/common-workflow/architecture/solutions/solution-command-integration.skill/solution-command-integration.skill.md|solution-command-integration]] - [[skills/common-workflow/architecture/solutions/solution-command-integration.skill/Implementation/Shared.csproj.extend.md|Shared.csproj extend]]
>>>>>>> 11a7c72188d1a29a8b798607e9dc6bf95097f294
```

When the content comes directly from the solution skill file and there is no separate implementation/template file, list the solution skill file once.

```example
__Applied solutions:__
<<<<<<< HEAD
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
=======
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
>>>>>>> 11a7c72188d1a29a8b798607e9dc6bf95097f294
