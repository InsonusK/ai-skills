---
name: plateau-design-system--project-design-system
description: The publishable design-system component library — theme, tokens, and ds-* components consumed by the platform and every embeddable app
domain: skill
type: template
plateau: design-system
project_kind: library
version: 20260711120000
tags:
  - skill/template/project
  - plateau/design-system
created_by:
  - "[[skills/angular/architecture/solutions/solution-design-system-structure.skill/solution-design-system-structure.skill.md|solution-design-system-structure]]"
  - "[[skills/angular/architecture/solutions/solution-design-system-tokens.skill/solution-design-system-tokens.skill.md|solution-design-system-tokens]]"
  - "[[skills/angular/architecture/solutions/solution-design-system-components.skill/solution-design-system-components.skill.md|solution-design-system-components]]"
---

> No solution produced a dedicated `design-system.project.create.md` file — this project is established implicitly by [[skills/angular/architecture/plateau/plateau-design-system/structure/plateau-design-system--repo-design-system.skill.md|repo-design-system]]'s `Repository.create`/`Repository.extend` entries. This skill exists so the project's classes have a consistent home, per the plateau's 3-tier (repo → project → class) convention.

# Goal

- Provide a single, independently versioned npm package that both the platform monorepo and every independently deployed embeddable app consume for theming (M3 theme + `--ds-*` tokens) and UI components

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-design-system-structure.skill/solution-design-system-structure.skill.md|solution-design-system-structure]]

# Core Principles

- Ivy partial compilation output (Angular Package Format), built with ng-packagr — not locked to one specific consumer Angular version
- Every custom component consumes `--mat-sys-*` tokens directly wherever Material already models the concept; `--ds-*` tokens exist only for genuine gaps (domain-specific semantic colors, spacing, radius)
- Every component uses `input()`/`output()`/`model()` exclusively, has its own `ds-*` selector, and fully encapsulates whether it delegates to Angular Material or is custom-built internally

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-design-system-tokens.skill/solution-design-system-tokens.skill.md|solution-design-system-tokens]]
- [[skills/angular/architecture/solutions/solution-design-system-components.skill/solution-design-system-components.skill.md|solution-design-system-components]]

# Structure

## Project Structure

```
/projects/design-system
  /src
    /lib
      /{component-name}
        [ds-{component-name}.component.ts](./classes/plateau-design-system--class-component-name.skill.md)
    /styles
      [theme.scss](./classes/plateau-design-system--class-theme.skill.md)
      [custom-tokens.scss](./classes/plateau-design-system--class-custom-tokens.skill.md)
  ng-package.json
  package.json
```

## Directory and class skills

| `Directory\|file` | Description | Pattern skill |
| ------------------ | ----------- | -------------- |
| /src/styles/theme.scss | Single `mat.theme()` definition, one fixed brand palette, applied at the root selector | [[skills/angular/architecture/plateau/plateau-design-system/structure/design-system/classes/plateau-design-system--class-theme.skill\|class-theme]] |
| /src/styles/custom-tokens.scss | `--ds-*` custom properties for concepts Material's token set doesn't cover | [[skills/angular/architecture/plateau/plateau-design-system/structure/design-system/classes/plateau-design-system--class-custom-tokens.skill\|class-custom-tokens]] |
| /src/lib/{component-name} | One directory per component — generic pattern for every `ds-*` component | [[skills/angular/architecture/plateau/plateau-design-system/structure/design-system/classes/plateau-design-system--class-component-name.skill\|class-component-name]] |

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-design-system-tokens.skill/solution-design-system-tokens.skill.md|solution-design-system-tokens]] - [[skills/angular/architecture/solutions/solution-design-system-tokens.skill/Implementation/Repository.extend|Repository.extend]]
- [[skills/angular/architecture/solutions/solution-design-system-components.skill/solution-design-system-components.skill.md|solution-design-system-components]] - [[skills/angular/architecture/solutions/solution-design-system-components.skill/Implementation/Repository.extend|Repository.extend]]

## NPM Packages

| Package | Version constraint | Purpose |
| ------- | ------------------- | ------- |
| @angular/material | latest supporting M3 | `mat.theme()`, M3 system tokens, Sass override mixins; used internally by delegating components, never exposed in the public API |
| ng-packagr | — | Library build, Angular Package Format / Ivy partial compilation output |
| @changesets/cli | — | Version bump classification and CHANGELOG generation |

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-design-system-structure.skill/solution-design-system-structure.skill.md|solution-design-system-structure]]
- [[skills/angular/architecture/solutions/solution-design-system-tokens.skill/solution-design-system-tokens.skill.md|solution-design-system-tokens]]
- [[skills/angular/architecture/solutions/solution-design-system-components.skill/solution-design-system-components.skill.md|solution-design-system-components]]

## What Does NOT Belong Here

- Multi-brand/per-tenant theming — deliberately deferred, single fixed palette only for now
- Any Angular Material type, selector, or enum surfaced in a component's public API
- Business/domain logic — this library only ever contains presentation and theming primitives

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-design-system-tokens.skill/solution-design-system-tokens.skill.md|solution-design-system-tokens]]
- [[skills/angular/architecture/solutions/solution-design-system-components.skill/solution-design-system-components.skill.md|solution-design-system-components]]

# Rules

## MUST
- [[skills/angular/architecture/plateau/plateau-design-system/structure/plateau-design-system--repo-design-system.skill#MUST|repo-design-system]]

## SHOULD
- [[skills/angular/architecture/plateau/plateau-design-system/structure/plateau-design-system--repo-design-system.skill#SHOULD|repo-design-system]]

## MUST NOT
- [[skills/angular/architecture/plateau/plateau-design-system/structure/plateau-design-system--repo-design-system.skill#MUST NOT|repo-design-system]]

# Check list

- [ ] The library builds via ng-packagr, producing Angular Package Format-compliant output
- [ ] No `--ds-*` token duplicates an existing `--mat-sys-*` concept
- [ ] No component's public API exposes any Angular Material type, selector, or enum
- [ ] Every form-participating component implements `ControlValueAccessor`

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-design-system-structure.skill/solution-design-system-structure.skill.md|solution-design-system-structure]]
- [[skills/angular/architecture/solutions/solution-design-system-tokens.skill/solution-design-system-tokens.skill.md|solution-design-system-tokens]]
- [[skills/angular/architecture/solutions/solution-design-system-components.skill/solution-design-system-components.skill.md|solution-design-system-components]]
