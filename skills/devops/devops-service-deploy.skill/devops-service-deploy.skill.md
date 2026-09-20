---
name: devops-service-deploy
description: Require every service repository to contain a deploy skill that describes how to deploy the service with Docker Compose, Docker Stack (Swarm), and Kubernetes (plain manifests or a Helm chart), including configuration templates, secret handling, devcontainer setup, and deployment examples.
whenToUse: when you are creating or updating a service and need to produce deployment documentation and artifacts for Docker Compose, Docker Stack, Kubernetes, or a Helm chart.
tags:
  - stack
  - app-type/service
  - concern/ci
  - concern/documentation
  - docker
  - kubernetes
  - k8s
  - helm
adr:
  - adr/use-concern-devops-tag.md
---

# Goal
- Every service repository must contain its own deployment skill so that any agent or operator can deploy the service consistently.
- The deployment skill must cover Docker Compose (local dev), Docker Stack / Swarm (single/multi-host production), and Kubernetes (cluster) deployment paths — Kubernetes itself via either plain manifests or a Helm chart, whichever the service actually uses (see "Kubernetes coverage").
- It must provide ready-to-use configuration templates, a documented secrets contract, and concrete deployment examples.
- The service repository itself must be devcontainer-ready and support supplying any configuration value as a secret file, independent of this skill's own templates.
- The service must expose a `/health-check` endpoint, and every deployment path must use it.

# Core Principle
- Deployment knowledge belongs in the same repository as the service it describes, not in a shared wiki or in an agent's memory.
- The deployment skill is created and updated together with the service code — never left as an afterthought.
- The skill is stack-agnostic in structure but concrete in content: it names real images, ports, volumes, environment variables, and dependencies for this service.
- Docker Compose is the path for local development; Docker Stack (Swarm) is the path for single/multi-host production without a Kubernetes cluster; Kubernetes is the path for multi-host / replicated production at cluster scale.
- Every configuration value the service reads must be suppliable as a file (`{NAME}_FILE`), not only as a plain environment variable — this is what lets the same secret mounted by Docker secrets, Swarm secrets, or a Kubernetes Secret reach the process without a code change per platform.
- This skill is tagged with `concern/ci` because it sits in the DevOps process alongside CI and deployment; see [ADR: use-concern-devops-tag](./adr/use-concern-devops-tag.md) for the trade-offs.

# Rule

## MUST
- **Local deploy skill** - When creating or modifying a service, create or update the deployment skill at `skills/devops/deploy-{service-name}.skill/deploy-{service-name}.skill.md` in the service repository.
  - Risk: without a local deploy skill, every future deployment depends on implicit knowledge and ad-hoc commands.
  - Fix: keep the skill next to the service code and update it whenever deployment-relevant facts change.
- **Templates grouped by application, not dumped flat** - Inside the deploy skill's `templates/` folder, group example files by what applies them: `templates/docker/` for everything Docker reads, `templates/k8s/` for everything plain `kubectl` reads, `templates/helm/` for a Helm chart. Inside `templates/docker/`, give Docker Compose and Docker Stack their own subfolders (`templates/docker/compose/`, `templates/docker/stack/`) whenever their examples differ — which they always do, since a stack manifest and a compose file are never the same file (see "Docker Stack coverage"). `templates/k8s/` and `templates/helm/` are alternatives for the same platform (see "Kubernetes coverage"), not both required — populate whichever one the service actually deploys with.
  - Violation: `templates/docker-compose.example.yml`, `templates/docker-stack.example.yml`, and `templates/k8s-deployment.example.yml` all sitting flat in the same `templates/` folder; a Helm chart's files sitting directly under `templates/k8s/` instead of their own `templates/helm/`.
  - Risk: a flat folder mixing Compose, Stack, and Kubernetes files forces an operator to read every filename to figure out which platform an example belongs to, and makes it easy to apply a Stack manifest with `docker compose` (or vice versa) by grabbing the wrong file; a Helm chart mixed into `templates/k8s/` looks like a second, competing set of plain manifests instead of the chart it actually is.
  - Fix: lay out `templates/` as:
    ```
    templates/
      docker/
        compose/   # docker-compose.example.yml, docker-compose-deploy.example.md
        stack/     # docker-stack.example.yml, docker-stack-migrate.example.yml, docker-stack-deploy.example.md
        env.example, docker-entrypoint.example.sh, dockerfile-snippet.example.md
      k8s/         # k8s-*.example.yml, k8s-deploy.example.md — plain-manifest path
      helm/        # chart-example/ (Chart.yaml, values.yaml, templates/), helm-deploy.example.md — Helm path
    ```
    Files that apply to Docker generally (the shared `.env.example`, the entrypoint script, the Dockerfile snippet) live directly under `templates/docker/`, not inside `compose/` or `stack/`.
