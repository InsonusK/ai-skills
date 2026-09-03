---
name: plateau-multi-tenant-design-system--class-tenants
description: The DS_TENANTS tuple and DsTenant union in projects/design-system/src/lib/tenants.ts — the design system's typed contract for which tenant ids exist, exported from public-api (VP1) — multi-tenant-design-system plateau
domain: skill
type: template
whenToUse: when editing src/lib/tenants.ts (adding a tenant id), or wiring the DsTenant contract into public-api (VP1)
plateau: multi-tenant-design-system
artifact_type: module
version: 20260903200000
tags:
  - skill/template/class
  - plateau/multi-tenant-design-system
  - stack/typescript
  - framework/angular
  - concern/architecture

created_by:
  - "[[skills/angular/architecture/v3.1/solutions/solution-design-system-multi-tenant-theming.skill/solution-design-system-multi-tenant-theming.skill.md|solution-design-system-multi-tenant-theming]]"
---

> `projects/design-system/src/lib/tenants.ts`, re-exported from `src/public-api.ts`. The only TypeScript this solution adds.

# Goal

- Give consumers a compile-time list of valid tenant ids so `data-tenant` is never a free string
- Keep the list in one place, next to the SCSS tenant files it mirrors

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-design-system-multi-tenant-theming.skill/solution-design-system-multi-tenant-theming.skill.md|solution-design-system-multi-tenant-theming]] - [[skills/angular/architecture/v3.1/solutions/solution-design-system-multi-tenant-theming.skill/Implementation/Tenants/tenants.ts.create.md|Tenants/tenants.ts.create]]

# Core Principles

- Apply ONE plateau template per artifact
- `DS_TENANTS` is `as const`; `DsTenant` is derived (`(typeof DS_TENANTS)[number]`) — never a hand-written parallel union
- The module exports the tuple and the type only — no `applyTenant` helper, no DOM code

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-design-system-multi-tenant-theming.skill/solution-design-system-multi-tenant-theming.skill.md|solution-design-system-multi-tenant-theming]] - [[skills/angular/architecture/v3.1/solutions/solution-design-system-multi-tenant-theming.skill/adr/tenant-resolution-strategy.md|tenant-resolution-strategy]]

# Naming convention

| use case | class name pattern | class name | file name pattern | file name |
| -------- | ------------------- | ---------- | -------------------- | --------- |
| Tenant id tuple | `DS_TENANTS` | `DS_TENANTS` | `tenants.ts` | `tenants.ts` |
| Tenant id union | `DsTenant` | `DsTenant` | `tenants.ts` | `tenants.ts` |

# Implementation

```typescript
// Skill: class-tenants
// Plateau: multi-tenant-design-system
// Version: 20260903200000

/**
 * Every tenant id the design system ships a palette for. Each entry has a matching
 * `styles/tenants/_<id>.scss` file and a `:root[data-tenant='<id>']` rule.
 * With no `data-tenant` attribute set, the base brand palette applies.
 */
export const DS_TENANTS = ['acme', 'globex'] as const;

export type DsTenant = (typeof DS_TENANTS)[number];
```

`src/public-api.ts` gains `export { DS_TENANTS } from './lib/tenants';` and `export type { DsTenant } from './lib/tenants';`.

A consuming app: `document.documentElement.dataset.tenant = tenant;` where `tenant: DsTenant` — ideally server-rendered into `index.html` to avoid a flash.

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-design-system-multi-tenant-theming.skill/solution-design-system-multi-tenant-theming.skill.md|solution-design-system-multi-tenant-theming]] - [[skills/angular/architecture/v3.1/solutions/solution-design-system-multi-tenant-theming.skill/Implementation/Tenants/tenants.ts.create.md|Tenants/tenants.ts.create]]

# Rules

## MUST
- `DS_TENANTS` is `as const`; `DsTenant = (typeof DS_TENANTS)[number]` — never a parallel string-literal union.
- Every `DS_TENANTS` entry has a matching `_<id>.scss` file and a `@use` line in `tenants.scss`.
- The file exports the tuple and the type only — no `applyTenant` helper, no DOM access.
- Never apply several plateau templates per artifact.

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-design-system-multi-tenant-theming.skill/solution-design-system-multi-tenant-theming.skill.md|solution-design-system-multi-tenant-theming]] - [[skills/angular/architecture/v3.1/solutions/solution-design-system-multi-tenant-theming.skill/Implementation/Tenants/tenants.ts.create.md|Tenants/tenants.ts.create]]

# Check list

- [ ] `DS_TENANTS` is `as const`; `DsTenant = (typeof DS_TENANTS)[number]`
- [ ] `public-api.ts` re-exports both
- [ ] The module contains no DOM access
- [ ] Every entry has a corresponding tenant SCSS file

# Unittest TestCases

- [ ] WHEN `tenants.ts` is imported THEN `DS_TENANTS` equals `['acme', 'globex']` and is `readonly`
- [ ] WHEN a value not in `DS_TENANTS` is assigned to a `DsTenant` variable THEN the compiler rejects it

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-design-system-multi-tenant-theming.skill/solution-design-system-multi-tenant-theming.skill.md|solution-design-system-multi-tenant-theming]] - [[skills/angular/architecture/v3.1/solutions/solution-design-system-multi-tenant-theming.skill/Implementation/Tenants/tenants.ts.create.md|Tenants/tenants.ts.create]]
