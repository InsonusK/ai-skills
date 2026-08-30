---
name: design-system-workspace-tooling
description: Which workspace tool hosts the design system's library and demo app
problem: The platform monorepo uses Nx (per the "Структура репозитория" solution), but the design system is a separate, much smaller repository with only two projects (the library and its demo app) — Nx's main benefits may not justify its overhead at this scale
decision: Use a plain Angular CLI multi-project workspace, not Nx
tags:
  - solution/design-system-structure
  - concern/documentation
  - concern/documentation/adr
---

# Problem

The design system lives in its own repository, separate from the platform monorepo, containing just two projects: the component library itself and a demo application for visual preview. We need to decide whether to bring Nx into this repository too (for consistency with the platform) or use a plain Angular CLI multi-project workspace.

# Selected variant

**Selected variant:** [[#Angular CLI multi-project workspace]]

# Searched variants

## Angular CLI multi-project workspace

### Description

A single `angular.json` lists the library and demo app projects. No additional tool sits on top of the Angular CLI.

### Benefits

- Nx's core benefits — affected-based builds, enforced module boundaries via tags, Module Federation generators — all assume a workspace with enough projects and enough dependency complexity to be worth automating. With only two projects and one dependency edge (demo depends on the library), there is nothing meaningful to detect as "affected" (a library change almost always affects the demo, since the demo exists to render the library) and nothing meaningful to enforce as a boundary
- Avoids introducing a second workspace-tooling ecosystem (`project.json` generators, Nx-specific conventions) for a repository too small to benefit from it
- Lower barrier to entry for any external contributor familiar with stock Angular CLI but not Nx

### Costs

- If the design system repository later grows to host multiple related packages (e.g. a separate icons package, a separate theming package), Nx's affected-builds and boundary enforcement would become relevant again — this would need to be revisited with its own ADR at that point
- Slight inconsistency in tooling between this repository and the platform monorepo, which does use Nx

## Nx workspace (for consistency with the platform monorepo)

### Description

Bring Nx into the design system repository as well, matching the platform's tooling choice from the "Структура репозитория" solution.

### Benefits

- Tooling consistency across every repository in the organization
- If the repository grows to multiple packages later, the infrastructure is already in place

### Costs

- None of Nx's actual benefits (affected-builds, enforced boundaries, Module Federation generators) apply meaningfully to a two-project repository — the tooling would be adopted for consistency's sake alone, not because it solves a real problem here
- Adds Nx's own concepts and generators for a repository simple enough not to need them
