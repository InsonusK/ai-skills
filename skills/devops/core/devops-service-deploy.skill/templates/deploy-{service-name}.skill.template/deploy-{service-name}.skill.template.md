---
name: deploy-{service-name}
description: How to deploy {service-name} with Docker Compose for local development, Docker Stack on one or several hosts, and Kubernetes — the configuration it needs, the commands, and how to verify the result
whenToUse: when you deploy, update, roll back, or troubleshoot {service-name}, or change its image, ports, environment variables, secrets, or dependencies
updated: {date}
tags:
  - stack
  - app-type/service
  - concern/ci
  - docker
  - kubernetes
---

# Goal
- {service-name} running from the image `{registry}/{service-name}:{version}` on the chosen path, answering `/health-check` with 200.
- Every secret of [[#Supply the configuration of this table]] created in the platform's secret store, none in a file of the repository.

# Core Principle
- The service runs as one container built from the repository's `Dockerfile`; configuration reaches it through environment variables and mounted secret files.
- **Three paths** - Docker Compose for local development, Docker Stack for one or several hosts without a cluster, Kubernetes for a replicated deployment.
- The examples in `templates/` are this service's real files; change them in the pull request that changes what they describe.

# Rule

## MUST

### Deploy a built, versioned image
Build the image from the repository's `Dockerfile`, tag it `{registry}/{service-name}:{version}`, and deploy that tag.
- Violation: deploying `latest`, or an image built before the last change.
- Risk: the running container does not match the code, and a rollback cannot name the previous build.
- Fix: `docker build -t {registry}/{service-name}:{version} .`, push, then deploy.

### Follow the guide of the path
Deploy with the commands of the path's guide, in order, verification included.
- Violation: `docker stack deploy` with the Compose file, or `kubectl apply` of a single manifest from memory.
- Risk: a step the guide has for a reason — the migration job, the secret — is skipped, and the service starts broken.
- Fix: the guide linked under [[#Example]] for Docker Compose, Docker Stack, or Kubernetes.

### Supply the configuration of this table
Give the service every variable of this table, each secret created in the platform's secret store and passed as the path of a mounted file in the variable's `_FILE` form.

| Variable | Purpose | Default | Secret |
| --- | --- | --- | --- |
| `APP_ENVIRONMENT` | runtime environment | `production` | no |
| `LOG_LEVEL` | log verbosity | `info` | no |
| `CONNECTION_STRING` | database connection | — | yes, given as `CONNECTION_STRING_FILE` |

Volumes: none. Depends on: a PostgreSQL database.

- Violation: `CONNECTION_STRING=...` typed into a manifest or a committed `.env`.
- Risk: the service starts with a missing value and fails at run time, or a secret shows in `docker inspect`, the process list, and the repository's history.
- Fix: a Docker or Swarm secret, or a Kubernetes Secret, mounted as a file; `docker-entrypoint.sh` reads it.

### Let the migration finish before the service starts
Run the migration job of the path and wait for it to complete before the service is started or updated.
- Violation: the service deployed while the migration job is still running, or after it failed.
- Risk: the new code runs against the old schema.
- Fix: the wait step of the guide — on Docker Stack the job is deployed and waited for separately, because Swarm ignores `depends_on`.

### Verify with /health-check
After every deploy or update, confirm that `/health-check` returns 200 on the deployed service.
- Risk: a container that started and a service that works are not the same thing.
- Fix: the verification commands at the end of each guide.

# Example
- Docker Compose: [[./templates/docker/compose/docker-compose.example.yml|docker-compose.example.yml]], [[./templates/docker/compose/docker-compose-deploy.example.md|guide]].
- Docker Stack: [[./templates/docker/stack/docker-stack.example.yml|docker-stack.example.yml]], [[./templates/docker/stack/docker-stack-migrate.example.yml|docker-stack-migrate.example.yml]], [[./templates/docker/stack/docker-stack-deploy.example.md|guide]].
- Shared by both: [[./templates/docker/env.example|env.example]].
- Kubernetes manifests: [[./templates/k8s/k8s-deploy.example.md|guide]] and the `k8s-*.example.yml` files beside it.
- Helm: [[./templates/helm/helm-deploy.example.md|guide]] and the chart in `templates/helm/chart-example/`.

# Check list
- [ ] The deployed image tag is `{version}`, built from the current code.
- [ ] Every secret exists in the secret store and is passed as a `_FILE` variable.
- [ ] The migration job completed before the service started.
- [ ] `/health-check` returns 200 on the deployed service.
