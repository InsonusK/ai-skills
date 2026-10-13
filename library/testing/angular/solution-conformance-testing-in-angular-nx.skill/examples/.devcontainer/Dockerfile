FROM node:24-bookworm
ENV PLAYWRIGHT_BROWSERS_PATH=/ms-playwright
RUN apt-get update && apt-get install -y --no-install-recommends make jq \
    && npx --yes playwright@1.64.0 install --with-deps chromium \
    && rm -rf /var/lib/apt/lists/*
