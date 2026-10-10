# Deploy {service-name} with Docker Stack (Swarm)

## Prerequisites
- Docker Engine >= 24 with Swarm mode initialized on the target node (`docker swarm init`, or the node already joined a swarm).
- The image is built and pushed to a registry every swarm node can pull from — `docker stack deploy` never builds; it only pulls. `build:` in a stack file is silently ignored.
- `.env` next to `docker-stack.example.yml`, created from this skill's [`env.example`](../env.example). The Docker CLI uses it only to interpolate `${VAR}` placeholders in the compose file at parse time — it is **not** injected into the container as `env_file:` would; Swarm mode ignores the `env_file` compose key entirely.
- Secret values are never placed in `.env`. Create each one as a Swarm secret first (see step 2).

## Steps

### 1. Build and push the image
```bash
docker build -t {registry}/{service-name}:{version} ..
docker push {registry}/{service-name}:{version}
```

### 2. Create Swarm secrets (once per environment, before the first deploy)
```bash
printf '%s' "$REAL_CONNECTION_STRING" | docker secret create connection_string -
```
Never pass the secret value as a CLI argument (`docker secret create connection_string "value"`) — it stays in shell history and process listings. Piping via `printf` avoids both.

### 3. Run the migration job and wait for completion (if [`docker-stack-migrate.example.yml`](./docker-stack-migrate.example.yml) applies)
`docker stack deploy` silently ignores `depends_on` entirely (the same limitation noted above for
`env_file`), so the migration job cannot be expressed as a dependency inside
`docker-stack.example.yml` — it has to be deployed as its own step instead, one command ahead of
step 4, and waited on before proceeding:
```bash
docker stack deploy -c docker-stack-migrate.example.yml --with-registry-auth {service-name}

until docker service ps --filter desired-state=Shutdown --format '{{.CurrentState}}' \
    {service-name}_migrate 2>/dev/null | grep -q '^Complete'; do
  sleep 2
done
```
If the job instead reports `Failed`, stop here — do not proceed to step 4 against a database that
never finished migrating.

This targets the **same stack name** (`{service-name}`) step 4 deploys to — it is one stack, not
two. `docker stack deploy` never removes a service just because a *later* deploy's file omits it
(that needs the explicit `--prune` flag, never used here), so `migrate` simply stays in the stack,
completed and costing nothing, after step 4 adds the app service alongside it. On the next deploy
that carries new migrations, re-running this step (with the new image tag) triggers a fresh job run
the same way.

### 4. Deploy the stack
```bash
docker stack deploy -c docker-stack.example.yml --with-registry-auth {service-name}
```

### 5. Verify the deployment
```bash
docker stack services {service-name}
docker stack ps {service-name}
docker service logs --tail=50 {service-name}_{service-name}
curl http://localhost:{host-port}/health-check
```

## Update to a new version
If the new version adds migrations, repeat step 3 with the new image tag and wait for `Complete`
before updating the app service — the same "job finishes first" rule applies to every deploy, not
only the first one.
```bash
docker service update --image {registry}/{service-name}:{version} {service-name}_{service-name}
```
`update_config.order: start-first` (set in [`docker-stack.example.yml`](./docker-stack.example.yml)) starts the new replica before stopping the old one, and `failure_action: rollback` reverts automatically if the new replica never becomes healthy.

## Roll back
```bash
docker service rollback {service-name}_{service-name}
```

## Remove the stack
```bash
docker stack rm {service-name}
```
