# 02 — Arquitectura del Sistema

> **Última actualización:** 2026-05-07
> **Scope:** AulaIA — arquitectura completa (Fases 1–5)

---

## Pipeline principal (vista de alto nivel)

```
┌─────────────────────────────────────────────────────────────────────────┐
│                        CLIENTES                                          │
│                                                                          │
│   ┌──────────────────┐          ┌──────────────────────────────────┐    │
│   │  App Móvil        │          │  App Web                          │    │
│   │  React Native     │          │  Next.js (React)                  │    │
│   │  (Expo)           │          │  mep.ezekl.com                    │    │
│   │  iOS + Android    │          │  (uso desde escritorio)           │    │
│   │  Asistencia QR    │          └──────────────┬───────────────────┘    │
│   │  Notas offline    │                         │                         │
│   └────────┬─────────┘                         │                         │
│            │                                    │                         │
│   SQLite (local)  ←──── PowerSync ←──────────────────────────────────┐  │
│   Offline-first          Sync Service                                  │  │
└────────────┼───────────────────────────────────┼────────────────────────┘  │
             │                                    │                            │
             │            ┌───────────────────────┘                            │
             │            │                                                     │
             ▼            ▼                                                     │
┌─────────────────────────────────────────────────────────────────────────┐    │
│              CLOUDFLARE  (DNS + Proxy + WAF + TLS)                       │    │
│                                                                          │    │
│  mep.ezekl.com  ──CNAME──►  swa-demo.azurestaticapps.net               │    │
│  api.mep.ezekl.com ─CNAME─►  app-demo-api.azurewebsites.net            │    │
└──────────────────────────────────┬──────────────────────────────────────┘    │
                                   │                                            │
                                   ▼ HTTPS + JWT (Auth0)                        │
┌─────────────────────────────────────────────────────────────────────────┐    │
│                    BACKEND — .NET 10 / ASP.NET Core                      │    │
│                    Azure App Service (Fase 1–2)  [api.mep.ezekl.com]    │    │
│                                                                          │ │
│  ┌──────────┐ ┌────────────┐ ┌──────────────┐ ┌──────────────────────┐ │ │
│  │ Auth /   │ │ Grupos &   │ │ Planeamiento │ │ Asistencia / Notas / │ │ │
│  │ Usuarios │ │ Estudiantes│ │ + IA         │ │ Reportes / Calendario│ │ │
│  └──────────┘ └────────────┘ └──────┬───────┘ └──────────────────────┘ │ │
│                                      │                                   │ │
│  ┌───────────────────────────────────┼──────────────────────────────┐   │ │
│  │            Entity Framework Core 10                               │   │ │
│  └───────────────────────────────────┼──────────────────────────────┘   │ │
└─────────────────────────────────────┼────────────────────────────────────┘ │
                                       │                                       │
          ┌────────────────────────────┼───────────────────────────┐          │
          │                            │                            │          │
          ▼                            ▼                            ▼          │
  ┌──────────────┐          ┌──────────────────┐         ┌─────────────────┐ │
  │  PostgreSQL  │          │ Azure AI Foundry  │         │  Azure Blob     │ │
  │  (datos de   │◄─────────│  GPT-5.5          │         │  Storage        │ │
  │  negocio)    │  PowerSync│  Planeamientos    │         │  PDFs / DOCX /  │ │
  └──────────────┘          └──────────────────┘         │  Exportaciones  │ │
          ▲                                               └─────────────────┘ │
          └─────────────────────────────────────────────────────────────────┘
                         PowerSync replica PG → SQLite local
```

---

## Arquitectura por capas

### Capa 1 — Clientes

| Cliente | Tecnología | Casos de uso principales |
|---------|-----------|--------------------------|
| App móvil | React Native (Expo) | Asistencia QR/manual, registro de notas, consulta de grupos, notificaciones push |
| App web | Next.js (React) | Generación de planeamientos, libro de notas completo, reportes, gestión de grupos desde escritorio |

