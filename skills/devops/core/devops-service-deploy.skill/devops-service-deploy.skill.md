---
name: devops-service-deploy
description: What a service repository must contain so that the service can be deployed by anyone — its own deploy skill with filled Docker Compose, Docker Stack, and Kubernetes or Helm examples, a health-check endpoint, secrets suppliable as files, and a documented configuration contract
whenToUse: when you create a service or change anything its deployment depends on — image, ports, environment variables, secrets, dependencies, migrations — and when you review a service repository's deploy skill
updated: 20261010
tags:
  - stack
  - app-type/service
  - concern/ci
  - concern/documentation
  - docker
  - kubernetes
  - helm
adr:
  - adr/use-concern-devops-tag.md
---

# Goal
- **The service's deploy skill** - `skills/devops/deploy-{service-name}.skill/` in the service repository, filled from this skill's template: the deploy skill, a Compose file, a Stack manifest, and either Kubernetes manifests or a Helm chart, each with a step-by-step guide.
- `docker-entrypoint.sh` at the repository root, an unchanged copy of this skill's asset, wired into the `Dockerfile`.
- A `/health-check` endpoint that every deployment path probes.
- `.env.example` at the repository root for the dev container, and `.env` in `.gitignore`.
- The root `README.md` and the image label pointing at the deploy skill.

# Core Principle
- **Deployment knowledge lives with the code** - How a service is deployed is written in its own repository and changed in the same pull request as the code, not kept in a wiki or an agent's memory.
- **Three paths, three purposes** - Docker Compose runs the service locally, Docker Stack runs it on one or several hosts without a cluster, Kubernetes runs it replicated in a cluster.
- **One secret mechanism** - Every variable can be given as a file, so a Docker secret, a Swarm secret, and a Kubernetes Secret reach the process the same way.
- The examples are filled with the service's real names, ports, and variables — a reader copies commands, not a pattern.
- The skill is tagged `concern/ci`; decision recorded in [[./adr/use-concern-devops-tag.md|use-concern-devops-tag]].

# Rule

## MUST

### Create the deploy skill from the template
Fill and copy the folder [[./templates/deploy-{service-name}.skill.template/deploy-{service-name}.skill.template.md|deploy-{service-name}.skill.template]] to `skills/devops/deploy-{service-name}.skill/` in the service repository, renaming the skill file to `deploy-{service-name}.skill.md` and replacing every placeholder of this table.

| Placeholder | Replace with |
| --- | --- |
| `{service-name}` | the service's name, as in its image and manifests |
| `{registry}` | the image registry and owner, such as `ghcr.io/{org}` |
| `{version}` | the version the examples deploy, never `latest` |
| `{namespace}` | the Kubernetes namespace |
| `{host-port}` | the port published on the host |
| `{date}` | the day the deploy skill is written, `YYYYMMDD` |

- Violation: deployment described only in the README, or the template copied with `{service-name}` left in it.
- Risk: every deployment depends on what one person remembers, and a copied command fails on a literal placeholder.
- Fix: copy the folder, fill it, and search the result for `{`; update it in every pull request that changes a deployment fact.

### Replace the sample configuration with the service's own
Replace the sample variables of the templates — `APP_ENVIRONMENT`, `LOG_LEVEL`, `CONNECTION_STRING`, the database dependency — with the variables, secrets, volumes, and dependencies the service really has, and state in each guide what it assumes about the environment.
- Violation: a service without a database whose Compose file still starts one, or a guide that assumes a locally built image without saying so.
- Risk: the examples drift from the service, and the first deployment by someone else fails.
- Fix: derive every value from the `Dockerfile` and the service's configuration code; list a local registry, a pre-created namespace, or a required CLI context before the first command.

### Keep the template's folder grouping
Keep the examples grouped as the template has them — `templates/docker/compose/`, `templates/docker/stack/`, `templates/docker/` for what both share, and `templates/k8s/` or `templates/helm/`.
- Violation: every example file flat in `templates/`.
- Risk: an operator applies a Stack manifest with `docker compose`, or the reverse, by taking the wrong file.
- Fix: restore the folders of the template.

### One Kubernetes path
Keep either `templates/k8s/` or `templates/helm/` — the one the service deploys with — and delete the other.
- Violation: plain manifests and a chart both kept "to be safe".
- Risk: the two drift apart the first time only one is updated.
- Fix: delete the unused folder; prefer the chart when the service already deploys with Helm.

### Give Docker Stack its own manifest
Keep `docker-stack.example.yml` a separate file with a `deploy:` block and Swarm secrets, never the Compose file reused.
- Violation: `docker stack deploy -c docker-compose.yml`.
- Risk: `docker stack deploy` silently ignores `build`, `env_file`, `restart`, `network_mode`, and `depends_on`, so the stack runs with the wrong configuration and no error.
- Fix: fill the Stack manifest of the template and its own deploy guide.

### Expose /health-check and probe it on every path
Implement an HTTP `/health-check` endpoint that returns 200 once the process and its dependencies are ready, and point the Compose and Stack `healthcheck` and the Kubernetes `livenessProbe` and `readinessProbe` at it.
- Violation: a manifest that starts the service with no health check, or one probing another path.
- Risk: an orchestrator cannot tell a hung process from a healthy one, and a rolling update routes traffic to a replica that never became ready.
- Fix: add the endpoint; keep the probes of the templates.

### Apply migrations in exactly one mode
When the service has a schema migration, apply it either as a one-shot job that finishes before the service starts, or at the service's own startup behind a flag — the second only for a deployment that can never have more than one instance.

