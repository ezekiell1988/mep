---
name: mep-azure-infra
description: >
  Referencia de infraestructura Azure del proyecto MEP (AulaIA). Documenta cómo autenticarse
  en el tenant correcto, dónde están todos los recursos del proyecto, sus nombres reales,
  y comandos Az CLI frecuentes para operar sobre ellos.
  Usar cuando se necesite conectar a Azure, verificar el estado de recursos, hacer deploy
  de imágenes Docker, configurar Container Apps, o consultar dónde vive algo en la nube.
  Triggers: az login, tenant, resource group, container app, acr, app service, postgresql,
  vm, key vault, storage, deploy, docker push, azure infra, rg-ezequiel.
applyTo: "**/*.{sh,yml,yaml}"
---

# mep-azure-infra — Infraestructura Azure del proyecto AulaIA

## 1. Autenticación

### Tenant correcto
El tenant de AulaIA es **distinto** al de otras cuentas (ej. clickeat). Siempre verificar antes de operar.

```bash
# Ver cuenta activa
az account show --query "{tenant:tenantId, subscription:name, user:user.name}" --output table

# Si el tenant NO es 2f80d4e1-..., hacer login:
az login --tenant 2f80d4e1-da0e-4b6d-84da-30f67e280e4b

# Verificar suscripciones disponibles en el tenant:
# [1] ITQS-Sponsorship-Cloud       7515e871-2a0a-40ae-a52b-339cce86c58b  ← default
# [2] Sponsorship-DEV-ITQS         d9a8cd11-1beb-4255-a890-72797ac44a61
# [3] Suscripción Visual Studio    2f35028e-b25e-4321-b11c-159769846e06
```

**Tenant ID:** `2f80d4e1-da0e-4b6d-84da-30f67e280e4b`
**Subscription:** `Sponsorship-DEV-ITQS` — ID: `d9a8cd11-1beb-4255-a890-72797ac44a61`
**Resource Group:** `rg-ezequiel`

---

## 2. Inventario de recursos

| Recurso | Nombre | Tipo | Región | Notas |
|---------|--------|------|--------|-------|
| Resource Group | `rg-ezequiel` | — | — | Contiene todo |
| VM | `demo-itqs` | Standard_DS1_v2 Linux | eastus | Aloja PostgreSQL |
| PostgreSQL | en `demo-itqs` | PostgreSQL (auto-gestionado) | eastus | DB: `aulaia`, user: `demoadmin` |
| Storage Account | `stdemomep` | Blob Storage LRS | eastus | Contenedores: planeamientos, reportes, exportaciones, adjuntos, plantillas, pagos, curriculum |
| Key Vault | `kv-demomep` | Standard | eastus | RBAC habilitado |
| Container Registry | `acrdemoitqs` | ACR | eastus | Login: `acrdemoitqs.azurecr.io` |
| Container Apps Env | `cae-demo-itqs` | Managed Environment | eastus | Cert gestionado para `itqs.ezekl.com` |
| Container App AulaIA | `ca-aulaia-api` | Container App | eastus | ⏳ Pendiente de crear — imagen: `acrdemoitqs.azurecr.io/aulaia-api:latest` |
| Managed Identity | `id-demo-itqs` | User-Assigned | eastus | Roles: AcrPull, KV Secrets User, Storage Blob Contributor |
| App Service | `app-demo-api` | B1 Linux .NET 10 | eastus | Plan: `asp-demomep`. Activo pero reemplazado por Container App |
| AI Foundry | `demo-itqs` + `demo-itqs-proj` | Cognitive Services | eastus | GPT-5.5, gpt-image-2, sora-2, gpt-realtime |
| AI Foundry (2) | `ebalt-mif64qfm-centralus` | Cognitive Services | centralus | Endpoint alternativo para realtime |

**Dominio:** `mep.ezekl.com` (web) · `api.mep.ezekl.com` (API) — DNS en Cloudflare

---

## 3. Variables de entorno de la API (.NET)

Estas son las variables que deben estar en el Container App (las mismas que tiene `app-demo-api`):

