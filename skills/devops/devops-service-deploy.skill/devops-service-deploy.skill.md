---
name: devops-service-deploy
description: Require every service repository to contain a deploy skill that describes how to deploy the service with Docker Compose, Docker Stack (Swarm), and Kubernetes, including configuration templates, secret handling, devcontainer setup, and deployment examples.
whenToUse: when you are creating or updating a service and need to produce deployment documentation and artifacts for Docker Compose, Docker Stack, and Kubernetes.
tags:
  - stack
  - app-type/service
  - concern/ci
  - concern/documentation
  - docker
  - kubernetes
  - k8s
adr:
  - adr/use-concern-devops-tag.md
---

# Goal
- Every service repository must contain its own deployment skill so that any agent or operator can deploy the service consistently.
- The deployment skill must cover Docker Compose (local dev), Docker Stack / Swarm (single/multi-host production), and Kubernetes (cluster) deployment paths.
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
- **Templates grouped by application, not dumped flat** - Inside the deploy skill's `templates/` folder, group example files by what applies them: `templates/docker/` for everything Docker reads, `templates/k8s/` for everything `kubectl`/Kubernetes reads. Inside `templates/docker/`, give Docker Compose and Docker Stack their own subfolders (`templates/docker/compose/`, `templates/docker/stack/`) whenever their examples differ — which they always do, since a stack manifest and a compose file are never the same file (see "Docker Stack coverage").
  - Violation: `templates/docker-compose.example.yml`, `templates/docker-stack.example.yml`, and `templates/k8s-deployment.example.yml` all sitting flat in the same `templates/` folder.
  - Risk: a flat folder mixing Compose, Stack, and Kubernetes files forces an operator to read every filename to figure out which platform an example belongs to, and makes it easy to apply a Stack manifest with `docker compose` (or vice versa) by grabbing the wrong file.
  - Fix: lay out `templates/` as:
    ```
    templates/
      docker/
        compose/   # docker-compose.example.yml, docker-compose-deploy.example.md
        stack/     # docker-stack.example.yml, docker-stack-deploy.example.md
        env.example, docker-entrypoint.example.sh, dockerfile-snippet.example.md
      k8s/         # k8s-*.example.yml, k8s-deploy.example.md
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
- **Kubernetes coverage** - The deployment skill must contain Kubernetes manifests or a Helm chart for the service.
  - Risk: cluster deployments are inconsistent or require manual recreation of resources.
  - Fix: provide Deployment, Service, ConfigMap, and Secret manifests (and Ingress / HPA when applicable).
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
- Provide a Helm chart in addition to plain manifests.
- Provide a Kustomize overlay.
- Provide a `skaffold.yaml` for local Kubernetes development.

# Example
See the templates in [./templates](./templates), grouped the same way the produced deploy skill must group them:
- [`service-deploy-skill-template.md`](./templates/service-deploy-skill-template.md) — skeleton for the deployment skill the agent must create in the service repository.
- `templates/docker/compose/`: [`docker-compose.example.yml`](./templates/docker/compose/docker-compose.example.yml) — Docker Compose template for a generic service; [`docker-compose-deploy.example.md`](./templates/docker/compose/docker-compose-deploy.example.md) — step-by-step Docker Compose deploy guide.
- `templates/docker/stack/`: [`docker-stack.example.yml`](./templates/docker/stack/docker-stack.example.yml) — Docker Stack (Swarm) manifest with `deploy:` policy and Swarm secrets; [`docker-stack-deploy.example.md`](./templates/docker/stack/docker-stack-deploy.example.md) — step-by-step Docker Stack deploy guide.
- `templates/docker/` (shared): [`env.example`](./templates/docker/env.example) — `.env.example` for Compose/Stack with secret variables clearly marked; [`docker-entrypoint.example.sh`](./templates/docker/docker-entrypoint.example.sh) — entrypoint script resolving the `{NAME}_FILE` convention; [`dockerfile-snippet.example.md`](./templates/docker/dockerfile-snippet.example.md) — Dockerfile snippet wiring in the entrypoint and the image description label.
- [`root-env.example`](./templates/root-env.example) — repository-root `.env.example` used by the devcontainer (not Docker- or Kubernetes-specific, so it stays outside both groups).
- `templates/k8s/`: [`k8s-deployment.example.yml`](./templates/k8s/k8s-deployment.example.yml) — Deployment template; [`k8s-service.example.yml`](./templates/k8s/k8s-service.example.yml) — Service template; [`k8s-configmap.example.yml`](./templates/k8s/k8s-configmap.example.yml) — ConfigMap template; [`k8s-secret.example.yml`](./templates/k8s/k8s-secret.example.yml) — Secret template; [`k8s-ingress.example.yml`](./templates/k8s/k8s-ingress.example.yml) — Ingress template; [`k8s-deploy.example.md`](./templates/k8s/k8s-deploy.example.md) — step-by-step Kubernetes deploy guide.

# Check list
- [ ] A deployment skill exists at `skills/devops/deploy-{service-name}.skill/deploy-{service-name}.skill.md`.
- [ ] The service implements `/health-check`, and every Compose `healthcheck`, Stack `healthcheck`, and Kubernetes `livenessProbe`/`readinessProbe` calls it.
- [ ] `templates/` is grouped as `templates/docker/compose/`, `templates/docker/stack/`, `templates/docker/` (shared Docker files), and `templates/k8s/` — never a flat folder mixing Compose, Stack, and Kubernetes files.
- [ ] The skill contains a Docker Compose configuration template and a deployment example.
- [ ] The skill contains a Docker Stack (Swarm) manifest, distinct from the Compose file, and a `docker stack deploy` example.
- [ ] The skill contains Kubernetes manifests (Deployment, Service, ConfigMap, Secret) and a deployment example.
- [ ] Environment variables, secrets, and volume mounts are documented, with secret variables visibly marked in the `.env.example`.
- [ ] The repository root contains a `.env.example` with devcontainer base settings, and root `.env` is gitignored.
- [ ] The service supports the `{NAME}_FILE` convention for every environment variable (entrypoint resolves it before the app starts).
- [ ] The root `README.md` links to the deploy skill, and the Dockerfile sets `LABEL org.opencontainers.image.description` to the same pointer.
- [ ] No real secret values are committed in examples.
- [ ] Image tags are pinned or the use of `latest` is explicitly justified.
- [ ] Verification commands are included in Docker Compose, Docker Stack, and Kubernetes examples.
