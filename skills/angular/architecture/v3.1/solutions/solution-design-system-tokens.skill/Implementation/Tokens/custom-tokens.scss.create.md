---
description: Custom --ds-* tokens covering concepts Material's own M3 token set does not model — semantic status/priority colors, spacing, radius
project_name: design-system
name: custom-tokens
element_kind: stylesheet
change_kind: create
tags:
  - solution/design-system-tokens
  - element/custom-tokens-scss
---

# Goals

- Give components a consistent, themeable set of tokens for domain-specific concepts (priority, workflow state, spacing, radius) that Material's own token set has no equivalent for

# Naming convention

| use case | token pattern | example |
| -------- | -------------- | ------- |
| Semantic status/priority color | --ds-color-{category}-{value} | --ds-color-priority-high, --ds-color-status-in-progress |
| Spacing scale | --ds-spacing-{size} | --ds-spacing-sm, --ds-spacing-md |
| Radius scale | --ds-radius-{size} | --ds-radius-sm, --ds-radius-lg |

# Implementation changes

```scss
// custom-tokens.scss
html {
  // Semantic status/priority colors — no Material equivalent
  --ds-color-priority-high: light-dark(#c62828, #ef9a9a);
  --ds-color-priority-medium: light-dark(#ef6c00, #ffcc80);
  --ds-color-priority-low: light-dark(#2e7d32, #a5d6a7);

  --ds-color-status-stop: light-dark(#c62828, #ef9a9a);
  --ds-color-status-start: light-dark(#2e7d32, #a5d6a7);
  --ds-color-status-in-progress: light-dark(#1565c0, #90caf9);

  // Spacing scale — no Material equivalent
  --ds-spacing-xs: 4px;
  --ds-spacing-sm: 8px;
  --ds-spacing-md: 16px;
  --ds-spacing-lg: 24px;

  // Radius scale — no Material equivalent
  --ds-radius-sm: 4px;
  --ds-radius-md: 8px;
  --ds-radius-lg: 16px;
}
```

# Rule changes

## MUST
- Every color token here must use `light-dark()`, consistent with how Material's own `--mat-sys-*` tokens are defined.
- A new `--ds-*` token must only be added when no `--mat-sys-*` equivalent exists — before adding one, check Material's own token set first.

## SHOULD
- **Adding a new `--ds-*` color token for a concept Material's palette already covers (e.g. a generic "error" or "warning" color)** — Consequence: creates a redundant, inconsistent second way to express something `--mat-sys-error`/`--mat-sys-on-error-container` already expresses — Instead: use the existing Material system token; reserve `--ds-*` for genuinely domain-specific concepts (priority, workflow state) with no Material equivalent

# Check list

- [ ] No `--ds-*` token duplicates an existing `--mat-sys-*` concept
- [ ] Every `--ds-*` color token uses `light-dark()`

# Unittest TestCases

- [ ] WHEN a component references `--ds-color-priority-high` in dark mode THEN
  - [ ] it resolves to the dark-mode value, consistent with how `--mat-sys-*` tokens behave
