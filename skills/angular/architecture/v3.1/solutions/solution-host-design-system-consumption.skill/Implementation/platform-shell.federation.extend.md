---
description: Extend apps/platform-shell to declare the design system as a version-negotiated federation singleton and apply the global theme at the document root
element_kind: repository
change_kind: extend
tags:
  - solution/host-design-system-consumption
  - element/platform-shell-federation
---

# Structure

No new directories. This extends `apps/platform-shell`'s federation config (from `solution-platform-embeddability`) and its root styles.

## Directory and file skills

| Directory/file | Description |
| --------------- | ----------- |
| /apps/platform-shell/federation.config.ts | Extended: declares the design system as a shared dependency with `singleton: true`, `strictVersion: false`, and the platform's currently targeted version range, per [[skills/angular/architecture/v3.1/solutions/solution-host-design-system-consumption.skill/adr/design-system-version-negotiation.md|design-system-version-negotiation]]. |
| /apps/platform-shell/src/styles.scss | Imports the design system's `theme.scss` and `custom-tokens.scss` at the root, per [[skills/angular/architecture/v3.1/solutions/solution-host-design-system-consumption.skill/adr/theme-application-scope.md|theme-application-scope]] — the only production consumer required to do so. |

# Rules

## MUST
- `federation.config.ts` MUST declare the design system with `singleton: true` and `strictVersion: false` — never `strictVersion: true`, per [[skills/angular/architecture/v3.1/solutions/solution-host-design-system-consumption.skill/adr/design-system-version-negotiation.md|design-system-version-negotiation]].
- `apps/platform-shell`'s root styles MUST import both `theme.scss` and `custom-tokens.scss` from the design system package.
- The platform's declared design-system version range MUST be kept up to date as the platform itself upgrades, so embeddable apps have an accurate, current range to negotiate against.

# Unittest TestCases

- [ ] WHEN an embeddable app's declared design-system version range matches the platform's currently loaded version THEN
  - [ ] it shares the single loaded instance, with no duplicate design-system bundle fetched
- [ ] WHEN an embeddable app's declared range does not match THEN
  - [ ] it loads its own separate copy, and the platform and other embeddable apps are unaffected

## SHOULD
- **Setting `strictVersion: true` for the design system shared dependency** — Consequence: reintroduces the lockstep-upgrade coupling this solution's ADR exists to avoid — any embeddable app behind on its design-system version would simply fail to load — Instead: always `strictVersion: false`, letting a mismatched consumer fall back to its own isolated copy
