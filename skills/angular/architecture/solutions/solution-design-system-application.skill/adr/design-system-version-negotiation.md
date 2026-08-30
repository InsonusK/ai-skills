---
name: design-system-version-negotiation
description: How the platform and independently deployed embeddable apps coordinate which version of the design system they run at runtime
problem: A strict singleton (one shared version, error on mismatch) forces every embeddable-app team to upgrade in lockstep with the platform, which conflicts with their independent deploy cadence (per the "Встраиваемость платформы" solution); no sharing at all loses the benefit of a consistent, deduplicated design system instance when versions do happen to align
decision: Share the design system as a federation singleton with version negotiation — singleton: true, strictVersion: false, each consumer declaring its own requiredVersion. When a consumer's required range is satisfied by the already-loaded version, it shares that singleton instance; when it isn't, that consumer falls back to loading its own separately-bundled copy, in isolation, without blocking the platform or any other team
tags:
  - solution/design-system-application
  - concern/documentation
  - concern/documentation/adr
---

# Problem

The design system is consumed by the platform shell and by every independently deployed embeddable app (per the "Встраиваемость платформы" solution), each with its own release cadence and no obligation to coordinate deploys with each other. A strict singleton shared dependency (one version at runtime, hard error on any mismatch) would force every embeddable-app team to upgrade to the platform's design-system version before they could deploy at all — directly conflicting with the independent-deploy requirement that motivated Native Federation in the first place. Treating the design system as a fully independent dependency per consumer (never shared) avoids that coupling but gives up the benefit — smaller total payload, visually consistent components sharing one CSS/JS instance — whenever versions do happen to align, which should be the common case in a healthy system.

# Selected variant

**Selected variant:** [[#Version-negotiated singleton]]

The design system is declared as a federation shared dependency with `singleton: true` and `strictVersion: false`. The platform declares the version it currently targets; each embeddable app declares, in its own federation config, the version range (`requiredVersion`) it supports. At runtime: if an embeddable app's required range is satisfied by the version already loaded (the platform's), it shares that single loaded instance. If it is not satisfied, that specific embeddable app falls back to loading and running its own separately-bundled copy of the design system, isolated from the shared instance — without blocking the platform or any other embeddable app.

# Searched variants

## Version-negotiated singleton

### Description

`singleton: true` establishes the intent to share one instance when possible; `strictVersion: false` means a version mismatch does not hard-error the whole application — instead, the mismatched consumer's own bundled copy is used for that consumer alone. Each side's `requiredVersion` (the platform's target, each embeddable app's declared supported range) drives whether sharing happens for that specific pairing.

### Benefits

- No embeddable-app team is forced to upgrade in lockstep with the platform's release cadence just to deploy — a team can lag behind (or move ahead) the platform's targeted version and still function correctly, on their own isolated copy
- Teams that do stay within the platform's currently targeted version range get the full benefit of a shared, deduplicated instance — smaller total payload, and guaranteed visual consistency for that pairing, without needing to explicitly configure anything beyond declaring their supported range
- Failure mode is graceful and localized: a version mismatch means one embeddable app runs its own isolated copy, not a broken page or a blocked deploy
- Encourages convergence over time without forcing it — an embeddable team is incentivized to stay within range for the payload/consistency benefit, but is never blocked from shipping if they can't yet

### Costs

- A consumer running its own isolated copy under a version mismatch may visually or behaviorally diverge from the platform's current design-system version until that team upgrades — a real, if usually minor and temporary, inconsistency risk
- Requires each embeddable app's team to actually declare and maintain an accurate `requiredVersion`/supported range in their own federation config — an ongoing (if small) maintenance responsibility, not something the platform can enforce unilaterally
- More nuanced to reason about than a strict singleton — "shared when compatible, isolated when not" requires understanding both states, rather than a single guaranteed outcome

## Strict singleton (singleton: true, strictVersion: true)

### Description

One shared design-system instance is mandatory; any consumer whose required range doesn't match the already-loaded version fails to load (a hard error), rather than falling back to its own copy.

### Benefits

- Guarantees visual and behavioral consistency across every part of the platform at all times — no consumer can ever run a divergent version
- Simplest to reason about: there is exactly one state, not two

### Costs

- Forces every embeddable-app team into lockstep with the platform's design-system version — an incompatible embeddable app simply fails to load, which directly conflicts with the independent-deploy guarantee the "Встраиваемость платформы" solution's Dynamic Federation was chosen to provide
- A single team lagging behind on a design-system upgrade can block their entire embeddable app from functioning within the platform, even if their own app logic works fine

## No sharing at all (each consumer bundles its own copy unconditionally)

### Description

The design system is never marked as a shared/singleton dependency; every consumer — the platform and every embeddable app — always loads and runs its own independently-bundled copy, regardless of version.

### Benefits

- Maximum independence — no version coordination needed between any consumers, ever
- Simplest possible federation configuration for this dependency

### Costs

- Gives up the deduplication/consistency benefit entirely, even in the common case where every consumer happens to be on a compatible version — every embeddable app pays the full payload cost of its own copy regardless
- No mechanism exists to converge toward a shared instance even when it would be free to do so
