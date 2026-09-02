---
name: solution-design-system-multi-tenant-theming
description: Swappable palettes and per-tenant theme resolution on top of the design system's single fixed brand palette — the design system's first Variation Point
domain: skill
type: architecture
version: 20260902000000
tags:
  - skill/architecture/solution
  - stack/typescript
  - design-system
  - framework/angular-material
  - design-tokens
  - framework/angular
  - concern/architecture
  - solution/design-system-multi-tenant-theming

whenToUse: when the design system must support more than one brand or per-tenant theming, or when reviewing how a tenant's palette is resolved and applied
creates:
  - projects/design-system/src/styles/tenants/* (per-tenant palette definitions)
extends:
  - projects/design-system (theme resolution over the token layer)
depends_on:
  - "[[skills/angular/architecture/v3.1/solutions/solution-design-system-tokens.skill/solution-design-system-tokens.skill.md|solution-design-system-tokens]]"
adr: []
---

> **Draft contract — no consumer yet.** `solution-design-system-tokens` **explicitly defers** this: "multi-brand/per-tenant theming is explicitly deferred to a future solution, informed by real requirements if and when they appear." This is the `design-system` catalog's single VP (`MultiTenantTheming`). The shape below is a sketch; full Implementation is deferred until a real multi-tenant requirement exists.

# Goal
- Generalize the single fixed brand palette `solution-design-system-tokens` establishes into a set of swappable palettes.
- Resolve a tenant's palette at runtime (or build time) and apply it at the same root selector the single-tenant theme uses.
- Keep every existing `--mat-sys-*` / `--ds-*` consumption unchanged — only the palette values behind the tokens vary per tenant.

# Core Principle
- Each tenant is a palette definition (`--ds-*` and Material override values) in `projects/design-system/src/styles/tenants/{tenant}.scss`.
- A single resolution point picks the active tenant and applies its palette at the document root — the same `light-dark()` machinery still handles light/dark within each tenant.
- Components never learn about tenants — they consume tokens as before; the token *values* are what changes.
- `HybridDesignTokens` becomes "the single-tenant variant" of a `Theming` VP when this is composed.

# Boundaries
- `design-system` catalog VP1. `requires HybridDesignTokens` — it generalizes that feature's single palette.
- Does not change component authoring (`solution-design-system-components`) or the workspace/release setup (`solution-design-system-structure`).
- Tenant *selection* (which tenant is active for a given user/request) is a consuming-app concern, not the design system's.

# Rules

## MUST
- Never let a component reference a tenant name or a tenant-specific value directly.
  - Risk: components become tenant-aware and the token indirection breaks.
  - Fix: components consume `--mat-sys-*` / `--ds-*`; only the palette file behind them varies.
- Never apply more than one tenant palette at the document root at a time.

# Check list
- [ ] Each tenant is one palette file; components reference no tenant name.
- [ ] Exactly one tenant palette is applied at the root at any time.
- [ ] `light-dark()` still resolves light/dark within the active tenant.
