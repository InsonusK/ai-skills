---
name: deploy-{service-name}
description: Deploy {service-name} with Docker Compose, Docker Stack (Swarm), and Kubernetes.
whenToUse: when you need to deploy, update, or troubleshoot {service-name} in Docker Compose, Docker Stack, or Kubernetes.
tags:
  - stack
  - app-type/service
  - concern/ci
  - concern/documentation
  - docker
  - docker-compose
  - docker-stack
  - kubernetes
  - k8s
  - deployment
  - devops
---

# Goal
- Deploy {service-name} for local development with Docker Compose.
- Deploy {service-name} consistently on a single/multi-host with Docker Stack (Swarm).
- Deploy {service-name} consistently in a Kubernetes cluster.
- Provide reusable configuration templates and concrete command examples.

# Core Principle
- The service is deployed as a container built from the repository's Dockerfile.
- Configuration is injected through environment variables and mounted files; secrets are never baked into the image or passed as plain environment variables — every variable supports the `{NAME}_FILE` convention (see [`docker-entrypoint.example.sh`](./templates/docker/docker-entrypoint.example.sh)).
- Docker Compose is used for local development; Docker Stack (Swarm) is used for single/multi-host production without a Kubernetes cluster; Kubernetes is used for cluster-scale replicated production.
- The repository root's `.env.example` (gitignored `.env`) is the devcontainer's configuration contract; it is separate from this skill's own `.env.example` used for Compose/Stack.
- Templates are grouped by what applies them, not dumped flat: `templates/docker/compose/`, `templates/docker/stack/`, `templates/docker/` (shared Docker files), and either `templates/k8s/` (plain manifests) or `templates/helm/` (a chart) — whichever this service actually deploys with, not both.

# Rule

## MUST
- Build the image from the repository's Dockerfile before deploying.
  - Violation: deploying without building, or deploying an image built from stale source.
  - Risk: the running container does not match the current code, causing unpredictable failures or silent regressions.
  - Fix: run `docker build` or `docker compose build` from the repository root and verify the produced image tag before starting the deployment.
- Implement a `/health-check` endpoint in the service, and wire every deployment path's health check / probe to it.
  - Violation: the service has no `/health-check` route, or a Compose `healthcheck`, Stack `healthcheck`, or Kubernetes `livenessProbe`/`readinessProbe` is missing or points at a different path.
  - Risk: without it, Compose's restart policy, Swarm's rolling update, and Kubernetes' rolling update all lose the only signal that tells them a replica is actually ready — a hung or half-started process gets treated as healthy.
  - Fix: return 200 from `/health-check` once the process and its dependencies are ready, and point `docker-compose.yml`'s `healthcheck`, `docker-stack.example.yml`'s `healthcheck`, and `k8s-deployment.example.yml`'s `livenessProbe`/`readinessProbe` at it.
- Provide a `docker-compose.yml` with the service, health check, restart policy, and required dependencies.
  - Violation: the Docker Compose file is missing, lacks a health check, or omits a dependency.
  - Risk: the service starts without required dependencies, crashes silently, or cannot be restarted automatically after a failure.
  - Fix: include every dependency, a restart policy, and a health check endpoint in `docker-compose.yml`.
- Provide a `docker-stack.example.yml` (or equivalent) with a `deploy:` block and Swarm secrets, distinct from the Compose file.
  - Violation: reusing `docker-compose.yml` as the stack file — `docker stack deploy` silently ignores `build`, `env_file`, `restart`, and `network_mode`.
  - Fix: give the stack manifest its own `deploy:` policy and `secrets:` block, and its own `docker stack deploy` / `docker service update` / `docker stack rm` command sequence.
- Provide either plain Kubernetes manifests (Deployment, Service, ConfigMap, Secret) or a Helm chart — whichever {service-name} actually deploys with, never both as the steady state.
  - Violation: the Kubernetes manifests/chart are incomplete or missing a required resource; both a manifest set and a chart exist for the same service outside a deliberate, temporary migration between them.
  - Risk: the workload cannot be scheduled, reached, or configured correctly in the cluster; maintaining both paths for one service means they silently drift once only one gets updated.
  - Fix, plain manifests: create Deployment, Service, ConfigMap, and Secret manifests tuned for this service; add Ingress or HPA when exposed or scaled. Fix, Helm: build a real, `helm lint`-clean chart from [`templates/helm/chart-example/`](./templates/helm/chart-example/) — see that skill's own "Helm chart coverage" rule.