**Código compartido entre móvil y web:**
- Lógica de negocio (cálculo de promedios, validaciones)
- Modelos de datos TypeScript
- Llamadas a la API (cliente HTTP compartido)
- Store de PowerSync

---

### Capa 2 — Backend (.NET 10)

**Patrón:** Minimal APIs con **Feature Folders / Vertical Slices** (Module Pattern del skill dotnet-10-csharp-14). Sin Controllers — todo son handlers estáticos por slice.

**Estructura de proyecto propuesta:**

```
src/
├── AulaIA.Api/                         ← Proyecto principal ASP.NET Core
│   ├── Features/
│   │   ├── Grupos/
│   │   │   ├── GruposModule.cs          ← AddGruposModule() + MapGruposEndpoints()
│   │   │   ├── Endpoints/
│   │   │   │   ├── CreateGrupo.cs       ← record Request/Response + Handle()
│   │   │   │   ├── GetGrupo.cs
│   │   │   │   └── ListGrupos.cs
│   │   │   └── Services/
│   │   ├── Estudiantes/
│   │   │   ├── EstudiantesModule.cs
│   │   │   └── Endpoints/
│   │   ├── Asistencia/
│   │   │   ├── AsistenciaModule.cs
│   │   │   └── Endpoints/
│   │   ├── Notas/
│   │   │   ├── NotasModule.cs
│   │   │   └── Endpoints/
│   │   ├── Planeamiento/
│   │   │   ├── PlaneamientoModule.cs
│   │   │   ├── Endpoints/
│   │   │   │   ├── GenerarPlaneamiento.cs
│   │   │   │   └── GetPlaneamiento.cs
│   │   │   └── Services/
│   │   │       └── PlaneamientoAiService.cs  ← usa IHttpClientFactory
│   │   ├── Reportes/
│   │   │   ├── ReportesModule.cs
│   │   │   └── Endpoints/
│   │   └── PowerSync/
│   │       ├── PowerSyncModule.cs
│   │       └── Endpoints/
│   │           └── GetPowerSyncToken.cs  ← /api/powersync/token
│   ├── Shared/                          ← Cross-cutting: DbContext, errores, extensiones
│   │   ├── Persistence/
│   │   │   ├── AulaIADbContext.cs
│   │   │   └── Migrations/
│   │   ├── Options/                     ← Clases Options con ValidateOnStart()
│   │   │   ├── JwtOptions.cs
│   │   │   ├── AzureAiOptions.cs
│   │   │   └── BlobStorageOptions.cs
│   │   └── Storage/
│   │       └── BlobStorageService.cs
│   ├── Program.cs
│   └── appsettings.json
```

**Patrón de módulo (ejemplo):**

```csharp
// Features/Asistencia/AsistenciaModule.cs
public static class AsistenciaModule
{
    public static IServiceCollection AddAsistenciaModule(this IServiceCollection s)
        => s.AddScoped<IAsistenciaService, AsistenciaService>();

    public static IEndpointRouteBuilder MapAsistenciaEndpoints(this IEndpointRouteBuilder app)
    {
        var g = app.MapGroup("/api/asistencia")
            .WithTags("Asistencia")
            .RequireAuthorization();
        g.MapPost("/", RegistrarAsistencia.Handle);
        g.MapGet("/{grupoId}", GetAsistencia.Handle);
        return app;
    }
}

// Features/Asistencia/Endpoints/RegistrarAsistencia.cs
public static class RegistrarAsistencia
{
    public record Request(
        [Required] Guid GrupoId,
        [Required] DateOnly Fecha,
        [Required, MinLength(1)] List<AsistenciaRecord> Records);
    public record Response(int Saved, Guid SyncId);

    public static async Task<Results<Created<Response>, ValidationProblem>>
        Handle(Request req, IAsistenciaService svc, CancellationToken ct)
    {
        var result = await svc.RegistrarAsync(req, ct);
        return TypedResults.Created($"/api/asistencia/{req.GrupoId}",
            new Response(result.Saved, result.SyncId));
    }
}
```

**Program.cs (orden de middleware obligatorio):**