- **`/health-check` endpoint is mandatory** - The service must implement an HTTP `/health-check` endpoint, and every deployment path (Docker Compose `healthcheck`, Docker Stack `healthcheck`, Kubernetes `livenessProbe`/`readinessProbe`) must be wired to call it.
  - Violation: a service with no `/health-check` route, or a Compose/Stack/Kubernetes manifest that starts the service with no health check / probe at all.
  - Risk: without a health check, an orchestrator (Compose restart policy, Swarm, Kubernetes) cannot tell a hung or half-started process from a healthy one — Swarm's `start-first` rolling update and Kubernetes' rolling update both rely on the probe to decide whether to route traffic to a replica or roll back, so a missing endpoint silently defeats zero-downtime deploys.
  - Fix: implement `/health-check` in the service (return 200 once the process and its dependencies — database, cache, ... — are ready), and point every `healthcheck`/`livenessProbe`/`readinessProbe` in the deployment skill's templates at it, exactly as shown in [`docker-compose.example.yml`](./templates/docker/compose/docker-compose.example.yml), [`docker-stack.example.yml`](./templates/docker/stack/docker-stack.example.yml), and [`k8s-deployment.example.yml`](./templates/k8s/k8s-deployment.example.yml).
- **Docker Compose coverage** - The deployment skill must contain a `docker-compose.yml` template or a reference to a checked-in `docker-compose.yml` / `docker-compose.prod.yml`.
  - Risk: operators cannot spin up the service locally or on a single host.
  - Fix: provide a complete Docker Compose file with image, ports, environment variables, health checks, volumes, and dependencies.
- **Docker Stack coverage** - The deployment skill must also contain a Docker Stack (Swarm) manifest and deployment example, distinct from the Docker Compose one.
  - Risk: `docker stack deploy` silently ignores keys that plain `docker compose` honors (`build`, `env_file`, `restart`, `network_mode`, ...); reusing the Compose example as-is produces a stack file that deploys with the wrong configuration and no error.
  - Fix: provide a stack-specific compose file with a `deploy:` block (replicas, restart/update policy, resources) and Swarm secrets, plus a `docker stack deploy` / `docker service update` / `docker stack rm` command sequence.
- **Kubernetes coverage — plain manifests or a Helm chart, never both for the same service** - The deployment skill must contain either plain Kubernetes manifests or a Helm chart, whichever the service actually deploys with — pick one path, don't maintain both for the same service.
  - Risk: cluster deployments are inconsistent or require manual recreation of resources; maintaining both a manifest set and a chart for one service means they drift the moment only one gets updated.
  - Fix, plain manifests: provide Deployment, Service, ConfigMap, and Secret manifests (and Ingress / HPA when applicable) — see `templates/k8s/`.
  - Fix, Helm chart: provide a real, installable chart (`Chart.yaml`, `values.yaml`, `templates/` with Deployment/Service/ConfigMap/Secret/Ingress) — see [`templates/helm/chart-example/`](./templates/helm/chart-example/) and its own [`helm-deploy.example.md`](./templates/helm/helm-deploy.example.md). Prefer this path whenever the service already deploys via Helm: a migration Job wired as a `pre-install,pre-upgrade` hook blocks the release natively, with no extra deploy-script step the plain-manifest path needs (see the "migration step" rule below).
