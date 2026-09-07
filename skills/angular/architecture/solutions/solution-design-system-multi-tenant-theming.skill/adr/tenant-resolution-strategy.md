---
name: tenant-resolution-strategy
description: How the active tenant's palette is selected and applied — a CSS [data-tenant] attribute on the document root, set by the consuming application; no runtime JS token rewriting, no per-tenant CSS bundles, no build-time baking
problem: solution-design-system-multi-tenant-theming generalizes the single fixed brand palette into swappable per-tenant palettes. Something has to pick which tenant is active for a given user/request and apply its palette. Tenant selection is a consuming-app concern, but the mechanism the design system ships determines what the consuming app can do.
decision: The design system ships each tenant as a CSS rule set scoped to :root[data-tenant='<id>'], overriding only the colour system tokens. The consuming application sets document.documentElement.dataset.tenant to a value from the design system's exported DsTenant union. No JavaScript rewrites custom properties at runtime; no per-tenant CSS file is loaded on demand; no per-tenant app bundle is built.
tags:
  - solution/design-system-multi-tenant-theming
  - stack/typescript
  - framework/angular-material
  - concern/architecture
  - concern/documentation
  - concern/documentation/adr
---

# Problem

`solution-design-system-tokens` establishes one fixed brand palette, applied by `mat.theme()` at the document root, with `light-dark()` handling light/dark and **no JavaScript theme toggle**. This solution generalizes that into a set of tenant palettes. Two questions follow:

1. **How is a tenant's palette expressed and shipped** — as CSS the consumer includes, as compiled per-tenant files, or as data the consumer's build turns into CSS?
2. **How does the active tenant get applied** — a CSS selector the consumer toggles, a script that rewrites custom properties, or a separate bundle per tenant?

Tenant *selection* (which tenant a given logged-in user belongs to) is the consuming application's job. But the mechanism the design system ships constrains how cheap and how flash-free that selection can be, and it must not walk back the "no JS theme toggle" principle the base token layer rests on.

# Selected variant

**Selected variant:** [[#A [data-tenant] attribute selector, set by the consuming app (selected)]]

# Searched variants

## A [data-tenant] attribute selector, set by the consuming app (selected)

### Description

Each tenant ships as one SCSS file that emits a rule set scoped to `:root[data-tenant='<id>']`, overriding the Material colour system tokens (`--mat-sys-primary` and the rest of the colour set) and any tenant-specific `--ds-*` values, all still through `light-dark()`. The consuming application imports `design-system/styles/tenants` once (after `design-system/styles/theme`) and sets `document.documentElement.dataset.tenant = '<id>'` — the `<id>` typed against the design system's exported `DsTenant` union. With no attribute set, the base `:root` palette from `theme.scss` applies.

### Benefits

- **No JavaScript touches styling.** Setting one attribute is not a "theme toggle" in the sense the base ADR forbids — the browser's cascade does all the work, `light-dark()` still resolves light/dark within the active tenant, and there is no custom-property enumeration in JS to keep in sync with the token layer.
- **No flash.** The attribute can be set on `<html>` in `index.html` or the earliest possible bootstrap step (from a value the server already rendered into the page), so the correct palette is in effect before first paint. Even set later, only the palette shifts — layout, typography, and density never move.
- **One shared instance.** The tenants stylesheet is part of the single design-system package every consumer (monolith, platform-host, every embeddable remote) already loads once; a federated remote inherits the host's active tenant automatically because it inherits the host's `<html>`.
- **Components stay tenant-blind.** They consume `--mat-sys-*` / `--ds-*` exactly as before; only the values behind the tokens change under the attribute.
- **Adding a tenant is one file.** No build-config change, no bundle to wire, no route to register.

### Costs

- Every tenant's colour tokens ship to every consumer even if only one tenant is ever used by that deployment. Accepted: the colour system token set is small (a few dozen custom properties per tenant), far smaller than re-emitting a full `mat.theme()`, and a deployment that truly needs only one brand simply does not use this solution.
- The consuming app is responsible for setting the attribute early enough to avoid a flash — the design system documents this but cannot enforce it.
- Overriding `mat.theme()`'s colour output at a more specific selector relies on Material keeping its colour tokens as CSS custom properties on the theme selector (it does, and has since M3 landed) — a hard change there would need this revisited.

## A runtime applyTenant() helper that rewrites custom properties

### Description

The design system ships `applyTenant(id)` which reads a per-tenant token map and sets `--mat-sys-*` / `--ds-*` on `document.documentElement.style` at runtime.

### Benefits

- Only the active tenant's values are ever in the DOM.
- The consumer calls one typed function instead of knowing about an attribute.

### Costs

- This **is** the JavaScript theme toggle `solution-design-system-tokens`'s `light-dark-mode-strategy` ADR rejected — a JS enumeration of every themable custom property that has to track the token layer exactly, and drifts the moment Material adds or renames one.
- A flash of the default palette until the script runs, unless it is inlined and blocking in `<head>`.
- `light-dark()` interaction gets awkward — the script has to decide light vs dark itself or write `light-dark()` strings as values.

## Per-tenant compiled CSS files, loaded on demand

### Description

The build produces `tenant-acme.css`, `tenant-globex.css`, …; the consuming app injects a `<link>` for the active one.

### Benefits

- Only the active tenant's CSS is downloaded.

### Costs

- Loading a stylesheet after first paint is a guaranteed flash; loading it blocking in `<head>` needs the tenant known before the app bundle, i.e. server-rendered anyway — at which point a server-set attribute is simpler.
- A `<link>` orchestration layer (which file, from where, cache-busting) that every consumer reimplements.
- A federated remote would have to coordinate its own tenant stylesheet with the host's — the attribute approach makes this free.

## Build-time per-tenant application bundles

### Description

Each consuming app is built once per tenant with the tenant's palette compiled in.

### Benefits

- Smallest possible CSS per deployment; tenant fixed and unmissable.

### Costs

- Multiplies every consumer's CI matrix by the tenant count.
- Incompatible with the federation single-instance model — a host and its remotes would each need matching per-tenant builds, and a remote serving multiple hosts on different tenants becomes impossible.
- A new tenant means re-releasing every application, not shipping one CSS file.
