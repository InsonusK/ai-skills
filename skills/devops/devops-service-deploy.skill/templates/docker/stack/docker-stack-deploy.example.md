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

### 3. Deploy the stack
```bash
docker stack deploy -c docker-stack.example.yml --with-registry-auth {service-name}
```

### 4. Verify the deployment
```bash
docker stack services {service-name}
docker stack ps {service-name}
docker service logs --tail=50 {service-name}_{service-name}
curl http://localhost:{host-port}/health-check
```

## Update to a new version
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
