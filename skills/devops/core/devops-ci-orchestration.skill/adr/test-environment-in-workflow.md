---
name: test-environment-in-workflow
description: Where a project prepares the services its tests need in CI, given that both workflow files are ready files copied from a skill
problem: Both workflow files were copied byte for byte, and the same skill set told a project to add a step that starts its database and to set the job's env. A project whose tests need a database could not follow both.
decision: The test environment of the test-kind job — steps before the test step and the job's env — is the one part of a copied workflow a project writes itself. Everything else in the file stays as the skill ships it.
tags:
  - stack
  - concern/ci
  - concern/documentation
  - concern/documentation/adr
---

# Problem
`devops-github-wf-pull-request` and `devops-github-wf-release` required `.github/workflows/*.yml` to be byte-identical to their assets. `devops-ci-orchestration` required a `docker compose` step and `env` entries in the same files for a project whose tests need a service. A Go service with a PostgreSQL store hit both rules at once: without `DATABASE_DSN` its store scenarios fail. Where does a project prepare what its tests need?

# Selected variant
[[#The project writes the test environment in the workflow]]
- Decided by the owner on 2026-10-10: what matters is that tests pass, the version is checked, and the delivery builds; how the stand is prepared is left to the project.

# Searched variants

## The project writes the test environment in the workflow

**Selected.**

### Description
Each workflow asset marks one place in the `test-kind` job, between `make init` and the test step. A project adds there the steps that start its services and gives the job the `env` its tests read. Triggers, jobs, conditions, and `make` calls stay as shipped.

### Benefits
- One rule instead of a new target and a convention for connection settings.
- A project with unusual needs — two databases, an emulator, a seeded bucket — is not blocked by the skill.
- The rest of the file is still the same in every project, so a fix in the skill is still a copy.

### Costs
- The files are no longer byte-identical; an update from the skill re-adds the project's block by hand.
- Starting the services is not behind a locally runnable command; a mistake in it shows only on a runner.

## A make target for test services

### Description
The workflow calls `make test-services` after `make init`; the project implements it, and its `Makefile` gives the connection settings a default for CI.

### Benefits
- Both workflow files stay byte-identical.
- The same target starts the services on a developer's machine.

### Costs
- A new target in the caller contract, a default no-op in the shared `testing.mk`, and a rule for how connection settings reach the tests without the job's `env`.
- More machinery than the problem needs — rejected as too elaborate.
