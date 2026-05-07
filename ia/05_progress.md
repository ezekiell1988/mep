# 05 — Progreso del Proyecto

> **Última actualización:** 2026-05-06
> **Fase activa:** Fase 3 — Notas y Trabajo Cotidiano (Fase 2 completa ✅)

---

## ✅ Completado

**Fase 0 — Infraestructura Azure y setup del proyecto** (2026-05-05)

**Fase 1 — Core: Grupos, Estudiantes y Asistencia QR** (2026-05-05)

---

## 🔄 En progreso — Fase 2

> **Decisiones de diseño confirmadas (2026-05-04):**
> - Stack definido: .NET 10 + EF Core 10 + PostgreSQL + PowerSync + Auth0 + Azure AI Foundry GPT-5.5
> - Patrón backend: Feature Folders / Vertical Slices (sin Controllers)
> - Dominio: `mep.ezekl.com` (web) · `api.mep.ezekl.com` (API) — DNS en Cloudflare
> - Resource group: `rg-ezequiel` · todos los recursos con prefijo `demo`
> - Autenticación: Managed Identity para comunicación Azure-to-Azure (cero API keys en código)

### PC-01: Infraestructura Azure
| Tarea | Estado |
|-------|--------|
| F0-01 · Storage Account `stdemomep` + 5 contenedores | ✅ |
| F0-02 · Key Vault `kv-demomep` | ✅ |
| F0-03 · PostgreSQL 16 en VM `demo-itqs` (ver `credentials/db.txt`) + DB `aulaia` | ✅ |
| F0-04 · App Service Plan `asp-demomep` + App Service `app-demo-api` | ✅ |
| F0-05 · Managed Identity asignada (roles KV/Storage pendientes — Fase 3) | ⚠️ |
| F0-06 · AI Foundry configurado — credenciales en `credentials/ai.txt` | ✅ |
| F0-07 · Static Web App — **cancelado** (Next.js SPA servido desde App Service único) | ❌ |
| F0-08 · PowerSync Cloud conectado a PostgreSQL (`aulaia`) — publication `powersync` activa | ✅ |

### PC-02: DNS y Dominio
| Tarea | Estado |
|-------|--------|
| F0-15 · CNAME `mep.ezekl.com` → `app-demo-api` en Cloudflare (web + api unificados) | ✅ |
| F0-15 · CNAME `api.mep.ezekl.com` → `app-demo-api` en Cloudflare | ✅ |

### PC-03: Repositorio y CI/CD
| Tarea | Estado |
|-------|--------|
| F0-09 · Solución .NET 10 `AulaIA.Api` con Feature Folders | ✅ |
| F0-10 · Proyecto Next.js `aulaia-web` (output: export, SPA estático, 0 vulnerabilidades) | ✅ |
| F0-11 · Proyecto Expo `aulaia-app` (SDK 55, TypeScript, mobile/) | ✅ |
| F0-12 · GitHub Actions CI/CD — workflow unificado `deploy.yml` (Next.js export → wwwroot + .NET publish → App Service) | ✅ |

### PC-04: Base de datos y Auth
| Tarea | Estado |
|-------|--------|
| F0-13 · Migración EF Core `InitialCreate` + seed data (20 instituciones MEP, usuarios admin/docente) | ✅ |
| F0-14 · Auth0: tenant `aulaia-mep`, API, apps web/móvil, roles (admin/teacher/director), Action roles claim | ✅ |
| F0-15 · DNS Cloudflare: `mep.ezekl.com` + `api.mep.ezekl.com` → `app-demo-api` (proxy ✅) | ✅ |

