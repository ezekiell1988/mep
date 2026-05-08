---
name: mep-deploy
description: >
  Pasos para hacer deploy manual de AulaIA al Container App de Azure.
  Cubre build Docker linux/amd64, push al ACR y actualización del Container App.
  Usar cuando se necesite desplegar una nueva versión de la API o el SPA.
  Triggers: deploy, subir cambios, actualizar container app, docker push, acr push,
  nueva versión, publicar, release.
---

# mep-deploy — Deploy manual de AulaIA a Azure

## Requisitos previos

- Docker Desktop corriendo (con soporte BuildKit / buildx)
- `az login` activo en el tenant correcto (`2f80d4e1-da0e-4b6d-84da-30f67e280e4b`)
- Estar en la raíz del repo (`/Users/ezequielbaltodanocubillo/Documents/ezekl/mep`)

Verificar login de Azure:
```bash
az account show --query "{sub:id, tenant:tenantId}" -o json
# Debe mostrar: d9a8cd11-1beb-4255-a890-72797ac44a61 / 2f80d4e1-da0e-4b6d-84da-30f67e280e4b
# Si no: az login --tenant 2f80d4e1-da0e-4b6d-84da-30f67e280e4b
```

---

## Pasos de deploy

### 1. Login al ACR
```bash
az acr login --name acrdemoitqs
```

### 2. Build de la imagen (siempre linux/amd64 — Mac Apple Silicon)
```bash
# Con el SHA del commit actual como tag
SHA=$(git rev-parse --short HEAD)

docker buildx build \
  --platform linux/amd64 \
  --tag acrdemoitqs.azurecr.io/aulaia-api:latest \
  --tag acrdemoitqs.azurecr.io/aulaia-api:$SHA \
  --push \
  .
```

> ⚠️ **Crítico**: siempre usar `--platform linux/amd64`. Sin esto, Mac Apple Silicon genera una imagen `arm64` que no corre en Azure.

### 3. Actualizar el Container App
```bash
az containerapp update \
  --name ca-aulaia-api \
  --resource-group rg-ezequiel \
  --image acrdemoitqs.azurecr.io/aulaia-api:$SHA
```

### 4. Verificar que levantó bien
```bash
FQDN="https://ca-aulaia-api.whitewater-319185f7.eastus.azurecontainerapps.io"

# Health check de la API
curl -s $FQDN/health
# Esperado: {"status":"healthy","version":"1.0.0"}

# SPA — verifica que el index.html carga (Next.js static export)
curl -s -o /dev/null -w "%{http_code}" $FQDN/
# Esperado: 200

# Una ruta interna del SPA (debe devolver 200 con el fallback index.html)
curl -s -o /dev/null -w "%{http_code}" $FQDN/grupos
# Esperado: 200

# Rutas de la API (no deben ir al SPA)
curl -s -o /dev/null -w "%{http_code}" $FQDN/scalar
# Esperado: 200
```

Si alguna devuelve 404, revisar logs:
```bash
az containerapp logs show \
  --name ca-aulaia-api \
  --resource-group rg-ezequiel \
  --tail 50 --follow
```

### 5. Limpiar imágenes antiguas del ACR (conservar últimas 4)
Las revisiones del Container App se nombran `ca-aulaia-api--0000001`, `--0000002`, etc. — incrementan con cada `containerapp update`. Para poder hacer rollback a cualquiera de las últimas 4, conservamos los últimos 4 tags SHA en el ACR y borramos el resto.

```bash
# Ver todos los tags SHA (excluye latest, buildcache)
az acr repository show-tags \
  --name acrdemoitqs \
  --repository aulaia-api \
  --orderby time_desc \
  --output tsv | grep -v "^latest$" | grep -v "buildcache"

# Borrar todos excepto los últimos 4 SHA
TAGS_TO_DELETE=$(az acr repository show-tags \
  --name acrdemoitqs \
  --repository aulaia-api \
  --orderby time_desc \
  --output tsv | grep -v "^latest$" | grep -v "buildcache" | tail -n +5)

for TAG in $TAGS_TO_DELETE; do
  echo "Borrando tag: $TAG"
  az acr repository delete \
    --name acrdemoitqs \
    --image aulaia-api:$TAG \
    --yes
done
echo "Limpieza completada. Tags restantes:"
az acr repository show-tags --name acrdemoitqs --repository aulaia-api --orderby time_desc --output tsv
```

### Rollback a una revisión anterior
```bash
# Ver revisiones disponibles
az containerapp revision list \
  --name ca-aulaia-api \
  --resource-group rg-ezequiel \
  --query "[].{name:name, image:properties.template.containers[0].image, active:properties.active}" \
  -o table

# Activar una revisión anterior (reemplaza la activa)
az containerapp update \
  --name ca-aulaia-api \
  --resource-group rg-ezequiel \
  --image acrdemoitqs.azurecr.io/aulaia-api:<SHA_ANTERIOR>
```

---

## Script completo de deploy + limpieza

```bash
SHA=$(git rev-parse --short HEAD) && \
az acr login --name acrdemoitqs && \
docker buildx build --platform linux/amd64 \
  --tag acrdemoitqs.azurecr.io/aulaia-api:latest \
  --tag acrdemoitqs.azurecr.io/aulaia-api:$SHA \
  --push . && \
az containerapp update \
  --name ca-aulaia-api \
  --resource-group rg-ezequiel \
  --image acrdemoitqs.azurecr.io/aulaia-api:$SHA && \
echo "✓ Deploy $SHA completado" && \
\
FQDN="https://ca-aulaia-api.whitewater-319185f7.eastus.azurecontainerapps.io" && \
sleep 5 && \
echo "API health: $(curl -s $FQDN/health)" && \
echo "SPA /    :  $(curl -s -o /dev/null -w '%{http_code}' $FQDN/)" && \
echo "SPA /grupos: $(curl -s -o /dev/null -w '%{http_code}' $FQDN/grupos)" && \
\
echo "Limpiando imágenes antiguas (conservando últimas 4)..." && \
TAGS_TO_DELETE=$(az acr repository show-tags \
  --name acrdemoitqs --repository aulaia-api \
  --orderby time_desc --output tsv \
  | grep -v "^latest$" | grep -v "buildcache" | tail -n +5) && \
for TAG in $TAGS_TO_DELETE; do
  az acr repository delete --name acrdemoitqs --image aulaia-api:$TAG --yes --output none
  echo "  Borrado: $TAG"
done && \
echo "✓ Listo"
```

---

## Referencia de recursos

| Recurso | Valor |
|---------|-------|
| ACR | `acrdemoitqs.azurecr.io` |
| Container App | `ca-aulaia-api` |
| Resource Group | `rg-ezequiel` |
| FQDN | `ca-aulaia-api.whitewater-319185f7.eastus.azurecontainerapps.io` |
| Container Apps Env | `cae-demo-itqs` (eastus) |
| Puerto interno | `8080` |

---

## Qué hace el Dockerfile

Build multi-stage en 3 etapas:
1. **Node 22-alpine** — `npm ci` + `npm run build` del SPA Next.js → `/web/out/`
2. **dotnet/sdk:10.0** — `dotnet publish` de la API → `/publish/`
3. **dotnet/aspnet:10.0** — runtime final, copia `/publish` + `/web/out` → `wwwroot/`

Un solo contenedor sirve tanto la API como el SPA estático.
