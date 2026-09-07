---
name: plateau-component-create
description: Define how to build a Plateau Component — an optional, cross-cutting capability (logging, tracing, caching, ...) that attaches to a plateau's composition root without being composed into the plateau itself
whenToUse: when a capability needs to be attachable, as an optional add-on, to a plateau (or several plateaus) at its composition root — registered once via DI/middleware, working identically regardless of which Solutions the plateau composed, and never touching a module's own project files — as opposed to a Solution, which is composed into a specific plateau's `created_by` and may extend module-internal classes
tags:
  - skill/architecture/component/design
  - stack
  - concern/architecture
---

# Goal
- Give an optional, cross-cutting capability (logging, tracing, caching, health checks, ...) a shape distinct from a Solution and a Plateau, so it can attach to any qualifying plateau without forcing that plateau to branch into "with X" / "without X" variants.
- Let an agent decide, before building a new architectural unit, whether it is a Solution, a Plateau, or a Plateau Component.

# Core Principle
- A **Plateau Component** is a self-contained capability delivered as its own project and wired in exactly once, at the composition root (DI container / middleware / pipeline registration) — never inside a module's own project.
- A Plateau Component never appears in a plateau's `created_by` and is never assembled into a plateau's `structure/` folder (see [[skills/common-workflow/architecture/design/plateau-create-by-solutions.skill/plateau-create-by-solutions.skill|plateau-create-by-solutions]]). It is applied separately, when composing an actual service from a chosen plateau, not baked into the plateau's own definition — this is what lets the same plateau be used with or without the component, and lets one component attach to many unrelated plateaus, without multiplying plateau variants.
- Because a Plateau Component never touches module-internal files and never requires a specific Solution's types, its `built_on_plateau` names only the minimum composition-root baseline it needs (e.g. the plateau that first establishes `App.Host` and, if relevant, the pipeline it hooks into) — it is automatically usable on that plateau and on every plateau composed on top of it (deeper in the same `parent_plateaus` lineage), without being re-declared or re-applied per plateau.
- A Plateau Component never `depends_on` a Solution. If a capability cannot be built without referencing a specific Solution's type, it is not a Component — see [Solution vs Plateau vs Component](#solution-vs-plateau-vs-component).

