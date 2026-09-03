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
- `federation.config.ts` declares the design system with `singleton: true` and `strictVersion: false` — never `strictVersion: true`.
  - Risk: `strictVersion: true` fails to load any embeddable app that is behind on its design-system version, forcing lockstep upgrades across teams.
  - Fix: `strictVersion: false` lets a mismatched consumer fall back to its own isolated copy; per [[skills/angular/architecture/v3.1/solutions/solution-host-design-system-consumption.skill/adr/design-system-version-negotiation.md|design-system-version-negotiation]].
- `apps/platform-shell`'s root styles import both `theme.scss` and `custom-tokens.scss` from the design system package.
  - Risk: the platform is the only required production consumer of the theme — omit it and every mounted app renders un-themed.
  - Fix: `@use` both files in `src/styles.scss` at the document root; per [[skills/angular/architecture/v3.1/solutions/solution-host-design-system-consumption.skill/adr/theme-application-scope.md|theme-application-scope]].
- The platform's declared design-system version range is kept up to date as the platform upgrades.
  - Risk: a stale host range gives embeddable apps a wrong target to negotiate against, so they needlessly load isolated copies.
  - Fix: bump the range in `federation.config.ts` as part of each design-system upgrade.

# Unittest TestCases

- [ ] WHEN an embeddable app's declared design-system version range matches the platform's currently loaded version THEN
  - [ ] it shares the single loaded instance, with no duplicate design-system bundle fetched
- [ ] WHEN an embeddable app's declared range does not match THEN
  - [ ] it loads its own separate copy, and the platform and other embeddable apps are unaffected

## SHOULD
- **Setting `strictVersion: true` for the design system shared dependency** — Consequence: reintroduces the lockstep-upgrade coupling this solution's ADR exists to avoid — any embeddable app behind on its design-system version would simply fail to load — Instead: always `strictVersion: false`, letting a mismatched consumer fall back to its own isolated copy
