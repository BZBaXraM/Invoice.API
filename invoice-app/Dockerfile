# syntax=docker/dockerfile:1

FROM node:22-bookworm-slim AS build
WORKDIR /app

# Angular's build-time SSR route extraction (used to prerender the auth
# pages) shells out to a real `node` binary — Bun's own `node` compat shim
# isn't sufficient for it, so both toolchains are needed here even though
# `bun install`/`bun run` (matching the project's bun.lock) drive the build.
RUN apt-get update \
    && apt-get install -y --no-install-recommends curl unzip ca-certificates \
    && curl -fsSL https://bun.sh/install | bash \
    && ln -s /root/.bun/bin/bun /usr/local/bin/bun \
    && rm -rf /var/lib/apt/lists/*

COPY package.json bun.lock ./
RUN bun install --frozen-lockfile

COPY . .

# The API base URL is compile-time config in Angular (environment.ts). The
# default keeps the local docker-compose behavior (direct cross-origin calls
# to the host-mapped HTTPS port); CI overrides it with the public domain,
# where the API is same-origin behind Caddy under /api (deploy/Caddyfile).
ARG API_URL=https://localhost:7030/api
RUN sed -i "s|https://localhost:7030/api|${API_URL}|" src/environments/environment.ts

RUN bun run build

FROM nginx:alpine AS runtime
COPY --from=build /app/dist/invoice-app/browser /usr/share/nginx/html
# With "outputMode: server" (angular.json), the browser output only contains
# index.html for the prerendered auth routes — every RenderMode.Client route
# (everything past login) is normally shelled dynamically by the Node SSR
# server. Since nginx serves static files only, the un-rendered server-side
# shell template (just `<app-root>` + script tags, no server-injected state)
# is used as the root index.html so nginx's SPA fallback has something to serve.
COPY --from=build /app/dist/invoice-app/server/index.server.html /usr/share/nginx/html/index.html
COPY nginx.conf /etc/nginx/conf.d/default.conf

EXPOSE 80 443
