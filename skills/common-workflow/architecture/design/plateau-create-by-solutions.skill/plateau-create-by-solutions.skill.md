---
name: plateau-create-by-solutions
description: Define how to build plateau skills from a set of solution skills, for any target language/stack
whenToUse: when you write skills for building a plateau
updated: 20260909
tags:
  - skill/architecture/plateau/design
  - stack
  - concern/architecture
---

# Goal
- A plateau folder at `{output}/{plateau-name}/` containing `plateau-{plateau-name}.skill/plateau-{plateau-name}.skill.md`, `plateau-{plateau-name}.skill/example/`, `structure/`, and (when decisions were made) `adr/`.
- A repository-level skill, one skill per contributed project/package, and one per contributed class/module — each file named `plateau-{plateau-name}--…`, with a `name` header equal to the file name minus `.skill.md` and a `description` stating which plateau the element belongs to.
- Every `.create.md`/`.extend.md` for the same element merged into a single skill carrying an `__Applied solutions:__` trailer that lists every contributor.
- A runnable example application under `example/`, evolved from the parent plateau's example when `parent_plateaus` is non-empty.
- A plateau-level ADR, following [[skills/common-workflow/architecture/design/adr-create.skill/adr-create.skill|adr-create]], for every conflict resolution or solution exclusion.
- Plateau root frontmatter filled: `name`, `version` (UTC `YYYYMMDDHHMMSS`), `parent_plateaus`, `created_by`, `standalone`.

# Core Principle
- **Aggregate, do not reinvent** - A plateau is built by aggregating the `Implementation/` files the selected solutions produce; read [[skills/common-workflow/architecture/design/solution-create.skill/solution-create.skill|solution-create]] first for how those files are structured per stack.
- **Plateau-scoped identity** - Every element skill's file name and `name` header carry the `plateau-{plateau-name}--` prefix, because different plateaus routinely define the same element and a bare `name` collides in any tool that indexes skills by `name`.
- **Union, then delta** - A plateau with a non-empty `parent_plateaus` is the union of every parent's content by default, plus the delta its own `created_by` solutions add on top; see [[skills/common-workflow/architecture/design/solution-plateau-hierarchy.skill.md|solution-plateau-hierarchy]].
- **The plateau owns combination decisions** - Resolving a conflict between two solutions or two parents, and excluding a solution, are decisions the plateau owns — not any single structural skill — and are recorded as plateau-level ADRs following [[skills/common-workflow/architecture/design/adr-create.skill/adr-create.skill|adr-create]].
- **A Component is not a Solution** - A Plateau Component (an optional cross-cutting capability like logging or tracing) never appears in `{solutions}`, `created_by`, or `structure/`; verify a suspicious input against [[skills/common-workflow/architecture/design/plateau-component-create.skill/plateau-component-create.skill.md|plateau-component-create]] before assembling it.

# Scope
Covers building the plateau skill files (root, repository, project/package, class/module) from a set of solution skills, for the `dotnet`, `python`, and `typescript`/`angular` stacks. Does not cover writing the solution skills themselves ([[skills/common-workflow/architecture/design/solution-create.skill/solution-create.skill|solution-create]]), the plateau-map / variability-map catalog views, or attaching a Plateau Component to an already-composed service.

# Workflow

Inputs:
- `{plateau-name}` — name of the created plateau.
- `{solutions}` — list of solutions to implement in the plateau.
- `{parent_plateaus}` — optional list of existing plateaus this plateau composes in addition to `{solutions}`; empty when built from scratch. Merge semantics: [[skills/common-workflow/architecture/design/solution-plateau-hierarchy.skill.md|solution-plateau-hierarchy]].
- `{standalone}` — whether the plateau is usable/deployable on its own (`true`) or exists only to be composed (`false`). Ask the user if unclear.
- `{stack}` — target language/stack (`dotnet`, `python`, `typescript`, …). Detect from `{solutions}` (`domain`/`tags` headers) or ask.
- `{output}` — folder for the created skills. Default `skills/{stack}/architecture/plateau`.

Before starting, read [[skills/common-workflow/architecture/design/solution-create.skill/solution-create.skill|solution-create]] and [[skills/common-workflow/architecture/design/adr-create.skill/adr-create.skill|adr-create]]; read [[skills/common-workflow/architecture/design/solution-plateau-hierarchy.skill.md|solution-plateau-hierarchy]] when `{parent_plateaus}` is non-empty; check any composition-root-only candidate against [[skills/common-workflow/architecture/design/plateau-component-create.skill/plateau-component-create.skill.md|plateau-component-create]].

