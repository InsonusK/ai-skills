# Deploy {service-name} with Docker Compose

## Prerequisites
- Docker Engine >= 24
- Docker Compose plugin (`docker compose`)
- `.env` file created from `.env.example`
- Ports `{host-port}` and dependencies available on the host

## Steps

### 1. Build the image
```bash
docker compose build
```

### 2. Start the service
```bash
docker compose up -d
```
If [`docker-compose.example.yml`](./docker-compose.example.yml) defines a `migrate` service, Compose
runs it to completion first — `{service-name}`'s `depends_on: migrate: condition:
service_completed_successfully` blocks it from starting until `migrate` exits `0`. No separate step
needed; unlike Docker Stack and Kubernetes, Compose enforces this ordering natively.

### 3. Verify the deployment
```bash
docker compose ps
docker compose logs --tail=50 {service-name}
curl http://localhost:{host-port}/health-check
```

## Update to a new version
```bash
docker compose pull {service-name}
docker compose up -d {service-name}
```

## Stop the service
```bash
docker compose down
```
