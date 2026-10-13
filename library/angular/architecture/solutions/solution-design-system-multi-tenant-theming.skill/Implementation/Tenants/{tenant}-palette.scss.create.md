---
description: Generic pattern for one tenant's palette file — a :root[data-tenant='<id>'] block that @includes the ds-tenant-theme mixin with the tenant's Material palette and any tenant-specific --ds-* colour overrides
project_name: design-system
name: "{tenant}-palette"
element_kind: style
change_kind: create
tags:
  - solution/design-system-multi-tenant-theming
  - element/tenant-palette-scss
---

# How this generic file is used

Not tied to one tenant. Every tenant is one file at `projects/design-system/styles/tenants/_{tenant}.scss`, and one `@use './{tenant}';` line in `tenants.scss`.

# Goals

- Define one tenant's brand colour, scoped to its `data-tenant` attribute value
- Keep the file to a single `@include` — no bare `mat.theme()`, no typography, no density

# Naming convention

| use case | file name pattern | file name | selector pattern | selector |
| -------- | ----------------- | --------- | ---------------- | -------- |
| Tenant palette partial | `_{tenant}.scss` | `_globex.scss` | `:root[data-tenant='{tenant}']` | `:root[data-tenant='globex']` |

# Implementation changes

File: `projects/design-system/styles/tenants/_globex.scss` (worked example)

```scss
// Skill: class-tenant-palette
// Plateau: multi-tenant-design-system
// Version: <UTC timestamp at generation>
@use '@angular/material' as mat;
@use './tenant-theme' as tenant;

:root[data-tenant='globex'] {
  @include tenant.ds-tenant-theme(
    mat.$cyan-palette,
    (
      // only colours Material does not model — see custom-tokens.scss for the base values
      --ds-color-status-in-progress: light-dark(#00838f, #4dd0e1),
    )
  );
}
```

Then in `tenants.scss`:

```scss
// projects/design-system/styles/tenants/tenants.scss
@use './acme';
@use './globex';
```

# Rule changes

## MUST
- The file contains exactly one `:root[data-tenant='<id>']` block and one `@include tenant.ds-tenant-theme(...)`.
  - Risk: a bare `mat.theme()` or extra rules in a tenant file reintroduce per-tenant typography/base-style bloat and let tenants drift.
  - Fix: everything goes through the mixin; the block wraps it in the tenant selector.
- The selector attribute value equals the tenant's id in the `DsTenant` union.
  - Risk: `:root[data-tenant='globex-corp']` while `DsTenant` says `'globex'` — the consumer sets the attribute and nothing matches.
  - Fix: the selector value, the `DS_TENANTS` entry, and the file's `{tenant}` stem are the same kebab string.
- `--ds-*` overrides are only for colours Material does not model, each a `light-dark()` value.
  - Risk: overriding `--ds-spacing-*` or `--ds-radius-*` per tenant makes tenants differ on layout rhythm, not just brand.
  - Fix: the override map holds `--ds-color-*` entries only.
- A new tenant is added by creating the file **and** adding its `@use` line to `tenants.scss` **and** its id to `DS_TENANTS`.
  - Risk: a file with no `@use` line never ships; an id missing from `DS_TENANTS` cannot be passed type-safely by a consumer.
  - Fix: all three edits in the same change.

# Check list

- [ ] One `:root[data-tenant='<id>']` block, one `@include`
- [ ] The attribute value matches the `DsTenant` entry and the file stem
- [ ] Overrides are `--ds-color-*` with `light-dark()` values
- [ ] `tenants.scss` `@use`s the file and `DS_TENANTS` lists the id

# Unittest TestCases

- [ ] WHEN `data-tenant='globex'` is on `<html>` and a component using `--mat-sys-primary` renders THEN
  - [ ] its resolved colour is the globex palette's primary, in both light and dark schemes
- [ ] WHEN `data-tenant` is removed THEN
  - [ ] the same component falls back to the base brand palette
