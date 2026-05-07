# 04 — Tareas Accionables

> **Última actualización:** 2026-05-04
> **Prioridad actual:** Fase 0 — Infraestructura Azure y setup del proyecto
> **Resource Group:** `rg-ezequiel` | **Convención:** prefijo `demo`

---

## TASK-F0-01: Crear Storage Account `stdemo` y contenedores

**Estado:** ⏳ Pendiente
**Módulo:** Infraestructura Azure
**Prerequisitos:** Tener sesión activa con `az login --use-device-code`

### Pasos

```bash
# 1. Crear Storage Account (nombre debe ser único globalmente, solo minúsculas/números)
az storage account create \
  --name stdemo \
  --resource-group rg-ezequiel \
  --location eastus \
  --sku Standard_LRS \
  --kind StorageV2 \
  --min-tls-version TLS1_2 \
  --allow-blob-public-access false

# 2. Crear contenedores (acceso privado — siempre)
az storage container create --name planeamientos --account-name stdemo --auth-mode login
az storage container create --name reportes       --account-name stdemo --auth-mode login
az storage container create --name exportaciones  --account-name stdemo --auth-mode login
az storage container create --name adjuntos       --account-name stdemo --auth-mode login
az storage container create --name plantillas     --account-name stdemo --auth-mode login
```

**Resultado esperado:** 5 contenedores privados creados en `stdemo`.

---

## TASK-F0-02: Crear Key Vault `kv-demo`

**Estado:** ⏳ Pendiente
**Módulo:** Infraestructura Azure
**Prerequisitos:** TASK-F0-01 completada

### Pasos

```bash
# 1. Crear Key Vault
az keyvault create \
  --name kv-demo \
  --resource-group rg-ezequiel \
  --location eastus \
  --sku standard \
  --enable-rbac-authorization true

# 2. Verificar creación
az keyvault show --name kv-demo --resource-group rg-ezequiel --query "name"
```

**Nota:** RBAC habilitado desde el inicio — se asignará acceso al App Service vía Managed Identity en TASK-F0-05.

**Resultado esperado:** Key Vault `kv-demo` activo con RBAC habilitado.

---

## TASK-F0-03: Crear PostgreSQL Flexible Server `psql-demo`

**Estado:** ⏳ Pendiente
**Módulo:** Infraestructura Azure
**Prerequisitos:** TASK-F0-02 completada

### Pasos

```bash
# 1. Crear servidor PostgreSQL Flexible Server
az postgres flexible-server create \
  --name psql-demo \
  --resource-group rg-ezequiel \
  --location eastus \
  --tier Burstable \
  --sku-name Standard_B1ms \
  --version 16 \
  --storage-size 32 \
  --admin-user demoadmin \
  --admin-password "<contraseña-segura>" \
  --public-access 0.0.0.0

# 2. Crear base de datos
az postgres flexible-server db create \
  --server-name psql-demo \
  --resource-group rg-ezequiel \
  --database-name aulaia

# 3. Guardar connection string en Key Vault
az keyvault secret set \
  --vault-name kv-demo \
  --name "ConnectionStrings--Default" \
  --value "Host=psql-demo.postgres.database.azure.com;Database=aulaia;Username=demoadmin;Password=<contraseña-segura>;SslMode=Require"
```

**Nota de seguridad:** La contraseña NUNCA va en código ni en archivos del repo. Solo en Key Vault.

**Resultado esperado:** Servidor `psql-demo` con base de datos `aulaia` y connection string en Key Vault.

---

## TASK-F0-04: Crear App Service Plan `asp-demo` y App Service `app-demo-api`

**Estado:** ⏳ Pendiente
**Módulo:** Infraestructura Azure
**Prerequisitos:** TASK-F0-01 completada

### Pasos

```bash
# 1. Crear App Service Plan (Linux, B1)
az appservice plan create \
  --name asp-demo \
  --resource-group rg-ezequiel \
  --location eastus \
  --sku B1 \
  --is-linux

# 2. Crear App Service para el backend .NET 10
az webapp create \
  --name app-demo-api \
  --resource-group rg-ezequiel \
  --plan asp-demo \
  --runtime "DOTNETCORE:10.0"

# 3. Configurar variables de entorno base
az webapp config appsettings set \
  --name app-demo-api \
  --resource-group rg-ezequiel \
  --settings \
    ASPNETCORE_ENVIRONMENT=Production \
    KeyVaultName=kv-demo
```

