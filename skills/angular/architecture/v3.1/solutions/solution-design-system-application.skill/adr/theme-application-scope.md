---
name: theme-application-scope
description: Which consumer(s) are responsible for applying the design system's global theme (mat.theme() and custom tokens) at runtime
problem: The design system's theme is applied via CSS custom properties at the document root; in production every embeddable app renders into the same document as the platform shell (per Native Federation, unlike iframe or Web Components), so requiring every consumer to independently apply the theme would be redundant
decision: Only apps/platform-shell is required to apply the global theme in production. Each embeddable app's own repository still imports the theme for its own standalone local development/preview, but this is not required for correctness once mounted inside the platform — CSS custom properties set at the document root cascade to any DOM node in that document, regardless of which federated bundle rendered it
tags:
  - solution/design-system-application
  - concern/documentation
  - concern/documentation/adr
---

# Problem

The design system's theme (`theme.scss`'s `mat.theme()` output, plus `custom-tokens.scss`) works by setting CSS custom properties at the document's root selector. Per `solution-platform-embeddability`, Native Federation means the platform shell and every mounted embeddable app share one JS runtime and one DOM document — unlike an iframe or Web Components approach, where each piece would need its own independently applied theme due to genuine document/shadow-DOM isolation. We need to decide whether every consumer (platform and each embeddable app) must apply the theme independently, or whether one application of it at the document root is sufficient for the whole assembled page.

# Selected variant

**Selected variant:** [[#Platform-shell applies the theme in production; embeddable apps apply it only for standalone development]]

`apps/platform-shell` is the only consumer required to apply the theme in production, at the document root. Every embeddable app's own repository still imports and applies the theme for its own standalone local development/preview (since during local development, that app is not running inside the platform's document) — but this is understood to be redundant, not required, once that app's components are actually mounted inside the platform shell in production, since they inherit the platform's already-applied root-level CSS custom properties.

# Searched variants

## Platform-shell applies the theme in production; embeddable apps apply it only for standalone development

### Description

Only `apps/platform-shell` includes `theme.scss`/`custom-tokens.scss` in its production build's applied styles at the root selector. Each embeddable app's repository also imports these files (so that running the app standalone, outside the platform, during local development still looks correct), but this application of the theme is inert/redundant once that app's components are mounted inside the platform's document, since Angular Material and this design system's own components resolve their styling from whatever CSS custom properties are already present at the document root — which the platform has already set.

### Benefits

- No duplicated theme CSS needs to ship as part of every embeddable app's production bundle for correctness — the platform's own application of the theme already covers every mounted component, first-party or federated
- Embeddable app teams still get correct, on-brand visual output during their own local, standalone development (outside the platform), since they import the theme for that purpose
- Directly follows from the shared-document architecture Native Federation was chosen for in `solution-platform-embeddability` — this is a natural consequence, not an extra mechanism to build

### Costs

- Relies on every embeddable app actually being mounted inside the platform's document in production for this to hold — if an embeddable app is ever served completely standalone in production (not embedded), it would need its own theme application there too, which is exactly what its already-imported theme (for local dev purposes) would also cover
- A developer unfamiliar with this reasoning might assume their embeddable app's own imported theme is "doing the work" in production and be confused to learn it's actually redundant there — this reasoning needs to be understood, not just followed by convention

## Every consumer independently applies the theme, including in production

### Description

Both the platform shell and every embeddable app apply `theme.scss`/`custom-tokens.scss` at their own root selector, always, in every environment including production.

### Benefits

- No dependency on the shared-document assumption holding — correct even if an embeddable app is ever served standalone in production
- Conceptually simpler rule: "every consumer always applies the theme," no special-casing for the platform vs. embeddable apps

### Costs

- Redundant CSS shipped in every embeddable app's production bundle, duplicating exactly what the platform's own root-level application already provides once mounted — a real, if modest, unnecessary payload cost repeated across every embeddable app
- Slightly misrepresents the actual architecture: since CSS custom properties cascade from the document root regardless of which bundle set them, having multiple bundles redundantly re-set the same root-level properties provides no additional correctness benefit inside the platform, only outside it
