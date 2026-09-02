---
name: brand-theming-scope
description: Whether the design system supports one fixed brand palette or multiple swappable brand palettes
problem: The platform hosts embeddable apps from separate teams (per `solution-federation-host`); a multi-brand/per-tenant theming capability could be built now, or deferred until an actual need for it appears
decision: Ship a single, fixed brand palette for now; explicitly defer multi-brand/per-tenant palette-swapping to a future ADR, revisited if and when it's actually needed
tags:
  - solution/design-system-tokens
  - concern/documentation
  - concern/documentation/adr
---

# Problem

Different embeddable apps in this platform are built by separate teams. It's plausible that at some point different teams or tenants might want a different brand palette applied to the same component library. Building that capability now (a swappable-palette theming architecture) is a real option, but there is no concrete requirement for it yet — building it speculatively risks solving a problem that may never materialize, or that would look different once real requirements exist.

# Selected variant

**Selected variant:** [[#Single fixed brand palette, revisit later]]

The design system ships one brand palette, defined once via `mat.theme()`, consumed by the platform and every embeddable app. No palette-swapping mechanism is built now.

# Searched variants

## Single fixed brand palette, revisit later

### Description

One `mat.theme()` definition, one brand palette, used everywhere. If a genuine need for per-tenant/multi-brand theming arises later, it becomes its own dedicated solution with its own ADR, informed by the actual requirements at that time (which tenants, how palettes are supplied, whether it's build-time or runtime-configurable).

### Benefits

- No speculative complexity built against a requirement that doesn't exist yet — avoids guessing at a palette-swapping architecture that might not match whatever the actual future requirement turns out to be
- Keeps this solution's scope focused and simple, consistent with shipping the smallest solution that satisfies the current, real requirement

### Costs

- If multi-brand theming is needed later, some of what's built now (a single hardcoded theme definition) will need to be reworked into a more parameterized form — this rework cost is accepted as the price of not over-building now

## Build swappable multi-brand/per-tenant theming now

### Description

Design the theming architecture from the start to support multiple brand palettes, selectable per embeddable app or tenant.

### Benefits

- No future rework needed if multi-brand theming does turn out to be required
- Embeddable teams could diverge their branding immediately if desired

### Costs

- Meaningfully more upfront design and implementation work for a requirement that is currently hypothetical, not concrete
- Risks building the wrong shape of solution — the actual requirements for tenant-specific theming (how palettes are supplied, at build time or runtime, how many tenants, whether it's even desired by the business) aren't known yet, and guessing now could produce an architecture that doesn't fit once real requirements emerge
