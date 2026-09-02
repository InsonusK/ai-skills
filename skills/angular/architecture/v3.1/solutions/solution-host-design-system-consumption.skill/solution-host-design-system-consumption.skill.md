---
name: solution-host-design-system-consumption
description: The federation host consumes the design-system package as a version-negotiated federation singleton (singleton true, strictVersion false) and applies the theme once at the document root in production, so mounted remotes inherit it through the shared document
domain: skill
type: architecture
version: 20260902000000
tags:
  - skill/architecture/solution
  - stack/typescript
  - design-system
  - framework/native-federation
  - framework/angular
  - concern/architecture
  - solution/host-design-system-consumption

whenToUse: when onboarding the design system into a federation host, configuring it as a version-negotiated shared dependency, or reviewing why a remote is or isn't sharing the host's design-system instance
creates: []
extends:
  - apps/platform-shell (federation config, root styles)
depends_on:
  - "[[skills/angular/architecture/v3.1/solutions/solution-federation-host.skill/solution-federation-host.skill.md|solution-federation-host]]"
adr:
  - "[[skills/angular/architecture/v3.1/solutions/solution-host-design-system-consumption.skill/adr/design-system-version-negotiation.md|design-system-version-negotiation]]"
  - "[[skills/angular/architecture/v3.1/solutions/solution-host-design-system-consumption.skill/adr/theme-application-scope.md|theme-application-scope]]"
---

# Goal
- Give the host and every remote one deduplicated design-system instance whenever their declared version ranges align — without making that alignment mandatory.
- Apply the theme's CSS once, at the host document root, so mounted remotes inherit it with no redundant payload.

# Capabilities
- A remote team ships on its own design-system version without being blocked by a host upgrade it has not adopted.
- Teams within the host's targeted range automatically get a smaller payload and guaranteed visual consistency, by declaring their range alone.
- Only the host ships the theme CSS in production.

# Core Principle
- The host declares `design-system` as a federation singleton with **version negotiation** — `singleton: true`, `strictVersion: false`. Sharing happens automatically when ranges are compatible; an isolated copy is loaded otherwise.
- `apps/platform-shell` is the only consumer that applies the global theme in production; mounted remotes' components render into the same DOM document and inherit its CSS custom properties.
- This upgrades the plain npm dependency a monolith already had — the design system is consumed the same way, only the federation sharing config is added.

# Boundaries
- The **host** side. The remote side (a remote declaring its own `requiredVersion`) is [[skills/angular/architecture/v3.1/solutions/solution-remote-design-system-consumption.skill/solution-remote-design-system-consumption.skill.md|solution-remote-design-system-consumption]].
- `platform-host` VP1. Not a federation prerequisite — a host whose remotes each bring their own UI can skip it (feature-model open question).
- Does not model the `design-system` package itself — see the `design-system` catalog.

# Adr
- [[skills/angular/architecture/v3.1/solutions/solution-host-design-system-consumption.skill/adr/design-system-version-negotiation.md|design-system-version-negotiation]] — version-negotiated singleton over a strict singleton or fully independent per-consumer versions.
- [[skills/angular/architecture/v3.1/solutions/solution-host-design-system-consumption.skill/adr/theme-application-scope.md|theme-application-scope]] — the host applies the theme in production; remotes apply it only for standalone dev.

# Requirements

SOLUTION:
- [[skills/angular/architecture/v3.1/solutions/solution-federation-host.skill/solution-federation-host.skill.md|solution-federation-host]]
  - the federation config this extends with a shared-dependency declaration

NPM:
- the `design-system` package — declared as a version-negotiated shared dependency in the host's federation config.

# Template Skill Mutations

REPOSITORY:
- [[skills/angular/architecture/v3.1/solutions/solution-host-design-system-consumption.skill/Implementation/platform-shell.federation.extend.md|apps/platform-shell]] - extend - declare `design-system` as a version-negotiated shared dependency; apply the theme at the root

# Rules

## MUST
- [[skills/angular/architecture/v3.1/solutions/solution-host-design-system-consumption.skill/Implementation/platform-shell.federation.extend.md#MUST|platform-shell.federation.extend]]
- Never set `strictVersion: true` for the design system — that reintroduces lockstep coupling with every remote.
  - Risk: a host design-system upgrade blocks every remote that has not adopted it.
  - Fix: `strictVersion: false` — a mismatch degrades to an isolated copy, never a hard failure.
- Never apply the theme from more than one place in production.
  - Risk: duplicated CSS payload; specificity conflicts.
  - Fix: the host root is the single application point; remotes inherit.

## SHOULD
- Avoid declaring an unbounded `requiredVersion` for the host — keep it an accurate range.

# Check list
- [ ] `design-system` is declared `singleton: true`, `strictVersion: false` in the host's federation config.
- [ ] Only `apps/platform-shell` applies the theme in production.
- [ ] A version mismatch degrades to an isolated copy for the mismatched consumer alone.