```csharp
var builder = WebApplication.CreateBuilder(args);

// Validación built-in .NET 10
builder.Services.AddValidation();
builder.Services.AddProblemDetails();
builder.Services.AddOpenApi();

// Seguridad
builder.Services.AddAuthentication().AddJwtBearer();  // Auth0
builder.Services.AddAuthorizationBuilder()
    .AddPolicy("Docente", p => p.RequireRole("docente"))
    .AddPolicy("Director", p => p.RequireRole("director", "admin"));
builder.Services.AddRateLimiter(opts => opts
    .AddSlidingWindowLimiter("planeamiento-ai", o =>
    { o.Window = TimeSpan.FromMinutes(1); o.PermitLimit = 5; }));

// Options con ValidateOnStart() — OBLIGATORIO
builder.Services.AddOptions<AzureAiOptions>()
    .BindConfiguration(AzureAiOptions.Section)
    .ValidateDataAnnotations().ValidateOnStart();
builder.Services.AddOptions<BlobStorageOptions>()
    .BindConfiguration(BlobStorageOptions.Section)
    .ValidateDataAnnotations().ValidateOnStart();

// HTTP clients con resiliencia — IHttpClientFactory
builder.Services.AddHttpClient<PlaneamientoAiService>()
    .AddStandardResilienceHandler();  // Retry + Circuit breaker automático

// Caché para planeamientos generados
builder.Services.AddOutputCache();
builder.Services.AddHealthChecks()
    .AddNpgSql(/* connection string */);

// Módulos
builder.Services
    .AddGruposModule()
    .AddEstudiantesModule()
    .AddAsistenciaModule()
    .AddNotasModule()
    .AddPlaneamientoModule()
    .AddReportesModule()
    .AddPowerSyncModule()
    .AddSuscripcionesModule()
    .AddPagosAdminModule()
    .AddReferidosModule();

// Hangfire jobs
builder.Services.AddHangfire(cfg => cfg.UsePostgreSqlStorage(connStr));
builder.Services.AddHangfireServer();
RecurringJob.AddOrUpdate<UpdateExchangeRateJob>("bccr-tc", j => j.Execute(CancellationToken.None), Cron.Daily(6));      // 6am hora CR
RecurringJob.AddOrUpdate<CheckExpiredSubscriptionsJob>("check-subs", j => j.Execute(CancellationToken.None), Cron.Daily(7));
RecurringJob.AddOrUpdate<CalculateCommissionsJob>("commissions", j => j.Execute(CancellationToken.None), Cron.Monthly(1, 8)); // día 1 a las 8am

// Hangfire jobs
builder.Services.AddHangfire(...);
builder.Services.AddHangfireServer();
RecurringJob.AddOrUpdate<UpdateExchangeRateJob>("bccr-tc", j => j.Execute(CancellationToken.None), Cron.Daily(6));   // 6am CR
RecurringJob.AddOrUpdate<CheckExpiredSubscriptionsJob>("check-subs", j => j.Execute(CancellationToken.None), Cron.Daily(7));
RecurringJob.AddOrUpdate<CalculateCommissionsJob>("commissions", j => j.Execute(CancellationToken.None), Cron.Monthly(1, 8)); // día 1 a las 8am

var app = builder.Build();

// Orden de middleware (CRÍTICO — no reordenar)
app.UseExceptionHandler();
app.UseHttpsRedirection();
app.UseCors();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.UseOutputCache();

app.MapOpenApi();
app.MapHealthChecks("/health").AllowAnonymous();
app.MapGruposEndpoints()
   .MapEstudiantesEndpoints()
   .MapAsistenciaEndpoints()
   .MapNotasEndpoints()
   .MapPlaneamientoEndpoints()
   .MapReportesEndpoints()
   .MapPowerSyncEndpoints();

app.Run();
```

---

### Capa 3 — Base de datos (PostgreSQL + EF Core)

**Modelo de entidades principal:**