| Path | Job mode |
| --- | --- |
| Docker Compose | a `migrate` service; the service `depends_on` it with `condition: service_completed_successfully` |
| Docker Stack | `docker-stack-migrate.example.yml`, deployed to the same stack first and waited for — Swarm ignores `depends_on` |
| Kubernetes manifests | a `Job`, applied and waited for with `kubectl wait --for=condition=complete` before the Deployment |
| Helm | the chart's `pre-install,pre-upgrade` hook Job |

- Violation: the service migrates on every start with replicas above one, or a job and a startup migration are both wired "as a safety net".
- Risk: every replica attempts the migration on every restart; with both modes a skipped job is silently healed by the service, and the broken pipeline is never noticed.
- Fix: default to the job; choose startup migration only where the deploy skill states the deployment is pinned to one instance, and remove the job there.

### Drive the chart's migration mode from one value
In a Helm chart, render the migration Job and set the Deployment's `MIGRATE_ON_START` from the same `migrate.mode` value, and run `helm lint` and `helm template` in both modes.
- Violation: one value that enables the Job and another that sets `MIGRATE_ON_START`.
- Risk: a values override sets them inconsistently, and both modes run at once.
- Fix: keep the template chart's `migrate.mode` (`job` or `onStart`) as the only switch.

### Resolve NAME_FILE variables in the entrypoint
Copy [[./assets/docker-entrypoint.sh|docker-entrypoint.sh]] verbatim to the repository root and add these lines to the `Dockerfile`, with the service's start command in `CMD`.
```dockerfile
LABEL org.opencontainers.image.description="Deploy instructions: skills/devops/deploy-{service-name}.skill/deploy-{service-name}.skill.md"
COPY docker-entrypoint.sh /usr/local/bin/docker-entrypoint.sh
RUN chmod +x /usr/local/bin/docker-entrypoint.sh
ENTRYPOINT ["docker-entrypoint.sh"]
```
- Violation: the service reads `CONNECTION_STRING` only, with no way to give it as `CONNECTION_STRING_FILE`.
- Risk: a plain variable shows in `docker inspect` and the process list, and a secret mounted as a file — what Docker, Swarm, and Kubernetes all provide — cannot be used without a code change.
- Fix: the entrypoint exports `NAME` from the file `NAME_FILE` names, for every variable, before it starts the process.

### Document the configuration contract
List every environment variable, secret, and volume the service needs — purpose, default, and whether it is a secret — in the configuration table of the deploy skill, and group the secrets in `templates/docker/env.example` under a `# --- SECRET ---` heading, given through `NAME_FILE`.
- Violation: `CONNECTION_STRING=` listed among ordinary settings.
- Risk: the service starts with a missing value and fails at run time, or an operator pastes a real secret into a file that is committed.
- Fix: one table in the deploy skill; the marked section in `env.example`.

### Never commit a real secret
Keep only placeholder values in every example and in `.env.example`.
- Violation: a working connection string in `k8s-secret.example.yml`.
- Risk: the credential is in the repository's history and in every copy of the example.
- Fix: a placeholder; the real value is created in the secret store at deploy time.

### Pin the image tag
Use an explicit version tag in every production example.
- Violation: `image: {registry}/{service-name}:latest` in a Stack or Kubernetes manifest.
- Risk: a rollback cannot name the previous build, and replicas started at different times run different code.
- Fix: `{registry}/{service-name}:{version}`.

### Keep a root .env.example for the dev container
Fill and copy [[./templates/root-env.example|root-env.example]] to `.env.example` at the repository root, and add `.env` to `.gitignore`.
- Violation: a committed `.env`, or no `.env.example`.
- Risk: a new contributor's dev container starts the service with wrong settings, or a locally filled `.env` with real credentials is committed.
- Fix: development defaults only in `.env.example`; `.env` ignored, `!.env.example` kept.

### Point the README at the deploy skill
Link `skills/devops/deploy-{service-name}.skill/deploy-{service-name}.skill.md` from the root `README.md`.
- Risk: a person never learns the deploy skill exists; the image label of [[#Resolve NAME_FILE variables in the entrypoint]] covers whoever has only the image.
- Fix: one line in the README's deployment section.

## SHOULD

### Limits and labels in Kubernetes
Set resource requests and limits, a namespace, and the standard `app.kubernetes.io/*` labels in the Kubernetes manifests or chart.

### A helper for the common commands
Wrap the most used deploy commands in `Makefile` targets.

## MAY

### Both Kubernetes paths during a migration
Keep plain manifests and a chart side by side while the service moves from one to the other, and say so in the deploy skill.

### Further tooling
Add a Kustomize overlay or a `skaffold.yaml` for local Kubernetes development.

# Check list
- [ ] `skills/devops/deploy-{service-name}.skill/` exists; searching it for `{` finds no placeholder.
- [ ] The examples name the service's real variables, ports, and dependencies; no sample variable is left.
- [ ] The folders are `templates/docker/compose/`, `templates/docker/stack/`, `templates/docker/`, and one of `templates/k8s/` or `templates/helm/`.
- [ ] The Stack manifest is its own file with a `deploy:` block.
- [ ] `/health-check` exists, and every `healthcheck` and probe calls it.
- [ ] A migration runs in exactly one mode per deployment; a chart derives it from `migrate.mode` and passes `helm lint` and `helm template` in both modes.
- [ ] `docker-entrypoint.sh` is byte-identical to this skill's asset, and the `Dockerfile` has the four lines.
- [ ] Every variable, secret, and volume is listed; secrets are marked in `env.example`.
- [ ] No real secret is committed; no production example uses `latest`.
- [ ] Root `.env.example` exists and `.env` is ignored.
- [ ] The root `README.md` links the deploy skill.
