---
name: mep-github
description: >
  Referencia de GitHub para el proyecto MEP (AulaIA). Documenta cómo autenticarse con
  GitHub CLI, dónde vive el repositorio, cómo agregar/actualizar secrets y variables de
  Actions, y el estado de los secrets requeridos por el workflow de deploy.
  Triggers: gh secret, github actions, secrets, variables, gh auth, gh cli, repo github,
  workflow secrets, ACR_USERNAME, AZURE_CREDENTIALS, AZURE_WEBAPP_PUBLISH_PROFILE.
applyTo: "**/*.yml"
---

# mep-github — GitHub CLI y Secrets del proyecto AulaIA

## 1. Repositorio

| Campo | Valor |
|-------|-------|
| Repo | `ezekiell1988/mep` |
| Rama principal | `main` |
| URL | https://github.com/ezekiell1988/mep |
| Settings → Secrets | https://github.com/ezekiell1988/mep/settings/secrets/actions |
| Settings → Variables | https://github.com/ezekiell1988/mep/settings/variables/actions |
| Actions | https://github.com/ezekiell1988/mep/actions |

---

## 2. Autenticación GitHub CLI

```bash
# Ver cuenta activa
gh auth status

# Login (si no está logueado)
gh auth login

# Cuenta activa: ezekiell1988 (keyring)
# Protocol: https
# Token scopes: gist, read:org, repo, workflow
```

---

## 3. Secrets de GitHub Actions

### Ver secrets actuales
```bash
gh secret list --repo ezekiell1988/mep
gh variable list --repo ezekiell1988/mep
```

### Agregar / actualizar un secret
```bash
# Desde valor directo
gh secret set NOMBRE_SECRET --body "valor" --repo ezekiell1988/mep

# Desde archivo
gh secret set AZURE_WEBAPP_PUBLISH_PROFILE < publish-profile.xml --repo ezekiell1988/mep
```

### Agregar / actualizar una variable (no secreta)
```bash
gh variable set NOMBRE_VAR --body "valor" --repo ezekiell1988/mep
```

---

## 4. Estado de secrets requeridos por `deploy.yml`

| Secret / Variable | Tipo | Estado | Descripción |
|-------------------|------|--------|-------------|
| `AZURE_WEBAPP_PUBLISH_PROFILE` | Secret | ✅ Existe | Publish profile del App Service `app-demo-api` |
| `AZURE_SCM_USERNAME` | Secret | ✅ Existe | SCM username del App Service |
| `AZURE_SCM_PASSWORD` | Secret | ✅ Existe | SCM password del App Service |
| `ACR_LOGIN_SERVER` | **Variable** | ⏳ Pendiente | `acrdemoitqs.azurecr.io` |
| `ACR_USERNAME` | Secret | ⏳ Pendiente | `acrdemoitqs` |
| `ACR_PASSWORD` | Secret | ⏳ Pendiente | Admin password del ACR |
| `AZURE_TENANT_ID` | Secret | ✅ Existe | `2f80d4e1-da0e-4b6d-84da-30f67e280e4b` |
| `AZURE_SUBSCRIPTION_ID` | Secret | ✅ Existe | `d9a8cd11-1beb-4255-a890-72797ac44a61` |
| `AZURE_CREDENTIALS` | Secret | ⏳ **Pendiente (bloqueante)** | JSON de Service Principal — ver sección 6 |

### Comandos para agregar los pendientes
```bash
# Variable pública (no secreta)
gh variable set ACR_LOGIN_SERVER --body "acrdemoitqs.azurecr.io" --repo ezekiell1988/mep

# Secrets de ACR e identidad Azure
gh secret set ACR_USERNAME --body "acrdemoitqs" --repo ezekiell1988/mep
gh secret set ACR_PASSWORD --body "<ACR_ADMIN_PASSWORD>" --repo ezekiell1988/mep
gh secret set AZURE_TENANT_ID --body "2f80d4e1-da0e-4b6d-84da-30f67e280e4b" --repo ezekiell1988/mep
gh secret set AZURE_SUBSCRIPTION_ID --body "d9a8cd11-1beb-4255-a890-72797ac44a61" --repo ezekiell1988/mep

# AZURE_CREDENTIALS: una vez que ITQS entregue el JSON del Service Principal:
gh secret set AZURE_CREDENTIALS --repo ezekiell1988/mep < credentials/azure-sp.json
```

---

## 5. Flujo del workflow `deploy.yml`

El workflow se dispara en cada push a `main` o manualmente.

```
push a main
  ├── Build Next.js (npm ci + npm run build → out/)
  ├── Build .NET (dotnet publish → publish/)
  ├── Copia out/ → publish/wwwroot/
  ├── Deploy App Service (zip → azure/webapps-deploy)       ← siempre
  └── Si ACR_LOGIN_SERVER está configurado:
        ├── docker buildx build --platform linux/amd64 → push ACR
        ├── az login (AZURE_USERNAME + AZURE_PASSWORD)
        └── az containerapp update → ca-aulaia-api           ← deploy Container App
```

> ⚠️ **Nota:** `ACR_LOGIN_SERVER` es una **variable** (no secret) — en el workflow se lee con `vars.ACR_LOGIN_SERVER`. Las demás son secrets y se leen con `secrets.*`.

---

## 6. Obtener Service Principal (AZURE_CREDENTIALS)

La cuenta `ebaltodano@itqscr.com` tiene **MFA corporativo** → `az login --username/--password` no funciona en CI. Se necesita un Service Principal creado por alguien con permisos de Azure AD (admin de ITQS).

### Pedir al admin de ITQS que ejecute:
```bash
az login  # con su cuenta admin
az ad sp create-for-rbac \
  --name "sp-aulaia-github-actions" \
  --role Contributor \
  --scopes /subscriptions/d9a8cd11-1beb-4255-a890-72797ac44a61/resourceGroups/rg-ezequiel \
  --sdk-auth
```
El JSON resultante tiene este formato:
```json
{
  "clientId": "...",
  "clientSecret": "...",
  "subscriptionId": "d9a8cd11-1beb-4255-a890-72797ac44a61",
  "tenantId": "2f80d4e1-da0e-4b6d-84da-30f67e280e4b",
  ...
}
```
Guardarlo en `credentials/azure-sp.json` (NO commitear) y subirlo a GitHub:
```bash
gh secret set AZURE_CREDENTIALS --repo ezekiell1988/mep < credentials/azure-sp.json
```

### Mientras no existe el SP: deploy manual desde local
El CI ya pushea la imagen a ACR automáticamente. Solo hay que actualizar el Container App desde local:
```bash
# Después de que el Action termine:
az containerapp update \
  --name ca-aulaia-api \
  --resource-group rg-ezequiel \
  --image acrdemoitqs.azurecr.io/aulaia-api:latest
```
