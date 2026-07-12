---
name: token-consumption-strategy
description: How design system components consume design tokens — Angular Material's own M3 system tokens versus a fully custom semantic layer
problem: Angular Material's M3 theming already generates a semantic (not primitive) layer of CSS custom properties (--mat-sys-*) for colors, typography, and elevation; building a full custom aliasing layer over all of it would duplicate an already-semantic layer, but Material's token set does not model domain-specific concepts this application needs (priority levels, workflow states, spacing scale)
decision: Consume --mat-sys-* tokens directly wherever Material already models the concept (colors, typography, elevation); define a small set of custom --ds-* tokens only for genuine gaps — domain-specific semantic colors (e.g. high/medium/low priority, stop/start/in-progress states) and a spacing/radius scale
---

# Problem

Angular Material's M3 theming (stable since Angular Material 19+) generates `--mat-sys-*` CSS custom properties that are already named by role/intent (e.g. `--mat-sys-primary`, `--mat-sys-on-surface`), not raw values — this is already the "semantic token" layer a traditional three-tier design-token architecture (primitive → semantic → component) would otherwise need to build from scratch. However, this application has domain-specific color/state concepts Material's own token set does not model at all — priority levels (high/medium/low), workflow states (stop/start/in-progress), and a spacing/border-radius scale. We need to decide whether to build a full custom semantic layer over everything Material provides, or consume Material's own tokens directly and only add what's genuinely missing.

# Selected variant

**Selected variant:** [[#Hybrid: --mat-sys-* directly + custom --ds-* tokens for gaps only]]

Design system components consume `--mat-sys-*` tokens directly for anything Material's own M3 token set already models — colors, typography, elevation. A separate, small set of `--ds-*` custom properties is defined only for concepts Material does not cover: domain-specific semantic colors (priority/state categories) and a spacing/radius scale.

# Searched variants

## Hybrid: --mat-sys-* directly + custom --ds-* tokens for gaps only

### Description

Components reference `--mat-sys-primary`, `--mat-sys-on-surface`, `--mat-sys-headline-medium`, etc. directly in their styles, exactly as Angular Material's own components do and as the framework's own guidance recommends ("your custom components should consume these tokens too"). A separate, small token set — `--ds-color-priority-high`, `--ds-color-status-in-progress`, `--ds-spacing-*`, `--ds-radius-*` — covers only what M3 doesn't model, defined once at the theme root alongside the `mat.theme()` mixin's output.

### Benefits

- No duplicated aliasing layer over tokens that are already named by intent — `--mat-sys-primary` already tells you what it's for, wrapping it in `--ds-color-primary: var(--mat-sys-primary)` would add indirection with no naming improvement
- Matches Angular Material's own stated guidance for custom components in a Material-based application
- Automatically inherits any future improvements/corrections Material makes to its own token set (color contrast fixes, new roles) without this design system needing to track and re-alias them
- The custom `--ds-*` layer stays small and focused on concepts that genuinely have no Material equivalent, making it easy to know, for any given token, whether it comes from Material or from this design system

### Costs

- Two token namespaces to know (`--mat-sys-*` and `--ds-*`) instead of one uniform prefix — a contributor needs to know which concepts live where
- If the application ever needed to move away from Angular Material as a foundation, every direct `--mat-sys-*` reference in custom components would need to be migrated — a full custom aliasing layer would have absorbed that risk instead. This is accepted as a low-likelihood scenario not worth designing around now

## Full custom semantic layer over everything, including colors/typography

### Description

Define `--ds-color-primary: var(--mat-sys-primary)` (and equivalents for every other Material system token actually used), with all custom components referencing only `--ds-*`, never `--mat-sys-*` directly.

### Benefits

- Single, uniform token namespace for all custom components
- Full insulation from Material's own token naming, if it were ever to change or if Material were ever replaced as the foundation

### Costs

- Duplicates an already-semantic layer for no naming benefit — `--mat-sys-primary` already communicates intent as clearly as `--ds-color-primary` would
- Every new Material system token a component might need requires first adding an alias entry, an extra step for no benefit when the alias is a 1:1 passthrough
- Directly contradicts Angular Material's own documented guidance for custom components in an M3-based application

## No custom tokens at all — use only what Material provides, work around domain-specific needs ad hoc

### Description

Rely entirely on `--mat-sys-*`; represent domain-specific concepts (priority, workflow state) with ad hoc, per-component color choices rather than a defined token set.

### Benefits

- No additional token layer to design or maintain at all

### Costs

- Domain-specific semantic colors (priority, workflow state) end up defined inconsistently, component by component, with no single source of truth — exactly the problem a design system exists to prevent
- No consistent way to theme these domain-specific concepts together with the rest of the palette (e.g. adjusting for dark mode, per this solution's light/dark decision)