```
Institution
  ├── id (uuid)
  ├── name
  ├── circuit (circuito MEP)
  └── regional_direction

User (docente/director/admin — identidad en Auth0, datos en PG)
  ├── id (uuid — mismo que sub de Auth0)
  ├── auth0_id
  ├── role (docente | director | admin)
  └── institution_id (FK)

Group (sección)
  ├── id (uuid)
  ├── teacher_id (FK → User)
  ├── subject (asignatura)
  ├── grade_level (7, 8, 9, 10, 11...)
  ├── section_name (ej. "7-3")
  ├── school_year
  └── lessons_per_week

Student
  ├── id (uuid)
  ├── full_name
  ├── id_number (cédula/expediente)
  ├── qr_code (uuid único — no expone datos personales)
  └── group_id (FK → Group)

Subscription
  ├── id (uuid)
  ├── user_id (FK → User)
  ├── plan (basic | professional | institutional)
  ├── status (trialing | active | past_due | cancelled)
  ├── trial_ends_at
  ├── current_period_end
  ├── activated_by_admin_id (FK → User, nullable)
  └── activated_at

PaymentRequest
  ├── id (uuid)
  ├── user_id (FK → User)
  ├── plan
  ├── amount_usd
  ├── amount_crc                    ← calculado con exchange_rates.sell_rate del día
  ├── exchange_rate_used            ← auditoría
  ├── reference_code (unique)       ← formato AUI-YYYYMMDD-XXXX
  ├── status (pending | approved | rejected)
  ├── screenshot_url (nullable)
  ├── admin_note (nullable)
  ├── created_at
  ├── reviewed_at
  └── reviewed_by (FK → User, nullable)

ExchangeRate
  ├── id (uuid)
  ├── date (unique — clave natural; un registro por día hábil)
  ├── sell_rate (DECIMAL — tipo de cambio de venta BCCR USD→CRC)
  └── fetched_at

ReferralCode
  ├── id (uuid)
  ├── user_id (FK → User)
  ├── code (8 chars, unique)
  └── created_at

Commission
  ├── id (uuid)
  ├── referrer_user_id (FK → User)
  ├── referred_user_id (FK → User)
  ├── month (DateOnly — primer día del mes)
  ├── subscription_amount_usd
  ├── infra_deduction_usd
  ├── commission_amount_usd
  ├── status (pending | paid)
  ├── paid_at (nullable)
  └── sinpe_confirmation (nullable)

Accommodation (adecuación curricular)
  ├── id (uuid)
  ├── student_id (FK → Student)
  ├── type (significant | non_significant)
  ├── diagnosis
  ├── functioning_level
  ├── difficulties
  ├── strengths
  └── cae_recommendations

AttendanceRecord
  ├── id (uuid)
  ├── student_id (FK → Student)
  ├── group_id (FK → Group)
  ├── date
  ├── status (present | absent | late | early_exit | justified)
  └── justification_note

EvaluationActivity
  ├── id (uuid)
  ├── group_id (FK → Group)
  ├── name
  ├── type (daily_work | homework | project | exam | self_eval | co_eval | portfolio)
  ├── date
  └── max_score

Grade
  ├── id (uuid)
  ├── student_id (FK → Student)
  ├── activity_id (FK → EvaluationActivity)
  └── score

LessonPlan (planeamiento)
  ├── id (uuid)
  ├── teacher_id (FK → User)
  ├── group_id (FK → Group)
  ├── period_type (weekly | monthly | trimestral | semestral | annual)
  ├── trimester (1 | 2 | 3)
  ├── start_date / end_date
  ├── content (JSONB — contenido generado por la IA)
  ├── blob_url (PDF exportado en Azure Blob Storage)
  └── generated_at
```

---

### Capa 4 — Sincronización offline (PowerSync)

**Flujo de escrituras (offline → online):**

```
Docente en el aula (sin internet)
    │
    ▼
App escribe en SQLite local
(asistencia, notas, etc.)
    │
    ▼
PowerSync cola los cambios pendientes
    │
    ▼ (al recuperar conexión)
PowerSync envía a /api/sync/writes (.NET 10)
    │
    ▼
Backend valida con EF Core + reglas de negocio
    │
    ▼
PostgreSQL actualizado
```

