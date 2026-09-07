---
description: The DS_TENANTS tuple and DsTenant union — the design system's typed contract for which tenant ids exist, exported from public-api so a consuming app sets data-tenant against a known set
project_name: design-system
name: tenants
element_kind: module
change_kind: create
tags:
  - solution/design-system-multi-tenant-theming
  - element/tenants-ts
---

# How this generic file is used

Create once at `projects/design-system/src/lib/tenants.ts`; re-export from `src/public-api.ts`. It is the only TypeScript this solution adds.

# Goals

- Give consumers a compile-time list of valid tenant ids so `data-tenant` is never a free string
- Keep the list in one place, next to the SCSS tenant files it mirrors

# Naming convention

| use case | class name pattern | class name | file name pattern | file name |
| -------- | ------------------- | ---------- | -------------------- | --------- |
| Tenant id tuple | `DS_TENANTS` | `DS_TENANTS` | `tenants.ts` | `tenants.ts` |
| Tenant id union | `DsTenant` | `DsTenant` | `tenants.ts` | `tenants.ts` |

# Implementation changes

File: `projects/design-system/src/lib/tenants.ts`

```typescript
// Skill: class-tenants
// Plateau: multi-tenant-design-system
// Version: <UTC timestamp at generation>

/**
 * Every tenant id the design system ships a palette for. Each entry has a matching
 * `styles/tenants/_<id>.scss` file and a `:root[data-tenant='<id>']` rule.
 * With no `data-tenant` attribute set, the base brand palette applies.
 */
export const DS_TENANTS = ['acme', 'globex'] as const;

export type DsTenant = (typeof DS_TENANTS)[number];
```

`src/public-api.ts` gains:

```typescript
export { DS_TENANTS } from './lib/tenants';
export type { DsTenant } from './lib/tenants';
```

A consuming application then:

```typescript
import type { DsTenant } from 'design-system';

function applyTenant(tenant: DsTenant): void {
  document.documentElement.dataset.tenant = tenant; // set as early as possible — ideally server-rendered
}
```

# Rule changes

## MUST
- `DS_TENANTS` is `as const` and `DsTenant` is derived from it — never a hand-written string-literal union kept in parallel.
  - Risk: two lists drift; a tenant added to one but not the other is either untyped or unshippable.
  - Fix: one `as const` tuple; the union is `(typeof DS_TENANTS)[number]`.
- Every `DS_TENANTS` entry has a matching `_<id>.scss` file and a `@use` line in `tenants.scss`.
  - Risk: an id in the union with no stylesheet type-checks but does nothing at runtime.
  - Fix: adding a tenant touches `tenants.ts`, `_<id>.scss`, and `tenants.scss` together.
- The file exports the tuple and the type only — no `applyTenant` helper, no DOM code.
  - Risk: a helper in the library makes the design system responsible for the DOM write, which is the consuming app's job.
  - Fix: the library states the contract (the id set); the consumer sets the attribute.

# Check list

- [ ] `DS_TENANTS` is `as const`; `DsTenant = (typeof DS_TENANTS)[number]`
- [ ] `public-api.ts` re-exports both
- [ ] The module contains no DOM access
- [ ] Every entry has a corresponding tenant SCSS file

# Unittest TestCases

- [ ] WHEN `tenants.ts` is imported THEN
  - [ ] `DS_TENANTS` equals `['acme', 'globex']` and is frozen at the type level (`readonly`)
- [ ] WHEN a value not in `DS_TENANTS` is assigned to a `DsTenant` variable THEN
  - [ ] the TypeScript compiler rejects it
