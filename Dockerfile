# ╔══════════════════════════════════════════════════════════════════════════╗
# ║  AulaIA — Dockerfile multi-stage                                         ║
# ║  Produce una imagen única con la API .NET 10 + SPA Next.js en wwwroot   ║
# ╚══════════════════════════════════════════════════════════════════════════╝

# ── Stage 1: Build Next.js SPA ───────────────────────────────────────────
FROM node:22-alpine AS web-build
WORKDIR /web

COPY src/aulaia-web/package*.json ./
RUN npm ci --ignore-scripts

COPY src/aulaia-web/ ./
RUN npm run build
# Resultado: /web/out/  (static export de Next.js)


# ── Stage 2: Build .NET API ──────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS api-build
WORKDIR /src

# Restaurar dependencias en capa separada para aprovechar el cache de Docker
COPY src/AulaIA.Api/AulaIA.Api.csproj ./AulaIA.Api/
RUN dotnet restore AulaIA.Api/AulaIA.Api.csproj

# Compilar y publicar
COPY src/AulaIA.Api/ ./AulaIA.Api/
RUN dotnet publish AulaIA.Api/AulaIA.Api.csproj \
    --configuration Release \
    --output /publish \
    --no-restore

# Eliminar settings de Development del artefacto publicado (seguridad)
RUN rm -f /publish/appsettings.Development.json


# ── Stage 3: Imagen de runtime ───────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

# QuestPDF / SkiaSharp requieren librerías de fuentes en Linux (Debian)
RUN apt-get update \
    && apt-get install -y --no-install-recommends \
       libfontconfig1 \
       fonts-liberation \
    && rm -rf /var/lib/apt/lists/*

# Copiar artefactos de las etapas anteriores
COPY --from=api-build /publish .
COPY --from=web-build /web/out ./wwwroot

# Variables de entorno base — sobrescribir en tiempo de ejecución
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

EXPOSE 8080

ENTRYPOINT ["dotnet", "AulaIA.Api.dll"]