**Resultado esperado:** App Service `app-demo-api` corriendo en Linux .NET 10, configurado para leer secretos de Key Vault.

---

## TASK-F0-05: Habilitar Managed Identity + asignar roles

**Estado:** ⏳ Pendiente
**Módulo:** Infraestructura Azure
**Prerequisitos:** TASK-F0-02, TASK-F0-04 completadas

### Pasos

```bash
# 1. Habilitar System-Assigned Managed Identity en App Service
az webapp identity assign \
  --name app-demo-api \
  --resource-group rg-ezequiel

# 2. Obtener el principalId de la identidad (guardar este valor)
PRINCIPAL_ID=$(az webapp identity show \
  --name app-demo-api \
  --resource-group rg-ezequiel \
  --query principalId --output tsv)

# 3. Asignar rol "Key Vault Secrets User" sobre kv-demo
KV_ID=$(az keyvault show --name kv-demo --resource-group rg-ezequiel --query id --output tsv)
az role assignment create \
  --assignee $PRINCIPAL_ID \
  --role "Key Vault Secrets User" \
  --scope $KV_ID

# 4. Asignar rol "Storage Blob Data Contributor" sobre stdemo
STORAGE_ID=$(az storage account show --name stdemo --resource-group rg-ezequiel --query id --output tsv)
az role assignment create \
  --assignee $PRINCIPAL_ID \
  --role "Storage Blob Data Contributor" \
  --scope $STORAGE_ID
```

**Resultado esperado:** El App Service accede a Key Vault y Blob Storage sin connection strings con claves. Zero secrets en código.

---

## TASK-F0-06: Crear Azure AI Foundry Hub, Project y deploy GPT-5.5

**Estado:** ⏳ Pendiente
**Módulo:** Infraestructura Azure
**Prerequisitos:** TASK-F0-05 completada

### Pasos

```bash
# 1. Crear AI Foundry Hub
az ml workspace create \
  --name aif-demo \
  --resource-group rg-ezequiel \
  --location eastus \
  --kind hub

# 2. Crear AI Foundry Project
az ml workspace create \
  --name aiproj-demo \
  --resource-group rg-ezequiel \
  --location eastus \
  --kind project \
  --hub-id $(az ml workspace show --name aif-demo --resource-group rg-ezequiel --query id --output tsv)

# 3. Asignar rol "Azure AI Developer" al Managed Identity del App Service
AIPROJ_ID=$(az ml workspace show --name aiproj-demo --resource-group rg-ezequiel --query id --output tsv)
az role assignment create \
  --assignee $PRINCIPAL_ID \
  --role "Azure AI Developer" \
  --scope $AIPROJ_ID

# 4. Deploy del modelo GPT-5.5 — hacer desde el portal Azure AI Foundry:
#    Portal: https://ai.azure.com → aiproj-demo → Models + Endpoints → Deploy → gpt-5.5
#    Guardar el endpoint URL en Key Vault:
az keyvault secret set \
  --vault-name kv-demo \
  --name "AzureAI--Endpoint" \
  --value "https://<endpoint>.openai.azure.com/"
```

**Nota:** El deploy de GPT-5.5 se hace desde el portal AI Foundry (no tiene soporte completo aún en Az CLI). El endpoint se guarda en Key Vault; el App Service lo lee vía Managed Identity.

**Resultado esperado:** Hub + Project creados. Managed Identity con acceso al proyecto. Endpoint del modelo en Key Vault.

---

## TASK-F0-07: Crear Static Web App `swa-demo`

**Estado:** ⏳ Pendiente
**Módulo:** Infraestructura Azure
**Prerequisitos:** Repositorio GitHub configurado

### Pasos

```bash
# 1. Crear Static Web App conectada al repositorio GitHub
az staticwebapp create \
  --name swa-demo \
  --resource-group rg-ezequiel \
  --location eastus2 \
  --source https://github.com/ezekiell1988/mep \
  --branch main \
  --app-location "src/aulaia-web" \
  --output-location ".next" \
  --login-with-github
```

