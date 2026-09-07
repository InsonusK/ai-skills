---
name: plateau-design-system--project-design-system
description: The publishable design-system component library — one mat.theme(), a --ds-* token layer, and signal-based ds-* components that fully encapsulate Angular Material, built with ng-packagr to Angular Package Format — design-system plateau
domain: skill
type: template
whenToUse: when adding or editing anything under projects/design-system — a ds-* component, theme.scss / custom-tokens.scss, public-api.ts, ng-package.json, or the library's peer deps
plateau: design-system
project_kind: library
version: 20260903170000
tags:
  - skill/template/project
  - plateau/design-system
  - stack/typescript
  - framework/angular
  - concern/architecture

created_by:
  - "[[skills/angular/architecture/v3.1/solutions/solution-design-system-structure.skill/solution-design-system-structure.skill.md|solution-design-system-structure]]"
  - "[[skills/angular/architecture/v3.1/solutions/solution-design-system-tokens.skill/solution-design-system-tokens.skill.md|solution-design-system-tokens]]"
  - "[[skills/angular/architecture/v3.1/solutions/solution-design-system-components.skill/solution-design-system-components.skill.md|solution-design-system-components]]"
  - "[[skills/angular/architecture/v3.1/solutions/solution-design-system-ui-testing.skill/solution-design-system-ui-testing.skill.md|solution-design-system-ui-testing]]"
---

> No solution produces a dedicated `design-system.project.create` file — this project is established by [[skills/angular/architecture/v3.1/design-system/plateau/plateau-design-system/structure/plateau-design-system--repo-design-system.skill.md|repo-design-system]]'s `Repository.create`/`.extend` entries. This skill gives its classes a consistent home, per the plateau's repo → project → class convention.

# Goal

- Provide one independently versioned npm package that every consumer (platform monorepo, embeddable apps) uses for theming (`mat.theme()` + `--ds-*` tokens) and UI components — with zero required Angular Material knowledge at the call site

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-design-system-structure.skill/solution-design-system-structure.skill.md|solution-design-system-structure]] - [[skills/angular/architecture/v3.1/solutions/solution-design-system-structure.skill/Implementation/Repository.create.md|Repository.create]]

# Core Principles

- ng-packagr output (Angular Package Format, Ivy partial compilation) — not locked to one consumer Angular version
- `--mat-sys-*` tokens consumed directly wherever Material models the concept; `--ds-*` only for genuine gaps
- Every component: own `ds-*` selector, `input()`/`output()`/`model()` only, and full encapsulation of whether it delegates to Material or is custom-built
- The public surface (`public-api.ts` **and** the built `types/*.d.ts`) never contains an Angular Material selector, input, or type

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-design-system-tokens.skill/solution-design-system-tokens.skill.md|solution-design-system-tokens]] - [[skills/angular/architecture/v3.1/solutions/solution-design-system-tokens.skill/Implementation/Repository.extend.md|Repository.extend]]
- [[skills/angular/architecture/v3.1/solutions/solution-design-system-components.skill/solution-design-system-components.skill.md|solution-design-system-components]] - [[skills/angular/architecture/v3.1/solutions/solution-design-system-components.skill/Implementation/Repository.extend.md|Repository.extend]]

# Structure

## Project Structure

```
/projects/design-system
  ng-package.json                       <- dest dist/design-system; assets: styles/*.scss
  package.json                          <- name; peerDependencies incl. @angular/material + @angular/cdk
  /styles
    [theme.scss](./classes/plateau-design-system--class-theme.skill.md)            <- the single mat.theme(), applied at html
    [custom-tokens.scss](./classes/plateau-design-system--class-custom-tokens.skill.md)     <- --ds-* tokens for Material's gaps
  /src
    public-api.ts                       <- exports ds-* components + their own types only
    test-setup.ts                       <- @testing-library/jest-dom/vitest
    /lib
      /{component-name}
        [ds-{component-name}.component.ts](./classes/plateau-design-system--class-component-name.skill.md)
        /spec
          [{component-name}.component.spec.ts](./classes/plateau-design-system--class-component-name-component-spec.skill.md)
          [{component-name}.visual.spec.ts](./classes/plateau-design-system--class-component-name-visual-spec.skill.md)
          [{component-name}.style-snapshot.spec.ts](./classes/plateau-design-system--class-component-name-style-snapshot-spec.skill.md)
          [{component-name}.a11y.spec.ts](./classes/plateau-design-system--class-component-name-a11y-spec.skill.md)
          /preview
            {component-name}.preview.ts  <- imported by projects/demo (imports the PUBLISHED package)
          /snapshot                      <- committed .png + .styles.txt baselines
  /testing
    [read-visual-style-properties.ts](./classes/plateau-design-system--class-read-visual-style-properties.skill.md)   <- one shared curated property list
```

## Directory and class skills

