# Container App: ca-aulaia-api

**Creado:** 2026-05-08
**Estado:** ✅ Running

## Datos del recurso

| Campo | Valor |
|-------|-------|
| Nombre | `ca-aulaia-api` |
| Resource Group | `rg-ezequiel` |
| Environment | `cae-demo-itqs` |
| FQDN | `ca-aulaia-api.whitewater-319185f7.eastus.azurecontainerapps.io` |
| Región | eastus |
| CPU | 0.5 vCPU |
| Memoria | 1.0 Gi |
| Réplicas | min: 1 / max: 1 |
| Puerto interno | 8080 |
| Ingress | external (HTTPS) |

## Imagen

| Campo | Valor |
|-------|-------|
| Registry | `acrdemoitqs.azurecr.io` |
| Imagen | `aulaia-api` |
| Tag inicial | `latest` + `d33a2b5` (git sha) |
| Plataforma | `linux/amd64` ← **obligatorio para Azure desde Mac Apple Silicon** |

## Autenticación al registry

Se usa **admin credentials del ACR** (no Managed Identity para registry) porque el usuario `ebaltodano@itqscr.com` no tiene permisos para hacer `roleAssignments/write` sobre el ACR.

```
--registry-username acrdemoitqs
--registry-password <admin-password>   # guardada como secret en el Container App
```

La Managed Identity `id-demo-itqs` está asignada al Container App (`--user-assigned`) para otros accesos (Key Vault, Storage), pero **no** para el pull del ACR.

## Variables de entorno configuradas

| Variable | Valor |
|----------|-------|
| `ASPNETCORE_ENVIRONMENT` | `Production` |
| `Database__ConnectionString` | `Host=172.191.128.24;Port=5432;Database=aulaia;Username=demoadmin;SslMode=Prefer` |
| `Auth__Authority` | `https://aulaia-mep.us.auth0.com/` |
| `Auth__Audience` | `https://api.aulaia.mep.go.cr` |
| `Storage__ConnectionString` | connection string de `stdemomep` |
| `PowerSync__InstanceUrl` | `https://69f98b0463989ab5d2ed2a3b.powersync.journeyapps.com` |
| `PowerSync__SigningKey` | (secreto) |
| `PowerSync__KeyId` | `aulaia-v1` |
| `AI__Endpoint` | `https://demo-itqs-resource.openai.azure.com` |
| `AI__ApiKey` | (secreto) |
| `AI__DeploymentChat` | `gpt-5.5` |
| `AI__DeploymentImage` | `gpt-image-2` |
| `AI__DeploymentVideo` | `sora-2` |
| `AI__DeploymentRealtime` | `gpt-realtime` |
| `Sinpe__PhoneNumber` | `88888888` ⚠️ placeholder — cambiar antes de producción real |
| `Sinpe__AccountName` | `AulaIA` |

## Comando usado para crear

```bash
IDENTITY_ID=$(az identity show --resource-group rg-ezequiel --name id-demo-itqs --query id --output tsv)

az containerapp create \
  --name ca-aulaia-api \
  --resource-group rg-ezequiel \
  --environment cae-demo-itqs \
  --image acrdemoitqs.azurecr.io/aulaia-api:latest \
  --registry-server acrdemoitqs.azurecr.io \
  --registry-username acrdemoitqs \
  --registry-password "<admin-password>" \
  --user-assigned "$IDENTITY_ID" \
  --target-port 8080 \
  --ingress external \
  --min-replicas 1 \
  --max-replicas 1 \
  --cpu 0.5 \
  --memory 1.0Gi \
  --env-vars "..."
```

## Comando para actualizar imagen (deploy)

```bash
az containerapp update \
  --name ca-aulaia-api \
  --resource-group rg-ezequiel \
  --image acrdemoitqs.azurecr.io/aulaia-api:latest
```

## Pendientes

- [ ] Configurar dominio custom `api.mep.ezekl.com` → apuntar a este FQDN en Cloudflare
- [ ] Actualizar `deploy.yml` para hacer `az containerapp update` al final del workflow
- [ ] Cambiar `Sinpe__PhoneNumber` por el número real antes de lanzamiento
- [ ] Verificar que el endpoint `/health` o `/` responde correctamente