- If {service-name} has a database migration step, apply it via exactly one of Job mode or MigrateOnStart mode — never both, and MigrateOnStart only if this deployment is single-instance.
  - Violation: {service-name} runs its own migration unconditionally at startup, or with no documented instance-count justification; a manifest starts the app with no documented migration step ahead of it; or both a Job and a startup call are wired at once "for safety."
  - Risk: migrating from the app's own startup on anything but a genuinely single-instance deployment means every replica attempts it concurrently on every restart (not just deploys); wiring both modes at once doesn't corrupt data but silently masks a skipped/misconfigured Job step instead of failing loudly. See `solution-go-db-migrations.skill/adr/migration-mode-per-platform.md` for the full reasoning.
  - Fix: default to Job mode — Compose `depends_on: condition: service_completed_successfully`; Stack — deploy a `replicated-job` service separately and wait for it (`docker stack deploy` ignores `depends_on`); plain-manifest Kubernetes — a `Job` applied and waited on via `kubectl wait`; Helm chart — the chart's own `migrate-job.yaml`, a `pre-install,pre-upgrade` hook that blocks natively (see `templates/helm/chart-example/`, whose `values.yaml` `migrate.mode` key is the single source of truth for both the Job's rendering condition and the Deployment's `MIGRATE_ON_START`). Switch to MigrateOnStart only for a deployment explicitly pinned to a single instance.
- Document every environment variable and its source, marking secrets explicitly.
  - Violation: environment variables, secrets, or volume mounts are undocumented, only described informally, or a secret is listed the same way as a non-secret value.
  - Risk: operators deploy the service with missing configuration and it fails at runtime, or a secret is mishandled the same as ordinary config.
  - Fix: list every variable, its purpose, default value if any, whether it comes from a ConfigMap or Secret, and — in `.env.example` — group secret variables under a `# --- SECRET ---` marker consumed via `{NAME}_FILE`.
- Support the `{NAME}_FILE` convention for every environment variable, resolved in the entrypoint before the process starts.
  - Violation: the service only reads plain environment variables, with no file-based alternative.
  - Fix: copy [`docker-entrypoint.example.sh`](./templates/docker/docker-entrypoint.example.sh) into the service repository and wire it into the Dockerfile as shown in [`dockerfile-snippet.example.md`](./templates/docker/dockerfile-snippet.example.md).
- Provide a repository-root `.env.example` for the devcontainer, and gitignore the root `.env`.
  - Violation: the devcontainer has no documented configuration contract, or a filled `.env` is committed.
  - Fix: keep [`root-env.example`](./templates/root-env.example) at the repository root in sync with the devcontainer's expected variables, and add `.env` to the root `.gitignore`.
- Link the README to this skill, and set the Dockerfile's `LABEL org.opencontainers.image.description` to the same pointer.
  - Violation: the deploy skill exists but nothing in the README or the built image points to it.
  - Fix: add the link to the repository's root `README.md` and the `LABEL` line from [`dockerfile-snippet.example.md`](./templates/docker/dockerfile-snippet.example.md) to the Dockerfile.
- Group `templates/` by what applies them: `templates/docker/` for Docker (with `compose/` and `stack/` subfolders when their examples differ) and either `templates/k8s/` or `templates/helm/` for Kubernetes.
  - Violation: `docker-compose.example.yml`, `docker-stack.example.yml`, and `k8s-deployment.example.yml` all sitting flat in `templates/`; a Helm chart's files sitting inside `templates/k8s/` instead of their own `templates/helm/`.
  - Fix: lay out `templates/docker/compose/`, `templates/docker/stack/`, `templates/docker/` (shared: `env.example`, `docker-entrypoint.example.sh`, `dockerfile-snippet.example.md`), and whichever of `templates/k8s/` or `templates/helm/chart-example/` this service actually uses.