### PC-05: Fase 1 — Core
| Tarea | Estado |
|-------|--------|
| F1 · Auth0 JWT middleware en .NET (`AddAuthentication` + `AddAuthorization`) | ✅ |
| F1 · `ICurrentUserService` — resuelve Auth0 sub → User de BD, scoped por request | ✅ |
| F1 · Módulo Grupos: CRUD completo + endpoints con ownership por docente | ✅ |
| F1 · Módulo Estudiantes: CRUD + ownership + QrCode único por alumno | ✅ |
| F1 · Migración EF Core `AddStudentQrCode` — columna `qr_code` + índice único | ✅ |
| F1 · Módulo Asistencia: GET por fecha (lista completa), POST upsert, POST scan QR | ✅ |
| F1 · Endpoint `/api/powersync/token` — JWT HS256 firmado con sub del usuario | ✅ |
| F1 · App móvil: dependencias (react-native-auth0, @react-navigation, expo-camera, expo-secure-store, @powersync/react-native) | ✅ |
| F1 · App móvil: `AuthContext.tsx` — Auth0 PKCE login/logout, token en SecureStore | ✅ |
| F1 · App móvil: `api/client.ts` + `api/endpoints.ts` — wrapper tipado con Bearer token | ✅ |
| F1 · App móvil: `LoginScreen` — botón Auth0, spinner | ✅ |
| F1 · App móvil: `GruposScreen` — FlatList grupos, pull-to-refresh, logout | ✅ |
| F1 · App móvil: `EstudiantesScreen` — lista alumnos, botón Tomar Lista Hoy | ✅ |
| F1 · App móvil: `TomarListaScreen` — modo manual (ciclar estado) + modo QR (CameraView, cooldown 2s) | ✅ |
| F1 · App móvil: `AppNavigator.tsx` + `App.tsx` — NavigationContainer + AuthProvider, TS 0 errores | ✅ |
| F1 · App móvil: fixes RN best practices (Pressable, SafeAreaView edges, falsy &&, contentInsetAdjustmentBehavior) | ✅ |
| F1 · Sync Rules YAML en PowerSync Cloud dashboard (tablas: groups, students, attendance_records filtradas por teacher_sub) | ✅ |
| F1 · Endpoint `/api/powersync/crud` — recibe mutations de asistencia del Connector | ✅ |
| F1 · App móvil: `powersync/schema.ts` — tablas SQLite locales | ✅ |
| F1 · App móvil: `powersync/PowerSyncContext.tsx` — DB + Connector + Provider | ✅ |
| F1 · App móvil: rewire GruposScreen + EstudiantesScreen → useQuery SQLite | ✅ |
| F1 · App móvil: TomarListaScreen escribe en SQLite local (auto-sync) | ✅ |
| F1 · App web: auth Auth0 + vistas grupos + descarga QRs PDF | ✅ |
| F1 · Assets de la app: icon, adaptive-icon, splash-icon, favicon (Expo) + web icon (Next.js) | ✅ |

