# syntax=docker/dockerfile:1.7
# Multi-stage Dockerfile for TechQuiz.Api.
# Build context must be the repository root (we need access to all csproj files
# for `dotnet restore` to work in a multi-project solution).
#
# Build locally:   docker build -f src/TechQuiz.Api/Dockerfile -t techquiz-api .
# Used by:         docker-compose.yml (service `api`)

# ─── Build stage ─────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:9.0-alpine AS build
WORKDIR /src

# Copy solution + global.json + all csproj files first to maximize layer cache.
# `dotnet restore` only re-runs when these files change.
COPY global.json TechQuiz.sln ./
COPY src/TechQuiz.Domain/TechQuiz.Domain.csproj         src/TechQuiz.Domain/
COPY src/TechQuiz.Application/TechQuiz.Application.csproj src/TechQuiz.Application/
COPY src/TechQuiz.Infrastructure/TechQuiz.Infrastructure.csproj src/TechQuiz.Infrastructure/
COPY src/TechQuiz.Api/TechQuiz.Api.csproj               src/TechQuiz.Api/

RUN dotnet restore src/TechQuiz.Api/TechQuiz.Api.csproj

# Copy the rest of the source and publish.
COPY src/ src/
RUN dotnet publish src/TechQuiz.Api/TechQuiz.Api.csproj \
    --configuration Release \
    --no-restore \
    --output /app/publish \
    /p:UseAppHost=false

# ─── Runtime stage ───────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:9.0-alpine AS runtime
WORKDIR /app

# Run as non-root for least-privilege.
# The aspnet:9.0 image ships with a pre-created `app` user (UID/GID 1654).
USER app

COPY --from=build --chown=app:app /app/publish ./

ENV ASPNETCORE_URLS=http://+:8080 \
    ASPNETCORE_ENVIRONMENT=Production \
    DOTNET_RUNNING_IN_CONTAINER=true \
    DOTNET_NOLOGO=true

EXPOSE 8080

ENTRYPOINT ["dotnet", "TechQuiz.Api.dll"]
