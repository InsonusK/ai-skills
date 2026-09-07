---
name: registry-monolith-repository
description: Conflict Detection result for the `monolith-repository` element — the Nx workspace root, touched by nearly every monolith solution
tags:
  - concern/architecture
  - stack/typescript
  - element/monolith-repository
---

# Element
`element/monolith-repository` — the Nx workspace root: the `apps/`/`libs/` layout, the `type:*`/`scope:*` tag taxonomy, the `@nx/enforce-module-boundaries` allow-list, CI. Everything above the level of an individual project.

# Involved solutions
`.create` (owns the base layout + taxonomy):
- [[skills/angular/architecture/v3.1/solutions/solution-repository-structure.skill/solution-repository-structure.skill.md|solution-repository-structure]] — `Repository.create`

`.extend` (each adds one tag value, one allow-list row, one `libs/` project, one convention, or one build option):
- Present already at `plateau-online-monolith`: [[skills/angular/architecture/v3.1/solutions/solution-app-routing.skill/solution-app-routing.skill.md|solution-app-routing]], [[skills/angular/architecture/v3.1/solutions/solution-api-http-layer.skill/solution-api-http-layer.skill.md|solution-api-http-layer]], [[skills/angular/architecture/v3.1/solutions/solution-global-store.skill/solution-global-store.skill.md|solution-global-store]], [[skills/angular/architecture/v3.1/solutions/solution-logging-base.skill/solution-logging-base.skill.md|solution-logging-base]], [[skills/angular/architecture/v3.1/solutions/solution-app-testing.skill/solution-app-testing.skill.md|solution-app-testing]], [[skills/angular/architecture/v3.1/solutions/solution-ui-testing.skill/solution-ui-testing.skill.md|solution-ui-testing]]
- Added deeper in the chain (same kind of extension): [[skills/angular/architecture/v3.1/solutions/solution-performance-tuned-routing.skill/solution-performance-tuned-routing.skill.md|solution-performance-tuned-routing]] (VP1), [[skills/angular/architecture/v3.1/solutions/solution-offline-first.skill/solution-offline-first.skill.md|solution-offline-first]] (VP4), [[skills/angular/architecture/v3.1/solutions/solution-offline-sync.skill/solution-offline-sync.skill.md|solution-offline-sync]] (VP5), [[skills/angular/architecture/v3.1/solutions/solution-logging-global.skill/solution-logging-global.skill.md|solution-logging-global]] (VP6), [[skills/angular/architecture/v3.1/solutions/solution-authentication.skill/solution-authentication.skill.md|solution-authentication]] (VP7), [[skills/angular/architecture/v3.1/solutions/solution-federation-host.skill/solution-federation-host.skill.md|solution-federation-host]] (platform-host), and `solution-forms` (baseline convention)

# Classification
`FMN` / `TMN` — a repository-level bucket. Category `M` (workspace-config change). Kind `N` (independent): each solution adds a *distinct* item — a new `type:*` value, a new `depConstraints` row, a new `libs/` project folder, or a build option. No two edit the same line. Where a constraint exists (VP4/VP5/VP7 require VP2/VP3), it is `T`; otherwise `F`. Either way canonical: the granularity is the individual config entry, and each solution owns its own.

# Ordering
`source: ordering-only` — the Nx config is additive and order-independent (a tag value, an allow-list row, a project folder). No solution needs to run before another to place its entry.

# Resolution
**Canonical — resolved by design, no resolver.** This is the intended shape: the workspace root is a shared surface that every feature extends by one entry. `check.sh` and `nx run-many -t lint` enforce that the accumulated allow-list stays consistent.

# Architectural signal
N ≥ 3 (eventually 13 solutions). **Benign.** This is not a case for reconsidering VP boundaries — a repository root touched by nearly every feature is the correct design for an Nx monorepo, not a symptom of a mis-drawn variation point. The note is recorded per the delta-conflict-detection N≥3 rule only to make the "we looked at this and it is fine" explicit.