# Solution vs Plateau vs Component
Apply [[skills/common-workflow/architecture/design/solution-plateau-hierarchy.skill.md#Solution vs Plateau|solution-plateau-hierarchy's Solution vs Plateau test]] first — most new units are still one of those two. Consider a Plateau Component only when the candidate also satisfies every one of the following:

1. **Own project, never a module extension.** It ships as its own project/package and never creates or extends a `{Module}.*` project.
2. **Composition-root wiring only.** It registers itself exactly once, at the composition root (`App.Host` or equivalent) — DI registration, middleware, a pipeline behavior — and never generates or edits a class inside any module.
3. **Solution-agnostic behavior.** It behaves identically regardless of which Solutions the target plateau composed. It may rely on the plateau's composition-root surface existing (a DI container, a pipeline to hook into) but never on a specific Solution's type.
4. **Removable without changing the plateau's identity.** Removing it still leaves a fully coherent, nameable plateau — unlike a Solution in `created_by`, whose removal changes what the plateau is.

If any of these fails, the candidate is not a Component:
- Needs to generate or touch per-entity/per-module code → it is a Solution.
- Only becomes meaningful once a specific Solution's type exists (e.g. a validator that can inject a repository once persistence exists) → it is still a Solution; solve the forward reference inside the Solution/Plateau mechanism (an abstract pattern in the earlier Solution, a concrete `.extend.md` contributed later by the Solution that introduces the concrete type) — do not pull it out as a Component just to avoid the forward reference.
- Changes what the plateau fundamentally provides (e.g. adding persistence turns a stateless plateau into a statefull one) → it is part of the plateau's own composition, not an optional add-on.

# Fields
On a Plateau Component:

| Field | Type | Meaning |
| --- | --- | --- |
| `built_on_plateau` | single wikilink, optional | The minimum plateau whose composition root this component needs. Empty only when the component needs nothing beyond the bare composition root every plateau has. State it explicitly either way — never leave it unstated. |
| `creates` | list | Classes/projects this component creates — its own project and whatever it creates inside it. Same convention as a Solution's `creates`. |
| `extends` | list | Classes/projects this component extends — normally only the composition-root file (e.g. `App.Host.csproj`/`Program.cs`). Never a `{Module}.*` file. |
| `depends_on` | — | Never set on a Component. A Component does not depend on a Solution — see Core Principle. |

# Workflow

## How to build a component
1. Confirm the candidate really is a Component using [Solution vs Plateau vs Component](#solution-vs-plateau-vs-component). If any check fails, stop and build a Solution instead, following [[skills/common-workflow/architecture/design/solution-create.skill/solution-create.skill|solution-create]].
2. Detect the target language/stack, or ask the user if unclear.
3. Use the template folder matching the target stack:
   - .NET: [templates/dotnet/component-{Component}.skill.template](./templates/dotnet/component-{Component}.skill.template/)
   - Other stacks can be added as separate subfolders under `templates/` when needed, following the same shape as [[skills/common-workflow/architecture/design/solution-create.skill/solution-create.skill|solution-create]]'s own per-stack templates.
4. Fill the template with real content, following its own `# How Apply this template` section.
5. Determine the minimum `built_on_plateau` — the shallowest plateau that already has everything this component needs at the composition root (usually `App.Host` existing, sometimes also a pipeline to hook a behavior into). Ask the user if unclear.
6. Remove all `hint`, `example`, and `code example` blocks and the `# How Apply this template` block from the final skill file.
7. Place the finished skill at `skills/{stack}/architecture/component/component-{Component}.skill/component-{Component}.skill.md`.

# Rule

## MUST
- Confirm a candidate unit against [Solution vs Plateau vs Component](#solution-vs-plateau-vs-component) before building it as a Component.
  - Risk: a unit that actually needs per-module code, or that depends on a specific Solution's type, gets built as a Component and either can't be implemented without breaking rule 1/2, or silently smuggles a Solution-level dependency in through the back door.
  - Fix: run the test; if it fails, build a Solution instead — solving a forward reference to a not-yet-existing type via the Solution/Plateau mechanism, not by mislabeling the unit as a Component.
- Never add a Component to any plateau's `created_by`, and never assemble it into a plateau's `structure/` folder.
  - Risk: baking an optional capability into a plateau's own definition forces every future user of that plateau to take the capability, and forces a second plateau variant to exist for "without it" — exactly the branching this unit exists to avoid.
  - Fix: keep the component in its own catalog folder (`skills/{stack}/architecture/component/`) and apply it separately when composing an actual service.
- Never declare `depends_on` on a Solution from a Component.
  - Risk: a Component that depends on a specific Solution's type stops being usable across every plateau/service that doesn't happen to include that Solution, defeating the reason it was built as a Component instead of a Solution.
  - Fix: rely only on the composition-root surface named by `built_on_plateau`; if a specific Solution's type is genuinely required, build a Solution instead.
- Give every Component a project of its own; never create or extend a `{Module}.*` project or class from a Component's `Implementation/` files.
  - Risk: module-internal changes coming from a "component" are invisible to `plateau-create-by-solutions`'/`plateau-update-by-solutions`'s scans (which only look at Solutions), so they silently drift from whatever the module actually contains.
  - Fix: keep every Component `Implementation/` file scoped to the component's own project plus, at most, the composition-root registration file.
- State the minimum `built_on_plateau` explicitly, or explicitly note that the component needs nothing beyond the bare composition root, rather than leaving it unstated.
  - Risk: without a stated minimum, an agent cannot tell which plateaus the component is actually safe to attach to.
  - Fix: name the shallowest qualifying plateau, or state explicitly that any plateau qualifies.
- Remove all `hint`, `example`, and `code example` blocks, and the `# How Apply this template` block, from the final skill file.
  - Risk: the final skill is noisy and the agent cannot tell binding rules from authoring aids.
  - Fix: delete every `hint`/`example`/`code example` fenced block and the `# How Apply this template` section before committing.
- Tag the component skill and its Implementation files with the facet tags required by [skill-design](skills/common-workflow/skill-design.skill/skill-design.skill.md): the component file carries `component/{component-name}`; every file under `Implementation/` carries `component/{component-name}` and `element/{element-name}`.
  - Risk: tag-expression queries cannot resolve which files belong to the component.
  - Fix: fill the `tags:` block as shown in the templates.

## SHOULD
- Prefer a Component over duplicating the same optional capability's Solution across every plateau that might want it.
- Record architecture decisions made while building the component as an ADR, following [[skills/common-workflow/architecture/design/adr-create.skill/adr-create.skill|adr-create]].

## MAY
- Give a Component an empty `built_on_plateau` when it truly needs nothing beyond the bare composition root present in every plateau.

# Check list
- [ ] The candidate was checked against [Solution vs Plateau vs Component](#solution-vs-plateau-vs-component) before being built as a Component.
- [ ] The component is not listed in any plateau's `created_by` and not assembled into any plateau's `structure/`.
- [ ] The component declares no `depends_on` on a Solution.
- [ ] Every `Implementation/` file is scoped to the component's own project or the composition-root registration file — none touch a `{Module}.*` project or class.
- [ ] `built_on_plateau` is stated explicitly (a specific plateau, or explicitly "none required").
- [ ] No `hint`, `example`, `code example` blocks or `# How Apply this template` section remain in the final skill.
- [ ] Facet tags are filled on the component file and its Implementation files.
