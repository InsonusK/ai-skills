# Dockerfile snippet: `_FILE` convention + image description label

Add these lines to the service's `Dockerfile` so every environment variable
can be supplied through a mounted secret file, and so the built image's
registry description points at the deploy skill.

```dockerfile
# --- near the top: identify the image and point at its deploy instructions ---
LABEL org.opencontainers.image.description="Deploy instructions: skills/devops/deploy-{service-name}.skill/deploy-{service-name}.skill.md"
LABEL org.opencontainers.image.source="https://github.com/{org}/{repo}"

# --- near the end, after the app is copied in ---
COPY docker-entrypoint.sh /usr/local/bin/docker-entrypoint.sh
RUN chmod +x /usr/local/bin/docker-entrypoint.sh

ENTRYPOINT ["docker-entrypoint.sh"]
CMD ["{start-command}"]
```

`org.opencontainers.image.description` is read by GHCR (and most other OCI
registries) to populate the package's description on the registry page, so
the deploy skill's location becomes visible directly from the image without
opening the repository. See [`docker-entrypoint.example.sh`](./docker-entrypoint.example.sh)
for the script that resolves `{NAME}_FILE` variables before starting the
process; `{start-command}` is the service's own start command, one array item per argument — `["dotnet", "{service-name}.dll"]`, `["/{service-name}"]` for a Go binary. The service repository's own root `README.md` must also link to
`skills/devops/deploy-{service-name}.skill/deploy-{service-name}.skill.md`
so a human lands on the same instructions.
