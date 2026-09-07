---
name: solution-remote-design-system-consumption
description: A federation remote declares the design-system package as a version-negotiated federation singleton with an accurate requiredVersion range, sharing the host's instance when ranges align and falling back to its own bundled copy otherwise — never blocking its own deploy
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
  - solution/remote-design-system-consumption

whenToUse: when a remote needs design-system components, configuring its requiredVersion range, or reviewing why the remote loaded its own bundled copy
creates: []
extends:
  - "the remote's federation config (design-system as a version-negotiated singleton)"
  - "the remote's styles.scss (theme import for standalone dev only)"
depends_on:
  - "[[skills/angular/architecture/v3.1/solutions/solution-federation-remote.skill/solution-federation-remote.skill.md|solution-federation-remote]]"
adr:
  - "[[skills/angular/architecture/v3.1/solutions/solution-host-design-system-consumption.skill/adr/design-system-version-negotiation.md|design-system-version-negotiation]]"
---

# Goal
- Let a remote use design-system components while shipping on its own schedule and its own design-system version.
- Share the host's already-loaded instance when the versions align — no separate bundle, identical styling.

# Capabilities
- A version mismatch never blocks the remote's own deploy — it falls back to its own bundled copy and renders correctly.
- When the remote stays in the host's targeted range it gets the smaller payload and guaranteed visual consistency for free.

# Core Principle
- The remote declares `design-system` as a federation singleton with **version negotiation** — `singleton: true`, `strictVersion: false` — and an **accurate `requiredVersion`** range reflecting what it was built and tested against.
- Sharing happens automatically when the host's loaded version satisfies that range; otherwise the remote loads its own separately-bundled copy.
- The remote imports the theme only for **standalone local development** — in production its mounted components inherit the host shell's theme from the shared document.

# Boundaries
- The **remote** side. The host side is [[skills/angular/architecture/v3.1/solutions/solution-host-design-system-consumption.skill/solution-host-design-system-consumption.skill.md|solution-host-design-system-consumption]].
- `embeddable-app` VP2. A remote that renders no shared-styled UI does not compose this solution.
- Consumes the published `design-system` package (the `design-system` catalog); does not model it.

# Adr
- [[skills/angular/architecture/v3.1/solutions/solution-host-design-system-consumption.skill/adr/design-system-version-negotiation.md|design-system-version-negotiation]] — shared with the host: version-negotiated singleton is the mechanism both sides implement.

# Requirements

SOLUTION:
- [[skills/angular/architecture/v3.1/solutions/solution-federation-remote.skill/solution-federation-remote.skill.md|solution-federation-remote]]
  - the remote federation config this extends

NPM:
- the `design-system` package — declared as a version-negotiated shared dependency in the remote's federation config.

# Template Skill Mutations

REPOSITORY:
- [[skills/angular/architecture/v3.1/solutions/solution-remote-design-system-consumption.skill/Implementation/federation.extend.md|Remote federation config]] - extend - declare `design-system` as a version-negotiated singleton with an accurate `requiredVersion`; import the theme for standalone dev only

# Workflow

## Compatible versions — shared instance (happy path)

1. The host targets `design-system@3.2.0`; the remote declares `requiredVersion: ^3.0.0`.
2. `3.2.0` satisfies `^3.0.0` → the remote shares the host's already-loaded instance; no separate bundle.

## Incompatible versions — isolated fallback (happy path)

1. The host upgrades to `4.0.0`; the remote still declares `^3.0.0`.
2. `4.0.0` does not satisfy `^3.0.0` → the remote loads its own bundled `3.x` copy and renders correctly on the version it was built against — its deploy is not blocked.

# Rules

## MUST
- [[skills/angular/architecture/v3.1/solutions/solution-remote-design-system-consumption.skill/Implementation/federation.extend.md#MUST|federation.extend]]
- Never declare an unbounded `requiredVersion` — keep it an accurate range for what the remote was built and tested against.
  - Risk: the remote silently shares an incompatible host version and renders wrong.
  - Fix: a bounded range; a mismatch then degrades to a safe isolated copy.
- Never rely on the remote's own theme import in production.
  - Risk: duplicated CSS payload and possible specificity conflicts with the host's theme.
  - Fix: the theme import is dev-only; production inherits from the host document.

## SHOULD
- Avoid never updating `requiredVersion` after initial setup — bump it when the remote adopts a newer design-system.

# Check list
- [ ] `design-system` is `singleton: true`, `strictVersion: false` in the remote's federation config.
- [ ] `requiredVersion` is an accurate, bounded range.
- [ ] The theme import is understood as standalone-dev-only.
- [ ] A mismatch degrades to an isolated copy — never a hard failure.
