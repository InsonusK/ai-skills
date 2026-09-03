---
name: plateau-design-system--class-theme
description: The single mat.theme() definition applied at the design system's root selector — one fixed brand palette, light/dark via native light-dark() — design-system plateau
domain: skill
type: template
whenToUse: when creating or editing projects/design-system/styles/theme.scss, or reviewing how the brand palette and light/dark are configured
plateau: design-system
artifact_type: stylesheet
version: 20260903170000
tags:
  - skill/template/class
  - plateau/design-system
  - stack/typescript
  - framework/angular
  - concern/architecture

created_by:
  - "[[skills/angular/architecture/v3.1/solutions/solution-design-system-tokens.skill/solution-design-system-tokens.skill.md|solution-design-system-tokens]]"
---

> `projects/design-system/styles/theme.scss` — shipped as a package asset (`ng-package.json` `assets`), consumed as `@use 'design-system/styles/theme'`.

# Goal

- Define the one fixed brand palette as a single M3 theme, with light/dark handled by `light-dark()` — no JavaScript toggle

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-design-system-tokens.skill/solution-design-system-tokens.skill.md|solution-design-system-tokens]] - [[skills/angular/architecture/v3.1/solutions/solution-design-system-tokens.skill/Implementation/Tokens/theme.scss.create.md|Tokens/theme.scss.create]]

# Core Principles

- Apply ONE plateau template per artifact
- This file is the only place the brand palette is declared; components consume `--mat-sys-*` tokens directly

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-design-system-tokens.skill/solution-design-system-tokens.skill.md|solution-design-system-tokens]] - [[skills/angular/architecture/v3.1/solutions/solution-design-system-tokens.skill/Implementation/Tokens/theme.scss.create.md|Tokens/theme.scss.create]]

# Naming convention

| use case | file name |
| -------- | --------- |
| Root theme stylesheet | `theme.scss` |

# Implementation

```scss
// Skill: class-theme
// Plateau: design-system
// Version: 20260903170000
@use '@angular/material' as mat;

html {
  color-scheme: light dark; // follow OS preference

  @include mat.theme(
    (
      color: (
        theme-type: color-scheme,
        primary: mat.$violet-palette, // this application's one fixed brand palette
      ),
      typography: Roboto,
      density: 0,
    )
  );
}
```

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-design-system-tokens.skill/solution-design-system-tokens.skill.md|solution-design-system-tokens]] - [[skills/angular/architecture/v3.1/solutions/solution-design-system-tokens.skill/Implementation/Tokens/theme.scss.create.md|Tokens/theme.scss.create]]

# Rules

## MUST
- The theme is applied at the root selector (`html`), so every consumer inherits it consistently.
- `color-scheme` is `light dark` (OS preference) unless a future solution adds an explicit user-facing toggle.
- Only one palette is defined — no second/alternate palette or theme-swapping mechanism.

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-design-system-tokens.skill/solution-design-system-tokens.skill.md|solution-design-system-tokens]] - [[skills/angular/architecture/v3.1/solutions/solution-design-system-tokens.skill/Implementation/Tokens/theme.scss.create.md|Tokens/theme.scss.create]]


- **Applying the theme at a component-level selector instead of the root**
  - Consequence: tokens don't cascade consistently; components outside that selector fall back to Material's un-themed defaults
  - Instead: apply at `html` (or the highest selector available to the consumer)

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-design-system-tokens.skill/solution-design-system-tokens.skill.md|solution-design-system-tokens]] - [[skills/angular/architecture/v3.1/solutions/solution-design-system-tokens.skill/Implementation/Tokens/theme.scss.create.md|Tokens/theme.scss.create]]

# Check list

- [ ] The theme is applied exactly once, at the root selector
- [ ] `color-scheme` follows OS preference by default
- [ ] Only a single palette is defined

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-design-system-tokens.skill/solution-design-system-tokens.skill.md|solution-design-system-tokens]] - [[skills/angular/architecture/v3.1/solutions/solution-design-system-tokens.skill/Implementation/Tokens/theme.scss.create.md|Tokens/theme.scss.create]]

# Unittest TestCases

- [ ] WHEN the consuming application's OS is set to dark mode THEN every Material and custom component reflects the dark variant automatically, without any JavaScript

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-design-system-tokens.skill/solution-design-system-tokens.skill.md|solution-design-system-tokens]] - [[skills/angular/architecture/v3.1/solutions/solution-design-system-tokens.skill/Implementation/Tokens/theme.scss.create.md|Tokens/theme.scss.create]]