| `Directory\|file` | Description | Pattern skill |
| ------------------ | ----------- | -------------- |
| styles/theme.scss | Single `mat.theme()`, one fixed brand palette, `color-scheme: light dark`. | [[skills/angular/architecture/v3.1/design-system/plateau/plateau-design-system/structure/design-system/classes/plateau-design-system--class-theme.skill\|class-theme]] |
| styles/custom-tokens.scss | `--ds-*` custom properties for concepts Material's token set doesn't cover. | [[skills/angular/architecture/v3.1/design-system/plateau/plateau-design-system/structure/design-system/classes/plateau-design-system--class-custom-tokens.skill\|class-custom-tokens]] |
| src/lib/{component-name}/ | One directory per component — the generic `ds-*` authoring pattern. | [[skills/angular/architecture/v3.1/design-system/plateau/plateau-design-system/structure/design-system/classes/plateau-design-system--class-component-name.skill\|class-component-name]] |
| src/lib/{component-name}/spec/ | The four test layers + `preview/` + committed `snapshot/` baselines. | [[skills/angular/architecture/v3.1/design-system/plateau/plateau-design-system/structure/design-system/classes/plateau-design-system--class-component-name-component-spec.skill\|component-spec]] · [[skills/angular/architecture/v3.1/design-system/plateau/plateau-design-system/structure/design-system/classes/plateau-design-system--class-component-name-visual-spec.skill\|visual-spec]] · [[skills/angular/architecture/v3.1/design-system/plateau/plateau-design-system/structure/design-system/classes/plateau-design-system--class-component-name-style-snapshot-spec.skill\|style-snapshot-spec]] · [[skills/angular/architecture/v3.1/design-system/plateau/plateau-design-system/structure/design-system/classes/plateau-design-system--class-component-name-a11y-spec.skill\|a11y-spec]] |
| testing/read-visual-style-properties.ts | The one shared `VISUAL_STYLE_PROPERTIES` list every style-snapshot spec imports. | [[skills/angular/architecture/v3.1/design-system/plateau/plateau-design-system/structure/design-system/classes/plateau-design-system--class-read-visual-style-properties.skill\|read-visual-style-properties]] |
| src/public-api.ts | The public barrel — `ds-*` component classes + their own literal types only. | — |

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-design-system-tokens.skill/solution-design-system-tokens.skill.md|solution-design-system-tokens]] - [[skills/angular/architecture/v3.1/solutions/solution-design-system-tokens.skill/Implementation/Repository.extend.md|Repository.extend]]
- [[skills/angular/architecture/v3.1/solutions/solution-design-system-components.skill/solution-design-system-components.skill.md|solution-design-system-components]] - [[skills/angular/architecture/v3.1/solutions/solution-design-system-components.skill/Implementation/Repository.extend.md|Repository.extend]]
- [[skills/angular/architecture/v3.1/solutions/solution-design-system-ui-testing.skill/solution-design-system-ui-testing.skill.md|solution-design-system-ui-testing]] - [[skills/angular/architecture/v3.1/solutions/solution-design-system-ui-testing.skill/Implementation/demo.project.extend.md|demo.project.extend]]

## NPM Packages

| Package | Version constraint | Purpose |
| ------- | ------------------- | ------- |
| @angular/material, @angular/cdk | latest supporting M3 | `mat.theme()`, M3 tokens, Sass override mixins, `matButton` etc. — internal only. `peerDependencies`. |
| ng-packagr | 22.x | Library build (Angular Package Format) |
| @changesets/cli | latest | Version bump classification + CHANGELOG |

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-design-system-structure.skill/solution-design-system-structure.skill.md|solution-design-system-structure]] - [[skills/angular/architecture/v3.1/solutions/solution-design-system-structure.skill/Implementation/Repository.create.md|Repository.create]]

## What Does NOT Belong Here

- Multi-brand / per-tenant theming — deliberately deferred; one fixed palette only (VP1 `MultiTenantTheming`, aspirational)
- Any Angular Material type, selector, or enum on a component's public API — or in the built `types/*.d.ts`
- Business / domain logic — this library holds presentation and theming primitives only
- Preview components authored inside `projects/demo` — they live in each component's `spec/preview/`

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-design-system-tokens.skill/solution-design-system-tokens.skill.md|solution-design-system-tokens]] - [[skills/angular/architecture/v3.1/solutions/solution-design-system-tokens.skill/Implementation/Repository.extend.md|Repository.extend]]
- [[skills/angular/architecture/v3.1/solutions/solution-design-system-components.skill/solution-design-system-components.skill.md|solution-design-system-components]] - [[skills/angular/architecture/v3.1/solutions/solution-design-system-components.skill/Implementation/Repository.extend.md|Repository.extend]]

# Rules

## MUST
- [[skills/angular/architecture/v3.1/design-system/plateau/plateau-design-system/structure/plateau-design-system--repo-design-system.skill#must|repo-design-system]]

## SHOULD
- [[skills/angular/architecture/v3.1/design-system/plateau/plateau-design-system/structure/plateau-design-system--repo-design-system.skill#should|repo-design-system]]

# Check list

- [ ] The library builds via ng-packagr, producing Angular Package Format output
- [ ] `dist/design-system/types/*.d.ts` contains no `@angular/material` import
- [ ] No `--ds-*` token duplicates an existing `--mat-sys-*` concept
- [ ] `public-api.ts` exports only `ds-*` component classes and their own literal types
- [ ] Every form-participating component implements `ControlValueAccessor`
- [ ] Every `src/lib/{component}/` has a `spec/` folder with all four spec layers

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-design-system-structure.skill/solution-design-system-structure.skill.md|solution-design-system-structure]] - [[skills/angular/architecture/v3.1/solutions/solution-design-system-structure.skill/Implementation/Repository.create.md|Repository.create]]
- [[skills/angular/architecture/v3.1/solutions/solution-design-system-tokens.skill/solution-design-system-tokens.skill.md|solution-design-system-tokens]] - [[skills/angular/architecture/v3.1/solutions/solution-design-system-tokens.skill/Implementation/Repository.extend.md|Repository.extend]]
- [[skills/angular/architecture/v3.1/solutions/solution-design-system-components.skill/solution-design-system-components.skill.md|solution-design-system-components]] - [[skills/angular/architecture/v3.1/solutions/solution-design-system-components.skill/Implementation/Repository.extend.md|Repository.extend]]
