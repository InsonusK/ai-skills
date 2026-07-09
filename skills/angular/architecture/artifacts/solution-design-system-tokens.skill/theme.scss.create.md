---
description: The single mat.theme() definition applied at the design system's root selector
project_name: design-system
name: theme
artifact_type: service
change_kind: create
---

# Goals

- Define the one fixed brand palette (per [[../../adr/brand-theming-scope.md]]) as a single M3 theme, with light/dark handled via `light-dark()`

# Implementation changes

```code example
// theme.scss
@use '@angular/material' as mat;

html {
  color-scheme: light dark; // follow OS preference, per light-dark-mode-strategy ADR
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

# Rule changes

## MUST
- The theme MUST be applied at the root selector (`html`), per Angular Material's own guidance, so every consumer (the platform monorepo and every embeddable app importing this package) inherits it consistently.
- `color-scheme` MUST be set to `light dark` (following OS preference) unless a future solution introduces an explicit user-facing toggle.
- Only one palette MUST be defined — no second/alternate palette or theme-swapping mechanism, per [[../../adr/brand-theming-scope.md]].

# Anti-patterns

- **Applying the theme at a component-level selector instead of the root**
  - Consequence: tokens don't cascade consistently to every part of the consuming application, and components outside that selector's scope silently fall back to Material's un-themed defaults
  - Instead: always apply at `html` (or the highest-level selector available to the consumer)

# Check list

- [ ] The theme is applied exactly once, at the root selector
- [ ] `color-scheme` is set to follow OS preference by default
- [ ] Only a single palette is defined

# Unittest TestCases

- [ ] WHEN the consuming application's OS is set to dark mode THEN
  - [ ] every Material and custom component reflects the dark variant automatically, without any JavaScript
