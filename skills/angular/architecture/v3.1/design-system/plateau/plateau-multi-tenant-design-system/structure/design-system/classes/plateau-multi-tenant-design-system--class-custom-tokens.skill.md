---
name: plateau-multi-tenant-design-system--class-custom-tokens
description: Custom --ds-* tokens covering concepts Material's own M3 token set does not model — semantic status/priority colours, spacing, radius — each with light-dark() — multi-tenant-design-system plateau
domain: skill
type: template
whenToUse: when adding or editing a --ds-* token in projects/design-system/styles/custom-tokens.scss, or checking a token isn't a redundant alias of a --mat-sys-* concept
plateau: multi-tenant-design-system
artifact_type: stylesheet
version: 20260903200000
tags:
  - skill/template/class
  - plateau/multi-tenant-design-system
  - stack/typescript
  - framework/angular
  - concern/architecture

created_by:
  - "[[skills/angular/architecture/v3.1/solutions/solution-design-system-tokens.skill/solution-design-system-tokens.skill.md|solution-design-system-tokens]]"
---

> `projects/design-system/styles/custom-tokens.scss` — shipped as a package asset, consumed as `@use 'design-system/styles/custom-tokens'`.

# Goal

- Give components a consistent, themeable set of tokens for domain concepts (priority, workflow state, spacing, radius) Material's own token set has no equivalent for

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-design-system-tokens.skill/solution-design-system-tokens.skill.md|solution-design-system-tokens]] - [[skills/angular/architecture/v3.1/solutions/solution-design-system-tokens.skill/Implementation/Tokens/custom-tokens.scss.create.md|Tokens/custom-tokens.scss.create]]

# Core Principles

- Apply ONE plateau template per artifact
- Never re-alias a concept `--mat-sys-*` already models — this file is only for genuine gaps

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-design-system-tokens.skill/solution-design-system-tokens.skill.md|solution-design-system-tokens]] - [[skills/angular/architecture/v3.1/solutions/solution-design-system-tokens.skill/Implementation/Tokens/custom-tokens.scss.create.md|Tokens/custom-tokens.scss.create]]

# Naming convention

| use case | token pattern | example |
| -------- | -------------- | ------- |
| Semantic status/priority colour | `--ds-color-{category}-{value}` | `--ds-color-priority-high`, `--ds-color-status-in-progress` |
| Spacing scale | `--ds-spacing-{size}` | `--ds-spacing-sm` |
| Radius scale | `--ds-radius-{size}` | `--ds-radius-lg` |

# Implementation

```scss
// Skill: class-custom-tokens
// Plateau: design-system
// Version: 20260903200000
html {
  // Semantic status / priority colours — no Material equivalent
  --ds-color-priority-high: light-dark(#c62828, #ef9a9a);
  --ds-color-priority-medium: light-dark(#ef6c00, #ffcc80);
  --ds-color-priority-low: light-dark(#2e7d32, #a5d6a7);

  --ds-color-status-stop: light-dark(#c62828, #ef9a9a);
  --ds-color-status-start: light-dark(#2e7d32, #a5d6a7);
  --ds-color-status-in-progress: light-dark(#1565c0, #90caf9);

  // Spacing scale
  --ds-spacing-xs: 4px;
  --ds-spacing-sm: 8px;
  --ds-spacing-md: 16px;
  --ds-spacing-lg: 24px;

  // Radius scale
  --ds-radius-sm: 4px;
  --ds-radius-md: 8px;
  --ds-radius-lg: 16px;
}
```

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-design-system-tokens.skill/solution-design-system-tokens.skill.md|solution-design-system-tokens]] - [[skills/angular/architecture/v3.1/solutions/solution-design-system-tokens.skill/Implementation/Tokens/custom-tokens.scss.create.md|Tokens/custom-tokens.scss.create]]

# Rules

## MUST
- Every colour token here uses `light-dark()`, consistent with `--mat-sys-*`.
- A new `--ds-*` token is added only when no `--mat-sys-*` equivalent exists — check Material's token set first.

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-design-system-tokens.skill/solution-design-system-tokens.skill.md|solution-design-system-tokens]] - [[skills/angular/architecture/v3.1/solutions/solution-design-system-tokens.skill/Implementation/Tokens/custom-tokens.scss.create.md|Tokens/custom-tokens.scss.create]]


- **Adding a `--ds-*` colour token for a concept Material's palette already covers (a generic "error"/"warning")**
  - Consequence: a redundant, inconsistent second way to express something `--mat-sys-error` already expresses
  - Instead: use the Material system token; reserve `--ds-*` for genuinely domain-specific concepts

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-design-system-tokens.skill/solution-design-system-tokens.skill.md|solution-design-system-tokens]] - [[skills/angular/architecture/v3.1/solutions/solution-design-system-tokens.skill/Implementation/Tokens/custom-tokens.scss.create.md|Tokens/custom-tokens.scss.create]]

# Check list

- [ ] No `--ds-*` token duplicates an existing `--mat-sys-*` concept
- [ ] Every `--ds-*` colour token uses `light-dark()`

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-design-system-tokens.skill/solution-design-system-tokens.skill.md|solution-design-system-tokens]] - [[skills/angular/architecture/v3.1/solutions/solution-design-system-tokens.skill/Implementation/Tokens/custom-tokens.scss.create.md|Tokens/custom-tokens.scss.create]]

# Unittest TestCases

- [ ] WHEN a component references `--ds-color-priority-high` in dark mode THEN it resolves to the dark-mode value, consistent with `--mat-sys-*`

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-design-system-tokens.skill/solution-design-system-tokens.skill.md|solution-design-system-tokens]] - [[skills/angular/architecture/v3.1/solutions/solution-design-system-tokens.skill/Implementation/Tokens/custom-tokens.scss.create.md|Tokens/custom-tokens.scss.create]]