| Variable | Descripción |
|----------|-------------|
| `Database__ConnectionString` | Host=172.191.128.24;Port=5432;Database=aulaia;Username=demoadmin;SslMode=Prefer |
| `Auth__Authority` | https://aulaia-mep.us.auth0.com/ |
| `Auth__Audience` | https://api.aulaia.mep.go.cr |
| `Storage__ConnectionString` | Connection string de `stdemomep` |
| `PowerSync__InstanceUrl` | https://69f98b0463989ab5d2ed2a3b.powersync.journeyapps.com |
| `PowerSync__SigningKey` | (secreto) |
| `PowerSync__KeyId` | aulaia-v1 |
| `AI__Endpoint` | https://demo-itqs-resource.openai.azure.com |
| `AI__ApiKey` | (secreto) |
| `AI__DeploymentChat` | gpt-5.5 |
| `AI__DeploymentImage` | gpt-image-2 |
| `AI__DeploymentVideo` | sora-2 |
| `AI__DeploymentRealtime` | gpt-realtime |
| `Sinpe__PhoneNumber` | (número SINPE de AulaIA) |
| `Sinpe__AccountName` | AulaIA |

---

## 4. Docker — Build para Azure (linux/amd64 obligatorio)

> ⚠️ En Mac Apple Silicon el build por defecto produce `arm64`, que NO funciona en Azure Container Apps.
> **Siempre** usar `--platform linux/amd64`.

```bash
# Build correcto para Azure
docker buildx build \
  --platform linux/amd64 \
  -t acrdemoitqs.azurecr.io/aulaia-api:latest \
  -t acrdemoitqs.azurecr.io/aulaia-api:$(git rev-parse --short HEAD) \
  --push \
  .

# Login al ACR antes del push
az acr login --name acrdemoitqs

# Verificar plataforma de la imagen
docker inspect acrdemoitqs.azurecr.io/aulaia-api:latest --format '{{.Architecture}} / {{.Os}}'
# Debe decir: amd64 / linux
```

---

## 5. Comandos frecuentes

```bash
# Listar todos los recursos del proyecto
az resource list --resource-group rg-ezequiel --output table

# Ver estado del Container App
az containerapp show --resource-group rg-ezequiel --name ca-aulaia-api \
  --query "{image:properties.template.containers[0].image, fqdn:properties.configuration.ingress.fqdn, status:properties.runningStatus}" \
  --output table

# Ver logs en tiempo real del Container App
az containerapp logs show --resource-group rg-ezequiel --name ca-aulaia-api --follow

# Actualizar imagen del Container App (deploy)
az containerapp update \
  --resource-group rg-ezequiel \
  --name ca-aulaia-api \
  --image acrdemoitqs.azurecr.io/aulaia-api:latest

# Ver env vars del App Service (referencia)
az webapp config appsettings list --resource-group rg-ezequiel --name app-demo-api --output table

# Listar imágenes en ACR
az acr repository list --name acrdemoitqs --output table
az acr repository show-tags --name acrdemoitqs --repository aulaia-api --output table

# Estado de la VM (PostgreSQL)
az vm get-instance-view --resource-group rg-ezequiel --name demo-itqs \
  --query "instanceView.statuses[1].displayStatus" --output tsv

# Ver secretos del Key Vault
az keyvault secret list --vault-name kv-demomep --output table
```

---

## 6. CI/CD (GitHub Actions)

Archivo: `.github/workflows/deploy.yml`

El workflow hace:
1. Build Next.js → `/out/`
2. Build .NET → `publish/`
3. Copia `/out/` a `publish/wwwroot/`
4. Deploy a **App Service** `app-demo-api` (activo siempre)
5. Si `ACR_LOGIN_SERVER` está configurado → build Docker + push a ACR
6. ⏳ Pendiente: `az containerapp update` al final del workflow

**Secrets requeridos en GitHub:**
- `AZURE_WEBAPP_PUBLISH_PROFILE` — publish profile del App Service
- `ACR_USERNAME` / `ACR_PASSWORD` — credenciales del ACR
- Variable: `ACR_LOGIN_SERVER` = `acrdemoitqs.azurecr.io`
