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

### 3. Verify the deployment
```bash
docker compose ps
docker compose logs --tail=50 {service-name}
curl http://localhost:{host-port}/health
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
