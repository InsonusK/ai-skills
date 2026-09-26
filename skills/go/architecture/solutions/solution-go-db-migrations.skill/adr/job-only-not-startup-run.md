---
name: job-only-not-startup-run
description: Whether Migrate may be called from the app's own startup path, or only from a separate deploy-time job
problem: An earlier version of this solution offered two call sites for Migrate — cmd/{service}/main.go's own startup path, or a separate cmd/migrate deploy-time job — and left the choice between them to each team, documented per plateau. The catalog owner asked for concrete selection criteria between the two, then asked to drop the startup-run option entirely in favor of always requiring a separate job gated ahead of the app.
decision: cmd/migrate is the only caller of Migrate. cmd/{service}/main.go is never touched by this solution. The job/container that runs cmd/migrate is gated ahead of the app (re)starting via skills/devops/devops-service-deploy.skill/devops-service-deploy.skill.md's own "migration step" rule, on every platform that skill covers (Docker Compose, Docker Stack, Kubernetes).
tags:
  - solution/go-db-migrations
  - concern/documentation
  - concern/documentation/adr
  - stack/go
---

> **Superseded** by
> [[./migration-mode-per-platform.md|adr/migration-mode-per-platform.md]]. This ADR's own reasoning
> (concurrency risk, deploy-pipeline-as-failure-signal, Twelve-Factor's Admin Processes factor)
> still holds and is carried forward there — what changed is that the owner's real deployment
> topology (Docker Stack always single-replica; Kubernetes always via Helm, where a Job is "free" to
> gate on) makes MigrateOnStart mode safe for part of that topology, so the newer ADR reinstates it
> as a second, conditionally-safe mode instead of removing it outright. Kept here, unedited, per
> `adr-create`'s own "record the rejected options" principle — this was the real reasoning at the
> time, not a mistake to erase.

# Problem

The first version of this solution supported two call sites for the same `Migrate(ctx, dsn) error`
function: `cmd/{service}/main.go`'s own `run()`, before constructing `Store` (startup-run), or a
standalone `cmd/migrate` binary invoked by the deploy pipeline (deploy-job). The solution left the
choice between them to whichever team applied it, documented per plateau, with no stated criteria
for making that choice — the catalog owner pointed out this gap directly. Asked to think it through,
the owner then proposed dropping startup-run entirely: migrations always run as a separate job,
gated so the app container cannot start until the job completes, on the reasoning that this scales
better under load, removes any need to reason about concurrent-migration races, and costs nothing
to keep once the job has completed (a finished Job/task, unlike a running replica, holds no ongoing
resource).

# Selected variant

**Selected variant:** [[#Deploy-time job only, gated ahead of the app (selected)]]

# Searched variants

## Deploy-time job only, gated ahead of the app (selected)

### Description

`cmd/migrate` is the only path that ever calls `Migrate`. `cmd/{service}/main.go` is left exactly as
`solution-persistent-db` created it — this solution's `Implementation/` no longer touches it at all.
Making the job actually run, and complete, before the app (re)starts is
`devops-service-deploy.skill.md`'s own concern: its "migration step" MUST rule requires every
service with a migration step to wire it as a one-shot container/Job ahead of the app, per platform
— Docker Compose's native `depends_on: condition: service_completed_successfully`; Docker Stack's
`replicated-job` mode deployed and waited on as its own stack, since `docker stack deploy` silently
ignores `depends_on`; Kubernetes's `Job` applied and waited on via `kubectl wait`, or a Helm
`pre-install,pre-upgrade` hook.

### Benefits

- One call site, not two — nothing to choose, document, or get out of sync per plateau; the
  ambiguity the owner flagged is structurally impossible instead of merely discouraged.
- Matches the owner's own stated reasoning: a Job that has completed holds no replica, no memory,
  no CPU reservation between deploys — the closest thing to zero ongoing cost a deploy-time step can
  have, unlike an `initContainer`-per-replica shape (which would still cost N runs per deploy and
  would not have solved the "scales better" goal at all).
- Removes the *need* to reason about concurrent-replica migration races as a startup-path concern —
  there is exactly one job run per deploy, not one attempt per replica. (goose's session lock is
  kept as defense in depth against a retried/duplicated job run, not because concurrent replicas can
  race any more — see this solution's own Workflow.)
- Cleanly separates concerns already split this way elsewhere in this catalog: a Go-architecture
  solution provides the binary/code; a deployment-topology rule in the stack-agnostic
  `devops-service-deploy.skill.md` decides how any service — not just ones built on this catalog —
  gets that binary run ahead of its app container. The same rule now benefits every other service
  repository in this repository, not only ones built from this Go catalog.

### Costs

- Requires the deploying team's pipeline to support "run this to completion, then start that" —
  native in Compose and Kubernetes, but on Docker Stack it means a documented two-step script
  (deploy the job stack, poll for `Complete`, then deploy the app stack), not a single manifest.
  `devops-service-deploy.skill.md`'s own templates carry this cost, not this solution.
- A local `go run ./cmd/{service}` dev loop no longer migrates itself — a developer must also run
  `go run ./cmd/migrate` (or rely on their local Compose setup, which does this automatically via
  `depends_on`) before the app can serve a request that touches the store. A minor, one-time
  dev-loop step, not a production concern.

## Team's choice between startup-run and deploy-job, documented per plateau (rejected — this solution's own earlier design)

### Description

Keep both call sites (`cmd/{service}/main.go`'s startup path and `cmd/migrate`), and let each team
applying this solution pick one, stating the choice in that plateau's own root skill or README.

### Benefits

- A team with no deploy-pipeline job support (e.g. a single-container PaaS with no init-job
  primitive) could still get automatic migration-on-deploy by restarting the app.
- Slightly simpler local dev loop — `go run ./cmd/{service}` alone migrates and serves.

### Costs

- No stated criteria for making the choice — exactly the gap the catalog owner flagged: a solution
  that offers two options with no decision rule is incomplete, not flexible.
- Reintroduces the concurrent-replica migration question as something every team choosing
  startup-run must reason about themselves (mitigated by goose's advisory lock, but still a real
  question a Job-only design removes structurally rather than mitigating).
- Two call sites documented per plateau drift: nothing stops a future edit from wiring both, or
  neither, with no mechanical check catching the inconsistency the way "there is only one caller"
  does.

## Startup-run only (rejected)

### Description

Drop `cmd/migrate` entirely; `Migrate` is always called from `cmd/{service}/main.go`'s own `run()`
before constructing `Store`.

### Benefits

- Simplest possible deployment story — no separate job/container to wire on any platform.

### Costs

- Directly contradicts the catalog owner's second requirement from this solution's very first
  request: migrations must be runnable as a separate deploy-time job, not only bundled into service
  startup.
- Ties migration success to the app process's own crash-loop/restart policy instead of to the
  deploy pipeline's own pass/fail signal — a failed migration surfaces as a crash-looping app
  replica instead of a failed deploy step.