**Flujo de lecturas (online → offline):**

```
PostgreSQL (fuente de verdad)
    │
    ▼
PowerSync Service monitorea cambios en PG
    │
    ▼ (cuando hay conexión en el dispositivo)
SQLite del dispositivo actualizado automáticamente
    │
    ▼
App lee desde SQLite — siempre rápido, sin latencia de red
```

**Tablas sincronizadas al dispositivo:**
- `groups` (solo los del docente autenticado)
- `students` (de los grupos del docente)
- `attendance_records` (últimos 90 días)
- `evaluation_activities` (del año lectivo activo)
- `grades` (del año lectivo activo)
- `lesson_plans` (metadatos; el contenido JSONB solo si está marcado como "descargado")

**Tablas NO sincronizadas (solo en servidor):**
- `institutions`
- `users`
- `accommodations` (datos sensibles de menores — solo lectura en línea)

---

### Capa 5 — Módulo de IA (Azure AI Foundry — GPT-5.5)

**Flujo de generación de planeamiento:**

```
Cliente solicita planeamiento
    │
    ▼
Backend verifica caché (¿ya existe un planeamiento con esos parámetros?)
    ├── SÍ → devuelve planeamiento cacheado (sin llamar al LLM)
    └── NO →
         │
         ▼
    Backend construye el prompt del sistema:
    [contexto del programa oficial MEP] +
    [parámetros del docente] +
    [instrucciones de formato de salida JSON]
         │
         ▼
    Llamada a Azure AI Foundry (GPT-5.5)
    con Structured Output (JSON Schema)
         │
         ▼
    Backend valida la respuesta JSON
    (verifica que los aprendizajes existan en el programa)
         │
         ▼
    Guarda en PostgreSQL (tabla lesson_plans)
    Genera PDF → sube a Azure Blob Storage
         │
         ▼
    Devuelve planeamiento al cliente
```

**Estructura del prompt del sistema (diseño base):**

```
SYSTEM:
Eres un asistente pedagógico experto en los programas de estudio 
oficiales del Ministerio de Educación Pública (MEP) de Costa Rica.

Programa de referencia:
[PROGRAMA_MEP_COMPLETO — inyectado desde la base de conocimiento]

Reglas:
1. Solo usa aprendizajes esperados que existan en el programa oficial.
2. Usa terminología exacta del MEP.
3. Responde SOLO con el JSON del esquema indicado.
4. No inventes contenidos que no estén en el programa.

USER:
Genera un planeamiento {periodo} de {asignatura} para {nivel},
con {lecciones_semana} lecciones semanales,
del {fecha_inicio} al {fecha_fin}.
Lecciones disponibles: {total_lecciones}.
Plantilla: {plantilla_institucional | "plantilla estándar del MEP"}
```

---

### Capa 6 — Autenticación (Auth0)

**Flujo de autenticación:**

```
Usuario abre la app
    │
    ▼
Auth0 Universal Login (Authorization Code + PKCE)
    │
    ▼
Auth0 devuelve Access Token (JWT) + Refresh Token
    │
    ├─► App adjunta Access Token en header Authorization: Bearer {token}
    │   a todas las llamadas al backend
    │
    └─► App solicita a /api/powersync/token
        Backend valida el Access Token de Auth0
        y emite un JWT firmado para PowerSync
```

**Claims requeridos en el JWT de Auth0:**
- `sub` — identificador único del usuario
- `https://aulaia.app/role` — `docente | director | admin`
- `https://aulaia.app/institution_id` — UUID de la institución

---

### Capa 7 — Almacenamiento (Azure Blob Storage)

**Contenedores y acceso:**

| Contenedor | Contenido | Acceso |
|------------|-----------|--------|
| `planeamientos` | PDFs generados por la IA | SAS token temporal (15 min) generado por el backend |
| `reportes` | Actas de notas, asistencia, informes | SAS token temporal (15 min) |
| `exportaciones` | CSV/XLSX para el SEA | SAS token temporal (15 min) |
| `adjuntos` | Archivos subidos por el docente | SAS token temporal (60 min) |
| `plantillas` | Plantillas institucionales subidas | SAS token temporal (60 min) |
| `pagos` | Comprobantes SINPE subidos por usuarios | SAS token temporal (5 min) — solo admin puede leer |

