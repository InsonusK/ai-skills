---
name: plateau-design-system--class-custom-tokens
description: Custom --ds-* tokens covering concepts Material's own M3 token set does not model — semantic status/priority colors, spacing, radius — design-system plateau
domain: skill
type: template
plateau: design-system
artifact_type: stylesheet
version: 20260711120000
tags:
  - skill/template/class
  - plateau/design-system
  - stack/typescript
  - concern/architecture

created_by:
  - "[[skills/angular/architecture/solutions/solution-design-system-tokens.skill/solution-design-system-tokens.skill.md|solution-design-system-tokens]]"
---

# Goal

- Give components a consistent, themeable set of tokens for domain-specific concepts (priority, workflow state, spacing, radius) that Material's own token set has no equivalent for

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-design-system-tokens.skill/solution-design-system-tokens.skill.md|solution-design-system-tokens]] - [[skills/angular/architecture/solutions/solution-design-system-tokens.skill/Implementation/Tokens/custom-tokens.scss.create|Tokens/custom-tokens.scss.create]]

# Core Principles

- Apply ONE plateau template per class/artifact
- Never re-alias a concept `--mat-sys-*` already models — this file exists only for genuine gaps

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-design-system-tokens.skill/solution-design-system-tokens.skill.md|solution-design-system-tokens]] - [[skills/angular/architecture/solutions/solution-design-system-tokens.skill/Implementation/Tokens/custom-tokens.scss.create|Tokens/custom-tokens.scss.create]]

# Naming convention

| use case | token pattern | example | file name |
| -------- | -------------- | ------- | --------- |
| Semantic status/priority color | `--ds-color-{category}-{value}` | `--ds-color-priority-high`, `--ds-color-status-in-progress` | custom-tokens.scss |
| Spacing scale | `--ds-spacing-{size}` | `--ds-spacing-sm`, `--ds-spacing-md` | custom-tokens.scss |
| Radius scale | `--ds-radius-{size}` | `--ds-radius-sm`, `--ds-radius-lg` | custom-tokens.scss |

# Implementation

```scss
// Skill: class-custom-tokens
// Plateau: design-system
// Version: 20260711120000

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

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-design-system-tokens.skill/solution-design-system-tokens.skill.md|solution-design-system-tokens]] - [[skills/angular/architecture/solutions/solution-design-system-tokens.skill/Implementation/Tokens/custom-tokens.scss.create|Tokens/custom-tokens.scss.create]]

# Rules

## MUST
- Every color token here MUST use `light-dark()`, consistent with how Material's own `--mat-sys-*` tokens are defined.
- A new `--ds-*` token MUST only be added when no `--mat-sys-*` equivalent exists — check Material's own token set first.

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-design-system-tokens.skill/solution-design-system-tokens.skill.md|solution-design-system-tokens]] - [[skills/angular/architecture/solutions/solution-design-system-tokens.skill/Implementation/Tokens/custom-tokens.scss.create|Tokens/custom-tokens.scss.create]]

# Anti-patterns

- **Adding a new `--ds-*` color token for a concept Material's palette already covers (e.g. a generic "error"/"warning" color)**
  - Consequence: creates a redundant, inconsistent second way to express something `--mat-sys-error`/`--mat-sys-on-error-container` already expresses
  - Instead: use the existing Material system token; reserve `--ds-*` for genuinely domain-specific concepts with no Material equivalent

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-design-system-tokens.skill/solution-design-system-tokens.skill.md|solution-design-system-tokens]] - [[skills/angular/architecture/solutions/solution-design-system-tokens.skill/Implementation/Tokens/custom-tokens.scss.create|Tokens/custom-tokens.scss.create]]

# Check list

- [ ] No `--ds-*` token duplicates an existing `--mat-sys-*` concept
- [ ] Every `--ds-*` color token uses `light-dark()`

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-design-system-tokens.skill/solution-design-system-tokens.skill.md|solution-design-system-tokens]] - [[skills/angular/architecture/solutions/solution-design-system-tokens.skill/Implementation/Tokens/custom-tokens.scss.create|Tokens/custom-tokens.scss.create]]

# Unittest TestCases

- [ ] WHEN a component references `--ds-color-priority-high` in dark mode THEN
  - [ ] it resolves to the dark-mode value, consistent with how `--mat-sys-*` tokens behave

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-design-system-tokens.skill/solution-design-system-tokens.skill.md|solution-design-system-tokens]] - [[skills/angular/architecture/solutions/solution-design-system-tokens.skill/Implementation/Tokens/custom-tokens.scss.create|Tokens/custom-tokens.scss.create]]