1. Detect `{stack}` from `{solutions}` or ask the user.
2. Check whether `{output}/{plateau-name}/` already exists; if it does, ask the user whether to replace it.
3. Create `{output}/{plateau-name}/`.
4. Create `{output}/{plateau-name}/plateau-{plateau-name}.skill/` — holds the plateau root skill and its example application.
5. Create `{output}/{plateau-name}/plateau-{plateau-name}.skill/example/` and put a real, complete, minimal runnable example application there, demonstrating the plateau's patterns and referenced from the root skill. When `{parent_plateaus}` is non-empty, seed it from the closest parent's `example/` and then extend it — never recreate it from scratch.
6. Create `{output}/{plateau-name}/structure/`.
7. When `{parent_plateaus}` is non-empty, seed `structure/` from every parent's own `structure/` folder, merged by project/class per [[skills/common-workflow/architecture/design/solution-plateau-hierarchy.skill.md|solution-plateau-hierarchy]]'s union-by-default rule; on any conflict, stop and ask, then record a plateau-level ADR.
8. Discover every project/package and class/module contributed by `{solutions}` by scanning each solution's `Implementation/` folder, recognising the per-stack file patterns in [Recognize the solution Implementation file patterns](#recognize-the-solution-implementation-file-patterns) and normalising names per [Normalize placeholder and element names](#normalize-placeholder-and-element-names).
9. Create the repository-level skill from the stack's `templates/{stack}/` repo template, aggregating every `Repository.create.md`/`Repository.extend.md` plus each parent's repository-level content; keep repository-level content only, per [Keep the repository skill at plateau level](#keep-the-repository-skill-at-plateau-level).
10. For each discovered project/package, create its skill from the stack's project template, merging its `.create.md` and every `.extend.md` (Angular: including `.federation.extend.md` / top-level `.extend.md`); keep project/package-level content only.
11. For each discovered class/module, create its skill from the stack's class/module template, merging its `.create.md` and `.extend.md` (Angular: set `artifact_type` from the file pattern); keep class/module-level content only.
12. Create `plateau-{plateau-name}.skill/plateau-{plateau-name}.skill.md` from the stack's plateau template — the plateau summary (goal, core principles, capabilities, use-cases), not a code-generation template. With `{parent_plateaus}`, describe the union of every parent's summary plus the delta `{solutions}` add; without, describe the complete plateau built from all `created_by` solutions.
13. Fill every template with real content following its own `# How Apply this template` section, and remove every `hint`/`example` block from the final files.
14. Fill the plateau root frontmatter: `name` = `{plateau-name}`; `version` = current UTC `YYYYMMDDHHMMSS`; `parent_plateaus` = wikilinks to every composed plateau (empty when from scratch); `created_by` = wikilinks to every solution applied directly on top of the parents; `standalone` = `{standalone}` or ask.

# Rule

## MUST

### Name every element skill with the plateau prefix
Give every plateau element skill a file name starting with `plateau-{plateau-name}--`, a `name` header equal to that file name minus `.skill.md`, and a `description` that states which plateau the element belongs to.
- Violation: `name: class-entity` or `name: csproj-shared` — the bare element name.
- Risk: different plateaus routinely define overlapping elements (a shared parent's `structure/` is copied into every plateau that composes it; independent plateaus at the same depth need the same class/project), so every copy collides on the same `name` and breaks any tool that indexes skills by `name`.
- Fix: use the full `plateau-{plateau-name}--{element-name}` value for both the file name and the `name` header — e.g. `plateau-{plateau-name}--class-{name}.skill.md`, `plateau-{plateau-name}--csproj-{name}.skill.md`, `plateau-{plateau-name}--package-{name}.skill.md`, `plateau-{plateau-name}--sln-{plateau-name}.skill.md` (.NET) / `plateau-{plateau-name}--repo-{plateau-name}.skill.md` (Python/Angular).

### Write concrete whenToUse for every skill
Write `whenToUse` in the plateau root skill and in every element skill as one concrete sentence stating when the agent must open that specific skill — which file/folder is being created or edited, or which task needs that level of the plateau.
- Violation: the same generic sentence ("when working in the {plateau-name} plateau") copied across every element skill.
- Risk: the agent cannot decide from the sentence alone whether to open the skill, defeating the [[skills/design/skill-design.skill/skill-design.skill.md|skill-design]] `whenToUse` baseline.
- Fix: name the concrete file, folder, or task for each skill.

### Recognize the solution Implementation file patterns
Recognise the `Implementation/` file patterns for `{stack}` when discovering what a solution contributes, using the table for that stack.
- Risk: an unrecognised pattern drops a project, class, or repository-level change from the assembled plateau.
- Fix: match every `Implementation/` file against the stack's patterns below; `{Project}`/`{App}`/`{project}` may be concrete or a placeholder.

.NET (`stack: dotnet`):

| File pattern | Becomes |
| --- | --- |
| `Implementation/Repository.create.md` | Content for `plateau-{plateau-name}--sln-{plateau-name}.skill.md` |
| `Implementation/{Project}.csproj.create.md` | One `plateau-{plateau-name}--csproj-{normalized}.skill.md` |
| `Implementation/{Project}.csproj.extend.md` | Merged into the same `plateau-{plateau-name}--csproj-{normalized}.skill.md` |
| `Implementation/{Project}.csproj.create/{Class}.cs.create.md` | One `plateau-{plateau-name}--class-{normalized}.skill.md` |
| `Implementation/{Project}.csproj.extend/{Class}.cs.create.md` | One `plateau-{plateau-name}--class-{normalized}.skill.md` |
| `Implementation/{Project}.csproj.extend/{Class}.cs.extend.md` | Merged into the same `plateau-{plateau-name}--class-{normalized}.skill.md` |

Python (`stack: python`):

| File pattern | Becomes |
| --- | --- |
| `Implementation/Repository.create.md` | Content for `plateau-{plateau-name}--repo-{plateau-name}.skill.md` (stack-agnostic; used only when a change affects how several packages/apps relate) |
| `Implementation/{App}.create.md` (`element_kind: project`) | One `plateau-{plateau-name}--package-{normalized}.skill.md` |
| `Implementation/{App}.extend.md` (`element_kind: project`) | Merged into the same `plateau-{plateau-name}--package-{normalized}.skill.md` |
| `Implementation/{App}.{dotted.path}.py.create.md` (`element_kind: class`\|`functions`) | One `plateau-{plateau-name}--module-{normalized}.skill.md` |
| `Implementation/{App}.{dotted.path}.py.extend.md` | Merged into the same `plateau-{plateau-name}--module-{normalized}.skill.md` |
| `Implementation/{App}.{dotted.path}.__init__.py.create.md` (`element_kind: init`) | One `plateau-{plateau-name}--module-{normalized}.skill.md` |
| `Implementation/{App}.{dotted.path}.__init__.py.extend.md` | Merged into the same `plateau-{plateau-name}--module-{normalized}.skill.md` |

Angular / TypeScript (`stack: typescript`, `framework: angular`) — `Implementation/` files may sit directly under `Implementation/` or one level deep in a purely organizational topic subfolder (`GlobalStore/`, `Testing/`, …); ignore the subfolder when normalizing, use only the base name and the `element_kind`/`change_kind` frontmatter:

| File pattern | Becomes |
| --- | --- |
| `Implementation/**/Repository.create.md` / `.extend.md` | Content for `plateau-{plateau-name}--repo-{plateau-name}.skill.md` (Nx `apps/`/`libs/` layout, tag taxonomy, module-boundary allow-list, CI; or the repo layout of a non-Nx workspace) |
| `Implementation/**/{project}.project.create.md` (`element_kind: project`) | One `plateau-{plateau-name}--project-{normalized}.skill.md` |
| `Implementation/**/{project}.project.extend.md` (`element_kind: project`) | Merged into the same `plateau-{plateau-name}--project-{normalized}.skill.md` |
| `Implementation/**/{project}.federation.extend.md` / `{name}.extend.md` (`element_kind: project`) | Merged into the owning `plateau-{plateau-name}--project-{normalized}.skill.md` (or a new one if the project has no other file) |
| `Implementation/**/{name}.{artifact-type}.ts.create.md` / `.extend.md` (`element_kind: component`\|`service`\|`directive`\|`pipe`\|`guard`\|`interceptor`\|`resolver`\|`store`\|`module`) | One `plateau-{plateau-name}--class-{normalized}.skill.md`, `artifact_type` from the `.`-segment before `.ts` |
| `Implementation/**/{name}.ts.create.md` / `.extend.md` (no `.{artifact-type}` segment) | One `plateau-{plateau-name}--class-{normalized}.skill.md`, `artifact_type: module` |
| `Implementation/**/{name}.spec.ts.create.md` (`element_kind: spec`) | One `plateau-{plateau-name}--class-{normalized}.skill.md`, `artifact_type: spec` |
| `Implementation/**/{name}.scss.create.md` (`element_kind: style`) | One `plateau-{plateau-name}--class-{normalized}.skill.md`, `artifact_type: style` |
| `Implementation/**/{project}.project.create/{class}...create.md` / `.extend.md` | One `plateau-{plateau-name}--class-{normalized}.skill.md`, nested under its project's structure folder |
| `Implementation/**/{project}.project.extend/{class}...create.md` / `.extend.md` | One `plateau-{plateau-name}--class-{normalized}.skill.md` |

### Detect the stack before choosing templates
Detect `{stack}` before selecting a template folder, and use the `templates/{stack}/` folder that matches it (`templates/dotnet/`, `templates/python/`, `templates/angular/`).
- Risk: a mismatched template produces skill files in the wrong shape for the stack.
- Fix: resolve `{stack}` from the solutions' headers or the user first, then pick the template folder.

### Normalize placeholder and element names
Normalize every discovered project/package and class/module name to a kebab-case skill-file suffix using the table for its stack; normalize placeholders (`{Module}`, `{App}`, `{Feature}`, `{Service}`) to generic templates, never to concrete names.
- Violation: emitting `plateau-{plateau-name}--csproj-MyModule.skill.md` for a `{Module}.Api.csproj` placeholder.
- Risk: inconsistent names break cross-plateau merges and skill indexing.
- Fix: apply the normalization below.

.NET projects — kebab-case the file name: `Shared.csproj` → `csproj-shared`, `App.Host.csproj` → `csproj-app-host`, `App.Infrastructure.Migrations.csproj` → `csproj-app-infrastructure-migrations`, `{Module}.Domain.csproj` → `csproj-module-domain` (likewise `-module-api`, `-module-application`, `-module-interfaces`).

.NET classes — kebab-case, preserving the interface prefix `I` as `i-`: `ICommand.cs` → `class-i-command`, `ValidationBehavior.cs` → `class-validation-behavior`, `EntityVersionResolverFactory.cs` → `class-entity-version-resolver-factory`.

Python package/app roots — kebab-case: `{App}` → `package-app`, `myapp` → `package-myapp`, `{Service}` → `package-service`, `billing_service` → `package-billing-service`.

Python modules — drop the `{App}.` prefix and the trailing `.py`/`.__init__.py`, replace remaining `.` with `-`, `snake_case` → kebab-case, `__init__` → `init`: `{App}.cli.py` → `module-cli`, `{App}.cli.__init__.py` → `module-cli-init`, `{App}.service.backup_service.py` → `module-service-backup-service`.

Angular projects — drop the leading `apps/`/`libs/`/`projects/`, keep a deliberate repeated role segment (`libs/{feature}/feature` → `feature-feature`), replace `/` and `.` with `-`, kebab-case; `{Feature}` → `feature`, `{Module}` → `module`: `apps/platform-shell` → `project-platform-shell`, `libs/shared/http-core` → `project-shared-http-core`, `libs/{feature}/data-access` → `project-feature-data-access`, `projects/design-system` → `project-design-system`.

Angular classes/artifacts — drop the topic subfolder and the trailing `.{artifact-type}.ts`/`.ts`/`.spec.ts`/`.scss` and `.create`/`.extend`; keep the `.{artifact-type}` word only when it disambiguates; replace `.` and `/` with `-`; kebab-case; `{feature}`/`{component-name}` → `feature`/`component-name`: `GlobalStore/auth.store.ts` → `class-auth-store` (`store`), `UI/has-permission.directive.ts` → `class-has-permission-directive` (`directive`), `Logging/backend-log-sink.ts` → `class-backend-log-sink` (`module`), `Testing/{component-name}.visual.spec.ts` → `class-component-name-visual-spec` (`spec`), `Tokens/theme.scss` → `class-theme-style` (`style`).

### Merge .create.md and .extend.md into one skill
Merge the `.create.md` and every `.extend.md` for the same project/package/class/module into a single skill file, grouping content by section (Goal, Core Principles, Structure, Rules, Anti-patterns, Check list).
- Violation: separate skill files for `Shared.csproj.create.md` and `Shared.csproj.extend.md`.
- Risk: the element's requirements are split across files and no single skill describes it fully.
- Fix: one skill per element; `.create.md` supplies the base responsibilities, each `.extend.md` adds the responsibilities other solutions bring.

### Format every Applied solutions bullet
End every content section that summarizes one or more source solutions with an `__Applied solutions:__` list, each bullet being exactly two wikilinks separated by ` - ` — the parent solution skill file, then the specific implementation/template file that contributed the content — or a single wikilink to the solution skill file when there is no separate implementation/template file.
- Violation: a merged section with no `__Applied solutions:__` trailer, or a bullet that omits the parent solution skill link.
- Risk: the plateau loses traceability back to the solutions that produced each piece of content.
- Fix: add the trailer to every summarizing section in the required two-wikilink form.

### Keep the repository skill at plateau level
Keep the repository/root skill (`plateau-*--sln-*.skill.md` for .NET, `plateau-*--repo-*.skill.md` for Python/Angular) at the highest level: `## Project Structure` shows only project/package folders, and `## Directory and class skills` shows only project/package directories with their matching project/package template skill and a short description.
- Violation: listing individual class/module skill files or intra-project sub-folders in either section.
- Risk: the root skill duplicates class-level detail that belongs in the per-element skills and drifts from them.
- Fix: keep both sections to project/package granularity only.

### Include every contributing solution in created_by
List every solution that contributes at least one project/package, class/module, or repository-level change in `created_by` — including classification, taxonomy, or policy solutions that affect only the repository skill and the plateau root skill.
- Risk: an omitted solution's rules silently disappear from the plateau, and `__Applied solutions:__` trailers cannot cite it.
- Fix: add every contributing solution to `created_by` and to the relevant `__Applied solutions:__` lists, even when it has no direct code files.

### Resolve solution conflicts with a plateau-level ADR
When two solutions define conflicting rules for the same project/package/class/module, resolve the conflict or ask the user before merging, and record the resolution as a plateau-level ADR.
- Risk: a silent merge picks one solution's rule arbitrarily and the discarded alternative is lost.
- Fix: decide (or ask), then record the decision in `{output}/{plateau-name}/adr/` following [[skills/common-workflow/architecture/design/adr-create.skill/adr-create.skill|adr-create]].

### Build a parent_plateaus plateau as union plus delta
Give a plateau with a non-empty `parent_plateaus` the union of every parent's content by default — not one parent's delta — plus the delta the `created_by` solutions add or change on top, per [[skills/common-workflow/architecture/design/solution-plateau-hierarchy.skill.md|solution-plateau-hierarchy]].
- Violation: describing only the last parent plus the new solutions, dropping the other parents' content.
- Risk: the assembled plateau silently loses capabilities its parents provided.
- Fix: merge every parent's `structure/`, example, and summary first, then apply the `created_by` delta.

### Resolve parent conflicts before merging
When two parent plateaus disagree on the same project/package/class/module — or a parent disagrees with a `created_by` solution — stop and ask the user before merging, and record the resolution as a plateau-level ADR.
- Violation: silently overriding one parent's content with another's when `parent_plateaus` has more than one entry.
- Risk: the conflict and its resolution vanish, and the next maintainer re-hits it.
- Fix: stop, ask, record the ADR in `{output}/{plateau-name}/adr/`.

### Set standalone explicitly
Set `standalone: true` or `standalone: false` explicitly on every plateau, per [[skills/common-workflow/architecture/design/solution-plateau-hierarchy.skill.md|solution-plateau-hierarchy]].
- Risk: an unstated `standalone` leaves a reader unable to tell whether the plateau is deployable on its own.
- Fix: use `{standalone}` when given, otherwise ask the user, and write the field.

### Record every plateau-level decision as an ADR
Record every plateau-level decision (conflict resolution, solution exclusion) as an ADR in `{output}/{plateau-name}/adr/` following [[skills/common-workflow/architecture/design/adr-create.skill/adr-create.skill|adr-create]], list it in the plateau root skill's `adr:` YAML property, and link it from the relevant section of the root skill.
- Violation: a solution excluded because it has no `Implementation/` content, with the reason left only in chat.
- Risk: the decision and its alternatives are lost once the plateau is assembled.
- Fix: write the ADR, register it in `adr:`, link it from the root skill body.

### Place the plateau root skill inside its skill folder
Place the plateau root skill file at `plateau-{plateau-name}.skill/plateau-{plateau-name}.skill.md`, not directly under `{output}/{plateau-name}/`.
- Risk: a root skill outside the `.skill/` folder breaks the layout every consumer and tool expects.
- Fix: create `plateau-{plateau-name}.skill/` and put the root skill (and its `example/`) inside it.

### Create a runnable example that evolves the parent
Create a real, runnable example application in `plateau-{plateau-name}.skill/example/` that follows the plateau's patterns and is linked from the plateau root skill; when `parent_plateaus` is non-empty, copy it from the closest parent's `example/` and then extend it with this plateau's new patterns.
- Violation: building a child plateau's example from scratch when a parent example exists.
- Risk: the child example drifts from the parent's and stops being a faithful reference.
- Fix: seed from the parent, then add only the new patterns; link the example from the root skill.

### Follow the template and strip its scaffolding
Follow the `# How Apply this template` rules inside the selected template, and remove every `hint` and `example` block from the final skill files.
- Risk: leftover authoring aids make the final skills noisy and hide the binding rules.
- Fix: apply the template's own instructions, then delete every `hint`/`example` block.

### Never change other skills
Never change any skill other than the ones you are building, unless the template explicitly instructs it.
- Risk: an unrequested edit to a neighbouring skill is invisible to review and can break it.
- Fix: confine every edit to the plateau being built; raise anything else with the user.

### Never use the singular parent_plateau field
Never use the old singular `parent_plateau` field — every plateau uses the `parent_plateaus` list, even for a single parent.
- Risk: tooling that reads `parent_plateaus` ignores a `parent_plateau` value and treats the plateau as built from scratch.
- Fix: write `parent_plateaus` as a list; a single-element list expresses what `parent_plateau` used to mean.

### Never compose a Plateau Component
Never include a Plateau Component in `{solutions}`, `created_by`, or `structure/`.
- Violation: adding a logging or tracing component to `created_by` because a service needs it.
- Risk: baking an optional capability into the plateau's definition forces every user of the plateau to take it and forces a second "without it" plateau variant.
- Fix: leave the component out of assembly; it attaches separately to a composed service — see [[skills/common-workflow/architecture/design/plateau-component-create.skill/plateau-component-create.skill.md|plateau-component-create]].

## SHOULD

### Ask when a solution has no content
Ask the user, or record an exclusion ADR, when a solution in `{solutions}` has no `Implementation/` content and does not affect plateau structure.
- Risk: silently dropping it loses the record of why it is not in the plateau.
- Fix: confirm the exclusion with the user and write the ADR.

# Example
- [[skills/common-workflow/architecture/design/plateau-create-by-solutions.skill/examples/example-dotnet-plateau.md|.NET plateau example]]
- [[skills/common-workflow/architecture/design/plateau-create-by-solutions.skill/examples/example-python-plateau.md|Python plateau example]]

# Check list
- [ ] `{stack}` was detected before any template folder was selected.
- [ ] Every element skill file and `name` header carries the `plateau-{plateau-name}--` prefix; every `description` names the plateau.
- [ ] Every skill (root and element) has a concrete, non-generic `whenToUse`.
- [ ] Every `Implementation/` file was matched to a stack pattern; every placeholder name was normalized to a generic template.
- [ ] `.create.md` and `.extend.md` for the same element are merged into one skill file.
- [ ] Every summarizing section ends with an `__Applied solutions:__` list in the two-wikilink form.
- [ ] The repository/root skill's `## Project Structure` and `## Directory and class skills` stay at project/package granularity.
- [ ] Every contributing solution — including content-free classification solutions — is in `created_by`.
- [ ] With `parent_plateaus` non-empty: content is the union of every parent plus the `created_by` delta; `structure/` and `example/` were seeded from the parents.
- [ ] Every solution/parent conflict and every solution exclusion is recorded as an ADR in `{output}/{plateau-name}/adr/`, registered in the root skill's `adr:` and linked from its body.
- [ ] `standalone` is set explicitly; `parent_plateaus` (not `parent_plateau`) is used.
- [ ] The plateau root skill sits at `plateau-{plateau-name}.skill/plateau-{plateau-name}.skill.md` with a linked, runnable `example/`.
- [ ] No `hint`/`example` block and no `# How Apply this template` section remain in the final files.
- [ ] No Plateau Component appears in `{solutions}`, `created_by`, or `structure/`.