**Regla:** El cliente **nunca** recibe credenciales permanentes de Azure. Solo recibe URLs firmadas (SAS) con expiración corta.

---

## Contratos de interfaces críticas

### POST /api/planeamiento/generar

```http
POST /api/planeamiento/generar
Authorization: Bearer {access_token}
Content-Type: application/json

{
  "groupId": "uuid",
  "periodType": "monthly | trimestral | weekly | semestral | annual",
  "trimester": 1 | 2 | 3,
  "startDate": "2026-04-07",
  "endDate": "2026-04-30",
  "templateUrl": "https://blob.../plantilla.docx"  // opcional
}

Response 200:
{
  "planId": "uuid",
  "content": { ... },   // JSON estructurado del planeamiento
  "pdfUrl": "https://blob.../plan.pdf?sas=...",
  "cached": true | false,
  "generatedAt": "2026-04-07T10:00:00Z"
}
```

### POST /api/asistencia

```http
POST /api/asistencia
Authorization: Bearer {access_token}
Content-Type: application/json

{
  "groupId": "uuid",
  "date": "2026-04-07",
  "records": [
    { "studentId": "uuid", "status": "present", "entryTime": "07:35" },
    { "studentId": "uuid", "status": "absent" },
    { "studentId": "uuid", "status": "late", "justification": "cita médica" }
  ]
}

Response 201: { "saved": 35, "syncId": "uuid" }
```

### GET /api/powersync/token

```http
GET /api/powersync/token
Authorization: Bearer {auth0_access_token}

Response 200:
{
  "token": "eyJ...",   // JWT firmado para PowerSync
  "expiresAt": "2026-04-07T11:00:00Z"
}
```

---

## Diagrama de despliegue (Fase 1–2)

```
GitHub (main branch)
    │
    ▼ GitHub Actions CI/CD
    │
    ├──► Azure App Service (B1/S1)
    │    └── AulaIA.Api (.NET 10)
    │        └── Managed Identity →┬─► Azure AI Foundry (GPT-5.5)
    │                               ├─► Azure Blob Storage
    │                               └─► Key Vault (secretos)
    │
    ├──► Azure Static Web Apps (`swa-demo`)
    │    └── Next.js (app web)
    │
    ├──► Expo EAS Build
    │    └── React Native → App Store + Play Store
    │
    ├──► PostgreSQL
    │    └── Azure Database for PostgreSQL Flexible Server
    │
    ├──► PowerSync Service (self-hosted o tier cloud gratuito)
    │    └── Replica PG → SQLite en dispositivos
    │
    └──► Cloudflare DNS
         ├── mep.ezekl.com       CNAME → swa-demo.azurestaticapps.net
         └── api.mep.ezekl.com   CNAME → app-demo-api.azurewebsites.net
```

---

## Decisiones de arquitectura registradas

| ADR | Decisión | Archivo |
|-----|----------|---------|
| ADR-001 | .NET 10 + EF Core 10 para backend | `06_decisions.md` |
| ADR-002 | Azure AI Foundry — GPT-5.5 para LLM | `06_decisions.md` |
| ADR-003 | Azure Blob Storage para archivos | `06_decisions.md` |
| ADR-004 | App Service (F1–2) → Container Apps (F3+) | `06_decisions.md` |
| ADR-005 | PowerSync para sincronización offline | `06_decisions.md` |
| ADR-006 | Auth0 para autenticación | `06_decisions.md` |
| ADR-007 | Next.js SPA servido desde App Service único (sin Static Web App) | `06_decisions.md` |
| ADR-008 | Modelo de comisiones para Adriana Guido (20% neto, 12 meses) | `06_decisions.md` |
| ADR-009 | SINPE Móvil con verificación manual; TC del BCCR vía job Hangfire | `06_decisions.md` |
