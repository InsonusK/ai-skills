---
name: release-versioning-strategy
description: How the design system's version bumps and changelog are determined and published
problem: The design system is consumed by the platform and by every independently deployed embeddable app; a manual, discipline-dependent versioning process risks an incorrectly classified breaking change reaching multiple independent teams simultaneously
decision: Use Changesets
---

# Problem

Every release of the design system needs a correctly classified semver bump (patch/minor/major) and an accurate changelog entry. Because the package is consumed by several independently deployed codebases at once (the platform monorepo, and every embeddable app's own repository, per the "Встраиваемость платформы" solution), a misclassified release — especially a breaking change shipped as a minor or patch — has a wide, simultaneous blast radius across teams that don't coordinate deploys with each other.

# Selected variant

**Selected variant:** [[#Changesets]]

# Searched variants

## Changesets

### Description

Each pull request that changes the published package includes a small changeset file (a short markdown file stating the intended bump — patch/minor/major — and a human-readable description). On release, tooling aggregates all pending changesets into the version bump and CHANGELOG automatically.

### Benefits

- The bump classification is made deliberately, once, at the time the change is made — not inferred automatically from commit message formatting (which is easy to get wrong) nor left to be remembered later at release time
- CHANGELOG entries are written by the person who made the change, while the context is fresh, rather than reconstructed after the fact
- Widely adopted for exactly this scenario (a package consumed by multiple independent teams) — reduces the chance of a breaking change slipping out under a non-major version, which would simultaneously affect the platform and every independent embeddable-app team

### Costs

- Adds one extra step to the PR process (running the changeset CLI to generate the small markdown file) — a minor addition to contributor workflow

## Manual semver + manual CHANGELOG

### Description

A maintainer manually decides the version bump and hand-writes the CHANGELOG entry at release time.

### Benefits

- No additional tooling or PR-time step
- Full manual control over the final changelog wording

### Costs

- Relies entirely on the releaser's memory and discipline to correctly classify every change and not forget any since the last release — exactly the kind of process that degrades under deadline pressure
- Given multiple independent teams consume this package, a misclassified breaking change (shipped as non-major) can silently break several unrelated deployments at once, with no structural safeguard against it
