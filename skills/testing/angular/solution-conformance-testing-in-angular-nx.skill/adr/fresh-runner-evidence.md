---
name: fresh-runner-evidence
description: Disable task caches and validate fresh project output.
problem: How can Nx caching avoid masking whether this run executed the tests?
decision: Disable task caches and validate fresh project output.
updated: 20261009
tags:
  - solution/conformance-testing-in-angular-nx
  - stack/typescript
  - framework/angular
  - concern/documentation
  - concern/documentation/adr
---

# Problem
How can Nx caching avoid masking whether this run executed the tests?

# Selected variant
[Fresh isolated execution](#fresh-isolated-execution)

# Searched variants
## Fresh isolated execution
**Selected.**

### Description
Pass `--skip-nx-cache` to every Nx call a kind makes and keep Nx's own data below the kind directory; the standard targets and their cache settings stay as the generators wrote them, so a developer's `nx test` is still cached. Reject a selected project whose run left no native result. Preserve the installed Playwright browser location before moving XDG cache.

### Benefits
- Every selected suite provides fresh evidence; sequential host shutdown cannot leave invocation state blocking another project host.

### Costs
- Runs cost more than cached targets and project orchestration starts additional Nx processes.

## Replay cached native reports
### Description
Cache target outputs and allow Nx to restore previous results.

### Benefits
- Faster repeated runs.

### Costs
- Cannot prove execution in this run; cached results may refer to a different browser or environment.
