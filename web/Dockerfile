# syntax=docker/dockerfile:1.7
# Multi-stage Dockerfile for the TechQuiz web client.
# Build context is the `web/` directory (everything we need is here).
#
# Build locally:   docker build -t techquiz-web ./web
# Used by:         docker-compose.yml (service `web`)

# ─── Build stage ─────────────────────────────────────────────────────────────
FROM node:20-alpine AS build
WORKDIR /src

# Enable pnpm via Node 20's built-in corepack.
RUN corepack enable && corepack prepare pnpm@9 --activate

# Cache deps layer separately from source.
COPY package.json pnpm-lock.yaml ./
RUN pnpm install --frozen-lockfile

COPY . .
RUN pnpm build

# ─── Runtime stage ───────────────────────────────────────────────────────────
FROM nginx:alpine AS runtime

# SPA fallback config (try_files $uri /index.html).
COPY nginx.conf /etc/nginx/conf.d/default.conf

# Built static assets.
COPY --from=build /src/dist /usr/share/nginx/html

EXPOSE 80

# Default nginx ENTRYPOINT/CMD is fine.
