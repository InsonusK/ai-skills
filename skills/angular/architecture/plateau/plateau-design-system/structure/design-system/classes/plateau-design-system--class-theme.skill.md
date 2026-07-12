---
name: plateau-design-system--class-theme
description: The single mat.theme() definition applied at the design system's root selector — design-system plateau
domain: skill
type: template
plateau: design-system
artifact_type: stylesheet
version: 20260711120000
tags:
  - skill/template/class
  - plateau/design-system
created_by:
  - "[[skills/angular/architecture/solutions/solution-design-system-tokens.skill/solution-design-system-tokens.skill|solution-design-system-tokens]]"
---

# Goal

- Define the one fixed brand palette as a single M3 theme, with light/dark handled via `light-dark()`

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-design-system-tokens.skill/solution-design-system-tokens.skill|solution-design-system-tokens]] - [[skills/angular/architecture/solutions/solution-design-system-tokens.skill/Implementation/Tokens/theme.scss.create|Tokens/theme.scss.create]]

# Core Principles

- Apply ONE plateau template per class/artifact
- Consume `--mat-sys-*` tokens directly; this file is the only place the brand palette itself is declared

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-design-system-tokens.skill/solution-design-system-tokens.skill|solution-design-system-tokens]] - [[skills/angular/architecture/solutions/solution-design-system-tokens.skill/Implementation/Tokens/theme.scss.create|Tokens/theme.scss.create]]

# Naming convention

| use case | class name pattern | class name | file name pattern | file name |
| -------- | ------------------- | ---------- | -------------------- | --------- |
| Root theme stylesheet | — | — | theme.scss | theme.scss |

# Implementation

```scss
// Skill: class-theme
// Plateau: design-system
// Version: 20260711120000

@use '@angular/material' as mat;

html {
  color-scheme: light dark; // follow OS preference
  @include mat.theme((
    color: (
      theme-type: color-scheme,
      primary: mat.$violet-palette, // this application's one fixed brand palette
    ),
    typography: Roboto,
    density: 0,
  ));
}
```

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-design-system-tokens.skill/solution-design-system-tokens.skill|solution-design-system-tokens]] - [[skills/angular/architecture/solutions/solution-design-system-tokens.skill/Implementation/Tokens/theme.scss.create|Tokens/theme.scss.create]]

# Rules

## MUST
- The theme MUST be applied at the root selector (`html`), so every consumer (the platform monorepo and every embeddable app importing this package) inherits it consistently.
- `color-scheme` MUST be set to `light dark` (following OS preference) unless a future solution introduces an explicit user-facing toggle.
- Only one palette MUST be defined — no second/alternate palette or theme-swapping mechanism.

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-design-system-tokens.skill/solution-design-system-tokens.skill|solution-design-system-tokens]] - [[skills/angular/architecture/solutions/solution-design-system-tokens.skill/Implementation/Tokens/theme.scss.create|Tokens/theme.scss.create]]

# Anti-patterns

- **Applying the theme at a component-level selector instead of the root**
  - Consequence: tokens don't cascade consistently, and components outside that selector's scope silently fall back to Material's un-themed defaults
  - Instead: always apply at `html` (or the highest-level selector available to the consumer)

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-design-system-tokens.skill/solution-design-system-tokens.skill|solution-design-system-tokens]] - [[skills/angular/architecture/solutions/solution-design-system-tokens.skill/Implementation/Tokens/theme.scss.create|Tokens/theme.scss.create]]

# Check list

- [ ] The theme is applied exactly once, at the root selector
- [ ] `color-scheme` is set to follow OS preference by default
- [ ] Only a single palette is defined

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-design-system-tokens.skill/solution-design-system-tokens.skill|solution-design-system-tokens]] - [[skills/angular/architecture/solutions/solution-design-system-tokens.skill/Implementation/Tokens/theme.scss.create|Tokens/theme.scss.create]]

# Unittest TestCases

- [ ] WHEN the consuming application's OS is set to dark mode THEN
  - [ ] every Material and custom component reflects the dark variant automatically, without any JavaScript

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-design-system-tokens.skill/solution-design-system-tokens.skill|solution-design-system-tokens]] - [[skills/angular/architecture/solutions/solution-design-system-tokens.skill/Implementation/Tokens/theme.scss.create|Tokens/theme.scss.create]]
