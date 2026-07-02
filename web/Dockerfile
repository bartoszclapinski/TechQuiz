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

# The API base URL is baked into the bundle at build time (Vite). Render passes the service's
# VITE_API_BASE_URL env var as a build arg, so the deployed SPA points at the deployed API; unset
# locally, the client falls back to its http://localhost:8085 default (ADR-022).
ARG VITE_API_BASE_URL
ENV VITE_API_BASE_URL=$VITE_API_BASE_URL
RUN pnpm build

# ─── Runtime stage ───────────────────────────────────────────────────────────
FROM nginx:alpine AS runtime

# SPA fallback config as a template: the nginx image runs envsubst over /etc/nginx/templates/*.template
# at startup, filling ${PORT} from the environment. PORT defaults to 80 here (so docker-compose serves
# on 80); container hosts like Render inject their own PORT and nginx listens there (ADR-022).
ENV PORT=80
COPY nginx.conf.template /etc/nginx/templates/default.conf.template

# Built static assets.
COPY --from=build /src/dist /usr/share/nginx/html

EXPOSE 80

# Default nginx ENTRYPOINT/CMD is fine (the entrypoint processes templates before starting nginx).
