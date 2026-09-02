---
description: Generic pattern for how any embeddable app's own repository declares and consumes the design system — federation shared-dependency negotiation, theme import for standalone dev only
element_kind: repository
change_kind: extend
tags:
  - solution/remote-design-system-consumption
  - element/remote-design-system-federation
---

# How this generic file is used
This applies to any embeddable app repository (per `solution-platform-embeddability`'s embeddable-app baseline), extending the baseline structure that solution already establishes.

# Structure

## Directory and file skills

| Directory/file | Description |
| --------------- | ----------- |
| federation.config.ts | Declares the design system as a shared dependency: `singleton: true`, `strictVersion: false`, and this team's own `requiredVersion` range — the version(s) of the design system this app has been built and tested against. |
| /src/styles.scss | Imports the design system's `theme.scss`/`custom-tokens.scss`, used when this app is run standalone during local development — per [[skills/angular/architecture/v3.1/solutions/solution-host-design-system-consumption.skill/adr/theme-application-scope.md|theme-application-scope]], this import is redundant (but harmless) once the app is mounted inside the platform in production. |

# Rules

## MUST
- `federation.config.ts` MUST declare `requiredVersion` as an accurate reflection of the design-system version range this app has actually been built and tested against — not left as a wildcard/unbounded range, which would defeat the purpose of version negotiation.
- The team MUST keep `requiredVersion` updated as they adopt newer design-system versions, so version negotiation continues to reflect their real compatibility.
- This app's own `styles.scss` MUST still import the theme, for correct standalone local development — omitting it would make local preview visually incorrect, even though it's redundant in production.

## SHOULD
- The team SHOULD periodically update `requiredVersion` to track the platform's currently targeted design-system version, to get the shared-instance benefit (smaller payload, guaranteed consistency) rather than routinely running an isolated copy.

- **Declaring an unbounded or overly permissive `requiredVersion` (e.g. accepting any major version)** — Consequence: the app may end up sharing a design-system version it was never actually tested against, risking subtle visual or behavioral breakage that version negotiation was meant to prevent — Instead: declare the actual range this app has been built and verified against, updating it deliberately as that range changes
- **Never updating `requiredVersion` after the initial setup** — Consequence: the app permanently runs its own isolated copy of the design system, missing the shared-instance benefit indefinitely even as the platform and design system both move forward — Instead: periodically revisit and update the declared range as part of normal maintenance
# Unittest TestCases

- [ ] WHEN this app is run standalone (outside the platform) during local development THEN
  - [ ] it renders with the correct theme, since its own `styles.scss` applies it
- [ ] WHEN this app is mounted inside the platform and its `requiredVersion` matches the platform's loaded version THEN
  - [ ] no separate design-system bundle is fetched — the shared instance is used