- **A migration step runs as a one-shot job, OR at the app's own startup guarded by a single-instance condition — never both, and never unconditionally at startup** - When the service has a database schema migration step, exactly one of two modes applies it: **Job mode** (a one-shot container/Job that completes before the app (re)starts) or **MigrateOnStart mode** (the app calls it once at its own startup, behind an explicit config flag). MigrateOnStart mode is permitted **only** when at most one instance of the migrating process can ever run concurrently for that deployment — the moment more than one replica/instance is possible, Job mode is required.
  - Violation: the application calls its own migration routine unconditionally (not behind a flag), or with the flag left at whatever a multi-replica deployment happens to default to; a Compose/Stack/Kubernetes manifest starts the app with no documented migration step ahead of it; or both a Job step *and* an app-startup call are wired for the same deployment "as a safety net."
  - Risk: migrating from the app's own startup path on anything but a genuinely single-instance deployment means every replica attempts it concurrently on every restart — not only deploys, also crashes, OOM-kills, node evictions, and autoscaler scale-outs — undermining autoscaling specifically by adding a database round-trip and lock attempt to every new replica exactly when it's least wanted. Wiring both a Job **and** a startup call "for safety" does not corrupt data (a session lock still serializes concurrent attempts) but silently masks a broken/misconfigured deploy pipeline — the Job step being accidentally skipped stops failing loudly and instead gets quietly "self-healed" by the app, removing the only signal that the pipeline has drifted — and re-couples migration failure to the app's own crash-loop instead of the pipeline's pass/fail signal for that path. See `skills/go/architecture/solutions/solution-go-db-migrations.skill/adr/migration-mode-per-platform.md` for the full reasoning and a worked example of choosing between the two per platform.
  - Fix: default to Job mode; only switch a specific deployment to MigrateOnStart when that deployment's own topology genuinely guarantees a single instance (state this explicitly in that service's own deploy skill, e.g. a Docker Stack service pinned to `deploy.replicas: 1` — the generic templates below default to `replicas: 2` and therefore default to Job mode too), and never wire both modes for the same deployment. Per platform:
    - **Docker Compose**: always Job mode — a `migrate` service (`restart: "no"`); the app service's `depends_on` names it with `condition: service_completed_successfully` (Compose v2.20+/Engine 25+) — see [`docker-compose.example.yml`](./templates/docker/compose/docker-compose.example.yml). Already free, native, and safe regardless of replica count; there is no reason to ever choose MigrateOnStart here.
    - **Docker Stack (Swarm)**: Job mode — a `migrate` service with `deploy.mode: replicated-job` — **but `docker stack deploy` silently ignores `depends_on` entirely** (the same limitation this skill already documents for `env_file`), so ordering cannot live in one manifest applied in one command. Deploy it first, targeting the *same* stack name the app deploys to (this stays one stack, not two — `docker stack deploy` only removes a service absent from the file you're applying when you pass `--prune`, which this never does), wait until `docker service ps` reports it `Complete`, then deploy the app on top — see [`docker-stack-migrate.example.yml`](./templates/docker/stack/docker-stack-migrate.example.yml) and the wait step in [`docker-stack-deploy.example.md`](./templates/docker/stack/docker-stack-deploy.example.md). MigrateOnStart is an option here **only** for a service explicitly pinned to a single replica — skip the Job manifest entirely and set the app's own `MIGRATE_ON_START=true` instead.
    - **Kubernetes, plain manifests (no Helm)**: Job mode — a `Job` manifest, applied and waited on (`kubectl wait --for=condition=complete`) before the Deployment is applied. See [`k8s-migrate-job.example.yml`](./templates/k8s/k8s-migrate-job.example.yml) and [`k8s-deploy.example.md`](./templates/k8s/k8s-deploy.example.md).
    - **Kubernetes via Helm**: see [Helm chart coverage](#helm-chart-coverage-if-the-service-deploys-with-helm) below — Job mode there is wired as a chart-native `pre-install,pre-upgrade` hook, not a manual manifest.
- **Helm chart coverage (if the service deploys with Helm)** - A Helm-based deployment skill must be a complete, `helm lint`-clean chart under `templates/helm/`, and its migration step (if any) must be the chart's own hook-driven Job, gated by one values key that also controls the Deployment's `MIGRATE_ON_START` — never two independently-set values that could drift apart.
  - Violation: a values-driven flag for the migration mode that isn't the *same* value the Job's rendering condition and the Deployment's env var both read; a chart with no migrate Job at all for a service that has a migration step; a chart that hasn't been run through `helm lint`/`helm template` at least once.
  - Risk: two independent toggles (one deciding whether the Job renders, another setting `MIGRATE_ON_START` on the Deployment) can be set inconsistently by a values override, silently reintroducing "both modes wired at once" — the exact problem a single derived value exists to make structurally impossible, not just documented against.
  - Fix: follow [`templates/helm/chart-example/`](./templates/helm/chart-example/) exactly — `values.yaml`'s `migrate.mode` (`"job"` default or `"onStart"`) is the one source of truth; `templates/migrate-job.yaml` renders only `{{- if eq .Values.migrate.mode "job" }}`, and `templates/deployment.yaml`'s `MIGRATE_ON_START` env var is derived from that same key (`{{ eq .Values.migrate.mode "onStart" | ternary "true" "false" }}`), never set independently. Run `helm lint .` and `helm template .` (in both modes) before considering the chart done — see [`helm-deploy.example.md`](./templates/helm/helm-deploy.example.md).
- **Configuration contract** - The deployment skill must document the exact environment variables, secrets, and volume mounts the service needs.
  - Risk: the service starts with missing configuration and fails at runtime.
  - Fix: list every variable, its purpose, default value (if any), and whether it comes from a Secret or ConfigMap.
- **`.env` example with secrets marked** - The deployment skill must provide an `.env.example` for Docker Compose / Docker Stack that lists every variable and visibly marks which ones are secrets.
  - Violation: an `.env.example` that lists `CONNECTION_STRING=` alongside non-secret variables with no indication it is sensitive.
  - Risk: an operator treats a secret the same as ordinary config, commits a filled-in `.env`, or pastes the real value into a plain `environment:` entry that ends up in `docker inspect` output and shell history.
  - Fix: group secret variables under a clearly labeled section (for example `# --- SECRET ---`) and show them consumed through the `{NAME}_FILE` convention (see "`_FILE` secret-file convention" below) rather than as a plain value.
- **Repository-root `.env.example` for the devcontainer** - The service repository's root must contain a `.env.example` with the base settings needed to run the service inside the devcontainer.
  - Risk: without it, a new contributor's devcontainer fails to start the service or starts it with wrong defaults, and there is no single place documenting the devcontainer's configuration contract.
  - Fix: keep a root-level `.env.example` (see [`root-env.example`](./templates/root-env.example), which stays directly under `templates/` since it belongs to neither the `docker/` nor the `k8s/` group) in sync with the devcontainer's expected variables; a devcontainer may use development-only placeholder secrets since it is a single-developer trusted environment, but staging/production values still never go in it.
- **Root `.env` is gitignored** - The repository's `.gitignore` must ignore the root `.env` file (while still tracking `.env.example`).
  - Violation: `.env` is committed, or `.gitignore` has no entry for it.
  - Risk: a developer's locally filled `.env` — potentially containing real credentials copied in for convenience — gets committed and exposed in version control history.
  - Fix: add `.env` to the repository's root `.gitignore` (an explicit `!.env.example` if the `.gitignore` pattern would otherwise also hide the example file).
- **`_FILE` secret-file convention** - Every environment variable the service reads must be possible to supply as a file instead of a plain value, via a same-named `{NAME}_FILE` variable pointing at the file's path.
  - Violation: the service reads `CONNECTION_STRING` directly with no equivalent `CONNECTION_STRING_FILE` path, forcing every secret into a plain environment variable.
  - Risk: plain environment variables are visible in `docker inspect`, `/proc/<pid>/environ`, and process listings; without the `_FILE` variant, Docker/Swarm/Kubernetes secrets mounted as files (the standard secret-delivery mechanism in all three) cannot be consumed without a code change.
  - Fix: resolve `{NAME}_FILE` to `{NAME}` in the container entrypoint before the application starts, as shown in [`docker-entrypoint.example.sh`](./templates/docker/docker-entrypoint.example.sh), and wire it into the Dockerfile as shown in [`dockerfile-snippet.example.md`](./templates/docker/dockerfile-snippet.example.md).
- **README links to the deploy skill; image carries the same pointer** - The service repository's root `README.md` must link to `skills/devops/deploy-{service-name}.skill/deploy-{service-name}.skill.md`, and the Dockerfile must set `LABEL org.opencontainers.image.description` to the same pointer.
  - Risk: without the README link, a human contributor never discovers the deploy skill exists; without the image label, an operator who only has the built image (no source checkout) has no way to find deployment instructions.
  - Fix: add the link to the README and the `LABEL org.opencontainers.image.description="Deploy instructions: skills/devops/deploy-{service-name}.skill/deploy-{service-name}.skill.md"` line to the Dockerfile, exactly as shown in [`dockerfile-snippet.example.md`](./templates/docker/dockerfile-snippet.example.md).
- **Deployment examples** - The deployment skill must include step-by-step deployment examples for Docker Compose, Docker Stack, and Kubernetes.
  - Risk: operators guess the correct commands and miss flags such as `--build`, `--env-file`, or namespace/context.
  - Fix: provide copy-paste-ready command sequences, including prerequisites and verification steps.
- **Consistent examples** - Keep example values consistent with the service's actual defaults (port, image name, application name, config keys).
  - Risk: examples drift from reality and cause failed first-time deployments.
  - Fix: derive example values from the service's Dockerfile / source configuration and review them when those change.
- **No real secrets in examples** - Never store real secret values inside the skill or example files.
  - Violation: committing a connection string, API key, or certificate into `k8s-secret.example.yml`.
  - Risk: sensitive credentials are exposed in version control and copied into every deployment.
  - Fix: use placeholder values in examples and inject real secrets through a secret-management tool at deploy time.
- **No floating `latest` tag** - Never use `latest` as the only image tag in production-oriented examples without explaining the risk.
  - Violation: `image: "{registry}/{service-name}:latest"` in a production manifest without any warning.
  - Risk: rollbacks become unreliable and different replicas may run different builds.
  - Fix: pin an explicit version tag such as `{version}`; if `latest` is used for a specific reason, document why it is acceptable.
- **No environment-specific instructions without assumptions** - Never write deployment instructions that only work in one specific environment (for example, your laptop) without documenting the assumptions.
  - Violation: a step assumes a locally built image named `{service-name}:dev` without saying so.
  - Risk: another operator runs the same commands in a different environment and gets a failing or inconsistent deployment.
  - Fix: state every assumption such as local registry, pre-created namespace, or required CLI context before the first command.

## SHOULD
- Include health checks and restart policies in the Docker Compose template.
- Include resource requests and limits in Kubernetes Deployment manifests.
- Include a namespace and label conventions in Kubernetes manifests.
- Provide separate compose files for development and production (`docker-compose.yml` and `docker-compose.override.yml` / `docker-compose.prod.yml`).
- Provide a `Makefile` or `deploy.sh` helper script that wraps the most common deploy commands.
- Pin image tags explicitly instead of using `latest`.

## MAY
- Provide both a Helm chart and plain manifests for the same service when a real migration between
  them is in progress — a transitional state, not the steady state "Kubernetes coverage" expects.
- Provide a Kustomize overlay.
- Provide a `skaffold.yaml` for local Kubernetes development.

# Example
See the templates in [./templates](./templates), grouped the same way the produced deploy skill must group them:
- [`service-deploy-skill-template.md`](./templates/service-deploy-skill-template.md) — skeleton for the deployment skill the agent must create in the service repository.
- `templates/docker/compose/`: [`docker-compose.example.yml`](./templates/docker/compose/docker-compose.example.yml) — Docker Compose template for a generic service, including an optional `migrate` service wired via `depends_on: condition: service_completed_successfully`; [`docker-compose-deploy.example.md`](./templates/docker/compose/docker-compose-deploy.example.md) — step-by-step Docker Compose deploy guide.
- `templates/docker/stack/`: [`docker-stack.example.yml`](./templates/docker/stack/docker-stack.example.yml) — Docker Stack (Swarm) manifest with `deploy:` policy and Swarm secrets; [`docker-stack-migrate.example.yml`](./templates/docker/stack/docker-stack-migrate.example.yml) — the `replicated-job` migration service, deployed to the *same* stack name one step ahead of it and waited on, since Swarm ignores `depends_on`; [`docker-stack-deploy.example.md`](./templates/docker/stack/docker-stack-deploy.example.md) — step-by-step Docker Stack deploy guide, including the migration-job wait step.
- `templates/docker/` (shared): [`env.example`](./templates/docker/env.example) — `.env.example` for Compose/Stack with secret variables clearly marked; [`docker-entrypoint.example.sh`](./templates/docker/docker-entrypoint.example.sh) — entrypoint script resolving the `{NAME}_FILE` convention; [`dockerfile-snippet.example.md`](./templates/docker/dockerfile-snippet.example.md) — Dockerfile snippet wiring in the entrypoint and the image description label.
- [`root-env.example`](./templates/root-env.example) — repository-root `.env.example` used by the devcontainer (not Docker- or Kubernetes-specific, so it stays outside both groups).
- `templates/k8s/` (plain-manifest path): [`k8s-deployment.example.yml`](./templates/k8s/k8s-deployment.example.yml) — Deployment template; [`k8s-service.example.yml`](./templates/k8s/k8s-service.example.yml) — Service template; [`k8s-configmap.example.yml`](./templates/k8s/k8s-configmap.example.yml) — ConfigMap template; [`k8s-secret.example.yml`](./templates/k8s/k8s-secret.example.yml) — Secret template; [`k8s-ingress.example.yml`](./templates/k8s/k8s-ingress.example.yml) — Ingress template; [`k8s-migrate-job.example.yml`](./templates/k8s/k8s-migrate-job.example.yml) — Job template for the migration step; [`k8s-deploy.example.md`](./templates/k8s/k8s-deploy.example.md) — step-by-step Kubernetes deploy guide, including the `kubectl wait` step.
- `templates/helm/` (Helm path — use instead of `templates/k8s/`, not alongside it): [`chart-example/`](./templates/helm/chart-example/) — a complete, `helm lint`-clean chart (`Chart.yaml`, `values.yaml`, `.helmignore`, `templates/_helpers.tpl`/`deployment.yaml`/`service.yaml`/`configmap.yaml`/`secret.yaml`/`ingress.yaml`/`migrate-job.yaml`); [`helm-deploy.example.md`](./templates/helm/helm-deploy.example.md) — step-by-step `helm install`/`upgrade`/`rollback`/`uninstall` guide. `values.yaml`'s `migrate.mode` (`job` default / `onStart`) is the single source of truth both `migrate-job.yaml`'s rendering condition and `deployment.yaml`'s `MIGRATE_ON_START` env var derive from — see the "Helm chart coverage" rule above for why that matters.

# Check list
- [ ] A deployment skill exists at `skills/devops/deploy-{service-name}.skill/deploy-{service-name}.skill.md`.
- [ ] The service implements `/health-check`, and every Compose `healthcheck`, Stack `healthcheck`, and Kubernetes `livenessProbe`/`readinessProbe` calls it.
- [ ] `templates/` is grouped as `templates/docker/compose/`, `templates/docker/stack/`, `templates/docker/` (shared Docker files), and either `templates/k8s/` or `templates/helm/` — never a flat folder mixing them, and never both `k8s/` and `helm/` for the same service outside a deliberate, temporary migration between the two.
- [ ] The skill contains a Docker Compose configuration template and a deployment example.
- [ ] The skill contains a Docker Stack (Swarm) manifest, distinct from the Compose file, and a `docker stack deploy` example.
- [ ] The skill contains either Kubernetes manifests (Deployment, Service, ConfigMap, Secret) with a deployment example, or a `helm lint`-clean chart (see `templates/helm/chart-example/`) with a `helm install`/`upgrade` example.
- [ ] If the service has a migration step, exactly one mode applies it per deployment — Job mode (Compose `depends_on: condition: service_completed_successfully`; Stack `replicated-job` deployed and waited on separately; Kubernetes `Job` waited on via `kubectl wait`, or — on a Helm chart — the chart's own `pre-install,pre-upgrade` hook) or MigrateOnStart mode — never both, and MigrateOnStart only where that deployment is explicitly documented as single-instance.
- [ ] On a Helm chart with a migration step: `migrate-job.yaml`'s rendering condition and the Deployment's `MIGRATE_ON_START` env var are both derived from the *same* `values.yaml` key — never two independently-settable values.
- [ ] Environment variables, secrets, and volume mounts are documented, with secret variables visibly marked in the `.env.example`.
- [ ] The repository root contains a `.env.example` with devcontainer base settings, and root `.env` is gitignored.
- [ ] The service supports the `{NAME}_FILE` convention for every environment variable (entrypoint resolves it before the app starts).
- [ ] The root `README.md` links to the deploy skill, and the Dockerfile sets `LABEL org.opencontainers.image.description` to the same pointer.
- [ ] No real secret values are committed in examples.
- [ ] Image tags are pinned or the use of `latest` is explicitly justified.
- [ ] Verification commands are included in Docker Compose, Docker Stack, and Kubernetes examples.