### PC-06: Fase 2 — Planeamiento Didáctico con IA
| Tarea | Estado |
|-------|--------|
| F2 · NuGet: Hangfire.AspNetCore 1.8.20 + Hangfire.PostgreSql 1.20.10 + Azure.AI.OpenAI 2.1.0 + PdfPig 0.1.9 | ✅ |
| F2 · Entidades EF: `CurriculumUnit` (unidades del programa MEP, JSONB) + `LessonPlan` (planeamiento generado) | ✅ |
| F2 · `AiOptions.cs` — config Endpoint/DeploymentName/ApiKey con validación en start | ✅ |
| F2 · `StorageOptions` — container `curriculum` agregado | ✅ |
| F2 · `HangfireExtensions.cs` — Hangfire con PostgreSQL + dashboard `/hangfire` (solo admin) | ✅ |
| F2 · `HangfireAdminAuthFilter.cs` — dashboard protegido con rol admin Auth0 | ✅ |
| F2 · Migración EF Core `AddCurriculumAndPlanning` — tablas `curriculum_units` + `lesson_plans` | ✅ |
| F2 · `CurriculumModule.cs` — POST upload PDF → Blob → job, GET unidades validadas, POST validar (admin) | ✅ |
| F2 · `ExtractCurriculumJob.cs` — queue "curriculum": descarga PDF → PdfPig → GPT-5.5 → BD | ✅ |
| F2 · `PlaneamientoAiService.cs` — consulta curriculum validado → prompt GPT-5.5 → Markdown | ✅ |
| F2 · `PlaneamientoModule.cs` — POST crear (encola job), GET por id, GET lista por docente | ✅ |
| F2 · `GenerarPlaneamientoJob.cs` — queue "planeamiento": Pending→Generating→Ready/Failed | ✅ |
| F2 · App Service `app-demo-api` — 10 variables de entorno configuradas (DB, Auth, Storage, PowerSync, AI) | ✅ |
| F2 · `appsettings.Development.json` — credenciales AI Foundry + storage key completa | ✅ |
| F2 · Migración aplicada a BD de producción | ✅ |
| F2 · Subir PDFs MEP Artes Plásticas (III Ciclo: 7 unidades, I y II Ciclo: 14 unidades) — pipeline completo validado | ✅ |
| F2 · Refactor schema curricular: tabla `curriculum_extractions` (encabezado por PDF) + FK `curriculum_units.ExtractionId` | ✅ |
| F2 · `CurriculumExtraction` — entidad encabezado: `Asignatura`, `Ciclo`, `PdfSourceUrl`, `ModelUsed`, `TotalTokensUsed`, `UnidadCount`, `ExtractedAt` | ✅ |
| F2 · Migración `AddCurriculumExtractionHeader` — crea `curriculum_extractions`, agrega FK cascade, elimina columnas repetidas de `curriculum_units` | ✅ |
| F2 · Migración `AddModelUsedToExtraction` — columna `ModelUsed` varchar(100) en `curriculum_extractions` | ✅ |
| F2 · `ExtractCurriculumJob` — crea encabezado `CurriculumExtraction` antes de insertar units; `ModelUsed` tomado de `AiOptions.DeploymentChat` | ✅ |
| F2 · `ILlmAuditService` — `LogError` incluye cadena completa de `InnerException` | ✅ |
| F2 · Skill `mep-db-access` — documentación psql + queries para `curriculum_extractions` y `curriculum_units` | ✅ |
| F2 · App web: formulario parámetros del planeamiento | ✅ |
| F2 · App web: vista planeamiento generado (render Markdown + polling) | ✅ |
| F2 · App web: descarga .md + imprimir/PDF | ✅ |
| F2 · App móvil: vista planeamientos guardados (offline) | ✅ |
| F2 · `LessonPlanConfiguration` — `HasColumnName` explícito para todas las columnas (snake_case) | ✅ |
| F2 · Migración `RenameLessonPlanColumns` — renombra columnas PascalCase → snake_case en `lesson_plans` | ✅ |
| F2 · PowerSync Sync Rules — tabla `lesson_plans` agregada (`WHERE group_id = bucket.group_id`) | ✅ |

### PC-07: Tests de Integración — `AulaIA.Tests`
| Tarea | Estado |
|-------|--------|
| Tests · Proyecto `AulaIA.Tests` — xUnit + FluentAssertions + net10.0 | ✅ |
| Tests · `appsettings.test.template.json` — plantilla con placeholders (versionada) | ✅ |
| Tests · `appsettings.test.json` — valores reales (gitignoreado) | ✅ |
| Tests · `Infrastructure/TestConfig.cs` — lee config de archivo + env vars | ✅ |
| Tests · `Infrastructure/Auth0TokenHelper.cs` — obtiene token M2M de Auth0 (Client Credentials) | ✅ |
| Tests · `Infrastructure/ApiClient.cs` — `HttpClient` con Bearer token; `CreateAsync()` + `CreateAnonymous()` | ✅ |
| Tests · `Infrastructure/IntegrationTestBase.cs` — base con `IAsyncLifetime`, helpers `AssertStatusAsync` / `ReadJsonAsync` | ✅ |
| Tests · `SanityTests.cs` — GET /health → 200, GET /api/grupos sin token → 401, con token → 200 | ✅ |
| Tests · `CurriculumTests.cs` — upload PDF (202+jobId), upload no-PDF (400), poll extracción, validar unidad, listar curriculum | ✅ |
| Tests · `PlaneamientoTests.cs` — listar (200), crear sin body (400), id inexistente (404), flujo completo Pending→Ready, leer PDF | ✅ |