## SHOULD
- Include health checks and restart policies in the Docker Compose template.
- Include resource requests and limits in Kubernetes Deployment manifests.
- Include a namespace and label conventions in Kubernetes manifests.
- Provide separate compose files for development and production (`docker-compose.yml` and `docker-compose.override.yml` / `docker-compose.prod.yml`).
- Provide a `Makefile` or `deploy.sh` helper script that wraps the most common deploy commands.
- Pin image tags explicitly instead of using `latest`.

## MAY
- Provide both a Helm chart and plain manifests only during a deliberate, temporary migration between them.
- Provide a Kustomize overlay.
- Provide a `skaffold.yaml` for local Kubernetes development.

# Example
- `templates/docker/compose/`: [`docker-compose.example.yml`](./templates/docker/compose/docker-compose.example.yml), [Docker Compose deploy guide](./templates/docker/compose/docker-compose-deploy.example.md).
- `templates/docker/stack/`: [`docker-stack.example.yml`](./templates/docker/stack/docker-stack.example.yml), [`docker-stack-migrate.example.yml`](./templates/docker/stack/docker-stack-migrate.example.yml) (migration job, when applicable), [Docker Stack deploy guide](./templates/docker/stack/docker-stack-deploy.example.md).
- `templates/docker/` (shared): [`env.example`](./templates/docker/env.example), [`docker-entrypoint.example.sh`](./templates/docker/docker-entrypoint.example.sh), [`dockerfile-snippet.example.md`](./templates/docker/dockerfile-snippet.example.md).
- `templates/k8s/` (plain-manifest path): [`k8s-deployment.example.yml`](./templates/k8s/k8s-deployment.example.yml), [`k8s-service.example.yml`](./templates/k8s/k8s-service.example.yml), [`k8s-configmap.example.yml`](./templates/k8s/k8s-configmap.example.yml), [`k8s-secret.example.yml`](./templates/k8s/k8s-secret.example.yml), [`k8s-ingress.example.yml`](./templates/k8s/k8s-ingress.example.yml), [`k8s-migrate-job.example.yml`](./templates/k8s/k8s-migrate-job.example.yml) (migration job, when applicable), [Kubernetes deploy guide](./templates/k8s/k8s-deploy.example.md).
- `templates/helm/` (Helm path — use instead of `templates/k8s/`): [`chart-example/`](./templates/helm/chart-example/) — full chart (`Chart.yaml`, `values.yaml`, `templates/*.yaml`, `migrate-job.yaml` as a `pre-install,pre-upgrade` hook), [Helm deploy guide](./templates/helm/helm-deploy.example.md).
- Devcontainer contract: [`root-env.example`](./templates/root-env.example) (copied to the repository root).

# Check list
- [ ] Image is built from the repository Dockerfile.
- [ ] The service implements `/health-check`, and every Compose `healthcheck`, Stack `healthcheck`, and Kubernetes `livenessProbe`/`readinessProbe` calls it.
- [ ] `templates/` is grouped as `templates/docker/compose/`, `templates/docker/stack/`, `templates/docker/` (shared), and either `templates/k8s/` or `templates/helm/`.
- [ ] Docker Compose file exists and starts the service.
- [ ] Docker Stack manifest exists, has its own `deploy:`/`secrets:` block, and `docker stack deploy` applies cleanly.
- [ ] Kubernetes manifests exist and apply cleanly, or the Helm chart is `helm lint`-clean and `helm template` renders correctly in both migration modes.
- [ ] If {service-name} has a migration step, exactly one mode applies it (Job by default, MigrateOnStart only if explicitly single-instance) — never both; on a Helm chart, both the Job's render condition and `MIGRATE_ON_START` derive from the same `values.yaml` key.
- [ ] All environment variables are documented, with secrets marked in `.env.example`.
- [ ] Repository-root `.env.example` exists for the devcontainer, and root `.env` is gitignored.
- [ ] Every environment variable supports the `{NAME}_FILE` convention.
- [ ] Root `README.md` links to this skill; Dockerfile sets `LABEL org.opencontainers.image.description` to the same pointer.
- [ ] No real secret values are committed in examples.