**Resultado esperado:** Static Web App `swa-demo` conectada al repo. CI/CD automático en cada push a `main`.

---

## TASK-F0-08: Configurar PowerSync apuntando al PostgreSQL

**Estado:** ⏳ Pendiente
**Módulo:** Infraestructura Azure / Sincronización
**Prerequisitos:** TASK-F0-03 completada

### Pasos

1. Crear cuenta en [https://www.powersync.com](https://www.powersync.com) → Plan Free.
2. Crear nueva instancia → conectar a PostgreSQL:
   - Host: `psql-demo.postgres.database.azure.com`
   - Database: `aulaia`
   - User: `demoadmin`
   - SSL: requerido
3. Definir **Sync Rules** (qué tablas replica a qué usuario):

```yaml
# powersync.yaml — Sync Rules básicas
bucket_definitions:
  docente_data:
    # Cada docente solo ve sus propios grupos
    parameters:
      - SELECT id as teacher_id FROM users WHERE id = token_parameters.user_id
    data:
      - SELECT * FROM groups WHERE teacher_id = bucket.teacher_id
      - SELECT * FROM students WHERE group_id IN (
          SELECT id FROM groups WHERE teacher_id = bucket.teacher_id)
      - SELECT * FROM attendance_records WHERE group_id IN (
          SELECT id FROM groups WHERE teacher_id = bucket.teacher_id)
      - SELECT * FROM evaluation_activities WHERE group_id IN (
          SELECT id FROM groups WHERE teacher_id = bucket.teacher_id)
      - SELECT * FROM grades WHERE activity_id IN (
          SELECT id FROM evaluation_activities WHERE group_id IN (
            SELECT id FROM groups WHERE teacher_id = bucket.teacher_id))
```

4. Guardar el PowerSync endpoint URL en Key Vault:
```bash
az keyvault secret set \
  --vault-name kv-demo \
  --name "PowerSync--Endpoint" \
  --value "https://<instancia>.powersync.journeyapps.com"
```

**Resultado esperado:** PowerSync sincronizando datos del docente autenticado. Sync rules aplicando aislamiento por docente.

---

## TASK-F0-09: Crear solución .NET 10 con estructura Feature Folders

**Estado:** ⏳ Pendiente
**Módulo:** Backend
**Prerequisitos:** .NET 10 SDK instalado

### Pasos

```bash
# 1. Crear solución y proyecto
cd src
dotnet new sln --name AulaIA
dotnet new web --name AulaIA.Api --framework net10.0
dotnet sln add AulaIA.Api/AulaIA.Api.csproj

# 2. Instalar paquetes base
cd AulaIA.Api
dotnet add package Microsoft.EntityFrameworkCore.Design
dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer
dotnet add package Azure.Identity
dotnet add package Azure.Security.KeyVault.Secrets
dotnet add package Azure.Storage.Blobs
dotnet add package Azure.AI.OpenAI
dotnet add package Microsoft.Extensions.Http.Resilience
dotnet add package Scalar.AspNetCore

# 3. Crear estructura de carpetas Feature Folders
mkdir -p Features/Grupos/Endpoints
mkdir -p Features/Estudiantes/Endpoints
mkdir -p Features/Asistencia/Endpoints
mkdir -p Features/Notas/Endpoints
mkdir -p Features/Planeamiento/Endpoints
mkdir -p Features/Reportes/Endpoints
mkdir -p Features/PowerSync/Endpoints
mkdir -p Shared/Persistence/Migrations
mkdir -p Shared/Options
mkdir -p Shared/Storage
```

**Implementation hint:** Usar patrón Module del skill `dotnet-10-csharp-14` → `minimal-apis.md`. Cada Feature tiene su `XModule.cs` con `AddXModule()` + `MapXEndpoints()`. TypedResults obligatorio en todos los handlers.

**Resultado esperado:** Solución compilable con estructura Feature Folders. `dotnet build` sin errores.

---

## TASK-F0-10: Crear proyecto Next.js `aulaia-web`

**Estado:** ⏳ Pendiente
**Módulo:** Frontend Web
**Prerequisitos:** Node.js 20+ instalado

### Pasos

```bash
cd src
npx create-next-app@latest aulaia-web \
  --typescript \
  --tailwind \
  --eslint \
  --app \
  --src-dir \
  --import-alias "@/*"

cd aulaia-web
npm install @auth0/nextjs-auth0 @powersync/web
```

**Resultado esperado:** Proyecto Next.js con App Router, TypeScript y Tailwind listo para desarrollo.

---

## TASK-F0-11: Crear proyecto Expo `aulaia-app`

**Estado:** ⏳ Pendiente
**Módulo:** App Móvil
**Prerequisitos:** Node.js 20+ instalado

### Pasos

```bash
cd mobile
npx create-expo-app@latest aulaia-app --template blank-typescript

cd aulaia-app
npx expo install expo-camera expo-barcode-scanner
npm install react-native-auth0 @powersync/react-native
```

**Resultado esperado:** Proyecto Expo con TypeScript, cámara QR y PowerSync instalados.

---

## TASK-F0-12: Configurar GitHub Actions CI/CD

**Estado:** ⏳ Pendiente
**Módulo:** DevOps
**Prerequisitos:** TASK-F0-04, TASK-F0-07 completadas

### Pasos

1. En GitHub → Settings → Secrets → Actions, agregar:
   - `AZURE_WEBAPP_PUBLISH_PROFILE` — descargar desde portal App Service `app-demo-api`
   - `AZURE_STATIC_WEB_APPS_API_TOKEN` — desde portal Static Web App `swa-demo`

2. Crear `.github/workflows/api-deploy.yml`:

```yaml
name: Deploy Backend
on:
  push:
    branches: [main]
    paths: ['src/AulaIA.Api/**']

jobs:
  build-deploy:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '10.x'
      - run: dotnet publish src/AulaIA.Api/AulaIA.Api.csproj -c Release -o ./publish
      - uses: azure/webapps-deploy@v3
        with:
          app-name: app-demo-api
          publish-profile: ${{ secrets.AZURE_WEBAPP_PUBLISH_PROFILE }}
          package: ./publish
```

**Resultado esperado:** Push a `main` con cambios en `src/AulaIA.Api/` despliega automáticamente al App Service.

---

## TASK-F0-13: Crear migración inicial EF Core

**Estado:** ⏳ Pendiente
**Módulo:** Backend — Base de datos
**Prerequisitos:** TASK-F0-09 completada; TASK-F0-03 con PostgreSQL accesible

### Pasos

```bash
cd src/AulaIA.Api

# 1. Crear todas las entidades en Shared/Persistence/AulaIADbContext.cs
# (Institution, User, Group, Student, Accommodation,
#  AttendanceRecord, EvaluationActivity, Grade, LessonPlan)

# 2. Generar migración inicial
dotnet ef migrations add InitialCreate --output-dir Shared/Persistence/Migrations

# 3. Aplicar migración al PostgreSQL de Azure
dotnet ef database update \
  --connection "Host=psql-demo.postgres.database.azure.com;Database=aulaia;Username=demoadmin;Password=<pwd>;SslMode=Require"
```

**Implementation hint:** Usar skill `dotnet-ef-re-create` si la migración queda inconsistente. Code-first. Todas las entidades usan `Guid` como PK. `DateTime` → siempre `DateTimeOffset` para evitar problemas de timezone (Costa Rica = UTC-6).

**Resultado esperado:** Base de datos `aulaia` con todas las tablas creadas. `dotnet ef migrations list` muestra `InitialCreate` aplicada.

---

## TASK-F0-14: Configurar Auth0

**Estado:** ⏳ Pendiente
**Módulo:** Autenticación
**Prerequisitos:** Cuenta Auth0 creada

### Pasos

1. **Crear tenant** en [auth0.com](https://auth0.com) → nombre sugerido: `demo-aulaia`

2. **Crear API** (Resource Server):
   - Name: `AulaIA API`
   - Identifier (audience): `https://api.aulaia.app`
   - Signing Algorithm: RS256

3. **Crear aplicaciones:**
   - `AulaIA Web` → tipo: Regular Web Application → Callback: `https://mep.ezekl.com/api/auth/callback`
   - `AulaIA Mobile` → tipo: Native → Callback: `aulaia://callback`

4. **Crear roles:**
   - `docente`
   - `director`
   - `admin`

5. **Crear Action** (post-login) para agregar claims custom al token:
```javascript
exports.onExecutePostLogin = async (event, api) => {
  const roles = event.authorization?.roles ?? [];
  api.idToken.setCustomClaim('https://aulaia.app/role', roles[0] ?? 'docente');
  api.accessToken.setCustomClaim('https://aulaia.app/role', roles[0] ?? 'docente');
};
```

6. Guardar `Domain`, `ClientId` y `ClientSecret` en Key Vault:
```bash
az keyvault secret set --vault-name kv-demo --name "Auth0--Domain"   --value "<tenant>.auth0.com"
az keyvault secret set --vault-name kv-demo --name "Auth0--Audience" --value "https://api.aulaia.app"
```

**Resultado esperado:** Tenant Auth0 con API, 2 aplicaciones, 3 roles y Action de claims configurados. Secretos en Key Vault.

---

## TASK-F0-15: Configurar DNS en Cloudflare para `mep.ezekl.com`

**Estado:** ⏳ Pendiente
**Módulo:** DNS / Infraestructura
**Prerequisitos:** TASK-F0-04 y TASK-F0-07 completadas (App Service y Static Web App deben existir)

### Pasos

1. **Agregar dominio personalizado al Static Web App** (frontend web):
```bash
az staticwebapp hostname set \
  --name swa-demo \
  --resource-group rg-ezequiel \
  --hostname mep.ezekl.com
```
El portal devolverá un token de validación. Crear un registro TXT en Cloudflare para validar.

2. **Agregar dominio personalizado al App Service** (backend API):
```bash
az webapp config hostname add \
  --webapp-name app-demo-api \
  --resource-group rg-ezequiel \
  --hostname api.mep.ezekl.com
```

3. **Crear registros DNS en Cloudflare** (panel de ezekl.com):

| Tipo | Nombre | Destino | Proxy |
|------|--------|---------|-------|
| CNAME | `mep` | `<id>.azurestaticapps.net` | ☁️ Proxied |
| TXT | `_dnsauth.mep` | `<token-validación-swa>` | — |
| CNAME | `api.mep` | `app-demo-api.azurewebsites.net` | ☁️ Proxied |
| TXT | `asuid.api.mep` | `<custom-domain-verification-id>` | — |

> **Obtener `custom-domain-verification-id` del App Service:**
> ```bash
> az webapp show --name app-demo-api --resource-group rg-ezequiel \
>   --query "customDomainVerificationId" --output tsv
> ```

4. **Configurar SSL en App Service** (después de validar el dominio):
```bash
# Crear certificado gestionado por App Service (requiere dominio validado)
az webapp config ssl create \
  --name app-demo-api \
  --resource-group rg-ezequiel \
  --hostname api.mep.ezekl.com

# Vincular el certificado
az webapp config ssl bind \
  --name app-demo-api \
  --resource-group rg-ezequiel \
  --certificate-name api-mep-ezekl-com \
  --ssl-type SNI
```

> **Nota:** Static Web App gestiona su propio SSL automáticamente. No requiere pasos adicionales.

5. **Actualizar `appsettings.json` del backend** con la URL de origen permitida (CORS):
```json
{
  "AllowedOrigins": ["https://mep.ezekl.com"]
}
```

**Resultado esperado:**
- `https://mep.ezekl.com` resuelve al frontend Next.js con SSL válido.
- `https://api.mep.ezekl.com` resuelve al backend .NET 10 con SSL válido.
- Cloudflare activo como proxy (protección DDoS + caché de borde).

---

## TASK-F5-01: Migración EF Core — entidades de monetización

**Estado:** ⏳ Pendiente
**Módulo:** Fase 5 — Monetización
**Prerequisitos:** Fase 4 completada

**Contexto:** Agregar las 5 tablas nuevas del módulo de pagos/suscripciones/referidos a la BD mediante una sola migración `AddMonetizacion`.

**Pasos:**

1. Crear entidades en `Shared/Domain/`:
   - `Subscription.cs` — ver entidad en `02_architecture.md`
   - `PaymentRequest.cs` — incluye `ExchangeRateUsed` (decimal)
   - `ExchangeRate.cs` — índice único en `Date`
   - `ReferralCode.cs`
   - `Commission.cs`

2. Registrar en `AulaIADbContext` y configurar relaciones EF (FK, índices únicos).

3. Generar y aplicar la migración:
```bash
cd src/AulaIA.Api
dotnet ef migrations add AddMonetizacion
dotnet ef database update
```

**Resultado esperado:** 5 tablas nuevas en la BD. Build sin errores.

**Patrón de referencia:** PATTERN-01 (entidades con cascade delete) en `09_patterns.md`.

---

## TASK-F5-02: Job `UpdateExchangeRateJob` — tipo de cambio BCCR

**Estado:** ⏳ Pendiente
**Módulo:** Fase 5 — Monetización
**Prerequisitos:** TASK-F5-01

**Contexto:** SINPE Móvil opera solo en CRC. El job consulta el API SOAP del BCCR (indicador 318 — venta USD) una vez por día hábil a las 6am hora CR y guarda el resultado en `exchange_rates`. El token BCCR es gratuito.

**Pasos:**

1. Registrarse en `https://gee.bccr.fi.cr` para obtener token BCCR y guardarlo en Key Vault.

2. Crear `SinpeOptions.cs`:
```csharp
public class SinpeOptions
{
    public const string Section = "Sinpe";
    [Required] public string SinpeNumber { get; init; } = default!;
    [Required] public string BccrEmail { get; init; } = default!;
    [Required] public string BccrApiToken { get; init; } = default!;
}
```

3. Implementar `UpdateExchangeRateJob` siguiendo **PATTERN-05** de `09_patterns.md`.

4. Registrar el job en `Program.cs` como job diario a las 6am CR.

5. Agregar a `appsettings.Development.json` (valores de prueba del BCCR):
```json
"Sinpe": {
  "SinpeNumber": "88001234",
  "BccrEmail": "dev@example.com",
  "BccrApiToken": "TOKEN_BCCR_DEV"
}
```

**Resultado esperado:** Al ejecutar el job manualmente, se inserta un registro en `exchange_rates` con el TC de venta del día. Log visible en `llm-audit.md`.

**Patrón de referencia:** PATTERN-05 (`UpdateExchangeRateJob` BCCR) y PATTERN-03 (`ILlmAuditService`) en `09_patterns.md`.

---

## TASK-F5-03: Módulo Suscripciones — backend

**Estado:** ⏳ Pendiente
**Módulo:** Fase 5 — Monetización
**Prerequisitos:** TASK-F5-01, TASK-F5-02

**Contexto:** Endpoints para el flujo de pago SINPE: crear solicitud (genera `reference_code`, calcula `amount_crc` con TC del día), subir comprobante, consultar estado.

**Endpoints a implementar:**

| Método | Ruta | Descripción |
|--------|------|-------------|
| `GET` | `/api/suscripciones/estado` | Estado actual del plan del usuario autenticado |
| `POST` | `/api/suscripciones/solicitud-pago` | Crea `PaymentRequest`, devuelve `reference_code` + `amount_crc` + `sinpe_number` |
| `POST` | `/api/suscripciones/solicitud-pago/{id}/comprobante` | Sube captura de pantalla a Blob `pagos` |

**Lógica de `POST /solicitud-pago`:**
1. Leer TC del día de `exchange_rates` (fallback: último disponible).
2. Calcular `amount_crc = round(amount_usd × sell_rate, 0)`.
3. Generar `reference_code = $"AUI-{today:yyyyMMdd}-{Random4Chars()}"`.
4. Insertar `PaymentRequest` con `status = pending`.
5. Devolver: `{ referenceCode, amountUsd, amountCrc, sinpeNumber, instructions }`.

**Resultado esperado:** Un docente puede crear una solicitud de pago y recibir las instrucciones SINPE con el monto en colones.

---

## TASK-F5-04: Módulo Pagos — panel admin backend

**Estado:** ⏳ Pendiente
**Módulo:** Fase 5 — Monetización
**Prerequisitos:** TASK-F5-03

**Endpoints a implementar (todos requieren rol `admin`):**

| Método | Ruta | Descripción |
|--------|------|-------------|
| `GET` | `/api/admin/pagos/pendientes` | Lista de `PaymentRequest` con `status = pending` |
| `POST` | `/api/admin/pagos/{id}/aprobar` | Aprueba pago → activa `Subscription` 30 días; notifica usuario |
| `POST` | `/api/admin/pagos/{id}/rechazar` | Rechaza con nota → notifica usuario |
| `GET` | `/api/admin/suscripciones` | Lista de suscripciones activas con fecha de vencimiento |
| `GET` | `/api/admin/pagos/historial` | Historial completo para reporte mensual |

**Lógica de `POST /aprobar`:**
1. Verificar que `PaymentRequest.status == pending`.
2. Crear o actualizar `Subscription`: `status = active`, `current_period_end = today + 30 days`.
3. Actualizar `PaymentRequest`: `status = approved`, `reviewed_by`, `reviewed_at`.
4. Enviar email de confirmación al usuario.

---

## TASK-F5-05: Jobs de mantenimiento de suscripciones

**Estado:** ⏳ Pendiente
**Módulo:** Fase 5 — Monetización
**Prerequisitos:** TASK-F5-04

**Jobs a implementar:**

1. **`CheckExpiredSubscriptionsJob`** (Hangfire diario, 7am CR):
   - Consulta `Subscription` donde `current_period_end < today` y `status = active`.
   - Degrada a `status = past_due`.
   - Envía push/email al usuario.

2. **`SendRenewalRemindersJob`** (Hangfire diario, 8am CR):
   - Consulta suscripciones con `current_period_end` a 7 días y a 3 días.
   - Envía notificación con instrucciones SINPE para renovar.

3. **`CalculateCommissionsJob`** (Hangfire mensual, día 1, 8am CR):
   - Solo se ejecuta si el admin ingresó el costo de infraestructura Azure del mes anterior en el panel.
   - Calcula comisión: `20% × (subscription_amount - infra_share)` por referido activo en el mes.
   - Inserta registros en `commissions` con `status = pending`.
   - El admin marca como `paid` después de hacer el SINPE al referidor.

---

## TASK-F5-06: Módulo Referidos — backend

**Estado:** ⏳ Pendiente
**Módulo:** Fase 5 — Monetización
**Prerequisitos:** TASK-F5-01

**Endpoints a implementar:**

| Método | Ruta | Descripción |
|--------|------|-------------|
| `GET` | `/api/referidos/mi-codigo` | Devuelve el código del usuario (lo crea si no existe) |
| `GET` | `/api/referidos/panel` | Lista de referidos + comisiones por mes del usuario autenticado |
| `GET` | `/api/admin/referidos` | Vista completa admin de todos los referidores |
| `POST` | `/api/admin/comisiones/{id}/marcar-pagado` | Marca comisión como pagada (admin hace el SINPE al referidor manualmente) |

**Lógica de registro con referido:**
- Al crear un usuario (`POST /api/auth/register`), si el body incluye `referralCode` o la URL tiene `?ref=CODE`, guardar `referred_by_code` en `User`. No editable después.

---

## TASK-F5-07: UI Web — flujo de pago SINPE y panel admin

**Estado:** ⏳ Pendiente
**Módulo:** Fase 5 — Monetización
**Prerequisitos:** TASK-F5-03, TASK-F5-04

**Páginas/componentes a crear en `aulaia-web/src/app/`:**

| Ruta | Descripción |
|------|-------------|
| `/suscripcion` | Página de precios (3 planes) con CTA "Suscribirse" |
| `/suscripcion/pagar` | Pantalla SINPE: número destino, monto CRC, código referencia, instrucciones, botón "Ya pagué" + upload comprobante |
| `/admin/pagos` | Panel admin: pestañas Pendientes / Historial / Suscripciones activas |
| `/admin/cierre-mensual` | Ingresar costo infra Azure del mes + ejecutar job comisiones |
| `/perfil/referidos` | Panel de referidos y comisiones del docente |

**Componente banner trial** (global, en layout principal):
- Muestra días restantes del trial con progress bar.
- CTA "Activar suscripción" → redirige a `/suscripcion`.
- Se oculta cuando `status = active`.
